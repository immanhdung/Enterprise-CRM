import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { useParams, Link } from 'react-router-dom';
import {
  FileCheck,
  ArrowLeft,
  Sliders,
} from 'lucide-react';
import { PerformanceNavigation } from './PerformanceNavigation';
import { performanceService } from '@/services/performanceService';

export const CommissionPolicyDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();

  const { data: policy, isLoading } = useQuery({
    queryKey: ['commissions', 'policies', id],
    queryFn: () => performanceService.getCommissionPolicyById(id!),
    enabled: !!id,
  });

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  let parsedTiers: Array<{ minScore: number; maxScore: number; rate: number }> = [];
  if (policy?.tierRulesJson) {
    try {
      parsedTiers = JSON.parse(policy.tierRulesJson);
    } catch {
      parsedTiers = [];
    }
  }

  if (isLoading) {
    return (
      <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
        <PerformanceNavigation />
        <div className="p-12 text-center text-slate-400">Đang tải thông tin chính sách hoa hồng...</div>
      </div>
    );
  }

  if (!policy) {
    return (
      <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
        <PerformanceNavigation />
        <div className="p-12 text-center text-slate-400">Không tìm thấy chính sách hoa hồng.</div>
      </div>
    );
  }

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <PerformanceNavigation />

      <div className="p-6 space-y-6 max-w-5xl mx-auto w-full">
        {/* Back Link */}
        <Link
          to="/commissions/policies"
          className="inline-flex items-center gap-1.5 text-xs font-semibold text-slate-600 hover:text-slate-900"
        >
          <ArrowLeft className="w-4 h-4" />
          <span>Quay lại danh sách chính sách hoa hồng</span>
        </Link>

        {/* Header Profile */}
        <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-xs">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 rounded-xl bg-emerald-100 text-emerald-700 flex items-center justify-center font-bold">
                <FileCheck className="w-6 h-6" />
              </div>
              <div>
                <div className="flex items-center gap-2">
                  <h1 className="text-xl font-bold text-slate-900">{policy.name}</h1>
                  <span className="px-2 py-0.5 rounded text-xs font-mono font-bold bg-slate-100 text-slate-700">
                    {policy.code || 'POLICY'}
                  </span>
                  <span
                    className={`px-2 py-0.5 rounded-full text-xs font-semibold ${
                      policy.isActive ? 'bg-emerald-100 text-emerald-800' : 'bg-slate-100 text-slate-500'
                    }`}
                  >
                    {policy.isActive ? 'Đang kích hoạt' : 'Tạm dừng'}
                  </span>
                </div>
                <p className="text-xs text-slate-500 mt-1">{policy.description || 'Chính sách tính toán hoa hồng'}</p>
              </div>
            </div>

            <div className="text-right">
              <div className="text-xs text-slate-400">Phiên bản chính sách</div>
              <div className="text-lg font-bold text-slate-800">Version {policy.version || 1}.0</div>
            </div>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mt-6 pt-6 border-t border-slate-100 text-xs">
            <div>
              <span className="text-slate-400 block mb-1">Cơ sở tính toán:</span>
              <span className="font-bold text-slate-900">{policy.calculationBasis}</span>
            </div>
            <div>
              <span className="text-slate-400 block mb-1">Tỷ lệ mặc định:</span>
              <span className="font-bold text-emerald-600 text-base">{policy.ratePercent ?? policy.rate ?? 0}%</span>
            </div>
            <div>
              <span className="text-slate-400 block mb-1">Ngưỡng tối thiểu kích hoạt:</span>
              <span className="font-semibold text-slate-800">{formatVnd(policy.minThreshold)}</span>
            </div>
            <div>
              <span className="text-slate-400 block mb-1">Trần hoa hồng tối đa:</span>
              <span className="font-semibold text-slate-800">
                {policy.maxCap ? formatVnd(policy.maxCap) : 'Không giới hạn'}
              </span>
            </div>
          </div>
        </div>

        {/* Tier Rules Section */}
        {parsedTiers && parsedTiers.length > 0 && (
          <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-xs">
            <h3 className="font-bold text-sm text-slate-900 mb-3 flex items-center gap-2">
              <Sliders className="w-4 h-4 text-emerald-600" />
              <span>Bảng phân tầng bậc thang theo điểm hoàn thành KPI</span>
            </h3>
            <p className="text-xs text-slate-500 mb-4">
              Tỷ lệ hoa hồng thực nhận của nhân sự sẽ tự động co giãn dựa trên điểm hoàn thành KPI của tháng đó.
            </p>

            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs text-slate-600">
                <thead className="bg-slate-50 font-semibold text-slate-500 uppercase tracking-wider border-b border-slate-200">
                  <tr>
                    <th className="px-4 py-3">Khoảng điểm KPI (%)</th>
                    <th className="px-4 py-3 text-center">Tỷ lệ hoa hồng áp dụng</th>
                    <th className="px-4 py-3">Đánh giá bậc thang</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {parsedTiers.map((tier, idx) => (
                    <tr key={idx} className="hover:bg-slate-50/80">
                      <td className="px-4 py-3 font-semibold text-slate-800">
                        {tier.minScore}% - {tier.maxScore >= 999 ? 'Trở lên' : `${tier.maxScore}%`}
                      </td>
                      <td className="px-4 py-3 text-center">
                        <span className="font-extrabold text-emerald-600 text-sm">{tier.rate}%</span>
                      </td>
                      <td className="px-4 py-3 text-slate-500">
                        {tier.rate === 0
                          ? 'Dưới ngưỡng yêu cầu tối thiểu (0% hoa hồng)'
                          : tier.rate >= 5
                          ? 'Vượt trội xuất sắc (Tỷ lệ thưởng tối đa)'
                          : 'Đạt chỉ tiêu tiêu chuẩn'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
