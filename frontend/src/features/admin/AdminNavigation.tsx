import React from 'react';
import { NavLink } from 'react-router-dom';
import {
  Users,
  Building2,
  ShieldCheck,
  FileText,
  FileSpreadsheet,
  Webhook,
  Database,
  Settings,
  Grid3X3,
} from 'lucide-react';

export const AdminNavigation: React.FC = () => {
  const tabs = [
    { name: 'Người dùng', path: '/admin/users', icon: Users },
    { name: 'Cơ cấu Tổ chức', path: '/admin/organization', icon: Building2 },
    { name: 'Vai trò & Quyền', path: '/admin/roles', icon: ShieldCheck },
    { name: 'Ma trận Phân quyền', path: '/admin/permission-matrix', icon: Grid3X3 },
    { name: 'Nhật ký Hoạt động (Audit)', path: '/admin/audit-logs', icon: FileText },
    { name: 'Dữ liệu (Import/Export)', path: '/admin/data-management', icon: FileSpreadsheet },
    { name: 'API & Webhooks', path: '/admin/integrations', icon: Webhook },
    { name: 'Sao lưu & Phục hồi', path: '/admin/backups', icon: Database },
    { name: 'Cấu hình Hệ thống', path: '/admin/settings', icon: Settings },
  ];

  return (
    <div className="flex items-center gap-1 overflow-x-auto border-b border-slate-200 pb-2 mb-6 scrollbar-none">
      {tabs.map((tab) => {
        const Icon = tab.icon;
        return (
          <NavLink
            key={tab.path}
            to={tab.path}
            className={({ isActive }) =>
              `flex items-center gap-2 px-3.5 py-2 rounded-lg text-xs font-semibold whitespace-nowrap transition-colors ${
                isActive
                  ? 'bg-blue-600 text-white shadow-xs'
                  : 'text-slate-600 hover:text-slate-900 hover:bg-slate-100'
              }`
            }
          >
            <Icon className="w-4 h-4" />
            <span>{tab.name}</span>
          </NavLink>
        );
      })}
    </div>
  );
};
