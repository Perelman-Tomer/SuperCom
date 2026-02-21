import { useEffect, useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  MenuItem,
  Grid,
  Autocomplete,
  Chip,
  Box,
} from '@mui/material';
import { useForm, Controller } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { taskValidationSchema } from '../utils/validation';
import { Priority } from '../utils/helpers';
import { useAppSelector, useAppDispatch } from '../store/hooks';
import { fetchTags } from '../store/tagSlice';

const priorities = Object.values(Priority);

function TaskForm({ open, onClose, onSubmit, task }) {
  const dispatch = useAppDispatch();
  const { items: tags } = useAppSelector((state) => state.tags);
  const [selectedTags, setSelectedTags] = useState([]);

  const {
    control,
    handleSubmit,
    reset,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: yupResolver(taskValidationSchema),
    defaultValues: {
      title: '',
      description: '',
      dueDate: '',
      priority: 'Medium',
      userFullName: '',
      userTelephone: '',
      userEmail: '',
      tagIds: [],
    },
  });

  useEffect(() => {
    dispatch(fetchTags());
  }, [dispatch]);

  useEffect(() => {
    if (task) {
      reset({
        title: task.title,
        description: task.description || '',
        dueDate: task.dueDate ? task.dueDate.substring(0, 16) : '',
        priority: task.priority,
        userFullName: task.userFullName,
        userTelephone: task.userTelephone,
        userEmail: task.userEmail,
        tagIds: task.tags.map((t) => t.id),
      });
      setSelectedTags(task.tags);
    } else {
      reset({
        title: '',
        description: '',
        dueDate: '',
        priority: 'Medium',
        userFullName: '',
        userTelephone: '',
        userEmail: '',
        tagIds: [],
      });
      setSelectedTags([]);
    }
  }, [task, reset]);

  const handleFormSubmit = (data) => {
    onSubmit(data);
  };

  const handleTagChange = (_event, value) => {
    setSelectedTags(value);
    setValue(
      'tagIds',
      value.map((t) => t.id)
    );
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <form onSubmit={handleSubmit(handleFormSubmit)}>
        <DialogTitle>{task ? 'Edit Task' : 'Create New Task'}</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 1 }}>
            <Grid container spacing={2}>
              {/* Title */}
              <Grid size={12}>
                <Controller
                  name="title"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Title"
                      fullWidth
                      required
                      error={!!errors.title}
                      helperText={errors.title?.message}
                    />
                  )}
                />
              </Grid>

              {/* Description */}
              <Grid size={12}>
                <Controller
                  name="description"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Description"
                      fullWidth
                      multiline
                      rows={3}
                      error={!!errors.description}
                      helperText={errors.description?.message}
                    />
                  )}
                />
              </Grid>

              {/* Due Date */}
              <Grid size={{ xs: 12, sm: 6 }}>
                <Controller
                  name="dueDate"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Due Date"
                      type="datetime-local"
                      fullWidth
                      required
                      slotProps={{ inputLabel: { shrink: true } }}
                      error={!!errors.dueDate}
                      helperText={errors.dueDate?.message}
                    />
                  )}
                />
              </Grid>

              {/* Priority */}
              <Grid size={{ xs: 12, sm: 6 }}>
                <Controller
                  name="priority"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Priority"
                      select
                      fullWidth
                      required
                      error={!!errors.priority}
                      helperText={errors.priority?.message}
                    >
                      {priorities.map((p) => (
                        <MenuItem key={p} value={p}>
                          {p}
                        </MenuItem>
                      ))}
                    </TextField>
                  )}
                />
              </Grid>

              {/* User Full Name */}
              <Grid size={{ xs: 12, sm: 6 }}>
                <Controller
                  name="userFullName"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Full Name"
                      fullWidth
                      required
                      error={!!errors.userFullName}
                      helperText={errors.userFullName?.message}
                    />
                  )}
                />
              </Grid>

              {/* Telephone */}
              <Grid size={{ xs: 12, sm: 6 }}>
                <Controller
                  name="userTelephone"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Telephone"
                      fullWidth
                      required
                      error={!!errors.userTelephone}
                      helperText={errors.userTelephone?.message}
                    />
                  )}
                />
              </Grid>

              {/* Email */}
              <Grid size={12}>
                <Controller
                  name="userEmail"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Email"
                      type="email"
                      fullWidth
                      required
                      error={!!errors.userEmail}
                      helperText={errors.userEmail?.message}
                    />
                  )}
                />
              </Grid>

              {/* Tags */}
              <Grid size={12}>
                <Autocomplete
                  multiple
                  options={tags}
                  getOptionLabel={(option) => option.name}
                  value={selectedTags}
                  onChange={handleTagChange}
                  isOptionEqualToValue={(option, value) => option.id === value.id}
                  renderTags={(value, getTagProps) =>
                    value.map((option, index) => {
                      const { key, ...otherProps } = getTagProps({ index });
                      return <Chip key={key} label={option.name} size="small" color="primary" variant="outlined" {...otherProps} />;
                    })
                  }
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Tags"
                      placeholder="Select tags..."
                      error={!!errors.tagIds}
                      helperText={errors.tagIds?.message}
                    />
                  )}
                />
              </Grid>
            </Grid>
          </Box>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={onClose} color="inherit">
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={isSubmitting}>
            {task ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}

export default TaskForm;
