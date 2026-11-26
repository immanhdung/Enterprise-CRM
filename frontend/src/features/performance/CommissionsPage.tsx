import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  Percent,
  Play,
  Calendar,
  CheckCircle2,
  Clock,
  XCircle,
  FileCheck,
  Edit3,
  Sliders,
  DollarSign,
  AlertCircle,
  ShieldAlert,
} from 'lucide-react';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';
import type {
  CommissionCalculationItem,
  RunCommissionCalculationRequest,
  CommissionAdjustmentRequest,
  ApproveCommissionRequest,
} from '@/types/performance';

export const CommissionsPage: React.FC = () => {
  const queryClient = useQueryClient();
  const now = new Date();
  const [selectedMonth, setSelectedMonth] = useState<number>(now.getMonth() + 1);
  const [selectedYear, setSelectedYear] = useState<number>(now.getFullYear());
  const [statusFilter, setStatusFilter] = useState<string>('');

  // Modals
  const [isRunModalOpen, setIsRunModalOpen] = useState(false);
  const [runForm, setRunForm] = useState<RunCommissionCalculationRequest>({
    month: now.getMonth() + 1,
    year: now.getFullYear(),
  });

  const [isAdjustModalOpen, setIsAdjustModalOpen] = useState(false);
  const [selectedItemForAdjust, setSelectedItemForAdjust] = useState<CommissionCalculationItem | null>(null);
  const [adjustAmount, setAdjustAmount] = useState<number>(0);
  const [adjustReason, setAdjustReason] = useState<string>('');

  const [isApproveModalOpen, setIsApproveModalOpen] = useState(false);
  const [selectedItemForApprove, setSelectedItemForApprove] = useState<CommissionCalculationItem | null>(null);
  const [approveNotes, setApproveNotes] = useState<string>('');

  const [feedback, setFeedback] = useState<{ type: 'success' | 'error'; message: string } | null>(null);

  // Queries
  const { data: commissions, isLoading } = useQuery({
    queryKey: ['commissions', selectedMonth, selectedYear, statusFilter],
    queryFn: () =>
      performanceService.getCommissions({
        month: selectedMonth,
        year: selectedYear,
        status: statusFilter || undefined,
      }),
  });

  const { data: policies } = useQuery({
    queryKey: ['commissions', 'policies'],
    queryFn: () => performanceService.getCommissionPolicies(),
  });

  // Mutations
  const runCalculationMutation = useMutation({
    mutationFn: (req: RunCommissionCalculationRequest) =>
      performanceService.runCommissionCalculation(req),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['commissions'] });
      setIsRunModalOpen(false);
      setFeedback({
        type: 'success',
        message: `Đã tính toán thành công ${data.length} khoản hoa hồng cho kỳ ${runForm.month}/${runForm.year}!`,
      });
      setTimeout(() => setFeedback(null), 5000);
    },
    onError: (err: any) => {
      setFeedback({
        type: 'error',
        message: err?.response?.data?.message || 'Có lỗi khi chạy tính toán hoa hồng.',
      });
    },
  });

  const adjustMutation = useMutation({
    mutationFn: ({ id, req }: { id: string; req: CommissionAdjustmentRequest }) =>
      performanceService.adjustCommission(id, req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['commissions'] });
      setIsAdjustModalOpen(false);
      setSelectedItemForAdjust(null);
      setFeedback({
        type: 'success',
        message: 'Đã cập nhật điều chỉnh hoa hồng thành công và ghi nhận nhật ký kiểm toán.',
      });
      setTimeout(() => setFeedback(null), 5000);
    },
    onError: (err: any) => {
      setFeedback({
        type: 'error',
        message: err?.response?.data?.message || 'Không thể điều chỉnh hoa hồng.',
      });
    },
  });

  const approveMutation = useMutation({
    mutationFn: ({ id, req }: { id: string; req: ApproveCommissionRequest }) =>
      performanceService.approveCommission(id, req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['commissions'] });
      setIsApproveModalOpen(false);
      setSelectedItemForApprove(null);
      setFeedback({
        type: 'success',
        message: 'Đã phê duyệt khoản hoa hồng thành công!',
      });
      setTimeout(() => setFeedback(null), 5000);
    },
    onError: (err: any) => {
      setFeedback({
        type: 'error',
        message: err?.response?.data?.message || 'Không thể phê duyệt hoa hồng (Có thể do quy tắc chống tự duyệt).',
      });
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
      case 'Paid':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-bold bg-purple-100 text-purple-800">
            <DollarSign className="w-3 h-3" /> Đã thanh toán
          </span>
        );
      case 'Calculated':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-bold bg-blue-100 text-blue-800">
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
          <span className="inline-flex items-center px-2 py-0.5 rounded-full text-[11px] font-medium bg-slate-100 text-slate-700">
            {status}
          </span>
        );
    }
  };

  const totalCalculatedCommission = (commissions || []).reduce((acc, c) => acc + (c.netCommission || c.amount || 0), 0);
  const totalApprovedCommission = (commissions || [])
    .filter((c) => c.status === 'Approved' || c.status === 'Paid')
    .reduce((acc, c) => acc + (c.netCommission || c.amount || 0), 0);

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <PerformanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <div className="flex items-center gap-2">
              <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-100 text-emerald-800">
                Chính sách & Tính toán hoa hồng
              </span>
              <span className="text-xs text-slate-500">Module C & D – Commission Engine</span>
            </div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight mt-1 flex items-center gap-2">
              <Percent className="w-6 h-6 text-emerald-600" />
              <span>Quản Lý & Tính Toán Hoa Hồng Kinh Doanh</span>
            </h1>
            <p className="text-sm text-slate-500 mt-1">
              Tính hoa hồng theo dòng tiền thực thu (Confirmed Cash), giá trị hợp đồng, chính sách phân tầng KPI và chống gian lận.
            </p>
          </div>

          <div className="flex items-center gap-2.5">
            <Link
              to="/commissions/policies"
              className="inline-flex items-center gap-1.5 px-3.5 py-2 text-sm font-semibold text-slate-700 bg-white border border-slate-200 rounded-lg hover:bg-slate-50 shadow-xs transition"
            >
              <FileCheck className="w-4 h-4 text-slate-500" />
              <span>Chính sách hoa hồng</span>
            </Link>
            <button
              onClick={() => {
                setRunForm({ month: selectedMonth, year: selectedYear });
                setIsRunModalOpen(true);
              }}
              className="inline-flex items-center gap-1.5 px-4 py-2 text-sm font-semibold text-white bg-emerald-600 rounded-lg hover:bg-emerald-700 shadow-xs transition"
            >
              <Play className="w-4 h-4 fill-white" />
              <span>Chạy tính toán hoa hồng</span>
            </button>
          </div>
        </div>

        {/* Feedback Alert */}
        {feedback && (
          <div
            className={`p-4 rounded-xl flex items-center gap-3 text-sm ${
              feedback.type === 'success'
                ? 'bg-emerald-50 border border-emerald-200 text-emerald-800'
                : 'bg-rose-50 border border-rose-200 text-rose-800'
            }`}
          >
            {feedback.type === 'success' ? (
              <CheckCircle2 className="w-5 h-5 text-emerald-600 shrink-0" />
            ) : (
              <AlertCircle className="w-5 h-5 text-rose-600 shrink-0" />
            )}
            <span>{feedback.message}</span>
          </div>
        )}

        {/* Quick Metric Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-5">
          <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-xs text-slate-500 mb-1">
              <span>Tổng hoa hồng phát sinh kỳ này:</span>
              <span className="w-2 h-2 rounded-full bg-blue-500" />
            </div>
            <div className="text-2xl font-extrabold text-slate-900">{formatVnd(totalCalculatedCommission)}</div>
            <div className="text-xs text-slate-400 mt-1">Toàn bộ nhân sự trong kỳ {selectedMonth}/{selectedYear}</div>
          </div>

          <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-xs text-slate-500 mb-1">
              <span>Hoa hồng đã phê duyệt:</span>
              <span className="w-2 h-2 rounded-full bg-emerald-500" />
            </div>
            <div className="text-2xl font-extrabold text-emerald-600">{formatVnd(totalApprovedCommission)}</div>
            <div className="text-xs text-slate-400 mt-1">Sẵn sàng đưa vào bảng lương trả kỳ tới</div>
          </div>

          <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-xs text-slate-500 mb-1">
              <span>Số khoản hoa hồng ghi nhận:</span>
              <span className="w-2 h-2 rounded-full bg-purple-500" />
            </div>
            <div className="text-2xl font-extrabold text-slate-900">{commissions?.length || 0} khoản</div>
            <div className="text-xs text-slate-400 mt-1">Được tính từ hợp đồng và phiếu thu xác nhận</div>
          </div>
        </div>

        {/* Filters */}
        <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex flex-wrap items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-2 text-slate-600 font-medium text-sm">
              <Calendar className="w-4 h-4 text-slate-400" />
              <span>Kỳ hoa hồng:</span>
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
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="px-3 py-1.5 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Tất cả trạng thái</option>
              <option value="Calculated">Chờ duyệt (Calculated)</option>
              <option value="Approved">Đã duyệt (Approved)</option>
              <option value="Paid">Đã thanh toán (Paid)</option>
              <option value="Rejected">Từ chối (Rejected)</option>
            </select>

            <Link
              to="/commissions/adjustments"
              className="inline-flex items-center gap-1.5 text-xs font-semibold text-slate-600 hover:text-slate-900 bg-slate-100 hover:bg-slate-200 px-3 py-1.5 rounded-lg transition"
            >
              <Sliders className="w-3.5 h-3.5" />
              <span>Nhật ký điều chỉnh</span>
            </Link>
          </div>
        </div>

        {/* Commissions Table */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
          <div className="p-4 border-b border-slate-100 flex items-center justify-between">
            <h3 className="text-sm font-semibold text-slate-900 flex items-center gap-2">
              <Percent className="w-4 h-4 text-emerald-600" />
              <span>Bảng kết quả hoa hồng theo nhân sự</span>
            </h3>
            <span className="text-xs text-slate-400">
              Quy tắc tính: Tiền thực thu confirmed, trần tối đa & phân tầng KPI
            </span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-600">
              <thead className="bg-slate-50 text-xs font-semibold text-slate-500 uppercase tracking-wider border-b border-slate-200">
                <tr>
                  <th className="px-5 py-3.5">Nhân sự</th>
                  <th className="px-5 py-3.5">Chính sách áp dụng</th>
                  <th className="px-5 py-3.5 text-right">Doanh số gốc</th>
                  <th className="px-5 py-3.5 text-center">Tỷ lệ %</th>
                  <th className="px-5 py-3.5 text-right">Hoa hồng gốc</th>
                  <th className="px-5 py-3.5 text-right">Điều chỉnh (+/-)</th>
                  <th className="px-5 py-3.5 text-right">Hoa hồng thực nhận</th>
                  <th className="px-5 py-3.5 text-center">Trạng thái</th>
                  <th className="px-5 py-3.5 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {isLoading ? (
                  <tr>
                    <td colSpan={9} className="px-5 py-10 text-center text-slate-400">
                      Đang tải danh sách hoa hồng...
                    </td>
                  </tr>
                ) : !commissions || commissions.length === 0 ? (
                  <tr>
                    <td colSpan={9} className="px-5 py-12 text-center text-slate-400">
                      Chưa có dữ liệu hoa hồng trong tháng {selectedMonth}/{selectedYear}.
                      <div className="mt-2">
                        <button
                          onClick={() => {
                            setRunForm({ month: selectedMonth, year: selectedYear });
                            setIsRunModalOpen(true);
                          }}
                          className="text-emerald-600 font-semibold hover:underline text-xs"
                        >
                          + Bấm vào đây để chạy tính toán ngay
                        </button>
                      </div>
                    </td>
                  </tr>
                ) : (
                  commissions.map((item) => (
                    <tr key={item.id} className="hover:bg-slate-50/80 transition-colors">
                      <td className="px-5 py-3.5 font-medium text-slate-900">
                        <div className="flex items-center gap-2">
                          <div className="w-8 h-8 rounded-full bg-emerald-100 text-emerald-800 font-bold flex items-center justify-center text-xs">
                            {item.employeeName?.substring(0, 2).toUpperCase()}
                          </div>
                          <div>
                            <div>{item.employeeName}</div>
                            <div className="text-xs text-slate-400">{item.employeeCode}</div>
                          </div>
                        </div>
                      </td>
                      <td className="px-5 py-3.5 text-xs">
                        <div className="font-semibold text-slate-800">{item.policyName || 'Chính sách mặc định'}</div>
                        <div className="text-slate-400">Cơ sở: {item.calculationBasis || 'CollectedCash'}</div>
                      </td>
                      <td className="px-5 py-3.5 text-right font-medium text-slate-900">
                        {formatVnd(item.baseAmount)}
                      </td>
                      <td className="px-5 py-3.5 text-center font-bold text-slate-700">
                        {item.rateApplied || item.rate || 0}%
                      </td>
                      <td className="px-5 py-3.5 text-right font-medium text-slate-700">
                        {formatVnd(item.amount)}
                      </td>
                      <td className="px-5 py-3.5 text-right">
                        {item.adjustmentAmount && item.adjustmentAmount !== 0 ? (
                          <span
                            className={`font-semibold ${
                              item.adjustmentAmount > 0 ? 'text-emerald-600' : 'text-rose-600'
                            }`}
                          >
                            {item.adjustmentAmount > 0 ? '+' : ''}
                            {formatVnd(item.adjustmentAmount)}
                          </span>
                        ) : (
                          <span className="text-slate-300">-</span>
                        )}
                      </td>
                      <td className="px-5 py-3.5 text-right font-extrabold text-emerald-700">
                        {formatVnd(item.netCommission || item.amount)}
                      </td>
                      <td className="px-5 py-3.5 text-center">
                        {getStatusBadge(item.status)}
                      </td>
                      <td className="px-5 py-3.5 text-right">
                        <div className="flex items-center justify-end gap-2">
                          {item.status === 'Calculated' && (
                            <>
                              <button
                                onClick={() => {
                                  setSelectedItemForAdjust(item);
                                  setAdjustAmount(item.adjustmentAmount || 0);
                                  setAdjustReason('');
                                  setIsAdjustModalOpen(true);
                                }}
                                title="Điều chỉnh hoa hồng"
                                className="p-1.5 text-slate-500 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition"
                              >
                                <Edit3 className="w-4 h-4" />
                              </button>
                              <button
                                onClick={() => {
                                  setSelectedItemForApprove(item);
                                  setApproveNotes('');
                                  setIsApproveModalOpen(true);
                                }}
                                title="Phê duyệt hoa hồng"
                                className="p-1.5 text-slate-500 hover:text-emerald-600 hover:bg-emerald-50 rounded-lg transition"
                              >
                                <CheckCircle2 className="w-4 h-4" />
                              </button>
                            </>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Modal Chạy Tính Toán Hoa Hồng */}
      {isRunModalOpen && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <div className="flex items-center gap-2">
                <Play className="w-5 h-5 text-emerald-600" />
                <h3 className="text-lg font-bold text-slate-900">Tính toán hoa hồng bán hàng</h3>
              </div>
              <button
                onClick={() => setIsRunModalOpen(false)}
                className="text-slate-400 hover:text-slate-600 text-xl font-semibold"
              >
                &times;
              </button>
            </div>

            <form
              onSubmit={(e) => {
                e.preventDefault();
                runCalculationMutation.mutate(runForm);
              }}
              className="mt-4 space-y-4"
            >
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Tháng tính hoa hồng</label>
                <select
                  value={runForm.month}
                  onChange={(e) => setRunForm({ ...runForm, month: Number(e.target.value) })}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-emerald-500"
                >
                  {Array.from({ length: 12 }, (_, i) => i + 1).map((m) => (
                    <option key={m} value={m}>
                      Tháng {m}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Năm tính toán</label>
                <select
                  value={runForm.year}
                  onChange={(e) => setRunForm({ ...runForm, year: Number(e.target.value) })}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-emerald-500"
                >
                  {[2024, 2025, 2026, 2027].map((y) => (
                    <option key={y} value={y}>
                      Năm {y}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">
                  Chính sách hoa hồng áp dụng (Tùy chọn)
                </label>
                <select
                  value={runForm.policyId || ''}
                  onChange={(e) => setRunForm({ ...runForm, policyId: e.target.value || undefined })}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-emerald-500"
                >
                  <option value="">-- Tự động áp dụng theo chính sách đang kích hoạt --</option>
                  {(policies || []).map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.name} ({p.calculationBasis} - {p.rate}%)
                    </option>
                  ))}
                </select>
              </div>

              <div className="p-3 bg-amber-50 border border-amber-200 rounded-xl text-xs text-amber-900 leading-relaxed">
                💡 Hệ thống sẽ tự động quét toàn bộ khoản thanh toán <strong>Confirmed</strong> trong tháng {runForm.month}/{runForm.year}, đối chiếu với điểm KPI và áp dụng phân tầng hoa hồng chính xác.
              </div>

              <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsRunModalOpen(false)}
                  className="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800"
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  disabled={runCalculationMutation.isPending}
                  className="px-5 py-2 text-sm font-semibold text-white bg-emerald-600 rounded-lg hover:bg-emerald-700 disabled:opacity-50 transition"
                >
                  {runCalculationMutation.isPending ? 'Đang tính...' : 'Bắt đầu tính toán'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Điều Chỉnh Hoa Hồng (Audit Required) */}
      {isAdjustModalOpen && selectedItemForAdjust && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <div className="flex items-center gap-2">
                <Edit3 className="w-5 h-5 text-blue-600" />
                <h3 className="text-lg font-bold text-slate-900">Điều chỉnh hoa hồng nhân viên</h3>
              </div>
              <button
                onClick={() => setIsAdjustModalOpen(false)}
                className="text-slate-400 hover:text-slate-600 text-xl font-semibold"
              >
                &times;
              </button>
            </div>

            <form
              onSubmit={(e) => {
                e.preventDefault();
                if (!adjustReason.trim()) {
                  alert('Lý do điều chỉnh là bắt buộc theo quy định kiểm toán!');
                  return;
                }
                adjustMutation.mutate({
                  id: selectedItemForAdjust.id,
                  req: {
                    adjustmentAmount: Number(adjustAmount),
                    reason: adjustReason,
                  },
                });
              }}
              className="mt-4 space-y-4"
            >
              <div>
                <div className="text-xs text-slate-500">Nhân sự:</div>
                <div className="font-bold text-slate-900">{selectedItemForAdjust.employeeName}</div>
                <div className="text-xs text-slate-500 mt-1">
                  Hoa hồng gốc: <span className="font-semibold text-slate-800">{formatVnd(selectedItemForAdjust.amount)}</span>
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">
                  Số tiền điều chỉnh (VND) * (Dương: cộng thêm, Âm: trừ bớt)
                </label>
                <input
                  type="number"
                  value={adjustAmount}
                  onChange={(e) => setAdjustAmount(Number(e.target.value))}
                  required
                  step={100000}
                  className="w-full px-3 py-2 text-sm bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <div className="text-xs text-slate-500 mb-1">Hoa hồng thực nhận sau điều chỉnh:</div>
                <div className="text-base font-extrabold text-emerald-700">
                  {formatVnd((selectedItemForAdjust.amount || 0) + Number(adjustAmount))}
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">
                  Lý do điều chỉnh (Bắt buộc theo Audit) *
                </label>
                <textarea
                  value={adjustReason}
                  onChange={(e) => setAdjustReason(e.target.value)}
                  required
                  rows={3}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                  placeholder="Nhập lý do cụ thể: Thưởng thêm dự án khó, bù trừ điều chỉnh khách hoàn tiền, v.v..."
                />
              </div>

              <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsAdjustModalOpen(false)}
                  className="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800"
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  disabled={adjustMutation.isPending}
                  className="px-5 py-2 text-sm font-semibold text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition"
                >
                  {adjustMutation.isPending ? 'Đang lưu...' : 'Lưu điều chỉnh'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Phê Duyệt Hoa Hồng (Anti-self-approval) */}
      {isApproveModalOpen && selectedItemForApprove && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <div className="flex items-center gap-2">
                <CheckCircle2 className="w-5 h-5 text-emerald-600" />
                <h3 className="text-lg font-bold text-slate-900">Phê duyệt hoa hồng</h3>
              </div>
              <button
                onClick={() => setIsApproveModalOpen(false)}
                className="text-slate-400 hover:text-slate-600 text-xl font-semibold"
              >
                &times;
              </button>
            </div>

            <form
              onSubmit={(e) => {
                e.preventDefault();
                approveMutation.mutate({
                  id: selectedItemForApprove.id,
                  req: {
                    status: 'Approved',
                    notes: approveNotes,
                  },
                });
              }}
              className="mt-4 space-y-4"
            >
              <div className="p-3 bg-slate-50 rounded-xl space-y-1.5 text-xs text-slate-600 border border-slate-200">
                <div>Nhân sự nhận: <span className="font-bold text-slate-900">{selectedItemForApprove.employeeName}</span></div>
                <div>Số tiền phê duyệt: <span className="font-extrabold text-emerald-700 text-sm">{formatVnd(selectedItemForApprove.netCommission || selectedItemForApprove.amount)}</span></div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Ghi chú phê duyệt</label>
                <textarea
                  value={approveNotes}
                  onChange={(e) => setApproveNotes(e.target.value)}
                  rows={2}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-emerald-500"
                  placeholder="Ghi chú xác nhận duyệt hoa hồng..."
                />
              </div>

              <div className="p-2.5 bg-amber-50 border border-amber-200 rounded-lg flex items-center gap-2 text-xs text-amber-800">
                <ShieldAlert className="w-4 h-4 text-amber-600 shrink-0" />
                <span>Quy tắc chống tự duyệt: Bạn không thể tự phê duyệt hoa hồng của chính mình.</span>
              </div>

              <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsApproveModalOpen(false)}
                  className="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800"
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  disabled={approveMutation.isPending}
                  className="px-5 py-2 text-sm font-semibold text-white bg-emerald-600 rounded-lg hover:bg-emerald-700 disabled:opacity-50 transition"
                >
                  {approveMutation.isPending ? 'Đang duyệt...' : 'Xác nhận duyệt'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
