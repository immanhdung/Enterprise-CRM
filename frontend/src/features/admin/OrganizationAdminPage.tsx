import React, { useState, useEffect } from 'react';
import { AdminNavigation } from './AdminNavigation';
import { adminService } from '@/services/adminService';
import type { DepartmentHierarchy, OrganizationChangeHistory } from '@/types/admin';
import {
  Building2,
  Users,
  ArrowRightLeft,
  RefreshCw,
  ChevronDown,
  ChevronRight,
  History,
  X,
} from 'lucide-react';

export const OrganizationAdminPage: React.FC = () => {
  const [hierarchy, setHierarchy] = useState<DepartmentHierarchy[]>([]);
  const [history, setHistory] = useState<OrganizationChangeHistory[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'hierarchy' | 'history'>('hierarchy');
  const [expandedDepts, setExpandedDepts] = useState<Record<string, boolean>>({});

  // Transfer Modal
  const [showTransferModal, setShowTransferModal] = useState(false);
  const [transferForm, setTransferForm] = useState({
    employeeId: '',
    employeeName: '',
    newDepartmentId: '',
    newTeamId: '',
    newPosition: '',
    newBaseSalary: 0,
    effectiveDate: new Date().toISOString().split('T')[0],
    reason: '',
  });

  const fetchData = async () => {
    try {
      setLoading(true);
      const [h, hist] = await Promise.all([
        adminService.getHierarchy(),
        adminService.getChangeHistory(),
      ]);
      setHierarchy(h);
      setHistory(hist);

      // Expand all by default
      const exp: Record<string, boolean> = {};
      h.forEach((d) => (exp[d.id] = true));
      setExpandedDepts(exp);
    } catch (err) {
      console.error('Failed to load organization data:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const toggleDept = (deptId: string) => {
    setExpandedDepts((prev) => ({ ...prev, [deptId]: !prev[deptId] }));
  };

  const openTransferModal = (emp: { id: string; fullName: string; position: string }) => {
    setTransferForm({
      employeeId: emp.id,
      employeeName: emp.fullName,
      newDepartmentId: hierarchy[0]?.id || '',
      newTeamId: '',
      newPosition: emp.position,
      newBaseSalary: 18000000,
      effectiveDate: new Date().toISOString().split('T')[0],
      reason: 'Điều chuyển nhân sự theo kế hoạch vận hành doanh nghiệp.',
    });
    setShowTransferModal(true);
  };

  const handleTransferSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await adminService.transferEmployee({
        employeeId: transferForm.employeeId,
        newDepartmentId: transferForm.newDepartmentId,
        newTeamId: transferForm.newTeamId || undefined,
        newPosition: transferForm.newPosition || undefined,
        newBaseSalary: transferForm.newBaseSalary || undefined,
        effectiveDate: transferForm.effectiveDate,
        reason: transferForm.reason,
      });
      setShowTransferModal(false);
      alert('Đã thực hiện điều chuyển nhân sự thành công.');
      fetchData();
    } catch (err: any) {
      alert(`Lỗi điều chuyển: ${err?.message || 'Có lỗi xảy ra'}`);
    }
  };

  const formatVND = (val?: number | null) => {
    if (!val) return '—';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-xs">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-indigo-50 text-indigo-700 border border-indigo-200">
              Quản trị Hệ thống
            </span>
            <h1 className="text-2xl font-bold text-slate-900">Quản lý Cơ cấu Tổ chức & Điều chuyển</h1>
          </div>
          <p className="text-sm text-slate-500 mt-1">
            Xem sơ đồ phân cấp phòng ban, đội ngũ, danh bạ nhân sự và lịch sử luân chuyển công tác bảo toàn dữ liệu KPI/lương.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={fetchData}
            className="flex items-center gap-1.5 px-3.5 py-2 text-sm font-medium text-slate-700 bg-slate-50 hover:bg-slate-100 rounded-lg border border-slate-200 transition"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            Làm mới
          </button>
        </div>
      </div>

      <AdminNavigation />

      {/* View Switcher Tabs */}
      <div className="flex items-center gap-2 border-b border-slate-200 pb-3">
        <button
          onClick={() => setActiveTab('hierarchy')}
          className={`flex items-center gap-2 px-4 py-2 rounded-lg text-xs font-semibold transition ${
            activeTab === 'hierarchy'
              ? 'bg-blue-600 text-white'
              : 'text-slate-600 hover:bg-slate-100'
          }`}
        >
          <Building2 className="w-4 h-4" />
          Sơ đồ Phân cấp Tổ chức ({hierarchy.length} phòng ban)
        </button>
        <button
          onClick={() => setActiveTab('history')}
          className={`flex items-center gap-2 px-4 py-2 rounded-lg text-xs font-semibold transition ${
            activeTab === 'history'
              ? 'bg-blue-600 text-white'
              : 'text-slate-600 hover:bg-slate-100'
          }`}
        >
          <History className="w-4 h-4" />
          Lịch sử Điều chuyển & Bổ nhiệm ({history.length})
        </button>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-16">
          <RefreshCw className="w-8 h-8 animate-spin text-blue-600" />
        </div>
      ) : activeTab === 'hierarchy' ? (
        /* Hierarchy Tree View */
        <div className="space-y-4">
          {hierarchy.map((dept) => {
            const isExpanded = !!expandedDepts[dept.id];
            return (
              <div key={dept.id} className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
                {/* Department Header */}
                <div
                  onClick={() => toggleDept(dept.id)}
                  className="p-4 bg-slate-50/70 hover:bg-slate-100/60 flex items-center justify-between cursor-pointer border-b border-slate-200 transition"
                >
                  <div className="flex items-center gap-3">
                    <button className="text-slate-400">
                      {isExpanded ? <ChevronDown className="w-4 h-4" /> : <ChevronRight className="w-4 h-4" />}
                    </button>
                    <div className="p-2 bg-indigo-100 text-indigo-700 rounded-lg">
                      <Building2 className="w-4 h-4" />
                    </div>
                    <div>
                      <div className="font-bold text-slate-900 text-sm flex items-center gap-2">
                        {dept.name}
                        <span className="text-xs font-mono font-normal text-slate-500">({dept.code})</span>
                      </div>
                      <div className="text-xs text-slate-500 mt-0.5">
                        Trưởng phòng:{' '}
                        <span className="font-medium text-slate-700">{dept.managerName || 'Chưa bổ nhiệm'}</span> •{' '}
                        {dept.totalEmployees} nhân sự • {dept.teams.length} đội nhóm
                      </div>
                    </div>
                  </div>
                </div>

                {/* Teams & Direct Staff */}
                {isExpanded && (
                  <div className="p-4 space-y-4">
                    {/* Teams in department */}
                    {dept.teams.map((team) => (
                      <div key={team.id} className="border border-slate-200 rounded-xl p-3 bg-slate-50/30">
                        <div className="flex items-center justify-between mb-2">
                          <div className="flex items-center gap-2">
                            <div className="p-1.5 bg-blue-50 text-blue-600 rounded">
                              <Users className="w-3.5 h-3.5" />
                            </div>
                            <span className="font-semibold text-xs text-slate-800">{team.name}</span>
                            <span className="text-[11px] text-slate-500">
                              (Trưởng nhóm: {team.teamLeaderName || 'Chưa gán'}) • {team.memberCount} thành viên
                            </span>
                          </div>
                        </div>

                        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2">
                          {team.members.map((member) => (
                            <div
                              key={member.id}
                              className="p-2.5 bg-white border border-slate-200 rounded-lg flex items-center justify-between text-xs"
                            >
                              <div>
                                <div className="font-semibold text-slate-900">{member.fullName}</div>
                                <div className="text-[11px] text-slate-500">
                                  {member.position} • {member.employeeCode}
                                </div>
                              </div>
                              <button
                                onClick={() => openTransferModal(member)}
                                className="p-1.5 text-blue-600 hover:bg-blue-50 rounded text-[11px] flex items-center gap-1 font-medium"
                                title="Điều chuyển nhân sự"
                              >
                                <ArrowRightLeft className="w-3.5 h-3.5" />
                                <span>Chuyển</span>
                              </button>
                            </div>
                          ))}
                        </div>
                      </div>
                    ))}

                    {/* Direct staff (no team) */}
                    {dept.directEmployees.length > 0 && (
                      <div className="pt-2">
                        <div className="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-2">
                          Nhân sự trực thuộc phòng ban (Chưa vào Team)
                        </div>
                        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2">
                          {dept.directEmployees.map((member) => (
                            <div
                              key={member.id}
                              className="p-2.5 bg-white border border-slate-200 rounded-lg flex items-center justify-between text-xs"
                            >
                              <div>
                                <div className="font-semibold text-slate-900">{member.fullName}</div>
                                <div className="text-[11px] text-slate-500">
                                  {member.position} • {member.employeeCode}
                                </div>
                              </div>
                              <button
                                onClick={() => openTransferModal(member)}
                                className="p-1.5 text-blue-600 hover:bg-blue-50 rounded text-[11px] flex items-center gap-1 font-medium"
                                title="Điều chuyển nhân sự"
                              >
                                <ArrowRightLeft className="w-3.5 h-3.5" />
                                <span>Chuyển</span>
                              </button>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      ) : (
        /* History Table View */
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
          {history.length === 0 ? (
            <div className="text-center py-12 text-slate-400 text-sm">Chưa có lịch sử điều chuyển nào.</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs text-slate-600">
                <thead className="bg-slate-50 text-slate-700 font-semibold uppercase border-b border-slate-200">
                  <tr>
                    <th className="py-3 px-4">Nhân sự</th>
                    <th className="py-3 px-4">Loại thay đổi</th>
                    <th className="py-3 px-4">Phòng ban cũ &rarr; mới</th>
                    <th className="py-3 px-4">Đội nhóm cũ &rarr; mới</th>
                    <th className="py-3 px-4">Lương cơ bản</th>
                    <th className="py-3 px-4">Ngày hiệu lực</th>
                    <th className="py-3 px-4">Lý do điều chuyển</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {history.map((h) => (
                    <tr key={h.id} className="hover:bg-slate-50/60 transition">
                      <td className="py-3 px-4 font-semibold text-slate-900">{h.employeeName}</td>
                      <td className="py-3 px-4">
                        <span className="px-2 py-0.5 rounded text-[10px] font-semibold bg-blue-50 text-blue-700 border border-blue-200">
                          {h.changeType}
                        </span>
                      </td>
                      <td className="py-3 px-4">
                        <span className="text-slate-500">{h.oldDepartmentName || '—'}</span> &rarr;{' '}
                        <span className="font-semibold text-slate-800">{h.newDepartmentName}</span>
                      </td>
                      <td className="py-3 px-4">
                        <span className="text-slate-500">{h.oldTeamName || '—'}</span> &rarr;{' '}
                        <span className="font-semibold text-slate-800">{h.newTeamName || 'Trực thuộc'}</span>
                      </td>
                      <td className="py-3 px-4">
                        {h.oldBaseSalary && h.newBaseSalary ? (
                          <span>
                            {formatVND(h.oldBaseSalary)} &rarr;{' '}
                            <span className="font-semibold text-emerald-600">{formatVND(h.newBaseSalary)}</span>
                          </span>
                        ) : (
                          '—'
                        )}
                      </td>
                      <td className="py-3 px-4 text-slate-500">{h.effectiveDate}</td>
                      <td className="py-3 px-4 text-slate-700 max-w-xs truncate">{h.reason}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* Modal: Transfer Employee */}
      {showTransferModal && (
        <div className="fixed inset-0 bg-black/40 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl max-w-lg w-full p-6 shadow-xl border border-slate-200">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-base font-bold text-slate-900 flex items-center gap-2">
                <ArrowRightLeft className="w-5 h-5 text-blue-600" />
                Quyết định Điều chuyển Nhân sự
              </h3>
              <button onClick={() => setShowTransferModal(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <p className="text-xs text-slate-500 mb-4">
              Điều chuyển nhân sự <span className="font-semibold text-slate-900">{transferForm.employeeName}</span> sang phòng ban / đội nhóm mới. Dữ liệu KPI và lương lịch sử được bảo toàn nguyên vẹn.
            </p>

            <form onSubmit={handleTransferSubmit} className="space-y-4 text-xs">
              <div>
                <label className="block text-slate-600 mb-1 font-medium">Phòng ban Tiếp nhận</label>
                <select
                  required
                  value={transferForm.newDepartmentId}
                  onChange={(e) => setTransferForm({ ...transferForm, newDepartmentId: e.target.value, newTeamId: '' })}
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                >
                  {hierarchy.map((d) => (
                    <option key={d.id} value={d.id}>
                      {d.name} ({d.code})
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-slate-600 mb-1 font-medium">Đội nhóm (Tùy chọn)</label>
                <select
                  value={transferForm.newTeamId}
                  onChange={(e) => setTransferForm({ ...transferForm, newTeamId: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                >
                  <option value="">Trực thuộc phòng ban (Không vào Team)</option>
                  {hierarchy
                    .find((d) => d.id === transferForm.newDepartmentId)
                    ?.teams.map((t) => (
                      <option key={t.id} value={t.id}>
                        {t.name}
                      </option>
                    ))}
                </select>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-slate-600 mb-1 font-medium">Vị trí mới</label>
                  <input
                    type="text"
                    value={transferForm.newPosition}
                    onChange={(e) => setTransferForm({ ...transferForm, newPosition: e.target.value })}
                    className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-slate-600 mb-1 font-medium">Mức lương cơ bản mới (VNĐ)</label>
                  <input
                    type="number"
                    value={transferForm.newBaseSalary}
                    onChange={(e) => setTransferForm({ ...transferForm, newBaseSalary: Number(e.target.value) })}
                    className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>

              <div>
                <label className="block text-slate-600 mb-1 font-medium">Ngày có hiệu lực</label>
                <input
                  type="date"
                  required
                  value={transferForm.effectiveDate}
                  onChange={(e) => setTransferForm({ ...transferForm, effectiveDate: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-slate-600 mb-1 font-medium">Lý do điều chuyển</label>
                <textarea
                  required
                  rows={2}
                  value={transferForm.reason}
                  onChange={(e) => setTransferForm({ ...transferForm, reason: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div className="flex justify-end gap-2 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setShowTransferModal(false)}
                  className="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg font-medium"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-semibold"
                >
                  Xác nhận Điều chuyển
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
