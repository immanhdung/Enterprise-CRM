import React, { useState } from 'react';
import { AiNavigation } from './AiNavigation';
import { aiService } from '../../services/aiService';
import type { BusinessQueryResult } from '../../types/ai';
import { MessageSquareCode, Search, Sparkles, Table, Info } from 'lucide-react';

export const AiQueryPage: React.FC = () => {
  const [queryText, setQueryText] = useState('');
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<BusinessQueryResult | null>(null);

  const sampleQueries = [
    'Doanh thu thực thu và hợp đồng ký mới tháng này?',
    'Top 5 nhân viên sales xuất sắc nhất có doanh số thế nào?',
    'Tổng hợp công nợ quá hạn và các đợt sắp đến hạn?',
    'Phân bổ số lượng cơ hội bán hàng (Deals) theo giai đoạn pipeline?',
    'Nguồn mang lại nhiều khách hàng tiềm năng (Leads) nhất trong tháng?',
  ];

  const handleExecuteQuery = async (queryToRun?: string) => {
    const q = (queryToRun || queryText).trim();
    if (!q || loading) return;

    if (queryToRun) setQueryText(queryToRun);
    setLoading(true);

    try {
      const res = await aiService.executeBusinessQuery({ query: q });
      setResult(res);
    } catch (err: any) {
      alert(`Lỗi thực thi truy vấn: ${err?.message || 'Có lỗi xảy ra'}`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-6 max-w-6xl mx-auto">
      <div className="flex justify-between items-center mb-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Truy vấn Dữ liệu Tự nhiên (Natural Language Business Query)</h1>
          <p className="text-sm text-gray-500">
            Hỏi số liệu kinh doanh bằng tiếng Việt hoặc tiếng Anh – Hệ thống tự động biên dịch và truy vấn thời gian thực từ cơ sở dữ liệu
          </p>
        </div>
      </div>

      <AiNavigation />

      {/* Query Search Bar */}
      <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm mb-6">
        <form
          onSubmit={(e) => {
            e.preventDefault();
            handleExecuteQuery();
          }}
          className="flex gap-2"
        >
          <div className="relative flex-1">
            <Search className="w-5 h-5 absolute left-3.5 top-3 text-gray-400" />
            <input
              type="text"
              value={queryText}
              onChange={(e) => setQueryText(e.target.value)}
              placeholder="VD: Doanh thu thực thu tháng này, Top 5 sales xuất sắc, Công nợ quá hạn..."
              className="w-full pl-11 pr-4 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-purple-500"
            />
          </div>
          <button
            type="submit"
            disabled={loading || !queryText.trim()}
            className="flex items-center gap-2 bg-purple-600 hover:bg-purple-700 disabled:opacity-50 text-white px-6 py-2.5 rounded-lg text-sm font-semibold shadow-sm transition-colors"
          >
            <Sparkles className="w-4 h-4" />
            <span>{loading ? 'Đang truy vấn...' : 'Truy vấn'}</span>
          </button>
        </form>

        {/* Sample Query Suggestions */}
        <div className="mt-4">
          <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider block mb-2">
            Câu hỏi gợi ý nhanh:
          </span>
          <div className="flex flex-wrap gap-2">
            {sampleQueries.map((q, idx) => (
              <button
                key={idx}
                onClick={() => handleExecuteQuery(q)}
                className="text-xs bg-gray-50 hover:bg-purple-50 hover:text-purple-700 hover:border-purple-200 text-gray-600 px-3 py-1.5 rounded-full border border-gray-200 transition-colors"
              >
                {q}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Query Result Presentation */}
      {result && (
        <div className="space-y-6 animate-in fade-in">
          {/* Answer Summary Card */}
          <div className="bg-purple-50 border border-purple-200 p-5 rounded-lg shadow-sm">
            <div className="flex items-start gap-3">
              <div className="p-2 rounded-lg bg-purple-600 text-white shrink-0 mt-0.5">
                <MessageSquareCode className="w-5 h-5" />
              </div>
              <div className="flex-1">
                <div className="flex items-center justify-between">
                  <h3 className="font-bold text-gray-900 text-sm">{result.metricName}</h3>
                  <span className="text-xs bg-purple-100 text-purple-800 font-semibold px-2.5 py-0.5 rounded-full">
                    Kỳ: {result.period}
                  </span>
                </div>
                <p className="text-sm text-gray-800 font-medium mt-1 leading-relaxed">{result.answerText}</p>
                {result.summaryNote && (
                  <p className="text-xs text-gray-500 mt-2 flex items-center gap-1">
                    <Info className="w-3.5 h-3.5" />
                    <span>{result.summaryNote}</span>
                  </p>
                )}
              </div>
            </div>
          </div>

          {/* Structured Data Table */}
          {result.columns && result.columns.length > 0 && (
            <div className="bg-white rounded-lg border border-gray-200 shadow-sm overflow-hidden">
              <div className="px-6 py-3 border-b border-gray-200 bg-gray-50 flex items-center gap-2">
                <Table className="w-4 h-4 text-gray-500" />
                <h4 className="text-sm font-semibold text-gray-700">Dữ liệu Bảng trích xuất ({result.rows?.length || 0} dòng)</h4>
              </div>
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200 text-sm">
                  <thead className="bg-gray-50 text-gray-700 font-semibold">
                    <tr>
                      {result.columns.map((col, idx) => (
                        <th key={idx} className="px-6 py-3 text-left">
                          {col}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {result.rows.map((row, rIdx) => (
                      <tr key={rIdx} className="hover:bg-gray-50">
                        {result.columns.map((col, cIdx) => {
                          const val = row[col];
                          const isNumber = typeof val === 'number';
                          return (
                            <td key={cIdx} className="px-6 py-3 text-gray-900">
                              {isNumber ? Number(val).toLocaleString('vi-VN') : String(val ?? '')}
                            </td>
                          );
                        })}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
