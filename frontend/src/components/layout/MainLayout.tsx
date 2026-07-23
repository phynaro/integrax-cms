import { Sidebar } from './Sidebar';
import { Header } from './Header';

interface MainLayoutProps {
  children: React.ReactNode;
  user?: {
    displayName: string;
    email: string;
    role: string;
  };
}

export function MainLayout({ children, user }: MainLayoutProps) {
  return (
    <div className="flex h-screen overflow-hidden">
      <Sidebar userRole={user?.role} />
      <div className="flex flex-1 flex-col overflow-hidden">
        <Header user={user} />
        <main className="flex-1 overflow-auto p-6">
          {children}
        </main>
      </div>
    </div>
  );
}
