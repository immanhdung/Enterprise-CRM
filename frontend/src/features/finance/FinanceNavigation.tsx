import React from 'react';
import { NavLink } from 'react-router-dom';
import {
  LayoutDashboard,
  Receipt,
  CreditCard,
  FileCheck2,
  AlertCircle,
  TrendingUp,
  Scale,
} from 'lucide-react';
import { cn } from '@/utils/cn';

interface NavItem {
  name: string;
  path: string;
  icon: React.ElementType;
}

const navItems: NavItem[] = [
  { name: 'Overview', path: '/finance', icon: LayoutDashboard },
  { name: 'Contract Schedules', path: '/finance/contracts', icon: FileCheck2 },
  { name: 'Payments & Cash', path: '/finance/payments', icon: CreditCard },
  { name: 'E-Invoices', path: '/finance/invoices', icon: Receipt },
  { name: 'Debt & Aging', path: '/finance/receivables', icon: AlertCircle },
  { name: 'Revenue Analytics', path: '/finance/revenue', icon: TrendingUp },
  { name: 'Reconciliation', path: '/finance/reconciliation', icon: Scale },
];

export const FinanceNavigation: React.FC = () => {
  return (
    <div className="bg-white border-b border-slate-200 px-6 py-2">
      <div className="flex items-center space-x-1 overflow-x-auto no-scrollbar">
        {navItems.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            end={item.path === '/finance'}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-2 px-3.5 py-2 text-sm font-medium rounded-lg transition-colors whitespace-nowrap',
                isActive
                  ? 'bg-blue-50 text-blue-700 font-semibold'
                  : 'text-slate-600 hover:text-slate-900 hover:bg-slate-50'
              )
            }
          >
            <item.icon className="w-4 h-4" />
            <span>{item.name}</span>
          </NavLink>
        ))}
      </div>
    </div>
  );
};
