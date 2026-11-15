import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { AlertCircle, Loader2 } from 'lucide-react';
import { Modal } from '../../../components/common/Modal';
import { leadService } from '../../../services/leadService';
import { employeeService, type EmployeeSimpleDto } from '../../../services/employeeService';
import type { CreateLeadRequest, DuplicateCheckResult } from '../../../types/crm';

const createLeadSchema = z.object({
  fullName: z.string().min(2, 'Full name must be at least 2 characters'),
  companyName: z.string().optional(),
  phone: z.string().optional(),
  email: z.string().email('Invalid email address').optional().or(z.literal('')),
  position: z.string().optional(),
  source: z.string().optional(),
  campaign: z.string().optional(),
  description: z.string().optional(),
  priority: z.enum(['Low', 'Medium', 'High', 'Urgent']),
  expectedBudget: z.number().min(0).optional(),
  nextFollowUpDate: z.string().optional(),
  assignedEmployeeId: z.string().optional(),
});

type CreateLeadFormData = z.infer<typeof createLeadSchema>;

interface CreateLeadModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const CreateLeadModal: React.FC<CreateLeadModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [employees, setEmployees] = useState<EmployeeSimpleDto[]>([]);
  const [dupCheck, setDupCheck] = useState<DuplicateCheckResult | null>(null);
  const [isCheckingDup, setIsCheckingDup] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    watch,
    reset,
    formState: { errors },
  } = useForm<CreateLeadFormData>({
    resolver: zodResolver(createLeadSchema),
    defaultValues: {
      fullName: '',
      companyName: '',
      phone: '',
      email: '',
      position: '',
      source: 'Website',
      priority: 'Medium',
      expectedBudget: 0,
    },
  });

  const emailValue = watch('email');
  const phoneValue = watch('phone');

  useEffect(() => {
    if (isOpen) {
      reset();
      setDupCheck(null);
      setSubmitError(null);
      employeeService.getEmployees().then((res) => {
        if (res.data) setEmployees(res.data);
      }).catch(console.error);
    }
  }, [isOpen, reset]);

  // Debounced duplicate detection
  useEffect(() => {
    if (!emailValue && !phoneValue) {
      setDupCheck(null);
      return;
    }

    const timer = setTimeout(async () => {
      try {
        setIsCheckingDup(true);
        const res = await leadService.checkDuplicates({
          email: emailValue || undefined,
          phone: phoneValue || undefined,
        });
        if (res.data) {
          setDupCheck(res.data);
        }
      } catch (err) {
        console.error('Duplicate check error:', err);
      } finally {
        setIsCheckingDup(false);
      }
    }, 500);

    return () => clearTimeout(timer);
  }, [emailValue, phoneValue]);

  const onSubmit = async (data: CreateLeadFormData) => {
    try {
      setIsSubmitting(true);
      setSubmitError(null);

      const payload: CreateLeadRequest = {
        fullName: data.fullName,
        companyName: data.companyName || undefined,
        phone: data.phone || undefined,
        email: data.email || undefined,
        position: data.position || undefined,
        source: data.source || undefined,
        campaign: data.campaign || undefined,
        description: data.description || undefined,
        priority: data.priority,
        expectedBudget: data.expectedBudget || undefined,
        nextFollowUpDate: data.nextFollowUpDate ? new Date(data.nextFollowUpDate).toISOString() : undefined,
        assignedEmployeeId: data.assignedEmployeeId || undefined,
      };

      const res = await leadService.createLead(payload);
      if (res.success) {
        onSuccess();
        onClose();
      } else {
        setSubmitError(res.message || 'Failed to create lead.');
      }
    } catch (err: unknown) {
      const errorMsg = err instanceof Error ? err.message : 'An error occurred while creating lead.';
      setSubmitError(errorMsg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Create New Lead"
      description="Register an inbound prospect with automated duplicate verification."
      maxWidth="xl"
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {submitError && (
          <div className="p-3 bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 rounded-lg text-sm text-red-600 dark:text-red-400 flex items-center gap-2">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{submitError}</span>
          </div>
        )}

        {/* Duplicate warning notification */}
        {dupCheck?.hasDuplicate && (
          <div className="p-3 bg-amber-50 dark:bg-amber-950/40 border border-amber-300 dark:border-amber-700 rounded-lg text-xs text-amber-800 dark:text-amber-300 flex items-start gap-2">
            <AlertCircle className="w-4 h-4 shrink-0 text-amber-600 mt-0.5" />
            <div>
              <p className="font-semibold">{dupCheck.message}</p>
              <p className="mt-0.5">
                Matched Lead: {dupCheck.existingLeadName} ({dupCheck.existingLeadEmail || dupCheck.existingLeadPhone})
              </p>
            </div>
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Full Name *
            </label>
            <input
              type="text"
              {...register('fullName')}
              placeholder="e.g. Sarah Jenkins"
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
            {errors.fullName && (
              <p className="text-red-500 text-xs mt-1">{errors.fullName.message}</p>
            )}
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Company Name
            </label>
            <input
              type="text"
              {...register('companyName')}
              placeholder="e.g. Acme Innovations"
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Email Address
            </label>
            <div className="relative">
              <input
                type="email"
                {...register('email')}
                placeholder="sarah@acme.com"
                className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
              />
              {isCheckingDup && (
                <Loader2 className="w-4 h-4 text-slate-400 animate-spin absolute right-3 top-2.5" />
              )}
            </div>
            {errors.email && (
              <p className="text-red-500 text-xs mt-1">{errors.email.message}</p>
            )}
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Phone Number
            </label>
            <input
              type="tel"
              {...register('phone')}
              placeholder="+1-555-0199"
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Position / Title
            </label>
            <input
              type="text"
              {...register('position')}
              placeholder="VP of Technology"
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Lead Source
            </label>
            <select
              {...register('source')}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            >
              <option value="Website">Website Form</option>
              <option value="Referral">Client Referral</option>
              <option value="ColdOutreach">Cold Outreach</option>
              <option value="InboundCall">Inbound Call</option>
              <option value="LinkedIn">LinkedIn</option>
              <option value="Conference">Conference / Expo</option>
            </select>
          </div>

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
              Expected Budget ($)
            </label>
            <input
              type="number"
              step="1000"
              {...register('expectedBudget', { valueAsNumber: true })}
              placeholder="50000"
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Next Follow-Up Date
            </label>
            <input
              type="date"
              {...register('nextFollowUpDate')}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Assign Sales Rep
            </label>
            <select
              {...register('assignedEmployeeId')}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Unassigned</option>
              {employees.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.fullName} ({emp.teamName || emp.departmentName || 'Sales'})
                </option>
              ))}
            </select>
          </div>
        </div>

        <div>
          <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
            Description / Requirements Notes
          </label>
          <textarea
            rows={3}
            {...register('description')}
            placeholder="Service requirements, company background, initial client request details..."
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
                <span>Creating Lead...</span>
              </>
            ) : (
              <span>Create Lead</span>
            )}
          </button>
        </div>
      </form>
    </Modal>
  );
};
