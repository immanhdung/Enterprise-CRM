import React, { useState, useEffect } from 'react';
import { AiNavigation } from './AiNavigation';
import { aiService } from '../../services/aiService';
import type { RevenueForecast } from '../../types/ai';
import { RefreshCw, Calendar, ShieldCheck, HelpCircle } from 'lucide-react';

export const RevenueForecastPage: React.FC = () => {
  const [horizonDays, setHorizonDays] = useState<number>(30);
  const [forecast, setForecast] = useState<RevenueForecast | null>(null);
  const [loading, setLoading] = useState(true);
  const [recalculating, setRecalculating] = useState(false);

  const fetchForecast = async (days = horizonDays) => {
    try {
      setLoading(true);
      const data = await aiService.getRevenueForecast(days);
      setForecast(data);
    } catch (err) {
      console.error('Failed to load forecast:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchForecast(horizonDays);
  }, [horizonDays]);

  const handleRecalculate = async () => {
    try {
      setRecalculating(true);
      const data = await aiService.recalculateForecast(horizonDays);
      setForecast(data);
      alert('Đã tính toán lại mô hình dự báo hồi quy tuyến tính.');
    } catch (err: any) {
      alert(`Lỗi: ${err?.message || 'Không thể tính lại dự báo'}`);
    } finally {
      setRecalculating(false);
    }
  };

  const horizons = [
    { days: 7, label: '7 ngày tới' },
    { days: 30, label: '30 ngày tới (Tháng)' },
    { days: 90, label: '90 ngày tới (Quý)' },
    { days: 365, label: '365 ngày tới (Năm)' },
  ];

  return (
    <div className="p-6 max-w-6xl mx-auto">
      <div className="flex justify-between items-center mb-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dự báo Doanh thu AI (Statistical Revenue Forecasting)</h1>
          <p className="text-sm text-gray-500">
            Mô hình toán học: Hồi quy tuyến tính dòng tiền thực thu lịch sử + Trọng số xác suất Deal trong Pipeline
          </p>
        </div>
        <button
          onClick={handleRecalculate}
          disabled={recalculating}
          className="flex items-center gap-2 bg-purple-600 hover:bg-purple-700 text-white px-4 py-2 rounded-lg text-sm font-medium shadow-sm transition-colors"
        >
          <RefreshCw className={`w-4 h-4 ${recalculating ? 'animate-spin' : ''}`} />
          <span>{recalculating ? 'Đang tính toán...' : 'Tính lại Dự báo'}</span>
        </button>
      </div>

      <AiNavigation />

      {/* Horizon Selector Tabs */}
      <div className="flex gap-2 mb-6">
        {horizons.map((h) => (
          <button
            key={h.days}
            onClick={() => setHorizonDays(h.days)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
              horizonDays === h.days
                ? 'bg-purple-600 text-white shadow-sm'
                : 'bg-white text-gray-600 hover:bg-gray-50 border border-gray-200'
            }`}
          >
            {h.label}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="flex justify-center items-center h-48 bg-white rounded-lg border border-gray-200">
          <RefreshCw className="w-6 h-6 animate-spin text-purple-600" />
        </div>
      ) : forecast ? (
        <div className="space-y-6">
          {/* Main Forecast Metrics */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="bg-white p-6 rounded-lg border border-purple-200 shadow-sm md:col-span-1">
              <span className="text-xs font-semibold text-purple-600 uppercase tracking-wider block">
                Doanh thu Dự kiến ({forecast.periodLabel})
              </span>
              <div className="text-3xl font-extrabold text-purple-900 mt-2">
                {forecast.predictedAmount.toLocaleString('vi-VN')} <span className="text-lg font-normal text-gray-500">VND</span>
              </div>
              <div className="flex items-center gap-2 text-xs text-green-700 bg-green-50 px-2 py-1 rounded mt-3 border border-green-200 font-medium">
                <ShieldCheck className="w-4 h-4 shrink-0" />
                <span>Độ tin cậy mô hình: {(forecast.confidenceScore * 100).toFixed(0)}% (Confidence Score)</span>
              </div>
            </div>

            <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm md:col-span-2">
              <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider block mb-3">
                Khoảng Dao động Tin cậy (85% Confidence Interval)
              </span>
              <div className="grid grid-cols-2 gap-4">
                <div className="bg-gray-50 p-4 rounded-lg border border-gray-200">
                  <span className="text-xs text-gray-500 font-medium">Giới hạn dưới (Kịch bản thận trọng -15%)</span>
                  <div className="text-xl font-bold text-gray-800 mt-1">
                    {forecast.lowerBound.toLocaleString('vi-VN')} VND
                  </div>
                </div>
                <div className="bg-gray-50 p-4 rounded-lg border border-gray-200">
                  <span className="text-xs text-gray-500 font-medium">Giới hạn trên (Kịch bản lạc quan +15%)</span>
                  <div className="text-xl font-bold text-gray-800 mt-1">
                    {forecast.upperBound.toLocaleString('vi-VN')} VND
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Visual Trend Bars */}
          <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm">
            <h3 className="text-base font-semibold text-gray-900 mb-6 flex items-center justify-between">
              <span>Đường xu hướng Lịch sử & Dự báo Phân bổ</span>
              <span className="text-xs font-normal text-gray-500 flex items-center gap-1">
                <Calendar className="w-4 h-4" /> Cập nhật: {new Date(forecast.forecastDate).toLocaleDateString('vi-VN')}
              </span>
            </h3>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Historical Trend */}
              <div>
                <h4 className="text-xs font-bold text-gray-600 uppercase tracking-wider mb-4">
                  Thực thu lịch sử các tháng trước
                </h4>
                <div className="space-y-3">
                  {forecast.historicalTrend.map((pt, idx) => (
                    <div key={idx} className="space-y-1">
                      <div className="flex justify-between text-xs text-gray-700">
                        <span className="font-medium">{pt.dateLabel}</span>
                        <span className="font-semibold">{pt.amount.toLocaleString('vi-VN')} VND</span>
                      </div>
                      <div className="w-full bg-gray-100 rounded-full h-2">
                        <div
                          className="bg-blue-600 h-2 rounded-full"
                          style={{ width: `${Math.min(100, (pt.amount / (forecast.predictedAmount || 1)) * 100)}%` }}
                        />
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              {/* Forecast Points */}
              <div>
                <h4 className="text-xs font-bold text-purple-700 uppercase tracking-wider mb-4">
                  Dự kiến theo phân kỳ ({forecast.periodLabel})
                </h4>
                <div className="space-y-3">
                  {forecast.forecastPoints.map((pt, idx) => (
                    <div key={idx} className="space-y-1">
                      <div className="flex justify-between text-xs text-gray-700">
                        <span className="font-medium text-purple-900">{pt.dateLabel} (Dự báo)</span>
                        <span className="font-semibold text-purple-700">{pt.amount.toLocaleString('vi-VN')} VND</span>
                      </div>
                      <div className="w-full bg-purple-50 rounded-full h-2">
                        <div
                          className="bg-purple-600 h-2 rounded-full"
                          style={{ width: `${Math.min(100, (pt.amount / (forecast.predictedAmount || 1)) * 100)}%` }}
                        />
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>

          {/* Model Assumptions Note */}
          <div className="bg-gray-50 border border-gray-200 p-5 rounded-lg text-xs text-gray-600">
            <h4 className="font-bold text-gray-800 mb-2 flex items-center gap-1.5">
              <HelpCircle className="w-4 h-4 text-purple-600" /> Giả định thuật toán mô hình ({forecast.modelType})
            </h4>
            <p className="leading-relaxed">
              Mô hình kết hợp dòng tiền lịch sử xác nhận (Historical Confirmed Run-rate) với tổng giá trị kỳ vọng của các thương vụ mở trong Pipeline, nhân với trọng số xác suất theo từng giai đoạn và chiết khấu thời gian (Time Horizon Discount Factor).
            </p>
          </div>
        </div>
      ) : null}
    </div>
  );
};
