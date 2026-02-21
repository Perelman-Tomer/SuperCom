import { configureStore } from '@reduxjs/toolkit';
import taskReducer from './taskSlice';
import tagReducer from './tagSlice';

export const store = configureStore({
  reducer: {
    tasks: taskReducer,
    tags: tagReducer,
  },
});
