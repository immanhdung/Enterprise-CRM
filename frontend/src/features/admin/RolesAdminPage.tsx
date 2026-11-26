import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { AdminNavigation } from './AdminNavigation';
import { adminService } from '@/services/adminService';
import type { RoleDto, PermissionDto } from '@/types/admin';
import {
  ShieldCheck,
  Plus,
  RefreshCw,
  Grid3X3,
  Trash2,
  X,
  Users,
} from 'lucide-react';

export const RolesAdminPage: React.FC = () => {
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [permissions, setPermissions] = useState<PermissionDto[]>([]);
  const [loading, setLoading] = useState(true);

  // Modal
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [createForm, setCreateForm] = useState({
    name: '',
    description: '',
    permissions: [] as string[],
  });

  const fetchRolesAndPermissions = async () => {
    try {
      setLoading(true);
      const [rList, pList] = await Promise.all([
        adminService.getRoles(),
        adminService.getPermissions(),
      ]);
      setRoles(rList);
      setPermissions(pList);
    } catch (err) {
      console.error('Failed to load roles:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRolesAndPermissions();
  }, []);

  const handleCreateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await adminService.createRole({
        name: createForm.name.trim(),
        description: createForm.description.trim(),
        permissions: createForm.permissions,
      });
      setShowCreateModal(false);
      setCreateForm({ name: '', description: '', permissions: [] });
      alert('Đã tạo vai trò mới thành công.');
      fetchRolesAndPermissions();
    } catch (err: any) {
      alert(`Lỗi tạo vai trò: ${err?.message || 'Có lỗi xảy ra'}`);
    }
  };

  const handleDeleteRole = async (role: RoleDto) => {
    if (role.isSystem) {
      alert('Không thể xóa vai trò mặc định của hệ thống.');
      return;
    }

    if (!window.confirm(`Bạn có chắc muốn xóa vai trò '${role.name}'?`)) return;

    try {
      await adminService.deleteRole(role.id);
      fetchRolesAndPermissions();
    } catch (err: any) {
      alert(`Lỗi xóa vai trò: ${err?.message || 'Có lỗi xảy ra'}`);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-xs">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-purple-50 text-purple-700 border border-purple-200">
              Quản trị Hệ thống
            </span>
            <h1 className="text-2xl font-bold text-slate-900">Quản lý Vai trò (Roles) & Phân quyền RBAC</h1>
          </div>
          <p className="text-sm text-slate-500 mt-1">
            Thiết lập danh mục vai trò người dùng, kiểm soát quyền truy cập module và phạm vi dữ liệu bảo mật (Data Scope).
          </p>
        </div>

        <div className="flex items-center gap-3">
          <Link
            to="/admin/permission-matrix"
            className="flex items-center gap-1.5 px-3.5 py-2 text-sm font-semibold text-purple-700 bg-purple-50 hover:bg-purple-100 rounded-lg border border-purple-200 transition"
          >
            <Grid3X3 className="w-4 h-4" />
            Ma trận Phân quyền
          </Link>
          <button
            onClick={() => setShowCreateModal(true)}
            className="flex items-center gap-1.5 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-semibold shadow-xs transition"
          >
            <Plus className="w-4 h-4" />
            Tạo Vai trò Mới
          </button>
        </div>
      </div>

      <AdminNavigation />

      {/* Roles Grid */}
      {loading ? (
        <div className="flex items-center justify-center py-16">
          <RefreshCw className="w-8 h-8 animate-spin text-blue-600" />
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
          {roles.map((r) => (
            <div
              key={r.id}
              className="bg-white rounded-2xl border border-slate-200 shadow-xs p-5 flex flex-col justify-between hover:border-purple-300 transition"
            >
              <div>
                <div className="flex items-start justify-between">
                  <div className="flex items-center gap-2">
                    <div className="p-2 bg-purple-50 text-purple-600 rounded-lg">
                      <ShieldCheck className="w-5 h-5" />
                    </div>
                    <div>
                      <h3 className="font-bold text-base text-slate-900">{r.name}</h3>
                      <div className="text-[11px] text-slate-400 mt-0.5">
                        {r.isSystem ? (
                          <span className="text-purple-600 font-semibold">Hệ thống (Mặc định)</span>
                        ) : (
                          <span className="text-blue-600 font-semibold">Tùy chỉnh (Custom)</span>
                        )}
                      </div>
                    </div>
                  </div>

                  {!r.isSystem && (
                    <button
                      onClick={() => handleDeleteRole(r)}
                      className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded"
                      title="Xóa vai trò"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  )}
                </div>

                <p className="text-xs text-slate-600 mt-3 line-clamp-2">{r.description}</p>

                <div className="mt-4 pt-3 border-t border-slate-100 flex items-center justify-between text-xs text-slate-500">
                  <span className="flex items-center gap-1.5">
                    <Users className="w-3.5 h-3.5 text-slate-400" />
                    <strong>{r.usersCount}</strong> người dùng
                  </span>
                  <span>
                    <strong>{r.permissions.length}</strong> quyền hạn
                  </span>
                </div>

                {/* Sample permissions badges */}
                <div className="mt-3 flex flex-wrap gap-1">
                  {r.permissions.slice(0, 4).map((p) => (
                    <span
                      key={p}
                      className="px-2 py-0.5 rounded text-[10px] font-mono bg-slate-100 text-slate-600"
                    >
                      {p}
                    </span>
                  ))}
                  {r.permissions.length > 4 && (
                    <span className="px-2 py-0.5 rounded text-[10px] bg-slate-100 text-slate-500 font-medium">
                      +{r.permissions.length - 4} quyền khác
                    </span>
                  )}
                </div>
              </div>

              <div className="mt-4 pt-3">
                <Link
                  to="/admin/permission-matrix"
                  className="w-full block text-center py-2 bg-slate-50 hover:bg-purple-50 text-purple-700 font-semibold rounded-lg text-xs border border-slate-200 transition"
                >
                  Cấu hình Quyền & Phạm vi (Data Scope) &rarr;
                </Link>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Modal: Create Custom Role */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black/40 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl max-w-xl w-full p-6 shadow-xl border border-slate-200">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-base font-bold text-slate-900 flex items-center gap-2">
                <Plus className="w-5 h-5 text-blue-600" />
                Tạo Vai trò Mới (Custom Role)
              </h3>
              <button onClick={() => setShowCreateModal(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleCreateSubmit} className="space-y-4 text-xs">
              <div>
                <label className="block text-slate-600 mb-1 font-medium">Tên Vai trò</label>
                <input
                  type="text"
                  required
                  placeholder="Ví dụ: ProjectDirector, CustomerSuccess..."
                  value={createForm.name}
                  onChange={(e) => setCreateForm({ ...createForm, name: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 font-semibold"
                />
              </div>

              <div>
                <label className="block text-slate-600 mb-1 font-medium">Mô tả mục đích vai trò</label>
                <input
                  type="text"
                  required
                  placeholder="Mô tả chức năng và phạm vi nghiệp vụ..."
                  value={createForm.description}
                  onChange={(e) => setCreateForm({ ...createForm, description: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-slate-600 mb-1 font-medium">Chọn quyền ban đầu ({createForm.permissions.length} quyền)</label>
                <div className="max-h-48 overflow-y-auto border border-slate-200 rounded-lg p-2 divide-y divide-slate-100">
                  {permissions.map((p) => (
                    <label key={p.code} className="flex items-center gap-2 py-1 px-1 cursor-pointer hover:bg-slate-50">
                      <input
                        type="checkbox"
                        checked={createForm.permissions.includes(p.code)}
                        onChange={(e) => {
                          const updated = e.target.checked
                            ? [...createForm.permissions, p.code]
                            : createForm.permissions.filter((x) => x !== p.code);
                          setCreateForm({ ...createForm, permissions: updated });
                        }}
                        className="rounded text-blue-600"
                      />
                      <div>
                        <span className="font-mono text-[11px] text-slate-800 font-semibold">{p.code}</span>
                        <span className="text-[10px] text-slate-500 ml-2">({p.description})</span>
                      </div>
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
                  Tạo Vai Trò
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
