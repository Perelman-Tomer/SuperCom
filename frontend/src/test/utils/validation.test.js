import { describe, it, expect } from 'vitest';
import { taskValidationSchema } from '../../utils/validation';

const validData = {
  title: 'Test Task',
  description: 'Some description',
  dueDate: '2026-03-15T10:00',
  priority: 'Medium',
  userFullName: 'John Doe',
  userTelephone: '050-1234567',
  userEmail: 'john@example.com',
  tagIds: [1, 2],
};

describe('taskValidationSchema', () => {
  it('validates correct data successfully', async () => {
    const result = await taskValidationSchema.validate(validData);
    expect(result.title).toBe('Test Task');
  });

  it('allows null description', async () => {
    const data = { ...validData, description: null };
    const result = await taskValidationSchema.validate(data);
    expect(result.description).toBeNull();
  });

  it('allows empty tagIds array', async () => {
    const data = { ...validData, tagIds: [] };
    const result = await taskValidationSchema.validate(data);
    expect(result.tagIds).toEqual([]);
  });

  // Title validation
  it('rejects empty title', async () => {
    const data = { ...validData, title: '' };
    await expect(taskValidationSchema.validate(data)).rejects.toThrow('Title is required');
  });

  it('rejects title exceeding 200 characters', async () => {
    const data = { ...validData, title: 'a'.repeat(201) };
    await expect(taskValidationSchema.validate(data)).rejects.toThrow('Title must not exceed 200 characters');
  });

  // Description validation
  it('rejects description exceeding 2000 characters', async () => {
    const data = { ...validData, description: 'a'.repeat(2001) };
    await expect(taskValidationSchema.validate(data)).rejects.toThrow('Description must not exceed 2000 characters');
  });

  // DueDate validation
  it('rejects empty dueDate', async () => {
    const data = { ...validData, dueDate: '' };
    await expect(taskValidationSchema.validate(data)).rejects.toThrow('Due date is required');
  });

  // Priority validation
  it('rejects invalid priority', async () => {
    const data = { ...validData, priority: 'InvalidPriority' };
    await expect(taskValidationSchema.validate(data)).rejects.toThrow();
  });

  it('rejects empty priority', async () => {
    const data = { ...validData, priority: '' };
    await expect(taskValidationSchema.validate(data)).rejects.toThrow();
  });

  // UserFullName validation
  it('rejects empty userFullName', async () => {
    const data = { ...validData, userFullName: '' };
    await expect(taskValidationSchema.validate(data)).rejects.toThrow('Full name is required');
  });

  it('rejects userFullName exceeding 150 characters', async () => {
    const data = { ...validData, userFullName: 'a'.repeat(151) };
    await expect(taskValidationSchema.validate(data)).rejects.toThrow('Name must not exceed 150 characters');
  });

  // UserTelephone validation
  it('rejects empty telephone', async () => {
    const data = { ...validData, userTelephone: '' };
    await expect(taskValidationSchema.validate(data)).rejects.toThrow('Telephone is required');
  });

  it('rejects invalid telephone format', async () => {
    const data = { ...validData, userTelephone: 'abc' };
    await expect(taskValidationSchema.validate(data)).rejects.toThrow('Please enter a valid phone number');
  });

  it('accepts valid phone formats', async () => {
    const validPhones = ['050-1234567', '+972501234567', '(050) 123-4567', '0501234567'];
    for (const phone of validPhones) {
      const data = { ...validData, userTelephone: phone };
      const result = await taskValidationSchema.validate(data);
      expect(result.userTelephone).toBe(phone);
    }
  });

  // UserEmail validation
  it('rejects empty email', async () => {
    const data = { ...validData, userEmail: '' };
    await expect(taskValidationSchema.validate(data)).rejects.toThrow('Email is required');
  });

  it('rejects invalid email format', async () => {
    const data = { ...validData, userEmail: 'not-an-email' };
    await expect(taskValidationSchema.validate(data)).rejects.toThrow('Please enter a valid email address');
  });

  it('rejects email exceeding 254 characters', async () => {
    const data = { ...validData, userEmail: 'a'.repeat(246) + '@test.com' };
    await expect(taskValidationSchema.validate(data)).rejects.toThrow();
  });
});
