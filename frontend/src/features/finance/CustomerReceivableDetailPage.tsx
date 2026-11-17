import React, { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Building2,
  ArrowLeft,
  Send,
  MessageSquare,
  AlertCircle,
} from 'lucide-react';
import { FinanceNavigation } from './FinanceNavigation';
import { financeService } from '@/services/financeService';
import { accountService } from '@/services/accountService';
import type { CreateReceivableCollectionLogRequest } from '@/types/finance';

export const CustomerReceivableDetailPage: React.FC = () => {
  const { id: accountId } = useParams<{ id: string }>();
  const queryClient = useQueryClient();

  const [activityType, setActivityType] = useState('Phone Call');
  const [notes, setNotes] = useState('');
  const [nextDate, setNextDate] = useState(
    new Date(Date.now() + 7 * 86400000).toISOString().split('T')[0]
  );
  const [status, setStatus] = useState('Customer Promised Payment');
  const [formError, setFormError] = useState<string | null>(null);

  const { data: accountData } = useQuery({
    queryKey: ['accounts', accountId],
    queryFn: () => accountService.getAccountById(accountId!),
    enabled: !!accountId,
  });

  const account = accountData?.data;

  const { data: receivables, isLoading: isRecLoading } = useQuery({
    queryKey: ['finance', 'receivables-customer', accountId],
    queryFn: () => financeService.getReceivablesList({ accountId }),
    enabled: !!accountId,
  });

  const { data: collectionLogs, isLoading: isLogsLoading } = useQuery({
    queryKey: ['finance', 'collection-logs-customer', accountId],
    queryFn: () => financeService.getCollectionLogs({ accountId }),
    enabled: !!accountId,
  });

  const addLogMutation = useMutation({
    mutationFn: (req: CreateReceivableCollectionLogRequest) => financeService.addCollectionLog(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['finance', 'collection-logs-customer', accountId] });
      queryClient.invalidateQueries({ queryKey: ['finance', 'receivables-list'] });
      setNotes('');
      setFormError(null);
    },
    onError: (err: any) => {
      setFormError(err.response?.data?.message || err.message || 'Failed to record collection log.');
    },
  });

  const handleAddLog = (e: React.FormEvent) => {
    e.preventDefault();
    if (!accountId) return;
    if (!notes.trim()) {
      setFormError('Activity notes are required.');
      return;
    }

    const firstContractId = receivables?.[0]?.contractId || accountId;

    addLogMutation.mutate({
      accountId,
      contractId: firstContractId,
      activityType,
      notes,
      nextFollowUpDate: nextDate,
      status,
    });
  };

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const totalOutstanding = receivables?.reduce((acc, curr) => acc + curr.outstandingAmount, 0) || 0;
  const maxOverdueDays = receivables?.reduce((acc, curr) => Math.max(acc, curr.overdueDays), 0) || 0;

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <FinanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Breadcrumb / Back Link */}
        <div className="flex items-center gap-2 text-xs text-slate-500">
          <Link to="/finance/receivables" className="hover:text-blue-600 flex items-center gap-1 font-medium">
            <ArrowLeft className="w-3.5 h-3.5" />
            <span>Back to Debt Aging</span>
          </Link>
          <span>/</span>
          <span className="text-slate-800 font-semibold">{account?.name || 'Customer Debt Profile'}</span>
        </div>

        {/* Customer Header Info */}
        <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-xs flex flex-col md:flex-row md:items-center justify-between gap-6">
          <div className="space-y-1">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-blue-100 text-blue-700 flex items-center justify-center font-bold text-lg">
                <Building2 className="w-5 h-5" />
              </div>
              <div>
                <h1 className="text-xl font-bold text-slate-900 tracking-tight">{account?.name}</h1>
                <div className="flex flex-wrap items-center gap-3 text-xs text-slate-500 mt-0.5">
                  <span>Code: {account?.code}</span>
                  {account?.taxId && <span>• MST: {account.taxId}</span>}
                  {account?.phone && <span>• Tel: {account.phone}</span>}
                  {account?.email && <span>• Email: {account.email}</span>}
                </div>
              </div>
            </div>
          </div>

          <div className="flex items-center gap-4 border-t md:border-t-0 md:border-l border-slate-100 pt-4 md:pt-0 md:pl-6">
            <div>
              <div className="text-xs text-slate-500 uppercase font-semibold">Total Outstanding Debt</div>
              <div className="text-2xl font-black text-red-600 mt-0.5">{formatVnd(totalOutstanding)}</div>
              <div className="text-xs font-semibold text-amber-600 mt-0.5">
                {maxOverdueDays > 0 ? `Max Overdue: ${maxOverdueDays} days` : 'Current / Due Soon'}
              </div>
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Outstanding Milestones Table */}
          <div className="lg:col-span-2 space-y-6">
            <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
              <div className="p-4 border-b border-slate-100 font-semibold text-sm text-slate-900 flex justify-between items-center">
                <span>Unsettled Contract Milestones ({receivables?.length || 0})</span>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse text-xs">
                  <thead>
                    <tr className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
                      <th className="py-2.5 px-3">Contract #</th>
                      <th className="py-2.5 px-3">Milestone</th>
                      <th className="py-2.5 px-3">Due Date</th>
                      <th className="py-2.5 px-3">Total Amount</th>
                      <th className="py-2.5 px-3">Paid</th>
                      <th className="py-2.5 px-3">Remaining Debt</th>
                      <th className="py-2.5 px-3">Aging Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {isRecLoading ? (
                      <tr>
                        <td colSpan={7} className="p-6 text-center text-slate-400">
                          Loading milestones...
                        </td>
                      </tr>
                    ) : receivables && receivables.length > 0 ? (
                      receivables.map((r) => (
                        <tr key={r.paymentScheduleId} className="hover:bg-slate-50 transition">
                          <td className="py-3 px-3 font-semibold text-slate-900">{r.contractNumber}</td>
                          <td className="py-3 px-3 text-slate-800">{r.milestoneTitle}</td>
                          <td className="py-3 px-3 font-mono text-slate-600">{r.dueDate}</td>
                          <td className="py-3 px-3 font-semibold text-slate-700">{formatVnd(r.totalAmount)}</td>
                          <td className="py-3 px-3 text-emerald-600 font-medium">{formatVnd(r.paidAmount)}</td>
                          <td className="py-3 px-3 font-bold text-red-600 text-sm">
                            {formatVnd(r.outstandingAmount)}
                          </td>
                          <td className="py-3 px-3">
                            <span
                              className={`inline-flex items-center px-2 py-0.5 rounded text-[11px] font-semibold ${
                                r.agingBracket === 'Current'
                                  ? 'bg-emerald-100 text-emerald-800'
                                  : r.agingBracket === '1-7 Days'
                                  ? 'bg-amber-100 text-amber-800'
                                  : r.agingBracket === '8-30 Days'
                                  ? 'bg-orange-100 text-orange-800'
                                  : 'bg-red-100 text-red-800'
                              }`}
                            >
                              {r.agingBracket} ({r.overdueDays}d)
                            </span>
                          </td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan={7} className="p-6 text-center text-slate-400">
                          No unpaid milestones for this client.
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Collection Activity History Stream */}
            <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
              <div className="p-4 border-b border-slate-100 font-semibold text-sm text-slate-900 flex justify-between items-center">
                <span>Debt Collection Activity History</span>
                <span className="text-xs text-slate-500">{collectionLogs?.length || 0} entries</span>
              </div>
              <div className="p-4 divide-y divide-slate-100 space-y-4">
                {isLogsLoading ? (
                  <div className="p-6 text-center text-slate-400 text-xs">Loading history...</div>
                ) : collectionLogs && collectionLogs.length > 0 ? (
                  collectionLogs.map((log) => (
                    <div key={log.id} className="pt-3 first:pt-0 space-y-1.5">
                      <div className="flex items-center justify-between text-xs">
                        <span className="font-bold text-slate-900 flex items-center gap-1.5">
                          <MessageSquare className="w-3.5 h-3.5 text-blue-600" />
                          {log.activityType}
                        </span>
                        <span className="text-slate-400 font-mono">
                          {new Date(log.loggedAt).toLocaleString('vi-VN')}
                        </span>
                      </div>
                      <p className="text-xs text-slate-700 bg-slate-50 p-2.5 rounded border border-slate-100 whitespace-pre-wrap">
                        {log.notes}
                      </p>
                      <div className="flex items-center justify-between text-[11px] text-slate-500 pt-0.5">
                        <span>Logged by: <strong>{log.loggedByName}</strong></span>
                        <div className="flex items-center gap-3">
                          {log.nextFollowUpDate && (
                            <span className="text-amber-700">Next Follow-up: {log.nextFollowUpDate}</span>
                          )}
                          <span className="font-semibold text-blue-700 bg-blue-50 px-2 py-0.5 rounded">
                            {log.status}
                          </span>
                        </div>
                      </div>
                    </div>
                  ))
                ) : (
                  <div className="p-6 text-center text-slate-400 text-xs">
                    No collection follow-up logs recorded yet for this client.
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Quick Log Form Column */}
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs h-fit space-y-4">
            <h3 className="font-bold text-slate-900 text-sm">Log New Follow-up Activity</h3>

            <form onSubmit={handleAddLog} className="space-y-3">
              {formError && (
                <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-xs flex items-center gap-2">
                  <AlertCircle className="w-4 h-4 shrink-0" />
                  <span>{formError}</span>
                </div>
              )}

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Activity Type</label>
                <select
                  value={activityType}
                  onChange={(e) => setActivityType(e.target.value)}
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                >
                  <option value="Phone Call">Phone Call with CFO / Accountant</option>
                  <option value="Email Reminder">Email Overdue Reminder</option>
                  <option value="Formal Legal Notice">Formal Written Notice</option>
                  <option value="Direct Meeting">In-person Meeting</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Outcome Status</label>
                <select
                  value={status}
                  onChange={(e) => setStatus(e.target.value)}
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                >
                  <option value="Customer Promised Payment">Customer Promised Payment</option>
                  <option value="Client Requested Extension">Client Requested Extension</option>
                  <option value="Customer Disputed">Customer Disputed Deliverable</option>
                  <option value="Resolved">Resolved / Paid</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Next Follow-up Date</label>
                <input
                  type="date"
                  value={nextDate}
                  onChange={(e) => setNextDate(e.target.value)}
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Discussion Notes *</label>
                <textarea
                  rows={4}
                  required
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="Record summary of conversation, key commitments, reasons for delay..."
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                />
              </div>

              <button
                type="submit"
                disabled={addLogMutation.isPending}
                className="w-full py-2 px-4 text-xs font-semibold text-white bg-blue-600 hover:bg-blue-700 rounded-lg shadow-xs transition disabled:opacity-50 flex items-center justify-center gap-1.5"
              >
                <Send className="w-3.5 h-3.5" />
                <span>{addLogMutation.isPending ? 'Saving...' : 'Submit Log Entry'}</span>
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
};
