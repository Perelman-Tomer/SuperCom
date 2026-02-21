import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Provider } from 'react-redux';
import { store } from '../store/index.js';
import App from '../App.jsx';

describe('App', () => {
  it('renders the application title', () => {
    render(
      <Provider store={store}>
        <App />
      </Provider>
    );

    expect(screen.getByText('Task Manager')).toBeInTheDocument();
  });

  it('renders the New Task button', () => {
    render(
      <Provider store={store}>
        <App />
      </Provider>
    );

    expect(screen.getByRole('button', { name: /new task/i })).toBeInTheDocument();
  });
});
