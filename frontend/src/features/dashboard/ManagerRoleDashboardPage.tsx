import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  Users,
  Briefcase,
  Target,
  Trophy,
  AlertTriangle,
  RefreshCw,
  Sparkles,
  Layers,
} from 'lucide-react';
import { aiService } from '@/services/aiService';

export const ManagerRoleDashboardPage: React.FC = () => {
  const { data: dashboard, isLoading, error, refetch } = useQuery({
    queryKey: ['managerDashboard'],
    queryFn: () => aiService.getManagerDashboard(),
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
        <h3 className="font-semibold text-lg">Không thể tải dữ liệu Manager Dashboard</h3>
        <p className="text-sm mt-1">Vui lòng kiểm tra lại quyền hạn hoặc liên hệ quản trị viên.</p>
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
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-indigo-50 text-indigo-700 border border-indigo-200">
              Quản lý Phòng ban
            </span>
            <h1 className="text-2xl font-bold text-slate-900">Bảng điều khiển Quản lý Bán hàng</h1>
          </div>
          <p className="text-sm text-slate-500 mt-1">
            Phòng ban: <span className="font-semibold text-slate-800">{dashboard.departmentName}</span> • Quy mô:{' '}
            <span className="font-semibold text-slate-800">{dashboard.teamMembersCount} nhân sự</span>
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
            to="/automation/lead-distribution"
            className="flex items-center gap-1.5 px-3.5 py-2 text-sm font-medium text-blue-700 bg-blue-50 hover:bg-blue-100 rounded-lg border border-blue-200 transition"
          >
            <Layers className="w-4 h-4" />
            Điều phối Lead
          </Link>
          <Link
            to="/ai-analytics/overview"
            className="flex items-center gap-1.5 px-3.5 py-2 text-sm font-medium text-purple-700 bg-purple-50 hover:bg-purple-100 rounded-lg border border-purple-200 transition"
          >
            <Sparkles className="w-4 h-4" />
            Phân tích AI
          </Link>
        </div>
      </div>

      {/* Team Target & Pipeline Summary */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs md:col-span-2">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase">Mục tiêu Doanh số Đội ngũ</span>
            <div className="p-2 bg-indigo-50 text-indigo-600 rounded-lg">
              <Target className="w-4 h-4" />
            </div>
          </div>
          <div className="text-2xl font-bold text-slate-900 mt-2">
            {formatVND(dashboard.teamAchievedRevenue)} / {formatVND(dashboard.teamTargetRevenue)}
          </div>
          <div className="mt-3">
            <div className="flex justify-between text-xs text-slate-500 mb-1">
              <span>Tiến độ hoàn thành</span>
              <span className="font-semibold text-indigo-600">{dashboard.attainmentPercent.toFixed(1)}%</span>
            </div>
            <div className="w-full bg-slate-100 rounded-full h-2 overflow-hidden">
              <div
                className="bg-indigo-600 h-2 rounded-full transition-all"
                style={{ width: `${Math.min(100, Math.max(0, dashboard.attainmentPercent))}%` }}
              />
            </div>
          </div>
        </div>

        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase">Tổng Pipeline Đội ngũ</span>
            <div className="p-2 bg-blue-50 text-blue-600 rounded-lg">
              <Briefcase className="w-4 h-4" />
            </div>
          </div>
          <div className="text-xl font-bold text-slate-900 mt-2">{formatVND(dashboard.totalPipelineValue)}</div>
          <div className="text-xs text-emerald-600 font-medium mt-1">
            {dashboard.totalWonDealsCount} hợp đồng / deals đã chốt
          </div>
        </div>

        {/* Operational Bottlenecks Warning Card */}
        <div className="bg-amber-50/50 p-5 rounded-xl border border-amber-200 shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-amber-800 uppercase">Cảnh Báo Vận Hành</span>
            <div className="p-2 bg-amber-100 text-amber-700 rounded-lg">
              <AlertTriangle className="w-4 h-4" />
            </div>
          </div>
          <div className="mt-2 space-y-1.5">
            <div className="flex justify-between text-xs">
              <span className="text-slate-600">Leads chưa phân bổ:</span>
              <span className={`font-bold ${dashboard.unassignedLeadsCount > 0 ? 'text-rose-600' : 'text-slate-700'}`}>
                {dashboard.unassignedLeadsCount}
              </span>
            </div>
            <div className="flex justify-between text-xs">
              <span className="text-slate-600">Deals ứ đọng (&gt;14 ngày):</span>
              <span className={`font-bold ${dashboard.stagnantDealsCount > 0 ? 'text-amber-600' : 'text-slate-700'}`}>
                {dashboard.stagnantDealsCount}
              </span>
            </div>
          </div>
          <Link
            to="/automation/alerts"
            className="text-xs text-amber-800 font-semibold hover:underline block mt-2 text-right"
          >
            Mở Trung tâm Cảnh báo &rarr;
          </Link>
        </div>
      </div>

      {/* Team Workload & Capacity Real-time Monitoring */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-xs p-5">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h2 className="font-semibold text-slate-900 flex items-center gap-2">
              <Users className="w-4 h-4 text-blue-600" />
              Tải trọng & Dung lượng Phân bổ Nhân viên (Workload Monitoring)
            </h2>
            <p className="text-xs text-slate-500 mt-0.5">
              Tối đa 50 leads hoạt động / nhân viên theo chính sách phân phối tự động
            </p>
          </div>
          <Link to="/automation/lead-distribution" className="text-xs text-blue-600 hover:underline">
            Cấu hình phân bổ
          </Link>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {dashboard.teamWorkloads.map((rep) => (
            <div
              key={rep.employeeId}
              className={`p-4 rounded-xl border transition ${
                rep.capacityUtilizationPercent >= 90
                  ? 'border-rose-200 bg-rose-50/30'
                  : rep.capacityUtilizationPercent >= 70
                  ? 'border-amber-200 bg-amber-50/20'
                  : 'border-slate-200 bg-slate-50/30'
              }`}
            >
              <div className="flex items-start justify-between">
                <div>
                  <div className="font-semibold text-slate-900 text-sm">{rep.employeeName}</div>
                  <div className="text-xs text-slate-500">
                    {rep.employeeCode} {rep.teamName && `• ${rep.teamName}`}
                  </div>
                </div>
                <span
                  className={`px-2 py-0.5 rounded text-[10px] font-bold ${
                    rep.isAvailable
                      ? 'bg-emerald-100 text-emerald-800'
                      : 'bg-slate-200 text-slate-600'
                  }`}
                >
                  {rep.isAvailable ? 'Sẵn sàng' : 'Tạm dừng'}
                </span>
              </div>

              <div className="mt-3">
                <div className="flex justify-between text-xs text-slate-600 mb-1">
                  <span>Leads phụ trách:</span>
                  <span className="font-semibold">
                    {rep.activeLeadsCount} / {rep.maxCapacity} ({rep.capacityUtilizationPercent.toFixed(0)}%)
                  </span>
                </div>
                <div className="w-full bg-slate-200 rounded-full h-2 overflow-hidden">
                  <div
                    className={`h-2 rounded-full transition-all ${
                      rep.capacityUtilizationPercent >= 90
                        ? 'bg-rose-500'
                        : rep.capacityUtilizationPercent >= 70
                        ? 'bg-amber-500'
                        : 'bg-emerald-500'
                    }`}
                    style={{ width: `${Math.min(100, Math.max(0, rep.capacityUtilizationPercent))}%` }}
                  />
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Team Leaderboard */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-xs p-5">
        <div className="flex items-center justify-between mb-4">
          <h2 className="font-semibold text-slate-900 flex items-center gap-2">
            <Trophy className="w-4 h-4 text-amber-500" />
            Bảng Xếp Hạng Doanh Số & KPI Đội Ngũ
          </h2>
          <Link to="/performance/rankings" className="text-xs text-blue-600 hover:underline">
            Xem toàn bộ bảng xếp hạng
          </Link>
        </div>

        {dashboard.leaderboard.length === 0 ? (
          <div className="text-center py-8 text-slate-400 text-sm">Chưa có dữ liệu xếp hạng</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-600">
              <thead className="bg-slate-50 text-xs uppercase font-semibold text-slate-500">
                <tr>
                  <th className="py-2.5 px-4">#</th>
                  <th className="py-2.5 px-4">Nhân sự</th>
                  <th className="py-2.5 px-4">Đội nhóm</th>
                  <th className="py-2.5 px-4 text-right">Doanh thu đạt</th>
                  <th className="py-2.5 px-4 text-center">Deals Won</th>
                  <th className="py-2.5 px-4 text-right">Tỷ lệ KPI</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {dashboard.leaderboard.map((item, index) => (
                  <tr key={index} className="hover:bg-slate-50/50">
                    <td className="py-3 px-4 font-bold text-slate-800">
                      {index === 0 ? '🥇' : index === 1 ? '🥈' : index === 2 ? '🥉' : `${index + 1}`}
                    </td>
                    <td className="py-3 px-4 font-medium text-slate-900">{item.employeeName}</td>
                    <td className="py-3 px-4 text-slate-500">{item.teamName || '—'}</td>
                    <td className="py-3 px-4 text-right font-semibold text-slate-900">
                      {formatVND(item.achievedRevenue)}
                    </td>
                    <td className="py-3 px-4 text-center font-medium text-emerald-600">{item.wonDeals}</td>
                    <td className="py-3 px-4 text-right">
                      <span
                        className={`inline-block px-2 py-0.5 rounded text-xs font-semibold ${
                          item.kpiAchievementRate >= 100
                            ? 'bg-emerald-100 text-emerald-800'
                            : item.kpiAchievementRate >= 80
                            ? 'bg-blue-100 text-blue-800'
                            : 'bg-slate-100 text-slate-700'
                        }`}
                      >
                        {item.kpiAchievementRate.toFixed(1)}%
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};
