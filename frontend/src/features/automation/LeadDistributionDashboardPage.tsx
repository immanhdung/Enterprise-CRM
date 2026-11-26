import React, { useState, useEffect } from 'react';
import { AutomationNavigation } from './AutomationNavigation';
import { automationService } from '../../services/automationService';
import type { LeadDistributionMetrics, LeadAssignmentRule, LeadAssignmentLog } from '../../types/automation';
import { Users, UserCheck, AlertCircle, RefreshCw, Plus, ShieldCheck } from 'lucide-react';

export const LeadDistributionDashboardPage: React.FC = () => {
  const [metrics, setMetrics] = useState<LeadDistributionMetrics | null>(null);
  const [rules, setRules] = useState<LeadAssignmentRule[]>([]);
  const [logs, setLogs] = useState<LeadAssignmentLog[]>([]);
  const [loading, setLoading] = useState(true);

  // Modal for new rule
  const [showAddRule, setShowAddRule] = useState(false);
  const [ruleName, setRuleName] = useState('');
  const [ruleMode, setRuleMode] = useState('RoundRobin');
  const [rulePriority, setRulePriority] = useState(1);
  const [ruleMaxCapacity, setRuleMaxCapacity] = useState<number | undefined>(50);

  const loadAllData = async () => {
    try {
      setLoading(true);
      const [m, r, l] = await Promise.all([
        automationService.getLeadDistributionMetrics(),
        automationService.getLeadRules(),
        automationService.getAssignmentLogs(undefined, 20),
      ]);
      setMetrics(m);
      setRules(r);
      setLogs(l);
    } catch (err) {
      console.error('Failed to load lead distribution data:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadAllData();
  }, []);

  const handleCreateRule = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!ruleName.trim()) return;

    try {
      await automationService.createLeadRule({
        name: ruleName.trim(),
        description: `Quy tắc phân bổ theo ${ruleMode}`,
        priority: rulePriority,
        mode: ruleMode,
        maxActiveLeadsPerRep: ruleMaxCapacity,
        isActive: true,
      });
      setShowAddRule(false);
      setRuleName('');
      loadAllData();
    } catch (err: any) {
      alert(`Lỗi: ${err?.message || 'Không thể tạo quy tắc'}`);
    }
  };

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Phân phối Lead Tự động (Lead Distribution Engine)</h1>
          <p className="text-sm text-gray-500">
            Hệ thống định tuyến thông minh: Round-Robin, Tải công việc (Workload), Vùng miền và Chuyên môn dịch vụ
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={() => setShowAddRule(true)}
            className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-3 py-1.5 rounded-lg text-sm font-medium transition-colors shadow-sm"
          >
            <Plus className="w-4 h-4" /> Thêm Quy tắc
          </button>
          <button
            onClick={loadAllData}
            disabled={loading}
            className="flex items-center gap-2 px-3 py-1.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
          </button>
        </div>
      </div>

      <AutomationNavigation />

      {/* KPI Stat Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <div className="bg-white p-5 rounded-lg border border-gray-200 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider">Phân bổ hôm nay</span>
            <UserCheck className="w-5 h-5 text-green-600" />
          </div>
          <div className="text-2xl font-bold text-gray-900 mt-2">
            {metrics?.totalAssignedToday ?? 0}
          </div>
          <div className="text-xs text-gray-500 mt-1">Lead được gắn cho sales</div>
        </div>

        <div className="bg-white p-5 rounded-lg border border-gray-200 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider">Phân bổ trong tháng</span>
            <Users className="w-5 h-5 text-blue-600" />
          </div>
          <div className="text-2xl font-bold text-gray-900 mt-2">
            {metrics?.totalAssignedThisMonth ?? 0}
          </div>
          <div className="text-xs text-gray-500 mt-1">Tổng lượt điều phối tự động</div>
        </div>

        <div className="bg-white p-5 rounded-lg border border-gray-200 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider">Lead chưa phân công</span>
            <AlertCircle className="w-5 h-5 text-amber-500" />
          </div>
          <div className="text-2xl font-bold text-amber-600 mt-2">
            {metrics?.totalUnassignedLeads ?? 0}
          </div>
          <div className="text-xs text-gray-500 mt-1">Đang chờ router xử lý</div>
        </div>

        <div className="bg-white p-5 rounded-lg border border-gray-200 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider">Quy tắc hoạt động</span>
            <ShieldCheck className="w-5 h-5 text-purple-600" />
          </div>
          <div className="text-2xl font-bold text-purple-700 mt-2">
            {rules.filter((r) => r.isActive).length}
          </div>
          <div className="text-xs text-gray-500 mt-1">Cấu hình định tuyến ưu tiên</div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
        {/* Sales Representatives Workload & Capacity */}
        <div className="lg:col-span-2 bg-white p-5 rounded-lg border border-gray-200 shadow-sm">
          <h2 className="text-base font-semibold text-gray-900 mb-4 flex items-center justify-between">
            <span>Công suất Phục vụ Sales Reps (Workload Capacity)</span>
            <span className="text-xs font-normal text-gray-500">Giới hạn định mức: 50 Lead/nhân sự</span>
          </h2>

          {!metrics?.repWorkloads || metrics.repWorkloads.length === 0 ? (
            <p className="text-sm text-gray-500 italic">Chưa có dữ liệu tải công việc nhân viên.</p>
          ) : (
            <div className="space-y-4">
              {metrics.repWorkloads.map((rep) => (
                <div key={rep.employeeId} className="space-y-1 text-sm">
                  <div className="flex justify-between items-center text-xs">
                    <div className="flex items-center gap-2">
                      <span className="font-semibold text-gray-900">{rep.employeeName}</span>
                      <span className="text-gray-400">({rep.employeeCode})</span>
                      {rep.teamName && (
                        <span className="text-[11px] bg-gray-100 text-gray-600 px-1.5 py-0.5 rounded">
                          {rep.teamName}
                        </span>
                      )}
                    </div>
                    <div className="flex items-center gap-2">
                      <span className="font-medium text-gray-700">
                        {rep.activeLeadsCount} / {rep.maxCapacity} leads
                      </span>
                      <span
                        className={`font-semibold ${
                          rep.capacityUtilizationPercent > 80
                            ? 'text-red-600'
                            : rep.capacityUtilizationPercent > 50
                            ? 'text-amber-600'
                            : 'text-green-600'
                        }`}
                      >
                        ({rep.capacityUtilizationPercent}%)
                      </span>
                    </div>
                  </div>

                  <div className="w-full bg-gray-100 rounded-full h-2.5 overflow-hidden">
                    <div
                      className={`h-2.5 rounded-full transition-all duration-300 ${
                        rep.capacityUtilizationPercent > 80
                          ? 'bg-red-500'
                          : rep.capacityUtilizationPercent > 50
                          ? 'bg-amber-500'
                          : 'bg-green-500'
                      }`}
                      style={{ width: `${Math.min(100, rep.capacityUtilizationPercent)}%` }}
                    />
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Active Assignment Rules */}
        <div className="bg-white p-5 rounded-lg border border-gray-200 shadow-sm">
          <h2 className="text-base font-semibold text-gray-900 mb-4">Quy tắc Phân bổ (Rules)</h2>
          {rules.length === 0 ? (
            <p className="text-sm text-gray-500 italic">Chưa có quy tắc phân bổ nào.</p>
          ) : (
            <div className="space-y-3">
              {rules.map((r) => (
                <div key={r.id} className="p-3 bg-gray-50 rounded-lg border border-gray-200 text-sm">
                  <div className="flex items-center justify-between">
                    <span className="font-semibold text-gray-900">{r.name}</span>
                    <span className="text-xs bg-blue-100 text-blue-800 font-medium px-2 py-0.5 rounded">
                      {r.mode}
                    </span>
                  </div>
                  <div className="text-xs text-gray-500 mt-1 flex justify-between">
                    <span>Ưu tiên: #{r.priority}</span>
                    <span>Tối đa: {r.maxActiveLeadsPerRep || 50} lead/rep</span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Recent Assignment Audit Log */}
      <div className="bg-white rounded-lg border border-gray-200 shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200 bg-gray-50">
          <h2 className="text-base font-semibold text-gray-900">Nhật ký Phân bổ gần đây (Audit Logs)</h2>
        </div>
        <table className="min-w-full divide-y divide-gray-200 text-sm">
          <thead className="bg-gray-50 text-gray-600 font-medium">
            <tr>
              <th className="px-6 py-3 text-left">Khách hàng (Lead)</th>
              <th className="px-6 py-3 text-left">Nhân viên được gán</th>
              <th className="px-6 py-3 text-left">Phương thức</th>
              <th className="px-6 py-3 text-left">Lý do điều phối</th>
              <th className="px-6 py-3 text-right">Thời gian</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {logs.map((log) => (
              <tr key={log.id} className="hover:bg-gray-50">
                <td className="px-6 py-3 font-medium text-gray-900">{log.leadName}</td>
                <td className="px-6 py-3 text-gray-800 font-semibold">{log.assignedEmployeeName || 'N/A'}</td>
                <td className="px-6 py-3">
                  <span className="inline-flex px-2 py-0.5 rounded text-xs font-medium bg-purple-50 text-purple-700 border border-purple-200">
                    {log.mode}
                  </span>
                </td>
                <td className="px-6 py-3 text-xs text-gray-600">{log.reason}</td>
                <td className="px-6 py-3 text-xs text-gray-500 text-right">
                  {new Date(log.timestamp).toLocaleString('vi-VN')}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Add Rule Modal */}
      {showAddRule && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full p-6 shadow-xl">
            <h3 className="text-lg font-bold text-gray-900 mb-4">Thêm Quy tắc Phân bổ Lead</h3>
            <form onSubmit={handleCreateRule} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Tên quy tắc *</label>
                <input
                  type="text"
                  value={ruleName}
                  onChange={(e) => setRuleName(e.target.value)}
                  placeholder="VD: Phân bổ Lead Doanh nghiệp theo Tải công việc"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Phương thức phân bổ</label>
                <select
                  value={ruleMode}
                  onChange={(e) => setRuleMode(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
                >
                  <option value="RoundRobin">Round-Robin (Xoay vòng thứ tự đều)</option>
                  <option value="Workload">Workload-based (Ưu tiên người ít lead nhất)</option>
                  <option value="Territory">Territory-based (Theo địa bàn / Tỉnh thành)</option>
                  <option value="Source">Source-based (Theo nguồn chiến dịch)</option>
                  <option value="ServiceSpecialization">ServiceSpecialization (Theo chuyên môn dịch vụ)</option>
                </select>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Độ ưu tiên</label>
                  <input
                    type="number"
                    min={1}
                    value={rulePriority}
                    onChange={(e) => setRulePriority(parseInt(e.target.value) || 1)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Giới hạn tải/Rep</label>
                  <input
                    type="number"
                    min={1}
                    value={ruleMaxCapacity || 50}
                    onChange={(e) => setRuleMaxCapacity(parseInt(e.target.value) || 50)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
                  />
                </div>
              </div>

              <div className="flex justify-end gap-3 pt-3">
                <button
                  type="button"
                  onClick={() => setShowAddRule(false)}
                  className="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium shadow-sm"
                >
                  Tạo quy tắc
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
