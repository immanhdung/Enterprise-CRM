import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Award,
  Plus,
} from 'lucide-react';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';
import type { CreateBonusPolicyRequest } from '@/types/performance';

export const BonusPoliciesPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [form, setForm] = useState<CreateBonusPolicyRequest>({
    name: '',
    type: 'Individual',
    period: 'Monthly',
    targetThreshold: 100000000,
    bonusAmount: 5000000,
    description: '',
  });

  const { data: policies, isLoading } = useQuery({
    queryKey: ['bonuses', 'policies'],
    queryFn: () => performanceService.getBonusPolicies(),
  });

  const createMutation = useMutation({
    mutationFn: (req: CreateBonusPolicyRequest) => performanceService.createBonusPolicy(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bonuses', 'policies'] });
      setIsModalOpen(false);
      setForm({
        name: '',
        type: 'Individual',
        period: 'Monthly',
        targetThreshold: 100000000,
        bonusAmount: 5000000,
        description: '',
      });
    },
  });

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
              <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-amber-100 text-amber-800">
                Danh mục chính sách
              </span>
              <span className="text-xs text-slate-500">Module E – Bonus Policies</span>
            </div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight mt-1 flex items-center gap-2">
              <Award className="w-6 h-6 text-amber-600" />
              <span>Chính Sách Tiền Thưởng (Bonus Catalog)</span>
            </h1>
            <p className="text-sm text-slate-500 mt-1">
              Quy định các gói khen thưởng theo chu kỳ tháng/quý/năm, điều kiện đạt ngưỡng và giá trị định mức.
            </p>
          </div>

          <div className="flex items-center gap-2.5">
            <button
              onClick={() => setIsModalOpen(true)}
              className="inline-flex items-center gap-1.5 px-4 py-2 text-sm font-semibold text-white bg-amber-600 rounded-lg hover:bg-amber-700 shadow-xs transition"
            >
              <Plus className="w-4 h-4" />
              <span>Tạo chính sách thưởng mới</span>
            </button>
          </div>
        </div>

        {/* Policies Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {isLoading ? (
            <div className="col-span-full py-12 text-center text-slate-400">
              Đang tải danh sách chính sách thưởng...
            </div>
          ) : !policies || policies.length === 0 ? (
            <div className="col-span-full py-12 text-center text-slate-400 bg-white rounded-2xl border border-slate-200">
              Chưa có chính sách thưởng nào được thiết lập.
            </div>
          ) : (
            policies.map((p) => (
              <div
                key={p.id}
                className="bg-white rounded-2xl border border-slate-200 p-6 shadow-xs flex flex-col justify-between hover:shadow-md transition-shadow relative overflow-hidden"
              >
                <div className={`absolute top-0 inset-x-0 h-1.5 ${p.isActive ? 'bg-amber-500' : 'bg-slate-300'}`} />

                <div>
                  <div className="flex items-center justify-between mb-2">
                    <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-amber-50 text-amber-800 border border-amber-200">
                      {p.type}
                    </span>
                    <span className="text-xs text-slate-400 font-medium">Chu kỳ: {p.period}</span>
                  </div>

                  <h3 className="font-bold text-base text-slate-900 mb-1">{p.name}</h3>
                  <p className="text-xs text-slate-500 mb-4">{p.description || 'Chính sách tiền thưởng theo thành tích.'}</p>

                  <div className="space-y-2 text-xs bg-slate-50 p-3.5 rounded-xl border border-slate-100 mb-4">
                    <div className="flex justify-between">
                      <span className="text-slate-500">Mức tiền thưởng:</span>
                      <span className="font-extrabold text-amber-600 text-sm">{formatVnd(p.bonusAmount)}</span>
                    </div>
                    {p.targetThreshold && (
                      <div className="flex justify-between">
                        <span className="text-slate-500">Ngưỡng đạt yêu cầu:</span>
                        <span className="font-semibold text-slate-800">{formatVnd(p.targetThreshold)}</span>
                      </div>
                    )}
                  </div>
                </div>

                <div className="pt-3 border-t border-slate-100 flex items-center justify-between text-xs text-slate-400">
                  <span>Trạng thái: {p.isActive ? '✅ Đang áp dụng' : 'Tạm dừng'}</span>
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      {/* Modal Tạo Chính Sách Thưởng Mới */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <div className="flex items-center gap-2">
                <Award className="w-5 h-5 text-amber-600" />
                <h3 className="text-lg font-bold text-slate-900">Tạo chính sách thưởng mới</h3>
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
                createMutation.mutate({
                  ...form,
                  targetThreshold: Number(form.targetThreshold),
                  bonusAmount: Number(form.bonusAmount),
                });
              }}
              className="mt-4 space-y-4"
            >
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Tên chính sách thưởng *</label>
                <input
                  type="text"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  required
                  placeholder="VD: Thưởng Top 1 Doanh Số Tháng"
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-amber-500"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Loại hình khen thưởng *</label>
                <select
                  value={form.type}
                  onChange={(e) => setForm({ ...form, type: e.target.value })}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-amber-500"
                >
                  <option value="Individual">Cá nhân xuất sắc (Individual)</option>
                  <option value="Team">Đội ngũ / Nhóm xuất sắc (Team)</option>
                  <option value="CustomerAcquisition">Phát triển khách hàng mới (Customer Acquisition)</option>
                  <option value="RevenueTarget">Đạt mốc doanh số lớn (Revenue Target)</option>
                  <option value="SpecialAchievement">Thành tích đặc biệt (Special Achievement)</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Chu kỳ áp dụng *</label>
                <select
                  value={form.period}
                  onChange={(e) => setForm({ ...form, period: e.target.value })}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-amber-500"
                >
                  <option value="Monthly">Hàng tháng (Monthly)</option>
                  <option value="Quarterly">Hàng quý (Quarterly)</option>
                  <option value="Yearly">Hàng năm (Yearly)</option>
                  <option value="AdHoc">Đột xuất (Ad-Hoc)</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Số tiền thưởng định mức (VND) *</label>
                <input
                  type="number"
                  value={form.bonusAmount}
                  onChange={(e) => setForm({ ...form, bonusAmount: Number(e.target.value) })}
                  required
                  min={100000}
                  step={500000}
                  className="w-full px-3 py-2 text-sm bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-amber-500"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Ngưỡng doanh số kích hoạt (VND)</label>
                <input
                  type="number"
                  value={form.targetThreshold}
                  onChange={(e) => setForm({ ...form, targetThreshold: Number(e.target.value) })}
                  min={0}
                  step={10000000}
                  className="w-full px-3 py-2 text-sm bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-amber-500"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Mô tả tiêu chuẩn khen thưởng</label>
                <textarea
                  value={form.description}
                  onChange={(e) => setForm({ ...form, description: e.target.value })}
                  rows={2}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-amber-500"
                  placeholder="Tiêu chí để đạt được phần thưởng..."
                />
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
                  className="px-5 py-2 text-sm font-semibold text-white bg-amber-600 rounded-lg hover:bg-amber-700 disabled:opacity-50 transition"
                >
                  {createMutation.isPending ? 'Đang tạo...' : 'Tạo chính sách'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
