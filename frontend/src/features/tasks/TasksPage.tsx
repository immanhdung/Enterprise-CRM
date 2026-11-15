import React, { useState, useEffect, useCallback } from 'react';
import dayjs from 'dayjs';
import {
  CheckSquare,
  Square,
  Plus,
  Search,
  AlertCircle,
  CheckCircle2,
  Calendar,
  User,
  Trash2,
  RefreshCw,
  Building2,
} from 'lucide-react';
import { taskService } from '../../services/taskService';
import { employeeService, type EmployeeSimpleDto } from '../../services/employeeService';
import { CreateTaskModal } from './components/CreateTaskModal';
import { StatusBadge } from '../../components/common/StatusBadge';
import type { TaskDto, PagedResult } from '../../types/crm';

export const TasksPage: React.FC = () => {
  const [data, setData] = useState<PagedResult<TaskDto>>({
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 15,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  });
  const [employees, setEmployees] = useState<EmployeeSimpleDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [notification, setNotification] = useState<string | null>(null);

  // Filters
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [priority, setPriority] = useState('');
  const [isOverdue, setIsOverdue] = useState(false);
  const [assignedEmployeeId, setAssignedEmployeeId] = useState('');
  const [pageNumber, setPageNumber] = useState(1);
  const [isCreateOpen, setIsCreateOpen] = useState(false);

  const fetchTasks = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const res = await taskService.getTasks({
        pageNumber,
        pageSize: 15,
        search: search.trim() || undefined,
        status: status || undefined,
        priority: priority || undefined,
        isOverdue: isOverdue ? true : undefined,
        assignedEmployeeId: assignedEmployeeId || undefined,
      });

      if (res.success && res.data) {
        setData(res.data);
      } else {
        setError(res.message || 'Failed to fetch tasks.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error loading tasks.');
    } finally {
      setIsLoading(false);
    }
  }, [pageNumber, search, status, priority, isOverdue, assignedEmployeeId]);

  useEffect(() => {
    fetchTasks();
  }, [fetchTasks]);

  useEffect(() => {
    employeeService.getEmployees().then((res) => {
      if (res.data) setEmployees(res.data);
    }).catch(console.error);
  }, []);

  const handleToggleStatus = async (task: TaskDto) => {
    const newStatus = task.status === 'Completed' ? 'Pending' : 'Completed';
    try {
      const res = await taskService.updateTaskStatus(task.id, newStatus);
      if (res.success) {
        setNotification(`Task marked as ${newStatus}.`);
        fetchTasks();
        setTimeout(() => setNotification(null), 3000);
      } else {
        setError(res.message || 'Failed to update task.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error updating task.');
    }
  };

  const handleDelete = async (id: string, title: string) => {
    if (!window.confirm(`Are you sure you want to delete task "${title}"?`)) return;
    try {
      const res = await taskService.deleteTask(id);
      if (res.success) {
        setNotification(`Task deleted.`);
        fetchTasks();
        setTimeout(() => setNotification(null), 3000);
      } else {
        setError(res.message || 'Failed to delete task.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error deleting task.');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white">
            Task & Action Items
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400">
            Follow up on leads, opportunities, service milestones, and deliverables.
          </p>
        </div>

        <button
          onClick={() => setIsCreateOpen(true)}
          className="px-3.5 py-2 text-xs font-semibold text-white bg-blue-600 hover:bg-blue-700 rounded-lg flex items-center gap-1.5 shadow-sm transition-colors"
        >
          <Plus className="w-4 h-4" />
          New Task
        </button>
      </div>

      {notification && (
        <div className="p-3 bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 dark:border-emerald-800 rounded-lg text-xs text-emerald-800 dark:text-emerald-300 flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4 shrink-0 text-emerald-600" />
          <span>{notification}</span>
        </div>
      )}

      {error && (
        <div className="p-3 bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 rounded-lg text-xs text-red-600 dark:text-red-400 flex items-center gap-2">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      {/* Filter Bar */}
      <div className="p-4 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-3">
        <div className="flex flex-col md:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-2.5" />
            <input
              type="text"
              placeholder="Search tasks..."
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setPageNumber(1);
              }}
              className="w-full pl-9 pr-3 py-2 border rounded-lg text-xs bg-slate-50 dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div className="flex flex-wrap items-center gap-2">
            <select
              value={status}
              onChange={(e) => {
                setStatus(e.target.value);
                setPageNumber(1);
              }}
              className="px-2.5 py-2 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
            >
              <option value="">All Statuses</option>
              <option value="Pending">Pending</option>
              <option value="InProgress">In Progress</option>
              <option value="Completed">Completed</option>
            </select>

            <select
              value={priority}
              onChange={(e) => {
                setPriority(e.target.value);
                setPageNumber(1);
              }}
              className="px-2.5 py-2 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
            >
              <option value="">All Priorities</option>
              <option value="Urgent">Urgent</option>
              <option value="High">High</option>
              <option value="Medium">Medium</option>
              <option value="Low">Low</option>
            </select>

            <button
              type="button"
              onClick={() => {
                setIsOverdue(!isOverdue);
                setPageNumber(1);
              }}
              className={`px-3 py-2 rounded-lg text-xs font-semibold transition-colors ${
                isOverdue
                  ? 'bg-rose-100 text-rose-700 dark:bg-rose-950/60 dark:text-rose-400 border border-rose-300'
                  : 'bg-slate-100 dark:bg-slate-700 text-slate-600 dark:text-slate-300 border border-slate-200 dark:border-slate-600'
              }`}
            >
              Overdue Only
            </button>

            <select
              value={assignedEmployeeId}
              onChange={(e) => {
                setAssignedEmployeeId(e.target.value);
                setPageNumber(1);
              }}
              className="px-2.5 py-2 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
            >
              <option value="">All Assignees</option>
              {employees.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.fullName}
                </option>
              ))}
            </select>

            <button
              onClick={() => {
                setSearch('');
                setStatus('');
                setPriority('');
                setIsOverdue(false);
                setAssignedEmployeeId('');
                setPageNumber(1);
              }}
              className="px-2.5 py-2 text-xs text-slate-500 hover:text-slate-700 dark:hover:text-slate-300 transition-colors"
            >
              Reset
            </button>
          </div>
        </div>
      </div>

      {/* Task List */}
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs divide-y divide-slate-100 dark:divide-slate-700/60">
        {isLoading ? (
          <div className="py-16 text-center text-slate-400">
            <RefreshCw className="w-6 h-6 animate-spin mx-auto mb-2 text-blue-500" />
            <span>Loading tasks...</span>
          </div>
        ) : data.items.length === 0 ? (
          <div className="py-16 text-center text-slate-400">
            <CheckSquare className="w-8 h-8 mx-auto mb-2 opacity-40" />
            <p className="font-medium text-slate-600 dark:text-slate-300">No tasks found</p>
            <p className="text-xs text-slate-400 mt-0.5">All clear or adjust filter parameters.</p>
          </div>
        ) : (
          data.items.map((task) => {
            const isTaskOverdue =
              task.dueDate &&
              task.status !== 'Completed' &&
              new Date(task.dueDate) < new Date();

            return (
              <div
                key={task.id}
                className="p-4 flex items-start justify-between gap-4 hover:bg-slate-50/70 dark:hover:bg-slate-700/20 transition-colors"
              >
                <div className="flex items-start gap-3">
                  <button
                    onClick={() => handleToggleStatus(task)}
                    className="mt-0.5 text-slate-400 hover:text-blue-600 transition-colors"
                  >
                    {task.status === 'Completed' ? (
                      <CheckSquare className="w-5 h-5 text-emerald-600 fill-emerald-100 dark:fill-emerald-950/40" />
                    ) : (
                      <Square className="w-5 h-5" />
                    )}
                  </button>

                  <div className="space-y-1">
                    <div className="flex flex-wrap items-center gap-2">
                      <span
                        className={`font-semibold text-sm ${
                          task.status === 'Completed'
                            ? 'line-through text-slate-400 dark:text-slate-500'
                            : 'text-slate-900 dark:text-white'
                        }`}
                      >
                        {task.title}
                      </span>

                      <span
                        className={`text-[10px] font-semibold px-2 py-0.5 rounded-full ${
                          task.priority === 'Urgent'
                            ? 'bg-rose-100 text-rose-700 dark:bg-rose-950/60 dark:text-rose-400'
                            : task.priority === 'High'
                            ? 'bg-orange-100 text-orange-700 dark:bg-orange-950/60 dark:text-orange-400'
                            : task.priority === 'Medium'
                            ? 'bg-blue-100 text-blue-700 dark:bg-blue-950/60 dark:text-blue-400'
                            : 'bg-slate-100 text-slate-600 dark:bg-slate-700 dark:text-slate-300'
                        }`}
                      >
                        {task.priority}
                      </span>

                      <StatusBadge status={task.status} />
                    </div>

                    {task.description && (
                      <p className="text-xs text-slate-500 dark:text-slate-400 leading-relaxed">
                        {task.description}
                      </p>
                    )}

                    <div className="flex flex-wrap items-center gap-4 text-xs text-slate-400 pt-1">
                      {task.dueDate && (
                        <span
                          className={`flex items-center gap-1 font-medium ${
                            isTaskOverdue ? 'text-rose-600 font-bold' : ''
                          }`}
                        >
                          <Calendar className="w-3.5 h-3.5" />
                          Due: {dayjs(task.dueDate).format('MMM D, YYYY')}
                          {isTaskOverdue && ' (Overdue)'}
                        </span>
                      )}

                      {task.assignedEmployeeName && (
                        <span className="flex items-center gap-1 text-slate-600 dark:text-slate-300 font-medium">
                          <User className="w-3.5 h-3.5" />
                          {task.assignedEmployeeName}
                        </span>
                      )}

                      {task.accountName && (
                        <span className="flex items-center gap-1 text-blue-600">
                          <Building2 className="w-3.5 h-3.5" />
                          {task.accountName}
                        </span>
                      )}
                    </div>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <button
                    onClick={() => handleDelete(task.id, task.title)}
                    className="p-1.5 text-slate-400 hover:text-red-600 rounded transition-colors"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>
            );
          })
        )}

        {/* Pagination Footer */}
        <div className="p-4 bg-slate-50 dark:bg-slate-900/40 flex items-center justify-between text-xs text-slate-500">
          <div>
            Showing <span className="font-semibold">{data.items.length}</span> of{' '}
            <span className="font-semibold">{data.totalCount}</span> tasks
          </div>

          <div className="flex items-center gap-2">
            <button
              disabled={!data.hasPreviousPage}
              onClick={() => setPageNumber(pageNumber - 1)}
              className="px-3 py-1.5 rounded border border-slate-300 dark:border-slate-700 disabled:opacity-40 hover:bg-white dark:hover:bg-slate-800 transition-colors"
            >
              Previous
            </button>
            <button
              disabled={!data.hasNextPage}
              onClick={() => setPageNumber(pageNumber + 1)}
              className="px-3 py-1.5 rounded border border-slate-300 dark:border-slate-700 disabled:opacity-40 hover:bg-white dark:hover:bg-slate-800 transition-colors"
            >
              Next
            </button>
          </div>
        </div>
      </div>

      <CreateTaskModal
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        onSuccess={() => {
          setNotification('Task created.');
          fetchTasks();
          setTimeout(() => setNotification(null), 3000);
        }}
      />
    </div>
  );
};
