import React from 'react';
import { NavLink } from 'react-router-dom';
import {
  TrendingUp,
  Target,
  Trophy,
  Percent,
  Award,
  Wallet,
  CheckSquare,
  FileCheck,
} from 'lucide-react';

interface NavItem {
  to: string;
  label: string;
  icon: React.ElementType;
}

export const PerformanceNavigation: React.FC = () => {
  const navItems: NavItem[] = [
    { to: '/performance/dashboard', label: 'Dashboard', icon: TrendingUp },
    { to: '/performance/kpis', label: 'KPI Management', icon: Target },
    { to: '/performance/assignments', label: 'Assign Targets', icon: CheckSquare },
    { to: '/performance/rankings', label: 'Sales Rankings', icon: Trophy },
    { to: '/commissions', label: 'Commissions', icon: Percent },
    { to: '/commissions/policies', label: 'Commission Policies', icon: FileCheck },
    { to: '/bonuses', label: 'Bonuses', icon: Award },
    { to: '/payroll', label: 'Payroll Management', icon: Wallet },
  ];

  return (
    <div className="bg-white border-b border-slate-200 px-6 py-2 shadow-xs">
      <div className="flex items-center gap-2 overflow-x-auto no-scrollbar">
        {navItems.map((item) => {
          const Icon = item.icon;
          return (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `flex items-center gap-2 px-3.5 py-2 rounded-lg text-xs font-semibold whitespace-nowrap transition-colors ${
                  isActive
                    ? 'bg-blue-50 text-blue-700 border border-blue-200'
                    : 'text-slate-600 hover:text-slate-900 hover:bg-slate-100'
                }`
              }
            >
              <Icon className="w-4 h-4" />
              <span>{item.label}</span>
            </NavLink>
          );
        })}
      </div>
    </div>
  );
};
