import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Scale,
  Play,
  History,
  X,
  AlertCircle,
} from 'lucide-react';
import { FinanceNavigation } from './FinanceNavigation';
import { financeService } from '@/services/financeService';
import type {
  ReconciliationItem,
  RunReconciliationRequest,
  ResolveDiscrepancyRequest,
} from '@/types/finance';

export const ReconciliationPage: React.FC = () => {
  const queryClient = useQueryClient();

  const [isRunModalOpen, setIsRunModalOpen] = useState(false);
  const [isResolveModalOpen, setIsResolveModalOpen] = useState(false);
  const [selectedItem, setSelectedItem] = useState<ReconciliationItem | null>(null);
  const [resolutionAction, setResolutionAction] = useState<'ConfirmAdjustment' | 'Dismiss'>('ConfirmAdjustment');
  const [resolutionNotes, setResolutionNotes] = useState('');
  const [formError, setFormError] = useState<string | null>(null);

  // Run form
  const [periodStart, setPeriodStart] = useState(
    new Date(Date.now() - 30 * 86400000).toISOString().split('T')[0]
  );
  const [periodEnd, setPeriodEnd] = useState(new Date().toISOString().split('T')[0]);
  const [notes, setNotes] = useState('Monthly scheduled ledger reconciliation');

  const { data: historyList } = useQuery({
    queryKey: ['finance', 'reconciliation-history'],
    queryFn: () => financeService.getReconciliationHistory(),
  });

  const latestRecord = historyList?.[0];

  const runMutation = useMutation({
    mutationFn: (req: RunReconciliationRequest) => financeService.runReconciliation(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['finance', 'reconciliation-history'] });
      setIsRunModalOpen(false);
      setFormError(null);
    },
    onError: (err: any) => {
      setFormError(err.response?.data?.message || err.message || 'Failed to run reconciliation.');
    },
  });

  const resolveMutation = useMutation({
    mutationFn: (data: { itemId: string; req: ResolveDiscrepancyRequest }) =>
      financeService.resolveDiscrepancy(data.itemId, data.req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['finance', 'reconciliation-history'] });
      setIsResolveModalOpen(false);
      setSelectedItem(null);
      setResolutionNotes('');
      setFormError(null);
    },
    onError: (err: any) => {
      setFormError(err.response?.data?.message || err.message || 'Failed to resolve discrepancy.');
    },
  });

  const handleRunSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    runMutation.mutate({
      periodStart,
      periodEnd,
      notes,
    });
  };

  const handleResolveSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedItem) return;
    if (!resolutionNotes.trim()) {
      setFormError('Resolution audit notes are mandatory.');
      return;
    }

    resolveMutation.mutate({
      itemId: selectedItem.id,
      req: {
        action: resolutionAction,
        resolutionNotes,
      },
    });
  };

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <FinanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Financial Reconciliation & Audit</h1>
            <p className="text-sm text-slate-500 mt-1">
              Cross-verify Contract Values vs Invoiced Revenue vs Collected Cash to detect missing payments and discrepancies.
            </p>
          </div>

          <button
            onClick={() => {
              setIsRunModalOpen(true);
              setFormError(null);
            }}
            className="inline-flex items-center gap-1.5 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 shadow-xs transition"
          >
            <Play className="w-4 h-4 fill-white" />
            <span>Run Period Reconciliation</span>
          </button>
        </div>

        {/* Latest Reconciliation Run Status */}
        {latestRecord ? (
          <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-xs space-y-6">
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 pb-4 border-b border-slate-100">
              <div>
                <div className="flex items-center gap-2">
                  <span className="text-xs font-bold text-slate-500 uppercase tracking-wider">
                    Reconciliation Period: {latestRecord.period}
                  </span>
                  <span
                    className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold ${
                      latestRecord.status === 'Balanced'
                        ? 'bg-emerald-100 text-emerald-800'
                        : latestRecord.status === 'Reconciled'
                        ? 'bg-blue-100 text-blue-800'
                        : 'bg-amber-100 text-amber-800'
                    }`}
                  >
                    {latestRecord.status}
                  </span>
                </div>
                <div className="text-xs text-slate-500 mt-1">
                  Reconciled on {latestRecord.reconciliationDate} by <strong>{latestRecord.reconciledByName}</strong>
                  {latestRecord.approvedByName && (
                    <span> • Approved by {latestRecord.approvedByName}</span>
                  )}
                </div>
              </div>

              <div className="text-right">
                <div className="text-xs text-slate-500">Discrepancies Detected</div>
                <div className="text-xl font-black text-red-600 mt-0.5">
                  {formatVnd(latestRecord.discrepancyAmount)} ({latestRecord.items?.length || 0} items)
                </div>
              </div>
            </div>

            {/* 3 Balanced Metrics */}
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
              <div className="p-4 bg-slate-50 rounded-lg border border-slate-200">
                <div className="text-xs text-slate-500 font-semibold uppercase">1. Incurred Contract Value</div>
                <div className="text-lg font-bold text-slate-900 mt-1">
                  {formatVnd(latestRecord.totalContractValue)}
                </div>
              </div>

              <div className="p-4 bg-indigo-50/50 rounded-lg border border-indigo-100">
                <div className="text-xs text-indigo-700 font-semibold uppercase">2. Invoiced Revenue</div>
                <div className="text-lg font-bold text-indigo-800 mt-1">
                  {formatVnd(latestRecord.totalInvoicedRevenue)}
                </div>
              </div>

              <div className="p-4 bg-emerald-50/50 rounded-lg border border-emerald-100">
                <div className="text-xs text-emerald-700 font-semibold uppercase">3. Confirmed Collected Cash</div>
                <div className="text-lg font-bold text-emerald-800 mt-1">
                  {formatVnd(latestRecord.totalCollectedRevenue)}
                </div>
              </div>
            </div>

            {/* Discrepancies Table */}
            <div>
              <div className="flex items-center justify-between mb-3">
                <h4 className="font-semibold text-slate-900 text-sm">Discrepancy Breakdown & Adjustments</h4>
                <span className="text-xs text-slate-500">Authorized audit confirmation required</span>
              </div>

              <div className="border border-slate-200 rounded-lg overflow-hidden">
                <table className="w-full text-left border-collapse text-xs">
                  <thead>
                    <tr className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
                      <th className="py-2.5 px-3">Type</th>
                      <th className="py-2.5 px-3">Contract / Ref</th>
                      <th className="py-2.5 px-3">Expected</th>
                      <th className="py-2.5 px-3">Actual</th>
                      <th className="py-2.5 px-3">Discrepancy</th>
                      <th className="py-2.5 px-3">Status</th>
                      <th className="py-2.5 px-3">Resolution Notes</th>
                      <th className="py-2.5 px-3 text-right">Action</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {latestRecord.items && latestRecord.items.length > 0 ? (
                      latestRecord.items.map((item) => (
                        <tr key={item.id} className="hover:bg-slate-50 transition">
                          <td className="py-3 px-3 font-semibold text-slate-900">
                            <span
                              className={`px-2 py-0.5 rounded text-[10px] font-bold ${
                                item.discrepancyType === 'MissingPayment'
                                  ? 'bg-red-100 text-red-800'
                                  : item.discrepancyType === 'UnmatchedInvoice'
                                  ? 'bg-amber-100 text-amber-800'
                                  : 'bg-purple-100 text-purple-800'
                              }`}
                            >
                              {item.discrepancyType}
                            </span>
                          </td>
                          <td className="py-3 px-3 text-slate-700 font-mono">
                            {item.contractNumber || item.invoiceNumber || item.paymentNumber || 'N/A'}
                          </td>
                          <td className="py-3 px-3 font-mono text-slate-700">{formatVnd(item.expectedAmount)}</td>
                          <td className="py-3 px-3 font-mono text-slate-700">{formatVnd(item.actualAmount)}</td>
                          <td className="py-3 px-3 font-mono font-bold text-red-600">{formatVnd(item.difference)}</td>
                          <td className="py-3 px-3">
                            <span
                              className={`px-2 py-0.5 rounded text-[11px] font-semibold ${
                                item.resolutionStatus === 'Confirmed'
                                  ? 'bg-emerald-100 text-emerald-800'
                                  : item.resolutionStatus === 'Dismissed'
                                  ? 'bg-slate-100 text-slate-700'
                                  : 'bg-amber-100 text-amber-800'
                              }`}
                            >
                              {item.resolutionStatus}
                            </span>
                          </td>
                          <td className="py-3 px-3 text-slate-500 text-[11px] max-w-[200px] truncate">
                            {item.resolutionNotes || '-'}
                          </td>
                          <td className="py-3 px-3 text-right">
                            {item.resolutionStatus === 'Pending' && (
                              <button
                                onClick={() => {
                                  setSelectedItem(item);
                                  setIsResolveModalOpen(true);
                                  setFormError(null);
                                }}
                                className="px-2.5 py-1 text-[11px] font-semibold text-blue-700 bg-blue-50 hover:bg-blue-100 rounded border border-blue-200"
                              >
                                Resolve
                              </button>
                            )}
                          </td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan={8} className="p-6 text-center text-emerald-600 font-medium">
                          ✓ All transactions are fully reconciled! No discrepancies detected.
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        ) : (
          <div className="bg-white p-12 rounded-xl border border-dashed border-slate-300 text-center text-slate-400">
            <Scale className="w-12 h-12 mx-auto text-slate-300 mb-3" />
            <h3 className="text-base font-semibold text-slate-700">No reconciliation records found</h3>
            <p className="text-xs text-slate-500 mt-1">
              Click &quot;Run Period Reconciliation&quot; above to cross-verify all contracts, payments and invoices.
            </p>
          </div>
        )}

        {/* Historical Reconciliation Runs */}
        {historyList && historyList.length > 1 && (
          <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
            <div className="p-4 border-b border-slate-100 font-semibold text-sm text-slate-900 flex items-center gap-2">
              <History className="w-4 h-4 text-slate-400" />
              <span>Prior Reconciliation Audits</span>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse text-xs">
                <thead>
                  <tr className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase">
                    <th className="py-2.5 px-4">Period</th>
                    <th className="py-2.5 px-4">Audit Date</th>
                    <th className="py-2.5 px-4">Invoiced Amount</th>
                    <th className="py-2.5 px-4">Collected Cash</th>
                    <th className="py-2.5 px-4">Discrepancy</th>
                    <th className="py-2.5 px-4">Auditor</th>
                    <th className="py-2.5 px-4">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {historyList.slice(1).map((rec) => (
                    <tr key={rec.id} className="hover:bg-slate-50">
                      <td className="py-3 px-4 font-bold text-slate-900">{rec.period}</td>
                      <td className="py-3 px-4 text-slate-600">{rec.reconciliationDate}</td>
                      <td className="py-3 px-4 font-mono text-slate-800">{formatVnd(rec.totalInvoicedRevenue)}</td>
                      <td className="py-3 px-4 font-mono text-emerald-600 font-bold">
                        {formatVnd(rec.totalCollectedRevenue)}
                      </td>
                      <td className="py-3 px-4 font-mono text-red-600">{formatVnd(rec.discrepancyAmount)}</td>
                      <td className="py-3 px-4 text-slate-700">{rec.reconciledByName}</td>
                      <td className="py-3 px-4">
                        <span className="px-2 py-0.5 rounded text-[10px] font-semibold bg-slate-100 text-slate-700">
                          {rec.status}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>

      {/* Modal: Run Reconciliation */}
      {isRunModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl max-w-md w-full overflow-hidden border border-slate-200">
            <div className="p-4 border-b border-slate-100 flex items-center justify-between">
              <h3 className="font-bold text-slate-900 text-base">Execute Period Reconciliation</h3>
              <button onClick={() => setIsRunModalOpen(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleRunSubmit} className="p-5 space-y-4">
              {formError && (
                <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-xs flex items-center gap-2">
                  <AlertCircle className="w-4 h-4 shrink-0" />
                  <span>{formError}</span>
                </div>
              )}

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Period Start Date *</label>
                  <input
                    type="date"
                    required
                    value={periodStart}
                    onChange={(e) => setPeriodStart(e.target.value)}
                    className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                  />
                </div>
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Period End Date *</label>
                  <input
                    type="date"
                    required
                    value={periodEnd}
                    onChange={(e) => setPeriodEnd(e.target.value)}
                    className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                  />
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Audit Notes / Comments</label>
                <textarea
                  rows={2}
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                />
              </div>

              <div className="flex items-center justify-end gap-2 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsRunModalOpen(false)}
                  className="px-4 py-2 text-xs font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={runMutation.isPending}
                  className="px-4 py-2 text-xs font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50"
                >
                  {runMutation.isPending ? 'Reconciling...' : 'Run Reconciliation'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal: Resolve Discrepancy */}
      {isResolveModalOpen && selectedItem && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl max-w-md w-full overflow-hidden border border-slate-200">
            <div className="p-4 border-b border-slate-100 flex items-center justify-between">
              <h3 className="font-bold text-slate-900 text-base">Authorized Discrepancy Resolution</h3>
              <button onClick={() => setIsResolveModalOpen(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleResolveSubmit} className="p-5 space-y-4">
              <div className="p-3 bg-amber-50 border border-amber-200 rounded-lg text-xs text-amber-900 space-y-1">
                <div>
                  <strong>Discrepancy:</strong> {selectedItem.discrepancyType}
                </div>
                <div>
                  <strong>Amount Difference:</strong>{' '}
                  <span className="font-bold text-red-700">{formatVnd(selectedItem.difference)}</span>
                </div>
              </div>

              {formError && (
                <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-xs flex items-center gap-2">
                  <AlertCircle className="w-4 h-4 shrink-0" />
                  <span>{formError}</span>
                </div>
              )}

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Resolution Action *</label>
                <select
                  value={resolutionAction}
                  onChange={(e) => setResolutionAction(e.target.value as any)}
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                >
                  <option value="ConfirmAdjustment">Confirm General Ledger Adjustment</option>
                  <option value="Dismiss">Dismiss as False Positive / Verified Exception</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">
                  Resolution Justification & Audit Notes *
                </label>
                <textarea
                  rows={3}
                  required
                  value={resolutionNotes}
                  onChange={(e) => setResolutionNotes(e.target.value)}
                  placeholder="Record justification, general ledger reference, or verified authorization details..."
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                />
              </div>

              <div className="flex items-center justify-end gap-2 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsResolveModalOpen(false)}
                  className="px-4 py-2 text-xs font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={resolveMutation.isPending}
                  className="px-4 py-2 text-xs font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50"
                >
                  {resolveMutation.isPending ? 'Saving...' : 'Authorize Resolution'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
