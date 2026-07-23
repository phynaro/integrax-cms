import { useState } from 'react';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import {
  LayoutDashboard,
  Users,
  Building2,
  FolderOpen,
  ClipboardList,
  ChevronLeft,
  ChevronRight,
} from 'lucide-react';

interface NavItem {
  title: string;
  href: string;
  icon: React.ComponentType<{ className?: string }>;
  roles?: string[];
}

const mainNav: NavItem[] = [
  { title: 'Dashboard', href: '/', icon: LayoutDashboard },
  { title: 'Clients', href: '/clients', icon: Building2 },
  { title: 'Portfolios', href: '/portfolios', icon: FolderOpen },
];

const adminNav: NavItem[] = [
  { title: 'Users', href: '/admin/users', icon: Users, roles: ['SystemAdmin'] },
  { title: 'Audit Log', href: '/admin/audit', icon: ClipboardList, roles: ['SystemAdmin', 'Manager'] },
];

interface SidebarProps {
  userRole?: string;
}

export function Sidebar({ userRole = 'SystemAdmin' }: SidebarProps) {
  const [collapsed, setCollapsed] = useState(false);
  const currentPath = window.location.pathname;

  const canAccess = (item: NavItem) => {
    if (!item.roles) return true;
    return item.roles.includes(userRole);
  };

  const NavLink = ({ item }: { item: NavItem }) => {
    const isActive = currentPath === item.href;
    const Icon = item.icon;

    return (
      <a
        href={item.href}
        className={cn(
          'flex items-center gap-3 rounded-lg px-3 py-2 text-sm transition-colors',
          isActive
            ? 'bg-primary text-primary-foreground'
            : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
          collapsed && 'justify-center px-2'
        )}
        title={collapsed ? item.title : undefined}
      >
        <Icon className="h-4 w-4 shrink-0" />
        {!collapsed && <span>{item.title}</span>}
      </a>
    );
  };

  return (
    <aside
      className={cn(
        'flex h-screen flex-col border-r bg-background transition-all duration-300',
        collapsed ? 'w-16' : 'w-64'
      )}
    >
      <div className="flex h-14 items-center border-b px-4">
        {!collapsed && <span className="font-semibold">Debt Collection</span>}
      </div>

      <nav className="flex-1 space-y-1 p-2">
        {mainNav.map((item) => (
          <NavLink key={item.href} item={item} />
        ))}

        {adminNav.some(canAccess) && (
          <>
            <div className="my-2 px-3">
              {!collapsed && (
                <span className="text-xs font-medium text-muted-foreground">ADMIN</span>
              )}
              {collapsed && <div className="border-t" />}
            </div>
            {adminNav.filter(canAccess).map((item) => (
              <NavLink key={item.href} item={item} />
            ))}
          </>
        )}
      </nav>

      <div className="border-t p-2">
        <Button
          variant="ghost"
          size="sm"
          className="w-full justify-center"
          onClick={() => setCollapsed(!collapsed)}
        >
          {collapsed ? <ChevronRight className="h-4 w-4" /> : <ChevronLeft className="h-4 w-4" />}
        </Button>
      </div>
    </aside>
  );
}
