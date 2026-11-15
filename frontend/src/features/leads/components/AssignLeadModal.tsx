import React, { useState, useEffect } from 'react';
import { Loader2, Users, Shuffle, BarChart3, AlertCircle } from 'lucide-react';
import { Modal } from '../../../components/common/Modal';
import { leadService } from '../../../services/leadService';
import { employeeService, type EmployeeSimpleDto } from '../../../services/employeeService';
import type { LeadDto } from '../../../types/crm';

interface AssignLeadModalProps {
  isOpen: boolean;
  onClose: () => void;
  lead: LeadDto | null;
  onSuccess: () => void;
}

export const AssignLeadModal: React.FC<AssignLeadModalProps> = ({
  isOpen,
  onClose,
  lead,
  onSuccess,
}) => {
  const [mode, setMode] = useState<'Manual' | 'RoundRobin' | 'WorkloadBased'>('Manual');
  const [targetEmployeeId, setTargetEmployeeId] = useState<string>('');
  const [notes, setNotes] = useState<string>('');
  const [employees, setEmployees] = useState<EmployeeSimpleDto[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setError(null);
      setNotes('');
      setMode('Manual');
      setTargetEmployeeId(lead?.assignedEmployeeId || '');
      employeeService.getEmployees().then((res) => {
        if (res.data) setEmployees(res.data);
      }).catch(console.error);
    }
  }, [isOpen, lead]);

  if (!lead) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (mode === 'Manual' && !targetEmployeeId) {
      setError('Please select an employee for manual assignment.');
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);

      const res = await leadService.assignLead(lead.id, {
        mode,
        targetEmployeeId: mode === 'Manual' ? targetEmployeeId : undefined,
        notes: notes.trim() || undefined,
      });

      if (res.success) {
        onSuccess();
        onClose();
      } else {
        setError(res.message || 'Failed to assign lead.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error assigning lead.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Assign Lead Ownership"
      description={`Transfer or route ownership for lead "${lead.fullName}" (${lead.companyName || 'No Company'}).`}
      maxWidth="md"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && (
          <div className="p-3 bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 rounded-lg text-xs text-red-600 dark:text-red-400 flex items-center gap-2">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{error}</span>
          </div>
        )}

        <div>
          <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-2">
            Assignment Strategy
          </label>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-2">
            <button
              type="button"
              onClick={() => setMode('Manual')}
              className={`p-3 text-left rounded-lg border text-xs font-medium transition-all ${
                mode === 'Manual'
                  ? 'border-blue-600 bg-blue-50 text-blue-900 dark:bg-blue-950/50 dark:border-blue-500 dark:text-blue-100 ring-2 ring-blue-500/20'
                  : 'border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300'
              }`}
            >
              <Users className="w-4 h-4 mb-1.5 text-blue-600" />
              <div className="font-semibold">Manual</div>
              <div className="text-[11px] text-slate-500 dark:text-slate-400 mt-0.5">
                Pick specific rep
              </div>
            </button>

            <button
              type="button"
              onClick={() => setMode('RoundRobin')}
              className={`p-3 text-left rounded-lg border text-xs font-medium transition-all ${
                mode === 'RoundRobin'
                  ? 'border-blue-600 bg-blue-50 text-blue-900 dark:bg-blue-950/50 dark:border-blue-500 dark:text-blue-100 ring-2 ring-blue-500/20'
                  : 'border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300'
              }`}
            >
              <Shuffle className="w-4 h-4 mb-1.5 text-indigo-600" />
              <div className="font-semibold">Round-Robin</div>
              <div className="text-[11px] text-slate-500 dark:text-slate-400 mt-0.5">
                Equal rotation
              </div>
            </button>

            <button
              type="button"
              onClick={() => setMode('WorkloadBased')}
              className={`p-3 text-left rounded-lg border text-xs font-medium transition-all ${
                mode === 'WorkloadBased'
                  ? 'border-blue-600 bg-blue-50 text-blue-900 dark:bg-blue-950/50 dark:border-blue-500 dark:text-blue-100 ring-2 ring-blue-500/20'
                  : 'border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300'
              }`}
            >
              <BarChart3 className="w-4 h-4 mb-1.5 text-emerald-600" />
              <div className="font-semibold">Workload</div>
              <div className="text-[11px] text-slate-500 dark:text-slate-400 mt-0.5">
                Rep with lowest load
              </div>
            </button>
          </div>
        </div>

        {mode === 'Manual' && (
          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Select Sales Representative *
            </label>
            <select
              value={targetEmployeeId}
              onChange={(e) => setTargetEmployeeId(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            >
              <option value="">-- Choose Employee --</option>
              {employees.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.fullName} ({emp.teamName || emp.departmentName || 'Sales'})
                </option>
              ))}
            </select>
          </div>
        )}

        <div>
          <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
            Assignment Reason / Notes
          </label>
          <textarea
            rows={2}
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="e.g. Account aligns with specialized manufacturing sector expertise."
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
                <span>Assigning...</span>
              </>
            ) : (
              <span>Confirm Assignment</span>
            )}
          </button>
        </div>
      </form>
    </Modal>
  );
};
