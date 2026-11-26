import React, { useState, useEffect } from 'react';
import { AdminNavigation } from './AdminNavigation';
import { adminService } from '@/services/adminService';
import type { AuditLog, AuditLogDetail } from '@/types/admin';
import {
  Search,
  RefreshCw,
  Download,
  Eye,
  CheckCircle2,
  XCircle,
  X,
} from 'lucide-react';

export const AuditLogsPage: React.FC = () => {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [moduleFilter, setModuleFilter] = useState('');
  const [actionFilter, setActionFilter] = useState('');
  const [selectedDetail, setSelectedDetail] = useState<AuditLogDetail | null>(null);

  const modules = ['Auth', 'Users', 'Roles', 'Leads', 'Deals', 'Finance', 'Payroll', 'Workflows', 'Admin', 'General'];

  const fetchLogs = async () => {
    try {
      setLoading(true);
      const data = await adminService.getLogs({
        search: search.trim() || undefined,
        module: moduleFilter || undefined,
        action: actionFilter || undefined,
        pageSize: 50,
      });
      setLogs(data);
    } catch (err) {
      console.error('Failed to load audit logs:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchLogs();
  }, [moduleFilter, actionFilter]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    fetchLogs();
  };

  const openDetail = async (id: string) => {
    try {
      const detail = await adminService.getLogById(id);
      setSelectedDetail(detail);
    } catch (err: any) {
      alert(`Không thể tải chi tiết log: ${err?.message || 'Lỗi kết nối'}`);
    }
  };

  const handleExport = () => {
    const url = `/api/v1/admin/audit-logs/export?module=${moduleFilter}&action=${actionFilter}`;
    window.open(url, '_blank');
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-xs">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-50 text-blue-700 border border-blue-200">
              Kiểm toán & Tuân thủ
            </span>
            <h1 className="text-2xl font-bold text-slate-900">Nhật Ký Hoạt Động & Kiểm Toán (Audit Logs)</h1>
          </div>
          <p className="text-sm text-slate-500 mt-1">
            Ghi vết bất biến toàn bộ thao tác quan trọng: đăng nhập, thay đổi dữ liệu, cấp quyền, duyệt lương và các hành động quản trị.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={handleExport}
            className="flex items-center gap-1.5 px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-sm font-semibold shadow-xs transition"
          >
            <Download className="w-4 h-4" />
            Xuất Báo Cáo Audit (CSV)
          </button>
        </div>
      </div>

      <AdminNavigation />

      {/* Filter Toolbar */}
      <div className="flex flex-col sm:flex-row items-center justify-between gap-4 bg-white p-4 rounded-xl border border-slate-200">
        <form onSubmit={handleSearchSubmit} className="flex items-center gap-2 w-full sm:w-80">
          <div className="relative w-full">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Tìm theo user, thực thể..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-9 pr-3 py-1.5 text-xs rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <button type="submit" className="px-3 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-xs font-medium">
            Tìm
          </button>
        </form>

        <div className="flex items-center gap-3 w-full sm:w-auto justify-end">
          <select
            value={moduleFilter}
            onChange={(e) => setModuleFilter(e.target.value)}
            className="px-3 py-1.5 text-xs rounded-lg border border-slate-200 bg-white text-slate-700 focus:outline-none"
          >
            <option value="">Tất cả phân hệ (Modules)</option>
            {modules.map((m) => (
              <option key={m} value={m}>
                Phân hệ: {m}
              </option>
            ))}
          </select>

          <select
            value={actionFilter}
            onChange={(e) => setActionFilter(e.target.value)}
            className="px-3 py-1.5 text-xs rounded-lg border border-slate-200 bg-white text-slate-700 focus:outline-none"
          >
            <option value="">Tất cả hành động (Actions)</option>
            <option value="Create">Tạo mới (Create)</option>
            <option value="Update">Cập nhật (Update)</option>
            <option value="Delete">Xóa (Delete)</option>
            <option value="Export">Xuất dữ liệu (Export)</option>
            <option value="Login">Đăng nhập (Login)</option>
          </select>

          <button
            onClick={fetchLogs}
            className="p-2 text-slate-500 hover:text-slate-800 bg-slate-50 hover:bg-slate-100 rounded-lg border border-slate-200 transition"
            title="Làm mới"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
          </button>
        </div>
      </div>

      {/* Audit Logs Table */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
        {loading ? (
          <div className="flex items-center justify-center py-16">
            <RefreshCw className="w-8 h-8 animate-spin text-blue-600" />
          </div>
        ) : logs.length === 0 ? (
          <div className="text-center py-12 text-slate-400 text-sm">Chưa có nhật ký hoạt động nào được ghi nhận.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-slate-600">
              <thead className="bg-slate-50 text-slate-700 font-semibold uppercase border-b border-slate-200">
                <tr>
                  <th className="py-3 px-4">Thời điểm</th>
                  <th className="py-3 px-4">Người thực hiện</th>
                  <th className="py-3 px-4">Hành động</th>
                  <th className="py-3 px-4">Phân hệ</th>
                  <th className="py-3 px-4">Đối tượng tác động</th>
                  <th className="py-3 px-4">Địa chỉ IP</th>
                  <th className="py-3 px-4 text-center">Kết quả</th>
                  <th className="py-3 px-4 text-right">Chi tiết</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {logs.map((log) => (
                  <tr key={log.id} className="hover:bg-slate-50/60 transition">
                    <td className="py-3 px-4 font-mono text-[11px] text-slate-500 whitespace-nowrap">
                      {new Date(log.timestamp).toLocaleString('vi-VN')}
                    </td>
                    <td className="py-3 px-4 font-medium text-slate-900">{log.userEmail}</td>
                    <td className="py-3 px-4">
                      <span className="px-2 py-0.5 rounded text-[10px] font-semibold bg-slate-100 text-slate-800">
                        {log.action}
                      </span>
                    </td>
                    <td className="py-3 px-4">
                      <span className="px-2 py-0.5 rounded text-[10px] font-semibold bg-blue-50 text-blue-700">
                        {log.module}
                      </span>
                    </td>
                    <td className="py-3 px-4">
                      <div className="font-semibold text-slate-800">{log.entityName}</div>
                      {log.entityId && <div className="text-[10px] font-mono text-slate-400">{log.entityId}</div>}
                    </td>
                    <td className="py-3 px-4 font-mono text-[11px] text-slate-500">{log.ipAddress || '—'}</td>
                    <td className="py-3 px-4 text-center">
                      {log.isSuccess ? (
                        <CheckCircle2 className="w-4 h-4 text-emerald-500 inline-block" />
                      ) : (
                        <XCircle className="w-4 h-4 text-rose-500 inline-block" />
                      )}
                    </td>
                    <td className="py-3 px-4 text-right">
                      <button
                        onClick={() => openDetail(log.id)}
                        className="p-1.5 text-blue-600 hover:bg-blue-50 rounded"
                        title="Xem chi tiết"
                      >
                        <Eye className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modal: Audit Log Detail */}
      {selectedDetail && (
        <div className="fixed inset-0 bg-black/40 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl max-w-2xl w-full p-6 shadow-xl border border-slate-200 max-h-[90vh] overflow-y-auto">
            <div className="flex justify-between items-center mb-4 pb-2 border-b border-slate-100">
              <div>
                <h3 className="text-base font-bold text-slate-900">Chi Tiết Sự Kiện Kiểm Toán (Audit Event)</h3>
                <div className="text-xs text-slate-500 mt-0.5">Mã ID: {selectedDetail.id}</div>
              </div>
              <button onClick={() => setSelectedDetail(null)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="grid grid-cols-2 gap-3 text-xs mb-4">
              <div>
                <span className="text-slate-500">Thời gian:</span>
                <span className="ml-2 font-mono text-slate-800">{new Date(selectedDetail.timestamp).toLocaleString('vi-VN')}</span>
              </div>
              <div>
                <span className="text-slate-500">Người thực hiện:</span>
                <span className="ml-2 font-semibold text-slate-800">{selectedDetail.userEmail}</span>
              </div>
              <div>
                <span className="text-slate-500">Hành động:</span>
                <span className="ml-2 font-semibold text-blue-600">{selectedDetail.action} ({selectedDetail.module})</span>
              </div>
              <div>
                <span className="text-slate-500">Địa chỉ IP:</span>
                <span className="ml-2 font-mono text-slate-800">{selectedDetail.ipAddress || '—'}</span>
              </div>
              {selectedDetail.userAgent && (
                <div className="col-span-2">
                  <span className="text-slate-500">User Agent:</span>
                  <span className="ml-2 text-slate-700 font-mono text-[11px] truncate block">{selectedDetail.userAgent}</span>
                </div>
              )}
            </div>

            <div className="space-y-3">
              {selectedDetail.oldValuesJson && (
                <div>
                  <div className="text-xs font-semibold text-slate-700 mb-1">Dữ liệu trước thay đổi (Old Values):</div>
                  <pre className="p-3 bg-slate-50 border border-slate-200 rounded-lg text-[11px] font-mono text-slate-800 overflow-x-auto max-h-40">
                    {selectedDetail.oldValuesJson}
                  </pre>
                </div>
              )}

              {selectedDetail.newValuesJson && (
                <div>
                  <div className="text-xs font-semibold text-slate-700 mb-1">Dữ liệu sau thay đổi (New Values):</div>
                  <pre className="p-3 bg-slate-50 border border-slate-200 rounded-lg text-[11px] font-mono text-slate-800 overflow-x-auto max-h-40">
                    {selectedDetail.newValuesJson}
                  </pre>
                </div>
              )}
            </div>

            <div className="flex justify-end pt-4 mt-4 border-t border-slate-100">
              <button
                onClick={() => setSelectedDetail(null)}
                className="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-xs font-medium"
              >
                Đóng
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
