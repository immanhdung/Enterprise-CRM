import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { AlertCircle, Loader2 } from 'lucide-react';
import { Modal } from '../../../components/common/Modal';
import { contactService } from '../../../services/contactService';
import { accountService } from '../../../services/accountService';
import type { CreateContactRequest, AccountDto } from '../../../types/crm';

const createContactSchema = z.object({
  accountId: z.string().min(1, 'Please select an associated Account'),
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  email: z.string().email('Invalid email').optional().or(z.literal('')),
  phone: z.string().optional(),
  position: z.string().optional(),
  department: z.string().optional(),
  isPrimary: z.boolean(),
  notes: z.string().optional(),
});

type CreateContactFormData = z.infer<typeof createContactSchema>;

interface CreateContactModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  defaultAccountId?: string;
}

export const CreateContactModal: React.FC<CreateContactModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  defaultAccountId,
}) => {
  const [accounts, setAccounts] = useState<AccountDto[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateContactFormData>({
    resolver: zodResolver(createContactSchema),
    defaultValues: {
      accountId: defaultAccountId || '',
      firstName: '',
      lastName: '',
      isPrimary: false,
    },
  });

  useEffect(() => {
    if (isOpen) {
      reset({
        accountId: defaultAccountId || '',
        firstName: '',
        lastName: '',
        isPrimary: false,
      });
      setError(null);
      accountService.getAccounts({ pageSize: 50 }).then((res) => {
        if (res.data?.items) setAccounts(res.data.items);
      }).catch(console.error);
    }
  }, [isOpen, defaultAccountId, reset]);

  const onSubmit = async (data: CreateContactFormData) => {
    try {
      setIsSubmitting(true);
      setError(null);

      const payload: CreateContactRequest = {
        accountId: data.accountId,
        firstName: data.firstName.trim(),
        lastName: data.lastName.trim(),
        email: data.email || undefined,
        phone: data.phone || undefined,
        position: data.position || undefined,
        department: data.department || undefined,
        isPrimary: data.isPrimary,
        notes: data.notes || undefined,
      };

      const res = await contactService.createContact(payload);
      if (res.success) {
        onSuccess();
        onClose();
      } else {
        setError(res.message || 'Failed to create contact.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error creating contact.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Create Account Contact"
      description="Add an executive, decision-maker, or point of contact."
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
            Associated Company / Account *
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

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              First Name *
            </label>
            <input
              type="text"
              {...register('firstName')}
              placeholder="e.g. David"
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
            {errors.firstName && (
              <p className="text-red-500 text-xs mt-1">{errors.firstName.message}</p>
            )}
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Last Name *
            </label>
            <input
              type="text"
              {...register('lastName')}
              placeholder="e.g. Miller"
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
            {errors.lastName && (
              <p className="text-red-500 text-xs mt-1">{errors.lastName.message}</p>
            )}
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Work Email
            </label>
            <input
              type="email"
              {...register('email')}
              placeholder="david@company.com"
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Direct Phone
            </label>
            <input
              type="tel"
              {...register('phone')}
              placeholder="+1-555-0144"
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Position / Job Title
            </label>
            <input
              type="text"
              {...register('position')}
              placeholder="Chief Technology Officer"
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
              Department
            </label>
            <input
              type="text"
              {...register('department')}
              placeholder="Information Technology"
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        <div className="flex items-center gap-2 pt-1">
          <input
            type="checkbox"
            id="isPrimary"
            {...register('isPrimary')}
            className="rounded border-slate-300 text-blue-600 focus:ring-blue-500"
          />
          <label htmlFor="isPrimary" className="text-xs font-medium text-slate-700 dark:text-slate-300 cursor-pointer">
            Set as Primary Contact for this Account
          </label>
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
              <span>Create Contact</span>
            )}
          </button>
        </div>
      </form>
    </Modal>
  );
};
