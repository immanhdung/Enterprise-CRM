import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  Award,
  Plus,
  Calendar,
  CheckCircle2,
  FileCheck,
  AlertCircle,
  ShieldAlert,
} from 'lucide-react';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';
import { employeeService } from '@/services/employeeService';
import type {
  BonusRecord,
  CreateBonusRecordRequest,
  ApproveBonusRecordRequest,
} from '@/types/performance';

export const BonusesPage: React.FC = () => {
  const queryClient = useQueryClient();
  const now = new Date();
  const [selectedMonth, setSelectedMonth] = useState<number>(now.getMonth() + 1);
  const [selectedYear, setSelectedYear] = useState<number>(now.getFullYear());

  // Modal Grant Bonus
  const [isGrantModalOpen, setIsGrantModalOpen] = useState(false);
  const [grantForm, setGrantForm] = useState<CreateBonusRecordRequest>({
    bonusPolicyId: '',
    employeeId: '',
    periodMonth: now.getMonth() + 1,
    periodYear: now.getFullYear(),
    amount: 5000000,
    reason: '',
  });

  // Modal Approve Bonus
  const [isApproveModalOpen, setIsApproveModalOpen] = useState(false);
  const [selectedBonusForApprove, setSelectedBonusForApprove] = useState<BonusRecord | null>(null);
  const [approveNotes, setApproveNotes] = useState('');

  const [feedback, setFeedback] = useState<{ type: 'success' | 'error'; message: string } | null>(null);

  // Queries
  const { data: bonusRecords, isLoading } = useQuery({
    queryKey: ['bonuses', 'records', selectedMonth, selectedYear],
    queryFn: () =>
      performanceService.getBonusRecords({
        month: selectedMonth,
        year: selectedYear,
      }),
  });

  const { data: policies } = useQuery({
    queryKey: ['bonuses', 'policies'],
    queryFn: () => performanceService.getBonusPolicies(),
  });

  const { data: employeesResponse } = useQuery({
    queryKey: ['employees'],
    queryFn: () => employeeService.getEmployees(),
  });
  const employees = employeesResponse?.data || [];

  // Mutations
  const grantBonusMutation = useMutation({
    mutationFn: (req: CreateBonusRecordRequest) => performanceService.createBonusRecord(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bonuses', 'records'] });
      setIsGrantModalOpen(false);
      setFeedback({
        type: 'success',
        message: 'Đã trao khoản thưởng thành công cho nhân sự!',
      });
      setTimeout(() => setFeedback(null), 5000);
    },
    onError: (err: any) => {
      setFeedback({
        type: 'error',
        message: err?.response?.data?.message || 'Có lỗi khi tạo khoản thưởng.',
      });
    },
  });

  const approveBonusMutation = useMutation({
    mutationFn: ({ id, req }: { id: string; req: ApproveBonusRecordRequest }) =>
      performanceService.approveBonusRecord(id, req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bonuses', 'records'] });
      setIsApproveModalOpen(false);
      setSelectedBonusForApprove(null);
      setFeedback({
        type: 'success',
        message: 'Đã phê duyệt khoản tiền thưởng thành công!',
      });
      setTimeout(() => setFeedback(null), 5000);
    },
    onError: (err: any) => {
      setFeedback({
        type: 'error',
        message: err?.response?.data?.message || 'Không thể duyệt thưởng (Chống tự duyệt).',
      });
    },
  });

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const totalBonuses = (bonusRecords || []).reduce((acc, b) => acc + (b.amount || 0), 0);

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <PerformanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <div className="flex items-center gap-2">
              <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-amber-100 text-amber-800">
                Thưởng thành tích & Dự án
              </span>
              <span className="text-xs text-slate-500">Module E – Bonus Management</span>
            </div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight mt-1 flex items-center gap-2">
              <Award className="w-6 h-6 text-amber-600" />
              <span>Quản Lý Khen Thưởng & Bonus Thành Tích</span>
            </h1>
            <p className="text-sm text-slate-500 mt-1">
              Trao thưởng Best Seller tháng, thưởng mở khách hàng mới, thưởng vượt chỉ tiêu và tích hợp vào bảng lương.
            </p>
          </div>

          <div className="flex items-center gap-2.5">
            <Link
              to="/bonuses/policies"
              className="inline-flex items-center gap-1.5 px-3.5 py-2 text-sm font-semibold text-slate-700 bg-white border border-slate-200 rounded-lg hover:bg-slate-50 shadow-xs transition"
            >
              <FileCheck className="w-4 h-4 text-slate-500" />
              <span>Chính sách thưởng</span>
            </Link>
            <button
              onClick={() => {
                setGrantForm((prev) => ({
                  ...prev,
                  periodMonth: selectedMonth,
                  periodYear: selectedYear,
                }));
                setIsGrantModalOpen(true);
              }}
              className="inline-flex items-center gap-1.5 px-4 py-2 text-sm font-semibold text-white bg-amber-600 rounded-lg hover:bg-amber-700 shadow-xs transition"
            >
              <Plus className="w-4 h-4" />
              <span>Trao thưởng mới</span>
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

        {/* Quick Summary Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-5">
          <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs">
            <div className="text-xs text-slate-500 mb-1">Tổng tiền thưởng phát sinh trong kỳ:</div>
            <div className="text-2xl font-extrabold text-amber-600">{formatVnd(totalBonuses)}</div>
            <div className="text-xs text-slate-400 mt-1">Tháng {selectedMonth}/{selectedYear}</div>
          </div>
          <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs">
            <div className="text-xs text-slate-500 mb-1">Số khoản khen thưởng:</div>
            <div className="text-2xl font-extrabold text-slate-900">{bonusRecords?.length || 0} khoản</div>
            <div className="text-xs text-slate-400 mt-1">Bao gồm cá nhân & đội ngũ</div>
          </div>
          <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs">
            <div className="text-xs text-slate-500 mb-1">Chính sách thưởng hiệu lực:</div>
            <div className="text-2xl font-extrabold text-blue-600">{policies?.length || 0} chính sách</div>
            <div className="text-xs text-slate-400 mt-1">Top seller, New Customer, Target</div>
          </div>
        </div>

        {/* Period Filter */}
        <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-2 text-slate-600 font-medium text-sm">
              <Calendar className="w-4 h-4 text-slate-400" />
              <span>Kỳ khen thưởng:</span>
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

        {/* Bonuses Table */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
          <div className="p-4 border-b border-slate-100 flex items-center justify-between">
            <h3 className="text-sm font-semibold text-slate-900 flex items-center gap-2">
              <Award className="w-4 h-4 text-amber-600" />
              <span>Danh sách tiền thưởng ghi nhận</span>
            </h3>
            <span className="text-xs text-slate-400">Tự động kết chuyển vào bảng lương khi duyệt</span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-600">
              <thead className="bg-slate-50 text-xs font-semibold text-slate-500 uppercase tracking-wider border-b border-slate-200">
                <tr>
                  <th className="px-5 py-3.5">Nhân viên nhận</th>
                  <th className="px-5 py-3.5">Chính sách thưởng</th>
                  <th className="px-5 py-3.5 text-right">Số tiền thưởng</th>
                  <th className="px-5 py-3.5">Lý do & Căn cứ</th>
                  <th className="px-5 py-3.5 text-center">Trạng thái</th>
                  <th className="px-5 py-3.5 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {isLoading ? (
                  <tr>
                    <td colSpan={6} className="px-5 py-10 text-center text-slate-400">
                      Đang tải danh sách tiền thưởng...
                    </td>
                  </tr>
                ) : !bonusRecords || bonusRecords.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="px-5 py-10 text-center text-slate-400">
                      Chưa có khoản tiền thưởng nào trong tháng {selectedMonth}/{selectedYear}.
                    </td>
                  </tr>
                ) : (
                  bonusRecords.map((item) => (
                    <tr key={item.id} className="hover:bg-slate-50/80 transition-colors">
                      <td className="px-5 py-3.5 font-medium text-slate-900">
                        <div className="flex items-center gap-2">
                          <div className="w-8 h-8 rounded-full bg-amber-100 text-amber-800 font-bold flex items-center justify-center text-xs">
                            {item.employeeName?.substring(0, 2).toUpperCase()}
                          </div>
                          <div>
                            <div>{item.employeeName}</div>
                            <div className="text-xs text-slate-400">{item.employeeCode}</div>
                          </div>
                        </div>
                      </td>
                      <td className="px-5 py-3.5 text-xs font-semibold text-slate-800">
                        {item.bonusPolicyName}
                      </td>
                      <td className="px-5 py-3.5 text-right font-extrabold text-amber-600">
                        {formatVnd(item.amount)}
                      </td>
                      <td className="px-5 py-3.5 text-xs text-slate-600 max-w-xs">
                        {item.reason}
                      </td>
                      <td className="px-5 py-3.5 text-center">
                        <span
                          className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-semibold ${
                            item.status === 'Approved'
                              ? 'bg-emerald-100 text-emerald-800'
                              : item.status === 'Paid'
                              ? 'bg-purple-100 text-purple-800'
                              : 'bg-amber-100 text-amber-800'
                          }`}
                        >
                          {item.status}
                        </span>
                      </td>
                      <td className="px-5 py-3.5 text-right">
                        {(item.status === 'Draft' || item.status === 'PendingApproval') && (
                          <button
                            onClick={() => {
                              setSelectedBonusForApprove(item);
                              setApproveNotes('');
                              setIsApproveModalOpen(true);
                            }}
                            className="inline-flex items-center gap-1 text-xs font-semibold text-emerald-600 hover:text-emerald-800 bg-emerald-50 px-2.5 py-1 rounded-lg"
                          >
                            <CheckCircle2 className="w-3.5 h-3.5" />
                            <span>Duyệt thưởng</span>
                          </button>
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Modal Trao Thưởng Mới */}
      {isGrantModalOpen && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <div className="flex items-center gap-2">
                <Award className="w-5 h-5 text-amber-600" />
                <h3 className="text-lg font-bold text-slate-900">Trao khoản thưởng mới</h3>
              </div>
              <button
                onClick={() => setIsGrantModalOpen(false)}
                className="text-slate-400 hover:text-slate-600 text-xl font-semibold"
              >
                &times;
              </button>
            </div>

            <form
              onSubmit={(e) => {
                e.preventDefault();
                if (!grantForm.employeeId || !grantForm.bonusPolicyId) {
                  alert('Vui lòng chọn đầy đủ nhân viên và chính sách thưởng.');
                  return;
                }
                grantBonusMutation.mutate({
                  ...grantForm,
                  amount: Number(grantForm.amount),
                });
              }}
              className="mt-4 space-y-4"
            >
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Nhân viên nhận thưởng *</label>
                <select
                  value={grantForm.employeeId}
                  onChange={(e) => setGrantForm({ ...grantForm, employeeId: e.target.value })}
                  required
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-amber-500"
                >
                  <option value="">-- Chọn nhân sự --</option>
                  {employees.map((emp) => (
                    <option key={emp.id} value={emp.id}>
                      {emp.fullName} ({emp.departmentName || 'Sales'})
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Chính sách thưởng *</label>
                <select
                  value={grantForm.bonusPolicyId}
                  onChange={(e) => {
                    const pol = (policies || []).find((p) => p.id === e.target.value);
                    setGrantForm({
                      ...grantForm,
                      bonusPolicyId: e.target.value,
                      amount: pol?.bonusAmount || grantForm.amount,
                    });
                  }}
                  required
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-amber-500"
                >
                  <option value="">-- Chọn chính sách thưởng --</option>
                  {(policies || []).map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.name} ({p.type} - {formatVnd(p.bonusAmount)})
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Số tiền thưởng (VND) *</label>
                <input
                  type="number"
                  value={grantForm.amount}
                  onChange={(e) => setGrantForm({ ...grantForm, amount: Number(e.target.value) })}
                  required
                  min={100000}
                  step={500000}
                  className="w-full px-3 py-2 text-sm bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-amber-500"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Lý do & Căn cứ khen thưởng *</label>
                <textarea
                  value={grantForm.reason}
                  onChange={(e) => setGrantForm({ ...grantForm, reason: e.target.value })}
                  required
                  rows={2}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-amber-500"
                  placeholder="VD: Đạt Best Seller tháng, mở mới 3 khách hàng lớn..."
                />
              </div>

              <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsGrantModalOpen(false)}
                  className="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800"
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  disabled={grantBonusMutation.isPending}
                  className="px-5 py-2 text-sm font-semibold text-white bg-amber-600 rounded-lg hover:bg-amber-700 disabled:opacity-50 transition"
                >
                  {grantBonusMutation.isPending ? 'Đang lưu...' : 'Xác nhận trao thưởng'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Duyệt Thưởng (Anti-Self-Approval) */}
      {isApproveModalOpen && selectedBonusForApprove && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <div className="flex items-center gap-2">
                <CheckCircle2 className="w-5 h-5 text-emerald-600" />
                <h3 className="text-lg font-bold text-slate-900">Phê duyệt tiền thưởng</h3>
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
                approveBonusMutation.mutate({
                  id: selectedBonusForApprove.id,
                  req: {
                    approved: true,
                    notes: approveNotes,
                  },
                });
              }}
              className="mt-4 space-y-4"
            >
              <div className="p-3 bg-slate-50 rounded-xl space-y-1.5 text-xs text-slate-600 border border-slate-200">
                <div>Nhân sự: <span className="font-bold text-slate-900">{selectedBonusForApprove.employeeName}</span></div>
                <div>Tiền thưởng: <span className="font-extrabold text-amber-600 text-sm">{formatVnd(selectedBonusForApprove.amount)}</span></div>
                <div>Lý do: <span className="text-slate-800">{selectedBonusForApprove.reason}</span></div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Ghi chú phê duyệt</label>
                <textarea
                  value={approveNotes}
                  onChange={(e) => setApproveNotes(e.target.value)}
                  rows={2}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-emerald-500"
                  placeholder="Ghi chú xác nhận duyệt thưởng..."
                />
              </div>

              <div className="p-2.5 bg-amber-50 border border-amber-200 rounded-lg flex items-center gap-2 text-xs text-amber-800">
                <ShieldAlert className="w-4 h-4 text-amber-600 shrink-0" />
                <span>Quy tắc chống tự duyệt: Bạn không thể tự phê duyệt tiền thưởng của chính mình.</span>
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
                  disabled={approveBonusMutation.isPending}
                  className="px-5 py-2 text-sm font-semibold text-white bg-emerald-600 rounded-lg hover:bg-emerald-700 disabled:opacity-50 transition"
                >
                  {approveBonusMutation.isPending ? 'Đang duyệt...' : 'Xác nhận duyệt thưởng'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
