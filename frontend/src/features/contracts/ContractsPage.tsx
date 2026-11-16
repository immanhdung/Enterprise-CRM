import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Plus,
  Search,
  FileCheck,
  Download,
  Eye,
  Trash2,
  RefreshCw,
  FileText,
  Sparkles,
} from 'lucide-react';
import { contractService, type CreateContractRequest, type CreateContractFromQuotationRequest } from '../../services/contractService';
import { contractTemplateService } from '../../services/contractTemplateService';
import { quotationService } from '../../services/quotationService';
import { accountService } from '../../services/accountService';
import type { Contract, ContractTemplate, Quotation } from '../../types/billing';
import type { AccountDto, PagedResult } from '../../types/crm';
import { Modal } from '../../components/common/Modal';

export const ContractsPage: React.FC = () => {
  const navigate = useNavigate();

  // Data states
  const [data, setData] = useState<PagedResult<Contract>>({
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 10,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  });
  const [accounts, setAccounts] = useState<AccountDto[]>([]);
  const [templates, setTemplates] = useState<ContractTemplate[]>([]);
  const [approvedQuotations, setApprovedQuotations] = useState<Quotation[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  // Filters
  const [search, setSearch] = useState('');
  const [selectedStatus, setSelectedStatus] = useState('');
  const [pageNumber, setPageNumber] = useState(1);

  // Modals state
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isFromQuoteOpen, setIsFromQuoteOpen] = useState(false);

  // Form states
  const [title, setTitle] = useState('');
  const [accountId, setAccountId] = useState('');
  const [templateId, setTemplateId] = useState('');
  const [startDate, setStartDate] = useState(new Date().toISOString().split('T')[0]);
  const [endDate, setEndDate] = useState(
    new Date(Date.now() + 365 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
  );
  const [totalValue, setTotalValue] = useState(0);
  const [scopeOfWork, setScopeOfWork] = useState('');
  const [paymentTerms, setPaymentTerms] = useState('50% advance upon contract signing, 50% upon final acceptance.');
  const [specialTerms, setSpecialTerms] = useState('');

  // From Quote state
  const [selectedQuoteId, setSelectedQuoteId] = useState('');
  const [fromQuoteTemplateId, setFromQuoteTemplateId] = useState('');
  const [fromQuoteTitle, setFromQuoteTitle] = useState('');

  const loadData = useCallback(async () => {
    setIsLoading(true);
    try {
      const res = await contractService.getContracts({
        pageNumber,
        pageSize: 10,
        search: search || undefined,
        status: selectedStatus || undefined,
      });
      if (res.data) setData(res.data);
    } catch (err) {
      console.error('Failed to load contracts', err);
    } finally {
      setIsLoading(false);
    }
  }, [pageNumber, search, selectedStatus]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // Load lookup options
  useEffect(() => {
    const loadLookups = async () => {
      try {
        const [accRes, tplRes, quoteRes] = await Promise.all([
          accountService.getAccounts({ pageSize: 100 }),
          contractTemplateService.getTemplates(false),
          quotationService.getQuotations({ status: 'Approved', pageSize: 50 }),
        ]);
        if (accRes.data?.items) setAccounts(accRes.data.items);
        if (tplRes.data) setTemplates(tplRes.data);
        if (quoteRes.data?.items) setApprovedQuotations(quoteRes.data.items);
      } catch (err) {
        console.error('Failed to load lookup items', err);
      }
    };
    loadLookups();
  }, []);

  const handleOpenCreate = () => {
    setTitle('');
    setAccountId(accounts.length > 0 ? accounts[0].id : '');
    setTemplateId(templates.length > 0 ? templates[0].id : '');
    setStartDate(new Date().toISOString().split('T')[0]);
    setEndDate(new Date(Date.now() + 365 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]);
    setTotalValue(0);
    setScopeOfWork('');
    setPaymentTerms('50% advance upon contract signing, 50% upon final acceptance.');
    setSpecialTerms('');
    setIsCreateOpen(true);
  };

  const handleOpenFromQuote = () => {
    if (approvedQuotations.length > 0) {
      const q = approvedQuotations[0];
      setSelectedQuoteId(q.id);
      setFromQuoteTitle(`Professional Service Agreement - ${q.accountName}`);
    } else {
      setSelectedQuoteId('');
      setFromQuoteTitle('');
    }
    setFromQuoteTemplateId(templates.length > 0 ? templates[0].id : '');
    setIsFromQuoteOpen(true);
  };

  const handleCreateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!accountId) {
      alert('Please select an Account');
      return;
    }

    const payload: CreateContractRequest = {
      title,
      accountId,
      templateId: templateId || undefined,
      startDate: new Date(startDate).toISOString(),
      endDate: new Date(endDate).toISOString(),
      totalValue,
      scopeOfWork,
      paymentTerms,
      specialTerms,
    };

    try {
      const res = await contractService.createContract(payload);
      setIsCreateOpen(false);
      if (res.data) {
        navigate(`/contracts/${res.data.id}`);
      } else {
        loadData();
      }
    } catch (err) {
      console.error('Failed to create contract', err);
    }
  };

  const handleFromQuoteSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedQuoteId) {
      alert('Please select an Approved Quotation');
      return;
    }

    const payload: CreateContractFromQuotationRequest = {
      quotationId: selectedQuoteId,
      templateId: fromQuoteTemplateId || undefined,
      title: fromQuoteTitle || undefined,
    };

    try {
      const res = await contractService.createFromQuotation(payload);
      setIsFromQuoteOpen(false);
      if (res.data) {
        navigate(`/contracts/${res.data.id}`);
      } else {
        loadData();
      }
    } catch (err) {
      console.error('Failed to create contract from quotation', err);
    }
  };

  const handleDownloadPdf = async (e: React.MouseEvent, contract: Contract) => {
    e.stopPropagation();
    try {
      await contractService.downloadPdf(contract.id, contract.contractNumber);
    } catch (err) {
      console.error('Failed to download PDF', err);
    }
  };

  const handleDelete = async (e: React.MouseEvent, id: string) => {
    e.stopPropagation();
    if (!confirm('Are you sure you want to cancel/delete this contract?')) return;
    try {
      await contractService.deleteContract(id);
      loadData();
    } catch (err) {
      console.error('Failed to delete contract', err);
    }
  };

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Draft':
        return <span className="px-2.5 py-0.5 rounded text-xs font-semibold bg-slate-100 text-slate-700">Draft</span>;
      case 'Approved':
        return <span className="px-2.5 py-0.5 rounded text-xs font-semibold bg-blue-100 text-blue-800">Approved</span>;
      case 'PendingSignature':
        return <span className="px-2.5 py-0.5 rounded text-xs font-semibold bg-amber-100 text-amber-800">Awaiting Signature</span>;
      case 'Signed':
        return <span className="px-2.5 py-0.5 rounded text-xs font-semibold bg-emerald-100 text-emerald-800">Signed</span>;
      case 'Active':
        return <span className="px-2.5 py-0.5 rounded text-xs font-semibold bg-emerald-100 text-emerald-800">Active</span>;
      case 'Renewed':
        return <span className="px-2.5 py-0.5 rounded text-xs font-semibold bg-purple-100 text-purple-800">Renewed</span>;
      case 'Expired':
        return <span className="px-2.5 py-0.5 rounded text-xs font-semibold bg-rose-100 text-rose-800">Expired</span>;
      default:
        return <span className="px-2.5 py-0.5 rounded text-xs font-semibold bg-slate-100 text-slate-700">{status}</span>;
    }
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 flex items-center gap-2">
            <FileCheck className="w-7 h-7 text-indigo-600" />
            Contract Lifecycle & E-Signatures
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            Manage legally-binding service contracts, variable appendices, template token merges, and electronic signatures.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={() => navigate('/contract-templates')}
            className="flex items-center gap-2 px-3 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-medium text-sm rounded-lg transition-colors"
          >
            <FileText className="w-4 h-4" />
            Templates
          </button>
          <button
            onClick={handleOpenFromQuote}
            className="flex items-center gap-2 px-3.5 py-2 bg-purple-600 hover:bg-purple-700 text-white font-medium text-sm rounded-lg shadow-sm transition-colors"
          >
            <Sparkles className="w-4 h-4" />
            From Approved Quotation
          </button>
          <button
            onClick={handleOpenCreate}
            className="flex items-center gap-2 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white font-medium text-sm rounded-lg shadow-sm transition-colors"
          >
            <Plus className="w-4 h-4" />
            New Contract
          </button>
        </div>
      </div>

      {/* Filters Bar */}
      <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex flex-wrap gap-4 items-center justify-between">
        <div className="flex flex-wrap items-center gap-3 flex-1 min-w-[280px]">
          <div className="relative flex-1 max-w-md">
            <Search className="w-4 h-4 absolute left-3 top-3 text-slate-400" />
            <input
              type="text"
              placeholder="Search by contract #, title, account..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-9 pr-4 py-2 border border-slate-300 rounded-lg text-sm focus:outline-hidden focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <select
            value={selectedStatus}
            onChange={(e) => setSelectedStatus(e.target.value)}
            className="px-3 py-2 border border-slate-300 rounded-lg text-sm bg-white text-slate-700 focus:outline-hidden focus:ring-2 focus:ring-indigo-500"
          >
            <option value="">All Statuses</option>
            <option value="Draft">Draft</option>
            <option value="Approved">Approved</option>
            <option value="PendingSignature">Pending Signature</option>
            <option value="Signed">Signed</option>
            <option value="Active">Active</option>
            <option value="Renewed">Renewed</option>
            <option value="Expired">Expired</option>
          </select>
        </div>

        <button
          onClick={loadData}
          className="p-2 border border-slate-300 rounded-lg hover:bg-slate-50 text-slate-600 transition-colors"
          title="Refresh contracts"
        >
          <RefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin' : ''}`} />
        </button>
      </div>

      {/* Data Table */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm text-slate-700">
            <thead className="bg-slate-50 border-b border-slate-200 text-xs uppercase font-semibold text-slate-500 tracking-wider">
              <tr>
                <th className="px-6 py-3.5">Contract #</th>
                <th className="px-6 py-3.5">Title & Account</th>
                <th className="px-6 py-3.5 text-right">Total Value</th>
                <th className="px-6 py-3.5">Contract Period</th>
                <th className="px-6 py-3.5 text-center">Status</th>
                <th className="px-6 py-3.5 text-center">E-Sign Status</th>
                <th className="px-6 py-3.5 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-200">
              {isLoading && data.items.length === 0 ? (
                <tr>
                  <td colSpan={7} className="px-6 py-10 text-center text-slate-400">
                    Loading contracts...
                  </td>
                </tr>
              ) : data.items.length === 0 ? (
                <tr>
                  <td colSpan={7} className="px-6 py-12 text-center text-slate-400">
                    No contracts found matching your search.
                  </td>
                </tr>
              ) : (
                data.items.map((c) => (
                  <tr
                    key={c.id}
                    onClick={() => navigate(`/contracts/${c.id}`)}
                    className="hover:bg-slate-50/75 cursor-pointer transition-colors"
                  >
                    <td className="px-6 py-4 font-mono font-medium text-indigo-600">
                      {c.contractNumber}
                    </td>
                    <td className="px-6 py-4">
                      <div className="font-semibold text-slate-900">{c.title}</div>
                      <div className="text-xs text-slate-500">{c.accountName}</div>
                    </td>
                    <td className="px-6 py-4 text-right font-bold text-slate-900">
                      {formatCurrency(c.totalValue)}
                    </td>
                    <td className="px-6 py-4 text-xs text-slate-600">
                      {new Date(c.startDate).toLocaleDateString()} -{' '}
                      {new Date(c.endDate).toLocaleDateString()}
                    </td>
                    <td className="px-6 py-4 text-center">
                      {getStatusBadge(c.status)}
                    </td>
                    <td className="px-6 py-4 text-center">
                      <span className="font-mono text-xs px-2 py-0.5 bg-slate-100 rounded text-slate-600 font-medium">
                        {c.esignStatus || 'Not Dispatched'}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-right space-x-2">
                      <button
                        onClick={(e) => handleDownloadPdf(e, c)}
                        title="Download PDF"
                        className="p-1.5 text-slate-400 hover:text-indigo-600 transition-colors"
                      >
                        <Download className="w-4 h-4" />
                      </button>
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          navigate(`/contracts/${c.id}`);
                        }}
                        title="View Details"
                        className="p-1.5 text-slate-400 hover:text-blue-600 transition-colors"
                      >
                        <Eye className="w-4 h-4" />
                      </button>
                      {c.status === 'Draft' && (
                        <button
                          onClick={(e) => handleDelete(e, c.id)}
                          title="Delete Contract"
                          className="p-1.5 text-slate-400 hover:text-rose-600 transition-colors"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {data.totalPages > 1 && (
          <div className="px-6 py-3 border-t border-slate-200 flex items-center justify-between text-xs text-slate-500">
            <div>
              Showing page {data.pageNumber} of {data.totalPages} ({data.totalCount} total)
            </div>
            <div className="flex gap-2">
              <button
                disabled={!data.hasPreviousPage}
                onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                className="px-3 py-1 border border-slate-300 rounded disabled:opacity-50 hover:bg-slate-50"
              >
                Previous
              </button>
              <button
                disabled={!data.hasNextPage}
                onClick={() => setPageNumber((p) => p + 1)}
                className="px-3 py-1 border border-slate-300 rounded disabled:opacity-50 hover:bg-slate-50"
              >
                Next
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Create Manual Contract Modal */}
      <Modal
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        title="Draft Service Contract"
      >
        <form onSubmit={handleCreateSubmit} className="space-y-4">
          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Contract Title *</label>
            <input
              type="text"
              required
              placeholder="e.g. Master Services Agreement - Cloud Infrastructure"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Client Account *</label>
              <select
                required
                value={accountId}
                onChange={(e) => setAccountId(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500"
              >
                <option value="">-- Select Client --</option>
                {accounts.map((a) => (
                  <option key={a.id} value={a.id}>
                    {a.name}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Contract Template</label>
              <select
                value={templateId}
                onChange={(e) => setTemplateId(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500"
              >
                <option value="">-- Standard Generic Template --</option>
                {templates.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.name} ({t.code})
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Total Value (VND) *</label>
              <input
                type="number"
                required
                min="0"
                step="500000"
                value={totalValue}
                onChange={(e) => setTotalValue(Number(e.target.value))}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Start Date *</label>
              <input
                type="date"
                required
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">End Date *</label>
              <input
                type="date"
                required
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Scope of Work</label>
            <textarea
              rows={3}
              placeholder="Outline deliverables, SLAs, milestones..."
              value={scopeOfWork}
              onChange={(e) => setScopeOfWork(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Payment Terms</label>
            <input
              type="text"
              value={paymentTerms}
              onChange={(e) => setPaymentTerms(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div className="flex justify-end gap-3 pt-3 border-t border-slate-200">
            <button
              type="button"
              onClick={() => setIsCreateOpen(false)}
              className="px-4 py-2 border border-slate-300 rounded-lg text-sm font-medium text-slate-700 hover:bg-slate-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-medium shadow-sm"
            >
              Create Contract Draft
            </button>
          </div>
        </form>
      </Modal>

      {/* From Approved Quotation Modal */}
      <Modal
        isOpen={isFromQuoteOpen}
        onClose={() => setIsFromQuoteOpen(false)}
        title="Generate Contract from Approved Quotation"
      >
        <form onSubmit={handleFromQuoteSubmit} className="space-y-4">
          <p className="text-xs text-slate-600">
            Automatically inherits client details, line items, deliverables, and commercial pricing directly from the quotation.
          </p>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Approved Quotation *</label>
            <select
              required
              value={selectedQuoteId}
              onChange={(e) => {
                setSelectedQuoteId(e.target.value);
                const q = approvedQuotations.find((item) => item.id === e.target.value);
                if (q) setFromQuoteTitle(`Service Contract - ${q.accountName}`);
              }}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-purple-500"
            >
              <option value="">-- Choose Approved Quotation --</option>
              {approvedQuotations.map((q) => (
                <option key={q.id} value={q.id}>
                  {q.quotationNumber} ({q.revisionNumber}) - {q.accountName} ({formatCurrency(q.totalAmount)})
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Contract Title</label>
            <input
              type="text"
              required
              value={fromQuoteTitle}
              onChange={(e) => setFromQuoteTitle(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-purple-500"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Contract Template</label>
            <select
              value={fromQuoteTemplateId}
              onChange={(e) => setFromQuoteTemplateId(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-purple-500"
            >
              <option value="">-- Standard Service Template --</option>
              {templates.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name} ({t.code})
                </option>
              ))}
            </select>
          </div>

          <div className="flex justify-end gap-3 pt-3 border-t border-slate-200">
            <button
              type="button"
              onClick={() => setIsFromQuoteOpen(false)}
              className="px-4 py-2 border border-slate-300 rounded-lg text-sm font-medium text-slate-700 hover:bg-slate-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg text-sm font-medium shadow-sm"
            >
              Generate Agreement
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
};
