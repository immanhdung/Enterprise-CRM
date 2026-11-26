import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  TrendingUp,
  AlertOctagon,
  Sparkles,
  RefreshCw,
  FileText,
  PieChart,
  ShieldAlert,
  ArrowUpRight,
} from 'lucide-react';
import { aiService } from '@/services/aiService';

export const ExecutiveRoleDashboardPage: React.FC = () => {
  const { data: dashboard, isLoading, error, refetch } = useQuery({
    queryKey: ['executiveDashboard'],
    queryFn: () => aiService.getExecutiveDashboard(),
  });

  const formatVND = (val: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <RefreshCw className="w-8 h-8 animate-spin text-blue-600" />
      </div>
    );
  }

  if (error || !dashboard) {
    return (
      <div className="p-6 bg-red-50 text-red-700 rounded-xl border border-red-200">
        <h3 className="font-semibold text-lg">Không thể tải dữ liệu Executive Dashboard</h3>
        <p className="text-sm mt-1">Chức năng chỉ dành cho Ban giám đốc (CEO/BOD) hoặc Quản trị viên cấp cao.</p>
        <button
          onClick={() => refetch()}
          className="mt-4 px-4 py-2 bg-red-600 text-white rounded-lg text-sm font-medium hover:bg-red-700 transition"
        >
          Thử lại
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Top Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-xs">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-purple-50 text-purple-700 border border-purple-200">
              Ban Lãnh Đạo (Executive / C-Level)
            </span>
            <h1 className="text-2xl font-bold text-slate-900">Báo Cáo Tổng Quan Doanh Nghiệp Toàn Diện</h1>
          </div>
          <p className="text-sm text-slate-500 mt-1">
            Tổng quan tài chính, dòng tiền thực thu, công nợ, dự báo doanh thu và chỉ số vận hành toàn công ty.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={() => refetch()}
            className="flex items-center gap-1.5 px-3.5 py-2 text-sm font-medium text-slate-700 bg-slate-50 hover:bg-slate-100 rounded-lg border border-slate-200 transition"
          >
            <RefreshCw className="w-4 h-4" />
            Làm mới
          </button>
          <Link
            to="/reports"
            className="flex items-center gap-1.5 px-3.5 py-2 text-sm font-medium text-indigo-700 bg-indigo-50 hover:bg-indigo-100 rounded-lg border border-indigo-200 transition"
          >
            <FileText className="w-4 h-4" />
            Báo cáo Quản trị
          </Link>
          <Link
            to="/ai-analytics/forecast"
            className="flex items-center gap-1.5 px-3.5 py-2 text-sm font-medium text-purple-700 bg-purple-50 hover:bg-purple-100 rounded-lg border border-purple-200 transition"
          >
            <Sparkles className="w-4 h-4" />
            Dự báo Doanh thu AI
          </Link>
        </div>
      </div>

      {/* Financial Health Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
          <div className="text-xs font-semibold text-slate-500 uppercase">Tổng Doanh Thu Hợp Đồng</div>
          <div className="text-xl font-bold text-slate-900 mt-1">{formatVND(dashboard.totalRevenue)}</div>
          <div className="text-xs text-slate-500 mt-2">Hợp đồng đang có hiệu lực</div>
        </div>

        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
          <div className="text-xs font-semibold text-slate-500 uppercase">Tiền Đã Thực Thu</div>
          <div className="text-xl font-bold text-emerald-600 mt-1">{formatVND(dashboard.totalCollectedCash)}</div>
          <div className="text-xs text-emerald-700 font-medium mt-2">
            Đạt {dashboard.totalRevenue > 0 ? ((dashboard.totalCollectedCash / dashboard.totalRevenue) * 100).toFixed(1) : 0}% giá trị hợp đồng
          </div>
        </div>

        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
          <div className="text-xs font-semibold text-slate-500 uppercase">Công Nợ Phải Thu</div>
          <div className="text-xl font-bold text-amber-600 mt-1">{formatVND(dashboard.totalReceivables)}</div>
          <div className="text-xs text-slate-500 mt-2">Chờ thanh toán theo kỳ</div>
        </div>

        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
          <div className="text-xs font-semibold text-slate-500 uppercase">Nợ Quá Hạn Cần Thu</div>
          <div className="text-xl font-bold text-rose-600 mt-1">{formatVND(dashboard.overdueDebtAmount)}</div>
          <div className="text-xs text-rose-600 font-medium mt-2">Cảnh báo rủi ro dòng tiền</div>
        </div>

        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
          <div className="text-xs font-semibold text-slate-500 uppercase">Chi Phí Lương Tháng Này</div>
          <div className="text-xl font-bold text-indigo-600 mt-1">{formatVND(dashboard.currentMonthPayrollTotal)}</div>
          <div className="text-xs text-slate-500 mt-2">Bao gồm lương cứng & hoa hồng</div>
        </div>
      </div>

      {/* KPI & Margin Highlights */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-linear-to-br from-slate-900 to-slate-800 text-white p-5 rounded-xl shadow-xs">
          <div className="text-xs font-medium uppercase tracking-wider text-slate-300">Tổng Giá Trị Pipeline</div>
          <div className="text-2xl font-bold mt-1">{formatVND(dashboard.totalPipelineValue)}</div>
          <div className="text-xs text-slate-400 mt-2">{dashboard.totalDealsInPipeline} cơ hội đang xúc tiến</div>
        </div>

        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
          <div className="text-xs font-semibold text-slate-500 uppercase">Deals Đã Chốt YTD</div>
          <div className="text-2xl font-bold text-slate-900 mt-1">{dashboard.totalWonDealsYearToDate}</div>
          <div className="text-xs text-emerald-600 font-semibold mt-2">Tỷ lệ chốt: {dashboard.winRatePercent.toFixed(1)}%</div>
        </div>

        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
          <div className="text-xs font-semibold text-slate-500 uppercase">Tỷ Suất Lợi Nhuận Gộp</div>
          <div className="text-2xl font-bold text-slate-900 mt-1">{dashboard.grossMarginPercent.toFixed(1)}%</div>
          <div className="text-xs text-slate-500 mt-2">Ước tính theo dịch vụ bàn giao</div>
        </div>

        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs flex flex-col justify-center">
          <Link
            to="/ai-assistant/query"
            className="flex items-center justify-between p-3 bg-purple-50 hover:bg-purple-100 rounded-lg border border-purple-200 transition group"
          >
            <div>
              <div className="text-xs font-semibold text-purple-900">Truy vấn Ngôn ngữ Tự nhiên</div>
              <div className="text-[11px] text-purple-700">Hỏi số liệu kinh doanh tức thì</div>
            </div>
            <Sparkles className="w-5 h-5 text-purple-600 group-hover:scale-110 transition" />
          </Link>
        </div>
      </div>

      {/* Receivables Aging Breakdown */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-xs p-5">
        <h2 className="font-semibold text-slate-900 mb-3 flex items-center gap-2">
          <PieChart className="w-4 h-4 text-blue-600" />
          Phân Tích Tuổi Nợ Khách Hàng (Receivables Aging)
        </h2>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {Object.entries(dashboard.receivablesAging).map(([bracket, amount]) => (
            <div
              key={bracket}
              className={`p-4 rounded-xl border ${
                bracket.includes('90') && amount > 0
                  ? 'border-rose-200 bg-rose-50/40'
                  : 'border-slate-200 bg-slate-50/40'
              }`}
            >
              <div className="text-xs font-medium text-slate-600">{bracket}</div>
              <div className="text-lg font-bold text-slate-900 mt-1">{formatVND(amount)}</div>
            </div>
          ))}
        </div>
      </div>

      {/* 2 Column: High-Value Deals & Critical Anomalies */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* High-Value Deals Closing Soon */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs p-5">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-semibold text-slate-900 flex items-center gap-2">
              <TrendingUp className="w-4 h-4 text-emerald-600" />
              Cơ hội giá trị cao dự kiến chốt sắp tới
            </h2>
            <Link to="/deals" className="text-xs text-blue-600 hover:underline">
              Xem tất cả
            </Link>
          </div>

          {dashboard.topDealsClosingSoon.length === 0 ? (
            <div className="text-center py-8 text-slate-400 text-sm">Chưa có cơ hội lớn nào sắp chốt</div>
          ) : (
            <div className="divide-y divide-slate-100">
              {dashboard.topDealsClosingSoon.map((deal) => (
                <div key={deal.id} className="py-3 flex items-center justify-between">
                  <div>
                    <Link
                      to={`/deals/${deal.id}`}
                      className="font-medium text-slate-800 hover:text-blue-600 text-sm flex items-center gap-1"
                    >
                      {deal.title}
                      <ArrowUpRight className="w-3.5 h-3.5 text-slate-400" />
                    </Link>
                    <div className="text-xs text-slate-500 mt-0.5">
                      Khách hàng: {deal.accountName} • {deal.stageName} ({deal.probability}%)
                    </div>
                  </div>
                  <div className="text-right">
                    <div className="font-semibold text-sm text-slate-900">{formatVND(deal.amount)}</div>
                    {deal.expectedCloseDate && (
                      <div className="text-xs text-slate-500">Dự kiến: {deal.expectedCloseDate}</div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Critical Anomalies & High-Priority Recommendations */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs p-5">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-semibold text-slate-900 flex items-center gap-2">
              <ShieldAlert className="w-4 h-4 text-rose-500" />
              Cảnh báo Bất thường & Khuyến nghị AI
            </h2>
            <Link to="/ai-analytics/anomalies" className="text-xs text-blue-600 hover:underline">
              Chi tiết
            </Link>
          </div>

          <div className="space-y-3">
            {dashboard.activeCriticalAnomalies.length === 0 && dashboard.highPriorityRecommendations.length === 0 ? (
              <div className="text-center py-8 text-slate-400 text-sm">
                Không phát hiện dấu hiệu bất thường nghiêm trọng
              </div>
            ) : (
              <>
                {dashboard.activeCriticalAnomalies.map((anomaly) => (
                  <div
                    key={anomaly.id}
                    className="p-3 bg-rose-50 border border-rose-200 rounded-lg flex items-start gap-2.5"
                  >
                    <AlertOctagon className="w-4 h-4 text-rose-600 shrink-0 mt-0.5" />
                    <div className="text-xs">
                      <div className="font-semibold text-rose-900">{anomaly.metricName}</div>
                      <div className="text-rose-700 mt-0.5">{anomaly.description}</div>
                    </div>
                  </div>
                ))}

                {dashboard.highPriorityRecommendations.map((rec) => (
                  <div
                    key={rec.id}
                    className="p-3 bg-indigo-50 border border-indigo-200 rounded-lg flex items-start gap-2.5"
                  >
                    <Sparkles className="w-4 h-4 text-indigo-600 shrink-0 mt-0.5" />
                    <div className="text-xs">
                      <div className="font-semibold text-indigo-900">{rec.title}</div>
                      <div className="text-indigo-700 mt-0.5">{rec.description}</div>
                    </div>
                  </div>
                ))}
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
