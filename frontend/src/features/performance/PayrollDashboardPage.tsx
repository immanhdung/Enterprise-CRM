import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  Wallet,
  Plus,
  Calendar,
  CheckCircle2,
  Clock,
  Lock,
  FileCheck,
  ArrowRight,
} from 'lucide-react';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';
import type { CreatePayrollPeriodRequest } from '@/types/performance';

export const PayrollDashboardPage: React.FC = () => {
  const queryClient = useQueryClient();
  const now = new Date();
  const [selectedYear, setSelectedYear] = useState<number>(now.getFullYear());
  const [isModalOpen, setIsModalOpen] = useState(false);

  const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split('T')[0];
  const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0).toISOString().split('T')[0];

  const [form, setForm] = useState<CreatePayrollPeriodRequest>({
    name: `Kỳ lương tháng ${now.getMonth() + 1}/${now.getFullYear()}`,
    startDate: startOfMonth,
    endDate: endOfMonth,
  });

  const { data: periods, isLoading } = useQuery({
    queryKey: ['payroll', 'periods', selectedYear],
    queryFn: () => performanceService.getPayrollPeriods(selectedYear),
  });

  const createMutation = useMutation({
    mutationFn: (req: CreatePayrollPeriodRequest) => performanceService.createPayrollPeriod(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payroll', 'periods'] });
      setIsModalOpen(false);
    },
  });

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Finalized':
      case 'Paid':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-bold bg-purple-100 text-purple-800 border border-purple-200">
            <Lock className="w-3 h-3" /> Đã chốt & Thanh toán
          </span>
        );
      case 'Approved':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-bold bg-emerald-100 text-emerald-800 border border-emerald-200">
            <CheckCircle2 className="w-3 h-3" /> Đã duyệt (Sẵn sàng khóa)
          </span>
        );
      case 'PendingDirectorApproval':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-bold bg-indigo-100 text-indigo-800 border border-indigo-200">
            <Clock className="w-3 h-3" /> Chờ Giám đốc / CEO duyệt
          </span>
        );
      case 'PendingManagerApproval':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-bold bg-amber-100 text-amber-800 border border-amber-200">
            <Clock className="w-3 h-3" /> Chờ Quản lý duyệt
          </span>
        );
      case 'Processing':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-bold bg-blue-100 text-blue-800 border border-blue-200">
            <Clock className="w-3 h-3" /> Đang tính toán
          </span>
        );
      default:
        return (
          <span className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium bg-slate-100 text-slate-700">
            {status}
          </span>
        );
    }
  };

  const totalGrossYear = (periods || []).reduce((acc, p) => acc + (p.totalGross || 0), 0);
  const totalNetYear = (periods || []).reduce((acc, p) => acc + (p.totalNet || 0), 0);
  const totalCommissionYear = (periods || []).reduce((acc, p) => acc + (p.totalCommission || 0), 0);
  const totalBonusesYear = (periods || []).reduce((acc, p) => acc + (p.totalBonuses || 0), 0);

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <PerformanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <div className="flex items-center gap-2">
              <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-purple-100 text-purple-800">
                Tính lương & Chốt thu nhập
              </span>
              <span className="text-xs text-slate-500">Module F – Payroll Management</span>
            </div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight mt-1 flex items-center gap-2">
              <Wallet className="w-7 h-7 text-purple-600" />
              <span>Quản Lý Bảng Lương & Chi Trả Thu Nhập</span>
            </h1>
            <p className="text-sm text-slate-500 mt-1">
              Tập hợp lương cơ bản, hoa hồng đã duyệt, thưởng thành tích, phụ cấp & khấu trừ vào bảng lương có kiểm soát nhiều cấp.
            </p>
          </div>

          <div className="flex items-center gap-2.5">
            <Link
              to="/payroll/approvals"
              className="inline-flex items-center gap-1.5 px-3.5 py-2 text-sm font-semibold text-slate-700 bg-white border border-slate-200 rounded-lg hover:bg-slate-50 shadow-xs transition"
            >
              <FileCheck className="w-4 h-4 text-slate-500" />
              <span>Hộp duyệt bảng lương</span>
            </Link>
            <button
              onClick={() => setIsModalOpen(true)}
              className="inline-flex items-center gap-1.5 px-4 py-2 text-sm font-semibold text-white bg-purple-600 rounded-lg hover:bg-purple-700 shadow-xs transition"
            >
              <Plus className="w-4 h-4" />
              <span>Tạo kỳ lương mới</span>
            </button>
          </div>
        </div>

        {/* Year Metric Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-4 gap-5">
          <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs">
            <div className="text-xs text-slate-500 mb-1">Tổng lương Gross ({selectedYear}):</div>
            <div className="text-2xl font-extrabold text-slate-900">{formatVnd(totalGrossYear)}</div>
            <div className="text-xs text-slate-400 mt-1">Bao gồm lương cứng & hoa hồng</div>
          </div>
          <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs">
            <div className="text-xs text-slate-500 mb-1">Tổng thực lĩnh (Net Pay):</div>
            <div className="text-2xl font-extrabold text-purple-700">{formatVnd(totalNetYear)}</div>
            <div className="text-xs text-slate-400 mt-1">Đã trừ các khoản giảm trừ</div>
          </div>
          <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs">
            <div className="text-xs text-slate-500 mb-1">Tổng hoa hồng đã chi:</div>
            <div className="text-2xl font-extrabold text-emerald-600">{formatVnd(totalCommissionYear)}</div>
            <div className="text-xs text-slate-400 mt-1">Từ các deal chốt thu tiền</div>
          </div>
          <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs">
            <div className="text-xs text-slate-500 mb-1">Tổng tiền thưởng đã chi:</div>
            <div className="text-2xl font-extrabold text-amber-600">{formatVnd(totalBonusesYear)}</div>
            <div className="text-xs text-slate-400 mt-1">Khen thưởng thành tích</div>
          </div>
        </div>

        {/* Year Filter */}
        <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-2 text-slate-600 font-medium text-sm">
              <Calendar className="w-4 h-4 text-slate-400" />
              <span>Năm tài chính:</span>
            </div>
            <select
              value={selectedYear}
              onChange={(e) => setSelectedYear(Number(e.target.value))}
              className="px-3 py-1.5 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-purple-500"
            >
              {[2024, 2025, 2026, 2027].map((y) => (
                <option key={y} value={y}>
                  Năm {y}
                </option>
              ))}
            </select>
          </div>

          <div className="text-xs text-slate-500">
            Hiển thị <span className="font-bold text-slate-800">{periods?.length || 0}</span> kỳ lương trong năm {selectedYear}
          </div>
        </div>

        {/* Periods Table */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
          <div className="p-4 border-b border-slate-100 flex items-center justify-between">
            <h3 className="text-sm font-semibold text-slate-900 flex items-center gap-2">
              <Wallet className="w-4 h-4 text-purple-600" />
              <span>Danh sách các kỳ trả lương</span>
            </h3>
            <span className="text-xs text-slate-400">Quy trình: Tính toán &rarr; Quản lý duyệt &rarr; Giám đốc duyệt &rarr; Khóa sổ</span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-600">
              <thead className="bg-slate-50 text-xs font-semibold text-slate-500 uppercase tracking-wider border-b border-slate-200">
                <tr>
                  <th className="px-5 py-3.5">Kỳ lương</th>
                  <th className="px-5 py-3.5">Thời gian chu kỳ</th>
                  <th className="px-5 py-3.5 text-right">Lương cơ bản</th>
                  <th className="px-5 py-3.5 text-right">Hoa hồng bán hàng</th>
                  <th className="px-5 py-3.5 text-right">Tiền thưởng</th>
                  <th className="px-5 py-3.5 text-right">Tổng thực lĩnh (Net)</th>
                  <th className="px-5 py-3.5 text-center">Trạng thái</th>
                  <th className="px-5 py-3.5 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {isLoading ? (
                  <tr>
                    <td colSpan={8} className="px-5 py-10 text-center text-slate-400">
                      Đang tải danh sách kỳ lương...
                    </td>
                  </tr>
                ) : !periods || periods.length === 0 ? (
                  <tr>
                    <td colSpan={8} className="px-5 py-12 text-center text-slate-400">
                      Chưa có kỳ lương nào trong năm {selectedYear}.
                      <div className="mt-2">
                        <button
                          onClick={() => setIsModalOpen(true)}
                          className="text-purple-600 font-semibold hover:underline text-xs"
                        >
                          + Bấm vào đây để tạo kỳ lương đầu tiên
                        </button>
                      </div>
                    </td>
                  </tr>
                ) : (
                  periods.map((item) => (
                    <tr key={item.id} className="hover:bg-slate-50/80 transition-colors">
                      <td className="px-5 py-3.5 font-bold text-slate-900">
                        {item.name}
                      </td>
                      <td className="px-5 py-3.5 text-xs text-slate-500">
                        {new Date(item.startDate).toLocaleDateString('vi-VN')} &rarr;{' '}
                        {new Date(item.endDate).toLocaleDateString('vi-VN')}
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
                        {getStatusBadge(item.status)}
                      </td>
                      <td className="px-5 py-3.5 text-right">
                        <Link
                          to={`/payroll/periods/${item.id}`}
                          className="inline-flex items-center gap-1 text-xs font-semibold text-purple-600 hover:text-purple-800"
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

      {/* Modal Tạo Kỳ Lương Mới */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <div className="flex items-center gap-2">
                <Wallet className="w-5 h-5 text-purple-600" />
                <h3 className="text-lg font-bold text-slate-900">Tạo kỳ lương mới</h3>
              </div>
              <button
                onClick={() => setIsModalOpen(false)}
                className="text-slate-400 hover:text-slate-600 text-xl font-semibold"
              >
                &times;
              </button>
            </div>

            <form
              onSubmit={(e) => {
                e.preventDefault();
                createMutation.mutate(form);
              }}
              className="mt-4 space-y-4"
            >
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Tên kỳ lương *</label>
                <input
                  type="text"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  required
                  placeholder="VD: Kỳ lương tháng 11/2026"
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-purple-500"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Ngày bắt đầu *</label>
                  <input
                    type="date"
                    value={form.startDate}
                    onChange={(e) => setForm({ ...form, startDate: e.target.value })}
                    required
                    className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-purple-500"
                  />
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Ngày kết thúc *</label>
                  <input
                    type="date"
                    value={form.endDate}
                    onChange={(e) => setForm({ ...form, endDate: e.target.value })}
                    required
                    className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-purple-500"
                  />
                </div>
              </div>

              <div className="p-3 bg-purple-50 border border-purple-200 rounded-xl text-xs text-purple-900 leading-relaxed">
                💡 Sau khi tạo, bạn có thể bấm <strong>"Chạy tính toán bảng lương"</strong> để hệ thống tự động gộp lương cơ bản, các khoản hoa hồng và tiền thưởng đã phê duyệt vào bảng lương.
              </div>

              <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsModalOpen(false)}
                  className="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800"
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  disabled={createMutation.isPending}
                  className="px-5 py-2 text-sm font-semibold text-white bg-purple-600 rounded-lg hover:bg-purple-700 disabled:opacity-50 transition"
                >
                  {createMutation.isPending ? 'Đang tạo...' : 'Tạo kỳ lương'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
