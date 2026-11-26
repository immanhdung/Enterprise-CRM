import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { AutomationNavigation } from './AutomationNavigation';
import { automationService } from '../../services/automationService';
import type { WorkflowDefinition } from '../../types/automation';
import { Plus, Play, Trash2, Edit, CheckCircle2, XCircle, RefreshCw } from 'lucide-react';

export const WorkflowsPage: React.FC = () => {
  const [workflows, setWorkflows] = useState<WorkflowDefinition[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [triggeringId, setTriggeringId] = useState<string | null>(null);

  const fetchWorkflows = async () => {
    try {
      setLoading(true);
      const data = await automationService.getWorkflows();
      setWorkflows(data);
    } catch (err) {
      console.error('Error fetching workflows:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchWorkflows();
  }, []);

  const handleTrigger = async (workflow: WorkflowDefinition) => {
    try {
      setTriggeringId(workflow.id);
      await automationService.triggerWorkflow(workflow.triggerEvent, 'ManualTrigger', workflow.id);
      alert(`Quy trình "${workflow.name}" đã được kích hoạt thành công.`);
      fetchWorkflows();
    } catch (err: any) {
      alert(`Lỗi kích hoạt: ${err?.message || 'Có lỗi xảy ra'}`);
    } finally {
      setTriggeringId(null);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa quy trình này?')) return;
    try {
      await automationService.deleteWorkflow(id);
      fetchWorkflows();
    } catch (err) {
      console.error('Error deleting workflow:', err);
    }
  };

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Quy trình Tự động (Workflow Automation)</h1>
          <p className="text-sm text-gray-500">
            Quản lý các kịch bản tự động hóa theo sự kiện: phân bổ lead, tạo tác vụ, gửi cảnh báo
          </p>
        </div>
        <Link
          to="/automation/workflows/create"
          className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium text-sm transition-colors shadow-sm"
        >
          <Plus className="w-4 h-4" />
          <span>Tạo Quy trình Mới</span>
        </Link>
      </div>

      <AutomationNavigation />

      {loading ? (
        <div className="flex justify-center items-center h-48 bg-white rounded-lg border border-gray-200">
          <RefreshCw className="w-6 h-6 animate-spin text-blue-600" />
        </div>
      ) : workflows.length === 0 ? (
        <div className="bg-white rounded-lg border border-gray-200 p-8 text-center">
          <p className="text-gray-500 mb-4">Chưa có quy trình tự động nào được thiết lập.</p>
          <Link
            to="/automation/workflows/create"
            className="inline-flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium"
          >
            <Plus className="w-4 h-4" />
            Tạo quy trình đầu tiên
          </Link>
        </div>
      ) : (
        <div className="bg-white rounded-lg border border-gray-200 shadow-sm overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200 text-sm">
            <thead className="bg-gray-50 text-gray-700 font-semibold">
              <tr>
                <th className="px-6 py-3 text-left">Tên quy trình</th>
                <th className="px-6 py-3 text-left">Phân loại</th>
                <th className="px-6 py-3 text-left">Sự kiện kích hoạt</th>
                <th className="px-6 py-3 text-center">Thứ tự</th>
                <th className="px-6 py-3 text-center">Trạng thái</th>
                <th className="px-6 py-3 text-center">Lượt chạy</th>
                <th className="px-6 py-3 text-left">Lần chạy cuối</th>
                <th className="px-6 py-3 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {workflows.map((w) => (
                <tr key={w.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-6 py-4 font-medium text-gray-900">
                    <div>{w.name}</div>
                    {w.description && <div className="text-xs text-gray-500">{w.description}</div>}
                  </td>
                  <td className="px-6 py-4">
                    <span className="inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-50 text-blue-700 border border-blue-200">
                      {w.category}
                    </span>
                  </td>
                  <td className="px-6 py-4 font-mono text-xs text-purple-700 font-medium">
                    {w.triggerEvent}
                  </td>
                  <td className="px-6 py-4 text-center text-gray-600">{w.executionOrder}</td>
                  <td className="px-6 py-4 text-center">
                    {w.isActive ? (
                      <span className="inline-flex items-center gap-1 text-green-700 bg-green-50 px-2 py-0.5 rounded-full text-xs font-medium border border-green-200">
                        <CheckCircle2 className="w-3.5 h-3.5" /> Hoạt động
                      </span>
                    ) : (
                      <span className="inline-flex items-center gap-1 text-gray-600 bg-gray-50 px-2 py-0.5 rounded-full text-xs font-medium border border-gray-200">
                        <XCircle className="w-3.5 h-3.5" /> Tạm dừng
                      </span>
                    )}
                  </td>
                  <td className="px-6 py-4 text-center">
                    <div className="font-semibold text-gray-900">{w.executionCount}</div>
                    {w.errorCount > 0 && (
                      <div className="text-xs text-red-600 font-medium">{w.errorCount} lỗi</div>
                    )}
                  </td>
                  <td className="px-6 py-4 text-gray-500 text-xs">
                    {w.lastExecutedAt
                      ? new Date(w.lastExecutedAt).toLocaleString('vi-VN')
                      : 'Chưa chạy'}
                  </td>
                  <td className="px-6 py-4 text-right space-x-2">
                    <button
                      onClick={() => handleTrigger(w)}
                      disabled={triggeringId === w.id}
                      title="Chạy thử nghiệm"
                      className="inline-flex items-center gap-1 px-2.5 py-1 text-xs font-medium text-blue-600 hover:text-blue-800 bg-blue-50 hover:bg-blue-100 rounded border border-blue-200"
                    >
                      <Play className="w-3 h-3" />
                      {triggeringId === w.id ? 'Đang chạy...' : 'Chạy thử'}
                    </button>
                    <Link
                      to={`/automation/workflows/${w.id}`}
                      title="Chỉnh sửa"
                      className="inline-flex items-center p-1 text-gray-500 hover:text-gray-700"
                    >
                      <Edit className="w-4 h-4" />
                    </Link>
                    <button
                      onClick={() => handleDelete(w.id)}
                      title="Xóa"
                      className="inline-flex items-center p-1 text-red-500 hover:text-red-700"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};
