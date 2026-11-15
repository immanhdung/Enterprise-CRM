import React from 'react';
import { NavLink } from 'react-router-dom';
import {
  LayoutDashboard,
  UserPlus,
  Building2,
  Users,
  KanbanSquare,
  CalendarCheck,
  FileSpreadsheet,
  FileCheck,
  Briefcase,
  Receipt,
  CircleDollarSign,
  Cpu,
  Sparkles,
  Settings,
  ChevronLeft,
  ChevronRight,
  ShieldCheck,
} from 'lucide-react';
import { useUIStore } from '@/stores/uiStore';
import { useAuthStore } from '@/stores/authStore';
import { cn } from '@/utils/cn';

interface NavItem {
  name: string;
  path: string;
  icon: React.ElementType;
  badge?: string;
  requiredRoles?: string[];
}

interface NavGroup {
  title: string;
  items: NavItem[];
}

const navigationGroups: NavGroup[] = [
  {
    title: 'CRM CORE',
    items: [
      { name: 'Dashboard', path: '/', icon: LayoutDashboard },
      { name: 'Leads', path: '/leads', icon: UserPlus, badge: 'Omnichannel' },
      { name: 'Accounts', path: '/accounts', icon: Building2 },
      { name: 'Contacts', path: '/contacts', icon: Users },
      { name: 'Pipeline Kanban', path: '/pipeline', icon: KanbanSquare, badge: 'DND' },
      { name: 'Deals & Opps', path: '/deals', icon: Briefcase },
      { name: 'Action Tasks', path: '/tasks', icon: CalendarCheck },
      { name: 'Appointments', path: '/appointments', icon: CalendarCheck },
      { name: 'Activity Stream', path: '/activities', icon: CalendarCheck },
    ],
  },
  {
    title: 'OPERATIONS & CPQ',
    items: [
      { name: 'Quotations', path: '/quotations', icon: FileSpreadsheet },
      { name: 'Contracts & E-Sign', path: '/contracts', icon: FileCheck },
      { name: 'Projects', path: '/projects', icon: Briefcase },
      { name: 'Finance & Invoices', path: '/finance', icon: Receipt, requiredRoles: ['Admin', 'CEO_BOD', 'Accountant'] },
    ],
  },
  {
    title: 'PERFORMANCE & AI',
    items: [
      { name: 'Commissions & Payroll', path: '/payroll', icon: CircleDollarSign, requiredRoles: ['Admin', 'CEO_BOD', 'Manager', 'Accountant'] },
      { name: 'Workflows & Jobs', path: '/workflows', icon: Cpu, requiredRoles: ['Admin', 'CEO_BOD', 'Manager'] },
      { name: 'AI Assistant', path: '/ai-assistant', icon: Sparkles, badge: 'AI' },
      { name: 'Administration', path: '/admin', icon: Settings, requiredRoles: ['Admin', 'CEO_BOD'] },
    ],
  },
];

export const Sidebar: React.FC = () => {
  const { isSidebarCollapsed, toggleSidebar } = useUIStore();
  const { hasRole, user } = useAuthStore();

  const isItemVisible = (item: NavItem) => {
    if (!item.requiredRoles || item.requiredRoles.length === 0) return true;
    if (user?.roles.includes('Admin')) return true;
    return item.requiredRoles.some((role) => hasRole(role));
  };

  return (
    <aside
      className={cn(
        'bg-white border-r border-slate-200 h-screen sticky top-0 flex flex-col transition-all duration-300 z-30 select-none shadow-xs',
        isSidebarCollapsed ? 'w-18' : 'w-64'
      )}
    >
      {/* Brand Header */}
      <div className="h-16 flex items-center justify-between px-4 border-b border-slate-100">
        <div className="flex items-center gap-3 overflow-hidden">
          <div className="w-9 h-9 rounded-lg bg-blue-600 flex items-center justify-center text-white font-bold shrink-0 shadow-sm shadow-blue-500/20">
            <ShieldCheck className="w-5 h-5" />
          </div>
          {!isSidebarCollapsed && (
            <div className="flex flex-col truncate">
              <span className="font-semibold text-sm text-slate-900 tracking-tight">Enterprise CRM</span>
              <span className="text-[11px] text-blue-600 font-medium">Service Edition</span>
            </div>
          )}
        </div>
        <button
          onClick={toggleSidebar}
          className="p-1.5 rounded-md hover:bg-slate-100 text-slate-400 hover:text-slate-600 transition-colors"
          title={isSidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        >
          {isSidebarCollapsed ? <ChevronRight className="w-4 h-4" /> : <ChevronLeft className="w-4 h-4" />}
        </button>
      </div>

      {/* Navigation List */}
      <div className="flex-1 overflow-y-auto py-4 px-3 space-y-6 scrollbar-thin">
        {navigationGroups.map((group, groupIdx) => {
          const visibleItems = group.items.filter(isItemVisible);
          if (visibleItems.length === 0) return null;

          return (
            <div key={groupIdx} className="space-y-1">
              {!isSidebarCollapsed && (
                <h3 className="px-3 text-[11px] font-semibold text-slate-400 uppercase tracking-wider mb-2">
                  {group.title}
                </h3>
              )}
              {visibleItems.map((item) => {
                const Icon = item.icon;
                return (
                  <NavLink
                    key={item.path}
                    to={item.path}
                    className={({ isActive }) =>
                      cn(
                        'flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors group relative',
                        isActive
                          ? 'bg-blue-50 text-blue-700 font-semibold shadow-xs'
                          : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900'
                      )
                    }
                  >
                    <Icon className="w-4 h-4 shrink-0 transition-transform group-hover:scale-105" />
                    {!isSidebarCollapsed && (
                      <span className="truncate flex-1 text-left">{item.name}</span>
                    )}
                    {!isSidebarCollapsed && item.badge && (
                      <span className="text-[10px] uppercase font-bold px-1.5 py-0.5 rounded bg-blue-100 text-blue-700">
                        {item.badge}
                      </span>
                    )}
                  </NavLink>
                );
              })}
            </div>
          );
        })}
      </div>

      {/* Footer System Status Indicator */}
      <div className="p-3 border-t border-slate-100 bg-slate-50/50">
        <div className="flex items-center gap-2.5 px-2 py-1.5">
          <div className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse shrink-0" />
          {!isSidebarCollapsed && (
            <div className="flex flex-col text-xs text-slate-500 truncate">
              <span className="font-medium text-slate-700">Supabase Connected</span>
              <span className="text-[10px] text-slate-400">PostgreSQL v17</span>
            </div>
          )}
        </div>
      </div>
    </aside>
  );
};
