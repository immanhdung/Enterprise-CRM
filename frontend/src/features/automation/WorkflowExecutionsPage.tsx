import React, { useState, useEffect } from 'react';
import { AutomationNavigation } from './AutomationNavigation';
import { automationService } from '../../services/automationService';
import type { WorkflowExecution } from '../../types/automation';
import { RefreshCw, CheckCircle2, AlertTriangle, XCircle, ChevronDown, ChevronRight, Clock } from 'lucide-react';

export const WorkflowExecutionsPage: React.FC = () => {
  const [executions, setExecutions] = useState<WorkflowExecution[]>([]);
  const [loading, setLoading] = useState(true);
  const [expandedId, setExpandedId] = useState<string | null>(null);

  const fetchExecutions = async () => {
    try {
      setLoading(true);
      const data = await automationService.getExecutions();
      setExecutions(data);
    } catch (err) {
      console.error('Failed to load executions:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchExecutions();
  }, []);

  const toggleExpand = (id: string) => {
    setExpandedId(expandedId === id ? null : id);
  };

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Lịch sử Thực thi (Workflow Executions)</h1>
          <p className="text-sm text-gray-500">Chi tiết nhật ký chạy tự động, thời gian xử lý và trạng thái từng bước</p>
        </div>
        <button
          onClick={fetchExecutions}
          disabled={loading}
          className="flex items-center gap-2 px-3 py-1.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
        >
          <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
          Làm mới
        </button>
      </div>

      <AutomationNavigation />

      {loading ? (
        <div className="flex justify-center items-center h-48 bg-white rounded-lg border border-gray-200">
          <RefreshCw className="w-6 h-6 animate-spin text-blue-600" />
        </div>
      ) : executions.length === 0 ? (
        <div className="bg-white rounded-lg border border-gray-200 p-8 text-center text-gray-500">
          Chưa có nhật ký thực thi quy trình nào được ghi lại.
        </div>
      ) : (
        <div className="space-y-3">
          {executions.map((e) => {
            const isExpanded = expandedId === e.id;
            return (
              <div key={e.id} className="bg-white rounded-lg border border-gray-200 shadow-sm overflow-hidden">
                <div
                  onClick={() => toggleExpand(e.id)}
                  className="flex items-center justify-between p-4 cursor-pointer hover:bg-gray-50 transition-colors"
                >
                  <div className="flex items-center gap-4">
                    <button className="text-gray-400">
                      {isExpanded ? <ChevronDown className="w-5 h-5" /> : <ChevronRight className="w-5 h-5" />}
                    </button>
                    <div>
                      <div className="flex items-center gap-2">
                        <span className="font-semibold text-gray-900">{e.workflowName || 'Quy trình Tự động'}</span>
                        <span className="font-mono text-xs bg-purple-50 text-purple-700 px-2 py-0.5 rounded border border-purple-200">
                          {e.triggerEvent}
                        </span>
                      </div>
                      <div className="text-xs text-gray-500 mt-1">
                        Đối tượng: <span className="font-medium text-gray-700">{e.entityType} ({e.entityId})</span>
                      </div>
                    </div>
                  </div>

                  <div className="flex items-center gap-6 text-sm">
                    <div className="flex items-center gap-1.5 text-gray-500 text-xs">
                      <Clock className="w-4 h-4" />
                      <span>{e.durationMs} ms</span>
                    </div>

                    <div>
                      {e.status === 'Success' && (
                        <span className="inline-flex items-center gap-1 text-green-700 bg-green-50 px-2.5 py-1 rounded-full text-xs font-medium border border-green-200">
                          <CheckCircle2 className="w-3.5 h-3.5" /> Thành công
                        </span>
                      )}
                      {e.status === 'Partial' && (
                        <span className="inline-flex items-center gap-1 text-amber-700 bg-amber-50 px-2.5 py-1 rounded-full text-xs font-medium border border-amber-200">
                          <AlertTriangle className="w-3.5 h-3.5" /> Một phần
                        </span>
                      )}
                      {e.status === 'Failed' && (
                        <span className="inline-flex items-center gap-1 text-red-700 bg-red-50 px-2.5 py-1 rounded-full text-xs font-medium border border-red-200">
                          <XCircle className="w-3.5 h-3.5" /> Thất bại
                        </span>
                      )}
                      {e.status === 'Skipped' && (
                        <span className="inline-flex items-center gap-1 text-gray-600 bg-gray-50 px-2.5 py-1 rounded-full text-xs font-medium border border-gray-200">
                          Bỏ qua
                        </span>
                      )}
                    </div>

                    <div className="text-xs text-gray-500 w-36 text-right">
                      {new Date(e.executedAt).toLocaleString('vi-VN')}
                    </div>
                  </div>
                </div>

                {isExpanded && (
                  <div className="bg-gray-50 p-4 border-t border-gray-200">
                    <h4 className="text-xs font-bold text-gray-700 uppercase tracking-wider mb-2">
                      Chi tiết từng bước ({e.logs?.length || 0} bước)
                    </h4>
                    {e.resultSummary && (
                      <p className="text-xs text-gray-600 mb-3 bg-white p-2.5 rounded border border-gray-200">
                        {e.resultSummary}
                      </p>
                    )}
                    {e.logs && e.logs.length > 0 ? (
                      <div className="space-y-2">
                        {e.logs.map((log) => (
                          <div
                            key={log.id}
                            className="flex items-center justify-between bg-white p-3 rounded border border-gray-200 text-xs"
                          >
                            <div className="flex items-center gap-2">
                              <span className="font-semibold text-gray-800">{log.stepName}</span>
                              <span className="text-gray-400">({log.actionType})</span>
                            </div>
                            <div className="flex items-center gap-4">
                              <span
                                className={`px-2 py-0.5 rounded text-[11px] font-medium ${
                                  log.status === 'Success'
                                    ? 'bg-green-100 text-green-800'
                                    : log.status === 'Failed'
                                    ? 'bg-red-100 text-red-800'
                                    : 'bg-gray-100 text-gray-800'
                                }`}
                              >
                                {log.status}
                              </span>
                              <span className="text-gray-500">
                                {new Date(log.executedAt).toLocaleTimeString('vi-VN')}
                              </span>
                            </div>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <p className="text-xs text-gray-400 italic">Không có nhật ký chi tiết các bước.</p>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};
