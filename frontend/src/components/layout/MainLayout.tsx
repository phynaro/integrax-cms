import { Outlet } from '@tanstack/react-router';
import { Sidebar } from './Sidebar';
import { Header } from './Header';
import { useAuth } from '@/features/auth';

export function MainLayout() {
  const { user } = useAuth();

  const userInfo = user ? {
    displayName: user.fullName,
    email: user.email,
    role: user.role,
  } : undefined;

  return (
    <div className="flex h-screen overflow-hidden">
      <Sidebar userRole={userInfo?.role} />
      <div className="flex flex-1 flex-col overflow-hidden">
        <Header user={userInfo} />
        <main className="flex-1 overflow-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
