import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  FileCheck,
  Plus,
  ArrowRight,
} from 'lucide-react';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';
import type { CreateCommissionPolicyRequest } from '@/types/performance';

export const CommissionPoliciesPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [form, setForm] = useState<CreateCommissionPolicyRequest>({
    name: '',
    code: '',
    policyType: 'Standard',
    calculationBasis: 'CollectedCash',
    customerType: 'All',
    ratePercent: 3,
    minThreshold: 0,
    maxCap: 50000000,
    description: '',
    effectiveFrom: new Date().toISOString().split('T')[0],
    tierRulesJson: JSON.stringify(
      [
        { minScore: 0, maxScore: 69, rate: 0 },
        { minScore: 70, maxScore: 99, rate: 2 },
        { minScore: 100, maxScore: 119, rate: 3 },
        { minScore: 120, maxScore: 999, rate: 5 },
      ],
      null,
      2
    ),
  });

  const { data: policies, isLoading } = useQuery({
    queryKey: ['commissions', 'policies'],
    queryFn: () => performanceService.getCommissionPolicies(),
  });

  const createMutation = useMutation({
    mutationFn: (req: CreateCommissionPolicyRequest) =>
      performanceService.createCommissionPolicy(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['commissions', 'policies'] });
      setIsModalOpen(false);
      setForm({
        name: '',
        code: '',
        policyType: 'Standard',
        calculationBasis: 'CollectedCash',
        customerType: 'All',
        ratePercent: 3,
        minThreshold: 0,
        maxCap: 50000000,
        description: '',
        effectiveFrom: new Date().toISOString().split('T')[0],
        tierRulesJson: '',
      });
    },
  });

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const getBasisLabel = (basis: string) => {
    switch (basis) {
      case 'CollectedCash':
        return 'Tiền mặt thực thu (Confirmed Cash)';
      case 'ContractValue':
        return 'Tổng giá trị hợp đồng ký kết';
      case 'TargetTierBased':
        return 'Phân tầng theo điểm KPI đạt được';
      case 'CustomerTypeBased':
        return 'Phân loại Khách hàng (Mới vs Cũ)';
      default:
        return basis;
    }
  };

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <PerformanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <div className="flex items-center gap-2">
              <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-100 text-emerald-800">
                Cấu hình chính sách
              </span>
              <span className="text-xs text-slate-500">Module C – Commission Policy Management</span>
            </div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight mt-1 flex items-center gap-2">
              <FileCheck className="w-6 h-6 text-emerald-600" />
              <span>Chính Sách Hoa Hồng Kinh Doanh</span>
            </h1>
            <p className="text-sm text-slate-500 mt-1">
              Thiết lập tỷ lệ %, trần hoa hồng tối đa, điều kiện kích hoạt và quy tắc phân tầng bậc thang theo kết quả KPI.
            </p>
          </div>

          <div className="flex items-center gap-2.5">
            <button
              onClick={() => setIsModalOpen(true)}
              className="inline-flex items-center gap-1.5 px-4 py-2 text-sm font-semibold text-white bg-emerald-600 rounded-lg hover:bg-emerald-700 shadow-xs transition"
            >
              <Plus className="w-4 h-4" />
              <span>Tạo chính sách mới</span>
            </button>
          </div>
        </div>

        {/* Policies Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {isLoading ? (
            <div className="col-span-full py-12 text-center text-slate-400">
              Đang tải danh sách chính sách hoa hồng...
            </div>
          ) : !policies || policies.length === 0 ? (
            <div className="col-span-full py-12 text-center text-slate-400 bg-white rounded-2xl border border-slate-200">
              Chưa có chính sách nào. Bấm vào "Tạo chính sách mới" để bắt đầu.
            </div>
          ) : (
            policies.map((p) => (
              <div
                key={p.id}
                className="bg-white rounded-2xl border border-slate-200 p-6 shadow-xs flex flex-col justify-between hover:shadow-md transition-shadow relative overflow-hidden"
              >
                <div className={`absolute top-0 inset-x-0 h-1.5 ${p.isActive ? 'bg-emerald-500' : 'bg-slate-300'}`} />

                <div>
                  <div className="flex items-center justify-between mb-2">
                    <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-slate-100 text-slate-700">
                      {p.code || 'POL-CONFIG'}
                    </span>
                    <span
                      className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-semibold ${
                        p.isActive ? 'bg-emerald-100 text-emerald-800' : 'bg-slate-100 text-slate-500'
                      }`}
                    >
                      {p.isActive ? 'Đang áp dụng' : 'Tạm dừng'}
                    </span>
                  </div>

                  <h3 className="font-bold text-base text-slate-900 mb-1">{p.name}</h3>
                  <p className="text-xs text-slate-500 line-clamp-2 mb-4">
                    {p.description || 'Chính sách tính hoa hồng kinh doanh tự động.'}
                  </p>

                  <div className="space-y-2 text-xs bg-slate-50 p-3.5 rounded-xl border border-slate-100 mb-4">
                    <div className="flex justify-between">
                      <span className="text-slate-500">Cơ sở tính toán:</span>
                      <span className="font-semibold text-slate-800 text-right">{getBasisLabel(p.calculationBasis)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-slate-500">Tỷ lệ cơ bản:</span>
                      <span className="font-bold text-emerald-600 text-sm">{p.ratePercent ?? p.rate ?? 0}%</span>
                    </div>
                    {p.maxCap && (
                      <div className="flex justify-between">
                        <span className="text-slate-500">Trần tối đa / khoản:</span>
                        <span className="font-semibold text-slate-800">{formatVnd(p.maxCap)}</span>
                      </div>
                    )}
                    <div className="flex justify-between pt-1 border-t border-slate-200">
                      <span className="text-slate-400">Phiên bản:</span>
                      <span className="font-bold text-slate-600">v{p.version || 1}.0</span>
                    </div>
                  </div>
                </div>

                <div className="pt-3 border-t border-slate-100 flex items-center justify-between">
                  <span className="text-xs text-slate-400">
                    {p.tierRulesJson ? '⚡ Có phân tầng KPI' : 'Cố định %'}
                  </span>
                  <Link
                    to={`/commissions/policies/${p.id}`}
                    className="inline-flex items-center gap-1 text-xs font-semibold text-blue-600 hover:text-blue-800"
                  >
                    <span>Chi tiết quy tắc</span>
                    <ArrowRight className="w-3.5 h-3.5" />
                  </Link>
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      {/* Modal Tạo Chính Sách Hoa Hồng Mới */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-xl w-full p-6 shadow-2xl border border-slate-100 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between pb-4 border-b border-slate-100">
              <div className="flex items-center gap-2">
                <FileCheck className="w-5 h-5 text-emerald-600" />
                <h3 className="text-lg font-bold text-slate-900">Tạo chính sách hoa hồng mới</h3>
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
                  ratePercent: Number(form.ratePercent || 0),
                  minThreshold: Number(form.minThreshold || 0),
                  maxCap: form.maxCap ? Number(form.maxCap) : undefined,
                  policyType: form.policyType || 'Standard',
                  effectiveFrom: form.effectiveFrom || new Date().toISOString().split('T')[0],
                });
              }}
              className="mt-4 space-y-4"
            >
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Mã chính sách *</label>
                  <input
                    type="text"
                    value={form.code}
                    onChange={(e) => setForm({ ...form, code: e.target.value.toUpperCase() })}
                    required
                    placeholder="VD: COMM_CASH_TIER"
                    className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-emerald-500 font-mono"
                  />
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Tên chính sách *</label>
                  <input
                    type="text"
                    value={form.name}
                    onChange={(e) => setForm({ ...form, name: e.target.value })}
                    required
                    placeholder="VD: Hoa hồng thực thu bậc thang KPI"
                    className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-emerald-500"
                  />
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Cơ sở tính toán *</label>
                  <select
                    value={form.calculationBasis}
                    onChange={(e) => setForm({ ...form, calculationBasis: e.target.value })}
                    className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-emerald-500"
                  >
                    <option value="CollectedCash">Tiền mặt thực thu (Collected Cash)</option>
                    <option value="ContractValue">Giá trị hợp đồng ký (Contract Value)</option>
                    <option value="TargetTierBased">Phân tầng theo điểm KPI đạt được</option>
                    <option value="CustomerTypeBased">Phân loại khách hàng (Mới / Hiện hữu)</option>
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Tỷ lệ cơ bản (%) *</label>
                  <input
                    type="number"
                    value={form.ratePercent || 0}
                    onChange={(e) => setForm({ ...form, ratePercent: Number(e.target.value) })}
                    required
                    min={0}
                    max={100}
                    step={0.1}
                    className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-emerald-500"
                  />
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">
                    Ngưỡng doanh thu tối thiểu để kích hoạt (VND)
                  </label>
                  <input
                    type="number"
                    value={form.minThreshold}
                    onChange={(e) => setForm({ ...form, minThreshold: Number(e.target.value) })}
                    min={0}
                    step={1000000}
                    className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-emerald-500"
                  />
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">
                    Mức trần hoa hồng tối đa / khoản (VND)
                  </label>
                  <input
                    type="number"
                    value={form.maxCap || ''}
                    onChange={(e) => setForm({ ...form, maxCap: Number(e.target.value) })}
                    min={0}
                    step={1000000}
                    placeholder="Không giới hạn nếu để trống"
                    className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-emerald-500"
                  />
                </div>
              </div>

              {form.calculationBasis === 'TargetTierBased' && (
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">
                    Cấu hình quy tắc phân tầng bậc thang theo điểm KPI (JSON)
                  </label>
                  <textarea
                    value={form.tierRulesJson}
                    onChange={(e) => setForm({ ...form, tierRulesJson: e.target.value })}
                    rows={5}
                    className="w-full px-3 py-2 text-xs font-mono bg-slate-900 text-emerald-400 border border-slate-700 rounded-lg focus:ring-2 focus:ring-emerald-500"
                  />
                  <p className="text-[11px] text-slate-500 mt-1">
                    Ví dụ: Điểm &lt;70% hưởng 0%, 70-99% hưởng 2%, 100-119% hưởng 3%, &ge;120% hưởng 5%.
                  </p>
                </div>
              )}

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Mô tả chính sách</label>
                <textarea
                  value={form.description}
                  onChange={(e) => setForm({ ...form, description: e.target.value })}
                  rows={2}
                  className="w-full px-3 py-2 text-sm bg-slate-50 border border-slate-200 rounded-lg focus:ring-2 focus:ring-emerald-500"
                  placeholder="Ghi chú điều kiện áp dụng..."
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
                  className="px-5 py-2 text-sm font-semibold text-white bg-emerald-600 rounded-lg hover:bg-emerald-700 disabled:opacity-50 transition"
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
