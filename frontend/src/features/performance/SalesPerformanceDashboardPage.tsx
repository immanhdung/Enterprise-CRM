import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  TrendingUp,
  DollarSign,
  Briefcase,
  CheckCircle2,
  Users,
  Trophy,
  ArrowUpRight,
  ArrowDownRight,
  Calendar,
  Layers,
  ChevronRight,
} from 'lucide-react';
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
} from 'recharts';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';
import type { PerformanceRankingItem } from '@/types/performance';

export const SalesPerformanceDashboardPage: React.FC = () => {
  const now = new Date();
  const [selectedMonth, setSelectedMonth] = useState<number>(now.getMonth() + 1);
  const [selectedYear, setSelectedYear] = useState<number>(now.getFullYear());
  const [timeframe, setTimeframe] = useState<'monthly' | 'quarterly'>('monthly');

  const { data: summary, isLoading: isSummaryLoading } = useQuery({
    queryKey: ['performance', 'summary', selectedMonth, selectedYear],
    queryFn: () =>
      performanceService.getPerformanceSummary({
        month: selectedMonth,
        year: selectedYear,
      }),
  });

  const { data: timeline, isLoading: isTimelineLoading } = useQuery({
    queryKey: ['performance', 'timeline', timeframe, selectedYear],
    queryFn: () =>
      performanceService.getTimelineAnalytics({
        timeframe,
        year: selectedYear,
      }),
  });

  const { data: rankings } = useQuery({
    queryKey: ['performance', 'rankings', selectedMonth, selectedYear],
    queryFn: () =>
      performanceService.getEmployeeRankings({
        month: selectedMonth,
        year: selectedYear,
      }),
  });

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <PerformanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Header & Filter Controls */}
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
          <div>
            <h1 className="text-xl font-bold text-slate-900 tracking-tight flex items-center gap-2">
              <TrendingUp className="w-5 h-5 text-blue-600" />
              <span>Sales Performance & Revenue Execution</span>
            </h1>
            <p className="text-xs text-slate-500 mt-1">
              Phân tích hiệu quả kinh doanh, tỷ lệ chốt Deal và thực thu tiền mặt theo tháng/quý.
            </p>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            <div className="flex items-center gap-1.5 bg-slate-100 p-1 rounded-lg border border-slate-200">
              <Calendar className="w-3.5 h-3.5 text-slate-500 ml-1.5" />
              <select
                value={selectedMonth}
                onChange={(e) => setSelectedMonth(Number(e.target.value))}
                className="bg-transparent text-xs font-semibold text-slate-700 focus:outline-none pr-2"
              >
                {Array.from({ length: 12 }, (_, i) => i + 1).map((m) => (
                  <option key={m} value={m}>
                    Tháng {m}
                  </option>
                ))}
              </select>
              <select
                value={selectedYear}
                onChange={(e) => setSelectedYear(Number(e.target.value))}
                className="bg-transparent text-xs font-semibold text-slate-700 focus:outline-none"
              >
                <option value={2025}>2025</option>
                <option value={2026}>2026</option>
                <option value={2027}>2027</option>
              </select>
            </div>

            <Link
              to="/performance/rankings"
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-amber-50 text-amber-700 border border-amber-200 hover:bg-amber-100 transition-colors"
            >
              <Trophy className="w-3.5 h-3.5" />
              <span>Bảng Vinh danh Sales</span>
            </Link>
          </div>
        </div>

        {/* 4 Core Financial Metrics (Strict Separation) */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-xs text-slate-500 font-medium">
              <span>Doanh thu Hợp đồng Ký mới</span>
              <DollarSign className="w-4 h-4 text-blue-600" />
            </div>
            <div className="text-xl font-bold text-slate-900 mt-2">
              {isSummaryLoading ? '...' : formatVnd(summary?.contractValue)}
            </div>
            <div className="flex items-center gap-1 text-[11px] font-semibold mt-1.5 text-emerald-600">
              {summary && summary.revenueGrowthPercent >= 0 ? (
                <>
                  <ArrowUpRight className="w-3.5 h-3.5" />
                  <span>+{summary.revenueGrowthPercent}% MoM</span>
                </>
              ) : (
                <>
                  <ArrowDownRight className="w-3.5 h-3.5 text-rose-500" />
                  <span className="text-rose-500">{summary?.revenueGrowthPercent}% MoM</span>
                </>
              )}
            </div>
          </div>

          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-xs text-slate-500 font-medium">
              <span>Tiền mặt Thực thu (Confirmed Cash)</span>
              <CheckCircle2 className="w-4 h-4 text-emerald-600" />
            </div>
            <div className="text-xl font-bold text-emerald-600 mt-2">
              {isSummaryLoading ? '...' : formatVnd(summary?.collectedCashRevenue)}
            </div>
            <div className="text-[11px] text-slate-500 mt-1.5">
              Xuất hóa đơn: {formatVnd(summary?.invoicedAmount)}
            </div>
          </div>

          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-xs text-slate-500 font-medium">
              <span>Số Deal Thành công (Won)</span>
              <Briefcase className="w-4 h-4 text-indigo-600" />
            </div>
            <div className="text-xl font-bold text-slate-900 mt-2">
              {isSummaryLoading ? '...' : `${summary?.wonDeals || 0} Deals`}
            </div>
            <div className="text-[11px] text-slate-500 mt-1.5">
              Win Rate: <span className="font-semibold text-indigo-600">{summary?.dealWinRate || 0}%</span> (Giá trị TB: {formatVnd(summary?.averageDealValue)})
            </div>
          </div>

          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-xs text-slate-500 font-medium">
              <span>Khách hàng Mới vs Cũ</span>
              <Users className="w-4 h-4 text-amber-600" />
            </div>
            <div className="text-xl font-bold text-slate-900 mt-2">
              {isSummaryLoading ? '...' : formatVnd(summary?.newCustomerRevenue)}
            </div>
            <div className="text-[11px] text-slate-500 mt-1.5">
              Tái ký / KH Cũ: {formatVnd(summary?.existingCustomerRevenue)}
            </div>
          </div>
        </div>

        {/* Operational Funnel Metrics */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <div className="bg-white p-3.5 rounded-lg border border-slate-200 text-center">
            <div className="text-xs text-slate-500">Tổng Lead tiếp nhận</div>
            <div className="text-lg font-bold text-slate-800 mt-1">{summary?.totalAssignedLeads || 0}</div>
          </div>
          <div className="bg-white p-3.5 rounded-lg border border-slate-200 text-center">
            <div className="text-xs text-slate-500">Lead đủ điều kiện (Qualified)</div>
            <div className="text-lg font-bold text-blue-600 mt-1">{summary?.qualifiedLeads || 0}</div>
          </div>
          <div className="bg-white p-3.5 rounded-lg border border-slate-200 text-center">
            <div className="text-xs text-slate-500">Lead chuyển đổi (Converted)</div>
            <div className="text-lg font-bold text-emerald-600 mt-1">{summary?.convertedLeads || 0}</div>
          </div>
          <div className="bg-white p-3.5 rounded-lg border border-slate-200 text-center">
            <div className="text-xs text-slate-500">Tỷ lệ chuyển đổi Lead</div>
            <div className="text-lg font-bold text-amber-600 mt-1">{summary?.leadConversionRate || 0}%</div>
          </div>
        </div>

        {/* Chart: Timeline Comparison */}
        <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-xs space-y-4">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
            <div>
              <h2 className="text-base font-bold text-slate-900 flex items-center gap-2">
                <Layers className="w-4 h-4 text-blue-600" />
                <span>Tiến độ Doanh số: Hợp đồng vs Hóa đơn vs Thực thu</span>
              </h2>
              <p className="text-xs text-slate-500 mt-0.5">
                So sánh chênh lệch giữa giá trị cam kết, xuất hóa đơn và tiền mặt thu về năm {selectedYear}.
              </p>
            </div>
            <div className="flex items-center gap-1 bg-slate-100 p-1 rounded-lg border border-slate-200 text-xs">
              <button
                type="button"
                onClick={() => setTimeframe('monthly')}
                className={`px-3 py-1 rounded-md font-semibold transition-colors ${
                  timeframe === 'monthly' ? 'bg-white text-blue-700 shadow-xs' : 'text-slate-600 hover:text-slate-900'
                }`}
              >
                12 Tháng
              </button>
              <button
                type="button"
                onClick={() => setTimeframe('quarterly')}
                className={`px-3 py-1 rounded-md font-semibold transition-colors ${
                  timeframe === 'quarterly' ? 'bg-white text-blue-700 shadow-xs' : 'text-slate-600 hover:text-slate-900'
                }`}
              >
                4 Quý
              </button>
            </div>
          </div>

          <div className="h-80 w-full pt-4">
            {isTimelineLoading ? (
              <div className="h-full flex items-center justify-center text-xs text-slate-400">
                Đang tải dữ liệu biểu đồ...
              </div>
            ) : timeline && timeline.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={timeline} margin={{ top: 10, right: 10, left: 20, bottom: 20 }}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E2E8F0" />
                  <XAxis dataKey="periodLabel" tick={{ fontSize: 11, fill: '#64748B' }} />
                  <YAxis
                    tick={{ fontSize: 11, fill: '#64748B' }}
                    tickFormatter={(v) => `${(v / 1_000_000).toFixed(0)}Tr`}
                  />
                  <Tooltip
                    formatter={(val: any) => [formatVnd(Number(val)), '']}
                    contentStyle={{ borderRadius: 8, fontSize: 12, border: '1px solid #CBD5E1' }}
                  />
                  <Legend wrapperStyle={{ fontSize: 12, paddingTop: 10 }} />
                  <Bar dataKey="contractValue" name="Hợp đồng ký kết" fill="#3B82F6" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="invoicedAmount" name="Hóa đơn xuất" fill="#F59E0B" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="collectedCash" name="Thực thu tiền mặt" fill="#10B981" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-full flex items-center justify-center text-xs text-slate-400">
                Không có dữ liệu cho mốc thời gian này.
              </div>
            )}
          </div>
        </div>

        {/* Top Performer Preview Table */}
        <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-xs space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-base font-bold text-slate-900 flex items-center gap-2">
              <Trophy className="w-4 h-4 text-amber-500" />
              <span>Top Nhân viên Xuất sắc Tháng {selectedMonth}/{selectedYear}</span>
            </h2>
            <Link
              to="/performance/rankings"
              className="text-xs font-semibold text-blue-600 hover:text-blue-800 flex items-center gap-1"
            >
              <span>Xem toàn bộ bảng xếp hạng</span>
              <ChevronRight className="w-3.5 h-3.5" />
            </Link>
          </div>

          <div className="overflow-x-auto border border-slate-200 rounded-lg">
            <table className="w-full text-left text-xs text-slate-700">
              <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase">
                <tr>
                  <th className="py-3 px-4">Hạng</th>
                  <th className="py-3 px-4">Nhân viên</th>
                  <th className="py-3 px-4">Phòng ban / Đội</th>
                  <th className="py-3 px-4 text-right">Deal Won</th>
                  <th className="py-3 px-4 text-right">Doanh số Hợp đồng</th>
                  <th className="py-3 px-4 text-right">Thực thu tiền mặt</th>
                  <th className="py-3 px-4 text-right">Đạt KPI</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {rankings && rankings.slice(0, 5).map((r: PerformanceRankingItem) => (
                  <tr key={r.employeeId} className="hover:bg-slate-50 transition-colors">
                    <td className="py-3 px-4 font-bold text-slate-900">
                      {r.rank === 1 && <span className="text-amber-500 font-black">🥇 #1</span>}
                      {r.rank === 2 && <span className="text-slate-400 font-black">🥈 #2</span>}
                      {r.rank === 3 && <span className="text-amber-700 font-black">🥉 #3</span>}
                      {r.rank > 3 && `#${r.rank}`}
                    </td>
                    <td className="py-3 px-4 font-semibold text-slate-800">
                      {r.employeeName} ({r.employeeCode})
                    </td>
                    <td className="py-3 px-4 text-slate-500">
                      {r.departmentName || 'Sales'} / {r.teamName || 'Direct'}
                    </td>
                    <td className="py-3 px-4 text-right font-bold text-slate-900">{r.wonDeals}</td>
                    <td className="py-3 px-4 text-right font-bold text-blue-600">{formatVnd(r.contractValue)}</td>
                    <td className="py-3 px-4 text-right font-bold text-emerald-600">{formatVnd(r.collectedCash)}</td>
                    <td className="py-3 px-4 text-right font-black text-indigo-600">{r.kpiAchievementPercent}%</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
};
