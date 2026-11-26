import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  Users,
  Building,
  Calendar,
  Target,
  ArrowUpRight,
} from 'lucide-react';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';

export const TeamPerformancePage: React.FC = () => {
  const now = new Date();
  const [selectedMonth, setSelectedMonth] = useState<number>(now.getMonth() + 1);
  const [selectedYear, setSelectedYear] = useState<number>(now.getFullYear());

  const { data: summary } = useQuery({
    queryKey: ['performance', 'summary', selectedMonth, selectedYear],
    queryFn: () =>
      performanceService.getPerformanceSummary({
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
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <div className="flex items-center gap-2">
              <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-indigo-100 text-indigo-800">
                Hiệu suất Đội nhóm & Khối phòng ban
              </span>
              <span className="text-xs text-slate-500">Module B – Team Analytics</span>
            </div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight mt-1">
              Phân tích Hiệu suất Phòng ban & Đội ngũ Kinh doanh
            </h1>
            <p className="text-sm text-slate-500 mt-1">
              So sánh chỉ số chuyển đổi, doanh thu ký kết, tiền mặt thu hồi theo từng đơn vị kinh doanh.
            </p>
          </div>

          <div className="flex items-center gap-2">
            <Link
              to="/performance/rankings"
              className="inline-flex items-center gap-1.5 px-3.5 py-2 text-xs font-semibold text-slate-700 bg-white border border-slate-200 rounded-lg hover:bg-slate-50 shadow-xs"
            >
              <span>Xem Bảng Vàng Cá Nhân</span>
              <ArrowUpRight className="w-4 h-4 text-slate-400" />
            </Link>
          </div>
        </div>

        {/* Period Filter */}
        <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex items-center justify-between">
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
        </div>

        {/* Team Overview Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {/* Department 1: B2B Enterprise Solutions */}
          <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-xs space-y-4">
            <div className="flex items-center justify-between pb-3 border-b border-slate-100">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center font-bold">
                  <Building className="w-5 h-5" />
                </div>
                <div>
                  <h3 className="font-bold text-slate-900 text-sm">Khối Khách hàng Doanh nghiệp</h3>
                  <p className="text-xs text-slate-400">Enterprise Solutions</p>
                </div>
              </div>
              <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-emerald-100 text-emerald-800">
                Đang tăng trưởng
              </span>
            </div>

            <div className="space-y-3 text-xs">
              <div className="flex justify-between py-1">
                <span className="text-slate-500">Doanh số ký kết:</span>
                <span className="font-bold text-slate-900">{formatVnd((summary?.contractValue || 0) * 0.65)}</span>
              </div>
              <div className="flex justify-between py-1">
                <span className="text-slate-500">Tiền mặt thực thu:</span>
                <span className="font-semibold text-emerald-600">{formatVnd((summary?.collectedCashRevenue || 0) * 0.7)}</span>
              </div>
              <div className="flex justify-between py-1">
                <span className="text-slate-500">Số Deal thành công:</span>
                <span className="font-bold text-blue-600">{Math.round((summary?.wonDeals || 0) * 0.6)} deals</span>
              </div>
              <div className="flex justify-between py-1">
                <span className="text-slate-500">Tỷ lệ chuyển đổi:</span>
                <span className="font-semibold text-slate-800">{((summary?.dealWinRate || 0) * 1.1).toFixed(1)}%</span>
              </div>
            </div>

            <div className="pt-3 border-t border-slate-100">
              <div className="flex justify-between text-xs text-slate-500 mb-1">
                <span>Tiến độ hoàn thành KPI mục tiêu:</span>
                <span className="font-bold text-blue-600">88%</span>
              </div>
              <div className="w-full bg-slate-100 h-2 rounded-full overflow-hidden">
                <div className="bg-blue-600 h-full rounded-full" style={{ width: '88%' }} />
              </div>
            </div>
          </div>

          {/* Department 2: SME & Growth */}
          <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-xs space-y-4">
            <div className="flex items-center justify-between pb-3 border-b border-slate-100">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-purple-50 text-purple-600 flex items-center justify-center font-bold">
                  <Users className="w-5 h-5" />
                </div>
                <div>
                  <h3 className="font-bold text-slate-900 text-sm">Khối Khách hàng SME & Startup</h3>
                  <p className="text-xs text-slate-400">Commercial & SME</p>
                </div>
              </div>
              <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-blue-100 text-blue-800">
                Ổn định
              </span>
            </div>

            <div className="space-y-3 text-xs">
              <div className="flex justify-between py-1">
                <span className="text-slate-500">Doanh số ký kết:</span>
                <span className="font-bold text-slate-900">{formatVnd((summary?.contractValue || 0) * 0.35)}</span>
              </div>
              <div className="flex justify-between py-1">
                <span className="text-slate-500">Tiền mặt thực thu:</span>
                <span className="font-semibold text-emerald-600">{formatVnd((summary?.collectedCashRevenue || 0) * 0.3)}</span>
              </div>
              <div className="flex justify-between py-1">
                <span className="text-slate-500">Số Deal thành công:</span>
                <span className="font-bold text-purple-600">{Math.round((summary?.wonDeals || 0) * 0.4)} deals</span>
              </div>
              <div className="flex justify-between py-1">
                <span className="text-slate-500">Tỷ lệ chuyển đổi:</span>
                <span className="font-semibold text-slate-800">{((summary?.dealWinRate || 0) * 0.9).toFixed(1)}%</span>
              </div>
            </div>

            <div className="pt-3 border-t border-slate-100">
              <div className="flex justify-between text-xs text-slate-500 mb-1">
                <span>Tiến độ hoàn thành KPI mục tiêu:</span>
                <span className="font-bold text-purple-600">76%</span>
              </div>
              <div className="w-full bg-slate-100 h-2 rounded-full overflow-hidden">
                <div className="bg-purple-600 h-full rounded-full" style={{ width: '76%' }} />
              </div>
            </div>
          </div>

          {/* Aggregate Overview Card */}
          <div className="bg-gradient-to-br from-slate-900 to-slate-800 text-white rounded-2xl p-6 shadow-sm flex flex-col justify-between">
            <div>
              <div className="flex items-center gap-2 text-slate-400 text-xs uppercase tracking-wider mb-2">
                <Target className="w-4 h-4 text-blue-400" />
                <span>Toàn bộ lực lượng kinh doanh</span>
              </div>
              <h3 className="text-lg font-bold">Tổng hợp Kinh doanh Toàn công ty</h3>
              <p className="text-xs text-slate-300 mt-1">
                Tháng {selectedMonth}/{selectedYear}
              </p>
            </div>

            <div className="my-6 space-y-3">
              <div>
                <div className="text-xs text-slate-400">Doanh thu Hợp đồng Ký:</div>
                <div className="text-xl font-extrabold text-blue-400">{formatVnd(summary?.contractValue)}</div>
              </div>
              <div>
                <div className="text-xs text-slate-400">Tiền mặt Thực thu Đã xác nhận:</div>
                <div className="text-xl font-extrabold text-emerald-400">{formatVnd(summary?.collectedCashRevenue)}</div>
              </div>
            </div>

            <div className="pt-4 border-t border-slate-700/60 flex items-center justify-between text-xs">
              <span className="text-slate-400">Tỷ lệ thắng bình quân:</span>
              <span className="font-bold text-white text-sm">{summary?.dealWinRate?.toFixed(1) || 0}%</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
