import { describe, it, expect, beforeAll, afterAll, afterEach } from 'vitest';
import { configureStore } from '@reduxjs/toolkit';
import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import tagReducer, { fetchTags, createTag, updateTag, deleteTag } from '../../store/tagSlice';

const API_BASE = 'http://localhost:5055/api';

const mockTags = [
  { id: 1, name: 'Bug' },
  { id: 2, name: 'Feature' },
  { id: 3, name: 'Urgent' },
];

const server = setupServer(
  http.get(`${API_BASE}/tags`, () => HttpResponse.json(mockTags)),
  http.post(`${API_BASE}/tags`, async ({ request }) => {
    const data = await request.json();
    return HttpResponse.json({ id: 99, name: data.name }, { status: 201 });
  }),
  http.put(`${API_BASE}/tags/:id`, async ({ params, request }) => {
    const data = await request.json();
    return HttpResponse.json({ id: Number(params.id), name: data.name });
  }),
  http.delete(`${API_BASE}/tags/:id`, () => new HttpResponse(null, { status: 204 }))
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

function createStore(preloadedTags = []) {
  return configureStore({
    reducer: { tags: tagReducer },
    preloadedState: {
      tags: { items: preloadedTags, loading: false, error: null },
    },
  });
}

describe('tagSlice async thunks', () => {
  it('fetchTags fulfilled — populates items', async () => {
    const store = createStore();

    await store.dispatch(fetchTags());

    const state = store.getState().tags;
    expect(state.items).toHaveLength(3);
    expect(state.items[0].name).toBe('Bug');
    expect(state.loading).toBe(false);
  });

  it('fetchTags pending — sets loading to true', () => {
    const store = createStore();

    store.dispatch(fetchTags());

    expect(store.getState().tags.loading).toBe(true);
  });

  it('fetchTags rejected — sets error', async () => {
    server.use(
      http.get(`${API_BASE}/tags`, () => HttpResponse.json({ message: 'Error' }, { status: 500 }))
    );

    const store = createStore();
    await store.dispatch(fetchTags());

    expect(store.getState().tags.error).toBeTruthy();
    expect(store.getState().tags.loading).toBe(false);
  });

  it('createTag fulfilled — adds and sorts items', async () => {
    const store = createStore(mockTags);

    await store.dispatch(createTag({ name: 'Alpha' }));

    const state = store.getState().tags;
    expect(state.items).toHaveLength(4);
    // Alpha should be first after sorting
    expect(state.items[0].name).toBe('Alpha');
  });

  it('createTag rejected — returns rejected action', async () => {
    server.use(
      http.post(`${API_BASE}/tags`, () =>
        HttpResponse.json({ message: 'Tag already exists' }, { status: 409 })
      )
    );

    const store = createStore(mockTags);
    const result = await store.dispatch(createTag({ name: 'Bug' }));

    // The thunk rejects, items remain unchanged
    expect(result.meta.requestStatus).toBe('rejected');
    expect(store.getState().tags.items).toHaveLength(3);
  });

  it('updateTag fulfilled — replaces item and re-sorts', async () => {
    const store = createStore(mockTags);

    await store.dispatch(updateTag({ id: 1, data: { name: 'Zzz-Renamed' } }));

    const state = store.getState().tags;
    const updated = state.items.find(t => t.id === 1);
    expect(updated.name).toBe('Zzz-Renamed');
    // Should be last after sort
    expect(state.items[state.items.length - 1].name).toBe('Zzz-Renamed');
  });

  it('updateTag rejected — returns rejected action, items unchanged', async () => {
    server.use(
      http.put(`${API_BASE}/tags/:id`, () =>
        HttpResponse.json({ message: 'Duplicate name' }, { status: 409 })
      )
    );

    const store = createStore(mockTags);
    const result = await store.dispatch(updateTag({ id: 1, data: { name: 'Feature' } }));

    expect(result.meta.requestStatus).toBe('rejected');
    // Items remain unchanged
    expect(store.getState().tags.items.find(t => t.id === 1).name).toBe('Bug');
  });

  it('deleteTag fulfilled — removes item', async () => {
    const store = createStore(mockTags);

    await store.dispatch(deleteTag(2));

    const state = store.getState().tags;
    expect(state.items).toHaveLength(2);
    expect(state.items.find(t => t.id === 2)).toBeUndefined();
  });
});
