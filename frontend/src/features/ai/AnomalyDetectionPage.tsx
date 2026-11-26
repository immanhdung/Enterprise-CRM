import React, { useState, useEffect } from 'react';
import { AiNavigation } from './AiNavigation';
import { aiService } from '../../services/aiService';
import type { AnomalyDetection } from '../../types/ai';
import { AlertOctagon, AlertTriangle, CheckCircle2, RefreshCw, ShieldAlert, ArrowUpRight } from 'lucide-react';

export const AnomalyDetectionPage: React.FC = () => {
  const [anomalies, setAnomalies] = useState<AnomalyDetection[]>([]);
  const [loading, setLoading] = useState(true);
  const [scanning, setScanning] = useState(false);

  const fetchAnomalies = async () => {
    try {
      setLoading(true);
      const data = await aiService.getAnomalies();
      setAnomalies(data);
    } catch (err) {
      console.error('Failed to load anomalies:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAnomalies();
  }, []);

  const handleScan = async () => {
    try {
      setScanning(true);
      const count = await aiService.runAnomalyScan();
      alert(`Đã hoàn tất quét: phát hiện ${count} chỉ số có độ lệch bất thường.`);
      fetchAnomalies();
    } catch (err: any) {
      alert(`Lỗi quét bất thường: ${err?.message || 'Có lỗi xảy ra'}`);
    } finally {
      setScanning(false);
    }
  };

  const handleResolve = async (id: string) => {
    try {
      await aiService.resolveAnomaly(id, { status: 'Resolved', resolutionNotes: 'Đã xử lý và kiểm soát rủi ro.' });
      fetchAnomalies();
    } catch (err) {
      console.error('Failed to resolve anomaly:', err);
    }
  };

  return (
    <div className="p-6 max-w-6xl mx-auto">
      <div className="flex justify-between items-center mb-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Phát hiện Bất thường Kinh doanh (AI Anomaly Detection)</h1>
          <p className="text-sm text-gray-500">
            Hệ thống phát hiện sớm các rủi ro: sụt giảm doanh số đột ngột, tắc nghẽn deal kéo dài, tỷ lệ deal thất bại tăng vọt
          </p>
        </div>
        <button
          onClick={handleScan}
          disabled={scanning}
          className="flex items-center gap-2 bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-lg text-sm font-medium shadow-sm transition-colors"
        >
          <ShieldAlert className={`w-4 h-4 ${scanning ? 'animate-spin' : ''}`} />
          <span>{scanning ? 'Đang quét phân tích...' : 'Quét Bất thường Ngay'}</span>
        </button>
      </div>

      <AiNavigation />

      {loading ? (
        <div className="flex justify-center items-center h-48 bg-white rounded-lg border border-gray-200">
          <RefreshCw className="w-6 h-6 animate-spin text-purple-600" />
        </div>
      ) : anomalies.length === 0 ? (
        <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
          <CheckCircle2 className="w-12 h-12 text-green-500 mx-auto mb-3" />
          <h3 className="text-base font-semibold text-gray-900">Không có chỉ số bất thường nào</h3>
          <p className="text-sm text-gray-500 mt-1">
            Tất cả các chỉ số dòng tiền, pipeline, tỷ lệ chốt và công nợ đều nằm trong biên độ an toàn.
          </p>
        </div>
      ) : (
        <div className="space-y-4">
          {anomalies.map((anom) => {
            const isCritical = anom.severity === 'Critical' || anom.severity === 'High';
            return (
              <div
                key={anom.id}
                className="bg-white rounded-lg border border-gray-200 shadow-sm p-5 hover:border-red-200 transition-colors"
              >
                <div className="flex items-start justify-between">
                  <div className="flex items-start gap-3">
                    <div className="mt-1">
                      {isCritical ? (
                        <AlertOctagon className="w-6 h-6 text-red-600" />
                      ) : (
                        <AlertTriangle className="w-6 h-6 text-amber-600" />
                      )}
                    </div>
                    <div>
                      <div className="flex items-center gap-2">
                        <h3 className="text-base font-bold text-gray-900">{anom.metricName}</h3>
                        <span
                          className={`text-xs px-2.5 py-0.5 rounded-full font-bold uppercase ${
                            isCritical
                              ? 'bg-red-50 text-red-700 border border-red-200'
                              : 'bg-amber-50 text-amber-700 border border-amber-200'
                          }`}
                        >
                          {anom.severity}
                        </span>
                        <span className="text-xs bg-gray-100 text-gray-700 px-2 py-0.5 rounded font-medium">
                          {anom.category}
                        </span>
                      </div>

                      <p className="text-sm text-gray-700 mt-1.5 leading-relaxed">{anom.description}</p>

                      <div className="flex flex-wrap items-center gap-4 text-xs text-gray-500 mt-3 pt-3 border-t border-gray-100">
                        <span className="flex items-center gap-1 font-semibold text-gray-800">
                          <ArrowUpRight className="w-3.5 h-3.5 text-red-500" />
                          Độ lệch: {anom.deviationPercent}%
                        </span>
                        <span>
                          Thực tế: <strong className="text-gray-900">{Number(anom.actualValue).toLocaleString('vi-VN')}</strong>
                        </span>
                        <span>
                          Kỳ vọng: <strong className="text-gray-900">{Number(anom.expectedValue).toLocaleString('vi-VN')}</strong>
                        </span>
                        <span>Thời gian phát hiện: {new Date(anom.detectedAt).toLocaleString('vi-VN')}</span>
                      </div>
                    </div>
                  </div>

                  <div>
                    {anom.status !== 'Resolved' ? (
                      <button
                        onClick={() => handleResolve(anom.id)}
                        className="px-3.5 py-1.5 text-xs font-semibold text-green-700 bg-green-50 hover:bg-green-100 border border-green-200 rounded-lg transition-colors"
                      >
                        Đánh dấu giải quyết
                      </button>
                    ) : (
                      <span className="text-xs text-green-700 font-semibold bg-green-50 px-2.5 py-1 rounded-full border border-green-200">
                        Đã giải quyết
                      </span>
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
