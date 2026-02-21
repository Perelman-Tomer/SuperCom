import { describe, it, expect } from 'vitest';
import { Priority, priorityColors, formatDate, isOverdue } from '../../utils/helpers';

describe('Priority enum', () => {
  it('has all four priority levels', () => {
    expect(Priority.Low).toBe('Low');
    expect(Priority.Medium).toBe('Medium');
    expect(Priority.High).toBe('High');
    expect(Priority.Critical).toBe('Critical');
  });

  it('has exactly 4 values', () => {
    expect(Object.keys(Priority)).toHaveLength(4);
  });
});

describe('priorityColors', () => {
  it('maps Low to default', () => {
    expect(priorityColors[Priority.Low]).toBe('default');
  });

  it('maps Medium to info', () => {
    expect(priorityColors[Priority.Medium]).toBe('info');
  });

  it('maps High to warning', () => {
    expect(priorityColors[Priority.High]).toBe('warning');
  });

  it('maps Critical to error', () => {
    expect(priorityColors[Priority.Critical]).toBe('error');
  });
});

describe('formatDate', () => {
  it('formats a date string to en-US locale', () => {
    const result = formatDate('2026-03-15T10:00:00Z');
    expect(result).toMatch(/Mar/);
    expect(result).toMatch(/15/);
    expect(result).toMatch(/2026/);
  });

  it('formats another date correctly', () => {
    const result = formatDate('2026-01-05T10:00:00Z');
    expect(result).toMatch(/Jan/);
    expect(result).toMatch(/2026/);
  });
});

describe('isOverdue', () => {
  it('returns true for a past date', () => {
    expect(isOverdue('2020-01-01T00:00:00Z')).toBe(true);
  });

  it('returns false for a future date', () => {
    const futureDate = new Date();
    futureDate.setFullYear(futureDate.getFullYear() + 1);
    expect(isOverdue(futureDate.toISOString())).toBe(false);
  });
});
