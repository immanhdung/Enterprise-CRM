import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  Users,
  Briefcase,
  CheckCircle2,
  Clock,
  Calendar,
  Sparkles,
  RefreshCw,
  ArrowUpRight,
  ChevronRight,
  TrendingUp,
} from 'lucide-react';
import { aiService } from '@/services/aiService';

export const SalesRoleDashboardPage: React.FC = () => {
  const { data: dashboard, isLoading, error, refetch } = useQuery({
    queryKey: ['salesDashboard'],
    queryFn: () => aiService.getSalesDashboard(),
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
        <h3 className="font-semibold text-lg">Không thể tải dữ liệu Sales Dashboard</h3>
        <p className="text-sm mt-1">Vui lòng kiểm tra lại kết nối mạng hoặc liên hệ quản trị viên.</p>
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
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-50 text-blue-700 border border-blue-200">
              Cá nhân
            </span>
            <h1 className="text-2xl font-bold text-slate-900">Bảng điều khiển Sales Cá nhân</h1>
          </div>
          <p className="text-sm text-slate-500 mt-1">
            Chào mừng trở lại, <span className="font-semibold text-slate-800">{dashboard.employeeName}</span>! Theo dõi tiến độ doanh số và nhiệm vụ hôm nay.
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
            to="/ai-assistant"
            className="flex items-center gap-1.5 px-3.5 py-2 text-sm font-medium text-purple-700 bg-purple-50 hover:bg-purple-100 rounded-lg border border-purple-200 transition"
          >
            <Sparkles className="w-4 h-4 text-purple-600" />
            Hỏi AI Copilot
          </Link>
        </div>
      </div>

      {/* Target Progress Bar */}
      <div className="bg-linear-to-r from-blue-600 to-indigo-700 text-white p-6 rounded-2xl shadow-sm">
        <div className="flex flex-col md:flex-row justify-between md:items-center gap-4">
          <div>
            <span className="text-xs font-medium uppercase tracking-wider text-blue-200">Mục tiêu Doanh số Tháng</span>
            <div className="text-3xl font-extrabold mt-1">
              {formatVND(dashboard.achievedRevenue)} / {formatVND(dashboard.targetRevenue)}
            </div>
            <p className="text-xs text-blue-100 mt-1">
              Đạt {dashboard.revenueAttainmentPercent.toFixed(1)}% chỉ tiêu KPI được giao
            </p>
          </div>
          <div className="bg-white/10 backdrop-blur-md px-5 py-3 rounded-xl border border-white/20">
            <div className="text-xs text-blue-100">Hoa hồng ước tính</div>
            <div className="text-2xl font-bold mt-0.5">{formatVND(dashboard.estimatedCommission)}</div>
          </div>
        </div>

        <div className="mt-4">
          <div className="w-full bg-white/20 rounded-full h-3 overflow-hidden">
            <div
              className="bg-emerald-400 h-3 rounded-full transition-all duration-500"
              style={{ width: `${Math.min(100, Math.max(0, dashboard.revenueAttainmentPercent))}%` }}
            />
          </div>
        </div>
      </div>

      {/* 4 Metric Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase">Leads Phụ Trách</span>
            <div className="p-2 bg-blue-50 text-blue-600 rounded-lg">
              <Users className="w-4 h-4" />
            </div>
          </div>
          <div className="text-2xl font-bold text-slate-900 mt-2">{dashboard.assignedLeadsCount}</div>
          <Link to="/leads" className="text-xs text-blue-600 hover:underline flex items-center gap-1 mt-2">
            Xem danh sách leads <ChevronRight className="w-3 h-3" />
          </Link>
        </div>

        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase">Cơ Hội (Deals) Đang Mở</span>
            <div className="p-2 bg-amber-50 text-amber-600 rounded-lg">
              <Briefcase className="w-4 h-4" />
            </div>
          </div>
          <div className="text-2xl font-bold text-slate-900 mt-2">{dashboard.activeDealsCount}</div>
          <div className="text-xs text-slate-500 mt-1">Pipeline: {formatVND(dashboard.pipelineValue)}</div>
        </div>

        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase">Deals Đã Chốt (Won)</span>
            <div className="p-2 bg-emerald-50 text-emerald-600 rounded-lg">
              <CheckCircle2 className="w-4 h-4" />
            </div>
          </div>
          <div className="text-2xl font-bold text-emerald-600 mt-2">{dashboard.wonDealsCount}</div>
          <div className="text-xs text-slate-500 mt-1">Góp phần vào hoa hồng</div>
        </div>

        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase">Lịch hẹn & Nhiệm vụ</span>
            <div className="p-2 bg-purple-50 text-purple-600 rounded-lg">
              <Calendar className="w-4 h-4" />
            </div>
          </div>
          <div className="text-2xl font-bold text-slate-900 mt-2">
            {dashboard.todayAppointmentsCount}{' '}
            <span className="text-sm font-normal text-slate-500">lịch hôm nay</span>
          </div>
          <div className="text-xs text-slate-500 mt-1">{dashboard.pendingTasksCount} nhiệm vụ đang chờ</div>
        </div>
      </div>

      {/* Two Column Layout: Key Deals & Urgent Tasks */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Key Deals */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs p-5">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-semibold text-slate-900 flex items-center gap-2">
              <TrendingUp className="w-4 h-4 text-blue-600" />
              Cơ hội trọng điểm sắp chốt
            </h2>
            <Link to="/pipeline" className="text-xs text-blue-600 hover:underline">
              Xem Kanban
            </Link>
          </div>

          {dashboard.keyDeals.length === 0 ? (
            <div className="text-center py-8 text-slate-400 text-sm">Chưa có cơ hội nào đang mở</div>
          ) : (
            <div className="divide-y divide-slate-100">
              {dashboard.keyDeals.map((deal) => (
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
                      <div className="text-xs text-slate-500">Hạn: {deal.expectedCloseDate}</div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Urgent Tasks */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs p-5">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-semibold text-slate-900 flex items-center gap-2">
              <Clock className="w-4 h-4 text-amber-500" />
              Nhiệm vụ cần ưu tiên xử lý
            </h2>
            <Link to="/tasks" className="text-xs text-blue-600 hover:underline">
              Tất cả nhiệm vụ
            </Link>
          </div>

          {dashboard.urgentTasks.length === 0 ? (
            <div className="text-center py-8 text-slate-400 text-sm">Không có nhiệm vụ khẩn cấp nào</div>
          ) : (
            <div className="divide-y divide-slate-100">
              {dashboard.urgentTasks.map((task) => (
                <div key={task.id} className="py-3 flex items-center justify-between">
                  <div className="flex items-start gap-2.5">
                    <span
                      className={`px-2 py-0.5 rounded text-[10px] font-bold mt-0.5 uppercase ${
                        task.priority === 'High' || task.priority === 'Urgent'
                          ? 'bg-rose-100 text-rose-700'
                          : 'bg-amber-100 text-amber-800'
                      }`}
                    >
                      {task.priority}
                    </span>
                    <div>
                      <div className="text-sm font-medium text-slate-800">{task.title}</div>
                      <div className="text-xs text-slate-500 mt-0.5">
                        Trạng thái: {task.status} {task.dueDate && `• Hạn chót: ${task.dueDate}`}
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
