import React, { useState, useEffect, useCallback } from 'react';
import dayjs from 'dayjs';
import {
  Calendar as CalendarIcon,
  Plus,
  Video,
  MapPin,
  Clock,
  User,
  Building2,
  Trash2,
  RefreshCw,
  AlertCircle,
  CheckCircle,
  ExternalLink,
} from 'lucide-react';
import { appointmentService } from '../../services/appointmentService';
import { employeeService, type EmployeeSimpleDto } from '../../services/employeeService';
import { CreateAppointmentModal } from './components/CreateAppointmentModal';
import { StatusBadge } from '../../components/common/StatusBadge';
import type { AppointmentDto } from '../../types/crm';

export const AppointmentsPage: React.FC = () => {
  const [appointments, setAppointments] = useState<AppointmentDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeSimpleDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [notification, setNotification] = useState<string | null>(null);

  // Filters
  const [employeeId, setEmployeeId] = useState('');
  const [status, setStatus] = useState('');
  const [isCreateOpen, setIsCreateOpen] = useState(false);

  const fetchAppointments = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const res = await appointmentService.getAppointments({
        employeeId: employeeId || undefined,
        status: status || undefined,
      });

      if (res.success && res.data) {
        setAppointments(res.data);
      } else {
        setError(res.message || 'Failed to fetch appointments.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error loading appointments.');
    } finally {
      setIsLoading(false);
    }
  }, [employeeId, status]);

  useEffect(() => {
    fetchAppointments();
  }, [fetchAppointments]);

  useEffect(() => {
    employeeService.getEmployees().then((res) => {
      if (res.data) setEmployees(res.data);
    }).catch(console.error);
  }, []);

  const handleStatusChange = async (id: string, newStatus: string) => {
    try {
      const res = await appointmentService.updateAppointmentStatus(id, newStatus);
      if (res.success) {
        setNotification(`Appointment status changed to ${newStatus}.`);
        fetchAppointments();
        setTimeout(() => setNotification(null), 3000);
      } else {
        setError(res.message || 'Failed to update appointment.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error updating status.');
    }
  };

  const handleDelete = async (id: string, title: string) => {
    if (!window.confirm(`Are you sure you want to cancel appointment "${title}"?`)) return;
    try {
      const res = await appointmentService.deleteAppointment(id);
      if (res.success) {
        setNotification('Appointment deleted.');
        fetchAppointments();
        setTimeout(() => setNotification(null), 3000);
      } else {
        setError(res.message || 'Failed to delete appointment.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error deleting appointment.');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white flex items-center gap-2">
            <CalendarIcon className="w-6 h-6 text-blue-600" />
            <span>Calendar & Appointments</span>
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400">
            Schedule executive meetings, technical demos, discovery calls, and contract reviews.
          </p>
        </div>

        <button
          onClick={() => setIsCreateOpen(true)}
          className="px-3.5 py-2 text-xs font-semibold text-white bg-blue-600 hover:bg-blue-700 rounded-lg flex items-center gap-1.5 shadow-sm transition-colors"
        >
          <Plus className="w-4 h-4" />
          Schedule Appointment
        </button>
      </div>

      {notification && (
        <div className="p-3 bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 dark:border-emerald-800 rounded-lg text-xs text-emerald-800 dark:text-emerald-300 flex items-center gap-2">
          <CheckCircle className="w-4 h-4 shrink-0 text-emerald-600" />
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
      <div className="p-4 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-2">
          <select
            value={employeeId}
            onChange={(e) => setEmployeeId(e.target.value)}
            className="px-2.5 py-1.5 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
          >
            <option value="">All Host Representatives</option>
            {employees.map((emp) => (
              <option key={emp.id} value={emp.id}>
                {emp.fullName}
              </option>
            ))}
          </select>

          <select
            value={status}
            onChange={(e) => setStatus(e.target.value)}
            className="px-2.5 py-1.5 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
          >
            <option value="">All Statuses</option>
            <option value="Scheduled">Scheduled</option>
            <option value="Completed">Completed</option>
            <option value="Rescheduled">Rescheduled</option>
            <option value="Cancelled">Cancelled</option>
          </select>

          <button
            onClick={() => {
              setEmployeeId('');
              setStatus('');
            }}
            className="px-2.5 py-1.5 text-xs text-slate-500 hover:text-slate-700 dark:hover:text-slate-300 transition-colors"
          >
            Reset
          </button>
        </div>

        <button
          onClick={fetchAppointments}
          title="Refresh Agenda"
          className="p-1.5 border rounded-lg text-slate-500 hover:text-slate-800 dark:hover:text-slate-200 border-slate-300 dark:border-slate-700 transition-colors"
        >
          <RefreshCw className={`w-3.5 h-3.5 ${isLoading ? 'animate-spin text-blue-500' : ''}`} />
        </button>
      </div>

      {/* Appointment Cards Grid */}
      {isLoading ? (
        <div className="py-20 text-center text-slate-400">
          <RefreshCw className="w-8 h-8 animate-spin mx-auto mb-2 text-blue-500" />
          <span>Loading agenda...</span>
        </div>
      ) : appointments.length === 0 ? (
        <div className="py-20 text-center text-slate-400 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs">
          <CalendarIcon className="w-8 h-8 mx-auto mb-2 opacity-40" />
          <p className="font-medium text-slate-600 dark:text-slate-300">No scheduled appointments</p>
          <p className="text-xs text-slate-400 mt-0.5">Click Schedule Appointment above to add client meetings.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {appointments.map((appt) => {
            const start = dayjs(appt.startTime);
            const end = dayjs(appt.endTime);

            return (
              <div
                key={appt.id}
                className="p-5 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs flex flex-col justify-between hover:border-blue-300 dark:hover:border-blue-700 transition-all space-y-4"
              >
                <div className="space-y-2">
                  <div className="flex items-start justify-between gap-2">
                    <span className="font-bold text-sm text-slate-900 dark:text-white line-clamp-2">
                      {appt.title}
                    </span>
                    <StatusBadge status={appt.status} />
                  </div>

                  <div className="p-2.5 rounded-lg bg-blue-50/60 dark:bg-blue-950/30 border border-blue-100 dark:border-blue-900/40 text-xs text-blue-900 dark:text-blue-300 space-y-1">
                    <div className="flex items-center gap-1.5 font-semibold">
                      <CalendarIcon className="w-3.5 h-3.5 text-blue-600 shrink-0" />
                      <span>{start.format('dddd, MMMM D, YYYY')}</span>
                    </div>
                    <div className="flex items-center gap-1.5 text-[11px] text-blue-700 dark:text-blue-400">
                      <Clock className="w-3 h-3 text-blue-500 shrink-0" />
                      <span>
                        {start.format('HH:mm')} - {end.format('HH:mm')} ({end.diff(start, 'minute')} mins)
                      </span>
                    </div>
                  </div>

                  {appt.description && (
                    <p className="text-xs text-slate-500 dark:text-slate-400 line-clamp-2">
                      {appt.description}
                    </p>
                  )}

                  <div className="space-y-1.5 text-xs text-slate-600 dark:text-slate-300 pt-1">
                    <div className="flex items-center gap-1.5">
                      <User className="w-3.5 h-3.5 text-slate-400 shrink-0" />
                      <span>Host: <span className="font-medium text-slate-800 dark:text-slate-200">{appt.employeeName}</span></span>
                    </div>

                    {appt.accountName && (
                      <div className="flex items-center gap-1.5">
                        <Building2 className="w-3.5 h-3.5 text-slate-400 shrink-0" />
                        <span className="font-medium text-slate-800 dark:text-slate-200">{appt.accountName}</span>
                      </div>
                    )}

                    {appt.meetingLink && (
                      <div className="flex items-center gap-1.5 text-blue-600 font-medium">
                        <Video className="w-3.5 h-3.5 shrink-0" />
                        <a
                          href={appt.meetingLink}
                          target="_blank"
                          rel="noreferrer"
                          className="hover:underline flex items-center gap-1 truncate"
                        >
                          Join Conference Call <ExternalLink className="w-2.5 h-2.5" />
                        </a>
                      </div>
                    )}

                    {appt.location && !appt.meetingLink && (
                      <div className="flex items-center gap-1.5 text-slate-500">
                        <MapPin className="w-3.5 h-3.5 shrink-0" />
                        <span>{appt.location}</span>
                      </div>
                    )}
                  </div>
                </div>

                <div className="pt-3 border-t border-slate-100 dark:border-slate-700/60 flex items-center justify-between text-xs">
                  <select
                    value={appt.status}
                    onChange={(e) => handleStatusChange(appt.id, e.target.value)}
                    className="text-[11px] px-2 py-1 rounded border bg-slate-50 dark:bg-slate-900 border-slate-200 dark:border-slate-700 text-slate-700 dark:text-slate-300 font-medium"
                  >
                    <option value="Scheduled">Scheduled</option>
                    <option value="Completed">Completed</option>
                    <option value="Rescheduled">Rescheduled</option>
                    <option value="Cancelled">Cancelled</option>
                  </select>

                  <button
                    onClick={() => handleDelete(appt.id, appt.title)}
                    className="p-1.5 text-slate-400 hover:text-red-600 transition-colors"
                  >
                    <Trash2 className="w-3.5 h-3.5" />
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}

      <CreateAppointmentModal
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        onSuccess={() => {
          setNotification('Appointment scheduled.');
          fetchAppointments();
          setTimeout(() => setNotification(null), 3000);
        }}
      />
    </div>
  );
};
