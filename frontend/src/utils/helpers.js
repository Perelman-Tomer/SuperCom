export const Priority = {
  Low: 'Low',
  Medium: 'Medium',
  High: 'High',
  Critical: 'Critical',
};

export const priorityColors = {
  [Priority.Low]: 'default',
  [Priority.Medium]: 'info',
  [Priority.High]: 'warning',
  [Priority.Critical]: 'error',
};

export const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
};

export const isOverdue = (dateString) => {
  return new Date(dateString) < new Date();
};
