import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { AlertCircle, Loader2 } from 'lucide-react';
import { Modal } from '../../../components/common/Modal';
import { taskService } from '../../../services/taskService';
import { employeeService, type EmployeeSimpleDto } from '../../../services/employeeService';
import { accountService } from '../../../services/accountService';
import type { CreateTaskRequest, AccountDto } from '../../../types/crm';

const createTaskSchema = z.object({
  title: z.string().min(2, 'Task title is required'),
  description: z.string().optional(),
  priority: z.enum(['Low', 'Medium', 'High', 'Urgent']),
  startDate: z.string().optional(),
  dueDate: z.string().optional(),
  assignedEmployeeId: z.string().optional(),
  accountId: z.string().optional(),
});

type CreateTaskFormData = z.infer<typeof createTaskSchema>;

interface CreateTaskModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const CreateTaskModal: React.FC<CreateTaskModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [employees, setEmployees] = useState<EmployeeSimpleDto[]>([]);
  const [accounts, setAccounts] = useState<AccountDto[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateTaskFormData>({
    resolver: zodResolver(createTaskSchema),
    defaultValues: {
      title: '',
      priority: 'Medium',
    },
  });

  useEffect(() => {
    if (isOpen) {
      reset({ title: '', priority: 'Medium' });
      setError(null);
      employeeService.getEmployees().then((res) => {
        if (res.data) setEmployees(res.data);
      }).catch(console.error);

      accountService.getAccounts({ pageSize: 50 }).then((res) => {
        if (res.data?.items) setAccounts(res.data.items);
      }).catch(console.error);
    }
  }, [isOpen, reset]);

  const onSubmit = async (data: CreateTaskFormData) => {
    try {
      setIsSubmitting(true);
      setError(null);

      const payload: CreateTaskRequest = {
        title: data.title.trim(),
        description: data.description?.trim() || undefined,
        priority: data.priority,
        startDate: data.startDate ? new Date(data.startDate).toISOString() : undefined,
        dueDate: data.dueDate ? new Date(data.dueDate).toISOString() : undefined,
        assignedEmployeeId: data.assignedEmployeeId || undefined,
        accountId: data.accountId || undefined,
      };

      const res = await taskService.createTask(payload);
      if (res.success) {
        onSuccess();
        onClose();
      } else {
        setError(res.message || 'Failed to create task.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error creating task.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Create Action Item / Task"
      description="Schedule a follow-up, client deliverable, or internal CRM action item."
      maxWidth="md"
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {error && (
          <div className="p-3 bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 rounded-lg text-xs text-red-600 dark:text-red-400 flex items-center gap-2">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{error}</span>
          </div>
        )}

        <div>
          <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
            Task Subject / Title *
          </label>
          <input
            type="text"
            {...register('title')}
            placeholder="e.g. Prepare Technical SOW for client sign-off"
            className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
          />
          {errors.title && (
            <p className="text-red-500 text-xs mt-1">{errors.title.message}</p>
          )}
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Priority
            </label>
            <select
              {...register('priority')}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            >
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
              <option value="Urgent">Urgent</option>
            </select>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Due Date
            </label>
            <input
              type="date"
              {...register('dueDate')}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Assignee
            </label>
            <select
              {...register('assignedEmployeeId')}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Unassigned</option>
              {employees.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.fullName}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Associated Client Account
            </label>
            <select
              {...register('accountId')}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            >
              <option value="">None</option>
              {accounts.map((acc) => (
                <option key={acc.id} value={acc.id}>
                  {acc.name}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div>
          <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
            Task Notes / Instructions
          </label>
          <textarea
            rows={2}
            {...register('description')}
            placeholder="Action items, criteria for done, customer references..."
            className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div className="flex justify-end gap-3 pt-3 border-t border-slate-100 dark:border-slate-800">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={isSubmitting}
            className="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 rounded-lg flex items-center gap-2 transition-colors shadow-sm"
          >
            {isSubmitting ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin" />
                <span>Creating...</span>
              </>
            ) : (
              <span>Create Task</span>
            )}
          </button>
        </div>
      </form>
    </Modal>
  );
};
