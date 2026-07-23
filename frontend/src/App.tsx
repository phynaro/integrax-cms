import { MainLayout } from '@/components/layout';

const mockUser = {
  displayName: 'Admin User',
  email: 'admin@example.com',
  role: 'SystemAdmin',
};

function App() {
  return (
    <MainLayout user={mockUser}>
      <div>
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <p className="text-muted-foreground">Welcome to Debt Collection System</p>
      </div>
    </MainLayout>
  );
}

export default App;
