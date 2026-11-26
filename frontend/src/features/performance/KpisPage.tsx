import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  Target,
  Plus,
  Calendar,
  CheckCircle2,
  Clock,
  XCircle,
  Eye,
  Calculator,
  Layers,
} from 'lucide-react';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';
import type { CreateKpiDefinitionRequest } from '@/types/performance';

export const KpisPage: React.FC = () => {
  const queryClient = useQueryClient();
  const now = new Date();
  const [selectedMonth, setSelectedMonth] = useState<number>(now.getMonth() + 1);
  const [selectedYear, setSelectedYear] = useState<number>(now.getFullYear());
  const [selectedTab, setSelectedTab] = useState<'assignments' | 'definitions'>('assignments');
  const [statusFilter, setStatusFilter] = useState<string>('');

  // Modal State for New Definition
  const [isDefModalOpen, setIsDefModalOpen] = useState(false);
  const [defForm, setDefForm] = useState<CreateKpiDefinitionRequest>({
    code: '',
    name: '',
    category: 'Sales',
    metricType: 'Currency',
    unit: 'VND',
    defaultTarget: 100000000,
    description: '',
  });

  const { data: assignments, isLoading: isAssignmentsLoading } = useQuery({
    queryKey: ['kpis', 'assignments', selectedMonth, selectedYear, statusFilter],
    queryFn: () =>
      performanceService.getKpiAssignments({
        month: selectedMonth,
        year: selectedYear,
        status: statusFilter || undefined,
      }),
  });

  const { data: definitions, isLoading: isDefsLoading } = useQuery({
    queryKey: ['kpis', 'definitions'],
    queryFn: () => performanceService.getKpiDefinitions(),
  });

  const createDefMutation = useMutation({
    mutationFn: (req: CreateKpiDefinitionRequest) => performanceService.createKpiDefinition(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['kpis', 'definitions'] });
      setIsDefModalOpen(false);
      setDefForm({
        code: '',
        name: '',
        category: 'Sales',
        metricType: 'Currency',
        unit: 'VND',
        defaultTarget: 100000000,
        description: '',
      });
    },
  });

  const autoCalcMutation = useMutation({
    mutationFn: (id: string) => performanceService.autoCalculateKpiActuals(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['kpis', 'assignments'] });
    },
  });

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Approved':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-bold bg-emerald-100 text-emerald-800">
            <CheckCircle2 className="w-3 h-3" /> Đã duyệt
          </span>
        );
      case 'Evaluated':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-bold bg-blue-100 text-blue-800">
            <Calculator className="w-3 h-3" /> Đã đánh giá
          </span>
        );
      case 'PendingApproval':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-bold bg-amber-100 text-amber-800">
            <Clock className="w-3 h-3" /> Chờ duyệt
          </span>
        );
      case 'Rejected':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-bold bg-rose-100 text-rose-800">
            <XCircle className="w-3 h-3" /> Từ chối
          </span>
        );
      default:
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-bold bg-slate-100 text-slate-700">
            Bản nháp
          </span>
        );
    }
  };

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <PerformanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Header */}
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
          <div>
            <h1 className="text-xl font-bold text-slate-900 tracking-tight flex items-center gap-2">
              <Target className="w-5 h-5 text-blue-600" />
              <span>KPI Management & Goals Alignment</span>
            </h1>
            <p className="text-xs text-slate-500 mt-1">
              Thiết lập chỉ tiêu doanh số, số lượng cuộc gọi, cuộc gặp và đánh giá tỷ lệ đạt mục tiêu.
            </p>
          </div>

          <div className="flex items-center gap-3">
            <Link
              to="/performance/assignments"
              className="flex items-center gap-1.5 px-3 py-2 rounded-lg text-xs font-semibold bg-blue-600 text-white hover:bg-blue-700 transition-colors shadow-xs"
            >
              <Plus className="w-3.5 h-3.5" />
              <span>Giao Chỉ tiêu Mới</span>
            </Link>
            <button
              type="button"
              onClick={() => setIsDefModalOpen(true)}
              className="flex items-center gap-1.5 px-3 py-2 rounded-lg text-xs font-semibold bg-slate-100 text-slate-700 border border-slate-200 hover:bg-slate-200 transition-colors"
            >
              <Layers className="w-3.5 h-3.5" />
              <span>Thêm Định nghĩa KPI</span>
            </button>
          </div>
        </div>

        {/* Tab Selector & Filter Bar */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 bg-white p-3 rounded-xl border border-slate-200">
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => setSelectedTab('assignments')}
              className={`px-3 py-1.5 rounded-lg text-xs font-bold transition-colors ${
                selectedTab === 'assignments'
                  ? 'bg-blue-50 text-blue-700 border border-blue-200'
                  : 'text-slate-600 hover:text-slate-900'
              }`}
            >
              Bảng Giao & Đánh giá KPI
            </button>
            <button
              type="button"
              onClick={() => setSelectedTab('definitions')}
              className={`px-3 py-1.5 rounded-lg text-xs font-bold transition-colors ${
                selectedTab === 'definitions'
                  ? 'bg-blue-50 text-blue-700 border border-blue-200'
                  : 'text-slate-600 hover:text-slate-900'
              }`}
            >
              Danh mục Chỉ số KPI ({definitions?.length || 0})
            </button>
          </div>

          {selectedTab === 'assignments' && (
            <div className="flex items-center gap-2">
              <div className="flex items-center gap-1 bg-slate-50 p-1 rounded-lg border border-slate-200">
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

              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
                className="bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold px-2 py-1.5 text-slate-700"
              >
                <option value="">Tất cả trạng thái</option>
                <option value="Approved">Đã duyệt</option>
                <option value="Evaluated">Đã đánh giá</option>
                <option value="PendingApproval">Chờ duyệt</option>
                <option value="Draft">Bản nháp</option>
              </select>
            </div>
          )}
        </div>

        {/* Tab 1: Assignments Table */}
        {selectedTab === 'assignments' && (
          <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs text-slate-700">
                <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase">
                  <tr>
                    <th className="py-3 px-4">Nhân viên / Đối tượng</th>
                    <th className="py-3 px-4">Phạm vi</th>
                    <th className="py-3 px-4 text-right">Mục tiêu Doanh thu</th>
                    <th className="py-3 px-4 text-right">Thực tế Đạt</th>
                    <th className="py-3 px-4 text-right">Deal Won</th>
                    <th className="py-3 px-4 text-right">Cuộc gọi / Gặp</th>
                    <th className="py-3 px-4 text-right">Điểm Hoàn thành</th>
                    <th className="py-3 px-4 text-center">Trạng thái</th>
                    <th className="py-3 px-4 text-center">Thao tác</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {isAssignmentsLoading ? (
                    <tr>
                      <td colSpan={9} className="py-8 text-center text-slate-400">
                        Đang tải danh sách KPI...
                      </td>
                    </tr>
                  ) : assignments && assignments.length > 0 ? (
                    assignments.map((k) => (
                      <tr key={k.id} className="hover:bg-slate-50 transition-colors">
                        <td className="py-3 px-4 font-semibold text-slate-900">
                          <div>{k.employeeName}</div>
                          <div className="text-[11px] text-slate-400 font-normal">{k.employeeCode} • {k.departmentName}</div>
                        </td>
                        <td className="py-3 px-4">
                          <span className="px-2 py-0.5 rounded text-[11px] bg-slate-100 text-slate-700 font-medium">
                            {k.scope}
                          </span>
                        </td>
                        <td className="py-3 px-4 text-right font-bold text-slate-800">
                          {formatVnd(k.targetRevenue)}
                        </td>
                        <td className="py-3 px-4 text-right font-bold text-blue-600">
                          {formatVnd(k.achievedRevenue)}
                        </td>
                        <td className="py-3 px-4 text-right font-semibold">
                          {k.achievedDealsWon} / {k.targetDealsWon}
                        </td>
                        <td className="py-3 px-4 text-right text-slate-600">
                          {k.achievedCalls}/{k.targetCalls} gọi • {k.achievedMeetings}/{k.targetMeetings} gặp
                        </td>
                        <td className="py-3 px-4 text-right">
                          <span className={`font-black text-sm ${
                            k.achievementScore >= 100
                              ? 'text-emerald-600'
                              : k.achievementScore >= 70
                              ? 'text-blue-600'
                              : 'text-amber-600'
                          }`}>
                            {k.achievementScore}%
                          </span>
                        </td>
                        <td className="py-3 px-4 text-center">{getStatusBadge(k.status)}</td>
                        <td className="py-3 px-4 text-center">
                          <div className="flex items-center justify-center gap-2">
                            <button
                              type="button"
                              onClick={() => autoCalcMutation.mutate(k.id)}
                              title="Tự động tính thực tế từ dữ liệu CRM"
                              disabled={autoCalcMutation.isPending}
                              className="p-1 rounded text-slate-500 hover:text-blue-600 hover:bg-blue-50 transition-colors"
                            >
                              <Calculator className="w-4 h-4" />
                            </button>
                            <Link
                              to={`/performance/kpis/${k.id}`}
                              className="p-1 rounded text-slate-500 hover:text-blue-600 hover:bg-blue-50 transition-colors"
                              title="Xem chi tiết & Đánh giá"
                            >
                              <Eye className="w-4 h-4" />
                            </Link>
                          </div>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={9} className="py-8 text-center text-slate-400">
                        Chưa có KPI nào được giao trong Tháng {selectedMonth}/{selectedYear}.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* Tab 2: Definitions Table */}
        {selectedTab === 'definitions' && (
          <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs text-slate-700">
                <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase">
                  <tr>
                    <th className="py-3 px-4">Mã KPI</th>
                    <th className="py-3 px-4">Tên Chỉ số</th>
                    <th className="py-3 px-4">Phân loại</th>
                    <th className="py-3 px-4">Đơn vị tính</th>
                    <th className="py-3 px-4 text-right">Chỉ tiêu Mặc định</th>
                    <th className="py-3 px-4">Mô tả</th>
                    <th className="py-3 px-4 text-center">Trạng thái</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {isDefsLoading ? (
                    <tr>
                      <td colSpan={7} className="py-8 text-center text-slate-400">
                        Đang tải định nghĩa KPI...
                      </td>
                    </tr>
                  ) : definitions && definitions.length > 0 ? (
                    definitions.map((d) => (
                      <tr key={d.id} className="hover:bg-slate-50 transition-colors">
                        <td className="py-3 px-4 font-bold text-blue-700">{d.code}</td>
                        <td className="py-3 px-4 font-semibold text-slate-900">{d.name}</td>
                        <td className="py-3 px-4">
                          <span className={`px-2 py-0.5 rounded text-[11px] font-semibold ${
                            d.category === 'Sales'
                              ? 'bg-emerald-50 text-emerald-700'
                              : d.category === 'Activity'
                              ? 'bg-blue-50 text-blue-700'
                              : 'bg-purple-50 text-purple-700'
                          }`}>
                            {d.category}
                          </span>
                        </td>
                        <td className="py-3 px-4 text-slate-600">{d.unit}</td>
                        <td className="py-3 px-4 text-right font-bold text-slate-800">
                          {d.unit === 'VND' ? formatVnd(d.defaultTarget) : `${d.defaultTarget} ${d.unit}`}
                        </td>
                        <td className="py-3 px-4 text-slate-500 max-w-xs truncate">{d.description || '—'}</td>
                        <td className="py-3 px-4 text-center">
                          {d.isActive ? (
                            <span className="px-2 py-0.5 rounded-full text-[11px] font-bold bg-emerald-100 text-emerald-700">
                              Hoạt động
                            </span>
                          ) : (
                            <span className="px-2 py-0.5 rounded-full text-[11px] font-bold bg-slate-100 text-slate-500">
                              Đã khóa
                            </span>
                          )}
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={7} className="py-8 text-center text-slate-400">
                        Chưa có định nghĩa KPI nào.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>

      {/* Modal: New KPI Definition */}
      {isDefModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-xs p-4">
          <div className="bg-white rounded-xl shadow-xl border border-slate-200 max-w-md w-full p-6 space-y-4">
            <div className="flex items-center justify-between border-b border-slate-100 pb-3">
              <h3 className="text-base font-bold text-slate-900 flex items-center gap-2">
                <Target className="w-4 h-4 text-blue-600" />
                <span>Thêm Định nghĩa KPI Mới</span>
              </h3>
              <button
                type="button"
                onClick={() => setIsDefModalOpen(false)}
                className="text-slate-400 hover:text-slate-600"
              >
                ✕
              </button>
            </div>

            <div className="space-y-3 text-xs">
              <div>
                <label className="block text-slate-600 font-semibold mb-1">Mã chỉ số (Code)*</label>
                <input
                  type="text"
                  value={defForm.code}
                  onChange={(e) => setDefForm({ ...defForm, code: e.target.value })}
                  placeholder="Vd: REVENUE_TARGET, CALLS_DAILY"
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-xs focus:outline-blue-500"
                />
              </div>

              <div>
                <label className="block text-slate-600 font-semibold mb-1">Tên chỉ số KPI*</label>
                <input
                  type="text"
                  value={defForm.name}
                  onChange={(e) => setDefForm({ ...defForm, name: e.target.value })}
                  placeholder="Vd: Doanh số Hợp đồng Ký mới"
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-xs focus:outline-blue-500"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-slate-600 font-semibold mb-1">Phân loại*</label>
                  <select
                    value={defForm.category}
                    onChange={(e) => setDefForm({ ...defForm, category: e.target.value })}
                    className="w-full border border-slate-300 rounded-lg px-3 py-2 text-xs focus:outline-blue-500 bg-white"
                  >
                    <option value="Sales">Sales (Doanh số / Deal)</option>
                    <option value="Activity">Activity (Cuộc gọi / Gặp gỡ)</option>
                    <option value="Conversion">Conversion (Tỷ lệ chuyển đổi)</option>
                  </select>
                </div>
                <div>
                  <label className="block text-slate-600 font-semibold mb-1">Đơn vị tính*</label>
                  <input
                    type="text"
                    value={defForm.unit}
                    onChange={(e) => setDefForm({ ...defForm, unit: e.target.value })}
                    placeholder="VND, Deals, Cuộc gọi, %"
                    className="w-full border border-slate-300 rounded-lg px-3 py-2 text-xs focus:outline-blue-500"
                  />
                </div>
              </div>

              <div>
                <label className="block text-slate-600 font-semibold mb-1">Chỉ tiêu Mặc định</label>
                <input
                  type="number"
                  value={defForm.defaultTarget}
                  onChange={(e) => setDefForm({ ...defForm, defaultTarget: Number(e.target.value) })}
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-xs focus:outline-blue-500"
                />
              </div>

              <div>
                <label className="block text-slate-600 font-semibold mb-1">Mô tả chi tiết</label>
                <textarea
                  rows={2}
                  value={defForm.description}
                  onChange={(e) => setDefForm({ ...defForm, description: e.target.value })}
                  placeholder="Quy tắc tính toán và điều kiện áp dụng..."
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-xs focus:outline-blue-500"
                />
              </div>
            </div>

            <div className="flex items-center justify-end gap-2 pt-2 border-t border-slate-100">
              <button
                type="button"
                onClick={() => setIsDefModalOpen(false)}
                className="px-3 py-2 rounded-lg text-xs font-semibold text-slate-600 hover:bg-slate-100"
              >
                Hủy
              </button>
              <button
                type="button"
                onClick={() => createDefMutation.mutate(defForm)}
                disabled={!defForm.code || !defForm.name || createDefMutation.isPending}
                className="px-4 py-2 rounded-lg text-xs font-semibold bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50"
              >
                {createDefMutation.isPending ? 'Đang lưu...' : 'Tạo Định nghĩa'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
