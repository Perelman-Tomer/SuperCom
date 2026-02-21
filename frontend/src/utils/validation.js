import * as yup from 'yup';

export const taskValidationSchema = yup.object({
  title: yup.string().required('Title is required').max(200, 'Title must not exceed 200 characters'),
  description: yup.string().max(2000, 'Description must not exceed 2000 characters').nullable(),
  dueDate: yup.string().required('Due date is required'),
  priority: yup.string().required('Priority is required').oneOf(['Low', 'Medium', 'High', 'Critical']),
  userFullName: yup.string().required('Full name is required').max(150, 'Name must not exceed 150 characters'),
  userTelephone: yup
    .string()
    .required('Telephone is required')
    .matches(/^[+]?[0-9\-\s()]{7,20}$/, 'Please enter a valid phone number'),
  userEmail: yup.string().required('Email is required').email('Please enter a valid email address').max(254),
  tagIds: yup.array().of(yup.number().required()).defined(),
});
