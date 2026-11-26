import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  Trophy,
  Calendar,
  Medal,
  TrendingUp,
  Search,
} from 'lucide-react';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';

export const EmployeePerformancePage: React.FC = () => {
  const now = new Date();
  const [selectedMonth, setSelectedMonth] = useState<number>(now.getMonth() + 1);
  const [selectedYear, setSelectedYear] = useState<number>(now.getFullYear());
  const [searchTerm, setSearchTerm] = useState('');
  const [departmentFilter] = useState('');

  const { data: rankings, isLoading } = useQuery({
    queryKey: ['performance', 'rankings', selectedMonth, selectedYear, departmentFilter],
    queryFn: () =>
      performanceService.getEmployeeRankings({
        month: selectedMonth,
        year: selectedYear,
        departmentId: departmentFilter || undefined,
      }),
  });

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const filteredRankings = (rankings || []).filter((item) => {
    if (!searchTerm) return true;
    const term = searchTerm.toLowerCase();
    return (
      item.employeeName.toLowerCase().includes(term) ||
      item.employeeCode.toLowerCase().includes(term) ||
      (item.departmentName && item.departmentName.toLowerCase().includes(term))
    );
  });

  const top1 = rankings && rankings.length > 0 ? rankings[0] : null;
  const top2 = rankings && rankings.length > 1 ? rankings[1] : null;
  const top3 = rankings && rankings.length > 2 ? rankings[2] : null;

  const getTierBadge = (score: number) => {
    if (score >= 120) {
      return (
        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold bg-purple-100 text-purple-800 border border-purple-200">
          🌟 Vượt chỉ tiêu (&ge;120%)
        </span>
      );
    }
    if (score >= 100) {
      return (
        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold bg-emerald-100 text-emerald-800 border border-emerald-200">
          ✅ Đạt xuất sắc (100-119%)
        </span>
      );
    }
    if (score >= 70) {
      return (
        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold bg-blue-100 text-blue-800 border border-blue-200">
          ⚖️ Đạt yêu cầu (70-99%)
        </span>
      );
    }
    return (
      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold bg-rose-100 text-rose-800 border border-rose-200">
        ⚠️ Cần cải thiện (&lt;70%)
      </span>
    );
  };

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <PerformanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <div className="flex items-center gap-2">
              <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-amber-100 text-amber-800">
                Bảng vàng thành tích
              </span>
              <span className="text-xs text-slate-500">Module B – Sales Performance Analytics</span>
            </div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight mt-1 flex items-center gap-2">
              <Trophy className="w-7 h-7 text-amber-500" />
              <span>Bảng Xếp Hạng Doanh Số & Hiệu Suất</span>
            </h1>
            <p className="text-sm text-slate-500 mt-1">
              Vinh danh Top Performer, đánh giá tỷ lệ chốt deal, tiền mặt thực thu và điểm KPI toàn diện.
            </p>
          </div>

          <div className="flex items-center gap-2">
            <Link
              to="/performance/dashboard"
              className="inline-flex items-center gap-1.5 px-3.5 py-2 text-xs font-semibold text-slate-700 bg-white border border-slate-200 rounded-lg hover:bg-slate-50 shadow-xs"
            >
              <TrendingUp className="w-4 h-4 text-blue-600" />
              <span>Xem Dashboard Doanh Số</span>
            </Link>
          </div>
        </div>

        {/* Filters bar */}
        <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex flex-wrap items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-2 text-slate-600 font-medium text-sm">
              <Calendar className="w-4 h-4 text-slate-400" />
              <span>Kỳ thống kê:</span>
            </div>
            <select
              value={selectedMonth}
              onChange={(e) => setSelectedMonth(Number(e.target.value))}
              className="px-3 py-1.5 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
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
              className="px-3 py-1.5 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              {[2024, 2025, 2026, 2027].map((y) => (
                <option key={y} value={y}>
                  Năm {y}
                </option>
              ))}
            </select>
          </div>

          <div className="flex items-center gap-3">
            <div className="relative">
              <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
              <input
                type="text"
                placeholder="Tìm nhân sự, mã số..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-9 pr-3 py-1.5 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500 w-48 sm:w-64"
              />
            </div>
          </div>
        </div>

        {/* Podium Display: Top 1, Top 2, Top 3 */}
        {rankings && rankings.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 pt-4">
            {/* Top 2 */}
            <div className="order-2 md:order-1 bg-white rounded-2xl border border-slate-200 p-6 shadow-xs flex flex-col items-center text-center relative overflow-hidden">
              <div className="absolute top-0 inset-x-0 h-2 bg-slate-400" />
              <div className="w-16 h-16 rounded-full bg-slate-100 border-2 border-slate-300 flex items-center justify-center text-2xl font-bold text-slate-600 mb-3 shadow-inner">
                🥈 #2
              </div>
              <h3 className="text-base font-bold text-slate-900">{top2?.employeeName || 'Đang cập nhật'}</h3>
              <p className="text-xs text-slate-400 mb-4">{top2?.departmentName || 'Kinh doanh'}</p>

              <div className="w-full space-y-2.5 text-xs bg-slate-50 p-3.5 rounded-xl border border-slate-100">
                <div className="flex justify-between">
                  <span className="text-slate-500">Doanh số ký:</span>
                  <span className="font-bold text-slate-800">{formatVnd(top2?.contractRevenue)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-500">Tiền mặt thực thu:</span>
                  <span className="font-semibold text-emerald-600">{formatVnd(top2?.collectedCash)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-500">Deals thắng:</span>
                  <span className="font-bold text-blue-600">{top2?.dealsWon || 0} deals</span>
                </div>
                <div className="flex justify-between pt-1 border-t border-slate-200">
                  <span className="text-slate-500 font-medium">Điểm KPI:</span>
                  <span className="font-bold text-slate-900">{top2?.kpiScore?.toFixed(1) || 0}%</span>
                </div>
              </div>
            </div>

            {/* Top 1 - Champion */}
            <div className="order-1 md:order-2 bg-gradient-to-b from-amber-50 to-white rounded-2xl border-2 border-amber-300 p-6 shadow-md flex flex-col items-center text-center relative overflow-hidden transform md:-translate-y-3">
              <div className="absolute top-0 inset-x-0 h-3 bg-amber-400" />
              <div className="w-20 h-20 rounded-full bg-amber-100 border-4 border-amber-400 flex items-center justify-center text-3xl font-extrabold text-amber-600 mb-3 shadow-md animate-pulse">
                🥇 #1
              </div>
              <span className="px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-amber-200 text-amber-900 uppercase tracking-wider mb-1">
                Top Performer
              </span>
              <h3 className="text-lg font-extrabold text-slate-900">{top1?.employeeName || 'Đang cập nhật'}</h3>
              <p className="text-xs text-slate-500 mb-4">{top1?.departmentName || 'Kinh doanh'}</p>

              <div className="w-full space-y-2.5 text-xs bg-amber-50/70 p-4 rounded-xl border border-amber-200/60">
                <div className="flex justify-between">
                  <span className="text-slate-600">Doanh số ký:</span>
                  <span className="font-bold text-slate-900 text-sm">{formatVnd(top1?.contractRevenue)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-600">Tiền mặt thực thu:</span>
                  <span className="font-bold text-emerald-700">{formatVnd(top1?.collectedCash)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-600">Deals thắng:</span>
                  <span className="font-bold text-blue-700">{top1?.dealsWon || 0} deals</span>
                </div>
                <div className="flex justify-between pt-1 border-t border-amber-200">
                  <span className="text-slate-700 font-semibold">Điểm KPI tổng hợp:</span>
                  <span className="font-extrabold text-amber-700 text-sm">{top1?.kpiScore?.toFixed(1) || 0}%</span>
                </div>
              </div>
            </div>

            {/* Top 3 */}
            <div className="order-3 bg-white rounded-2xl border border-slate-200 p-6 shadow-xs flex flex-col items-center text-center relative overflow-hidden">
              <div className="absolute top-0 inset-x-0 h-2 bg-amber-600/70" />
              <div className="w-16 h-16 rounded-full bg-amber-50 border-2 border-amber-600/50 flex items-center justify-center text-2xl font-bold text-amber-700 mb-3 shadow-inner">
                🥉 #3
              </div>
              <h3 className="text-base font-bold text-slate-900">{top3?.employeeName || 'Đang cập nhật'}</h3>
              <p className="text-xs text-slate-400 mb-4">{top3?.departmentName || 'Kinh doanh'}</p>

              <div className="w-full space-y-2.5 text-xs bg-slate-50 p-3.5 rounded-xl border border-slate-100">
                <div className="flex justify-between">
                  <span className="text-slate-500">Doanh số ký:</span>
                  <span className="font-bold text-slate-800">{formatVnd(top3?.contractRevenue)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-500">Tiền mặt thực thu:</span>
                  <span className="font-semibold text-emerald-600">{formatVnd(top3?.collectedCash)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-500">Deals thắng:</span>
                  <span className="font-bold text-blue-600">{top3?.dealsWon || 0} deals</span>
                </div>
                <div className="flex justify-between pt-1 border-t border-slate-200">
                  <span className="text-slate-500 font-medium">Điểm KPI:</span>
                  <span className="font-bold text-slate-900">{top3?.kpiScore?.toFixed(1) || 0}%</span>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Full Rankings Table */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
          <div className="p-4 border-b border-slate-100 flex items-center justify-between">
            <h3 className="text-sm font-semibold text-slate-900 flex items-center gap-2">
              <Medal className="w-4 h-4 text-blue-600" />
              <span>Bảng chi tiết toàn bộ nhân sự kinh doanh</span>
            </h3>
            <span className="text-xs text-slate-400">
              Hiển thị {filteredRankings.length} nhân sự
            </span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-600">
              <thead className="bg-slate-50 text-xs font-semibold text-slate-500 uppercase tracking-wider border-b border-slate-200">
                <tr>
                  <th className="px-5 py-3.5 w-16 text-center">Hạng</th>
                  <th className="px-5 py-3.5">Nhân viên</th>
                  <th className="px-5 py-3.5">Phòng ban</th>
                  <th className="px-5 py-3.5 text-right">Doanh số ký kết</th>
                  <th className="px-5 py-3.5 text-right">Tiền mặt thực thu</th>
                  <th className="px-5 py-3.5 text-center">Deals thắng</th>
                  <th className="px-5 py-3.5 text-center">Tỷ lệ chốt</th>
                  <th className="px-5 py-3.5 text-center">Điểm KPI</th>
                  <th className="px-5 py-3.5 text-center">Phân loại</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {isLoading ? (
                  <tr>
                    <td colSpan={9} className="px-5 py-10 text-center text-slate-400">
                      Đang tổng hợp dữ liệu bảng xếp hạng...
                    </td>
                  </tr>
                ) : filteredRankings.length === 0 ? (
                  <tr>
                    <td colSpan={9} className="px-5 py-10 text-center text-slate-400">
                      Không tìm thấy dữ liệu xếp hạng trong kỳ này.
                    </td>
                  </tr>
                ) : (
                  filteredRankings.map((item) => (
                    <tr
                      key={item.employeeId}
                      className={`hover:bg-slate-50/80 transition-colors ${
                        item.rank === 1
                          ? 'bg-amber-50/30 font-medium'
                          : item.rank === 2
                          ? 'bg-slate-50/50'
                          : ''
                      }`}
                    >
                      <td className="px-5 py-3.5 text-center font-bold">
                        {item.rank === 1 ? (
                          <span className="text-lg">🥇</span>
                        ) : item.rank === 2 ? (
                          <span className="text-lg">🥈</span>
                        ) : item.rank === 3 ? (
                          <span className="text-lg">🥉</span>
                        ) : (
                          <span className="text-slate-400 text-xs">#{item.rank}</span>
                        )}
                      </td>
                      <td className="px-5 py-3.5 font-semibold text-slate-900">
                        <div className="flex items-center gap-2.5">
                          <div className="w-8 h-8 rounded-full bg-blue-100 text-blue-700 font-bold flex items-center justify-center text-xs">
                            {item.employeeName?.substring(0, 2).toUpperCase()}
                          </div>
                          <div>
                            <div>{item.employeeName}</div>
                            <div className="text-xs text-slate-400 font-normal">{item.employeeCode}</div>
                          </div>
                        </div>
                      </td>
                      <td className="px-5 py-3.5 text-xs text-slate-600">
                        {item.departmentName || 'Kinh doanh'}
                      </td>
                      <td className="px-5 py-3.5 text-right font-bold text-slate-900">
                        {formatVnd(item.contractRevenue)}
                      </td>
                      <td className="px-5 py-3.5 text-right font-semibold text-emerald-600">
                        {formatVnd(item.collectedCash)}
                      </td>
                      <td className="px-5 py-3.5 text-center font-bold text-blue-600">
                        {item.dealsWon}
                      </td>
                      <td className="px-5 py-3.5 text-center font-medium text-slate-800">
                        {item.winRate?.toFixed(1) || 0}%
                      </td>
                      <td className="px-5 py-3.5 text-center">
                        <span className="font-extrabold text-slate-900">
                          {item.kpiScore?.toFixed(1) || 0}%
                        </span>
                      </td>
                      <td className="px-5 py-3.5 text-center">
                        {getTierBadge(item.kpiScore || 0)}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
};
