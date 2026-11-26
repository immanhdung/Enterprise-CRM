import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Sliders,
  Search,
  ShieldCheck,
  User,
  Clock,
} from 'lucide-react';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';
import type { CompensationAuditLog } from '@/types/performance';

export const CommissionAdjustmentsPage: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [entityFilter, setEntityFilter] = useState('');

  const { data: logs, isLoading } = useQuery({
    queryKey: ['compensation', 'audit-logs', entityFilter],
    queryFn: () =>
      performanceService.getCompensationAuditLogs({
        entityType: entityFilter || undefined,
      }),
  });

  const filteredLogs = (logs || []).filter((log: CompensationAuditLog) => {
    if (!searchTerm) return true;
    const term = searchTerm.toLowerCase();
    return (
      log.reason.toLowerCase().includes(term) ||
      log.action.toLowerCase().includes(term) ||
      log.entityType.toLowerCase().includes(term)
    );
  });

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <PerformanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <div className="flex items-center gap-2">
              <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-100 text-blue-800">
                Nhật ký kiểm toán & Điều chỉnh
              </span>
              <span className="text-xs text-slate-500">Module G – Audit & Governance</span>
            </div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight mt-1 flex items-center gap-2">
              <Sliders className="w-6 h-6 text-blue-600" />
              <span>Nhật Ký Điều Chỉnh & Kiểm Toán Thu Nhập</span>
            </h1>
            <p className="text-sm text-slate-500 mt-1">
              Ghi nhận toàn bộ thao tác can thiệp thủ công đối với hoa hồng, KPI, thưởng và bảng lương với lý do bắt buộc.
            </p>
          </div>
        </div>

        {/* Filters */}
        <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex flex-wrap items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <select
              value={entityFilter}
              onChange={(e) => setEntityFilter(e.target.value)}
              className="px-3 py-1.5 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Tất cả đối tượng (All Entities)</option>
              <option value="Commission">Hoa hồng (Commission)</option>
              <option value="Bonus">Thưởng (Bonus)</option>
              <option value="Payroll">Bảng lương (Payroll)</option>
              <option value="KPI">Chỉ tiêu (KPI)</option>
            </select>
          </div>

          <div className="flex items-center gap-3">
            <div className="relative">
              <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
              <input
                type="text"
                placeholder="Tìm lý do, hành động..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-9 pr-3 py-1.5 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500 w-64"
              />
            </div>
          </div>
        </div>

        {/* Audit Logs Table */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
          <div className="p-4 border-b border-slate-100 flex items-center justify-between">
            <h3 className="text-sm font-semibold text-slate-900 flex items-center gap-2">
              <ShieldCheck className="w-4 h-4 text-blue-600" />
              <span>Dòng vết kiểm toán thu nhập (Compensation Audit Trail)</span>
            </h3>
            <span className="text-xs text-slate-400">Hiển thị {filteredLogs.length} bản ghi</span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-600">
              <thead className="bg-slate-50 text-xs font-semibold text-slate-500 uppercase tracking-wider border-b border-slate-200">
                <tr>
                  <th className="px-5 py-3.5">Thời gian</th>
                  <th className="px-5 py-3.5">Đối tượng</th>
                  <th className="px-5 py-3.5">Hành động</th>
                  <th className="px-5 py-3.5">Người thực hiện</th>
                  <th className="px-5 py-3.5">Lý do điều chỉnh (Audit Reason)</th>
                  <th className="px-5 py-3.5">Giá trị cũ / mới</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {isLoading ? (
                  <tr>
                    <td colSpan={6} className="px-5 py-10 text-center text-slate-400">
                      Đang tải dữ liệu kiểm toán...
                    </td>
                  </tr>
                ) : filteredLogs.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="px-5 py-10 text-center text-slate-400">
                      Không có bản ghi điều chỉnh nào.
                    </td>
                  </tr>
                ) : (
                  filteredLogs.map((log: CompensationAuditLog) => (
                    <tr key={log.id} className="hover:bg-slate-50/80 transition-colors">
                      <td className="px-5 py-3.5 text-xs text-slate-500 whitespace-nowrap">
                        <div className="flex items-center gap-1.5">
                          <Clock className="w-3.5 h-3.5 text-slate-400" />
                          <span>{new Date(log.timestamp).toLocaleString('vi-VN')}</span>
                        </div>
                      </td>
                      <td className="px-5 py-3.5">
                        <span className="px-2 py-0.5 rounded text-xs font-bold bg-slate-100 text-slate-800">
                          {log.entityType}
                        </span>
                      </td>
                      <td className="px-5 py-3.5 font-medium text-slate-900">
                        {log.action}
                      </td>
                      <td className="px-5 py-3.5 text-xs text-slate-700">
                        <div className="flex items-center gap-1.5">
                          <User className="w-3.5 h-3.5 text-slate-400" />
                          <span>{log.actorName || log.actorUserId}</span>
                        </div>
                      </td>
                      <td className="px-5 py-3.5 text-xs text-slate-800 font-medium max-w-xs">
                        {log.reason}
                      </td>
                      <td className="px-5 py-3.5 text-xs text-slate-500 font-mono">
                        {log.oldValueJson || log.newValueJson ? (
                          <div className="truncate max-w-xs">
                            <span className="text-slate-400">{log.oldValueJson || 'None'}</span> &rarr;{' '}
                            <span className="text-blue-600 font-bold">{log.newValueJson || 'None'}</span>
                          </div>
                        ) : (
                          '-'
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
};
