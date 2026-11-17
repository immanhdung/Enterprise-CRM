import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  AlertCircle,
  Search,
  ArrowRight,
  X,
} from 'lucide-react';
import { FinanceNavigation } from './FinanceNavigation';
import { financeService } from '@/services/financeService';
import type {
  AccountsReceivableItem,
  CreateReceivableCollectionLogRequest,
} from '@/types/finance';

export const ReceivablesPage: React.FC = () => {
  const queryClient = useQueryClient();

  const [searchTerm, setSearchTerm] = useState('');
  const [selectedBracket, setSelectedBracket] = useState<string>('');
  const [onlyOverdue, setOnlyOverdue] = useState(false);

  // Collection Log Modal state
  const [isLogModalOpen, setIsLogModalOpen] = useState(false);
  const [selectedReceivable, setSelectedReceivable] = useState<AccountsReceivableItem | null>(null);
  const [logForm, setLogForm] = useState<CreateReceivableCollectionLogRequest>({
    accountId: '',
    contractId: '',
    paymentScheduleId: undefined,
    activityType: 'Phone Call',
    notes: '',
    nextFollowUpDate: new Date(Date.now() + 7 * 86400000).toISOString().split('T')[0],
    status: 'Customer Promised Payment',
  });
  const [formError, setFormError] = useState<string | null>(null);

  const { data: agingSummary } = useQuery({
    queryKey: ['finance', 'aging-summary'],
    queryFn: () => financeService.getAgingSummary(),
  });

  const { data: receivablesList, isLoading: isListLoading } = useQuery({
    queryKey: ['finance', 'receivables-list', searchTerm, selectedBracket, onlyOverdue],
    queryFn: () =>
      financeService.getReceivablesList({
        searchTerm: searchTerm || undefined,
        agingBracket: selectedBracket || undefined,
        onlyOverdue: onlyOverdue || undefined,
      }),
  });

  const addLogMutation = useMutation({
    mutationFn: (req: CreateReceivableCollectionLogRequest) => financeService.addCollectionLog(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['finance', 'receivables-list'] });
      setIsLogModalOpen(false);
      setSelectedReceivable(null);
      setFormError(null);
    },
    onError: (err: any) => {
      setFormError(err.response?.data?.message || err.message || 'Failed to record collection activity.');
    },
  });

  const handleOpenLogModal = (item: AccountsReceivableItem) => {
    setSelectedReceivable(item);
    setLogForm({
      accountId: item.accountId,
      contractId: item.contractId,
      paymentScheduleId: item.paymentScheduleId,
      activityType: 'Phone Call',
      notes: '',
      nextFollowUpDate: new Date(Date.now() + 7 * 86400000).toISOString().split('T')[0],
      status: 'Customer Promised Payment',
    });
    setIsLogModalOpen(true);
    setFormError(null);
  };

  const handleLogSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!logForm.notes.trim()) {
      setFormError('Activity notes are required.');
      return;
    }
    addLogMutation.mutate(logForm);
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
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Accounts Receivable & Debt Aging</h1>
            <p className="text-sm text-slate-500 mt-1">
              Track outstanding customer debt by aging brackets (1–7d, 8–30d, 31–60d, &gt;60d) and log collection activities.
            </p>
          </div>
        </div>

        {/* 5 Aging Summary Metric Tiles */}
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-3">
          <div
            onClick={() => {
              setSelectedBracket('');
              setOnlyOverdue(false);
            }}
            className={`p-3.5 bg-white rounded-xl border cursor-pointer transition shadow-xs ${
              selectedBracket === '' && !onlyOverdue ? 'border-blue-600 ring-2 ring-blue-100' : 'border-slate-200 hover:bg-slate-50'
            }`}
          >
            <div className="text-xs font-semibold text-slate-500 uppercase tracking-wider">All Receivables</div>
            <div className="text-base font-bold text-slate-900 mt-1">{formatVnd(agingSummary?.totalReceivables)}</div>
            <div className="text-[11px] text-slate-400 mt-0.5">{agingSummary?.accountsWithDebtCount || 0} clients</div>
          </div>

          <div
            onClick={() => {
              setSelectedBracket('Current');
              setOnlyOverdue(false);
            }}
            className={`p-3.5 bg-white rounded-xl border cursor-pointer transition shadow-xs ${
              selectedBracket === 'Current' ? 'border-emerald-600 ring-2 ring-emerald-100' : 'border-slate-200 hover:bg-slate-50'
            }`}
          >
            <div className="text-xs font-semibold text-emerald-700 uppercase tracking-wider">Due Soon / Current</div>
            <div className="text-base font-bold text-emerald-700 mt-1">{formatVnd(agingSummary?.currentAmount)}</div>
            <div className="text-[11px] text-emerald-600/70 mt-0.5">Not yet overdue</div>
          </div>

          <div
            onClick={() => {
              setSelectedBracket('1-7 Days');
              setOnlyOverdue(true);
            }}
            className={`p-3.5 bg-white rounded-xl border cursor-pointer transition shadow-xs ${
              selectedBracket === '1-7 Days' ? 'border-amber-500 ring-2 ring-amber-100' : 'border-slate-200 hover:bg-slate-50'
            }`}
          >
            <div className="text-xs font-semibold text-amber-600 uppercase tracking-wider">1–7 Days Overdue</div>
            <div className="text-base font-bold text-amber-700 mt-1">{formatVnd(agingSummary?.overdue1To7Days)}</div>
            <div className="text-[11px] text-amber-600/70 mt-0.5">Gentle reminder</div>
          </div>

          <div
            onClick={() => {
              setSelectedBracket('8-30 Days');
              setOnlyOverdue(true);
            }}
            className={`p-3.5 bg-white rounded-xl border cursor-pointer transition shadow-xs ${
              selectedBracket === '8-30 Days' ? 'border-amber-600 ring-2 ring-amber-100' : 'border-slate-200 hover:bg-slate-50'
            }`}
          >
            <div className="text-xs font-semibold text-amber-700 uppercase tracking-wider">8–30 Days Overdue</div>
            <div className="text-base font-bold text-amber-800 mt-1">{formatVnd(agingSummary?.overdue8To30Days)}</div>
            <div className="text-[11px] text-amber-700/70 mt-0.5">Formal notice</div>
          </div>

          <div
            onClick={() => {
              setSelectedBracket('31-60 Days');
              setOnlyOverdue(true);
            }}
            className={`p-3.5 bg-white rounded-xl border cursor-pointer transition shadow-xs ${
              selectedBracket === '31-60 Days' ? 'border-orange-600 ring-2 ring-orange-100' : 'border-slate-200 hover:bg-slate-50'
            }`}
          >
            <div className="text-xs font-semibold text-orange-700 uppercase tracking-wider">31–60 Days Overdue</div>
            <div className="text-base font-bold text-orange-800 mt-1">{formatVnd(agingSummary?.overdue31To60Days)}</div>
            <div className="text-[11px] text-orange-700/70 mt-0.5">High priority</div>
          </div>

          <div
            onClick={() => {
              setSelectedBracket('>60 Days');
              setOnlyOverdue(true);
            }}
            className={`p-3.5 bg-red-50/70 rounded-xl border cursor-pointer transition shadow-xs ${
              selectedBracket === '>60 Days' ? 'border-red-600 ring-2 ring-red-100' : 'border-red-200 hover:bg-red-50'
            }`}
          >
            <div className="text-xs font-semibold text-red-700 uppercase tracking-wider">&gt;60 Days Overdue</div>
            <div className="text-base font-bold text-red-700 mt-1">{formatVnd(agingSummary?.overdueMoreThan60Days)}</div>
            <div className="text-[11px] text-red-600/80 mt-0.5">Critical debt risk</div>
          </div>
        </div>

        {/* Filter and Search Bar */}
        <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex flex-col sm:flex-row items-center justify-between gap-4">
          <div className="relative flex-1 w-full sm:max-w-md">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Search by customer name, contract #, contact person..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-9 pr-4 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-1 focus:ring-blue-500 bg-white"
            />
          </div>

          <div className="flex items-center gap-4 w-full sm:w-auto">
            <label className="flex items-center gap-2 text-xs font-semibold text-slate-700 cursor-pointer">
              <input
                type="checkbox"
                checked={onlyOverdue}
                onChange={(e) => setOnlyOverdue(e.target.checked)}
                className="rounded border-slate-300 text-blue-600 focus:ring-blue-500"
              />
              <span>Only Overdue ({agingSummary?.overdueContractsCount || 0})</span>
            </label>

            <select
              value={selectedBracket}
              onChange={(e) => setSelectedBracket(e.target.value)}
              className="text-xs border border-slate-300 rounded-lg px-3 py-2 bg-white text-slate-700 focus:ring-1 focus:ring-blue-500"
            >
              <option value="">All Aging Brackets</option>
              <option value="Current">Current / Due soon</option>
              <option value="1-7 Days">1–7 Days</option>
              <option value="8-30 Days">8–30 Days</option>
              <option value="31-60 Days">31–60 Days</option>
              <option value=">60 Days">&gt;60 Days</option>
            </select>
          </div>
        </div>

        {/* Receivables Table */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
                  <th className="py-3 px-4">Customer & Contact</th>
                  <th className="py-3 px-4">Contract & Milestone</th>
                  <th className="py-3 px-4">Due Date</th>
                  <th className="py-3 px-4">Amount Due</th>
                  <th className="py-3 px-4">Paid</th>
                  <th className="py-3 px-4">Outstanding Debt</th>
                  <th className="py-3 px-4">Aging Bracket</th>
                  <th className="py-3 px-4">Latest Action</th>
                  <th className="py-3 px-4 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {isListLoading ? (
                  <tr>
                    <td colSpan={9} className="py-12 text-center text-slate-400">
                      Loading receivables...
                    </td>
                  </tr>
                ) : receivablesList && receivablesList.length > 0 ? (
                  receivablesList.map((item) => (
                    <tr key={item.paymentScheduleId} className="hover:bg-slate-50/70 transition">
                      <td className="py-3.5 px-4">
                        <Link
                          to={`/finance/receivables/${item.accountId}`}
                          className="font-bold text-slate-900 hover:text-blue-600 transition"
                        >
                          {item.accountName}
                        </Link>
                        <div className="text-[11px] text-slate-500 mt-0.5">
                          {item.contactPerson || 'No Contact'} • {item.contactPhone || item.contactEmail || 'N/A'}
                        </div>
                      </td>
                      <td className="py-3.5 px-4">
                        <div className="font-semibold text-slate-800">{item.contractNumber}</div>
                        <div className="text-[11px] text-slate-500 mt-0.5">{item.milestoneTitle}</div>
                      </td>
                      <td className="py-3.5 px-4 font-mono text-slate-700">{item.dueDate}</td>
                      <td className="py-3.5 px-4 font-semibold text-slate-700">{formatVnd(item.totalAmount)}</td>
                      <td className="py-3.5 px-4 text-emerald-600 font-medium">{formatVnd(item.paidAmount)}</td>
                      <td className="py-3.5 px-4 font-bold text-red-600 text-sm">
                        {formatVnd(item.outstandingAmount)}
                      </td>
                      <td className="py-3.5 px-4">
                        <span
                          className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-[11px] font-semibold ${
                            item.agingBracket === 'Current'
                              ? 'bg-emerald-100 text-emerald-800'
                              : item.agingBracket === '1-7 Days'
                              ? 'bg-amber-100 text-amber-800'
                              : item.agingBracket === '8-30 Days'
                              ? 'bg-orange-100 text-orange-800'
                              : 'bg-red-100 text-red-800'
                          }`}
                        >
                          {item.agingBracket}
                          {item.overdueDays > 0 && ` (${item.overdueDays}d)`}
                        </span>
                      </td>
                      <td className="py-3.5 px-4">
                        {item.lastCollectionActivityDate ? (
                          <div>
                            <div className="text-slate-800 font-medium">
                              {new Date(item.lastCollectionActivityDate).toLocaleDateString('vi-VN')}
                            </div>
                            <div className="text-[11px] text-slate-500 truncate max-w-[130px]">
                              {item.lastCollectionStatus || 'Logged'}
                            </div>
                          </div>
                        ) : (
                          <span className="text-slate-400">No logs yet</span>
                        )}
                      </td>
                      <td className="py-3.5 px-4 text-right">
                        <div className="flex items-center justify-end gap-1.5">
                          <button
                            onClick={() => handleOpenLogModal(item)}
                            className="px-2.5 py-1 text-[11px] font-semibold text-blue-700 bg-blue-50 hover:bg-blue-100 rounded border border-blue-200"
                            title="Log Call / Notice / Follow-up"
                          >
                            Log Follow-up
                          </button>
                          <Link
                            to={`/finance/receivables/${item.accountId}`}
                            className="p-1 text-slate-400 hover:text-slate-600 rounded"
                            title="View Customer Debt File"
                          >
                            <ArrowRight className="w-4 h-4" />
                          </Link>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={9} className="py-12 text-center text-slate-400">
                      No outstanding receivables found matching criteria.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Modal: Log Collection Activity */}
      {isLogModalOpen && selectedReceivable && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl max-w-md w-full overflow-hidden border border-slate-200">
            <div className="p-4 border-b border-slate-100 flex items-center justify-between">
              <div>
                <h3 className="font-bold text-slate-900 text-base">Log Debt Collection Activity</h3>
                <p className="text-xs text-slate-500 mt-0.5">{selectedReceivable.accountName}</p>
              </div>
              <button onClick={() => setIsLogModalOpen(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleLogSubmit} className="p-5 space-y-4">
              {formError && (
                <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-xs flex items-center gap-2">
                  <AlertCircle className="w-4 h-4 shrink-0" />
                  <span>{formError}</span>
                </div>
              )}

              <div className="p-3 bg-slate-50 rounded-lg border border-slate-200 text-xs text-slate-700 space-y-1">
                <div>
                  <strong>Contract:</strong> {selectedReceivable.contractNumber}
                </div>
                <div>
                  <strong>Milestone:</strong> {selectedReceivable.milestoneTitle}
                </div>
                <div>
                  <strong>Outstanding Debt:</strong>{' '}
                  <span className="text-red-600 font-bold">{formatVnd(selectedReceivable.outstandingAmount)}</span>
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Activity Type *</label>
                <select
                  value={logForm.activityType}
                  onChange={(e) => setLogForm({ ...logForm, activityType: e.target.value })}
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                >
                  <option value="Phone Call">Phone Call with CFO / Accountant</option>
                  <option value="Email Reminder">Email Overdue Notice</option>
                  <option value="Formal Legal Notice">Formal Notice (Công văn nhắc nợ)</option>
                  <option value="Direct Meeting">Direct Meeting</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Collection Status Outcome</label>
                <select
                  value={logForm.status}
                  onChange={(e) => setLogForm({ ...logForm, status: e.target.value })}
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                >
                  <option value="Customer Promised Payment">Customer Promised Payment</option>
                  <option value="Pending Internal Approval">Client Requested Extension</option>
                  <option value="Customer Disputed">Customer Disputed Deliverable</option>
                  <option value="Resolved">Payment Received / Resolved</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Next Follow-Up Date</label>
                <input
                  type="date"
                  value={logForm.nextFollowUpDate || ''}
                  onChange={(e) => setLogForm({ ...logForm, nextFollowUpDate: e.target.value })}
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Detailed Discussion Notes *</label>
                <textarea
                  rows={3}
                  required
                  value={logForm.notes}
                  onChange={(e) => setLogForm({ ...logForm, notes: e.target.value })}
                  placeholder="e.g. Spoke to Chief Accountant Nguyen. They agreed to schedule wire transfer by end of next week once milestone signoff is countersigned."
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                />
              </div>

              <div className="flex items-center justify-end gap-2 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsLogModalOpen(false)}
                  className="px-4 py-2 text-xs font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={addLogMutation.isPending}
                  className="px-4 py-2 text-xs font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50"
                >
                  {addLogMutation.isPending ? 'Saving...' : 'Save Activity Log'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
