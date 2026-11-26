import React from 'react';
import { NavLink } from 'react-router-dom';
import { Bot, MessageSquareCode, TrendingUp, AlertOctagon, Lightbulb, Sliders } from 'lucide-react';

export const AiNavigation: React.FC = () => {
  const navItems = [
    { to: '/ai-assistant', label: 'Trợ lý AI (Copilot Chat)', icon: Bot },
    { to: '/ai-assistant/query', label: 'Truy vấn Tự nhiên (NL Query)', icon: MessageSquareCode },
    { to: '/ai-analytics/overview', label: 'Phân tích Hiệu suất', icon: TrendingUp },
    { to: '/ai-analytics/forecast', label: 'Dự báo Doanh thu', icon: TrendingUp },
    { to: '/ai-analytics/anomalies', label: 'Phát hiện Bất thường', icon: AlertOctagon },
    { to: '/ai-analytics/recommendations', label: 'Đề xuất Hành động', icon: Lightbulb },
    { to: '/ai-assistant/settings', label: 'Cấu hình Provider', icon: Sliders },
  ];

  return (
    <div className="flex border-b border-gray-200 bg-white px-6 pt-4 mb-6 overflow-x-auto">
      <div className="flex space-x-6">
        {navItems.map((item) => {
          const Icon = item.icon;
          return (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === '/ai-assistant'}
              className={({ isActive }) =>
                `flex items-center gap-2 pb-3 px-1 text-sm font-medium border-b-2 transition-colors whitespace-nowrap ${
                  isActive
                    ? 'border-purple-600 text-purple-600'
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
