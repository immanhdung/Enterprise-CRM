import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  FileCheck,
  CheckCircle2,
  Clock,
  ShieldAlert,
} from 'lucide-react';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';
import type { PayrollPeriod, ApprovePayrollRequest } from '@/types/performance';

export const PayrollApprovalsPage: React.FC = () => {
  const queryClient = useQueryClient();
  const now = new Date();
  const [selectedYear] = useState<number>(now.getFullYear());

  const [selectedPeriodForApprove, setSelectedPeriodForApprove] = useState<PayrollPeriod | null>(null);
  const [approveLevel, setApproveLevel] = useState<'Manager' | 'Director'>('Manager');
  const [approveNotes, setApproveNotes] = useState('');
  const [feedback, setFeedback] = useState<{ type: 'success' | 'error'; message: string } | null>(null);

  const { data: periods, isLoading } = useQuery({
    queryKey: ['payroll', 'periods', selectedYear],
    queryFn: () => performanceService.getPayrollPeriods(selectedYear),
  });

  const approveMutation = useMutation({
    mutationFn: ({ id, req }: { id: string; req: ApprovePayrollRequest }) =>
      performanceService.approvePayrollPeriod(id, req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payroll', 'periods'] });
      setSelectedPeriodForApprove(null);
      setFeedback({
        type: 'success',
        message: 'Đã phê duyệt kỳ lương thành công!',
      });
      setTimeout(() => setFeedback(null), 5000);
    },
    onError: (err: any) => {
      setFeedback({
        type: 'error',
        message: err?.response?.data?.message || 'Không thể phê duyệt (Chống tự duyệt).',
      });
    },
  });

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const pendingPeriods = (periods || []).filter(
    (p) => p.status === 'PendingManagerApproval' || p.status === 'PendingDirectorApproval' || p.status === 'Processing'
  );

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <PerformanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <div className="flex items-center gap-2">
              <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-indigo-100 text-indigo-800">
                Trung tâm phê duyệt
              </span>
              <span className="text-xs text-slate-500">Module G – Approval Workflows</span>
            </div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight mt-1 flex items-center gap-2">
              <FileCheck className="w-6 h-6 text-indigo-600" />
              <span>Hộp Duyệt Bảng Lương & Chi Trả Thu Nhập</span>
            </h1>
            <p className="text-sm text-slate-500 mt-1">
              Phê duyệt 2 cấp: Trưởng bộ phận / Quản lý kiểm tra & Ban Giám Đốc / CEO phê duyệt giải ngân.
            </p>
          </div>
        </div>

        {/* Feedback alert */}
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
              <ShieldAlert className="w-5 h-5 text-rose-600 shrink-0" />
            )}
            <span>{feedback.message}</span>
          </div>
        )}

        {/* Pending Approval List */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
          <div className="p-4 border-b border-slate-100 flex items-center justify-between">
            <h3 className="text-sm font-semibold text-slate-900 flex items-center gap-2">
              <Clock className="w-4 h-4 text-amber-500" />
              <span>Các kỳ lương đang chờ phê duyệt</span>
            </h3>
            <span className="text-xs text-slate-400">Hiển thị {pendingPeriods.length} kỳ lương</span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-600">
              <thead className="bg-slate-50 text-xs font-semibold text-slate-500 uppercase tracking-wider border-b border-slate-200">
                <tr>
                  <th className="px-5 py-3.5">Kỳ lương</th>
                  <th className="px-5 py-3.5 text-right">Lương cứng</th>
                  <th className="px-5 py-3.5 text-right">Hoa hồng bán hàng</th>
                  <th className="px-5 py-3.5 text-right">Tiền thưởng</th>
                  <th className="px-5 py-3.5 text-right">Tổng thực lĩnh (Net)</th>
                  <th className="px-5 py-3.5 text-center">Cấp duyệt hiện tại</th>
                  <th className="px-5 py-3.5 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {isLoading ? (
                  <tr>
                    <td colSpan={7} className="px-5 py-10 text-center text-slate-400">
                      Đang tải danh sách chờ duyệt...
                    </td>
                  </tr>
                ) : pendingPeriods.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="px-5 py-12 text-center text-slate-400">
                      ✅ Hiện không có kỳ lương nào đang chờ duyệt.
                    </td>
                  </tr>
                ) : (
                  pendingPeriods.map((item) => (
                    <tr key={item.id} className="hover:bg-slate-50/80 transition-colors">
                      <td className="px-5 py-3.5 font-bold text-slate-900">
                        {item.name}
                      </td>
                      <td className="px-5 py-3.5 text-right font-medium text-slate-800">
                        {formatVnd(item.totalBaseSalary)}
                      </td>
                      <td className="px-5 py-3.5 text-right font-semibold text-emerald-600">
                        {formatVnd(item.totalCommission)}
                      </td>
                      <td className="px-5 py-3.5 text-right font-semibold text-amber-600">
                        {formatVnd(item.totalBonuses)}
                      </td>
                      <td className="px-5 py-3.5 text-right font-extrabold text-purple-700 text-base">
                        {formatVnd(item.totalNet)}
                      </td>
                      <td className="px-5 py-3.5 text-center">
                        <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-bold bg-amber-100 text-amber-800">
                          <Clock className="w-3 h-3" />
                          {item.status === 'PendingDirectorApproval'
                            ? 'Cần Giám đốc / CEO duyệt'
                            : 'Cần Quản lý duyệt'}
                        </span>
                      </td>
                      <td className="px-5 py-3.5 text-right">
                        <div className="flex items-center justify-end gap-2">
                          <Link
                            to={`/payroll/periods/${item.id}`}
                            className="text-xs font-semibold text-slate-600 hover:text-slate-900 px-2.5 py-1 rounded-lg bg-slate-100 hover:bg-slate-200"
                          >
                            Xem chi tiết
                          </Link>
                          <button
                            onClick={() => {
                              setSelectedPeriodForApprove(item);
                              setApproveLevel(
                                item.status === 'PendingDirectorApproval' ? 'Director' : 'Manager'
                              );
                              setApproveNotes('');
                            }}
                            className="inline-flex items-center gap-1 text-xs font-semibold text-white bg-indigo-600 hover:bg-indigo-700 px-3 py-1 rounded-lg shadow-xs"
                          >
                            <CheckCircle2 className="w-3.5 h-3.5" />
                            <span>Phê duyệt</span>
                          </button>
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

      {/* Modal Duyệt Nhanh */}
      {selectedPeriodForApprove && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <div className="flex items-center gap-2">
                <CheckCircle2 className="w-5 h-5 text-indigo-600" />
                <h3 className="text-lg font-bold text-slate-900">
                  Xác nhận duyệt ({approveLevel === 'Manager' ? 'Cấp Quản lý' : 'Cấp Giám đốc / CEO'})
                </h3>
              </div>
              <button
                onClick={() => setSelectedPeriodForApprove(null)}
                className="text-slate-400 hover:text-slate-600 text-xl font-semibold"
              >
                &times;
              </button>
            </div>

            <form
              onSubmit={(e) => {
                e.preventDefault();
                approveMutation.mutate({
                  id: selectedPeriodForApprove.id,
                  req: {
                    roleLevel: approveLevel,
                    approved: true,
                    comments: approveNotes,
                  },
                });
              }}
              className="mt-4 space-y-4"
            >
              <div className="p-3 bg-indigo-50 rounded-xl space-y-1 text-xs text-indigo-900 border border-indigo-200">
                <div>Kỳ lương: <span className="font-bold">{selectedPeriodForApprove.name}</span></div>
                <div>Số tiền thực lĩnh: <span className="font-extrabold text-sm">{formatVnd(selectedPeriodForApprove.totalNet)}</span></div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Ghi chú xác nhận</label>
                <textarea
                  value={approveNotes}
                  onChange={(e) => setApproveNotes(e.target.value)}
                  rows={2}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-indigo-500"
                  placeholder="Ghi chú duyệt..."
                />
              </div>

              <div className="p-2.5 bg-amber-50 border border-amber-200 rounded-lg flex items-center gap-2 text-xs text-amber-800">
                <ShieldAlert className="w-4 h-4 text-amber-600 shrink-0" />
                <span>Quy tắc chống tự duyệt: Hệ thống kiểm tra ID tài khoản và từ chối nếu bạn tự duyệt bảng lương của chính mình.</span>
              </div>

              <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setSelectedPeriodForApprove(null)}
                  className="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800"
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  disabled={approveMutation.isPending}
                  className="px-5 py-2 text-sm font-semibold text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition"
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
