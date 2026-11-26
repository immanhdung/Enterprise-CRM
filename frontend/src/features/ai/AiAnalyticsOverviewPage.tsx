import React, { useState, useEffect } from 'react';
import { AiNavigation } from './AiNavigation';
import { aiService } from '../../services/aiService';
import type { SalesAnalysisSummary } from '../../types/ai';
import { TrendingUp, CheckCircle, AlertTriangle, Lightbulb, RefreshCw, Activity, Award } from 'lucide-react';

export const AiAnalyticsOverviewPage: React.FC = () => {
  const [analysis, setAnalysis] = useState<SalesAnalysisSummary | null>(null);
  const [loading, setLoading] = useState(true);

  const fetchAnalysis = async () => {
    try {
      setLoading(true);
      const data = await aiService.getPerformanceAnalysis();
      setAnalysis(data);
    } catch (err) {
      console.error('Failed to load performance analysis:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAnalysis();
  }, []);

  return (
    <div className="p-6 max-w-6xl mx-auto">
      <div className="flex justify-between items-center mb-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Phân tích Hiệu suất Kinh doanh AI (AI Sales Analytics)</h1>
          <p className="text-sm text-gray-500">
            Tự động đánh giá điểm mạnh, điểm yếu và các cơ hội tăng trưởng doanh thu dựa trên dữ liệu CRM thực tế
          </p>
        </div>
        <button
          onClick={fetchAnalysis}
          disabled={loading}
          className="flex items-center gap-2 px-3.5 py-1.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
        >
          <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
          Phân tích lại
        </button>
      </div>

      <AiNavigation />

      {loading ? (
        <div className="flex justify-center items-center h-48 bg-white rounded-lg border border-gray-200">
          <RefreshCw className="w-6 h-6 animate-spin text-purple-600" />
        </div>
      ) : analysis ? (
        <div className="space-y-6">
          {/* Executive Overview Summary */}
          <div className="bg-gradient-to-r from-purple-900 to-indigo-900 text-white p-6 rounded-xl shadow-md">
            <div className="flex items-center gap-2 text-purple-300 text-xs font-semibold uppercase tracking-wider mb-2">
              <TrendingUp className="w-4 h-4" /> Báo cáo Đánh giá Thông minh
            </div>
            <p className="text-base font-medium leading-relaxed">{analysis.overviewText}</p>
          </div>

          {/* Key Health Scores */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm flex items-center justify-between">
              <div>
                <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider">
                  Tỷ lệ chốt hợp đồng (Win Rate)
                </span>
                <div className="text-3xl font-bold text-gray-900 mt-2">{analysis.currentMonthWinRate}%</div>
                <p className="text-xs text-gray-500 mt-1">So với các thương vụ đã đóng trong tháng</p>
              </div>
              <div className="w-14 h-14 rounded-full bg-blue-50 text-blue-600 flex items-center justify-center">
                <Award className="w-7 h-7" />
              </div>
            </div>

            <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm flex items-center justify-between">
              <div>
                <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider">
                  Điểm sức khỏe Pipeline (Pipeline Health)
                </span>
                <div className="text-3xl font-bold text-purple-700 mt-2">
                  {analysis.pipelineHealthScore} / 100
                </div>
                <p className="text-xs text-gray-500 mt-1">Dựa trên tốc độ chuyển đổi và độ cân bằng phễu</p>
              </div>
              <div className="w-14 h-14 rounded-full bg-purple-50 text-purple-600 flex items-center justify-center">
                <Activity className="w-7 h-7" />
              </div>
            </div>
          </div>

          {/* Strengths & Weaknesses */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Strengths */}
            <div className="bg-white p-6 rounded-lg border border-green-200 shadow-sm">
              <h3 className="text-base font-bold text-green-900 flex items-center gap-2 mb-4">
                <CheckCircle className="w-5 h-5 text-green-600" />
                Điểm mạnh nổi bật (Strengths)
              </h3>
              <ul className="space-y-3">
                {analysis.strengths.map((str, idx) => (
                  <li key={idx} className="flex items-start gap-2.5 text-sm text-gray-700">
                    <span className="w-1.5 h-1.5 rounded-full bg-green-500 mt-2 shrink-0" />
                    <span>{str}</span>
                  </li>
                ))}
              </ul>
            </div>

            {/* Weaknesses */}
            <div className="bg-white p-6 rounded-lg border border-amber-200 shadow-sm">
              <h3 className="text-base font-bold text-amber-900 flex items-center gap-2 mb-4">
                <AlertTriangle className="w-5 h-5 text-amber-600" />
                Điểm cần khắc phục (Weaknesses)
              </h3>
              <ul className="space-y-3">
                {analysis.weaknesses.map((weak, idx) => (
                  <li key={idx} className="flex items-start gap-2.5 text-sm text-gray-700">
                    <span className="w-1.5 h-1.5 rounded-full bg-amber-500 mt-2 shrink-0" />
                    <span>{weak}</span>
                  </li>
                ))}
              </ul>
            </div>
          </div>

          {/* Actionable Recommendations */}
          <div className="bg-white p-6 rounded-lg border border-purple-200 shadow-sm">
            <h3 className="text-base font-bold text-purple-900 flex items-center gap-2 mb-4">
              <Lightbulb className="w-5 h-5 text-purple-600" />
              Khuyến nghị hành động tối ưu (Actionable Recommendations)
            </h3>
            <div className="space-y-3">
              {analysis.actionableRecommendations.map((rec, idx) => (
                <div key={idx} className="flex items-start gap-3 bg-purple-50 p-3.5 rounded-lg border border-purple-100 text-sm text-gray-800">
                  <span className="w-6 h-6 rounded-full bg-purple-200 text-purple-800 font-bold text-xs flex items-center justify-center shrink-0">
                    {idx + 1}
                  </span>
                  <span className="mt-0.5">{rec}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
};
