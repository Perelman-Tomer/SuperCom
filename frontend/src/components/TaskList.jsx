import { useEffect, useState, useCallback } from 'react';
import {
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Chip,
  Typography,
  Button,
  Tooltip,
  TableSortLabel,
  TextField,
  InputAdornment,
  Alert,
  Snackbar,
  CircularProgress,
  Tabs,
  Tab,
  Badge,
  TablePagination,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Search as SearchIcon,
  CheckCircle as CheckCircleIcon,
  RadioButtonUnchecked as RadioButtonUncheckedIcon,
} from '@mui/icons-material';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import { fetchTasks, createTask, updateTask, deleteTask, clearError, toggleTaskCompletion } from '../store/taskSlice';
import { priorityColors, formatDate, isOverdue } from '../utils/helpers';
import TaskForm from './TaskForm';
import ConfirmDialog from './ConfirmDialog';

function TaskList() {
  const dispatch = useAppDispatch();
  const { items: tasks, loading, error } = useAppSelector((state) => state.tasks);

  const [formOpen, setFormOpen] = useState(false);
  const [editingTask, setEditingTask] = useState(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [taskToDelete, setTaskToDelete] = useState(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [orderBy, setOrderBy] = useState('createdAt');
  const [order, setOrder] = useState('desc');
  const [snackbar, setSnackbar] = useState({
    open: false,
    message: '',
    severity: 'success',
  });
  const [activeTab, setActiveTab] = useState(0);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);

  useEffect(() => {
    dispatch(fetchTasks());
  }, [dispatch]);

  const handleSort = (property) => {
    const isAsc = orderBy === property && order === 'asc';
    setOrder(isAsc ? 'desc' : 'asc');
    setOrderBy(property);
    setPage(0);
  };

  const handleCreateOpen = () => {
    setEditingTask(null);
    setFormOpen(true);
  };

  const handleEditOpen = (task) => {
    setEditingTask(task);
    setFormOpen(true);
  };

  const handleDeleteOpen = (task) => {
    setTaskToDelete(task);
    setDeleteDialogOpen(true);
  };

  const handleFormSubmit = useCallback(
    async (data) => {
      try {
        if (editingTask) {
          const updateData = {
            title: data.title,
            description: data.description || undefined,
            dueDate: new Date(data.dueDate).toISOString(),
            priority: data.priority,
            userFullName: data.userFullName,
            userTelephone: data.userTelephone,
            userEmail: data.userEmail,
            tagIds: data.tagIds,
          };
          await dispatch(updateTask({ id: editingTask.id, data: updateData })).unwrap();
          setSnackbar({ open: true, message: 'Task updated successfully!', severity: 'success' });
        } else {
          const createData = {
            title: data.title,
            description: data.description || undefined,
            dueDate: new Date(data.dueDate).toISOString(),
            priority: data.priority,
            userFullName: data.userFullName,
            userTelephone: data.userTelephone,
            userEmail: data.userEmail,
            tagIds: data.tagIds,
          };
          await dispatch(createTask(createData)).unwrap();
          setSnackbar({ open: true, message: 'Task created successfully!', severity: 'success' });
        }
        setFormOpen(false);
        setEditingTask(null);
      } catch (err) {
        setSnackbar({ open: true, message: String(err), severity: 'error' });
      }
    },
    [dispatch, editingTask]
  );

  const handleDeleteConfirm = useCallback(async () => {
    if (taskToDelete) {
      try {
        await dispatch(deleteTask(taskToDelete.id)).unwrap();
        setSnackbar({ open: true, message: 'Task deleted successfully!', severity: 'success' });
      } catch (err) {
        setSnackbar({ open: true, message: String(err), severity: 'error' });
      }
    }
    setDeleteDialogOpen(false);
    setTaskToDelete(null);
  }, [dispatch, taskToDelete]);

  const handleToggleCompletion = useCallback(
    async (task) => {
      try {
        await dispatch(toggleTaskCompletion(task.id)).unwrap();
        setSnackbar({
          open: true,
          message: task.isCompleted ? 'Task marked as active!' : 'Task marked as done!',
          severity: 'success',
        });
      } catch (err) {
        setSnackbar({ open: true, message: String(err), severity: 'error' });
      }
    },
    [dispatch]
  );

  // Filter and sort
  const filteredTasks = tasks
    .filter(
      (task) =>
        task.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
        task.userFullName.toLowerCase().includes(searchQuery.toLowerCase()) ||
        task.userEmail.toLowerCase().includes(searchQuery.toLowerCase()) ||
        task.tags.some((tag) => tag.name.toLowerCase().includes(searchQuery.toLowerCase()))
    )
    .sort((a, b) => {
      const priorityOrder = { Low: 0, Medium: 1, High: 2, Critical: 3 };
      let comparison = 0;
      switch (orderBy) {
        case 'title':
          comparison = a.title.localeCompare(b.title);
          break;
        case 'dueDate':
          comparison = new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime();
          break;
        case 'priority':
          comparison = priorityOrder[a.priority] - priorityOrder[b.priority];
          break;
        case 'userFullName':
          comparison = a.userFullName.localeCompare(b.userFullName);
          break;
        case 'createdAt':
          comparison = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
          break;
      }
      return order === 'asc' ? comparison : -comparison;
    });

  const activeTasks = filteredTasks.filter((t) => !t.isCompleted);
  const completedTasks = filteredTasks.filter((t) => t.isCompleted);
  const completedCount = tasks.filter((t) => t.isCompleted).length;
  const displayedTasks = activeTab === 0 ? filteredTasks : activeTab === 1 ? activeTasks : completedTasks;
  const paginatedTasks = displayedTasks.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage);

  return (
    <Box>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1" fontWeight="bold">
          Task Manager
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={handleCreateOpen} size="large">
          New Task
        </Button>
      </Box>

      {/* Search */}
      <TextField
        placeholder="Search tasks by title, name, email, or tag..."
        value={searchQuery}
        onChange={(e) => { setSearchQuery(e.target.value); setPage(0); }}
        fullWidth
        sx={{ mb: 2 }}
        slotProps={{
          input: {
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
          },
        }}
      />

      {/* Tabs */}
      <Tabs
        value={activeTab}
        onChange={(_, newValue) => { setActiveTab(newValue); setPage(0); }}
        sx={{ mb: 2 }}
      >
        <Tab label="All" />
        <Tab label="Active" />
        <Tab
          label={
            <Badge badgeContent={completedCount} color="success" max={999} sx={{ '& .MuiBadge-badge': { right: -12, top: 2 } }}>
              Completed
            </Badge>
          }
          sx={{ pr: 3 }}
        />
      </Tabs>

      {/* Error */}
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => dispatch(clearError())}>
          {error}
        </Alert>
      )}

      {/* Loading */}
      {loading && (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      )}

      {/* Task Table */}
      {!loading && (
        <>
        <TableContainer component={Paper} elevation={2} sx={{ maxHeight: 'calc(100vh - 330px)', overflow: 'auto' }}>
          <Table stickyHeader>
            <TableHead>
              <TableRow>
                <TableCell sx={{ color: 'white', fontWeight: 'bold', backgroundColor: 'primary.main', width: 60 }}>
                  #
                </TableCell>
                <TableCell sx={{ color: 'white', fontWeight: 'bold', backgroundColor: 'primary.main' }}>
                  <TableSortLabel
                    active={orderBy === 'title'}
                    direction={orderBy === 'title' ? order : 'asc'}
                    onClick={() => handleSort('title')}
                    sx={{ color: 'white !important', '& .MuiTableSortLabel-icon': { color: 'white !important' } }}
                  >
                    Title
                  </TableSortLabel>
                </TableCell>
                <TableCell sx={{ color: 'white', fontWeight: 'bold', backgroundColor: 'primary.main' }}>
                  <TableSortLabel
                    active={orderBy === 'dueDate'}
                    direction={orderBy === 'dueDate' ? order : 'asc'}
                    onClick={() => handleSort('dueDate')}
                    sx={{ color: 'white !important', '& .MuiTableSortLabel-icon': { color: 'white !important' } }}
                  >
                    Due Date
                  </TableSortLabel>
                </TableCell>
                <TableCell sx={{ color: 'white', fontWeight: 'bold', backgroundColor: 'primary.main' }}>
                  <TableSortLabel
                    active={orderBy === 'priority'}
                    direction={orderBy === 'priority' ? order : 'asc'}
                    onClick={() => handleSort('priority')}
                    sx={{ color: 'white !important', '& .MuiTableSortLabel-icon': { color: 'white !important' } }}
                  >
                    Priority
                  </TableSortLabel>
                </TableCell>
                <TableCell sx={{ color: 'white', fontWeight: 'bold', backgroundColor: 'primary.main' }}>
                  <TableSortLabel
                    active={orderBy === 'userFullName'}
                    direction={orderBy === 'userFullName' ? order : 'asc'}
                    onClick={() => handleSort('userFullName')}
                    sx={{ color: 'white !important', '& .MuiTableSortLabel-icon': { color: 'white !important' } }}
                  >
                    Assigned To
                  </TableSortLabel>
                </TableCell>
                <TableCell sx={{ color: 'white', fontWeight: 'bold', backgroundColor: 'primary.main' }}>Tags</TableCell>
                <TableCell sx={{ color: 'white', fontWeight: 'bold', backgroundColor: 'primary.main' }} align="center">
                  Actions
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {displayedTasks.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                    <Typography color="text.secondary">
                      {searchQuery
                        ? 'No tasks match your search.'
                        : activeTab === 2
                          ? 'No completed tasks yet.'
                          : 'No tasks yet. Click "New Task" to create one!'}
                    </Typography>
                  </TableCell>
                </TableRow>
              ) : (
                paginatedTasks.map((task, index) => (
                  <TableRow
                    key={task.id}
                    hover
                    sx={{
                      backgroundColor: !task.isCompleted && isOverdue(task.dueDate) ? 'error.50' : 'inherit',
                      '&:hover': { backgroundColor: !task.isCompleted && isOverdue(task.dueDate) ? 'error.100' : undefined },
                      opacity: task.isCompleted ? 0.7 : 1,
                    }}
                  >
                    <TableCell>
                      <Typography variant="body2" color="text.secondary">
                        {page * rowsPerPage + index + 1}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography fontWeight="medium" sx={{ textDecoration: task.isCompleted ? 'line-through' : 'none' }}>
                        {task.title}
                      </Typography>
                      {task.description && (
                        <Typography variant="body2" color="text.secondary" noWrap sx={{ maxWidth: 300 }}>
                          {task.description}
                        </Typography>
                      )}
                    </TableCell>
                    <TableCell>
                      <Typography
                        color={!task.isCompleted && isOverdue(task.dueDate) ? 'error' : 'text.primary'}
                        fontWeight={!task.isCompleted && isOverdue(task.dueDate) ? 'bold' : 'normal'}
                      >
                        {formatDate(task.dueDate)}
                      </Typography>
                      {!task.isCompleted && isOverdue(task.dueDate) && (
                        <Typography variant="caption" color="error">
                          Overdue
                        </Typography>
                      )}
                    </TableCell>
                    <TableCell>
                      <Chip label={task.priority} color={priorityColors[task.priority]} size="small" />
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">{task.userFullName}</Typography>
                      <Typography variant="caption" color="text.secondary">
                        {task.userEmail}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                        {task.tags.map((tag) => (
                          <Chip key={tag.id} label={tag.name} size="small" variant="outlined" />
                        ))}
                      </Box>
                    </TableCell>
                    <TableCell align="center">
                      <Tooltip title={task.isCompleted ? 'Mark as active' : 'Mark as done'}>
                        <IconButton
                          color={task.isCompleted ? 'success' : 'default'}
                          onClick={() => handleToggleCompletion(task)}
                          size="small"
                        >
                          {task.isCompleted ? <CheckCircleIcon /> : <RadioButtonUncheckedIcon />}
                        </IconButton>
                      </Tooltip>
                      {!task.isCompleted && (
                        <Tooltip title="Edit">
                          <IconButton color="primary" onClick={() => handleEditOpen(task)} size="small">
                            <EditIcon />
                          </IconButton>
                        </Tooltip>
                      )}
                      <Tooltip title="Delete">
                        <IconButton color="error" onClick={() => handleDeleteOpen(task)} size="small">
                          <DeleteIcon />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
        <TablePagination
          component="div"
          count={displayedTasks.length}
          page={page}
          onPageChange={(_, newPage) => setPage(newPage)}
          rowsPerPage={rowsPerPage}
          onRowsPerPageChange={(e) => { setRowsPerPage(parseInt(e.target.value, 10)); setPage(0); }}
          rowsPerPageOptions={[5, 10, 25]}
        />
        </>
      )}

      {/* Task Form Dialog */}
      <TaskForm open={formOpen} onClose={() => setFormOpen(false)} onSubmit={handleFormSubmit} task={editingTask} />

      {/* Delete Confirmation Dialog */}
      <ConfirmDialog
        open={deleteDialogOpen}
        title="Delete Task"
        message={`Are you sure you want to delete "${taskToDelete?.title}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteDialogOpen(false)}
      />

      {/* Snackbar */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert severity={snackbar.severity} onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}

export default TaskList;
