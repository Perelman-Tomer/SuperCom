import { ThemeProvider, createTheme, CssBaseline, Container, AppBar, Toolbar, Typography, Box } from '@mui/material';
import { Assignment as TaskIcon } from '@mui/icons-material';
import TaskList from './components/TaskList';

const theme = createTheme({
  palette: {
    primary: { main: '#1976d2' },
    secondary: { main: '#dc004e' },
    background: { default: '#f5f5f5' },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
  },
});

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Box sx={{ flexGrow: 1 }}>
        <AppBar position="static" elevation={1}>
          <Toolbar>
            <TaskIcon sx={{ mr: 2 }} />
            <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
              Task Manager
            </Typography>
          </Toolbar>
        </AppBar>
        <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
          <TaskList />
        </Container>
      </Box>
    </ThemeProvider>
  );
}

export default App;
