import React, { useState, useEffect } from 'react';
import { AiNavigation } from './AiNavigation';
import { aiService } from '../../services/aiService';
import type { AiProviderConfig, AiUsageSummary } from '../../types/ai';
import { Sliders, Save, Key, Cpu, Zap, Activity, RefreshCw } from 'lucide-react';

export const AiSettingsPage: React.FC = () => {
  const [config, setConfig] = useState<AiProviderConfig | null>(null);
  const [usage, setUsage] = useState<AiUsageSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  // Form states
  const [providerType, setProviderType] = useState('Gemini');
  const [modelName, setModelName] = useState('gemini-1.5-flash');
  const [endpointUrl, setEndpointUrl] = useState('');
  const [apiKey, setApiKey] = useState('');
  const [temperature, setTemperature] = useState(0.2);
  const [maxTokens, setMaxTokens] = useState(2048);
  const [isActive, setIsActive] = useState(true);

  const loadData = async () => {
    try {
      setLoading(true);
      const [cfg, usg] = await Promise.all([
        aiService.getProviderConfig(),
        aiService.getUsageSummary(),
      ]);
      setConfig(cfg);
      setUsage(usg);

      setProviderType(cfg.providerType);
      setModelName(cfg.modelName);
      setEndpointUrl(cfg.endpointUrl || '');
      setTemperature(cfg.temperature);
      setMaxTokens(cfg.maxTokens);
      setIsActive(cfg.isActive);
    } catch (err) {
      console.error('Failed to load AI settings:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setSaving(true);
      await aiService.updateProviderConfig({
        providerType,
        modelName,
        endpointUrl: endpointUrl.trim() || undefined,
        apiKey: apiKey.trim() || undefined,
        temperature,
        maxTokens,
        isActive,
      });
      alert('Cấu hình AI Provider đã được cập nhật thành công.');
      setApiKey('');
      loadData();
    } catch (err: any) {
      alert(`Lỗi lưu cấu hình: ${err?.message || 'Có lỗi xảy ra'}`);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="p-6 max-w-5xl mx-auto">
      <div className="flex justify-between items-center mb-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Cấu hình Nhà cung cấp AI (AI Provider & Usage)</h1>
          <p className="text-sm text-gray-500">
            Quản lý API kết nối Google Gemini, OpenAI, thiết lập tham số sinh văn bản và theo dõi chi phí Token
          </p>
        </div>
      </div>

      <AiNavigation />

      {loading ? (
        <div className="flex items-center justify-center py-12">
          <RefreshCw className="w-8 h-8 animate-spin text-purple-600" />
        </div>
      ) : (
        <>
          {/* Usage Overview Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div className="bg-white p-5 rounded-lg border border-gray-200 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider">Tổng lượt truy vấn AI</span>
            <Zap className="w-5 h-5 text-purple-600" />
          </div>
          <div className="text-2xl font-bold text-gray-900 mt-2">{usage?.totalRequests ?? 0}</div>
          <div className="text-xs text-gray-500 mt-1">Lượt gọi Chat & NLP Query</div>
        </div>

        <div className="bg-white p-5 rounded-lg border border-gray-200 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider">Tổng Tokens Tiêu thụ</span>
            <Cpu className="w-5 h-5 text-blue-600" />
          </div>
          <div className="text-2xl font-bold text-gray-900 mt-2">
            {Number(usage?.totalTokens ?? 0).toLocaleString('vi-VN')}
          </div>
          <div className="text-xs text-gray-500 mt-1">Prompt + Completion Tokens</div>
        </div>

        <div className="bg-white p-5 rounded-lg border border-gray-200 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider">Ước tính Chi phí API</span>
            <Activity className="w-5 h-5 text-green-600" />
          </div>
          <div className="text-2xl font-bold text-green-700 mt-2">
            ${usage?.totalEstimatedCostUsd?.toFixed(4) ?? '0.0000'} <span className="text-xs font-normal text-gray-500">USD</span>
          </div>
          <div className="text-xs text-gray-500 mt-1">Theo biểu giá chuẩn Gemini Flash</div>
        </div>
      </div>

      {/* Configuration Form */}
      <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm mb-6">
        <h2 className="text-base font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <Sliders className="w-5 h-5 text-purple-600" />
          Thông số Nhà cung cấp AI
        </h2>

        <form onSubmit={handleSave} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Loại Provider</label>
              <select
                value={providerType}
                onChange={(e) => setProviderType(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white"
              >
                <option value="Gemini">Google Gemini (Khuyến nghị)</option>
                <option value="OpenAI">OpenAI (GPT-4o / GPT-4o-mini)</option>
                <option value="LocalCompatible">Local / Tương thích OpenAI Endpoint</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Tên Model</label>
              <input
                type="text"
                value={modelName}
                onChange={(e) => setModelName(e.target.value)}
                placeholder="VD: gemini-1.5-flash, gpt-4o-mini"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                API Key {config?.hasApiKey && <span className="text-xs text-green-600 font-semibold">(Đã thiết lập)</span>}
              </label>
              <div className="relative">
                <Key className="w-4 h-4 absolute left-3 top-3 text-gray-400" />
                <input
                  type="password"
                  value={apiKey}
                  onChange={(e) => setApiKey(e.target.value)}
                  placeholder={config?.hasApiKey ? '•••••••••••••••••••• (Nhập để thay đổi)' : 'Dán API Key vào đây...'}
                  className="w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg text-sm"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Endpoint tùy biến (Tùy chọn)</label>
              <input
                type="text"
                value={endpointUrl}
                onChange={(e) => setEndpointUrl(e.target.value)}
                placeholder="https://your-custom-endpoint.com/v1"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Nhiệt độ sáng tạo (Temperature): {temperature}
              </label>
              <input
                type="range"
                min="0.0"
                max="1.0"
                step="0.05"
                value={temperature}
                onChange={(e) => setTemperature(parseFloat(e.target.value))}
                className="w-full accent-purple-600"
              />
              <div className="flex justify-between text-[11px] text-gray-400 mt-1">
                <span>0.0 (Chính xác số liệu)</span>
                <span>1.0 (Sáng tạo văn phong)</span>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Giới hạn Tokens đầu ra (Max Tokens)</label>
              <input
                type="number"
                min="256"
                max="8192"
                step="256"
                value={maxTokens}
                onChange={(e) => setMaxTokens(parseInt(e.target.value) || 2048)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
              />
            </div>
          </div>

          <div className="flex items-center justify-between pt-4 border-t border-gray-200">
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="activeProvider"
                checked={isActive}
                onChange={(e) => setIsActive(e.target.checked)}
                className="w-4 h-4 text-purple-600 rounded"
              />
              <label htmlFor="activeProvider" className="text-sm font-medium text-gray-800">
                Kích hoạt chế độ AI trực tuyến (Online Provider Mode)
              </label>
            </div>

            <button
              type="submit"
              disabled={saving}
              className="flex items-center gap-2 bg-purple-600 hover:bg-purple-700 text-white px-5 py-2 rounded-lg text-sm font-semibold shadow-sm transition-colors"
            >
              <Save className="w-4 h-4" />
              <span>{saving ? 'Đang lưu...' : 'Lưu Cấu hình'}</span>
            </button>
          </div>
        </form>
      </div>

      {/* Recent AI Usage Logs */}
      {usage?.recentLogs && usage.recentLogs.length > 0 && (
        <div className="bg-white rounded-lg border border-gray-200 shadow-sm overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200 bg-gray-50">
            <h3 className="text-sm font-bold text-gray-900">Nhật ký Truy vấn AI gần đây (Recent Usage Logs)</h3>
          </div>
          <table className="min-w-full divide-y divide-gray-200 text-xs">
            <thead className="bg-gray-50 text-gray-600 font-semibold">
              <tr>
                <th className="px-6 py-3 text-left">Thao tác (Operation)</th>
                <th className="px-6 py-3 text-left">Model</th>
                <th className="px-6 py-3 text-center">Prompt Tokens</th>
                <th className="px-6 py-3 text-center">Completion Tokens</th>
                <th className="px-6 py-3 text-center">Thời gian (ms)</th>
                <th className="px-6 py-3 text-right">Thời điểm</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {usage.recentLogs.map((log) => (
                <tr key={log.id} className="hover:bg-gray-50">
                  <td className="px-6 py-3 font-semibold text-gray-900">{log.operationType}</td>
                  <td className="px-6 py-3 text-gray-600 font-mono">{log.modelName}</td>
                  <td className="px-6 py-3 text-center">{log.promptTokens}</td>
                  <td className="px-6 py-3 text-center">{log.completionTokens}</td>
                  <td className="px-6 py-3 text-center text-gray-500">{log.durationMs} ms</td>
                  <td className="px-6 py-3 text-right text-gray-400">
                    {new Date(log.timestamp).toLocaleTimeString('vi-VN')}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
        </>
      )}
    </div>
  );
};
