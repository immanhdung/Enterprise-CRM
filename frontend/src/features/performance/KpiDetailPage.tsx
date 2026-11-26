import React, { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Target,
  ArrowLeft,
  Calculator,
  CheckCircle2,
  PhoneCall,
  Users,
  Briefcase,
  DollarSign,
} from 'lucide-react';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';
import type { EvaluateKpiRequest, ApproveKpiRequest } from '@/types/performance';

export const KpiDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();

  const [isEvalModalOpen, setIsEvalModalOpen] = useState(false);
  const [isApproveModalOpen, setIsApproveModalOpen] = useState(false);
  const [evalForm, setEvalForm] = useState<EvaluateKpiRequest>({
    achievedRevenue: undefined,
    achievedDealsWon: undefined,
    achievedNewLeads: undefined,
    achievedCalls: undefined,
    achievedMeetings: undefined,
    achievedTasksCompleted: undefined,
    achievedConversionRate: undefined,
    evaluationNotes: '',
  });
  const [approveForm, setApproveForm] = useState<{ approved: boolean; comments: string }>({
    approved: true,
    comments: '',
  });

  const { data: kpi, isLoading } = useQuery({
    queryKey: ['kpis', 'detail', id],
    queryFn: () => performanceService.getKpiAssignmentById(id!),
    enabled: !!id,
  });

  const autoCalcMutation = useMutation({
    mutationFn: () => performanceService.autoCalculateKpiActuals(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['kpis', 'detail', id] });
    },
  });

  const evaluateMutation = useMutation({
    mutationFn: (req: EvaluateKpiRequest) => performanceService.evaluateKpiAssignment(id!, req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['kpis', 'detail', id] });
      setIsEvalModalOpen(false);
    },
  });

  const approveMutation = useMutation({
    mutationFn: (req: ApproveKpiRequest) => performanceService.approveKpiAssignment(id!, req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['kpis', 'detail', id] });
      setIsApproveModalOpen(false);
    },
  });

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const getMetricProgress = (achieved: number, target: number) => {
    if (target <= 0) return 100;
    return Math.min(100, Math.round((achieved / target) * 100));
  };

  if (isLoading) {
    return (
      <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
        <PerformanceNavigation />
        <div className="p-8 text-center text-slate-400 text-xs">Đang tải hồ sơ KPI...</div>
      </div>
    );
  }

  if (!kpi) {
    return (
      <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
        <PerformanceNavigation />
        <div className="p-8 text-center text-rose-500 text-xs">Không tìm thấy bản ghi KPI này.</div>
      </div>
    );
  }

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <PerformanceNavigation />

      <div className="p-6 space-y-6 max-w-5xl mx-auto w-full">
        {/* Back breadcrumb */}
        <div className="flex items-center gap-2 text-xs text-slate-500">
          <Link to="/performance/kpis" className="hover:text-blue-600 flex items-center gap-1 font-medium">
            <ArrowLeft className="w-3.5 h-3.5" />
            <span>Quay lại Quản lý KPI</span>
          </Link>
          <span>/</span>
          <span className="font-semibold text-slate-800">
            {kpi.employeeName} – Tháng {kpi.periodMonth}/{kpi.periodYear}
          </span>
        </div>

        {/* Profile Card Header */}
        <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-xs flex flex-col md:flex-row md:items-center justify-between gap-6">
          <div className="space-y-1">
            <div className="flex items-center gap-3">
              <div className="w-11 h-11 rounded-xl bg-blue-100 text-blue-700 flex items-center justify-center font-bold text-lg">
                <Target className="w-6 h-6" />
              </div>
              <div>
                <h1 className="text-xl font-bold text-slate-900 tracking-tight">
                  Hồ sơ KPI: {kpi.employeeName}
                </h1>
                <div className="text-xs text-slate-500">
                  Mã NV: <span className="font-semibold text-slate-700">{kpi.employeeCode}</span> • Phòng ban:{' '}
                  <span className="font-semibold text-slate-700">{kpi.departmentName || 'Sales'}</span> • Kỳ:{' '}
                  <span className="font-semibold text-blue-600">Tháng {kpi.periodMonth}/{kpi.periodYear}</span>
                </div>
              </div>
            </div>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            <button
              type="button"
              onClick={() => autoCalcMutation.mutate()}
              disabled={autoCalcMutation.isPending}
              className="flex items-center gap-1.5 px-3 py-2 rounded-lg text-xs font-semibold bg-blue-50 text-blue-700 border border-blue-200 hover:bg-blue-100 transition-colors"
            >
              <Calculator className="w-3.5 h-3.5" />
              <span>{autoCalcMutation.isPending ? 'Đang tính...' : 'Tính tự động từ CRM'}</span>
            </button>
            <button
              type="button"
              onClick={() => {
                setEvalForm({
                  achievedRevenue: kpi.achievedRevenue,
                  achievedDealsWon: kpi.achievedDealsWon,
                  achievedNewLeads: kpi.achievedNewLeads,
                  achievedCalls: kpi.achievedCalls,
                  achievedMeetings: kpi.achievedMeetings,
                  achievedTasksCompleted: kpi.achievedTasksCompleted,
                  achievedConversionRate: kpi.achievedConversionRate,
                  evaluationNotes: kpi.approvalNotes || '',
                });
                setIsEvalModalOpen(true);
              }}
              className="flex items-center gap-1.5 px-3 py-2 rounded-lg text-xs font-semibold bg-slate-100 text-slate-700 hover:bg-slate-200 transition-colors"
            >
              <span>Đánh giá Thủ công</span>
            </button>
            <button
              type="button"
              onClick={() => setIsApproveModalOpen(true)}
              className="flex items-center gap-1.5 px-4 py-2 rounded-lg text-xs font-semibold bg-emerald-600 text-white hover:bg-emerald-700 transition-colors shadow-xs"
            >
              <CheckCircle2 className="w-3.5 h-3.5" />
              <span>Xét duyệt KPI</span>
            </button>
          </div>
        </div>

        {/* Score Card */}
        <div className="bg-linear-to-r from-blue-900 to-indigo-900 text-white p-6 rounded-xl shadow-md flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div>
            <div className="text-xs text-blue-200 font-semibold uppercase tracking-wider">
              Tổng điểm Hoàn thành KPI (Weighted Score)
            </div>
            <div className="text-4xl font-black mt-1 tracking-tight flex items-baseline gap-2">
              <span>{kpi.achievementScore}%</span>
              <span className="text-sm font-normal text-blue-200">
                (Trọng số: {kpi.weightPercent}%)
              </span>
            </div>
            <div className="text-xs text-blue-200 mt-2">
              Trạng thái:{' '}
              <span className="font-bold text-white uppercase bg-white/20 px-2 py-0.5 rounded">
                {kpi.status}
              </span>
            </div>
          </div>

          <div className="text-right text-xs text-blue-200 space-y-1">
            {kpi.evaluatedAt && (
              <div>Đánh giá lúc: {new Date(kpi.evaluatedAt).toLocaleString('vi-VN')}</div>
            )}
            {kpi.approvedAt && (
              <div>Phê duyệt lúc: {new Date(kpi.approvedAt).toLocaleString('vi-VN')}</div>
            )}
            {kpi.approvalNotes && (
              <div className="text-amber-200 max-w-sm truncate text-left italic">
                "{kpi.approvalNotes}"
              </div>
            )}
          </div>
        </div>

        {/* Detailed Metrics Breakdown */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {/* Sales Revenue */}
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs space-y-2">
            <div className="flex items-center justify-between text-xs font-semibold text-slate-700">
              <span className="flex items-center gap-1.5 text-blue-600">
                <DollarSign className="w-4 h-4" /> Doanh thu Hợp đồng Ký kết
              </span>
              <span className="font-black text-slate-900">
                {formatVnd(kpi.achievedRevenue)} / {formatVnd(kpi.targetRevenue)}
              </span>
            </div>
            <div className="w-full bg-slate-100 rounded-full h-2.5 overflow-hidden">
              <div
                className="bg-blue-600 h-2.5 rounded-full"
                style={{ width: `${getMetricProgress(kpi.achievedRevenue, kpi.targetRevenue)}%` }}
              />
            </div>
            <div className="text-right text-[11px] font-bold text-blue-700">
              {kpi.targetRevenue > 0
                ? `${Math.round((kpi.achievedRevenue / kpi.targetRevenue) * 100)}% chỉ tiêu`
                : '100%'}
            </div>
          </div>

          {/* Deals Won */}
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs space-y-2">
            <div className="flex items-center justify-between text-xs font-semibold text-slate-700">
              <span className="flex items-center gap-1.5 text-indigo-600">
                <Briefcase className="w-4 h-4" /> Số Deal Thành công (Won)
              </span>
              <span className="font-black text-slate-900">
                {kpi.achievedDealsWon} / {kpi.targetDealsWon} Deals
              </span>
            </div>
            <div className="w-full bg-slate-100 rounded-full h-2.5 overflow-hidden">
              <div
                className="bg-indigo-600 h-2.5 rounded-full"
                style={{ width: `${getMetricProgress(kpi.achievedDealsWon, kpi.targetDealsWon)}%` }}
              />
            </div>
            <div className="text-right text-[11px] font-bold text-indigo-700">
              {kpi.targetDealsWon > 0
                ? `${Math.round((kpi.achievedDealsWon / kpi.targetDealsWon) * 100)}% chỉ tiêu`
                : '100%'}
            </div>
          </div>

          {/* New Leads */}
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs space-y-2">
            <div className="flex items-center justify-between text-xs font-semibold text-slate-700">
              <span className="flex items-center gap-1.5 text-emerald-600">
                <Users className="w-4 h-4" /> Khách hàng Tiềm năng (Leads)
              </span>
              <span className="font-black text-slate-900">
                {kpi.achievedNewLeads} / {kpi.targetNewLeads} Leads
              </span>
            </div>
            <div className="w-full bg-slate-100 rounded-full h-2.5 overflow-hidden">
              <div
                className="bg-emerald-600 h-2.5 rounded-full"
                style={{ width: `${getMetricProgress(kpi.achievedNewLeads, kpi.targetNewLeads)}%` }}
              />
            </div>
            <div className="text-right text-[11px] font-bold text-emerald-700">
              {kpi.targetNewLeads > 0
                ? `${Math.round((kpi.achievedNewLeads / kpi.targetNewLeads) * 100)}% chỉ tiêu`
                : '100%'}
            </div>
          </div>

          {/* Calls & Meetings */}
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs space-y-2">
            <div className="flex items-center justify-between text-xs font-semibold text-slate-700">
              <span className="flex items-center gap-1.5 text-amber-600">
                <PhoneCall className="w-4 h-4" /> Cuộc gọi & Gặp gỡ Khách hàng
              </span>
              <span className="font-black text-slate-900">
                {kpi.achievedCalls}/{kpi.targetCalls} gọi • {kpi.achievedMeetings}/{kpi.targetMeetings} gặp
              </span>
            </div>
            <div className="w-full bg-slate-100 rounded-full h-2.5 overflow-hidden">
              <div
                className="bg-amber-600 h-2.5 rounded-full"
                style={{ width: `${getMetricProgress(kpi.achievedCalls, kpi.targetCalls)}%` }}
              />
            </div>
            <div className="text-right text-[11px] font-bold text-amber-700">
              {kpi.targetCalls > 0
                ? `${Math.round((kpi.achievedCalls / kpi.targetCalls) * 100)}% cuộc gọi`
                : '100%'}
            </div>
          </div>
        </div>
      </div>

      {/* Modal: Manual Evaluation */}
      {isEvalModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-xs p-4">
          <div className="bg-white rounded-xl shadow-xl border border-slate-200 max-w-md w-full p-6 space-y-4">
            <div className="flex items-center justify-between border-b border-slate-100 pb-3">
              <h3 className="text-base font-bold text-slate-900">Đánh giá Kết quả KPI</h3>
              <button
                type="button"
                onClick={() => setIsEvalModalOpen(false)}
                className="text-slate-400 hover:text-slate-600"
              >
                ✕
              </button>
            </div>

            <div className="space-y-3 text-xs">
              <div>
                <label className="block text-slate-600 font-semibold mb-1">Doanh thu Thực tế đạt (VND)</label>
                <input
                  type="number"
                  value={evalForm.achievedRevenue || 0}
                  onChange={(e) => setEvalForm({ ...evalForm, achievedRevenue: Number(e.target.value) })}
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-xs focus:outline-blue-500"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-slate-600 font-semibold mb-1">Số Deal Won đạt</label>
                  <input
                    type="number"
                    value={evalForm.achievedDealsWon || 0}
                    onChange={(e) => setEvalForm({ ...evalForm, achievedDealsWon: Number(e.target.value) })}
                    className="w-full border border-slate-300 rounded-lg px-3 py-2 text-xs focus:outline-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-slate-600 font-semibold mb-1">Số Lead chuyển đổi</label>
                  <input
                    type="number"
                    value={evalForm.achievedNewLeads || 0}
                    onChange={(e) => setEvalForm({ ...evalForm, achievedNewLeads: Number(e.target.value) })}
                    className="w-full border border-slate-300 rounded-lg px-3 py-2 text-xs focus:outline-blue-500"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-slate-600 font-semibold mb-1">Số cuộc gọi đạt</label>
                  <input
                    type="number"
                    value={evalForm.achievedCalls || 0}
                    onChange={(e) => setEvalForm({ ...evalForm, achievedCalls: Number(e.target.value) })}
                    className="w-full border border-slate-300 rounded-lg px-3 py-2 text-xs focus:outline-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-slate-600 font-semibold mb-1">Số cuộc gặp đạt</label>
                  <input
                    type="number"
                    value={evalForm.achievedMeetings || 0}
                    onChange={(e) => setEvalForm({ ...evalForm, achievedMeetings: Number(e.target.value) })}
                    className="w-full border border-slate-300 rounded-lg px-3 py-2 text-xs focus:outline-blue-500"
                  />
                </div>
              </div>

              <div>
                <label className="block text-slate-600 font-semibold mb-1">Nhận xét / Ghi chú đánh giá</label>
                <textarea
                  rows={2}
                  value={evalForm.evaluationNotes}
                  onChange={(e) => setEvalForm({ ...evalForm, evaluationNotes: e.target.value })}
                  placeholder="Ghi chú về nỗ lực và lý do sai lệch..."
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-xs focus:outline-blue-500"
                />
              </div>
            </div>

            <div className="flex items-center justify-end gap-2 pt-2 border-t border-slate-100">
              <button
                type="button"
                onClick={() => setIsEvalModalOpen(false)}
                className="px-3 py-2 rounded-lg text-xs font-semibold text-slate-600 hover:bg-slate-100"
              >
                Hủy
              </button>
              <button
                type="button"
                onClick={() => evaluateMutation.mutate(evalForm)}
                disabled={evaluateMutation.isPending}
                className="px-4 py-2 rounded-lg text-xs font-semibold bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50"
              >
                {evaluateMutation.isPending ? 'Đang lưu...' : 'Lưu Đánh giá'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal: Approval */}
      {isApproveModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-xs p-4">
          <div className="bg-white rounded-xl shadow-xl border border-slate-200 max-w-sm w-full p-6 space-y-4">
            <div className="flex items-center justify-between border-b border-slate-100 pb-3">
              <h3 className="text-base font-bold text-slate-900">Phê duyệt Kết quả KPI</h3>
              <button
                type="button"
                onClick={() => setIsApproveModalOpen(false)}
                className="text-slate-400 hover:text-slate-600"
              >
                ✕
              </button>
            </div>

            <div className="space-y-3 text-xs">
              <div className="flex items-center gap-4">
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="radio"
                    name="approveChoice"
                    checked={approveForm.approved}
                    onChange={() => setApproveForm({ ...approveForm, approved: true })}
                  />
                  <span className="font-bold text-emerald-700">Duyệt Đạt (Approve)</span>
                </label>
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="radio"
                    name="approveChoice"
                    checked={!approveForm.approved}
                    onChange={() => setApproveForm({ ...approveForm, approved: false })}
                  />
                  <span className="font-bold text-rose-700">Từ chối (Reject)</span>
                </label>
              </div>

              <div>
                <label className="block text-slate-600 font-semibold mb-1">Nhận xét của Cấp Quản lý</label>
                <textarea
                  rows={3}
                  value={approveForm.comments}
                  onChange={(e) => setApproveForm({ ...approveForm, comments: e.target.value })}
                  placeholder="Góp ý hoặc chỉ đạo tiếp theo..."
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-xs focus:outline-blue-500"
                />
              </div>
            </div>

            <div className="flex items-center justify-end gap-2 pt-2 border-t border-slate-100">
              <button
                type="button"
                onClick={() => setIsApproveModalOpen(false)}
                className="px-3 py-2 rounded-lg text-xs font-semibold text-slate-600 hover:bg-slate-100"
              >
                Hủy
              </button>
              <button
                type="button"
                onClick={() => approveMutation.mutate(approveForm)}
                disabled={approveMutation.isPending}
                className={`px-4 py-2 rounded-lg text-xs font-semibold text-white ${
                  approveForm.approved ? 'bg-emerald-600 hover:bg-emerald-700' : 'bg-rose-600 hover:bg-rose-700'
                } disabled:opacity-50`}
              >
                {approveMutation.isPending ? 'Đang lưu...' : 'Xác nhận Quyết định'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
