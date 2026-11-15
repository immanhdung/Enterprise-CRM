import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { AlertCircle, Loader2 } from 'lucide-react';
import { Modal } from '../../../components/common/Modal';
import { dealService } from '../../../services/dealService';
import { accountService } from '../../../services/accountService';
import { contactService } from '../../../services/contactService';
import { employeeService, type EmployeeSimpleDto } from '../../../services/employeeService';
import type { CreateDealRequest, AccountDto, ContactDto } from '../../../types/crm';

const createDealSchema = z.object({
  title: z.string().min(2, 'Deal title must be at least 2 characters'),
  accountId: z.string().min(1, 'Please select an Account'),
  contactId: z.string().optional(),
  stageId: z.string().min(1, 'Please select a Pipeline Stage'),
  amount: z.number().min(0, 'Amount must be greater than or equal to 0'),
  probabilityOverride: z.number().min(0).max(100).optional(),
  expectedClosingDate: z.string().optional(),
  source: z.string().optional(),
  description: z.string().optional(),
  assignedEmployeeId: z.string().optional(),
});

type CreateDealFormData = z.infer<typeof createDealSchema>;

interface CreateDealModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  defaultStageId?: string;
  defaultAccountId?: string;
}

export const CreateDealModal: React.FC<CreateDealModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  defaultStageId,
  defaultAccountId,
}) => {
  const [accounts, setAccounts] = useState<AccountDto[]>([]);
  const [contacts, setContacts] = useState<ContactDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeSimpleDto[]>([]);
  const [stages, setStages] = useState<{ id: string; name: string }[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    watch,
    reset,
    formState: { errors },
  } = useForm<CreateDealFormData>({
    resolver: zodResolver(createDealSchema),
    defaultValues: {
      title: '',
      accountId: defaultAccountId || '',
      stageId: defaultStageId || '',
      amount: 10000,
      source: 'Direct',
    },
  });

  const selectedAccountId = watch('accountId');

  useEffect(() => {
    if (isOpen) {
      reset({
        title: '',
        accountId: defaultAccountId || '',
        stageId: defaultStageId || '',
        amount: 10000,
        source: 'Direct',
      });
      setError(null);

      accountService.getAccounts({ pageSize: 50 }).then((res) => {
        if (res.data?.items) setAccounts(res.data.items);
      }).catch(console.error);

      employeeService.getEmployees().then((res) => {
        if (res.data) setEmployees(res.data);
      }).catch(console.error);

      dealService.getKanbanBoard().then((res) => {
        if (res.data?.columns) {
          setStages(res.data.columns.map((c) => ({ id: c.stageId, name: c.stageName })));
        }
      }).catch(console.error);
    }
  }, [isOpen, defaultStageId, defaultAccountId, reset]);

  // Load contacts for selected account
  useEffect(() => {
    if (selectedAccountId) {
      contactService.getContacts({ accountId: selectedAccountId, pageSize: 50 }).then((res) => {
        if (res.data?.items) setContacts(res.data.items);
      }).catch(console.error);
    } else {
      setContacts([]);
    }
  }, [selectedAccountId]);

  const onSubmit = async (data: CreateDealFormData) => {
    try {
      setIsSubmitting(true);
      setError(null);

      const payload: CreateDealRequest = {
        title: data.title.trim(),
        accountId: data.accountId,
        contactId: data.contactId || undefined,
        stageId: data.stageId,
        amount: data.amount,
        probabilityOverride: data.probabilityOverride || undefined,
        expectedClosingDate: data.expectedClosingDate ? new Date(data.expectedClosingDate).toISOString() : undefined,
        source: data.source || undefined,
        description: data.description || undefined,
        assignedEmployeeId: data.assignedEmployeeId || undefined,
      };

      const res = await dealService.createDeal(payload);
      if (res.success) {
        onSuccess();
        onClose();
      } else {
        setError(res.message || 'Failed to create deal.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error creating deal.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Create New Opportunity / Deal"
      description="Track and forecast revenue for a potential service engagement."
      maxWidth="lg"
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {error && (
          <div className="p-3 bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 rounded-lg text-xs text-red-600 dark:text-red-400 flex items-center gap-2">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{error}</span>
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="md:col-span-2">
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Deal Title *
            </label>
            <input
              type="text"
              {...register('title')}
              placeholder="e.g. Enterprise Cloud Modernization"
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
            {errors.title && (
              <p className="text-red-500 text-xs mt-1">{errors.title.message}</p>
            )}
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Client Account *
            </label>
            <select
              {...register('accountId')}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            >
              <option value="">-- Choose Account --</option>
              {accounts.map((acc) => (
                <option key={acc.id} value={acc.id}>
                  {acc.name} ({acc.code})
                </option>
              ))}
            </select>
            {errors.accountId && (
              <p className="text-red-500 text-xs mt-1">{errors.accountId.message}</p>
            )}
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Key Contact Person
            </label>
            <select
              {...register('contactId')}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            >
              <option value="">None / Unassigned</option>
              {contacts.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.fullName} ({c.position || 'Contact'})
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Initial Pipeline Stage *
            </label>
            <select
              {...register('stageId')}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            >
              <option value="">-- Choose Stage --</option>
              {stages.map((stg) => (
                <option key={stg.id} value={stg.id}>
                  {stg.name}
                </option>
              ))}
            </select>
            {errors.stageId && (
              <p className="text-red-500 text-xs mt-1">{errors.stageId.message}</p>
            )}
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Deal Amount ($) *
            </label>
            <input
              type="number"
              step="500"
              {...register('amount', { valueAsNumber: true })}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
            {errors.amount && (
              <p className="text-red-500 text-xs mt-1">{errors.amount.message}</p>
            )}
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Target Closing Date
            </label>
            <input
              type="date"
              {...register('expectedClosingDate')}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Lead Owner / Rep
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
        </div>

        <div>
          <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
            Scope Summary / Opportunity Notes
          </label>
          <textarea
            rows={2}
            {...register('description')}
            placeholder="Contract deliverables, key milestones, commercial assumptions..."
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
                <span>Creating Deal...</span>
              </>
            ) : (
              <span>Create Opportunity</span>
            )}
          </button>
        </div>
      </form>
    </Modal>
  );
};
