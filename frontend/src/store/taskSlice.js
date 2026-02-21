import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { taskApi } from '../services/api';

const initialState = {
  items: [],
  selectedTask: null,
  loading: false,
  error: null,
};

export const fetchTasks = createAsyncThunk('tasks/fetchAll', async (_, { rejectWithValue }) => {
  try {
    return await taskApi.getAll();
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch tasks');
  }
});

export const fetchTaskById = createAsyncThunk('tasks/fetchById', async (id, { rejectWithValue }) => {
  try {
    return await taskApi.getById(id);
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch task');
  }
});

export const createTask = createAsyncThunk('tasks/create', async (data, { rejectWithValue }) => {
  try {
    return await taskApi.create(data);
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to create task');
  }
});

export const updateTask = createAsyncThunk('tasks/update', async ({ id, data }, { rejectWithValue }) => {
  try {
    return await taskApi.update(id, data);
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to update task');
  }
});

export const deleteTask = createAsyncThunk('tasks/delete', async (id, { rejectWithValue }) => {
  try {
    await taskApi.delete(id);
    return id;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to delete task');
  }
});

export const toggleTaskCompletion = createAsyncThunk('tasks/toggleCompletion', async (id, { rejectWithValue }) => {
  try {
    return await taskApi.toggleCompletion(id);
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to toggle task completion');
  }
});

const taskSlice = createSlice({
  name: 'tasks',
  initialState,
  reducers: {
    clearSelectedTask: (state) => { state.selectedTask = null; },
    clearError: (state) => { state.error = null; },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchTasks.pending, (state) => { state.loading = true; state.error = null; })
      .addCase(fetchTasks.fulfilled, (state, action) => { state.loading = false; state.items = action.payload; })
      .addCase(fetchTasks.rejected, (state, action) => { state.loading = false; state.error = action.payload; })
      .addCase(fetchTaskById.fulfilled, (state, action) => { state.selectedTask = action.payload; })
      .addCase(createTask.fulfilled, (state, action) => { state.items.unshift(action.payload); })
      .addCase(createTask.rejected, (state, action) => { state.error = action.payload; })
      .addCase(updateTask.fulfilled, (state, action) => {
        const index = state.items.findIndex((t) => t.id === action.payload.id);
        if (index !== -1) { state.items[index] = action.payload; }
        state.selectedTask = action.payload;
      })
      .addCase(updateTask.rejected, (state, action) => { state.error = action.payload; })
      .addCase(deleteTask.fulfilled, (state, action) => {
        state.items = state.items.filter((t) => t.id !== action.payload);
        if (state.selectedTask?.id === action.payload) { state.selectedTask = null; }
      })
      .addCase(deleteTask.rejected, (state, action) => { state.error = action.payload; })
      .addCase(toggleTaskCompletion.fulfilled, (state, action) => {
        const index = state.items.findIndex((t) => t.id === action.payload.id);
        if (index !== -1) { state.items[index] = action.payload; }
      })
      .addCase(toggleTaskCompletion.rejected, (state, action) => { state.error = action.payload; });
  },
});

export const { clearSelectedTask, clearError } = taskSlice.actions;
export default taskSlice.reducer;
