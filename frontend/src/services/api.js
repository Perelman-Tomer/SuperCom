import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5055/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Task API
export const taskApi = {
  getAll: () => api.get('/tasks').then((res) => res.data),
  getById: (id) => api.get(`/tasks/${id}`).then((res) => res.data),
  create: (data) => api.post('/tasks', data).then((res) => res.data),
  update: (id, data) => api.put(`/tasks/${id}`, data).then((res) => res.data),
  delete: (id) => api.delete(`/tasks/${id}`),
  toggleCompletion: (id) => api.patch(`/tasks/${id}/toggle-completion`).then((res) => res.data),
};

// Tag API
export const tagApi = {
  getAll: () => api.get('/tags').then((res) => res.data),
  create: (data) => api.post('/tags', data).then((res) => res.data),
  update: (id, data) => api.put(`/tags/${id}`, data).then((res) => res.data),
  delete: (id) => api.delete(`/tags/${id}`),
};

export default api;
