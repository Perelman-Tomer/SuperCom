import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import tagReducer from '../../store/tagSlice';
import TaskForm from '../../components/TaskForm';

// Create a mock store for each test with pre-loaded tags
function createMockStore(tags = []) {
  return configureStore({
    reducer: { tags: tagReducer },
    preloadedState: {
      tags: { items: tags, loading: false, error: null },
    },
  });
}

const mockTags = [
  { id: 1, name: 'Bug' },
  { id: 2, name: 'Feature' },
];

describe('TaskForm', () => {
  let mockOnSubmit;
  let mockOnClose;

  beforeEach(() => {
    mockOnSubmit = vi.fn();
    mockOnClose = vi.fn();
  });

  const renderForm = (props = {}) => {
    const store = createMockStore(mockTags);
    return render(
      <Provider store={store}>
        <TaskForm
          open={true}
          onClose={mockOnClose}
          onSubmit={mockOnSubmit}
          task={null}
          {...props}
        />
      </Provider>
    );
  };

  it('renders all form fields when open', () => {
    renderForm();

    expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/due date/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/priority/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/full name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/telephone/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/tags/i)).toBeInTheDocument();
  });

  it('shows "Create New Task" title when no task provided', () => {
    renderForm();
    expect(screen.getByText('Create New Task')).toBeInTheDocument();
  });

  it('shows "Edit Task" title when task is provided', () => {
    const task = {
      id: 1,
      title: 'Test Task',
      description: 'Desc',
      dueDate: '2026-03-15T10:00:00Z',
      priority: 'High',
      userFullName: 'John Doe',
      userTelephone: '050-1234567',
      userEmail: 'john@example.com',
      tags: [{ id: 1, name: 'Bug' }],
    };
    renderForm({ task });
    expect(screen.getByText('Edit Task')).toBeInTheDocument();
  });

  it('populates fields in edit mode', () => {
    const task = {
      id: 1,
      title: 'My Task',
      description: 'My Description',
      dueDate: '2026-03-15T10:00:00Z',
      priority: 'High',
      userFullName: 'Jane Smith',
      userTelephone: '052-9876543',
      userEmail: 'jane@example.com',
      tags: [],
    };
    renderForm({ task });

    expect(screen.getByLabelText(/title/i)).toHaveValue('My Task');
    expect(screen.getByLabelText(/description/i)).toHaveValue('My Description');
    expect(screen.getByLabelText(/full name/i)).toHaveValue('Jane Smith');
    expect(screen.getByLabelText(/telephone/i)).toHaveValue('052-9876543');
    expect(screen.getByLabelText(/email/i)).toHaveValue('jane@example.com');
  });

  it('shows validation errors when submitting empty form', async () => {
    renderForm();

    // Use fireEvent.submit directly on the form (MUI Dialog portal + jsdom workaround)
    const form = document.querySelector('form');
    fireEvent.submit(form);

    // react-hook-form + yup validation is async; wait for error helper text
    await waitFor(() => {
      expect(screen.getByText(/title is required/i)).toBeInTheDocument();
    });

    // onSubmit should NOT have been called
    expect(mockOnSubmit).not.toHaveBeenCalled();
  });

  it('calls onClose when Cancel is clicked', async () => {
    const user = userEvent.setup();
    renderForm();

    await user.click(screen.getByRole('button', { name: /cancel/i }));

    expect(mockOnClose).toHaveBeenCalledTimes(1);
  });

  it('shows Update button in edit mode', () => {
    const task = {
      id: 1, title: 'T', description: '', dueDate: '2026-03-15T10:00:00Z',
      priority: 'Low', userFullName: 'A', userTelephone: '050-1234567',
      userEmail: 'a@b.com', tags: [],
    };
    renderForm({ task });
    expect(screen.getByRole('button', { name: /update/i })).toBeInTheDocument();
  });

  it('does not render when closed', () => {
    renderForm({ open: false });
    expect(screen.queryByText('Create New Task')).not.toBeInTheDocument();
  });
});
