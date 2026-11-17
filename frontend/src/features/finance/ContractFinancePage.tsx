import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  FileText,
  Plus,
  Trash2,
  CheckCircle,
  AlertCircle,
  Layers,
  Sparkles,
  X,
} from 'lucide-react';
import { FinanceNavigation } from './FinanceNavigation';
import { contractService } from '@/services/contractService';
import { financeService } from '@/services/financeService';
import type { InstallmentPlanItemInput } from '@/types/finance';
import type { Contract } from '@/types/billing';

export const ContractFinancePage: React.FC = () => {
  const queryClient = useQueryClient();
  const [selectedContractId, setSelectedContractId] = useState<string | null>(null);
  const [isGenerateModalOpen, setIsGenerateModalOpen] = useState(false);
  const [modalContract, setModalContract] = useState<{ id: string; number: string; totalValue: number } | null>(null);

  // Form state for generating multi-installment schedule
  const [installments, setInstallments] = useState<InstallmentPlanItemInput[]>([
    { milestoneTitle: 'Advance Payment / Kickoff', percentage: 30, dueDate: new Date().toISOString().split('T')[0], notes: '30% upon signing' },
    { milestoneTitle: 'Milestone 1: Core Architecture Delivery', percentage: 40, dueDate: new Date(Date.now() + 30 * 86400000).toISOString().split('T')[0], notes: '40% upon prototype signoff' },
    { milestoneTitle: 'Final Acceptance & Handover', percentage: 30, dueDate: new Date(Date.now() + 90 * 86400000).toISOString().split('T')[0], notes: '30% upon final acceptance' },
  ]);

  const [formError, setFormError] = useState<string | null>(null);

  const { data: contractsData, isLoading } = useQuery({
    queryKey: ['contracts', 'finance-view'],
    queryFn: () => contractService.getContracts({ pageSize: 50 }),
  });

  const { data: financialSummary, isLoading: isSummaryLoading } = useQuery({
    queryKey: ['finance', 'contract-summary', selectedContractId],
    queryFn: () => financeService.getContractFinancialSummary(selectedContractId!),
    enabled: !!selectedContractId,
  });

  const generateMutation = useMutation({
    mutationFn: (data: { contractId: string; installments: InstallmentPlanItemInput[] }) =>
      financeService.generateContractSchedules(data.contractId, { installments: data.installments }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['finance', 'contract-summary'] });
      queryClient.invalidateQueries({ queryKey: ['contracts'] });
      setIsGenerateModalOpen(false);
      setFormError(null);
    },
    onError: (err: any) => {
      setFormError(err.response?.data?.message || err.message || 'Failed to generate installment plan.');
    },
  });

  const totalPercentage = installments.reduce((acc, curr) => acc + (Number(curr.percentage) || 0), 0);

  const handleOpenGenerateModal = (contract: { id: string; contractNumber: string; totalValue: number }) => {
    setModalContract({ id: contract.id, number: contract.contractNumber, totalValue: contract.totalValue });
    setIsGenerateModalOpen(true);
    setFormError(null);
  };

  const handleAddInstallment = () => {
    setInstallments([
      ...installments,
      { milestoneTitle: `Milestone ${installments.length + 1}`, percentage: 0, dueDate: new Date().toISOString().split('T')[0] },
    ]);
  };

  const handleRemoveInstallment = (index: number) => {
    setInstallments(installments.filter((_, i) => i !== index));
  };

  const handleInstallmentChange = (index: number, field: keyof InstallmentPlanItemInput, value: any) => {
    const updated = [...installments];
    updated[index] = { ...updated[index], [field]: value };
    setInstallments(updated);
  };

  const handleSaveInstallments = (e: React.FormEvent) => {
    e.preventDefault();
    if (Math.abs(totalPercentage - 100) > 0.01) {
      setFormError(`Installment percentages must sum to exactly 100%. Current sum: ${totalPercentage}%`);
      return;
    }
    if (!modalContract) return;

    generateMutation.mutate({
      contractId: modalContract.id,
      installments,
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
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Contract Payment Schedules</h1>
            <p className="text-sm text-slate-500 mt-1">
              Multi-installment milestone plans, percentage breakdown (e.g. 30/40/30), paid vs outstanding tracking.
            </p>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Contracts List */}
          <div className="lg:col-span-1 bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
            <div className="p-4 border-b border-slate-100 font-semibold text-sm text-slate-900 flex justify-between items-center">
              <span>Contracts ({contractsData?.data?.items?.length || 0})</span>
            </div>

            <div className="divide-y divide-slate-100 max-h-[700px] overflow-y-auto">
              {isLoading ? (
                <div className="p-8 text-center text-slate-400 text-sm">Loading contracts...</div>
              ) : contractsData?.data?.items && contractsData.data.items.length > 0 ? (
                contractsData.data.items.map((c: Contract) => {
                  const isSelected = selectedContractId === c.id;
                  return (
                    <div
                      key={c.id}
                      onClick={() => setSelectedContractId(c.id)}
                      className={`p-4 cursor-pointer transition flex flex-col gap-1.5 ${
                        isSelected ? 'bg-blue-50/70 border-l-4 border-blue-600' : 'hover:bg-slate-50'
                      }`}
                    >
                      <div className="flex items-center justify-between">
                        <span className="font-semibold text-sm text-slate-900">{c.contractNumber}</span>
                        <span className="text-xs font-semibold text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded">
                          {formatVnd(c.totalValue)}
                        </span>
                      </div>
                      <div className="text-xs text-slate-600 font-medium truncate">{c.title}</div>
                      <div className="text-xs text-slate-400">{c.accountName}</div>
                      <div className="flex items-center justify-between mt-2 pt-2 border-t border-slate-100/60">
                        <span className="text-xs text-slate-500 font-mono">
                          {c.startDate} ~ {c.endDate}
                        </span>
                        <button
                          type="button"
                          onClick={(e) => {
                            e.stopPropagation();
                            handleOpenGenerateModal(c);
                          }}
                          className="text-xs font-medium text-blue-600 hover:text-blue-800"
                        >
                          Plan Schedules
                        </button>
                      </div>
                    </div>
                  );
                })
              ) : (
                <div className="p-8 text-center text-slate-400 text-sm">No contracts found.</div>
              )}
            </div>
          </div>

          {/* Selected Contract Financial Details & Schedules */}
          <div className="lg:col-span-2 space-y-6">
            {selectedContractId ? (
              isSummaryLoading ? (
                <div className="bg-white p-12 rounded-xl border border-slate-200 text-center text-slate-400">
                  Loading financial summary...
                </div>
              ) : financialSummary ? (
                <>
                  {/* Summary Bar */}
                  <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
                    <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 pb-4 border-b border-slate-100">
                      <div>
                        <span className="text-xs font-bold text-blue-600 uppercase tracking-wider">
                          {financialSummary.contractNumber}
                        </span>
                        <h2 className="text-lg font-bold text-slate-900 mt-0.5">{financialSummary.contractTitle}</h2>
                        <div className="text-xs text-slate-500 mt-0.5">Account: {financialSummary.accountName}</div>
                      </div>
                      <button
                        onClick={() =>
                          handleOpenGenerateModal({
                            id: financialSummary.contractId,
                            contractNumber: financialSummary.contractNumber,
                            totalValue: financialSummary.totalValue,
                          })
                        }
                        className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 shadow-xs transition"
                      >
                        <Sparkles className="w-3.5 h-3.5" />
                        <span>Regenerate Installments</span>
                      </button>
                    </div>

                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 pt-4">
                      <div>
                        <div className="text-xs text-slate-500 font-medium">Contract Value</div>
                        <div className="text-base font-bold text-slate-900 mt-0.5">
                          {formatVnd(financialSummary.totalValue)}
                        </div>
                      </div>
                      <div>
                        <div className="text-xs text-slate-500 font-medium">Paid Cash</div>
                        <div className="text-base font-bold text-emerald-600 mt-0.5">
                          {formatVnd(financialSummary.paidAmount)}
                        </div>
                      </div>
                      <div>
                        <div className="text-xs text-slate-500 font-medium">Outstanding Balance</div>
                        <div className="text-base font-bold text-amber-600 mt-0.5">
                          {formatVnd(financialSummary.outstandingAmount)}
                        </div>
                      </div>
                      <div>
                        <div className="text-xs text-slate-500 font-medium">Milestones</div>
                        <div className="text-base font-bold text-purple-600 mt-0.5">
                          {financialSummary.paidSchedulesCount} / {financialSummary.totalSchedulesCount} Paid
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Payment Schedules Table */}
                  <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
                    <div className="p-4 border-b border-slate-100 font-semibold text-sm text-slate-900 flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <Layers className="w-4 h-4 text-blue-600" />
                        <span>Installment Schedule Milestones</span>
                      </div>
                      <span className="text-xs text-slate-500">
                        Total {financialSummary.schedules.length} installments
                      </span>
                    </div>

                    <div className="overflow-x-auto">
                      <table className="w-full text-left border-collapse text-xs">
                        <thead>
                          <tr className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
                            <th className="py-2.5 px-3">#</th>
                            <th className="py-2.5 px-3">Milestone Title</th>
                            <th className="py-2.5 px-3">% Ratio</th>
                            <th className="py-2.5 px-3">Due Date</th>
                            <th className="py-2.5 px-3">Amount Due</th>
                            <th className="py-2.5 px-3">Paid Amount</th>
                            <th className="py-2.5 px-3">Status</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-100">
                          {financialSummary.schedules.map((s) => (
                            <tr key={s.id} className="hover:bg-slate-50 transition">
                              <td className="py-3 px-3 font-medium text-slate-900">{s.installmentNumber}</td>
                              <td className="py-3 px-3">
                                <div className="font-semibold text-slate-900">{s.milestoneTitle}</div>
                                {s.notes && <div className="text-slate-400 text-[11px] mt-0.5">{s.notes}</div>}
                              </td>
                              <td className="py-3 px-3 font-semibold text-blue-600">{s.percentage}%</td>
                              <td className="py-3 px-3 text-slate-600 font-mono">{s.dueDate}</td>
                              <td className="py-3 px-3 font-semibold text-slate-900">{formatVnd(s.amount)}</td>
                              <td className="py-3 px-3 text-emerald-600 font-medium">{formatVnd(s.paidAmount)}</td>
                              <td className="py-3 px-3">
                                <span
                                  className={`inline-flex items-center px-2 py-0.5 rounded text-[11px] font-semibold ${
                                    s.status === 'Paid'
                                      ? 'bg-emerald-100 text-emerald-800'
                                      : s.status === 'PartiallyPaid'
                                      ? 'bg-blue-100 text-blue-800'
                                      : s.status === 'Overdue'
                                      ? 'bg-red-100 text-red-800'
                                      : 'bg-amber-100 text-amber-800'
                                  }`}
                                >
                                  {s.status}
                                </span>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>

                  {/* Confirmed Payments on this Contract */}
                  <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
                    <div className="p-4 border-b border-slate-100 font-semibold text-sm text-slate-900 flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <CheckCircle className="w-4 h-4 text-emerald-600" />
                        <span>Recorded Payments on this Contract</span>
                      </div>
                      <span className="text-xs text-slate-500">{financialSummary.payments.length} transactions</span>
                    </div>

                    <div className="divide-y divide-slate-100">
                      {financialSummary.payments.length > 0 ? (
                        financialSummary.payments.map((p) => (
                          <div key={p.id} className="p-3.5 flex items-center justify-between text-xs hover:bg-slate-50">
                            <div>
                              <div className="font-semibold text-slate-900">{p.paymentNumber}</div>
                              <div className="text-slate-500 mt-0.5">
                                {new Date(p.paymentDate).toLocaleDateString('vi-VN')} • Ref: {p.transactionReference || 'N/A'} • {p.bankName || 'Direct'}
                              </div>
                            </div>
                            <div className="text-right">
                              <div className="font-bold text-slate-900">{formatVnd(p.amount)}</div>
                              <span
                                className={`inline-flex items-center px-2 py-0.5 rounded text-[10px] font-medium mt-0.5 ${
                                  p.status === 'Confirmed'
                                    ? 'bg-emerald-100 text-emerald-800'
                                    : p.status === 'Reversed'
                                    ? 'bg-purple-100 text-purple-800'
                                    : 'bg-amber-100 text-amber-800'
                                }`}
                              >
                                {p.status}
                              </span>
                            </div>
                          </div>
                        ))
                      ) : (
                        <div className="p-6 text-center text-slate-400 text-xs">No payments recorded for this contract yet.</div>
                      )}
                    </div>
                  </div>
                </>
              ) : null
            ) : (
              <div className="bg-white p-16 rounded-xl border border-dashed border-slate-300 text-center text-slate-400">
                <FileText className="w-12 h-12 mx-auto text-slate-300 mb-3" />
                <h3 className="text-base font-semibold text-slate-700">Select a contract from the list</h3>
                <p className="text-xs text-slate-500 mt-1">
                  View multi-installment breakdown, milestone payment dates, paid status, and recorded transactions.
                </p>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Modal: Generate Installment Plan */}
      {isGenerateModalOpen && modalContract && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl max-w-2xl w-full overflow-hidden border border-slate-200 animate-in fade-in zoom-in duration-150">
            <div className="p-4 border-b border-slate-100 flex items-center justify-between">
              <div>
                <h3 className="font-bold text-slate-900 text-base">Generate Installment Schedule</h3>
                <p className="text-xs text-slate-500 mt-0.5">
                  Contract {modalContract.number} • Total: {formatVnd(modalContract.totalValue)}
                </p>
              </div>
              <button
                onClick={() => setIsGenerateModalOpen(false)}
                className="text-slate-400 hover:text-slate-600 p-1"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleSaveInstallments} className="p-5 space-y-4">
              {formError && (
                <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-xs flex items-center gap-2">
                  <AlertCircle className="w-4 h-4 shrink-0" />
                  <span>{formError}</span>
                </div>
              )}

              <div className="flex items-center justify-between text-xs font-semibold text-slate-700">
                <span>Milestone Installments</span>
                <span className={`px-2 py-0.5 rounded font-mono ${totalPercentage === 100 ? 'bg-emerald-100 text-emerald-800' : 'bg-amber-100 text-amber-800'}`}>
                  Total: {totalPercentage}% / 100%
                </span>
              </div>

              <div className="space-y-3 max-h-[350px] overflow-y-auto pr-1">
                {installments.map((item, index) => {
                  const estAmount = Math.round(modalContract.totalValue * ((Number(item.percentage) || 0) / 100));
                  return (
                    <div key={index} className="p-3 bg-slate-50 border border-slate-200 rounded-lg space-y-2">
                      <div className="flex items-center justify-between gap-2">
                        <span className="text-xs font-bold text-slate-600">Installment #{index + 1}</span>
                        {installments.length > 1 && (
                          <button
                            type="button"
                            onClick={() => handleRemoveInstallment(index)}
                            className="text-red-500 hover:text-red-700 p-1"
                          >
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        )}
                      </div>

                      <div className="grid grid-cols-1 sm:grid-cols-3 gap-2">
                        <div className="sm:col-span-2">
                          <label className="block text-[11px] font-medium text-slate-600 mb-0.5">Milestone Title</label>
                          <input
                            type="text"
                            required
                            value={item.milestoneTitle}
                            onChange={(e) => handleInstallmentChange(index, 'milestoneTitle', e.target.value)}
                            className="w-full text-xs px-2.5 py-1.5 border border-slate-300 rounded focus:ring-1 focus:ring-blue-500 bg-white"
                            placeholder="e.g. Kickoff 30%"
                          />
                        </div>
                        <div>
                          <label className="block text-[11px] font-medium text-slate-600 mb-0.5">Ratio (%)</label>
                          <input
                            type="number"
                            step="0.01"
                            min="1"
                            max="100"
                            required
                            value={item.percentage}
                            onChange={(e) => handleInstallmentChange(index, 'percentage', parseFloat(e.target.value) || 0)}
                            className="w-full text-xs px-2.5 py-1.5 border border-slate-300 rounded focus:ring-1 focus:ring-blue-500 bg-white font-semibold"
                          />
                        </div>
                      </div>

                      <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 pt-1">
                        <div>
                          <label className="block text-[11px] font-medium text-slate-600 mb-0.5">Due Date</label>
                          <input
                            type="date"
                            required
                            value={item.dueDate}
                            onChange={(e) => handleInstallmentChange(index, 'dueDate', e.target.value)}
                            className="w-full text-xs px-2.5 py-1.5 border border-slate-300 rounded focus:ring-1 focus:ring-blue-500 bg-white"
                          />
                        </div>
                        <div>
                          <label className="block text-[11px] font-medium text-slate-600 mb-0.5">Calculated Amount</label>
                          <div className="text-xs font-bold text-slate-800 px-2.5 py-1.5 bg-slate-100 rounded border border-slate-200">
                            {formatVnd(estAmount)}
                          </div>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>

              <div className="flex items-center justify-between pt-2">
                <button
                  type="button"
                  onClick={handleAddInstallment}
                  className="inline-flex items-center gap-1 text-xs font-semibold text-blue-600 hover:text-blue-800"
                >
                  <Plus className="w-3.5 h-3.5" />
                  <span>Add Another Milestone</span>
                </button>
              </div>

              <div className="flex items-center justify-end gap-2 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsGenerateModalOpen(false)}
                  className="px-4 py-2 text-xs font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={generateMutation.isPending || Math.abs(totalPercentage - 100) > 0.01}
                  className="px-4 py-2 text-xs font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50"
                >
                  {generateMutation.isPending ? 'Generating...' : 'Save & Generate Schedules'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
