import React, { useState, useEffect } from 'react';
import { AdminNavigation } from './AdminNavigation';
import { adminService } from '@/services/adminService';
import type { SystemConfiguration, ConfigurationHistory } from '@/types/admin';
import {
  Settings,
  RefreshCw,
  Search,
  Save,
  History,
  X,
  Sliders,
  Shield,
  Bell,
  Layers,
  Database,
  Building,
} from 'lucide-react';

const CATEGORIES = [
  { id: 'ALL', name: 'Tất cả cấu hình', icon: Sliders },
  { id: 'General', name: 'Chung (General)', icon: Building },
  { id: 'CRM', name: 'Nghiệp vụ CRM', icon: Layers },
  { id: 'Security', name: 'Bảo mật & Phiên', icon: Shield },
  { id: 'Notification', name: 'Thông báo', icon: Bell },
  { id: 'Integration', name: 'Tích hợp & API', icon: Database },
];

export const SystemSettingsPage: React.FC = () => {
  const [settings, setSettings] = useState<SystemConfiguration[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedCategory, setSelectedCategory] = useState('ALL');
  const [search, setSearch] = useState('');

  // Edit Modal State
  const [editingSetting, setEditingSetting] = useState<SystemConfiguration | null>(null);
  const [editValue, setEditValue] = useState('');
  const [editDescription, setEditDescription] = useState('');
  const [saving, setSaving] = useState(false);

  // History Modal State
  const [historyTarget, setHistoryTarget] = useState<SystemConfiguration | null>(null);
  const [historyRecords, setHistoryRecords] = useState<ConfigurationHistory[]>([]);
  const [loadingHistory, setLoadingHistory] = useState(false);

  const fetchSettings = async () => {
    try {
      setLoading(true);
      const catParam = selectedCategory === 'ALL' ? undefined : selectedCategory;
      const data = await adminService.getSettings(catParam);
      setSettings(data);
    } catch (err) {
      console.error('Failed to load settings:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSettings();
  }, [selectedCategory]);

  const handleEditClick = (setting: SystemConfiguration) => {
    setEditingSetting(setting);
    setEditValue(setting.value);
    setEditDescription(setting.description || '');
  };

  const handleSaveSetting = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingSetting) return;

    try {
      setSaving(true);
      await adminService.updateSetting(
        editingSetting.category,
        editingSetting.key,
        editValue,
        editDescription
      );
      setEditingSetting(null);
      fetchSettings();
    } catch (err: any) {
      alert(`Lỗi lưu cấu hình: ${err?.response?.data?.message || err.message}`);
    } finally {
      setSaving(false);
    }
  };

  const handleViewHistory = async (setting: SystemConfiguration) => {
    setHistoryTarget(setting);
    try {
      setLoadingHistory(true);
      const hist = await adminService.getSettingHistory(setting.id);
      setHistoryRecords(hist);
    } catch (err) {
      console.error('Failed to load setting history:', err);
      setHistoryRecords([]);
    } finally {
      setLoadingHistory(false);
    }
  };

  const filteredSettings = settings.filter(s => {
    const matchSearch =
      s.key.toLowerCase().includes(search.toLowerCase()) ||
      (s.description && s.description.toLowerCase().includes(search.toLowerCase())) ||
      s.value.toLowerCase().includes(search.toLowerCase());
    return matchSearch;
  });

  return (
    <div className="space-y-6">
      <AdminNavigation />

      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 flex items-center gap-2">
            <Settings className="w-7 h-7 text-indigo-600" />
            Cấu hình Hệ thống & Tham số Doanh nghiệp
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            Quản lý tập trung các tham số toàn hệ thống, ngưỡng chuyển đổi CRM, chính sách bảo mật và tích hợp.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={fetchSettings}
            className="inline-flex items-center gap-2 px-3 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50"
          >
            <RefreshCw className="w-4 h-4" />
            Làm mới
          </button>
        </div>
      </div>

      {/* Category Tabs */}
      <div className="flex flex-wrap gap-2 border-b border-slate-200 pb-3">
        {CATEGORIES.map(cat => {
          const Icon = cat.icon;
          const isActive = selectedCategory === cat.id;
          return (
            <button
              key={cat.id}
              onClick={() => setSelectedCategory(cat.id)}
              className={`inline-flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                isActive
                  ? 'bg-indigo-600 text-white shadow-sm'
                  : 'bg-white text-slate-600 border border-slate-200 hover:bg-slate-50 hover:text-slate-900'
              }`}
            >
              <Icon className="w-4 h-4" />
              {cat.name}
            </button>
          );
        })}
      </div>

      {/* Search & Filter Bar */}
      <div className="bg-white p-4 rounded-xl border border-slate-200 flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="w-4 h-4 absolute left-3 top-3 text-slate-400" />
          <input
            type="text"
            placeholder="Tìm theo khóa (Key), giá trị hoặc mô tả..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-4 py-2 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </div>
      </div>

      {/* Settings Grid / Table */}
      <div className="bg-white rounded-xl border border-slate-200 overflow-hidden shadow-sm">
        {loading ? (
          <div className="p-12 text-center text-slate-500">Đang tải danh sách tham số cấu hình...</div>
        ) : filteredSettings.length === 0 ? (
          <div className="p-12 text-center text-slate-500">Không tìm thấy tham số cấu hình nào.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-600">
              <thead className="bg-slate-50 border-b border-slate-200 text-xs font-semibold text-slate-700 uppercase tracking-wider">
                <tr>
                  <th className="px-6 py-3.5">Khóa cấu hình (Key)</th>
                  <th className="px-6 py-3.5">Phân loại</th>
                  <th className="px-6 py-3.5">Giá trị hiện tại</th>
                  <th className="px-6 py-3.5">Mô tả nghiệp vụ</th>
                  <th className="px-6 py-3.5">Cập nhật lần cuối</th>
                  <th className="px-6 py-3.5 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200">
                {filteredSettings.map(setting => (
                  <tr key={setting.id} className="hover:bg-slate-50/75 transition-colors">
                    <td className="px-6 py-4 font-mono font-medium text-slate-900">
                      {setting.key}
                    </td>
                    <td className="px-6 py-4">
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-slate-100 text-slate-800">
                        {setting.category}
                      </span>
                    </td>
                    <td className="px-6 py-4 font-medium text-indigo-700 max-w-xs truncate">
                      {setting.value}
                    </td>
                    <td className="px-6 py-4 text-slate-500 max-w-md">
                      {setting.description || '—'}
                    </td>
                    <td className="px-6 py-4 text-xs text-slate-500 whitespace-nowrap">
                      {setting.updatedAtUtc ? new Date(setting.updatedAtUtc).toLocaleString('vi-VN') : '—'}
                    </td>
                    <td className="px-6 py-4 text-right space-x-2 whitespace-nowrap">
                      <button
                        onClick={() => handleEditClick(setting)}
                        className="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-indigo-600 hover:text-indigo-800 bg-indigo-50 hover:bg-indigo-100 rounded-md transition-colors"
                      >
                        Chỉnh sửa
                      </button>
                      <button
                        onClick={() => handleViewHistory(setting)}
                        className="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-slate-600 hover:text-slate-800 bg-slate-100 hover:bg-slate-200 rounded-md transition-colors"
                      >
                        <History className="w-3.5 h-3.5" />
                        Lịch sử
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Edit Setting Modal */}
      {editingSetting && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-white rounded-xl shadow-xl border border-slate-200 w-full max-w-lg overflow-hidden animate-in fade-in zoom-in-95 duration-150">
            <div className="flex items-center justify-between px-6 py-4 border-b border-slate-200">
              <h2 className="text-lg font-semibold text-slate-900 flex items-center gap-2">
                <Settings className="w-5 h-5 text-indigo-600" />
                Cập nhật tham số cấu hình
              </h2>
              <button
                onClick={() => setEditingSetting(null)}
                className="text-slate-400 hover:text-slate-600"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <form onSubmit={handleSaveSetting} className="p-6 space-y-4">
              <div>
                <label className="block text-xs font-semibold text-slate-700 uppercase mb-1">
                  Nhóm cấu hình
                </label>
                <input
                  type="text"
                  disabled
                  value={editingSetting.category}
                  className="w-full px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50 text-slate-500 font-mono"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 uppercase mb-1">
                  Khóa cấu hình (Key)
                </label>
                <input
                  type="text"
                  disabled
                  value={editingSetting.key}
                  className="w-full px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50 text-slate-500 font-mono font-medium"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 uppercase mb-1">
                  Giá trị mới *
                </label>
                <textarea
                  rows={3}
                  required
                  value={editValue}
                  onChange={(e) => setEditValue(e.target.value)}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 font-mono"
                  placeholder="Nhập giá trị tham số..."
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 uppercase mb-1">
                  Mô tả & Ghi chú
                </label>
                <input
                  type="text"
                  value={editDescription}
                  onChange={(e) => setEditDescription(e.target.value)}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  placeholder="Mô tả mục đích cấu hình này..."
                />
              </div>

              <div className="pt-4 flex items-center justify-end gap-3 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setEditingSetting(null)}
                  className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50"
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  disabled={saving}
                  className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50"
                >
                  <Save className="w-4 h-4" />
                  {saving ? 'Đang lưu...' : 'Lưu cấu hình'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Setting History Modal */}
      {historyTarget && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-white rounded-xl shadow-xl border border-slate-200 w-full max-w-2xl overflow-hidden animate-in fade-in zoom-in-95 duration-150">
            <div className="flex items-center justify-between px-6 py-4 border-b border-slate-200">
              <div>
                <h2 className="text-lg font-semibold text-slate-900 flex items-center gap-2">
                  <History className="w-5 h-5 text-indigo-600" />
                  Lịch sử thay đổi: {historyTarget.key}
                </h2>
                <p className="text-xs text-slate-500 mt-0.5">
                  Lưu vết nhật ký kiểm toán mọi phiên bản thay đổi tham số cấu hình.
                </p>
              </div>
              <button
                onClick={() => setHistoryTarget(null)}
                className="text-slate-400 hover:text-slate-600"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="p-6 max-h-[60vh] overflow-y-auto">
              {loadingHistory ? (
                <div className="p-8 text-center text-slate-500">Đang tải lịch sử...</div>
              ) : historyRecords.length === 0 ? (
                <div className="p-8 text-center text-slate-500">Chưa có lịch sử thay đổi nào cho tham số này.</div>
              ) : (
                <div className="space-y-4">
                  {historyRecords.map(item => (
                    <div key={item.id} className="p-4 bg-slate-50 rounded-lg border border-slate-200 space-y-2">
                      <div className="flex items-center justify-between text-xs text-slate-500">
                        <span className="font-medium text-slate-700">Người sửa: {item.changedByEmail || 'Hệ thống'}</span>
                        <span>{new Date(item.timestampUtc).toLocaleString('vi-VN')}</span>
                      </div>
                      <div className="grid grid-cols-2 gap-3 text-xs font-mono">
                        <div className="p-2 bg-red-50 border border-red-200 rounded text-red-700">
                          <span className="font-sans font-semibold block text-red-800 text-[10px] uppercase">Giá trị cũ:</span>
                          {item.oldValue || '<Trống>'}
                        </div>
                        <div className="p-2 bg-emerald-50 border border-emerald-200 rounded text-emerald-700">
                          <span className="font-sans font-semibold block text-emerald-800 text-[10px] uppercase">Giá trị mới:</span>
                          {item.newValue}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            <div className="px-6 py-4 bg-slate-50 border-t border-slate-200 flex justify-end">
              <button
                onClick={() => setHistoryTarget(null)}
                className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50"
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
