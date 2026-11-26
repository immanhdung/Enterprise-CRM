import React, { useState, useEffect } from 'react';
import { AdminNavigation } from './AdminNavigation';
import { adminService } from '@/services/adminService';
import type { UserSummary, UserDetail } from '@/types/admin';
import {
  UserPlus,
  Search,
  RefreshCw,
  KeyRound,
  Shield,
  X,
  Check,
} from 'lucide-react';

export const UsersAdminPage: React.FC = () => {
  const [users, setUsers] = useState<UserSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [selectedUser, setSelectedUser] = useState<UserDetail | null>(null);

  // Modals
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showResetModal, setShowResetModal] = useState(false);
  const [showRolesModal, setShowRolesModal] = useState(false);

  // Forms state
  const [newPassword, setNewPassword] = useState('');
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
  const [createForm, setCreateForm] = useState({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    roles: ['Sales'],
    position: 'Sales Representative',
    phone: '',
    baseSalary: 15000000,
  });

  const availableRoles = ['Admin', 'CEO_BOD', 'Manager', 'Sales', 'Accountant'];

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const data = await adminService.getUsers({
        search: search.trim() || undefined,
        status: statusFilter || undefined,
      });
      setUsers(data);
    } catch (err) {
      console.error('Failed to load users:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, [statusFilter]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    fetchUsers();
  };

  const handleToggleStatus = async (user: UserSummary) => {
    const newStatus = user.status === 'Active' ? 'Suspended' : 'Active';
    if (!window.confirm(`Bạn có chắc muốn chuyển trạng thái tài khoản ${user.email} thành ${newStatus}?`)) return;

    try {
      await adminService.updateStatus(user.id, newStatus);
      fetchUsers();
    } catch (err: any) {
      alert(`Lỗi cập nhật trạng thái: ${err?.message || 'Có lỗi xảy ra'}`);
    }
  };

  const handleCreateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await adminService.createUser({
        email: createForm.email.trim(),
        password: createForm.password,
        firstName: createForm.firstName.trim(),
        lastName: createForm.lastName.trim(),
        roles: createForm.roles,
        position: createForm.position,
        phone: createForm.phone,
        baseSalary: createForm.baseSalary,
      });
      setShowCreateModal(false);
      setCreateForm({
        email: '',
        password: '',
        firstName: '',
        lastName: '',
        roles: ['Sales'],
        position: 'Sales Representative',
        phone: '',
        baseSalary: 15000000,
      });
      fetchUsers();
    } catch (err: any) {
      alert(`Lỗi tạo tài khoản: ${err?.message || 'Có lỗi xảy ra'}`);
    }
  };

  const handleResetPasswordSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedUser || !newPassword.trim()) return;

    try {
      await adminService.resetPassword(selectedUser.id, newPassword.trim());
      setShowResetModal(false);
      setNewPassword('');
      alert(`Mật khẩu tài khoản ${selectedUser.email} đã được đặt lại thành công.`);
    } catch (err: any) {
      alert(`Lỗi đặt lại mật khẩu: ${err?.message || 'Có lỗi xảy ra'}`);
    }
  };

  const handleAssignRolesSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedUser) return;

    try {
      await adminService.assignRoles(selectedUser.id, selectedRoles);
      setShowRolesModal(false);
      fetchUsers();
      alert(`Đã cập nhật vai trò cho ${selectedUser.email}.`);
    } catch (err: any) {
      alert(`Lỗi phân vai trò: ${err?.message || 'Có lỗi xảy ra'}`);
    }
  };

  const openRolesModal = async (u: UserSummary) => {
    const detail = await adminService.getUserById(u.id);
    setSelectedUser(detail);
    setSelectedRoles(detail.roles || []);
    setShowRolesModal(true);
  };

  const openResetModal = (u: UserSummary) => {
    setSelectedUser(u as any);
    setNewPassword('');
    setShowResetModal(true);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-xs">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-50 text-blue-700 border border-blue-200">
              Quản trị Hệ thống
            </span>
            <h1 className="text-2xl font-bold text-slate-900">Quản trị Người dùng & Tài khoản</h1>
          </div>
          <p className="text-sm text-slate-500 mt-1">
            Quản lý danh sách người dùng, cấp phát tài khoản, phân quyền vai trò, khóa phiên và đặt lại mật khẩu an toàn.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={() => setShowCreateModal(true)}
            className="flex items-center gap-1.5 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-semibold shadow-xs transition"
          >
            <UserPlus className="w-4 h-4" />
            Tạo Người dùng Mới
          </button>
        </div>
      </div>

      <AdminNavigation />

      {/* Filter Toolbar */}
      <div className="flex flex-col sm:flex-row items-center justify-between gap-4 bg-white p-4 rounded-xl border border-slate-200">
        <form onSubmit={handleSearchSubmit} className="flex items-center gap-2 w-full sm:w-80">
          <div className="relative w-full">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Tìm theo tên, email..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-9 pr-3 py-1.5 text-xs rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <button type="submit" className="px-3 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-xs font-medium">
            Tìm
          </button>
        </form>

        <div className="flex items-center gap-3 w-full sm:w-auto justify-end">
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="px-3 py-1.5 text-xs rounded-lg border border-slate-200 bg-white text-slate-700 focus:outline-none"
          >
            <option value="">Tất cả trạng thái</option>
            <option value="Active">Đang hoạt động (Active)</option>
            <option value="Suspended">Đang tạm khóa (Suspended)</option>
            <option value="Inactive">Ngừng hoạt động (Inactive)</option>
          </select>

          <button
            onClick={fetchUsers}
            className="p-2 text-slate-500 hover:text-slate-800 bg-slate-50 hover:bg-slate-100 rounded-lg border border-slate-200 transition"
            title="Làm mới"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
          </button>
        </div>
      </div>

      {/* Users Table */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
        {loading ? (
          <div className="flex items-center justify-center py-16">
            <RefreshCw className="w-8 h-8 animate-spin text-blue-600" />
          </div>
        ) : users.length === 0 ? (
          <div className="text-center py-12 text-slate-400 text-sm">Không tìm thấy người dùng nào phù hợp.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-slate-600">
              <thead className="bg-slate-50 text-slate-700 font-semibold uppercase border-b border-slate-200">
                <tr>
                  <th className="py-3 px-4">Họ và Tên</th>
                  <th className="py-3 px-4">Email</th>
                  <th className="py-3 px-4">Phòng ban & Vị trí</th>
                  <th className="py-3 px-4">Vai trò</th>
                  <th className="py-3 px-4 text-center">Trạng thái</th>
                  <th className="py-3 px-4">Đăng nhập gần nhất</th>
                  <th className="py-3 px-4 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {users.map((u) => (
                  <tr key={u.id} className="hover:bg-slate-50/60 transition">
                    <td className="py-3 px-4 font-semibold text-slate-900 flex items-center gap-2">
                      <div className="w-7 h-7 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center text-[11px] font-bold">
                        {u.firstName?.[0] || 'U'}
                      </div>
                      <span>{u.fullName}</span>
                    </td>
                    <td className="py-3 px-4 font-mono text-[11px] text-slate-600">{u.email}</td>
                    <td className="py-3 px-4">
                      <div className="font-medium text-slate-800">{u.departmentName || 'Chưa gán'}</div>
                      <div className="text-[11px] text-slate-500">{u.position || '—'}</div>
                    </td>
                    <td className="py-3 px-4">
                      <div className="flex flex-wrap gap-1">
                        {u.roles.map((r) => (
                          <span
                            key={r}
                            className="px-2 py-0.5 rounded text-[10px] font-semibold bg-purple-50 text-purple-700 border border-purple-200"
                          >
                            {r}
                          </span>
                        ))}
                      </div>
                    </td>
                    <td className="py-3 px-4 text-center">
                      <span
                        className={`inline-block px-2 py-0.5 rounded-full text-[10px] font-semibold ${
                          u.status === 'Active'
                            ? 'bg-emerald-50 text-emerald-700 border border-emerald-200'
                            : u.status === 'Suspended'
                            ? 'bg-rose-50 text-rose-700 border border-rose-200'
                            : 'bg-slate-100 text-slate-600'
                        }`}
                      >
                        {u.status === 'Active' ? 'Hoạt động' : u.status === 'Suspended' ? 'Tạm khóa' : 'Ngừng'}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-slate-500">
                      {u.lastLoginAt ? new Date(u.lastLoginAt).toLocaleString('vi-VN') : 'Chưa đăng nhập'}
                    </td>
                    <td className="py-3 px-4 text-right space-x-1">
                      <button
                        onClick={() => openRolesModal(u)}
                        className="p-1.5 text-slate-600 hover:text-purple-600 hover:bg-purple-50 rounded"
                        title="Phân vai trò"
                      >
                        <Shield className="w-3.5 h-3.5" />
                      </button>
                      <button
                        onClick={() => openResetModal(u)}
                        className="p-1.5 text-slate-600 hover:text-blue-600 hover:bg-blue-50 rounded"
                        title="Đặt lại mật khẩu"
                      >
                        <KeyRound className="w-3.5 h-3.5" />
                      </button>
                      <button
                        onClick={() => handleToggleStatus(u)}
                        className={`p-1.5 rounded ${
                          u.status === 'Active'
                            ? 'text-rose-600 hover:bg-rose-50'
                            : 'text-emerald-600 hover:bg-emerald-50'
                        }`}
                        title={u.status === 'Active' ? 'Khóa tài khoản' : 'Kích hoạt lại'}
                      >
                        {u.status === 'Active' ? <X className="w-3.5 h-3.5" /> : <Check className="w-3.5 h-3.5" />}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modal: Create User */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black/40 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl max-w-lg w-full p-6 shadow-xl border border-slate-200">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-base font-bold text-slate-900 flex items-center gap-2">
                <UserPlus className="w-5 h-5 text-blue-600" />
                Cấp Tài khoản Người dùng Mới
              </h3>
              <button onClick={() => setShowCreateModal(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleCreateSubmit} className="space-y-4 text-xs">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-slate-600 mb-1 font-medium">Họ & Đệm</label>
                  <input
                    type="text"
                    required
                    value={createForm.firstName}
                    onChange={(e) => setCreateForm({ ...createForm, firstName: e.target.value })}
                    className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-slate-600 mb-1 font-medium">Tên</label>
                  <input
                    type="text"
                    required
                    value={createForm.lastName}
                    onChange={(e) => setCreateForm({ ...createForm, lastName: e.target.value })}
                    className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>

              <div>
                <label className="block text-slate-600 mb-1 font-medium">Email Doanh nghiệp</label>
                <input
                  type="email"
                  required
                  value={createForm.email}
                  onChange={(e) => setCreateForm({ ...createForm, email: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 font-mono"
                />
              </div>

              <div>
                <label className="block text-slate-600 mb-1 font-medium">Mật khẩu ban đầu</label>
                <input
                  type="password"
                  required
                  minLength={6}
                  value={createForm.password}
                  onChange={(e) => setCreateForm({ ...createForm, password: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-slate-600 mb-1 font-medium">Vị trí công việc</label>
                  <input
                    type="text"
                    value={createForm.position}
                    onChange={(e) => setCreateForm({ ...createForm, position: e.target.value })}
                    className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-slate-600 mb-1 font-medium">Số điện thoại</label>
                  <input
                    type="text"
                    value={createForm.phone}
                    onChange={(e) => setCreateForm({ ...createForm, phone: e.target.value })}
                    className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>

              <div>
                <label className="block text-slate-600 mb-1 font-medium">Vai trò (Roles)</label>
                <div className="flex flex-wrap gap-2 pt-1">
                  {availableRoles.map((r) => (
                    <label key={r} className="flex items-center gap-1.5 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={createForm.roles.includes(r)}
                        onChange={(e) => {
                          const updated = e.target.checked
                            ? [...createForm.roles, r]
                            : createForm.roles.filter((x) => x !== r);
                          setCreateForm({ ...createForm, roles: updated });
                        }}
                        className="rounded text-blue-600"
                      />
                      <span className="text-slate-700">{r}</span>
                    </label>
                  ))}
                </div>
              </div>

              <div className="flex justify-end gap-2 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg font-medium"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-semibold"
                >
                  Tạo Người Dùng
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal: Reset Password */}
      {showResetModal && selectedUser && (
        <div className="fixed inset-0 bg-black/40 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl max-w-sm w-full p-6 shadow-xl border border-slate-200">
            <h3 className="text-base font-bold text-slate-900 mb-2">Đặt lại Mật khẩu</h3>
            <p className="text-xs text-slate-500 mb-4">
              Đặt lại mật khẩu truy cập cho tài khoản <span className="font-semibold text-slate-800">{selectedUser.email}</span>.
            </p>

            <form onSubmit={handleResetPasswordSubmit} className="space-y-4 text-xs">
              <div>
                <label className="block text-slate-600 mb-1 font-medium">Mật khẩu mới (tối thiểu 6 ký tự)</label>
                <input
                  type="password"
                  required
                  minLength={6}
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div className="flex justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setShowResetModal(false)}
                  className="px-3.5 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg font-medium"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="px-3.5 py-1.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-semibold"
                >
                  Lưu Mật khẩu Mới
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal: Assign Roles */}
      {showRolesModal && selectedUser && (
        <div className="fixed inset-0 bg-black/40 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl max-w-sm w-full p-6 shadow-xl border border-slate-200">
            <h3 className="text-base font-bold text-slate-900 mb-2">Phân Vai trò</h3>
            <p className="text-xs text-slate-500 mb-4">
              Gán vai trò và quyền hạn cho <span className="font-semibold text-slate-800">{selectedUser.email}</span>.
            </p>

            <form onSubmit={handleAssignRolesSubmit} className="space-y-4 text-xs">
              <div className="space-y-2">
                {availableRoles.map((r) => (
                  <label key={r} className="flex items-center gap-2 p-2 rounded-lg hover:bg-slate-50 cursor-pointer border border-slate-100">
                    <input
                      type="checkbox"
                      checked={selectedRoles.includes(r)}
                      onChange={(e) => {
                        const updated = e.target.checked
                          ? [...selectedRoles, r]
                          : selectedRoles.filter((x) => x !== r);
                        setSelectedRoles(updated);
                      }}
                      className="rounded text-purple-600"
                    />
                    <span className="font-medium text-slate-800">{r}</span>
                  </label>
                ))}
              </div>

              <div className="flex justify-end gap-2 pt-2 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setShowRolesModal(false)}
                  className="px-3.5 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg font-medium"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="px-3.5 py-1.5 bg-purple-600 hover:bg-purple-700 text-white rounded-lg font-semibold"
                >
                  Lưu Thay Đổi
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
