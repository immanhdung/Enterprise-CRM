import React, { useState, useEffect } from 'react';
import { Loader2, ArrowRightLeft, Building2, User, Briefcase, AlertCircle, Sparkles } from 'lucide-react';
import { Modal } from '../../../components/common/Modal';
import { leadService } from '../../../services/leadService';
import { accountService } from '../../../services/accountService';
import type { LeadDto, AccountDto } from '../../../types/crm';

interface ConvertLeadModalProps {
  isOpen: boolean;
  onClose: () => void;
  lead: LeadDto | null;
  onSuccess: (result: {
    accountId: string;
    accountName: string;
    contactId: string;
    contactName: string;
    dealId: string;
    dealTitle: string;
    convertedAt: string;
  }) => void;
}

export const ConvertLeadModal: React.FC<ConvertLeadModalProps> = ({
  isOpen,
  onClose,
  lead,
  onSuccess,
}) => {
  const [accountMode, setAccountMode] = useState<'create' | 'existing'>('create');
  const [accounts, setAccounts] = useState<AccountDto[]>([]);
  const [selectedAccountId, setSelectedAccountId] = useState<string>('');
  const [accountName, setAccountName] = useState<string>('');
  const [dealTitle, setDealTitle] = useState<string>('');
  const [dealAmount, setDealAmount] = useState<number>(0);
  const [expectedClosingDate, setExpectedClosingDate] = useState<string>('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen && lead) {
      setError(null);
      setAccountMode(lead.companyName ? 'create' : 'existing');
      setAccountName(lead.companyName || `${lead.fullName}'s Organization`);
      setDealTitle(`${lead.companyName || lead.fullName} - Service Agreement`);
      setDealAmount(lead.expectedBudget || 10000);
      setExpectedClosingDate(new Date(Date.now() + 30 * 86400000).toISOString().split('T')[0]);

      accountService.getAccounts({ pageSize: 50 }).then((res) => {
        if (res.data?.items) setAccounts(res.data.items);
      }).catch(console.error);
    }
  }, [isOpen, lead]);

  if (!lead) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (accountMode === 'create' && !accountName.trim()) {
      setError('Please provide an Account Name.');
      return;
    }
    if (accountMode === 'existing' && !selectedAccountId) {
      setError('Please select an existing Account.');
      return;
    }
    if (!dealTitle.trim()) {
      setError('Please specify a Deal Title.');
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);

      const res = await leadService.convertLead(lead.id, {
        accountName: accountMode === 'create' ? accountName.trim() : undefined,
        existingAccountId: accountMode === 'existing' ? selectedAccountId : undefined,
        dealTitle: dealTitle.trim(),
        dealAmount: Number(dealAmount) || 0,
        expectedClosingDate: expectedClosingDate ? new Date(expectedClosingDate).toISOString() : undefined,
      });

      if (res.success && res.data) {
        onSuccess(res.data);
        onClose();
      } else {
        setError(res.message || 'Failed to convert lead.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error during lead conversion.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Convert Lead to Account, Contact & Deal"
      description={`Promote qualified lead "${lead.fullName}" into enterprise business entities.`}
      maxWidth="lg"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && (
          <div className="p-3 bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 rounded-lg text-xs text-red-600 dark:text-red-400 flex items-center gap-2">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{error}</span>
          </div>
        )}

        {/* Pipeline conversion summary banner */}
        <div className="p-3 bg-blue-50 dark:bg-blue-950/40 border border-blue-200 dark:border-blue-800 rounded-lg text-xs text-blue-900 dark:text-blue-200 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Sparkles className="w-4 h-4 text-blue-600 dark:text-blue-400" />
            <span>All activity history and communication notes will be linked to the new Deal and Account.</span>
          </div>
        </div>

        {/* Account Entity Section */}
        <div className="p-3.5 bg-slate-50 dark:bg-slate-800/60 border border-slate-200 dark:border-slate-700 rounded-lg space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-800 dark:text-slate-200 flex items-center gap-1.5">
              <Building2 className="w-4 h-4 text-blue-600" />
              1. Account Association
            </span>
            <div className="flex gap-2">
              <button
                type="button"
                onClick={() => setAccountMode('create')}
                className={`text-xs px-2.5 py-1 rounded-md font-medium transition-colors ${
                  accountMode === 'create'
                    ? 'bg-blue-600 text-white'
                    : 'bg-white dark:bg-slate-700 text-slate-600 dark:text-slate-300 border border-slate-300 dark:border-slate-600'
                }`}
              >
                Create New
              </button>
              <button
                type="button"
                onClick={() => setAccountMode('existing')}
                className={`text-xs px-2.5 py-1 rounded-md font-medium transition-colors ${
                  accountMode === 'existing'
                    ? 'bg-blue-600 text-white'
                    : 'bg-white dark:bg-slate-700 text-slate-600 dark:text-slate-300 border border-slate-300 dark:border-slate-600'
                }`}
              >
                Link Existing
              </button>
            </div>
          </div>

          {accountMode === 'create' ? (
            <div>
              <label className="block text-[11px] font-medium text-slate-600 dark:text-slate-400 mb-1">
                New Company / Account Name *
              </label>
              <input
                type="text"
                value={accountName}
                onChange={(e) => setAccountName(e.target.value)}
                placeholder="Acme Corporation"
                className="w-full px-3 py-1.5 border rounded-md text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white"
              />
            </div>
          ) : (
            <div>
              <label className="block text-[11px] font-medium text-slate-600 dark:text-slate-400 mb-1">
                Select Existing Account *
              </label>
              <select
                value={selectedAccountId}
                onChange={(e) => setSelectedAccountId(e.target.value)}
                className="w-full px-3 py-1.5 border rounded-md text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white"
              >
                <option value="">-- Choose Account --</option>
                {accounts.map((acc) => (
                  <option key={acc.id} value={acc.id}>
                    {acc.name} ({acc.code})
                  </option>
                ))}
              </select>
            </div>
          )}
        </div>

        {/* Contact Entity Section */}
        <div className="p-3.5 bg-slate-50 dark:bg-slate-800/60 border border-slate-200 dark:border-slate-700 rounded-lg">
          <span className="text-xs font-semibold text-slate-800 dark:text-slate-200 flex items-center gap-1.5 mb-2">
            <User className="w-4 h-4 text-emerald-600" />
            2. Contact Association (Auto-linked)
          </span>
          <div className="text-xs text-slate-600 dark:text-slate-300 bg-white dark:bg-slate-900 p-2.5 rounded border border-slate-200 dark:border-slate-700 flex flex-wrap gap-x-4 gap-y-1">
            <div><span className="font-medium text-slate-400">Name:</span> {lead.fullName}</div>
            <div><span className="font-medium text-slate-400">Email:</span> {lead.email || 'None'}</div>
            <div><span className="font-medium text-slate-400">Phone:</span> {lead.phone || 'None'}</div>
            <div><span className="font-medium text-slate-400">Role:</span> {lead.position || 'Representative'} (Primary)</div>
          </div>
        </div>

        {/* Deal Entity Section */}
        <div className="p-3.5 bg-slate-50 dark:bg-slate-800/60 border border-slate-200 dark:border-slate-700 rounded-lg space-y-3">
          <span className="text-xs font-semibold text-slate-800 dark:text-slate-200 flex items-center gap-1.5">
            <Briefcase className="w-4 h-4 text-purple-600" />
            3. Opportunity / Deal Details
          </span>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            <div className="md:col-span-2">
              <label className="block text-[11px] font-medium text-slate-600 dark:text-slate-400 mb-1">
                Deal Title *
              </label>
              <input
                type="text"
                value={dealTitle}
                onChange={(e) => setDealTitle(e.target.value)}
                placeholder="Deal Name"
                className="w-full px-3 py-1.5 border rounded-md text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white"
              />
            </div>

            <div>
              <label className="block text-[11px] font-medium text-slate-600 dark:text-slate-400 mb-1">
                Deal Amount ($) *
              </label>
              <input
                type="number"
                step="500"
                value={dealAmount}
                onChange={(e) => setDealAmount(Number(e.target.value))}
                className="w-full px-3 py-1.5 border rounded-md text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white"
              />
            </div>

            <div>
              <label className="block text-[11px] font-medium text-slate-600 dark:text-slate-400 mb-1">
                Target Closing Date
              </label>
              <input
                type="date"
                value={expectedClosingDate}
                onChange={(e) => setExpectedClosingDate(e.target.value)}
                className="w-full px-3 py-1.5 border rounded-md text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white"
              />
            </div>
          </div>
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
                <span>Executing Conversion...</span>
              </>
            ) : (
              <>
                <ArrowRightLeft className="w-4 h-4" />
                <span>Convert Lead Now</span>
              </>
            )}
          </button>
        </div>
      </form>
    </Modal>
  );
};
