import React, { useState, useEffect } from 'react';
import { AdminNavigation } from './AdminNavigation';
import { adminService } from '@/services/adminService';
import type { ApiKey, WebhookEndpoint, WebhookDelivery, ApiKeyCreatedResult } from '@/types/admin';
import {
  Webhook,
  Key,
  Plus,
  RefreshCw,
  Copy,
  Trash2,
  Send,
  CheckCircle2,
  XCircle,
  X,
} from 'lucide-react';

export const IntegrationsPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'apiKeys' | 'webhooks'>('apiKeys');
  const [apiKeys, setApiKeys] = useState<ApiKey[]>([]);
  const [webhooks, setWebhooks] = useState<WebhookEndpoint[]>([]);
  const [deliveries, setDeliveries] = useState<WebhookDelivery[]>([]);
  const [loading, setLoading] = useState(true);

  // Modals
  const [showCreateKeyModal, setShowCreateKeyModal] = useState(false);
  const [createdKeyResult, setCreatedKeyResult] = useState<ApiKeyCreatedResult | null>(null);
  const [keyName, setKeyName] = useState('');

  const [showCreateWebhookModal, setShowCreateWebhookModal] = useState(false);
  const [webhookForm, setWebhookForm] = useState({
    name: '',
    url: '',
    events: ['lead.created', 'deal.won'],
  });

  const availableEvents = [
    'lead.created',
    'lead.updated',
    'deal.created',
    'deal.won',
    'deal.lost',
    'contract.signed',
    'payment.received',
    'invoice.published',
  ];

  const fetchData = async () => {
    try {
      setLoading(true);
      const [keys, whs, dels] = await Promise.all([
        adminService.getApiKeys(),
        adminService.getWebhooks(),
        adminService.getWebhookDeliveries(undefined, 20),
      ]);
      setApiKeys(keys);
      setWebhooks(whs);
      setDeliveries(dels);
    } catch (err) {
      console.error('Failed to load integrations:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleCreateKey = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!keyName.trim()) return;

    try {
      const res = await adminService.createApiKey({
        name: keyName.trim(),
        scopes: 'crm.read,crm.write',
        rateLimitPerMinute: 60,
      });
      setShowCreateKeyModal(false);
      setKeyName('');
      setCreatedKeyResult(res);
      fetchData();
    } catch (err: any) {
      alert(`Lỗi tạo API Key: ${err?.message || 'Có lỗi xảy ra'}`);
    }
  };

  const handleRevokeKey = async (id: string, name: string) => {
    if (!window.confirm(`Bạn có chắc muốn thu hồi API Key '${name}'? Ứng dụng tích hợp sẽ mất quyền truy cập ngay lập tức.`)) return;

    try {
      await adminService.revokeApiKey(id);
      fetchData();
    } catch (err: any) {
      alert(`Lỗi thu hồi API Key: ${err?.message || 'Có lỗi xảy ra'}`);
    }
  };

  const handleCreateWebhook = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await adminService.createWebhook(webhookForm.name.trim(), webhookForm.url.trim(), webhookForm.events);
      setShowCreateWebhookModal(false);
      setWebhookForm({ name: '', url: '', events: ['lead.created', 'deal.won'] });
      alert('Đã đăng ký Webhook endpoint thành công.');
      fetchData();
    } catch (err: any) {
      alert(`Lỗi đăng ký Webhook: ${err?.message || 'Có lỗi xảy ra'}`);
    }
  };

  const handleTestWebhook = async (id: string) => {
    try {
      const res = await adminService.testWebhook(id);
      alert(`Kết quả kiểm tra: HTTP ${res.responseStatusCode || 500} (${res.status}) trong ${res.durationMs}ms`);
      fetchData();
    } catch (err: any) {
      alert(`Lỗi kiểm tra webhook: ${err?.message || 'Có lỗi xảy ra'}`);
    }
  };

  const handleDeleteWebhook = async (id: string, name: string) => {
    if (!window.confirm(`Bạn có chắc muốn xóa Webhook '${name}'?`)) return;

    try {
      await adminService.deleteWebhook(id);
      fetchData();
    } catch (err: any) {
      alert(`Lỗi xóa webhook: ${err?.message || 'Có lỗi xảy ra'}`);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-xs">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-50 text-blue-700 border border-blue-200">
              Tích hợp & Kết nối
            </span>
            <h1 className="text-2xl font-bold text-slate-900">Quản lý API Keys & Webhooks</h1>
          </div>
          <p className="text-sm text-slate-500 mt-1">
            Cấp khóa API an toàn (băm mật mã SHA-256), theo dõi giới hạn tần suất (Rate Limiting), và đăng ký Webhooks ký chữ ký số HMAC.
          </p>
        </div>

        <div className="flex items-center gap-3">
          {activeTab === 'apiKeys' ? (
            <button
              onClick={() => setShowCreateKeyModal(true)}
              className="flex items-center gap-1.5 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-semibold shadow-xs transition"
            >
              <Plus className="w-4 h-4" />
              Tạo API Key Mới
            </button>
          ) : (
            <button
              onClick={() => setShowCreateWebhookModal(true)}
              className="flex items-center gap-1.5 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-semibold shadow-xs transition"
            >
              <Plus className="w-4 h-4" />
              Thêm Webhook Endpoint
            </button>
          )}
        </div>
      </div>

      <AdminNavigation />

      {/* Tabs */}
      <div className="flex items-center gap-2 border-b border-slate-200 pb-3">
        <button
          onClick={() => setActiveTab('apiKeys')}
          className={`flex items-center gap-2 px-4 py-2 rounded-lg text-xs font-semibold transition ${
            activeTab === 'apiKeys' ? 'bg-blue-600 text-white' : 'text-slate-600 hover:bg-slate-100'
          }`}
        >
          <Key className="w-4 h-4" />
          Khóa API Doanh Nghiệp ({apiKeys.length})
        </button>
        <button
          onClick={() => setActiveTab('webhooks')}
          className={`flex items-center gap-2 px-4 py-2 rounded-lg text-xs font-semibold transition ${
            activeTab === 'webhooks' ? 'bg-blue-600 text-white' : 'text-slate-600 hover:bg-slate-100'
          }`}
        >
          <Webhook className="w-4 h-4" />
          Webhooks Sự Kiện ({webhooks.length})
        </button>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-16">
          <RefreshCw className="w-8 h-8 animate-spin text-blue-600" />
        </div>
      ) : activeTab === 'apiKeys' ? (
        /* API Keys View */
        <div className="space-y-6">
          <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
            {apiKeys.length === 0 ? (
              <div className="text-center py-12 text-slate-400 text-sm">Chưa có API Key nào được tạo.</div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-left text-xs text-slate-600">
                  <thead className="bg-slate-50 text-slate-700 font-semibold uppercase border-b border-slate-200">
                    <tr>
                      <th className="py-3 px-4">Tên Ứng dụng</th>
                      <th className="py-3 px-4">Tiền tố Khóa (Prefix)</th>
                      <th className="py-3 px-4">Phạm vi Quyền (Scopes)</th>
                      <th className="py-3 px-4">Giới hạn Tốc độ</th>
                      <th className="py-3 px-4 text-center">Trạng thái</th>
                      <th className="py-3 px-4">Tạo bởi</th>
                      <th className="py-3 px-4 text-right">Thao tác</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {apiKeys.map((k) => (
                      <tr key={k.id} className="hover:bg-slate-50/60 transition">
                        <td className="py-3 px-4 font-semibold text-slate-900">{k.name}</td>
                        <td className="py-3 px-4 font-mono text-[11px] text-slate-600">{k.keyPrefix}••••••••</td>
                        <td className="py-3 px-4">
                          <span className="px-2 py-0.5 rounded text-[10px] font-mono bg-slate-100 text-slate-700">
                            {k.scopes}
                          </span>
                        </td>
                        <td className="py-3 px-4">{k.rateLimitPerMinute} req/min</td>
                        <td className="py-3 px-4 text-center">
                          <span
                            className={`inline-block px-2 py-0.5 rounded-full text-[10px] font-semibold ${
                              k.isActive
                                ? 'bg-emerald-50 text-emerald-700 border border-emerald-200'
                                : 'bg-slate-100 text-slate-500'
                            }`}
                          >
                            {k.isActive ? 'Đang hoạt động' : 'Đã thu hồi'}
                          </span>
                        </td>
                        <td className="py-3 px-4 text-slate-500">{k.createdByEmail}</td>
                        <td className="py-3 px-4 text-right">
                          {k.isActive && (
                            <button
                              onClick={() => handleRevokeKey(k.id, k.name)}
                              className="p-1.5 text-rose-600 hover:bg-rose-50 rounded"
                              title="Thu hồi khóa"
                            >
                              <Trash2 className="w-3.5 h-3.5" />
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      ) : (
        /* Webhooks View */
        <div className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {webhooks.map((w) => (
              <div key={w.id} className="bg-white rounded-xl border border-slate-200 shadow-xs p-5 flex flex-col justify-between">
                <div>
                  <div className="flex items-start justify-between">
                    <div>
                      <h3 className="font-bold text-sm text-slate-900">{w.name}</h3>
                      <div className="text-[11px] font-mono text-slate-500 truncate mt-0.5 max-w-sm">{w.url}</div>
                    </div>
                    <div className="flex items-center gap-1">
                      <button
                        onClick={() => handleTestWebhook(w.id)}
                        className="p-1.5 text-blue-600 hover:bg-blue-50 rounded"
                        title="Gửi thử nghiệm Ping"
                      >
                        <Send className="w-3.5 h-3.5" />
                      </button>
                      <button
                        onClick={() => handleDeleteWebhook(w.id, w.name)}
                        className="p-1.5 text-rose-600 hover:bg-rose-50 rounded"
                        title="Xóa Webhook"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>
                    </div>
                  </div>

                  <div className="mt-3 flex flex-wrap gap-1">
                    {w.subscribedEvents.map((evt) => (
                      <span key={evt} className="px-2 py-0.5 rounded text-[10px] font-mono bg-purple-50 text-purple-700">
                        {evt}
                      </span>
                    ))}
                  </div>
                </div>

                <div className="mt-4 pt-3 border-t border-slate-100 flex items-center justify-between text-[11px] text-slate-500">
                  <span>Trạng thái: {w.isActive ? 'Đang gửi' : 'Tạm dừng'}</span>
                  {w.failureCount > 0 && <span className="text-rose-600 font-semibold">{w.failureCount} lần lỗi</span>}
                </div>
              </div>
            ))}
          </div>

          {/* Webhook Deliveries Log */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
            <div className="p-4 bg-slate-50 border-b border-slate-200 font-bold text-xs text-slate-800">
              Nhật ký Gửi Webhook Gần Đây ({deliveries.length})
            </div>
            {deliveries.length === 0 ? (
              <div className="p-6 text-center text-xs text-slate-400">Chưa có sự kiện webhook nào được phát.</div>
            ) : (
              <div className="divide-y divide-slate-100">
                {deliveries.map((d) => (
                  <div key={d.id} className="p-3.5 flex items-center justify-between hover:bg-slate-50 text-xs">
                    <div>
                      <div className="flex items-center gap-2">
                        <span className="font-mono font-semibold text-slate-800">{d.eventName}</span>
                        <span
                          className={`px-1.5 py-0.5 rounded text-[10px] font-bold ${
                            d.status === 'Success'
                              ? 'bg-emerald-50 text-emerald-700'
                              : 'bg-rose-50 text-rose-700'
                          }`}
                        >
                          {d.status} ({d.responseStatusCode || 500})
                        </span>
                      </div>
                      <div className="text-[11px] text-slate-500 mt-0.5">
                        {new Date(d.createdAtUtc).toLocaleString('vi-VN')} • Thời gian phản hồi: {d.durationMs}ms
                      </div>
                    </div>
                    {d.status === 'Success' ? (
                      <CheckCircle2 className="w-4 h-4 text-emerald-500" />
                    ) : (
                      <XCircle className="w-4 h-4 text-rose-500" />
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}

      {/* Modal: Create API Key */}
      {showCreateKeyModal && (
        <div className="fixed inset-0 bg-black/40 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl max-w-sm w-full p-6 shadow-xl border border-slate-200">
            <h3 className="text-base font-bold text-slate-900 mb-2">Tạo Khóa API Mới</h3>
            <p className="text-xs text-slate-500 mb-4">
              Nhập tên ứng dụng hoặc hệ thống đối tác cần tích hợp với CRM.
            </p>

            <form onSubmit={handleCreateKey} className="space-y-4 text-xs">
              <div>
                <label className="block text-slate-600 mb-1 font-medium">Tên Ứng dụng tích hợp</label>
                <input
                  type="text"
                  required
                  placeholder="Ví dụ: ERP Connector, Mobile App..."
                  value={keyName}
                  onChange={(e) => setKeyName(e.target.value)}
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div className="flex justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setShowCreateKeyModal(false)}
                  className="px-3.5 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg font-medium"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="px-3.5 py-1.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-semibold"
                >
                  Sinh Khóa API
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal: Display Plaintext Key */}
      {createdKeyResult && (
        <div className="fixed inset-0 bg-black/40 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl max-w-md w-full p-6 shadow-xl border border-slate-200">
            <div className="flex items-center gap-2 text-emerald-600 mb-2">
              <CheckCircle2 className="w-5 h-5" />
              <h3 className="text-base font-bold text-slate-900">Khóa API Đã Được Tạo</h3>
            </div>
            <p className="text-xs text-rose-600 font-semibold mb-3">
              Lưu ý quan trọng: Chuỗi khóa bí mật chỉ hiển thị MỘT LẦN DUY NHẤT. Hãy sao chép và lưu trữ an toàn ngay bây giờ!
            </p>

            <div className="p-3 bg-slate-50 border border-slate-200 rounded-lg font-mono text-xs text-slate-800 break-all select-all flex items-center justify-between gap-2">
              <span>{createdKeyResult.plaintextKey}</span>
              <button
                onClick={() => {
                  navigator.clipboard.writeText(createdKeyResult.plaintextKey);
                  alert('Đã sao chép khóa API vào Clipboard.');
                }}
                className="p-1.5 bg-white border border-slate-200 rounded hover:bg-slate-100 shrink-0"
                title="Sao chép"
              >
                <Copy className="w-4 h-4" />
              </button>
            </div>

            <div className="flex justify-end pt-4 mt-2">
              <button
                onClick={() => setCreatedKeyResult(null)}
                className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-xs font-semibold"
              >
                Tôi Đã Lưu Khóa An Toàn
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal: Create Webhook */}
      {showCreateWebhookModal && (
        <div className="fixed inset-0 bg-black/40 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl max-w-lg w-full p-6 shadow-xl border border-slate-200">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-base font-bold text-slate-900 flex items-center gap-2">
                <Webhook className="w-5 h-5 text-blue-600" />
                Đăng Ký Webhook Endpoint Mới
              </h3>
              <button onClick={() => setShowCreateWebhookModal(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleCreateWebhook} className="space-y-4 text-xs">
              <div>
                <label className="block text-slate-600 mb-1 font-medium">Tên Webhook</label>
                <input
                  type="text"
                  required
                  placeholder="Ví dụ: Slack Notifier, ERP Accounting Sync..."
                  value={webhookForm.name}
                  onChange={(e) => setWebhookForm({ ...webhookForm, name: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 font-semibold"
                />
              </div>

              <div>
                <label className="block text-slate-600 mb-1 font-medium">URL Đích (Endpoint HTTPS)</label>
                <input
                  type="url"
                  required
                  placeholder="https://your-domain.com/api/webhook"
                  value={webhookForm.url}
                  onChange={(e) => setWebhookForm({ ...webhookForm, url: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 font-mono text-[11px]"
                />
              </div>

              <div>
                <label className="block text-slate-600 mb-1 font-medium">Sự kiện Đăng ký (Events)</label>
                <div className="grid grid-cols-2 gap-2 p-2 border border-slate-200 rounded-lg max-h-40 overflow-y-auto">
                  {availableEvents.map((evt) => (
                    <label key={evt} className="flex items-center gap-2 text-[11px] cursor-pointer">
                      <input
                        type="checkbox"
                        checked={webhookForm.events.includes(evt)}
                        onChange={(e) => {
                          const updated = e.target.checked
                            ? [...webhookForm.events, evt]
                            : webhookForm.events.filter((x) => x !== evt);
                          setWebhookForm({ ...webhookForm, events: updated });
                        }}
                        className="rounded text-blue-600"
                      />
                      <span className="font-mono text-slate-700">{evt}</span>
                    </label>
                  ))}
                </div>
              </div>

              <div className="flex justify-end gap-2 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setShowCreateWebhookModal(false)}
                  className="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg font-medium"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-semibold"
                >
                  Lưu Webhook
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
