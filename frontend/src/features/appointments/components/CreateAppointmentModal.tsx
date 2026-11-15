import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { AlertCircle, Loader2 } from 'lucide-react';
import { Modal } from '../../../components/common/Modal';
import { appointmentService } from '../../../services/appointmentService';
import { employeeService, type EmployeeSimpleDto } from '../../../services/employeeService';
import { accountService } from '../../../services/accountService';
import type { CreateAppointmentRequest, AccountDto } from '../../../types/crm';

const createAppointmentSchema = z.object({
  title: z.string().min(2, 'Meeting title is required'),
  description: z.string().optional(),
  startTime: z.string().min(1, 'Start time is required'),
  endTime: z.string().min(1, 'End time is required'),
  location: z.string().optional(),
  meetingLink: z.string().optional(),
  employeeId: z.string().min(1, 'Host employee is required'),
  accountId: z.string().optional(),
});

type CreateAppointmentFormData = z.infer<typeof createAppointmentSchema>;

interface CreateAppointmentModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const CreateAppointmentModal: React.FC<CreateAppointmentModalProps> = ({
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
  } = useForm<CreateAppointmentFormData>({
    resolver: zodResolver(createAppointmentSchema),
    defaultValues: {
      title: '',
      location: 'Virtual Conference',
    },
  });

  useEffect(() => {
    if (isOpen) {
      reset();
      setError(null);
      employeeService.getEmployees().then((res) => {
        if (res.data) setEmployees(res.data);
      }).catch(console.error);

      accountService.getAccounts({ pageSize: 50 }).then((res) => {
        if (res.data?.items) setAccounts(res.data.items);
      }).catch(console.error);
    }
  }, [isOpen, reset]);

  const onSubmit = async (data: CreateAppointmentFormData) => {
    try {
      setIsSubmitting(true);
      setError(null);

      const payload: CreateAppointmentRequest = {
        title: data.title.trim(),
        description: data.description?.trim() || undefined,
        startTime: new Date(data.startTime).toISOString(),
        endTime: new Date(data.endTime).toISOString(),
        location: data.location?.trim() || undefined,
        meetingLink: data.meetingLink?.trim() || undefined,
        employeeId: data.employeeId,
        accountId: data.accountId || undefined,
      };

      const res = await appointmentService.createAppointment(payload);
      if (res.success) {
        onSuccess();
        onClose();
      } else {
        setError(res.message || 'Failed to schedule appointment.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error scheduling appointment.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Schedule Client Meeting / Appointment"
      description="Book executive reviews, demonstrations, discovery calls, or consultations."
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
            Meeting Title *
          </label>
          <input
            type="text"
            {...register('title')}
            placeholder="e.g. SOW Walkthrough & Commercial Review"
            className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
          />
          {errors.title && (
            <p className="text-red-500 text-xs mt-1">{errors.title.message}</p>
          )}
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Start Time *
            </label>
            <input
              type="datetime-local"
              {...register('startTime')}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
            {errors.startTime && (
              <p className="text-red-500 text-xs mt-1">{errors.startTime.message}</p>
            )}
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              End Time *
            </label>
            <input
              type="datetime-local"
              {...register('endTime')}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
            {errors.endTime && (
              <p className="text-red-500 text-xs mt-1">{errors.endTime.message}</p>
            )}
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Host / Sales Lead *
            </label>
            <select
              {...register('employeeId')}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            >
              <option value="">-- Choose Host --</option>
              {employees.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.fullName}
                </option>
              ))}
            </select>
            {errors.employeeId && (
              <p className="text-red-500 text-xs mt-1">{errors.employeeId.message}</p>
            )}
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

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Location / Room
            </label>
            <input
              type="text"
              {...register('location')}
              placeholder="e.g. Google Meet / Room 302"
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Meeting URL / Link
            </label>
            <input
              type="text"
              {...register('meetingLink')}
              placeholder="https://meet.google.com/..."
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        <div>
          <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
            Agenda / Discussion Points
          </label>
          <textarea
            rows={2}
            {...register('description')}
            placeholder="Agenda items, objectives, attendee notes..."
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
                <span>Scheduling...</span>
              </>
            ) : (
              <span>Schedule Appointment</span>
            )}
          </button>
        </div>
      </form>
    </Modal>
  );
};
