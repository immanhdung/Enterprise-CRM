import React, { useState, useEffect } from 'react';
import { AiNavigation } from './AiNavigation';
import { aiService } from '../../services/aiService';
import type { BusinessRecommendation } from '../../types/ai';
import { Lightbulb, Check, X, RefreshCw } from 'lucide-react';

export const BusinessRecommendationsPage: React.FC = () => {
  const [recommendations, setRecommendations] = useState<BusinessRecommendation[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchRecommendations = async () => {
    try {
      setLoading(true);
      const data = await aiService.getRecommendations();
      setRecommendations(data);
    } catch (err) {
      console.error('Failed to load recommendations:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRecommendations();
  }, []);

  const handleAction = async (id: string, action: 'Apply' | 'Dismiss') => {
    try {
      await aiService.applyRecommendation(id, action);
      fetchRecommendations();
    } catch (err) {
      console.error('Error applying recommendation:', err);
    }
  };

  return (
    <div className="p-6 max-w-6xl mx-auto">
      <div className="flex justify-between items-center mb-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Đề xuất Hành động Thông minh (AI Recommendations)</h1>
          <p className="text-sm text-gray-500">
            Các hành động tối ưu hóa kinh doanh: ưu tiên khách hàng tiềm năng, can thiệp deal lớn, đôn đốc thu hồi nợ
          </p>
        </div>
        <button
          onClick={fetchRecommendations}
          disabled={loading}
          className="flex items-center gap-2 px-3.5 py-1.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
        >
          <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
          Làm mới
        </button>
      </div>

      <AiNavigation />

      {loading ? (
        <div className="flex justify-center items-center h-48 bg-white rounded-lg border border-gray-200">
          <RefreshCw className="w-6 h-6 animate-spin text-purple-600" />
        </div>
      ) : recommendations.length === 0 ? (
        <div className="bg-white rounded-lg border border-gray-200 p-12 text-center text-gray-500">
          Hiện chưa có đề xuất mới nào.
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {recommendations.map((rec) => {
            const isUrgent = rec.priority === 'Urgent';
            const isHigh = rec.priority === 'High';
            return (
              <div
                key={rec.id}
                className="bg-white rounded-lg border border-gray-200 p-5 shadow-sm flex flex-col justify-between hover:border-purple-200 transition-colors"
              >
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <span
                      className={`text-[11px] px-2.5 py-0.5 rounded-full font-bold uppercase ${
                        isUrgent
                          ? 'bg-red-50 text-red-700 border border-red-200'
                          : isHigh
                          ? 'bg-amber-50 text-amber-700 border border-amber-200'
                          : 'bg-blue-50 text-blue-700 border border-blue-200'
                      }`}
                    >
                      {rec.priority}
                    </span>
                    <span className="text-xs bg-purple-50 text-purple-700 px-2 py-0.5 rounded font-medium">
                      {rec.category}
                    </span>
                  </div>

                  <h3 className="font-bold text-gray-900 text-sm mb-1.5 flex items-start gap-1.5">
                    <Lightbulb className="w-4 h-4 text-purple-600 shrink-0 mt-0.5" />
                    <span>{rec.title}</span>
                  </h3>

                  <p className="text-sm text-gray-600 leading-relaxed mb-4">{rec.description}</p>
                </div>

                <div className="pt-3 border-t border-gray-100 flex items-center justify-between">
                  <div className="text-xs text-gray-400">
                    Đối tượng: <span className="font-medium text-gray-700">{rec.targetEntityType}</span>
                  </div>

                  {rec.status === 'Pending' ? (
                    <div className="flex items-center gap-2">
                      <button
                        onClick={() => handleAction(rec.id, 'Dismiss')}
                        className="p-1.5 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded"
                        title="Bỏ qua"
                      >
                        <X className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => handleAction(rec.id, 'Apply')}
                        className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-purple-600 hover:bg-purple-700 text-white rounded-lg text-xs font-semibold shadow-sm transition-colors"
                      >
                        <Check className="w-3.5 h-3.5" />
                        <span>Áp dụng</span>
                      </button>
                    </div>
                  ) : (
                    <span className="text-xs font-medium text-purple-700 bg-purple-50 px-2.5 py-0.5 rounded">
                      Đã xử lý: {rec.status}
                    </span>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};
