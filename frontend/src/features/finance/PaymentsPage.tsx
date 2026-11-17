import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Plus,
  Search,
  Filter,
  AlertCircle,
  X,
} from 'lucide-react';
import { FinanceNavigation } from './FinanceNavigation';
import { financeService } from '@/services/financeService';
import { contractService } from '@/services/contractService';
import type { Payment, CreatePaymentRequest } from '@/types/finance';

export const PaymentsPage: React.FC = () => {
  const queryClient = useQueryClient();

  const [searchTerm, setSearchTerm] = useState('');
  const [selectedStatus, setSelectedStatus] = useState<string>('');
  const [isRecordModalOpen, setIsRecordModalOpen] = useState(false);
  const [isReverseModalOpen, setIsReverseModalOpen] = useState(false);
  const [selectedPayment, setSelectedPayment] = useState<Payment | null>(null);
  const [reversalReason, setReversalReason] = useState('');
  const [formError, setFormError] = useState<string | null>(null);

  // New Payment Form State
  const [newPayment, setNewPayment] = useState<CreatePaymentRequest>({
    contractId: '',
    accountId: '',
    paymentScheduleId: undefined,
    amount: 0,
    paymentDate: new Date().toISOString().split('T')[0],
    method: 1, // BankTransfer
    transactionReference: '',
    bankName: 'Techcombank',
    bankAccountNumber: '',
    senderName: '',
    notes: '',
  });

  const { data: paymentsData, isLoading } = useQuery({
    queryKey: ['finance', 'payments', searchTerm, selectedStatus],
    queryFn: () =>
      financeService.getPayments({
        searchTerm: searchTerm || undefined,
        status: selectedStatus || undefined,
        pageSize: 50,
      }),
  });

  const { data: contractsList } = useQuery({
    queryKey: ['contracts', 'dropdown'],
    queryFn: () => contractService.getContracts({ pageSize: 100 }),
  });

  const { data: contractSchedules } = useQuery({
    queryKey: ['finance', 'contract-schedules-dropdown', newPayment.contractId],
    queryFn: () => financeService.getContractSchedules(newPayment.contractId),
    enabled: !!newPayment.contractId,
  });

  const createPaymentMutation = useMutation({
    mutationFn: (req: CreatePaymentRequest) => financeService.createPayment(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['finance', 'payments'] });
      queryClient.invalidateQueries({ queryKey: ['finance', 'revenue-analytics'] });
      setIsRecordModalOpen(false);
      setFormError(null);
      resetNewPaymentForm();
    },
    onError: (err: any) => {
      setFormError(err.response?.data?.message || err.message || 'Failed to record payment.');
    },
  });

  const confirmMutation = useMutation({
    mutationFn: (id: string) => financeService.confirmPayment(id, { notes: 'Confirmed via Finance Management' }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['finance', 'payments'] });
      queryClient.invalidateQueries({ queryKey: ['finance', 'revenue-analytics'] });
      queryClient.invalidateQueries({ queryKey: ['finance', 'contract-summary'] });
    },
  });

  const reverseMutation = useMutation({
    mutationFn: (data: { id: string; reason: string }) =>
      financeService.reversePayment(data.id, { reversalReason: data.reason }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['finance', 'payments'] });
      queryClient.invalidateQueries({ queryKey: ['finance', 'revenue-analytics'] });
      queryClient.invalidateQueries({ queryKey: ['finance', 'contract-summary'] });
      setIsReverseModalOpen(false);
      setSelectedPayment(null);
      setReversalReason('');
    },
    onError: (err: any) => {
      setFormError(err.response?.data?.message || err.message || 'Failed to reverse payment.');
    },
  });

  const resetNewPaymentForm = () => {
    setNewPayment({
      contractId: '',
      accountId: '',
      paymentScheduleId: undefined,
      amount: 0,
      paymentDate: new Date().toISOString().split('T')[0],
      method: 1,
      transactionReference: '',
      bankName: 'Techcombank',
      bankAccountNumber: '',
      senderName: '',
      notes: '',
    });
  };

  const handleContractSelect = (contractId: string) => {
    const selectedContract = contractsList?.data?.items?.find((c) => c.id === contractId);
    setNewPayment({
      ...newPayment,
      contractId,
      accountId: selectedContract?.accountId || '',
      amount: selectedContract?.totalValue ? Math.round(selectedContract.totalValue * 0.3) : 0,
    });
  };

  const handleScheduleSelect = (scheduleId: string) => {
    const sched = contractSchedules?.find((s) => s.id === scheduleId);
    setNewPayment({
      ...newPayment,
      paymentScheduleId: scheduleId || undefined,
      amount: sched ? sched.amount - sched.paidAmount : newPayment.amount,
    });
  };

  const handleRecordSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newPayment.contractId) {
      setFormError('Please select a contract.');
      return;
    }
    if (newPayment.amount <= 0) {
      setFormError('Payment amount must be greater than zero.');
      return;
    }

    createPaymentMutation.mutate(newPayment);
  };

  const handleReverseSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedPayment) return;
    if (!reversalReason.trim() || reversalReason.length < 5) {
      setFormError('A clear reversal reason of at least 5 characters is required for audit.');
      return;
    }

    reverseMutation.mutate({
      id: selectedPayment.id,
      reason: reversalReason,
    });
  };

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const getMethodName = (method: number) => {
    switch (method) {
      case 1:
        return 'Bank Transfer';
      case 2:
        return 'Credit Card';
      case 3:
        return 'Cash';
      case 4:
        return 'Cheque';
      default:
        return 'Other';
    }
  };

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <FinanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Payments & Cash Collection</h1>
            <p className="text-sm text-slate-500 mt-1">
              Record customer wire transfers, confirm payments into collected revenue, or reverse disputed transactions.
            </p>
          </div>
          <button
            onClick={() => {
              setIsRecordModalOpen(true);
              setFormError(null);
            }}
            className="inline-flex items-center gap-1.5 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 shadow-xs transition"
          >
            <Plus className="w-4 h-4" />
            <span>Record Payment</span>
          </button>
        </div>

        {/* Filters */}
        <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex flex-col sm:flex-row items-center justify-between gap-4">
          <div className="relative flex-1 w-full sm:max-w-md">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Search by payment #, tx reference, customer, contract..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-9 pr-4 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-1 focus:ring-blue-500 bg-white"
            />
          </div>

          <div className="flex items-center gap-2 w-full sm:w-auto">
            <Filter className="w-4 h-4 text-slate-400 shrink-0" />
            <select
              value={selectedStatus}
              onChange={(e) => setSelectedStatus(e.target.value)}
              className="text-sm border border-slate-300 rounded-lg px-3 py-2 bg-white text-slate-700 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              <option value="">All Statuses</option>
              <option value="Confirmed">Confirmed (Counted in Revenue)</option>
              <option value="Pending">Pending Verification</option>
              <option value="Reversed">Reversed</option>
              <option value="Cancelled">Cancelled</option>
            </select>
          </div>
        </div>

        {/* Payments Data Table */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
                  <th className="py-3 px-4">Payment #</th>
                  <th className="py-3 px-4">Customer & Contract</th>
                  <th className="py-3 px-4">Milestone</th>
                  <th className="py-3 px-4">Amount</th>
                  <th className="py-3 px-4">Date & Method</th>
                  <th className="py-3 px-4">Bank / Sender Info</th>
                  <th className="py-3 px-4">Status</th>
                  <th className="py-3 px-4 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {isLoading ? (
                  <tr>
                    <td colSpan={8} className="py-12 text-center text-slate-400">
                      Loading payments...
                    </td>
                  </tr>
                ) : paymentsData?.items && paymentsData.items.length > 0 ? (
                  paymentsData.items.map((p) => (
                    <tr key={p.id} className="hover:bg-slate-50/70 transition">
                      <td className="py-3.5 px-4 font-bold text-slate-900 font-mono">{p.paymentNumber}</td>
                      <td className="py-3.5 px-4">
                        <div className="font-semibold text-slate-900">{p.accountName}</div>
                        <div className="text-[11px] text-slate-500 mt-0.5">{p.contractNumber}</div>
                      </td>
                      <td className="py-3.5 px-4 text-slate-700">{p.milestoneTitle || 'General Contract Payment'}</td>
                      <td className="py-3.5 px-4 font-bold text-slate-900 text-sm">{formatVnd(p.amount)}</td>
                      <td className="py-3.5 px-4">
                        <div className="text-slate-800">{new Date(p.paymentDate).toLocaleDateString('vi-VN')}</div>
                        <div className="text-[11px] text-slate-500 mt-0.5">{getMethodName(p.method)}</div>
                      </td>
                      <td className="py-3.5 px-4">
                        <div className="text-slate-800">{p.bankName || 'Direct'}</div>
                        <div className="text-[11px] text-slate-500 truncate max-w-[150px]">
                          Ref: {p.transactionReference || 'N/A'}
                        </div>
                      </td>
                      <td className="py-3.5 px-4">
                        <span
                          className={`inline-flex items-center px-2.5 py-1 rounded-full text-[11px] font-semibold ${
                            p.status === 'Confirmed'
                              ? 'bg-emerald-100 text-emerald-800'
                              : p.status === 'Pending'
                              ? 'bg-amber-100 text-amber-800'
                              : p.status === 'Reversed'
                              ? 'bg-purple-100 text-purple-800'
                              : 'bg-slate-100 text-slate-700'
                          }`}
                        >
                          {p.status}
                        </span>
                      </td>
                      <td className="py-3.5 px-4 text-right">
                        <div className="flex items-center justify-end gap-1.5">
                          {p.status === 'Pending' && (
                            <button
                              onClick={() => confirmMutation.mutate(p.id)}
                              disabled={confirmMutation.isPending}
                              className="px-2.5 py-1 text-[11px] font-medium text-emerald-700 bg-emerald-50 hover:bg-emerald-100 rounded border border-emerald-200"
                              title="Confirm payment into collected cash"
                            >
                              Confirm
                            </button>
                          )}
                          {p.status === 'Confirmed' && (
                            <button
                              onClick={() => {
                                setSelectedPayment(p);
                                setIsReverseModalOpen(true);
                                setFormError(null);
                              }}
                              className="px-2.5 py-1 text-[11px] font-medium text-purple-700 bg-purple-50 hover:bg-purple-100 rounded border border-purple-200"
                              title="Reverse payment with audit justification"
                            >
                              Reverse
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={8} className="py-12 text-center text-slate-400">
                      No payments found matching the selected criteria.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Modal: Record Payment */}
      {isRecordModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl max-w-lg w-full overflow-hidden border border-slate-200">
            <div className="p-4 border-b border-slate-100 flex items-center justify-between">
              <h3 className="font-bold text-slate-900 text-base">Record Customer Payment</h3>
              <button onClick={() => setIsRecordModalOpen(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleRecordSubmit} className="p-5 space-y-4">
              {formError && (
                <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-xs flex items-center gap-2">
                  <AlertCircle className="w-4 h-4 shrink-0" />
                  <span>{formError}</span>
                </div>
              )}

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Contract *</label>
                <select
                  required
                  value={newPayment.contractId}
                  onChange={(e) => handleContractSelect(e.target.value)}
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                >
                  <option value="">-- Select Contract --</option>
                  {contractsList?.data?.items?.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.contractNumber} - {c.title} ({c.accountName})
                    </option>
                  ))}
                </select>
              </div>

              {contractSchedules && contractSchedules.length > 0 && (
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Link to Milestone Installment</label>
                  <select
                    value={newPayment.paymentScheduleId || ''}
                    onChange={(e) => handleScheduleSelect(e.target.value)}
                    className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                  >
                    <option value="">-- General Payment (Unlinked) --</option>
                    {contractSchedules.map((s) => (
                      <option key={s.id} value={s.id}>
                        #{s.installmentNumber} {s.milestoneTitle} ({s.percentage}%) - Due: {formatVnd(s.amount - s.paidAmount)}
                      </option>
                    ))}
                  </select>
                </div>
              )}

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Payment Amount (VND) *</label>
                  <input
                    type="number"
                    step="1000"
                    min="1"
                    required
                    value={newPayment.amount}
                    onChange={(e) => setNewPayment({ ...newPayment, amount: parseFloat(e.target.value) || 0 })}
                    className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white font-bold"
                  />
                </div>
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Payment Date *</label>
                  <input
                    type="date"
                    required
                    value={newPayment.paymentDate}
                    onChange={(e) => setNewPayment({ ...newPayment, paymentDate: e.target.value })}
                    className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Payment Method</label>
                  <select
                    value={newPayment.method}
                    onChange={(e) => setNewPayment({ ...newPayment, method: parseInt(e.target.value) })}
                    className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                  >
                    <option value={1}>Bank Transfer (Wire)</option>
                    <option value={2}>Credit Card</option>
                    <option value={3}>Cash</option>
                    <option value={4}>Cheque</option>
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Transaction Ref / FT #</label>
                  <input
                    type="text"
                    value={newPayment.transactionReference || ''}
                    onChange={(e) => setNewPayment({ ...newPayment, transactionReference: e.target.value })}
                    placeholder="e.g. FT26315891290"
                    className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Bank Name</label>
                  <input
                    type="text"
                    value={newPayment.bankName || ''}
                    onChange={(e) => setNewPayment({ ...newPayment, bankName: e.target.value })}
                    placeholder="e.g. Techcombank, VCB"
                    className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                  />
                </div>
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Sender Client Name</label>
                  <input
                    type="text"
                    value={newPayment.senderName || ''}
                    onChange={(e) => setNewPayment({ ...newPayment, senderName: e.target.value })}
                    placeholder="e.g. FPT Software Corp"
                    className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                  />
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Internal Notes</label>
                <textarea
                  rows={2}
                  value={newPayment.notes || ''}
                  onChange={(e) => setNewPayment({ ...newPayment, notes: e.target.value })}
                  placeholder="Notes regarding wire receipt, invoice request, etc."
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                />
              </div>

              <div className="flex items-center justify-end gap-2 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsRecordModalOpen(false)}
                  className="px-4 py-2 text-xs font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={createPaymentMutation.isPending}
                  className="px-4 py-2 text-xs font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50"
                >
                  {createPaymentMutation.isPending ? 'Saving...' : 'Record Payment'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal: Reverse Payment */}
      {isReverseModalOpen && selectedPayment && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl max-w-md w-full overflow-hidden border border-slate-200">
            <div className="p-4 border-b border-slate-100 flex items-center justify-between">
              <h3 className="font-bold text-slate-900 text-base">Reverse Confirmed Payment</h3>
              <button onClick={() => setIsReverseModalOpen(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleReverseSubmit} className="p-5 space-y-4">
              <div className="p-3 bg-purple-50 border border-purple-200 rounded-lg text-xs text-purple-900">
                <strong>Audit Warning:</strong> Reversing payment <strong>{selectedPayment.paymentNumber}</strong> ({formatVnd(selectedPayment.amount)}) will deduct from collected revenue and reopen the linked contract installment schedule.
              </div>

              {formError && (
                <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-xs flex items-center gap-2">
                  <AlertCircle className="w-4 h-4 shrink-0" />
                  <span>{formError}</span>
                </div>
              )}

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">
                  Reversal Reason (Mandatory for Financial Audit) *
                </label>
                <textarea
                  rows={3}
                  required
                  value={reversalReason}
                  onChange={(e) => setReversalReason(e.target.value)}
                  placeholder="e.g. Erroneous wire received, customer requested chargeback, duplicate payment entry."
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-purple-500 bg-white"
                />
              </div>

              <div className="flex items-center justify-end gap-2 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsReverseModalOpen(false)}
                  className="px-4 py-2 text-xs font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={reverseMutation.isPending}
                  className="px-4 py-2 text-xs font-medium text-white bg-purple-600 rounded-lg hover:bg-purple-700 disabled:opacity-50"
                >
                  {reverseMutation.isPending ? 'Reversing...' : 'Confirm Reversal'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
