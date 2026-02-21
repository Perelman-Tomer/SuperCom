import { describe, it, expect, beforeAll, afterAll, afterEach } from 'vitest';
import { configureStore } from '@reduxjs/toolkit';
import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import taskReducer, {
  fetchTasks,
  createTask,
  updateTask,
  deleteTask,
  toggleTaskCompletion,
  clearError,
  clearSelectedTask,
} from '../../store/taskSlice';

const API_BASE = 'http://localhost:5055/api';

const mockTasks = [
  {
    id: 1,
    title: 'Task One',
    description: 'Desc',
    dueDate: '2026-03-15T10:00:00Z',
    priority: 'Medium',
    userFullName: 'User One',
    userEmail: 'user1@test.com',
    userTelephone: '050-1111111',
    tags: [],
    isCompleted: false,
    createdAt: '2026-02-01T10:00:00Z',
  },
  {
    id: 2,
    title: 'Task Two',
    description: null,
    dueDate: '2026-04-01T10:00:00Z',
    priority: 'High',
    userFullName: 'User Two',
    userEmail: 'user2@test.com',
    userTelephone: '050-2222222',
    tags: [{ id: 1, name: 'Bug' }],
    isCompleted: true,
    completedAt: '2026-02-20T10:00:00Z',
    createdAt: '2026-01-15T10:00:00Z',
  },
];

const server = setupServer(
  http.get(`${API_BASE}/tasks`, () => HttpResponse.json(mockTasks)),
  http.post(`${API_BASE}/tasks`, async ({ request }) => {
    const data = await request.json();
    return HttpResponse.json({ id: 99, ...data, tags: [], isCompleted: false, createdAt: new Date().toISOString() }, { status: 201 });
  }),
  http.put(`${API_BASE}/tasks/:id`, async ({ params, request }) => {
    const data = await request.json();
    return HttpResponse.json({ id: Number(params.id), ...data, tags: [], isCompleted: false, createdAt: '2026-02-01T10:00:00Z' });
  }),
  http.delete(`${API_BASE}/tasks/:id`, () => new HttpResponse(null, { status: 204 })),
  http.patch(`${API_BASE}/tasks/:id/toggle-completion`, ({ params }) =>
    HttpResponse.json({ ...mockTasks[0], id: Number(params.id), isCompleted: true, completedAt: new Date().toISOString() })
  )
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

function createStore(preloadedTasks = []) {
  return configureStore({
    reducer: { tasks: taskReducer },
    preloadedState: {
      tasks: { items: preloadedTasks, selectedTask: null, loading: false, error: null },
    },
  });
}

describe('taskSlice reducers', () => {
  it('clearError should reset error to null', () => {
    const store = configureStore({
      reducer: { tasks: taskReducer },
      preloadedState: {
        tasks: { items: [], selectedTask: null, loading: false, error: 'Some error' },
      },
    });

    store.dispatch(clearError());
    expect(store.getState().tasks.error).toBeNull();
  });

  it('clearSelectedTask should reset selectedTask to null', () => {
    const store = configureStore({
      reducer: { tasks: taskReducer },
      preloadedState: {
        tasks: { items: [], selectedTask: { id: 1, title: 'T' }, loading: false, error: null },
      },
    });

    store.dispatch(clearSelectedTask());
    expect(store.getState().tasks.selectedTask).toBeNull();
  });
});

describe('taskSlice async thunks', () => {
  it('fetchTasks fulfilled — populates items', async () => {
    const store = createStore();

    await store.dispatch(fetchTasks());

    const state = store.getState().tasks;
    expect(state.items).toHaveLength(2);
    expect(state.items[0].title).toBe('Task One');
    expect(state.loading).toBe(false);
  });

  it('fetchTasks pending — sets loading to true', () => {
    const store = createStore();

    store.dispatch(fetchTasks());

    // While pending
    expect(store.getState().tasks.loading).toBe(true);
  });

  it('fetchTasks rejected — sets error', async () => {
    server.use(
      http.get(`${API_BASE}/tasks`, () => HttpResponse.json({ message: 'Server error' }, { status: 500 }))
    );

    const store = createStore();
    await store.dispatch(fetchTasks());

    expect(store.getState().tasks.error).toBeTruthy();
    expect(store.getState().tasks.loading).toBe(false);
  });

  it('createTask fulfilled — prepends to items', async () => {
    const store = createStore(mockTasks);

    await store.dispatch(createTask({
      title: 'New Task',
      dueDate: '2026-05-01T10:00:00Z',
      priority: 'Low',
      userFullName: 'New User',
      userTelephone: '050-3333333',
      userEmail: 'new@test.com',
      tagIds: [],
    }));

    const state = store.getState().tasks;
    expect(state.items[0].title).toBe('New Task');
    expect(state.items).toHaveLength(3);
  });

  it('updateTask fulfilled — updates the item in place', async () => {
    const store = createStore(mockTasks);

    await store.dispatch(updateTask({
      id: 1,
      data: {
        title: 'Updated Title',
        dueDate: '2026-05-01T10:00:00Z',
        priority: 'High',
        userFullName: 'User One',
        userTelephone: '050-1111111',
        userEmail: 'user1@test.com',
        tagIds: [],
      },
    }));

    const state = store.getState().tasks;
    expect(state.items.find(t => t.id === 1).title).toBe('Updated Title');
  });

  it('deleteTask fulfilled — removes the item', async () => {
    const store = createStore(mockTasks);

    await store.dispatch(deleteTask(1));

    const state = store.getState().tasks;
    expect(state.items).toHaveLength(1);
    expect(state.items.find(t => t.id === 1)).toBeUndefined();
  });

  it('toggleTaskCompletion fulfilled — updates the item', async () => {
    const store = createStore(mockTasks);

    await store.dispatch(toggleTaskCompletion(1));

    const state = store.getState().tasks;
    const task = state.items.find(t => t.id === 1);
    expect(task.isCompleted).toBe(true);
  });
});
