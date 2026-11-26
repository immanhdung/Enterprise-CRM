import React, { useState, useEffect } from 'react';
import { AutomationNavigation } from './AutomationNavigation';
import { automationService } from '../../services/automationService';
import type { BusinessAlert } from '../../types/automation';
import { AlertTriangle, AlertCircle, Info, CheckCircle2, RefreshCw, ShieldAlert } from 'lucide-react';

export const AlertsCenterPage: React.FC = () => {
  const [alerts, setAlerts] = useState<BusinessAlert[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedCategory, setSelectedCategory] = useState<string>('All');
  const [scanning, setScanning] = useState(false);

  const fetchAlerts = async () => {
    try {
      setLoading(true);
      const cat = selectedCategory === 'All' ? undefined : selectedCategory;
      const data = await automationService.getActiveAlerts(cat);
      setAlerts(data);
    } catch (err) {
      console.error('Failed to load alerts:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAlerts();
  }, [selectedCategory]);

  const handleScan = async () => {
    try {
      setScanning(true);
      const count = await automationService.runAlertScan();
      alert(`Đã quét tự động xong: ${count} cảnh báo mới được phát hiện.`);
      fetchAlerts();
    } catch (err: any) {
      alert(`Lỗi quét: ${err?.message || 'Có lỗi xảy ra'}`);
    } finally {
      setScanning(false);
    }
  };

  const handleAcknowledge = async (id: string) => {
    try {
      await automationService.acknowledgeAlert(id);
      fetchAlerts();
    } catch (err) {
      console.error('Failed to acknowledge alert:', err);
    }
  };

  const handleResolve = async (id: string) => {
    try {
      await automationService.resolveAlert(id);
      fetchAlerts();
    } catch (err) {
      console.error('Failed to resolve alert:', err);
    }
  };

  const categories = [
    { key: 'All', label: 'Tất cả' },
    { key: 'Lead', label: 'Khách hàng (Lead)' },
    { key: 'Deal', label: 'Cơ hội bán hàng (Deal)' },
    { key: 'Customer', label: 'Hợp đồng & KH (Customer)' },
    { key: 'Finance', label: 'Tài chính & Công nợ (Finance)' },
  ];

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Trung tâm Cảnh báo Thông minh (Smart Alerts Center)</h1>
          <p className="text-sm text-gray-500">
            Giám sát tự động các rủi ro kinh doanh: lead bị đình trệ, cơ hội sắp đóng, hợp đồng hết hạn, công nợ quá hạn
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={handleScan}
            disabled={scanning}
            className="flex items-center gap-2 bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded-lg text-sm font-medium shadow-sm transition-colors"
          >
            <ShieldAlert className={`w-4 h-4 ${scanning ? 'animate-spin' : ''}`} />
            <span>{scanning ? 'Đang quét...' : 'Quét Cảnh báo Ngay'}</span>
          </button>
          <button
            onClick={fetchAlerts}
            disabled={loading}
            className="p-2 text-gray-600 hover:text-gray-900 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
          </button>
        </div>
      </div>

      <AutomationNavigation />

      {/* Category Filter Tabs */}
      <div className="flex gap-2 mb-6">
        {categories.map((cat) => (
          <button
            key={cat.key}
            onClick={() => setSelectedCategory(cat.key)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
              selectedCategory === cat.key
                ? 'bg-blue-600 text-white shadow-sm'
                : 'bg-white text-gray-600 hover:bg-gray-50 border border-gray-200'
            }`}
          >
            {cat.label}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="flex justify-center items-center h-48 bg-white rounded-lg border border-gray-200">
          <RefreshCw className="w-6 h-6 animate-spin text-blue-600" />
        </div>
      ) : alerts.length === 0 ? (
        <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
          <CheckCircle2 className="w-12 h-12 text-green-500 mx-auto mb-3" />
          <h3 className="text-base font-semibold text-gray-900">Không có cảnh báo tồn đọng</h3>
          <p className="text-sm text-gray-500 mt-1">
            Toàn bộ quy trình Lead, Deal, Hợp đồng và Tài chính đều đang vận hành đúng tiến độ.
          </p>
        </div>
      ) : (
        <div className="space-y-3">
          {alerts.map((alert) => {
            const isCritical = alert.severity === 'Critical';
            const isWarning = alert.severity === 'Warning';
            return (
              <div
                key={alert.id}
                className={`bg-white p-5 rounded-lg border shadow-sm transition-all ${
                  isCritical
                    ? 'border-l-4 border-l-red-500 border-red-100'
                    : isWarning
                    ? 'border-l-4 border-l-amber-500 border-amber-100'
                    : 'border-l-4 border-l-blue-500 border-gray-200'
                }`}
              >
                <div className="flex items-start justify-between">
                  <div className="flex items-start gap-3">
                    <div className="mt-0.5">
                      {isCritical && <AlertCircle className="w-5 h-5 text-red-600" />}
                      {isWarning && <AlertTriangle className="w-5 h-5 text-amber-600" />}
                      {!isCritical && !isWarning && <Info className="w-5 h-5 text-blue-600" />}
                    </div>
                    <div>
                      <div className="flex items-center gap-2">
                        <h3 className="font-semibold text-gray-900 text-sm">{alert.title}</h3>
                        <span
                          className={`text-[11px] px-2 py-0.5 rounded font-semibold uppercase ${
                            isCritical
                              ? 'bg-red-50 text-red-700 border border-red-200'
                              : isWarning
                              ? 'bg-amber-50 text-amber-700 border border-amber-200'
                              : 'bg-blue-50 text-blue-700 border border-blue-200'
                          }`}
                        >
                          {alert.severity}
                        </span>
                        <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded">
                          {alert.category}
                        </span>
                      </div>
                      <p className="text-sm text-gray-600 mt-1">{alert.message}</p>
                      <div className="flex items-center gap-4 text-xs text-gray-400 mt-2">
                        <span>Đối tượng: {alert.entityType} ({alert.entityId})</span>
                        <span>Thời gian: {new Date(alert.triggeredAt).toLocaleString('vi-VN')}</span>
                        {alert.status !== 'Active' && (
                          <span className="font-medium text-purple-600">Trạng thái: {alert.status}</span>
                        )}
                      </div>
                    </div>
                  </div>

                  <div className="flex items-center gap-2">
                    {alert.status === 'Active' && (
                      <button
                        onClick={() => handleAcknowledge(alert.id)}
                        className="px-3 py-1.5 text-xs font-medium text-gray-700 bg-gray-50 hover:bg-gray-100 rounded border border-gray-300"
                      >
                        Đã ghi nhận
                      </button>
                    )}
                    {alert.status !== 'Resolved' && (
                      <button
                        onClick={() => handleResolve(alert.id)}
                        className="px-3 py-1.5 text-xs font-medium text-green-700 bg-green-50 hover:bg-green-100 rounded border border-green-200"
                      >
                        Giải quyết
                      </button>
                    )}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};
