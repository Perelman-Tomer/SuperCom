import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { tagApi } from '../services/api';

const initialState = {
  items: [],
  loading: false,
  error: null,
};

export const fetchTags = createAsyncThunk('tags/fetchAll', async (_, { rejectWithValue }) => {
  try {
    return await tagApi.getAll();
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch tags');
  }
});

export const createTag = createAsyncThunk('tags/create', async (data, { rejectWithValue }) => {
  try {
    return await tagApi.create(data);
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to create tag');
  }
});

export const deleteTag = createAsyncThunk('tags/delete', async (id, { rejectWithValue }) => {
  try {
    await tagApi.delete(id);
    return id;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to delete tag');
  }
});

export const updateTag = createAsyncThunk('tags/update', async ({ id, data }, { rejectWithValue }) => {
  try {
    return await tagApi.update(id, data);
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to update tag');
  }
});

const tagSlice = createSlice({
  name: 'tags',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchTags.pending, (state) => { state.loading = true; state.error = null; })
      .addCase(fetchTags.fulfilled, (state, action) => { state.loading = false; state.items = action.payload; })
      .addCase(fetchTags.rejected, (state, action) => { state.loading = false; state.error = action.payload; })
      .addCase(createTag.fulfilled, (state, action) => {
        state.items.push(action.payload);
        state.items.sort((a, b) => a.name.localeCompare(b.name));
      })
      .addCase(deleteTag.fulfilled, (state, action) => {
        state.items = state.items.filter((t) => t.id !== action.payload);
      })
      .addCase(updateTag.fulfilled, (state, action) => {
        const index = state.items.findIndex((t) => t.id === action.payload.id);
        if (index !== -1) state.items[index] = action.payload;
        state.items.sort((a, b) => a.name.localeCompare(b.name));
      });
  },
});

export default tagSlice.reducer;
