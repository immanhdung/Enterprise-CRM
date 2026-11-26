import React from 'react';
import { NavLink } from 'react-router-dom';
import { GitBranch, History, Users, Bell } from 'lucide-react';

export const AutomationNavigation: React.FC = () => {
  const navItems = [
    { to: '/automation/workflows', label: 'Quy trình Tự động (Workflows)', icon: GitBranch },
    { to: '/automation/executions', label: 'Lịch sử Thực thi (Executions)', icon: History },
    { to: '/automation/lead-distribution', label: 'Phân phối Lead (Lead Distribution)', icon: Users },
    { to: '/automation/alerts', label: 'Trung tâm Cảnh báo (Alerts Center)', icon: Bell },
  ];

  return (
    <div className="flex border-b border-gray-200 bg-white px-6 pt-4 mb-6">
      <div className="flex space-x-6">
        {navItems.map((item) => {
          const Icon = item.icon;
          return (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `flex items-center gap-2 pb-3 px-1 text-sm font-medium border-b-2 transition-colors ${
                  isActive
                    ? 'border-blue-600 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
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
