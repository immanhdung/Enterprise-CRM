import React, { useState, useEffect } from 'react';
import { AdminNavigation } from './AdminNavigation';
import { adminService } from '@/services/adminService';
import type { RolePermissionMatrix } from '@/types/admin';
import {
  RefreshCw,
} from 'lucide-react';

export const PermissionMatrixPage: React.FC = () => {
  const [matrix, setMatrix] = useState<RolePermissionMatrix | null>(null);
  const [loading, setLoading] = useState(true);
  const [savingKey, setSavingKey] = useState<string | null>(null);

  const fetchMatrix = async () => {
    try {
      setLoading(true);
      const data = await adminService.getPermissionMatrix();
      setMatrix(data);
    } catch (err) {
      console.error('Failed to load permission matrix:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchMatrix();
  }, []);

  const handleTogglePermission = async (roleName: string, permissionCode: string, currentGranted: boolean, currentScope: string) => {
    const key = `${roleName}_${permissionCode}`;
    setSavingKey(key);

    try {
      await adminService.updateMatrixEntry(roleName, permissionCode, !currentGranted, currentScope);
      // Update local state smoothly
      setMatrix((prev) => {
        if (!prev) return null;
        const newRows = prev.rows.map((row) => {
          if (row.permissionCode === permissionCode) {
            return {
              ...row,
              roles: {
                ...row.roles,
                [roleName]: {
                  ...row.roles[roleName],
                  hasPermission: !currentGranted,
                },
              },
            };
          }
          return row;
        });
        return { ...prev, rows: newRows };
      });
    } catch (err: any) {
      alert(`Lỗi cập nhật quyền: ${err?.message || 'Có lỗi xảy ra'}`);
    } finally {
      setSavingKey(null);
    }
  };

  const handleChangeScope = async (roleName: string, permissionCode: string, newScope: string) => {
    const key = `${roleName}_${permissionCode}_scope`;
    setSavingKey(key);

    try {
      await adminService.updateMatrixEntry(roleName, permissionCode, true, newScope);
      setMatrix((prev) => {
        if (!prev) return null;
        const newRows = prev.rows.map((row) => {
          if (row.permissionCode === permissionCode) {
            return {
              ...row,
              roles: {
                ...row.roles,
                [roleName]: {
                  ...row.roles[roleName],
                  hasPermission: true,
                  dataScope: newScope,
                },
              },
            };
          }
          return row;
        });
        return { ...prev, rows: newRows };
      });
    } catch (err: any) {
      alert(`Lỗi cập nhật phạm vi dữ liệu: ${err?.message || 'Có lỗi xảy ra'}`);
    } finally {
      setSavingKey(null);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-xs">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-purple-50 text-purple-700 border border-purple-200">
              Bảo mật & Phân quyền
            </span>
            <h1 className="text-2xl font-bold text-slate-900">Ma trận Phân quyền & Phạm vi Dữ liệu (RBAC Matrix)</h1>
          </div>
          <p className="text-sm text-slate-500 mt-1">
            Giao diện trực quan ma trận phân quyền giữa tất cả các Vai trò và Quyền hạn. Kiểm soát chi tiết Data Scope (Own, Assigned, Team, Department, Company).
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={fetchMatrix}
            className="flex items-center gap-1.5 px-3.5 py-2 text-sm font-medium text-slate-700 bg-slate-50 hover:bg-slate-100 rounded-lg border border-slate-200 transition"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            Làm mới
          </button>
        </div>
      </div>

      <AdminNavigation />

      {/* Permission Matrix Table */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
        {loading || !matrix ? (
          <div className="flex items-center justify-center py-20">
            <RefreshCw className="w-8 h-8 animate-spin text-purple-600" />
          </div>
        ) : (
          <div className="overflow-x-auto max-h-[700px]">
            <table className="w-full text-left text-xs text-slate-600 border-collapse">
              <thead className="bg-slate-100 text-slate-700 font-bold uppercase sticky top-0 z-10 border-b border-slate-200">
                <tr>
                  <th className="py-3 px-4 min-w-[200px]">Quyền hạn (Permission)</th>
                  <th className="py-3 px-4 min-w-[120px]">Module</th>
                  {matrix.roleNames.map((r) => (
                    <th key={r} className="py-3 px-4 text-center min-w-[150px] border-l border-slate-200">
                      {r}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {matrix.rows.map((row) => (
                  <tr key={row.permissionCode} className="hover:bg-purple-50/20 transition">
                    <td className="py-3 px-4">
                      <div className="font-mono font-semibold text-slate-900">{row.permissionCode}</div>
                      <div className="text-[11px] text-slate-500 mt-0.5">{row.description}</div>
                    </td>
                    <td className="py-3 px-4">
                      <span className="px-2 py-0.5 rounded text-[10px] font-semibold bg-slate-100 text-slate-700">
                        {row.module}
                      </span>
                    </td>
                    {matrix.roleNames.map((roleName) => {
                      const entry = row.roles[roleName];
                      const isSaving = savingKey === `${roleName}_${row.permissionCode}` || savingKey === `${roleName}_${row.permissionCode}_scope`;

                      return (
                        <td key={roleName} className="py-3 px-4 text-center border-l border-slate-100">
                          <div className="flex flex-col items-center justify-center gap-1.5">
                            <label className="relative inline-flex items-center cursor-pointer">
                              <input
                                type="checkbox"
                                disabled={roleName === 'Admin' || isSaving}
                                checked={entry?.hasPermission || false}
                                onChange={() => handleTogglePermission(roleName, row.permissionCode, entry?.hasPermission || false, entry?.dataScope || 'Company')}
                                className="w-4 h-4 rounded text-purple-600 focus:ring-purple-500 cursor-pointer disabled:opacity-50"
                              />
                            </label>

                            {entry?.hasPermission && (
                              <select
                                disabled={roleName === 'Admin' || isSaving}
                                value={entry.dataScope || 'Company'}
                                onChange={(e) => handleChangeScope(roleName, row.permissionCode, e.target.value)}
                                className="text-[10px] font-medium py-0.5 px-1.5 rounded border border-slate-200 bg-white text-slate-700 focus:outline-none"
                              >
                                <option value="Company">Company (Toàn cty)</option>
                                <option value="Department">Department (Phòng ban)</option>
                                <option value="Team">Team (Đội nhóm)</option>
                                <option value="Assigned">Assigned (Được giao)</option>
                                <option value="Own">Own (Cá nhân)</option>
                              </select>
                            )}
                          </div>
                        </td>
                      );
                    })}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};
