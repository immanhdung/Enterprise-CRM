import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  Plus,
  Calendar,
  Target,
  ArrowRight,
  Sliders,
  AlertCircle,
  CheckCircle2,
} from 'lucide-react';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';
import { employeeService } from '@/services/employeeService';
import type { CreateKpiAssignmentRequest } from '@/types/performance';

export const KpiAssignmentsPage: React.FC = () => {
  const queryClient = useQueryClient();
  const now = new Date();
  const [selectedMonth, setSelectedMonth] = useState<number>(now.getMonth() + 1);
  const [selectedYear, setSelectedYear] = useState<number>(now.getFullYear());
  const [isAssignModalOpen, setIsAssignModalOpen] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Form State
  const [form, setForm] = useState<CreateKpiAssignmentRequest>({
    employeeId: '',
    scope: 'Individual',
    periodMonth: now.getMonth() + 1,
    periodYear: now.getFullYear(),
    targetRevenue: 200000000,
    targetDealsWon: 5,
    targetNewLeads: 20,
    targetCalls: 50,
    targetMeetings: 15,
    targetTasksCompleted: 30,
    targetConversionRate: 25,
    weightPercent: 100,
    notes: '',
  });

  const { data: assignments, isLoading: isAssignmentsLoading } = useQuery({
    queryKey: ['kpis', 'assignments', selectedMonth, selectedYear],
    queryFn: () =>
      performanceService.getKpiAssignments({
        month: selectedMonth,
        year: selectedYear,
      }),
  });

  const { data: employeesResponse } = useQuery({
    queryKey: ['employees'],
    queryFn: () => employeeService.getEmployees(),
  });
  const employees = employeesResponse?.data || [];

  const createAssignmentMutation = useMutation({
    mutationFn: (req: CreateKpiAssignmentRequest) => performanceService.createKpiAssignment(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['kpis', 'assignments'] });
      setIsAssignModalOpen(false);
      setSuccessMessage('Đã thiết lập chỉ tiêu KPI thành công cho nhân sự!');
      setErrorMessage(null);
      setTimeout(() => setSuccessMessage(null), 5000);
    },
    onError: (err: any) => {
      setErrorMessage(err?.response?.data?.message || 'Có lỗi xảy ra khi tạo chỉ tiêu KPI.');
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.employeeId) {
      setErrorMessage('Vui lòng chọn nhân viên kinh doanh.');
      return;
    }
    createAssignmentMutation.mutate({
      ...form,
      periodMonth: Number(form.periodMonth),
      periodYear: Number(form.periodYear),
      targetRevenue: Number(form.targetRevenue),
      targetDealsWon: Number(form.targetDealsWon),
      targetNewLeads: Number(form.targetNewLeads),
      targetCalls: Number(form.targetCalls),
      targetMeetings: Number(form.targetMeetings),
      targetTasksCompleted: Number(form.targetTasksCompleted),
      targetConversionRate: Number(form.targetConversionRate),
      weightPercent: Number(form.weightPercent),
    });
  };

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
              <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-100 text-blue-800">
                Chỉ tiêu & Phân bổ
              </span>
              <span className="text-xs text-slate-500">Module A – KPI Management</span>
            </div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight mt-1">
              Phân bổ & Thiết lập chỉ tiêu KPI
            </h1>
            <p className="text-sm text-slate-500 mt-1">
              Giao chỉ tiêu doanh thu, số deal chốt, cuộc gọi, cuộc hẹn và tỷ lệ chuyển đổi cho cá nhân, nhóm và phòng ban.
            </p>
          </div>

          <div className="flex items-center gap-2.5">
            <button
              onClick={() => {
                setForm((prev) => ({
                  ...prev,
                  periodMonth: selectedMonth,
                  periodYear: selectedYear,
                }));
                setIsAssignModalOpen(true);
              }}
              className="inline-flex items-center gap-1.5 px-4 py-2 text-sm font-semibold text-white bg-blue-600 rounded-lg hover:bg-blue-700 shadow-xs transition"
            >
              <Plus className="w-4 h-4" />
              <span>Giao chỉ tiêu mới</span>
            </button>
          </div>
        </div>

        {/* Feedback alerts */}
        {successMessage && (
          <div className="p-4 bg-emerald-50 border border-emerald-200 rounded-xl flex items-center gap-3 text-emerald-800 text-sm">
            <CheckCircle2 className="w-5 h-5 text-emerald-600 shrink-0" />
            <span>{successMessage}</span>
          </div>
        )}
        {errorMessage && (
          <div className="p-4 bg-rose-50 border border-rose-200 rounded-xl flex items-center gap-3 text-rose-800 text-sm">
            <AlertCircle className="w-5 h-5 text-rose-600 shrink-0" />
            <span>{errorMessage}</span>
          </div>
        )}

        {/* Filters */}
        <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex flex-wrap items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-2 text-slate-600 font-medium text-sm">
              <Calendar className="w-4 h-4 text-slate-400" />
              <span>Kỳ đánh giá:</span>
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

          <div className="text-xs text-slate-500">
            Tổng cộng <span className="font-bold text-slate-800">{assignments?.length || 0}</span> chỉ tiêu được giao trong tháng {selectedMonth}/{selectedYear}
          </div>
        </div>

        {/* Assignments Table */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
          <div className="p-4 border-b border-slate-100 flex items-center justify-between">
            <h3 className="text-sm font-semibold text-slate-900 flex items-center gap-2">
              <Target className="w-4 h-4 text-blue-600" />
              <span>Danh sách chỉ tiêu theo nhân sự</span>
            </h3>
            <span className="text-xs text-slate-400">Tự động đối chiếu với kết quả thực tế</span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-600">
              <thead className="bg-slate-50 text-xs font-semibold text-slate-500 uppercase tracking-wider border-b border-slate-200">
                <tr>
                  <th className="px-5 py-3.5">Nhân viên</th>
                  <th className="px-5 py-3.5">Phạm vi</th>
                  <th className="px-5 py-3.5">Chỉ tiêu Doanh thu</th>
                  <th className="px-5 py-3.5">Chỉ tiêu Deal</th>
                  <th className="px-5 py-3.5">Lead / Gọi / Hẹn</th>
                  <th className="px-5 py-3.5">Tỷ lệ CĐ (%)</th>
                  <th className="px-5 py-3.5">Trọng số</th>
                  <th className="px-5 py-3.5">Trạng thái</th>
                  <th className="px-5 py-3.5 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {isAssignmentsLoading ? (
                  <tr>
                    <td colSpan={9} className="px-5 py-8 text-center text-slate-400">
                      Đang tải danh sách chỉ tiêu...
                    </td>
                  </tr>
                ) : !assignments || assignments.length === 0 ? (
                  <tr>
                    <td colSpan={9} className="px-5 py-12 text-center text-slate-400">
                      Chưa có chỉ tiêu nào được giao trong tháng {selectedMonth}/{selectedYear}.
                      <div className="mt-2">
                        <button
                          onClick={() => setIsAssignModalOpen(true)}
                          className="text-blue-600 font-semibold hover:underline text-xs"
                        >
                          + Giao chỉ tiêu ngay
                        </button>
                      </div>
                    </td>
                  </tr>
                ) : (
                  assignments.map((item) => (
                    <tr key={item.id} className="hover:bg-slate-50/80 transition-colors">
                      <td className="px-5 py-3.5 font-medium text-slate-900">
                        <div className="flex items-center gap-2">
                          <div className="w-8 h-8 rounded-full bg-blue-100 text-blue-700 font-bold flex items-center justify-center text-xs">
                            {item.employeeName?.substring(0, 2).toUpperCase() || 'NV'}
                          </div>
                          <div>
                            <div>{item.employeeName}</div>
                            <div className="text-xs text-slate-400">{item.employeeCode}</div>
                          </div>
                        </div>
                      </td>
                      <td className="px-5 py-3.5">
                        <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded text-xs font-medium bg-slate-100 text-slate-700">
                          {item.scope}
                        </span>
                      </td>
                      <td className="px-5 py-3.5 font-semibold text-slate-900">
                        {formatVnd(item.targetRevenue)}
                      </td>
                      <td className="px-5 py-3.5">
                        <span className="font-semibold text-slate-800">{item.targetDealsWon}</span> deals
                      </td>
                      <td className="px-5 py-3.5 text-xs text-slate-500">
                        <div>Leads: <span className="font-medium text-slate-700">{item.targetNewLeads}</span></div>
                        <div>Cuộc gọi: <span className="font-medium text-slate-700">{item.targetCalls}</span> / Hẹn: <span className="font-medium text-slate-700">{item.targetMeetings}</span></div>
                      </td>
                      <td className="px-5 py-3.5 font-medium text-slate-800">
                        {item.targetConversionRate}%
                      </td>
                      <td className="px-5 py-3.5">
                        <span className="font-bold text-blue-600">{item.weightPercent}%</span>
                      </td>
                      <td className="px-5 py-3.5">
                        <span
                          className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-semibold ${
                            item.status === 'Approved'
                              ? 'bg-emerald-100 text-emerald-800'
                              : item.status === 'Evaluated'
                              ? 'bg-blue-100 text-blue-800'
                              : item.status === 'PendingApproval'
                              ? 'bg-amber-100 text-amber-800'
                              : 'bg-slate-100 text-slate-700'
                          }`}
                        >
                          {item.status}
                        </span>
                      </td>
                      <td className="px-5 py-3.5 text-right">
                        <Link
                          to={`/performance/kpis/${item.id}`}
                          className="inline-flex items-center gap-1 text-xs font-semibold text-blue-600 hover:text-blue-800"
                        >
                          <span>Xem chi tiết</span>
                          <ArrowRight className="w-3.5 h-3.5" />
                        </Link>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Modal Giao Chỉ Tiêu KPI Mới */}
      {isAssignModalOpen && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-2xl w-full p-6 shadow-2xl border border-slate-100 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <div className="flex items-center gap-2">
                <Target className="w-5 h-5 text-blue-600" />
                <h3 className="text-lg font-bold text-slate-900">Giao chỉ tiêu KPI mới</h3>
              </div>
              <button
                onClick={() => setIsAssignModalOpen(false)}
                className="text-slate-400 hover:text-slate-600 text-xl font-semibold"
              >
                &times;
              </button>
            </div>

            <form onSubmit={handleSubmit} className="mt-4 space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">
                    Nhân sự nhận chỉ tiêu *
                  </label>
                  <select
                    value={form.employeeId}
                    onChange={(e) => setForm({ ...form, employeeId: e.target.value })}
                    required
                    className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="">-- Chọn nhân sự Sales --</option>
                    {employees.map((emp) => (
                      <option key={emp.id} value={emp.id}>
                        {emp.fullName} ({emp.departmentName || 'Sales'})
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">
                    Phạm vi áp dụng
                  </label>
                  <select
                    value={form.scope}
                    onChange={(e) => setForm({ ...form, scope: e.target.value })}
                    className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="Individual">Cá nhân (Individual)</option>
                    <option value="Team">Nhóm kinh doanh (Team)</option>
                    <option value="Department">Phòng ban (Department)</option>
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Tháng áp dụng</label>
                  <select
                    value={form.periodMonth}
                    onChange={(e) => setForm({ ...form, periodMonth: Number(e.target.value) })}
                    className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                  >
                    {Array.from({ length: 12 }, (_, i) => i + 1).map((m) => (
                      <option key={m} value={m}>
                        Tháng {m}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Năm áp dụng</label>
                  <select
                    value={form.periodYear}
                    onChange={(e) => setForm({ ...form, periodYear: Number(e.target.value) })}
                    className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                  >
                    {[2024, 2025, 2026, 2027].map((y) => (
                      <option key={y} value={y}>
                        Năm {y}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              <div className="p-4 bg-slate-50 rounded-xl border border-slate-200 space-y-4">
                <div className="font-semibold text-xs text-slate-800 uppercase tracking-wider flex items-center gap-1.5">
                  <Sliders className="w-3.5 h-3.5 text-blue-600" />
                  <span>Các chỉ số mục tiêu cụ thể</span>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs font-medium text-slate-600 mb-1">
                      Mục tiêu Doanh số ký kết (VND) *
                    </label>
                    <input
                      type="number"
                      value={form.targetRevenue}
                      onChange={(e) => setForm({ ...form, targetRevenue: Number(e.target.value) })}
                      required
                      min={0}
                      step={1000000}
                      className="w-full px-3 py-2 text-sm bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-xs font-medium text-slate-600 mb-1">
                      Mục tiêu số Deal thắng (Deals Won) *
                    </label>
                    <input
                      type="number"
                      value={form.targetDealsWon}
                      onChange={(e) => setForm({ ...form, targetDealsWon: Number(e.target.value) })}
                      required
                      min={0}
                      className="w-full px-3 py-2 text-sm bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-xs font-medium text-slate-600 mb-1">
                      Số Lead mới cần tạo / tiếp nhận
                    </label>
                    <input
                      type="number"
                      value={form.targetNewLeads}
                      onChange={(e) => setForm({ ...form, targetNewLeads: Number(e.target.value) })}
                      min={0}
                      className="w-full px-3 py-2 text-sm bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-xs font-medium text-slate-600 mb-1">
                      Số cuộc gọi khách hàng mục tiêu
                    </label>
                    <input
                      type="number"
                      value={form.targetCalls}
                      onChange={(e) => setForm({ ...form, targetCalls: Number(e.target.value) })}
                      min={0}
                      className="w-full px-3 py-2 text-sm bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-xs font-medium text-slate-600 mb-1">
                      Số cuộc hẹn / demo mục tiêu
                    </label>
                    <input
                      type="number"
                      value={form.targetMeetings}
                      onChange={(e) => setForm({ ...form, targetMeetings: Number(e.target.value) })}
                      min={0}
                      className="w-full px-3 py-2 text-sm bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-xs font-medium text-slate-600 mb-1">
                      Tỷ lệ chuyển đổi mục tiêu (%)
                    </label>
                    <input
                      type="number"
                      value={form.targetConversionRate}
                      onChange={(e) => setForm({ ...form, targetConversionRate: Number(e.target.value) })}
                      min={0}
                      max={100}
                      className="w-full px-3 py-2 text-sm bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-2 border-t border-slate-200">
                  <div>
                    <label className="block text-xs font-medium text-slate-600 mb-1">
                      Số công việc cần hoàn thành
                    </label>
                    <input
                      type="number"
                      value={form.targetTasksCompleted}
                      onChange={(e) => setForm({ ...form, targetTasksCompleted: Number(e.target.value) })}
                      min={0}
                      className="w-full px-3 py-2 text-sm bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-xs font-medium text-slate-600 mb-1">
                      Trọng số đánh giá (% Weight)
                    </label>
                    <input
                      type="number"
                      value={form.weightPercent}
                      onChange={(e) => setForm({ ...form, weightPercent: Number(e.target.value) })}
                      min={1}
                      max={100}
                      className="w-full px-3 py-2 text-sm bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                    />
                  </div>
                </div>
              </div>

              <div>
                <label className="block text-xs font-medium text-slate-700 mb-1">Ghi chú / Hướng dẫn</label>
                <textarea
                  value={form.notes}
                  onChange={(e) => setForm({ ...form, notes: e.target.value })}
                  rows={2}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                  placeholder="Ghi chú chi tiết cho nhân sự về mục tiêu tháng này..."
                />
              </div>

              <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsAssignModalOpen(false)}
                  className="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800"
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  disabled={createAssignmentMutation.isPending}
                  className="px-5 py-2 text-sm font-semibold text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition"
                >
                  {createAssignmentMutation.isPending ? 'Đang lưu...' : 'Xác nhận giao chỉ tiêu'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
