import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams, Link } from 'react-router-dom';
import {
  Wallet,
  ArrowLeft,
  Play,
  CheckCircle2,
  Lock,
  Sliders,
  FileText,
  AlertCircle,
  ShieldAlert,
  UserCheck,
} from 'lucide-react';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';
import type {
  PayrollRecord,
  AddPayrollAdjustmentRequest,
  ApprovePayrollRequest,
} from '@/types/performance';

export const PayrollPeriodDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();

  // Modals
  const [isAdjModalOpen, setIsAdjModalOpen] = useState(false);
  const [selectedRecordForAdj, setSelectedRecordForAdj] = useState<PayrollRecord | null>(null);
  const [adjForm, setAdjForm] = useState<AddPayrollAdjustmentRequest>({
    allowanceDelta: 0,
    deductionDelta: 0,
    reason: '',
  });

  const [isApproveModalOpen, setIsApproveModalOpen] = useState(false);
  const [approveLevel, setApproveLevel] = useState<'Manager' | 'Director'>('Manager');
  const [approveNotes, setApproveNotes] = useState('');

  const [feedback, setFeedback] = useState<{ type: 'success' | 'error'; message: string } | null>(null);

  // Queries
  const { data: period, isLoading } = useQuery({
    queryKey: ['payroll', 'periods', id],
    queryFn: () => performanceService.getPayrollPeriodById(id!),
    enabled: !!id,
  });

  const { data: records, isLoading: isRecordsLoading } = useQuery({
    queryKey: ['payroll', 'periods', id, 'records'],
    queryFn: () => performanceService.getPayrollRecords(id!),
    enabled: !!id,
  });

  // Mutations
  const processMutation = useMutation({
    mutationFn: () => performanceService.processPayroll(id!),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['payroll', 'periods', id] });
      queryClient.invalidateQueries({ queryKey: ['payroll', 'periods', id, 'records'] });
      setFeedback({
        type: 'success',
        message: `Đã tính toán bảng lương thành công cho ${data.length} nhân sự!`,
      });
      setTimeout(() => setFeedback(null), 5000);
    },
    onError: (err: any) => {
      setFeedback({
        type: 'error',
        message: err?.response?.data?.message || 'Có lỗi khi chạy tính bảng lương.',
      });
    },
  });

  const adjustmentMutation = useMutation({
    mutationFn: ({ recordId, req }: { recordId: string; req: AddPayrollAdjustmentRequest }) =>
      performanceService.addPayrollAdjustment(recordId, req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payroll', 'periods', id] });
      queryClient.invalidateQueries({ queryKey: ['payroll', 'periods', id, 'records'] });
      setIsAdjModalOpen(false);
      setSelectedRecordForAdj(null);
      setFeedback({
        type: 'success',
        message: 'Đã điều chỉnh phụ cấp/khấu trừ thành công!',
      });
      setTimeout(() => setFeedback(null), 5000);
    },
    onError: (err: any) => {
      setFeedback({
        type: 'error',
        message: err?.response?.data?.message || 'Không thể điều chỉnh bảng lương.',
      });
    },
  });

  const approveMutation = useMutation({
    mutationFn: (req: ApprovePayrollRequest) => performanceService.approvePayrollPeriod(id!, req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payroll', 'periods', id] });
      setIsApproveModalOpen(false);
      setFeedback({
        type: 'success',
        message: `Đã phê duyệt bảng lương cấp ${approveLevel} thành công!`,
      });
      setTimeout(() => setFeedback(null), 5000);
    },
    onError: (err: any) => {
      setFeedback({
        type: 'error',
        message: err?.response?.data?.message || 'Không thể phê duyệt (Quy tắc chống tự duyệt).',
      });
    },
  });

  const finalizeMutation = useMutation({
    mutationFn: () => performanceService.finalizePayrollPeriod(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payroll', 'periods', id] });
      queryClient.invalidateQueries({ queryKey: ['payroll', 'periods', id, 'records'] });
      setFeedback({
        type: 'success',
        message: 'Bảng lương đã được KHÓA SỔ & hoàn tất chi trả thành công! Toàn bộ hoa hồng và thưởng đã được đánh dấu Paid.',
      });
      setTimeout(() => setFeedback(null), 5000);
    },
    onError: (err: any) => {
      setFeedback({
        type: 'error',
        message: err?.response?.data?.message || 'Không thể khóa sổ bảng lương.',
      });
    },
  });

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  if (isLoading) {
    return (
      <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
        <PerformanceNavigation />
        <div className="p-12 text-center text-slate-400">Đang tải chi tiết kỳ lương...</div>
      </div>
    );
  }

  if (!period) {
    return (
      <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
        <PerformanceNavigation />
        <div className="p-12 text-center text-slate-400">Không tìm thấy thông tin kỳ lương.</div>
      </div>
    );
  }

  const isFinalized = period.status === 'Finalized' || period.status === 'Paid';

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <PerformanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Back Link */}
        <Link
          to="/payroll"
          className="inline-flex items-center gap-1.5 text-xs font-semibold text-slate-600 hover:text-slate-900"
        >
          <ArrowLeft className="w-4 h-4" />
          <span>Quay lại danh sách các kỳ lương</span>
        </Link>

        {/* Header Profile */}
        <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-xs">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <div className="flex items-center gap-2">
                <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-purple-100 text-purple-800">
                  Chi tiết kỳ lương
                </span>
                <span className="text-xs text-slate-400">
                  {new Date(period.startDate).toLocaleDateString('vi-VN')} &rarr;{' '}
                  {new Date(period.endDate).toLocaleDateString('vi-VN')}
                </span>
              </div>
              <h1 className="text-2xl font-bold text-slate-900 tracking-tight mt-1">{period.name}</h1>
            </div>

            {/* Workflow Action Buttons */}
            <div className="flex flex-wrap items-center gap-2.5">
              {!isFinalized && (
                <>
                  <button
                    onClick={() => processMutation.mutate()}
                    disabled={processMutation.isPending}
                    className="inline-flex items-center gap-1.5 px-3.5 py-2 text-xs font-semibold text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition"
                  >
                    <Play className="w-3.5 h-3.5 fill-white" />
                    <span>{processMutation.isPending ? 'Đang tính...' : 'Tính lại bảng lương'}</span>
                  </button>

                  {period.status === 'Processing' && (
                    <button
                      onClick={() => {
                        setApproveLevel('Manager');
                        setApproveNotes('');
                        setIsApproveModalOpen(true);
                      }}
                      className="inline-flex items-center gap-1.5 px-3.5 py-2 text-xs font-semibold text-white bg-amber-600 rounded-lg hover:bg-amber-700 transition"
                    >
                      <UserCheck className="w-3.5 h-3.5" />
                      <span>Quản lý duyệt</span>
                    </button>
                  )}

                  {period.status === 'PendingDirectorApproval' && (
                    <button
                      onClick={() => {
                        setApproveLevel('Director');
                        setApproveNotes('');
                        setIsApproveModalOpen(true);
                      }}
                      className="inline-flex items-center gap-1.5 px-3.5 py-2 text-xs font-semibold text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 transition"
                    >
                      <CheckCircle2 className="w-3.5 h-3.5" />
                      <span>Giám đốc / CEO duyệt</span>
                    </button>
                  )}

                  {period.status === 'Approved' && (
                    <button
                      onClick={() => {
                        if (confirm('Bạn có chắc chắn muốn KHÓA SỔ kỳ lương này? Toàn bộ hoa hồng và thưởng sẽ được kết chuyển thành ĐÃ TRẢ (Paid) và không thể chỉnh sửa thêm.')) {
                          finalizeMutation.mutate();
                        }
                      }}
                      disabled={finalizeMutation.isPending}
                      className="inline-flex items-center gap-1.5 px-4 py-2 text-xs font-bold text-white bg-purple-700 rounded-lg hover:bg-purple-800 disabled:opacity-50 transition"
                    >
                      <Lock className="w-3.5 h-3.5" />
                      <span>{finalizeMutation.isPending ? 'Đang khóa...' : 'Khóa sổ & Trả lương'}</span>
                    </button>
                  )}
                </>
              )}

              {isFinalized && (
                <span className="inline-flex items-center gap-1.5 px-3.5 py-2 text-xs font-bold text-purple-800 bg-purple-100 rounded-lg">
                  <Lock className="w-4 h-4" />
                  <span>Kỳ lương đã khóa sổ an toàn</span>
                </span>
              )}
            </div>
          </div>

          {/* Feedback alerts */}
          {feedback && (
            <div
              className={`mt-4 p-4 rounded-xl flex items-center gap-3 text-sm ${
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

          {/* Summary Breakdown Cards */}
          <div className="grid grid-cols-2 sm:grid-cols-6 gap-3 mt-6 pt-6 border-t border-slate-100 text-xs">
            <div className="bg-slate-50 p-3 rounded-xl border border-slate-200">
              <span className="text-slate-400 block mb-1">Lương cơ bản:</span>
              <span className="font-bold text-slate-800">{formatVnd(period.totalBaseSalary)}</span>
            </div>
            <div className="bg-slate-50 p-3 rounded-xl border border-slate-200">
              <span className="text-slate-400 block mb-1">Hoa hồng duyệt:</span>
              <span className="font-bold text-emerald-600">{formatVnd(period.totalCommission)}</span>
            </div>
            <div className="bg-slate-50 p-3 rounded-xl border border-slate-200">
              <span className="text-slate-400 block mb-1">Tiền thưởng:</span>
              <span className="font-bold text-amber-600">{formatVnd(period.totalBonuses)}</span>
            </div>
            <div className="bg-slate-50 p-3 rounded-xl border border-slate-200">
              <span className="text-slate-400 block mb-1">Phụ cấp:</span>
              <span className="font-bold text-blue-600">{formatVnd(period.totalAllowances)}</span>
            </div>
            <div className="bg-slate-50 p-3 rounded-xl border border-slate-200">
              <span className="text-slate-400 block mb-1">Khấu trừ:</span>
              <span className="font-bold text-rose-600">-{formatVnd(period.totalDeductions)}</span>
            </div>
            <div className="bg-purple-50 p-3 rounded-xl border border-purple-200">
              <span className="text-purple-600 font-semibold block mb-1">Tổng thực lĩnh (Net):</span>
              <span className="font-extrabold text-purple-900 text-sm">{formatVnd(period.totalNet)}</span>
            </div>
          </div>
        </div>

        {/* Employee Records Table */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
          <div className="p-4 border-b border-slate-100 flex items-center justify-between">
            <h3 className="text-sm font-semibold text-slate-900 flex items-center gap-2">
              <Wallet className="w-4 h-4 text-purple-600" />
              <span>Bảng kê thu nhập chi tiết từng nhân sự</span>
            </h3>
            <span className="text-xs text-slate-400">Hiển thị {records?.length || 0} nhân sự</span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-600">
              <thead className="bg-slate-50 text-xs font-semibold text-slate-500 uppercase tracking-wider border-b border-slate-200">
                <tr>
                  <th className="px-5 py-3.5">Nhân sự</th>
                  <th className="px-5 py-3.5 text-right">Lương cứng</th>
                  <th className="px-5 py-3.5 text-right">Hoa hồng duyệt</th>
                  <th className="px-5 py-3.5 text-right">Tiền thưởng</th>
                  <th className="px-5 py-3.5 text-right">Phụ cấp</th>
                  <th className="px-5 py-3.5 text-right">Khấu trừ</th>
                  <th className="px-5 py-3.5 text-right">Gross</th>
                  <th className="px-5 py-3.5 text-right">Thực lĩnh (Net)</th>
                  <th className="px-5 py-3.5 text-right">Phiếu lương</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {isRecordsLoading ? (
                  <tr>
                    <td colSpan={9} className="px-5 py-10 text-center text-slate-400">
                      Đang tải danh sách nhân sự...
                    </td>
                  </tr>
                ) : !records || records.length === 0 ? (
                  <tr>
                    <td colSpan={9} className="px-5 py-12 text-center text-slate-400">
                      Chưa có dữ liệu bảng lương. Bấm vào <strong>"Tính lại bảng lương"</strong> để tổng hợp dữ liệu.
                    </td>
                  </tr>
                ) : (
                  records.map((r) => (
                    <tr key={r.id} className="hover:bg-slate-50/80 transition-colors">
                      <td className="px-5 py-3.5 font-medium text-slate-900">
                        <div className="flex items-center gap-2">
                          <div className="w-8 h-8 rounded-full bg-purple-100 text-purple-800 font-bold flex items-center justify-center text-xs">
                            {r.employeeName?.substring(0, 2).toUpperCase()}
                          </div>
                          <div>
                            <div>{r.employeeName}</div>
                            <div className="text-xs text-slate-400">{r.employeeCode}</div>
                          </div>
                        </div>
                      </td>
                      <td className="px-5 py-3.5 text-right font-medium text-slate-800">
                        {formatVnd(r.baseSalary)}
                      </td>
                      <td className="px-5 py-3.5 text-right font-semibold text-emerald-600">
                        {formatVnd(r.commissionApproved || r.commissionEarned)}
                      </td>
                      <td className="px-5 py-3.5 text-right font-semibold text-amber-600">
                        {formatVnd(r.bonuses)}
                      </td>
                      <td className="px-5 py-3.5 text-right font-medium text-blue-600">
                        {formatVnd(r.allowances)}
                      </td>
                      <td className="px-5 py-3.5 text-right font-medium text-rose-600">
                        -{formatVnd(r.deductions)}
                      </td>
                      <td className="px-5 py-3.5 text-right font-bold text-slate-900">
                        {formatVnd(r.grossSalary)}
                      </td>
                      <td className="px-5 py-3.5 text-right font-extrabold text-purple-700 text-base">
                        {formatVnd(r.netSalary)}
                      </td>
                      <td className="px-5 py-3.5 text-right">
                        <div className="flex items-center justify-end gap-2">
                          {!isFinalized && (
                            <button
                              onClick={() => {
                                setSelectedRecordForAdj(r);
                                setAdjForm({ allowanceDelta: 0, deductionDelta: 0, reason: '' });
                                setIsAdjModalOpen(true);
                              }}
                              title="Thêm phụ cấp / khấu trừ"
                              className="p-1.5 text-slate-500 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition"
                            >
                              <Sliders className="w-4 h-4" />
                            </button>
                          )}
                          <Link
                            to={`/payroll/employees/${r.employeeId}`}
                            title="Xem phiếu lương"
                            className="p-1.5 text-slate-500 hover:text-purple-600 hover:bg-purple-50 rounded-lg transition"
                          >
                            <FileText className="w-4 h-4" />
                          </Link>
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

      {/* Modal Điều Chỉnh Phụ Cấp / Khấu Trừ */}
      {isAdjModalOpen && selectedRecordForAdj && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <div className="flex items-center gap-2">
                <Sliders className="w-5 h-5 text-blue-600" />
                <h3 className="text-lg font-bold text-slate-900">Điều chỉnh thu nhập</h3>
              </div>
              <button
                onClick={() => setIsAdjModalOpen(false)}
                className="text-slate-400 hover:text-slate-600 text-xl font-semibold"
              >
                &times;
              </button>
            </div>

            <form
              onSubmit={(e) => {
                e.preventDefault();
                if (!adjForm.reason.trim()) {
                  alert('Vui lòng nhập lý do điều chỉnh theo quy định kiểm toán.');
                  return;
                }
                const delta = Number(adjForm.allowanceDelta || 0) - Number(adjForm.deductionDelta || 0);
                const adjType = (adjForm.allowanceDelta || 0) >= (adjForm.deductionDelta || 0) ? 'Allowance' : 'Penalty';
                adjustmentMutation.mutate({
                  recordId: selectedRecordForAdj.id,
                  req: {
                    payrollRecordId: selectedRecordForAdj.id,
                    type: adjType,
                    amount: Math.abs(delta),
                    reason: adjForm.reason,
                    allowanceDelta: Number(adjForm.allowanceDelta || 0),
                    deductionDelta: Number(adjForm.deductionDelta || 0),
                  },
                });
              }}
              className="mt-4 space-y-4"
            >
              <div className="text-xs text-slate-600 bg-slate-50 p-3 rounded-xl border border-slate-200">
                Nhân sự: <span className="font-bold text-slate-900">{selectedRecordForAdj.employeeName}</span>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">
                  Phụ cấp cộng thêm (VND)
                </label>
                <input
                  type="number"
                  value={adjForm.allowanceDelta}
                  onChange={(e) => setAdjForm({ ...adjForm, allowanceDelta: Number(e.target.value) })}
                  step={100000}
                  className="w-full px-3 py-2 text-sm bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">
                  Khoản khấu trừ thêm (VND)
                </label>
                <input
                  type="number"
                  value={adjForm.deductionDelta}
                  onChange={(e) => setAdjForm({ ...adjForm, deductionDelta: Number(e.target.value) })}
                  step={100000}
                  className="w-full px-3 py-2 text-sm bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">
                  Lý do điều chỉnh (Bắt buộc theo Audit) *
                </label>
                <textarea
                  value={adjForm.reason}
                  onChange={(e) => setAdjForm({ ...adjForm, reason: e.target.value })}
                  required
                  rows={2}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                  placeholder="Lý do: Phụ cấp đi lại công tác, khấu trừ tạm ứng..."
                />
              </div>

              <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsAdjModalOpen(false)}
                  className="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800"
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  disabled={adjustmentMutation.isPending}
                  className="px-5 py-2 text-sm font-semibold text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition"
                >
                  {adjustmentMutation.isPending ? 'Đang lưu...' : 'Lưu điều chỉnh'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Phê Duyệt Cấp Quản Lý / Giám Đốc */}
      {isApproveModalOpen && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <div className="flex items-center gap-2">
                <CheckCircle2 className="w-5 h-5 text-purple-600" />
                <h3 className="text-lg font-bold text-slate-900">
                  Phê duyệt bảng lương (Cấp {approveLevel === 'Manager' ? 'Quản lý' : 'Giám đốc / CEO'})
                </h3>
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
                  roleLevel: approveLevel,
                  approved: true,
                  comments: approveNotes,
                });
              }}
              className="mt-4 space-y-4"
            >
              <div className="p-3 bg-purple-50 rounded-xl space-y-1.5 text-xs text-purple-900 border border-purple-200">
                <div>Kỳ lương: <span className="font-bold">{period.name}</span></div>
                <div>Tổng chi lương Net: <span className="font-extrabold text-sm">{formatVnd(period.totalNet)}</span></div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Ghi chú phê duyệt</label>
                <textarea
                  value={approveNotes}
                  onChange={(e) => setApproveNotes(e.target.value)}
                  rows={2}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-purple-500"
                  placeholder="Xác nhận phê duyệt bảng lương..."
                />
              </div>

              <div className="p-2.5 bg-amber-50 border border-amber-200 rounded-lg flex items-center gap-2 text-xs text-amber-800">
                <ShieldAlert className="w-4 h-4 text-amber-600 shrink-0" />
                <span>Quy tắc chống tự duyệt: Bạn không thể tự duyệt bảng lương của chính mình.</span>
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
                  className="px-5 py-2 text-sm font-semibold text-white bg-purple-600 rounded-lg hover:bg-purple-700 disabled:opacity-50 transition"
                >
                  {approveMutation.isPending ? 'Đang duyệt...' : 'Xác nhận phê duyệt'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
