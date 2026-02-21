import { describe, it, expect, vi, beforeEach, beforeAll, afterAll, afterEach } from 'vitest';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import taskReducer from '../../store/taskSlice';
import tagReducer from '../../store/tagSlice';
import TaskList from '../../components/TaskList';

const API_BASE = 'http://localhost:5055/api';

const mockTasks = [
  {
    id: 1,
    title: 'Active Task One',
    description: 'First task description',
    dueDate: '2026-03-15T10:00:00Z',
    priority: 'High',
    userFullName: 'Dana Cohen',
    userEmail: 'dana@example.com',
    userTelephone: '050-1234567',
    tags: [{ id: 1, name: 'Bug' }],
    isCompleted: false,
    createdAt: '2026-02-01T10:00:00Z',
  },
  {
    id: 2,
    title: 'Completed Task Two',
    description: 'Second task',
    dueDate: '2026-04-01T10:00:00Z',
    priority: 'Low',
    userFullName: 'Yossi Levi',
    userEmail: 'yossi@example.com',
    userTelephone: '052-9876543',
    tags: [{ id: 2, name: 'Feature' }, { id: 3, name: 'Improvement' }],
    isCompleted: true,
    completedAt: '2026-02-20T10:00:00Z',
    createdAt: '2026-01-15T10:00:00Z',
  },
  {
    id: 3,
    title: 'Overdue Task Three',
    description: 'Overdue task',
    dueDate: '2025-12-01T10:00:00Z',
    priority: 'Critical',
    userFullName: 'Noa Shapira',
    userEmail: 'noa@example.com',
    userTelephone: '054-5551234',
    tags: [],
    isCompleted: false,
    createdAt: '2025-11-01T10:00:00Z',
  },
];

const mockTags = [{ id: 1, name: 'Bug' }, { id: 2, name: 'Feature' }, { id: 3, name: 'Improvement' }];

// MSW server to handle API calls triggered by useEffect
const server = setupServer(
  http.get(`${API_BASE}/tasks`, () => HttpResponse.json(mockTasks)),
  http.get(`${API_BASE}/tags`, () => HttpResponse.json(mockTags))
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

function createStore() {
  return configureStore({
    reducer: {
      tasks: taskReducer,
      tags: tagReducer,
    },
  });
}

describe('TaskList', () => {
  let store;

  beforeEach(() => {
    store = createStore();
  });

  const renderList = () => {
    return render(
      <Provider store={store}>
        <TaskList />
      </Provider>
    );
  };

  it('renders the Task Manager heading', () => {
    renderList();
    expect(screen.getByText('Task Manager')).toBeInTheDocument();
  });

  it('renders the New Task button', () => {
    renderList();
    expect(screen.getByRole('button', { name: /new task/i })).toBeInTheDocument();
  });

  it('renders task titles in the table', async () => {
    renderList();
    await waitFor(() => {
      expect(screen.getByText('Active Task One')).toBeInTheDocument();
    });
    expect(screen.getByText('Completed Task Two')).toBeInTheDocument();
    expect(screen.getByText('Overdue Task Three')).toBeInTheDocument();
  });

  it('renders user names for tasks', async () => {
    renderList();
    await waitFor(() => {
      expect(screen.getByText('Dana Cohen')).toBeInTheDocument();
    });
    expect(screen.getByText('Yossi Levi')).toBeInTheDocument();
    expect(screen.getByText('Noa Shapira')).toBeInTheDocument();
  });

  it('renders tag chips for tasks', async () => {
    renderList();
    await waitFor(() => {
      expect(screen.getByText('Bug')).toBeInTheDocument();
    });
    expect(screen.getByText('Feature')).toBeInTheDocument();
    expect(screen.getByText('Improvement')).toBeInTheDocument();
  });

  it('renders priority chips', async () => {
    renderList();
    await waitFor(() => {
      expect(screen.getByText('High')).toBeInTheDocument();
    });
    expect(screen.getByText('Low')).toBeInTheDocument();
    expect(screen.getByText('Critical')).toBeInTheDocument();
  });

  it('marks overdue tasks with Overdue label', async () => {
    renderList();
    await waitFor(() => {
      expect(screen.getByText('Overdue')).toBeInTheDocument();
    });
  });

  it('renders search input', () => {
    renderList();
    expect(screen.getByPlaceholderText(/search tasks/i)).toBeInTheDocument();
  });

  it('filters tasks by search query', async () => {
    const user = userEvent.setup();
    renderList();

    // Wait for tasks to load
    await waitFor(() => {
      expect(screen.getByText('Active Task One')).toBeInTheDocument();
    });

    const search = screen.getByPlaceholderText(/search tasks/i);
    await user.type(search, 'Dana');

    expect(screen.getByText('Active Task One')).toBeInTheDocument();
    expect(screen.queryByText('Completed Task Two')).not.toBeInTheDocument();
    expect(screen.queryByText('Overdue Task Three')).not.toBeInTheDocument();
  });

  it('filters tasks by tag name in search', async () => {
    const user = userEvent.setup();
    renderList();

    await waitFor(() => {
      expect(screen.getByText('Active Task One')).toBeInTheDocument();
    });

    const search = screen.getByPlaceholderText(/search tasks/i);
    await user.type(search, 'Feature');

    expect(screen.getByText('Completed Task Two')).toBeInTheDocument();
    expect(screen.queryByText('Active Task One')).not.toBeInTheDocument();
  });

  it('renders tab labels: All, Active, Completed', () => {
    renderList();
    expect(screen.getByRole('tab', { name: /all/i })).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: /active/i })).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: /completed/i })).toBeInTheDocument();
  });

  it('filters to only active tasks when Active tab is clicked', async () => {
    const user = userEvent.setup();
    renderList();

    await waitFor(() => {
      expect(screen.getByText('Active Task One')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('tab', { name: /active/i }));

    expect(screen.getByText('Active Task One')).toBeInTheDocument();
    expect(screen.getByText('Overdue Task Three')).toBeInTheDocument();
    expect(screen.queryByText('Completed Task Two')).not.toBeInTheDocument();
  });

  it('filters to only completed tasks when Completed tab is clicked', async () => {
    const user = userEvent.setup();
    renderList();

    await waitFor(() => {
      expect(screen.getByText('Active Task One')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('tab', { name: /completed/i }));

    expect(screen.getByText('Completed Task Two')).toBeInTheDocument();
    expect(screen.queryByText('Active Task One')).not.toBeInTheDocument();
    expect(screen.queryByText('Overdue Task Three')).not.toBeInTheDocument();
  });

  it('opens create dialog when New Task button is clicked', async () => {
    const user = userEvent.setup();
    renderList();

    await user.click(screen.getByRole('button', { name: /new task/i }));

    expect(screen.getByText('Create New Task')).toBeInTheDocument();
  });

  it('shows empty state message when no tasks', async () => {
    server.use(
      http.get(`${API_BASE}/tasks`, () => HttpResponse.json([]))
    );
    renderList();
    await waitFor(() => {
      expect(screen.getByText(/no tasks yet/i)).toBeInTheDocument();
    });
  });

  it('shows loading spinner initially', () => {
    renderList();
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('shows error alert when API fails', async () => {
    server.use(
      http.get(`${API_BASE}/tasks`, () => HttpResponse.json({ message: 'Server error' }, { status: 500 }))
    );
    renderList();
    await waitFor(() => {
      expect(screen.getByText(/server error|failed to fetch/i)).toBeInTheDocument();
    });
  });

  it('renders sortable column headers after loading', async () => {
    renderList();
    await waitFor(() => {
      expect(screen.getByText('Title')).toBeInTheDocument();
    });
    expect(screen.getByText('Due Date')).toBeInTheDocument();
    expect(screen.getByText('Priority')).toBeInTheDocument();
    expect(screen.getByText('Assigned To')).toBeInTheDocument();
  });
});
