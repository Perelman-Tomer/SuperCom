import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import ConfirmDialog from '../../components/ConfirmDialog';

describe('ConfirmDialog', () => {
  const defaultProps = {
    open: true,
    title: 'Delete Task',
    message: 'Are you sure you want to delete this task?',
    onConfirm: vi.fn(),
    onCancel: vi.fn(),
  };

  it('renders title and message when open', () => {
    render(<ConfirmDialog {...defaultProps} />);

    expect(screen.getByText('Delete Task')).toBeInTheDocument();
    expect(screen.getByText('Are you sure you want to delete this task?')).toBeInTheDocument();
  });

  it('renders Cancel and Delete buttons', () => {
    render(<ConfirmDialog {...defaultProps} />);

    expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /delete/i })).toBeInTheDocument();
  });

  it('calls onConfirm when Delete button is clicked', async () => {
    const user = userEvent.setup();
    const onConfirm = vi.fn();
    render(<ConfirmDialog {...defaultProps} onConfirm={onConfirm} />);

    await user.click(screen.getByRole('button', { name: /delete/i }));

    expect(onConfirm).toHaveBeenCalledTimes(1);
  });

  it('calls onCancel when Cancel button is clicked', async () => {
    const user = userEvent.setup();
    const onCancel = vi.fn();
    render(<ConfirmDialog {...defaultProps} onCancel={onCancel} />);

    await user.click(screen.getByRole('button', { name: /cancel/i }));

    expect(onCancel).toHaveBeenCalledTimes(1);
  });

  it('does not render dialog content when closed', () => {
    render(<ConfirmDialog {...defaultProps} open={false} />);

    expect(screen.queryByText('Delete Task')).not.toBeInTheDocument();
    expect(screen.queryByText('Are you sure you want to delete this task?')).not.toBeInTheDocument();
  });
});
