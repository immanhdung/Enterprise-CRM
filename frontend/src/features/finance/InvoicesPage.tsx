import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Receipt,
  Plus,
  Download,
  RefreshCw,
  XCircle,
  Search,
  ExternalLink,
  AlertCircle,
  X,
  FileText,
  Trash2,
} from 'lucide-react';
import { FinanceNavigation } from './FinanceNavigation';
import { financeService } from '@/services/financeService';
import { contractService } from '@/services/contractService';
import type { Invoice, IssueInvoiceRequest, InvoiceItemInput } from '@/types/finance';

export const InvoicesPage: React.FC = () => {
  const queryClient = useQueryClient();

  const [activeTab, setActiveTab] = useState<'invoices' | 'requests'>('invoices');
  const [searchTerm, setSearchTerm] = useState('');
  const [isIssueModalOpen, setIsIssueModalOpen] = useState(false);
  const [isCancelModalOpen, setIsCancelModalOpen] = useState(false);
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | null>(null);
  const [cancelReason, setCancelReason] = useState('');
  const [formError, setFormError] = useState<string | null>(null);

  // New Invoice Form State
  const [issueRequest, setIssueRequest] = useState<IssueInvoiceRequest>({
    contractId: '',
    accountId: '',
    customerName: '',
    customerTaxCode: '',
    customerAddress: '',
    customerEmail: '',
    issueDate: new Date().toISOString().split('T')[0],
    electronicInvoiceProvider: 'Mock',
    items: [
      { description: 'Cloud Engineering & Architecture Services', unit: 'Gói', quantity: 1, unitPrice: 50000000, taxRate: 8 },
    ],
  });

  const { data: invoicesData, isLoading: isInvoicesLoading } = useQuery({
    queryKey: ['finance', 'invoices', searchTerm],
    queryFn: () => financeService.getInvoices({ searchTerm: searchTerm || undefined, pageSize: 50 }),
    enabled: activeTab === 'invoices',
  });

  const { data: invoiceRequestsData, isLoading: isRequestsLoading } = useQuery({
    queryKey: ['finance', 'invoice-requests'],
    queryFn: () => financeService.getInvoiceRequests({ pageSize: 50 }),
    enabled: activeTab === 'requests',
  });

  const { data: contractsList } = useQuery({
    queryKey: ['contracts', 'dropdown'],
    queryFn: () => contractService.getContracts({ pageSize: 100 }),
  });

  const issueInvoiceMutation = useMutation({
    mutationFn: (req: IssueInvoiceRequest) => financeService.issueInvoice(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['finance', 'invoices'] });
      queryClient.invalidateQueries({ queryKey: ['finance', 'revenue-analytics'] });
      setIsIssueModalOpen(false);
      setFormError(null);
    },
    onError: (err: any) => {
      setFormError(err.response?.data?.message || err.message || 'Failed to issue electronic invoice.');
    },
  });

  const cancelInvoiceMutation = useMutation({
    mutationFn: (data: { id: string; reason: string }) =>
      financeService.cancelInvoice(data.id, { cancellationReason: data.reason }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['finance', 'invoices'] });
      queryClient.invalidateQueries({ queryKey: ['finance', 'revenue-analytics'] });
      setIsCancelModalOpen(false);
      setSelectedInvoice(null);
      setCancelReason('');
    },
    onError: (err: any) => {
      setFormError(err.response?.data?.message || err.message || 'Failed to cancel invoice.');
    },
  });

  const syncStatusMutation = useMutation({
    mutationFn: (id: string) => financeService.syncInvoiceStatus(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['finance', 'invoices'] });
    },
  });

  const approveRequestMutation = useMutation({
    mutationFn: (id: string) => financeService.approveInvoiceRequest(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['finance', 'invoice-requests'] });
      queryClient.invalidateQueries({ queryKey: ['finance', 'invoices'] });
      queryClient.invalidateQueries({ queryKey: ['finance', 'revenue-analytics'] });
    },
  });

  const handleContractSelect = (contractId: string) => {
    const c = contractsList?.data?.items?.find((item) => item.id === contractId);
    if (!c) return;
    setIssueRequest({
      ...issueRequest,
      contractId,
      accountId: c.accountId,
      customerName: c.accountName,
      items: [
        {
          description: `Dịch vụ theo hợp đồng ${c.contractNumber} - ${c.title}`,
          unit: 'Gói',
          quantity: 1,
          unitPrice: Math.round(c.totalValue / 1.08),
          taxRate: 8,
        },
      ],
    });
  };

  const handleAddItem = () => {
    setIssueRequest({
      ...issueRequest,
      items: [...issueRequest.items, { description: '', unit: 'Unit', quantity: 1, unitPrice: 0, taxRate: 8 }],
    });
  };

  const handleRemoveItem = (index: number) => {
    setIssueRequest({
      ...issueRequest,
      items: issueRequest.items.filter((_, i) => i !== index),
    });
  };

  const handleItemChange = (index: number, field: keyof InvoiceItemInput, value: any) => {
    const updated = [...issueRequest.items];
    updated[index] = { ...updated[index], [field]: value };
    setIssueRequest({ ...issueRequest, items: updated });
  };

  const handleDownloadPdf = async (id: string, invoiceNumber: string) => {
    try {
      const blob = await financeService.downloadInvoicePdf(id);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${invoiceNumber}.html`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (e) {
      console.error('Failed to download invoice:', e);
    }
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
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Electronic Invoice Management</h1>
            <p className="text-sm text-slate-500 mt-1">
              Issue compliant electronic VAT invoices, lookup codes, PDF generation, and provider status sync.
            </p>
          </div>
          <button
            onClick={() => {
              setIsIssueModalOpen(true);
              setFormError(null);
            }}
            className="inline-flex items-center gap-1.5 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 shadow-xs transition"
          >
            <Plus className="w-4 h-4" />
            <span>Issue Direct Invoice</span>
          </button>
        </div>

        {/* Tab Selector */}
        <div className="flex items-center gap-3 border-b border-slate-200">
          <button
            onClick={() => setActiveTab('invoices')}
            className={`pb-3 text-sm font-semibold transition border-b-2 flex items-center gap-2 ${
              activeTab === 'invoices'
                ? 'border-blue-600 text-blue-600'
                : 'border-transparent text-slate-500 hover:text-slate-800'
            }`}
          >
            <Receipt className="w-4 h-4" />
            <span>Issued E-Invoices</span>
          </button>
          <button
            onClick={() => setActiveTab('requests')}
            className={`pb-3 text-sm font-semibold transition border-b-2 flex items-center gap-2 ${
              activeTab === 'requests'
                ? 'border-blue-600 text-blue-600'
                : 'border-transparent text-slate-500 hover:text-slate-800'
            }`}
          >
            <FileText className="w-4 h-4" />
            <span>Invoice Requests ({invoiceRequestsData?.totalCount || 0})</span>
          </button>
        </div>

        {/* Active Tab Content */}
        {activeTab === 'invoices' ? (
          <>
            {/* Filter Bar */}
            <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex flex-col sm:flex-row items-center justify-between gap-4">
              <div className="relative flex-1 w-full sm:max-w-md">
                <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="text"
                  placeholder="Search by invoice #, lookup code, customer, tax code..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-9 pr-4 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-1 focus:ring-blue-500 bg-white"
                />
              </div>
            </div>

            {/* Invoices Table */}
            <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
              <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse text-xs">
                  <thead>
                    <tr className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
                      <th className="py-3 px-4">Invoice # & Series</th>
                      <th className="py-3 px-4">Customer</th>
                      <th className="py-3 px-4">Issue Date</th>
                      <th className="py-3 px-4">Subtotal</th>
                      <th className="py-3 px-4">Tax (VAT)</th>
                      <th className="py-3 px-4">Total Amount</th>
                      <th className="py-3 px-4">Status & Provider</th>
                      <th className="py-3 px-4">Lookup Code</th>
                      <th className="py-3 px-4 text-right">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {isInvoicesLoading ? (
                      <tr>
                        <td colSpan={9} className="py-12 text-center text-slate-400">
                          Loading invoices...
                        </td>
                      </tr>
                    ) : invoicesData?.items && invoicesData.items.length > 0 ? (
                      invoicesData.items.map((inv) => (
                        <tr key={inv.id} className="hover:bg-slate-50/70 transition">
                          <td className="py-3 px-4">
                            <div className="font-bold text-slate-900 font-mono">{inv.invoiceNumber}</div>
                            <div className="text-[11px] text-slate-500 font-mono mt-0.5">Series: {inv.invoiceSeries}</div>
                          </td>
                          <td className="py-3 px-4">
                            <div className="font-semibold text-slate-900">{inv.customerName}</div>
                            {inv.customerTaxCode && (
                              <div className="text-[11px] text-slate-500 mt-0.5">MST: {inv.customerTaxCode}</div>
                            )}
                          </td>
                          <td className="py-3 px-4 text-slate-700 font-mono">{inv.issueDate}</td>
                          <td className="py-3 px-4 font-mono text-slate-700">{formatVnd(inv.subtotal)}</td>
                          <td className="py-3 px-4 font-mono text-slate-500">
                            {formatVnd(inv.taxAmount)} ({inv.taxRate}%)
                          </td>
                          <td className="py-3 px-4 font-bold text-slate-900 font-mono text-sm">
                            {formatVnd(inv.totalAmount)}
                          </td>
                          <td className="py-3 px-4">
                            <span
                              className={`inline-flex items-center px-2 py-0.5 rounded text-[11px] font-semibold ${
                                inv.status === 2 || inv.status === 3
                                  ? 'bg-emerald-100 text-emerald-800'
                                  : inv.status === 4
                                  ? 'bg-red-100 text-red-800'
                                  : 'bg-slate-100 text-slate-700'
                              }`}
                            >
                              {inv.status === 2
                                ? 'Published'
                                : inv.status === 3
                                ? 'SentToCustomer'
                                : inv.status === 4
                                ? 'Cancelled'
                                : 'Draft'}
                            </span>
                            <div className="text-[10px] text-slate-400 font-medium mt-0.5">
                              {inv.electronicInvoiceProvider || 'Mock'}
                            </div>
                          </td>
                          <td className="py-3 px-4">
                            {inv.lookupCode ? (
                              <div className="flex items-center gap-1">
                                <span className="font-mono text-xs text-blue-700 bg-blue-50 px-2 py-0.5 rounded">
                                  {inv.lookupCode}
                                </span>
                                {inv.lookupUrl && (
                                  <a
                                    href={inv.lookupUrl}
                                    target="_blank"
                                    rel="noreferrer"
                                    className="text-slate-400 hover:text-blue-600"
                                    title="Open electronic lookup link"
                                  >
                                    <ExternalLink className="w-3.5 h-3.5" />
                                  </a>
                                )}
                              </div>
                            ) : (
                              <span className="text-slate-400">N/A</span>
                            )}
                          </td>
                          <td className="py-3 px-4 text-right">
                            <div className="flex items-center justify-end gap-1.5">
                              <button
                                onClick={() => handleDownloadPdf(inv.id, inv.invoiceNumber)}
                                className="p-1.5 text-slate-600 hover:text-blue-600 hover:bg-slate-100 rounded"
                                title="Download PDF / Invoice View"
                              >
                                <Download className="w-3.5 h-3.5" />
                              </button>
                              <button
                                onClick={() => syncStatusMutation.mutate(inv.id)}
                                disabled={syncStatusMutation.isPending}
                                className="p-1.5 text-slate-600 hover:text-emerald-600 hover:bg-slate-100 rounded"
                                title="Sync with E-Invoice Provider"
                              >
                                <RefreshCw className="w-3.5 h-3.5" />
                              </button>
                              {inv.status !== 4 && (
                                <button
                                  onClick={() => {
                                    setSelectedInvoice(inv);
                                    setIsCancelModalOpen(true);
                                    setFormError(null);
                                  }}
                                  className="p-1.5 text-slate-600 hover:text-red-600 hover:bg-slate-100 rounded"
                                  title="Cancel Invoice"
                                >
                                  <XCircle className="w-3.5 h-3.5" />
                                </button>
                              )}
                            </div>
                          </td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan={9} className="py-12 text-center text-slate-400">
                          No invoices found.
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          </>
        ) : (
          /* Invoice Requests Tab */
          <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
            <div className="p-4 border-b border-slate-100 font-semibold text-sm text-slate-900 flex justify-between items-center">
              <span>Invoice Issuance Requests from Sales</span>
              <span className="text-xs text-slate-500">Requires Accountant Approval</span>
            </div>

            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse text-xs">
                <thead>
                  <tr className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
                    <th className="py-3 px-4">Contract #</th>
                    <th className="py-3 px-4">Customer</th>
                    <th className="py-3 px-4">Requested Amount</th>
                    <th className="py-3 px-4">Requested By</th>
                    <th className="py-3 px-4">Request Date</th>
                    <th className="py-3 px-4">Status</th>
                    <th className="py-3 px-4">Issued Invoice #</th>
                    <th className="py-3 px-4 text-right">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {isRequestsLoading ? (
                    <tr>
                      <td colSpan={8} className="py-12 text-center text-slate-400">
                        Loading requests...
                      </td>
                    </tr>
                  ) : invoiceRequestsData?.items && invoiceRequestsData.items.length > 0 ? (
                    invoiceRequestsData.items.map((r) => (
                      <tr key={r.id} className="hover:bg-slate-50 transition">
                        <td className="py-3.5 px-4 font-semibold text-slate-900">{r.contractNumber}</td>
                        <td className="py-3.5 px-4 text-slate-800">{r.accountName}</td>
                        <td className="py-3.5 px-4 font-bold text-slate-900 text-sm">{formatVnd(r.amount)}</td>
                        <td className="py-3.5 px-4 text-slate-600">{r.requestedByName || 'Sales Rep'}</td>
                        <td className="py-3.5 px-4 text-slate-500 font-mono">
                          {new Date(r.requestDate).toLocaleDateString('vi-VN')}
                        </td>
                        <td className="py-3.5 px-4">
                          <span
                            className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-[11px] font-semibold ${
                              r.status === 'Issued'
                                ? 'bg-emerald-100 text-emerald-800'
                                : r.status === 'Rejected'
                                ? 'bg-red-100 text-red-800'
                                : 'bg-amber-100 text-amber-800'
                            }`}
                          >
                            {r.status}
                          </span>
                        </td>
                        <td className="py-3.5 px-4 font-mono font-semibold text-blue-700">
                          {r.issuedInvoiceNumber || '-'}
                        </td>
                        <td className="py-3.5 px-4 text-right">
                          {r.status === 'Pending' && (
                            <button
                              onClick={() => approveRequestMutation.mutate(r.id)}
                              disabled={approveRequestMutation.isPending}
                              className="px-3 py-1 text-[11px] font-semibold text-white bg-blue-600 hover:bg-blue-700 rounded shadow-xs"
                            >
                              Approve & Issue
                            </button>
                          )}
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={8} className="py-12 text-center text-slate-400">
                        No pending invoice requests found.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>

      {/* Modal: Issue Direct Invoice */}
      {isIssueModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl max-w-2xl w-full overflow-hidden border border-slate-200">
            <div className="p-4 border-b border-slate-100 flex items-center justify-between">
              <h3 className="font-bold text-slate-900 text-base">Issue Electronic VAT Invoice</h3>
              <button onClick={() => setIsIssueModalOpen(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form
              onSubmit={(e) => {
                e.preventDefault();
                issueInvoiceMutation.mutate(issueRequest);
              }}
              className="p-5 space-y-4"
            >
              {formError && (
                <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-xs flex items-center gap-2">
                  <AlertCircle className="w-4 h-4 shrink-0" />
                  <span>{formError}</span>
                </div>
              )}

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Contract *</label>
                  <select
                    required
                    value={issueRequest.contractId}
                    onChange={(e) => handleContractSelect(e.target.value)}
                    className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                  >
                    <option value="">-- Select Contract --</option>
                    {contractsList?.data?.items?.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.contractNumber} - {c.title}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">E-Invoice Provider</label>
                  <select
                    value={issueRequest.electronicInvoiceProvider}
                    onChange={(e) => setIssueRequest({ ...issueRequest, electronicInvoiceProvider: e.target.value })}
                    className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                  >
                    <option value="Mock">Mock / Internal Sandbox</option>
                    <option value="MISA">MISA meInvoice (API Ready)</option>
                    <option value="VNPT">VNPT E-Invoice</option>
                    <option value="Viettel">Viettel Sinvoice</option>
                  </select>
                </div>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Customer / Company Name *</label>
                  <input
                    type="text"
                    required
                    value={issueRequest.customerName}
                    onChange={(e) => setIssueRequest({ ...issueRequest, customerName: e.target.value })}
                    className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                  />
                </div>
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Tax Code (Mã số thuế)</label>
                  <input
                    type="text"
                    value={issueRequest.customerTaxCode || ''}
                    onChange={(e) => setIssueRequest({ ...issueRequest, customerTaxCode: e.target.value })}
                    placeholder="e.g. 0108923456"
                    className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 bg-white"
                  />
                </div>
              </div>

              {/* Items List */}
              <div className="space-y-2">
                <div className="flex items-center justify-between text-xs font-semibold text-slate-700">
                  <span>Goods / Services Line Items</span>
                  <button
                    type="button"
                    onClick={handleAddItem}
                    className="text-blue-600 hover:text-blue-800 inline-flex items-center gap-1 font-medium text-xs"
                  >
                    <Plus className="w-3.5 h-3.5" />
                    <span>Add Item</span>
                  </button>
                </div>

                <div className="space-y-2 max-h-48 overflow-y-auto pr-1">
                  {issueRequest.items.map((item, idx) => (
                    <div key={idx} className="p-2.5 bg-slate-50 border border-slate-200 rounded-lg grid grid-cols-12 gap-2 items-center">
                      <div className="col-span-5">
                        <input
                          type="text"
                          required
                          placeholder="Description"
                          value={item.description}
                          onChange={(e) => handleItemChange(idx, 'description', e.target.value)}
                          className="w-full text-xs px-2 py-1 border border-slate-300 rounded bg-white"
                        />
                      </div>
                      <div className="col-span-2">
                        <input
                          type="number"
                          min="1"
                          required
                          placeholder="Qty"
                          value={item.quantity}
                          onChange={(e) => handleItemChange(idx, 'quantity', parseFloat(e.target.value) || 1)}
                          className="w-full text-xs px-2 py-1 border border-slate-300 rounded bg-white"
                        />
                      </div>
                      <div className="col-span-3">
                        <input
                          type="number"
                          min="0"
                          step="1000"
                          required
                          placeholder="Price"
                          value={item.unitPrice}
                          onChange={(e) => handleItemChange(idx, 'unitPrice', parseFloat(e.target.value) || 0)}
                          className="w-full text-xs px-2 py-1 border border-slate-300 rounded bg-white font-mono"
                        />
                      </div>
                      <div className="col-span-2 flex items-center justify-end">
                        {issueRequest.items.length > 1 && (
                          <button
                            type="button"
                            onClick={() => handleRemoveItem(idx)}
                            className="text-red-500 hover:text-red-700 p-1"
                          >
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              <div className="flex items-center justify-end gap-2 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsIssueModalOpen(false)}
                  className="px-4 py-2 text-xs font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={issueInvoiceMutation.isPending}
                  className="px-4 py-2 text-xs font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50"
                >
                  {issueInvoiceMutation.isPending ? 'Issuing...' : 'Issue Invoice Now'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal: Cancel Invoice */}
      {isCancelModalOpen && selectedInvoice && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl max-w-md w-full overflow-hidden border border-slate-200">
            <div className="p-4 border-b border-slate-100 flex items-center justify-between">
              <h3 className="font-bold text-slate-900 text-base">Cancel Electronic Invoice</h3>
              <button onClick={() => setIsCancelModalOpen(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form
              onSubmit={(e) => {
                e.preventDefault();
                if (!cancelReason.trim()) {
                  setFormError('Cancellation reason is required.');
                  return;
                }
                cancelInvoiceMutation.mutate({ id: selectedInvoice.id, reason: cancelReason });
              }}
              className="p-5 space-y-4"
            >
              <div className="p-3 bg-red-50 border border-red-200 rounded-lg text-xs text-red-900">
                Are you sure you want to cancel invoice <strong>{selectedInvoice.invoiceNumber}</strong>? This action will notify the tax authority / electronic invoice provider.
              </div>

              {formError && (
                <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-xs flex items-center gap-2">
                  <AlertCircle className="w-4 h-4 shrink-0" />
                  <span>{formError}</span>
                </div>
              )}

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">
                  Cancellation Reason *
                </label>
                <textarea
                  rows={3}
                  required
                  value={cancelReason}
                  onChange={(e) => setCancelReason(e.target.value)}
                  placeholder="e.g. Erroneous quantity, client requested amendment, cancellation agreement signed."
                  className="w-full text-xs px-3 py-2 border border-slate-300 rounded-lg focus:ring-1 focus:ring-red-500 bg-white"
                />
              </div>

              <div className="flex items-center justify-end gap-2 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setIsCancelModalOpen(false)}
                  className="px-4 py-2 text-xs font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50"
                >
                  Close
                </button>
                <button
                  type="submit"
                  disabled={cancelInvoiceMutation.isPending}
                  className="px-4 py-2 text-xs font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 disabled:opacity-50"
                >
                  {cancelInvoiceMutation.isPending ? 'Cancelling...' : 'Confirm Cancellation'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
