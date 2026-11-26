import React, { useState, useEffect } from 'react';
import { AdminNavigation } from './AdminNavigation';
import { adminService } from '@/services/adminService';
import type { DataImportJob, DataExportJob, ImportExecutionResult } from '@/types/admin';
import {
  FileSpreadsheet,
  Upload,
  Download,
  RefreshCw,
  CheckCircle2,
  AlertTriangle,
  Play,
} from 'lucide-react';

export const DataManagementPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'import' | 'export'>('import');
  const [importJobs, setImportJobs] = useState<DataImportJob[]>([]);
  const [exportJobs, setExportJobs] = useState<DataExportJob[]>([]);
  const [loading, setLoading] = useState(true);

  // Import State
  const [entityType, setEntityType] = useState('Leads');
  const [rawJsonText, setRawJsonText] = useState(`[
  { "FullName": "Nguyễn Văn An", "Email": "an.nguyen@techcorp.vn", "Phone": "0912345678", "Company": "Tech Corp VN" },
  { "FullName": "Trần Thị Mai", "Email": "mai.tran@agency.io", "Phone": "0987654321", "Company": "Creative Agency" }
]`);
  const [processing, setProcessing] = useState(false);
  const [importResult, setImportResult] = useState<ImportExecutionResult | null>(null);

  // Export State
  const [exportEntity, setExportEntity] = useState('Leads');
  const [exportFormat, setExportFormat] = useState('Excel');

  const fetchData = async () => {
    try {
      setLoading(true);
      const [imp, exp] = await Promise.all([
        adminService.getImportJobs(),
        adminService.getExportJobs(),
      ]);
      setImportJobs(imp);
      setExportJobs(exp);
    } catch (err) {
      console.error('Failed to load data jobs:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleExecuteImport = async () => {
    setProcessing(true);
    setImportResult(null);

    try {
      let rows: any[] = [];
      try {
        rows = JSON.parse(rawJsonText);
        if (!Array.isArray(rows)) throw new Error('Data must be a JSON array of objects.');
      } catch (parseErr: any) {
        alert(`Dữ liệu JSON không hợp lệ: ${parseErr.message}`);
        setProcessing(false);
        return;
      }

      const res = await adminService.processImport(entityType, `${entityType.toLowerCase()}_import_${Date.now()}.json`, rows);
      setImportResult(res);
      fetchData();
    } catch (err: any) {
      alert(`Lỗi thực thi Import: ${err?.message || 'Có lỗi xảy ra'}`);
    } finally {
      setProcessing(false);
    }
  };

  const handleInitiateExport = async () => {
    try {
      const res = await adminService.initiateExport(exportEntity, exportFormat);
      alert(`Đã hoàn tất kết xuất dữ liệu cho '${exportEntity}'. Bắt đầu tải file...`);
      if (res.fileUrl) {
        window.open(res.fileUrl, '_blank');
      }
      fetchData();
    } catch (err: any) {
      alert(`Lỗi kết xuất dữ liệu: ${err?.message || 'Có lỗi xảy ra'}`);
    }
  };

  const handleDownloadSample = () => {
    let sample = '';
    if (entityType === 'Leads') {
      sample = 'FullName,Email,Phone,Company\n"Nguyễn Văn An","an.nguyen@techcorp.vn","0912345678","Tech Corp"\n"Trần Thị Mai","mai.tran@agency.io","0987654321","Agency VN"';
    } else {
      sample = 'Name,Email,Phone\n"Công ty TNHH Phát triển Phần mềm ABC","contact@abc.vn","0241234567"';
    }
    const blob = new Blob([sample], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `sample_${entityType.toLowerCase()}_template.csv`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-xs">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
              Quản trị Dữ liệu
            </span>
            <h1 className="text-2xl font-bold text-slate-900">Trung Tâm Nhập & Xuất Dữ Liệu (Import/Export Center)</h1>
          </div>
          <p className="text-sm text-slate-500 mt-1">
            Nhập liệu hàng loạt an toàn từ Excel/CSV, tự động phát hiện trùng lặp, bảo đảm tính toàn vẹn và xuất dữ liệu phân quyền.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={fetchData}
            className="flex items-center gap-1.5 px-3.5 py-2 text-sm font-medium text-slate-700 bg-slate-50 hover:bg-slate-100 rounded-lg border border-slate-200 transition"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            Làm mới
          </button>
        </div>
      </div>

      <AdminNavigation />

      {/* Tabs */}
      <div className="flex items-center gap-2 border-b border-slate-200 pb-3">
        <button
          onClick={() => setActiveTab('import')}
          className={`flex items-center gap-2 px-4 py-2 rounded-lg text-xs font-semibold transition ${
            activeTab === 'import' ? 'bg-blue-600 text-white' : 'text-slate-600 hover:bg-slate-100'
          }`}
        >
          <Upload className="w-4 h-4" />
          Nhập Dữ Liệu (Batch Import)
        </button>
        <button
          onClick={() => setActiveTab('export')}
          className={`flex items-center gap-2 px-4 py-2 rounded-lg text-xs font-semibold transition ${
            activeTab === 'export' ? 'bg-blue-600 text-white' : 'text-slate-600 hover:bg-slate-100'
          }`}
        >
          <Download className="w-4 h-4" />
          Xuất Dữ Liệu (Export Center)
        </button>
      </div>

      {activeTab === 'import' ? (
        <div className="space-y-6">
          {/* Import Setup Card */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-xs p-5">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-4">
              <div>
                <h2 className="text-base font-bold text-slate-900 flex items-center gap-2">
                  <FileSpreadsheet className="w-5 h-5 text-blue-600" />
                  Cấu hình Nhập Liệu
                </h2>
                <p className="text-xs text-slate-500 mt-0.5">
                  Chọn phân hệ cần nhập, tải mẫu định dạng và xác nhận bản ghi.
                </p>
              </div>

              <div className="flex items-center gap-2">
                <select
                  value={entityType}
                  onChange={(e) => setEntityType(e.target.value)}
                  className="px-3 py-1.5 text-xs font-semibold rounded-lg border border-slate-200 bg-white text-slate-800 focus:outline-none"
                >
                  <option value="Leads">Khách hàng tiềm năng (Leads)</option>
                  <option value="Accounts">Tổ chức Doanh nghiệp (Accounts)</option>
                  <option value="Contacts">Người liên hệ (Contacts)</option>
                </select>

                <button
                  type="button"
                  onClick={handleDownloadSample}
                  className="flex items-center gap-1.5 px-3 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-xs font-semibold"
                >
                  <Download className="w-3.5 h-3.5" />
                  Tải Mẫu CSV
                </button>
              </div>
            </div>

            <div className="space-y-3">
              <label className="block text-xs font-semibold text-slate-700">
                Dữ liệu mẫu JSON / Dán danh sách bản ghi:
              </label>
              <textarea
                rows={6}
                value={rawJsonText}
                onChange={(e) => setRawJsonText(e.target.value)}
                className="w-full p-3 font-mono text-xs border border-slate-200 rounded-lg bg-slate-50 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />

              <div className="flex items-center justify-between pt-2">
                <span className="text-xs text-slate-500">
                  Hệ thống tự động kiểm tra trùng lặp email/số điện thoại để tránh ghi đè dữ liệu.
                </span>

                <button
                  onClick={handleExecuteImport}
                  disabled={processing}
                  className="flex items-center gap-2 px-5 py-2 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white rounded-lg text-xs font-semibold shadow-xs transition"
                >
                  <Play className="w-4 h-4" />
                  <span>{processing ? 'Đang xử lý...' : 'Bắt Đầu Nhập Liệu'}</span>
                </button>
              </div>
            </div>

            {/* Import Result Notification */}
            {importResult && (
              <div
                className={`mt-4 p-4 rounded-xl border ${
                  importResult.failureCount === 0
                    ? 'bg-emerald-50 border-emerald-200 text-emerald-900'
                    : 'bg-amber-50 border-amber-200 text-amber-900'
                }`}
              >
                <div className="flex items-center gap-2 font-bold text-sm">
                  {importResult.failureCount === 0 ? (
                    <CheckCircle2 className="w-5 h-5 text-emerald-600" />
                  ) : (
                    <AlertTriangle className="w-5 h-5 text-amber-600" />
                  )}
                  <span>
                    Kết quả Nhập: {importResult.successCount} thành công, {importResult.failureCount} thất bại / bỏ qua
                  </span>
                </div>
                {importResult.errors.length > 0 && (
                  <ul className="mt-2 text-xs space-y-1 list-disc list-inside text-rose-700">
                    {importResult.errors.map((err, idx) => (
                      <li key={idx}>{err}</li>
                    ))}
                  </ul>
                )}
              </div>
            )}
          </div>

          {/* Import Jobs History */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
            <div className="p-4 bg-slate-50 border-b border-slate-200 font-bold text-xs text-slate-800">
              Lịch sử các Đợt Nhập Liệu Gần Đây ({importJobs.length})
            </div>
            {importJobs.length === 0 ? (
              <div className="p-6 text-center text-xs text-slate-400">Chưa có đợt nhập liệu nào.</div>
            ) : (
              <div className="divide-y divide-slate-100">
                {importJobs.map((j) => (
                  <div key={j.id} className="p-3.5 flex items-center justify-between hover:bg-slate-50 text-xs">
                    <div>
                      <div className="font-semibold text-slate-900">{j.fileName} ({j.entityType})</div>
                      <div className="text-slate-500 text-[11px] mt-0.5">
                        Tạo bởi: {j.uploadedByEmail} • {new Date(j.createdAtUtc).toLocaleString('vi-VN')}
                      </div>
                    </div>
                    <div className="text-right">
                      <span className="font-semibold text-emerald-600">{j.successCount} thành công</span>
                      {j.failureCount > 0 && <span className="ml-2 text-rose-600 font-semibold">{j.failureCount} lỗi</span>}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      ) : (
        /* Export Tab */
        <div className="space-y-6">
          <div className="bg-white rounded-xl border border-slate-200 shadow-xs p-5">
            <h2 className="text-base font-bold text-slate-900 mb-2">Kết Xuất Dữ Liệu Tức Thì</h2>
            <p className="text-xs text-slate-500 mb-4">
              Chọn phân hệ cần xuất khẩu, định dạng file tương thích (Excel / CSV) có bảo toàn phân quyền người dùng.
            </p>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 max-w-md">
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Phân hệ dữ liệu</label>
                <select
                  value={exportEntity}
                  onChange={(e) => setExportEntity(e.target.value)}
                  className="w-full px-3 py-2 text-xs rounded-lg border border-slate-200 bg-white"
                >
                  <option value="Leads">Khách hàng tiềm năng (Leads)</option>
                  <option value="Accounts">Doanh nghiệp khách hàng (Accounts)</option>
                  <option value="Deals">Cơ hội bán hàng (Deals)</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Định dạng file</label>
                <select
                  value={exportFormat}
                  onChange={(e) => setExportFormat(e.target.value)}
                  className="w-full px-3 py-2 text-xs rounded-lg border border-slate-200 bg-white"
                >
                  <option value="Excel">Microsoft Excel (.xlsx)</option>
                  <option value="CSV">Comma Separated Values (.csv)</option>
                </select>
              </div>
            </div>

            <div className="pt-4">
              <button
                onClick={handleInitiateExport}
                className="flex items-center gap-2 px-5 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-semibold shadow-xs transition"
              >
                <Download className="w-4 h-4" />
                <span>Tiến Hành Kết Xuất Dữ Liệu</span>
              </button>
            </div>
          </div>

          {/* Export Jobs History */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
            <div className="p-4 bg-slate-50 border-b border-slate-200 font-bold text-xs text-slate-800">
              Lịch sử các Tác Vụ Xuất Dữ Liệu ({exportJobs.length})
            </div>
            {exportJobs.length === 0 ? (
              <div className="p-6 text-center text-xs text-slate-400">Chưa có tác vụ xuất dữ liệu nào.</div>
            ) : (
              <div className="divide-y divide-slate-100">
                {exportJobs.map((j) => (
                  <div key={j.id} className="p-3.5 flex items-center justify-between hover:bg-slate-50 text-xs">
                    <div>
                      <div className="font-semibold text-slate-900">{j.entityType} ({j.format})</div>
                      <div className="text-slate-500 text-[11px] mt-0.5">
                        Yêu cầu bởi: {j.requestedByEmail} • {new Date(j.createdAtUtc).toLocaleString('vi-VN')}
                      </div>
                    </div>
                    <div className="flex items-center gap-3">
                      <span className="font-semibold text-slate-700">{j.totalRecords} bản ghi</span>
                      {j.fileUrl && (
                        <a
                          href={j.fileUrl}
                          className="px-3 py-1 bg-blue-50 text-blue-600 hover:bg-blue-100 rounded text-xs font-semibold"
                        >
                          Tải Về
                        </a>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};
