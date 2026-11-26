import React, { useState, useEffect } from 'react';
import { adminService } from '@/services/adminService';
import type { NotificationSummary } from '@/types/admin';
import {
  Bell,
  CheckCircle2,
  CheckCheck,
  Trash2,
  Send,
  RefreshCw,
  Search,
  Info,
  AlertCircle,
  ExternalLink,
  X,
  Layers,
  Sparkles,
  ShieldCheck,
  CreditCard,
  FileText,
  UserCheck,
} from 'lucide-react';

const CATEGORIES = [
  { id: 'ALL', name: 'Tất cả danh mục' },
  { id: 'System', name: 'Hệ thống (System)' },
  { id: 'Lead', name: 'Khách hàng tiềm năng (Lead)' },
  { id: 'Deal', name: 'Cơ hội (Deal)' },
  { id: 'Contract', name: 'Hợp đồng (Contract)' },
  { id: 'Payment', name: 'Tài chính & Thanh toán' },
  { id: 'Approval', name: 'Phê duyệt (Approval)' },
  { id: 'AI', name: 'Trí tuệ nhân tạo (AI)' },
];

export const NotificationCenterPage: React.FC = () => {
  const [summary, setSummary] = useState<NotificationSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [filterUnreadOnly, setFilterUnreadOnly] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState('ALL');
  const [search, setSearch] = useState('');

  // Dispatch Modal
  const [showDispatchModal, setShowDispatchModal] = useState(false);
  const [dispatchTitle, setDispatchTitle] = useState('');
  const [dispatchMessage, setDispatchMessage] = useState('');
  const [dispatchLink, setDispatchLink] = useState('');
  const [dispatchCategory, setDispatchCategory] = useState('System');
  const [dispatchPriority, setDispatchPriority] = useState('Medium');
  const [sending, setSending] = useState(false);

  const fetchNotifications = async () => {
    try {
      setLoading(true);
      const res = await adminService.getNotifications(filterUnreadOnly, 100);
      setSummary(res);
    } catch (err) {
      console.error('Failed to load notifications:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchNotifications();
  }, [filterUnreadOnly]);

  const handleMarkAsRead = async (id: string) => {
    try {
      await adminService.markAsRead(id);
      fetchNotifications();
    } catch (err) {
      console.error('Failed to mark read:', err);
    }
  };

  const handleMarkAllAsRead = async () => {
    try {
      await adminService.markAllAsRead();
      fetchNotifications();
    } catch (err) {
      console.error('Failed to mark all read:', err);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Bạn có chắc muốn xóa thông báo này?')) return;
    try {
      await adminService.deleteNotification(id);
      fetchNotifications();
    } catch (err) {
      console.error('Failed to delete notification:', err);
    }
  };

  const handleDispatch = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setSending(true);
      await adminService.dispatchNotification({
        title: dispatchTitle.trim(),
        message: dispatchMessage.trim(),
        link: dispatchLink.trim() || undefined,
        category: dispatchCategory,
        priority: dispatchPriority,
      });
      setShowDispatchModal(false);
      setDispatchTitle('');
      setDispatchMessage('');
      setDispatchLink('');
      fetchNotifications();
      alert('Đã gửi thông báo thành công.');
    } catch (err: any) {
      alert(`Lỗi phát thông báo: ${err?.response?.data?.message || err.message}`);
    } finally {
      setSending(false);
    }
  };

  const items = summary?.recentNotifications || [];
  const filteredItems = items.filter(n => {
    const matchCat = selectedCategory === 'ALL' || n.category === selectedCategory;
    const matchSearch =
      n.title.toLowerCase().includes(search.toLowerCase()) ||
      n.message.toLowerCase().includes(search.toLowerCase());
    return matchCat && matchSearch;
  });

  const getPriorityBadge = (priority: string) => {
    switch (priority?.toLowerCase()) {
      case 'urgent':
        return <span className="px-2 py-0.5 rounded text-xs font-bold bg-rose-100 text-rose-800">Khẩn cấp</span>;
      case 'high':
        return <span className="px-2 py-0.5 rounded text-xs font-semibold bg-amber-100 text-amber-800">Cao</span>;
      case 'low':
        return <span className="px-2 py-0.5 rounded text-xs font-normal bg-slate-100 text-slate-700">Thấp</span>;
      default:
        return <span className="px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">Bình thường</span>;
    }
  };

  const getCategoryIcon = (category: string) => {
    switch (category?.toLowerCase()) {
      case 'lead':
        return <UserCheck className="w-4 h-4 text-emerald-600" />;
      case 'deal':
        return <Layers className="w-4 h-4 text-blue-600" />;
      case 'contract':
        return <FileText className="w-4 h-4 text-indigo-600" />;
      case 'payment':
        return <CreditCard className="w-4 h-4 text-emerald-700" />;
      case 'approval':
        return <ShieldCheck className="w-4 h-4 text-amber-600" />;
      case 'ai':
        return <Sparkles className="w-4 h-4 text-purple-600" />;
      default:
        return <Info className="w-4 h-4 text-slate-500" />;
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 flex items-center gap-2">
            <Bell className="w-7 h-7 text-indigo-600" />
            Trung tâm Thông báo Doanh nghiệp (Notification Center)
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            Theo dõi dòng sự kiện thời gian thực, nhắc việc, cảnh báo tiến độ và thông điệp điều hành.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={fetchNotifications}
            className="inline-flex items-center gap-2 px-3 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50"
          >
            <RefreshCw className="w-4 h-4" />
            Làm mới
          </button>
          <button
            onClick={handleMarkAllAsRead}
            disabled={!summary?.unreadCount}
            className="inline-flex items-center gap-2 px-3 py-2 text-sm font-medium text-indigo-700 bg-indigo-50 border border-indigo-200 rounded-lg hover:bg-indigo-100 disabled:opacity-50"
          >
            <CheckCheck className="w-4 h-4" />
            Đánh dấu tất cả đã đọc
          </button>
          <button
            onClick={() => setShowDispatchModal(true)}
            className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 shadow-sm"
          >
            <Send className="w-4 h-4" />
            Gửi thông báo mới
          </button>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="p-4 bg-white rounded-xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <p className="text-xs font-semibold text-slate-500 uppercase tracking-wider">Tổng số thông báo</p>
            <p className="text-2xl font-bold text-slate-900 mt-1">{summary?.totalCount ?? 0}</p>
          </div>
          <div className="w-10 h-10 rounded-full bg-slate-100 flex items-center justify-center text-slate-600">
            <Bell className="w-5 h-5" />
          </div>
        </div>
        <div className="p-4 bg-white rounded-xl border border-indigo-200 bg-indigo-50/30 shadow-sm flex items-center justify-between">
          <div>
            <p className="text-xs font-semibold text-indigo-600 uppercase tracking-wider">Chưa đọc (Cần xử lý)</p>
            <p className="text-2xl font-bold text-indigo-700 mt-1">{summary?.unreadCount ?? 0}</p>
          </div>
          <div className="w-10 h-10 rounded-full bg-indigo-100 flex items-center justify-center text-indigo-600">
            <AlertCircle className="w-5 h-5" />
          </div>
        </div>
        <div className="p-4 bg-white rounded-xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div>
            <p className="text-xs font-semibold text-slate-500 uppercase tracking-wider">Trạng thái kết nối</p>
            <div className="flex items-center gap-1.5 mt-1">
              <span className="w-2.5 h-2.5 rounded-full bg-emerald-500 animate-pulse"></span>
              <span className="text-sm font-semibold text-slate-800">Trực tuyến (Real-time Active)</span>
            </div>
          </div>
          <div className="w-10 h-10 rounded-full bg-emerald-100 flex items-center justify-center text-emerald-600">
            <CheckCircle2 className="w-5 h-5" />
          </div>
        </div>
      </div>

      {/* Filter / Search Bar */}
      <div className="bg-white p-4 rounded-xl border border-slate-200 flex flex-col md:flex-row items-center justify-between gap-4">
        <div className="relative flex-1 w-full">
          <Search className="w-4 h-4 absolute left-3 top-3 text-slate-400" />
          <input
            type="text"
            placeholder="Tìm kiếm nội dung thông báo..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-4 py-2 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </div>
        <div className="flex flex-wrap items-center gap-3 w-full md:w-auto">
          <select
            value={selectedCategory}
            onChange={(e) => setSelectedCategory(e.target.value)}
            className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
          >
            {CATEGORIES.map(c => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
          <button
            onClick={() => setFilterUnreadOnly(!filterUnreadOnly)}
            className={`px-3 py-2 text-sm font-medium rounded-lg border transition-colors ${
              filterUnreadOnly
                ? 'bg-indigo-600 text-white border-indigo-600'
                : 'bg-white text-slate-700 border-slate-200 hover:bg-slate-50'
            }`}
          >
            {filterUnreadOnly ? 'Đang lọc: Chưa đọc' : 'Chỉ xem chưa đọc'}
          </button>
        </div>
      </div>

      {/* Notification Items List */}
      <div className="space-y-3">
        {loading ? (
          <div className="p-12 text-center text-slate-500 bg-white rounded-xl border border-slate-200">
            Đang tải thông báo...
          </div>
        ) : filteredItems.length === 0 ? (
          <div className="p-12 text-center text-slate-500 bg-white rounded-xl border border-slate-200">
            Không có thông báo nào phù hợp với bộ lọc hiện tại.
          </div>
        ) : (
          filteredItems.map(item => (
            <div
              key={item.id}
              className={`p-4 rounded-xl border transition-all ${
                item.isRead
                  ? 'bg-white border-slate-200 text-slate-600'
                  : 'bg-indigo-50/40 border-indigo-200 text-slate-900 shadow-sm'
              } flex flex-col sm:flex-row items-start justify-between gap-4`}
            >
              <div className="flex items-start gap-3.5 flex-1">
                <div className="mt-1 p-2 rounded-lg bg-white border border-slate-200 shadow-xs">
                  {getCategoryIcon(item.category)}
                </div>
                <div className="space-y-1 flex-1">
                  <div className="flex flex-wrap items-center gap-2">
                    <h3 className="font-semibold text-sm text-slate-900">
                      {item.title}
                    </h3>
                    {getPriorityBadge(item.priority)}
                    <span className="text-xs text-slate-500">
                      [{item.category}]
                    </span>
                    {!item.isRead && (
                      <span className="inline-block w-2 h-2 rounded-full bg-indigo-600"></span>
                    )}
                  </div>
                  <p className="text-sm text-slate-600 leading-relaxed">
                    {item.message}
                  </p>
                  <div className="flex items-center gap-4 text-xs text-slate-400 pt-1">
                    <span>{new Date(item.createdAt).toLocaleString('vi-VN')}</span>
                    {item.link && (
                      <a
                        href={item.link}
                        className="inline-flex items-center gap-1 text-indigo-600 hover:text-indigo-800 font-medium"
                      >
                        Xem chi tiết <ExternalLink className="w-3 h-3" />
                      </a>
                    )}
                  </div>
                </div>
              </div>

              {/* Actions */}
              <div className="flex items-center gap-2 self-end sm:self-center">
                {!item.isRead && (
                  <button
                    onClick={() => handleMarkAsRead(item.id)}
                    className="p-1.5 text-slate-400 hover:text-indigo-600 hover:bg-slate-100 rounded-lg transition-colors"
                    title="Đánh dấu đã đọc"
                  >
                    <CheckCircle2 className="w-4 h-4" />
                  </button>
                )}
                <button
                  onClick={() => handleDelete(item.id)}
                  className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors"
                  title="Xóa thông báo"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            </div>
          ))
        )}
      </div>

      {/* Dispatch Modal */}
      {showDispatchModal && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-white rounded-xl shadow-xl border border-slate-200 w-full max-w-lg overflow-hidden animate-in fade-in zoom-in-95 duration-150">
            <div className="flex items-center justify-between px-6 py-4 border-b border-slate-200">
              <h2 className="text-lg font-semibold text-slate-900 flex items-center gap-2">
                <Send className="w-5 h-5 text-indigo-600" />
                Gửi thông báo mới
              </h2>
              <button
                onClick={() => setShowDispatchModal(false)}
                className="text-slate-400 hover:text-slate-600"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <form onSubmit={handleDispatch} className="p-6 space-y-4">
              <div>
                <label className="block text-xs font-semibold text-slate-700 uppercase mb-1">
                  Tiêu đề thông báo *
                </label>
                <input
                  type="text"
                  required
                  value={dispatchTitle}
                  onChange={(e) => setDispatchTitle(e.target.value)}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  placeholder="Ví dụ: Nhắc nhở nộp báo cáo tuần..."
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 uppercase mb-1">
                  Nội dung chi tiết *
                </label>
                <textarea
                  rows={3}
                  required
                  value={dispatchMessage}
                  onChange={(e) => setDispatchMessage(e.target.value)}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  placeholder="Nội dung thông báo cần truyền đạt..."
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 uppercase mb-1">
                    Danh mục
                  </label>
                  <select
                    value={dispatchCategory}
                    onChange={(e) => setDispatchCategory(e.target.value)}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  >
                    <option value="System">System</option>
                    <option value="Lead">Lead</option>
                    <option value="Deal">Deal</option>
                    <option value="Contract">Contract</option>
                    <option value="Payment">Payment</option>
                    <option value="Approval">Approval</option>
                    <option value="AI">AI</option>
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-semibold text-slate-700 uppercase mb-1">
                    Mức độ ưu tiên
                  </label>
                  <select
                    value={dispatchPriority}
                    onChange={(e) => setDispatchPriority(e.target.value)}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  >
                    <option value="Low">Thấp (Low)</option>
                    <option value="Medium">Bình thường (Medium)</option>
                    <option value="High">Cao (High)</option>
                    <option value="Urgent">Khẩn cấp (Urgent)</option>
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 uppercase mb-1">
                  Đường dẫn liên kết (Link tùy chọn)
                </label>
                <input
                  type="text"
                  value={dispatchLink}
                  onChange={(e) => setDispatchLink(e.target.value)}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  placeholder="/contracts/..."
                />
              </div>

              <div className="pt-4 flex items-center justify-end gap-3 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setShowDispatchModal(false)}
                  className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50"
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  disabled={sending}
                  className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50"
                >
                  <Send className="w-4 h-4" />
                  {sending ? 'Đang phát...' : 'Phát thông báo'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
