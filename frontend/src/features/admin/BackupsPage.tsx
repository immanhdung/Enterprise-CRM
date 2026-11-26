import React, { useState, useEffect } from 'react';
import { AdminNavigation } from './AdminNavigation';
import { adminService } from '@/services/adminService';
import type { BackupRecord, RestoreRecord, SupabasePitrStatus } from '@/types/admin';
import {
  RefreshCw,
  Plus,
  Play,
  ShieldCheck,
  CheckCircle2,
  Clock,
  X,
} from 'lucide-react';

export const BackupsPage: React.FC = () => {
  const [backups, setBackups] = useState<BackupRecord[]>([]);
  const [restores, setRestores] = useState<RestoreRecord[]>([]);
  const [pitrStatus, setPitrStatus] = useState<SupabasePitrStatus | null>(null);
  const [loading, setLoading] = useState(true);

  // Modal
  const [showBackupModal, setShowBackupModal] = useState(false);
  const [backupName, setBackupName] = useState('');
  const [backupNotes, setBackupNotes] = useState('');
  const [executingDrill, setExecutingDrill] = useState(false);
  const [selectedDrillLog, setSelectedDrillLog] = useState<string | null>(null);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [bList, rList, pStatus] = await Promise.all([
        adminService.getBackups(),
        adminService.getRestores(),
        adminService.getSupabasePitrStatus(),
      ]);
      setBackups(bList);
      setRestores(rList);
      setPitrStatus(pStatus);
    } catch (err) {
      console.error('Failed to load backup data:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleInitiateBackup = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await adminService.initiateBackup(backupName.trim() || undefined, backupNotes.trim() || undefined);
      setShowBackupModal(false);
      setBackupName('');
      setBackupNotes('');
      alert('Đã hoàn tất sao lưu dữ liệu và kiểm tra toàn vẹn băm SHA-256 thành công.');
      fetchData();
    } catch (err: any) {
      alert(`Lỗi sao lưu: ${err?.message || 'Có lỗi xảy ra'}`);
    }
  };

  const handleRunRestoreDrill = async (backupId: string) => {
    if (!window.confirm('Tiến hành diễn tập phục hồi an toàn (Non-destructive drill) trên môi trường kiểm thử cô lập? Dữ liệu sản xuất thực tế hoàn toàn không bị ảnh hưởng.')) return;

    try {
      setExecutingDrill(true);
      const res = await adminService.executeRestoreDrill(backupId);
      setSelectedDrillLog(res.executionLog || 'Diễn tập hoàn tất thành công.');
      fetchData();
    } catch (err: any) {
      alert(`Lỗi diễn tập phục hồi: ${err?.message || 'Có lỗi xảy ra'}`);
    } finally {
      setExecutingDrill(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-xs">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
              Phòng chống Thảm họa & Sẵn sàng
            </span>
            <h1 className="text-2xl font-bold text-slate-900">Sao Lưu, Phục Hồi & Phục Hồi Điểm Thời Gian (PITR)</h1>
          </div>
          <p className="text-sm text-slate-500 mt-1">
            Quản trị bản sao lưu cơ sở dữ liệu PostgreSQL, mã băm toàn vẹn SHA-256, diễn tập phục hồi cô lập và trạng thái Supabase PITR.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={() => setShowBackupModal(true)}
            className="flex items-center gap-1.5 px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-sm font-semibold shadow-xs transition"
          >
            <Plus className="w-4 h-4" />
            Tạo Bản Sao Lưu Ngay
          </button>
        </div>
      </div>

      <AdminNavigation />

      {/* Supabase Continuous Archiving (PITR) Status Card */}
      {pitrStatus && (
        <div className="bg-linear-to-r from-slate-900 to-indigo-950 text-white p-6 rounded-2xl shadow-sm">
          <div className="flex flex-col md:flex-row justify-between md:items-center gap-4">
            <div>
              <div className="flex items-center gap-2">
                <ShieldCheck className="w-5 h-5 text-emerald-400" />
                <span className="font-bold text-base">Supabase Point-in-Time Recovery (PITR) & Continuous WAL</span>
              </div>
              <p className="text-xs text-slate-300 mt-1 max-w-2xl">{pitrStatus.statusMessage}</p>
            </div>

            <div className="flex items-center gap-3 bg-white/10 backdrop-blur-md px-4 py-2.5 rounded-xl border border-white/20">
              <Clock className="w-4 h-4 text-emerald-300" />
              <div className="text-xs">
                <div className="text-slate-300">Cửa sổ lưu trữ (Retention):</div>
                <div className="font-bold text-white text-sm">{pitrStatus.retentionDays} Ngày liên tục</div>
              </div>
            </div>
          </div>

          <div className="mt-4 pt-3 border-t border-white/10 grid grid-cols-1 sm:grid-cols-2 text-xs text-slate-300">
            <div>Điểm phục hồi sớm nhất: <span className="font-mono text-white">{new Date(pitrStatus.earliestRestorePointUtc).toLocaleString('vi-VN')}</span></div>
            <div>Điểm phục hồi trễ nhất: <span className="font-mono text-white">{new Date(pitrStatus.latestRestorePointUtc).toLocaleString('vi-VN')}</span></div>
          </div>
        </div>
      )}

      {/* Backups Table */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
        <div className="p-4 bg-slate-50 border-b border-slate-200 font-bold text-xs text-slate-800 flex items-center justify-between">
          <span>Danh sách Bản Sao Lưu (Backup Snapshots)</span>
          <button onClick={fetchData} className="text-slate-500 hover:text-slate-800">
            <RefreshCw className={`w-3.5 h-3.5 ${loading ? 'animate-spin' : ''}`} />
          </button>
        </div>

        {loading ? (
          <div className="flex items-center justify-center py-16">
            <RefreshCw className="w-8 h-8 animate-spin text-blue-600" />
          </div>
        ) : backups.length === 0 ? (
          <div className="p-8 text-center text-xs text-slate-400">Chưa có bản sao lưu nào.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-slate-600">
              <thead className="bg-slate-50 text-slate-700 font-semibold uppercase border-b border-slate-200">
                <tr>
                  <th className="py-3 px-4">Tên Bản Sao Lưu</th>
                  <th className="py-3 px-4">Loại</th>
                  <th className="py-3 px-4">Dung lượng</th>
                  <th className="py-3 px-4">Mã băm SHA-256 (Toàn vẹn)</th>
                  <th className="py-3 px-4 text-center">Trạng thái</th>
                  <th className="py-3 px-4">Thời điểm tạo</th>
                  <th className="py-3 px-4 text-right">Diễn tập Phục hồi</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {backups.map((b) => (
                  <tr key={b.id} className="hover:bg-slate-50/60 transition">
                    <td className="py-3 px-4 font-semibold text-slate-900">{b.backupName}</td>
                    <td className="py-3 px-4">
                      <span className="px-2 py-0.5 rounded text-[10px] font-semibold bg-slate-100 text-slate-700">
                        {b.backupType}
                      </span>
                    </td>
                    <td className="py-3 px-4 font-medium text-slate-800">{b.fileSizeFormatted}</td>
                    <td className="py-3 px-4 font-mono text-[10px] text-slate-500 truncate max-w-[140px]" title={b.checksumSha256 || ''}>
                      {b.checksumSha256 || '—'}
                    </td>
                    <td className="py-3 px-4 text-center">
                      <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
                        <CheckCircle2 className="w-3 h-3" />
                        {b.status}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-slate-500">
                      {new Date(b.createdAtUtc).toLocaleString('vi-VN')}
                    </td>
                    <td className="py-3 px-4 text-right">
                      <button
                        onClick={() => handleRunRestoreDrill(b.id)}
                        disabled={executingDrill}
                        className="inline-flex items-center gap-1 px-2.5 py-1 bg-purple-50 hover:bg-purple-100 text-purple-700 rounded text-xs font-semibold transition"
                      >
                        <Play className="w-3 h-3" />
                        <span>Diễn tập (Drill)</span>
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Restore Drills History */}
      {restores.length > 0 && (
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
          <div className="p-4 bg-slate-50 border-b border-slate-200 font-bold text-xs text-slate-800">
            Lịch sử Diễn tập & Xác minh Phục hồi ({restores.length})
          </div>
          <div className="divide-y divide-slate-100">
            {restores.map((r) => (
              <div key={r.id} className="p-3.5 flex items-center justify-between hover:bg-slate-50 text-xs">
                <div>
                  <div className="font-semibold text-slate-900">{r.backupName} &rarr; Môi trường: {r.targetEnvironment}</div>
                  <div className="text-[11px] text-slate-500 mt-0.5">
                    Xác minh bởi: {r.verifiedByEmail} • {new Date(r.createdAtUtc).toLocaleString('vi-VN')}
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <span className="px-2 py-0.5 rounded text-[10px] font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
                    {r.status}
                  </span>
                  <button
                    onClick={() => setSelectedDrillLog(r.executionLog || 'Không có log chi tiết.')}
                    className="text-xs text-blue-600 hover:underline font-medium"
                  >
                    Xem nhật ký
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Modal: Initiate Backup */}
      {showBackupModal && (
        <div className="fixed inset-0 bg-black/40 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl max-w-sm w-full p-6 shadow-xl border border-slate-200">
            <h3 className="text-base font-bold text-slate-900 mb-2">Tạo Bản Sao Lưu Tức Thì</h3>
            <p className="text-xs text-slate-500 mb-4">
              Hệ thống sẽ tạo snapshot dữ liệu PostgreSQL và tính mã băm kiểm tra toàn vẹn SHA-256.
            </p>

            <form onSubmit={handleInitiateBackup} className="space-y-4 text-xs">
              <div>
                <label className="block text-slate-600 mb-1 font-medium">Tên Gợi Nhớ (Tùy chọn)</label>
                <input
                  type="text"
                  placeholder="backup_on_demand_before_release"
                  value={backupName}
                  onChange={(e) => setBackupName(e.target.value)}
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 font-mono text-[11px]"
                />
              </div>

              <div>
                <label className="block text-slate-600 mb-1 font-medium">Ghi chú mục đích sao lưu</label>
                <textarea
                  rows={2}
                  placeholder="Ghi chú lý do sao lưu..."
                  value={backupNotes}
                  onChange={(e) => setBackupNotes(e.target.value)}
                  className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500"
                />
              </div>

              <div className="flex justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setShowBackupModal(false)}
                  className="px-3.5 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg font-medium"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="px-3.5 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg font-semibold"
                >
                  Bắt Đầu Sao Lưu
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal: Drill Execution Log */}
      {selectedDrillLog && (
        <div className="fixed inset-0 bg-black/40 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl max-w-xl w-full p-6 shadow-xl border border-slate-200">
            <div className="flex justify-between items-center mb-3 pb-2 border-b border-slate-100">
              <h3 className="text-base font-bold text-slate-900">Nhật Ký Diễn Tập Phục Hồi Cô Lập</h3>
              <button onClick={() => setSelectedDrillLog(null)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <pre className="p-4 bg-slate-950 text-emerald-400 rounded-xl font-mono text-xs overflow-x-auto max-h-60 whitespace-pre-wrap">
              {selectedDrillLog}
            </pre>

            <div className="flex justify-end pt-4 mt-2">
              <button
                onClick={() => setSelectedDrillLog(null)}
                className="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-xs font-semibold"
              >
                Đóng
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
