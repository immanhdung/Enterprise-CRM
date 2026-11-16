import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Plus,
  Search,
  FileSpreadsheet,
  Download,
  Eye,
  Trash2,
  RefreshCw,
} from 'lucide-react';
import { quotationService, type CreateQuotationRequest, type QuotationItemInput } from '../../services/quotationService';
import { accountService } from '../../services/accountService';
import { serviceCatalogService } from '../../services/serviceCatalogService';
import type { Quotation, ServiceItem, PriceList } from '../../types/billing';
import type { AccountDto, PagedResult } from '../../types/crm';
import { Modal } from '../../components/common/Modal';

export const QuotationsPage: React.FC = () => {
  const navigate = useNavigate();

  // Data states
  const [data, setData] = useState<PagedResult<Quotation>>({
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 10,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  });
  const [accounts, setAccounts] = useState<AccountDto[]>([]);
  const [services, setServices] = useState<ServiceItem[]>([]);
  const [priceLists, setPriceLists] = useState<PriceList[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  // Filters
  const [search, setSearch] = useState('');
  const [selectedStatus, setSelectedStatus] = useState('');
  const [pageNumber, setPageNumber] = useState(1);

  // Create Modal state
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [accountId, setAccountId] = useState('');
  const [priceListId, setPriceListId] = useState('');
  const [validUntil, setValidUntil] = useState(
    new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
  );
  const [terms, setTerms] = useState('Payment within 30 days of deliverable acceptance.');
  const [notes, setNotes] = useState('');
  const [items, setItems] = useState<QuotationItemInput[]>([]);

  const loadData = useCallback(async () => {
    setIsLoading(true);
    try {
      const res = await quotationService.getQuotations({
        pageNumber,
        pageSize: 10,
        search: search || undefined,
        status: selectedStatus || undefined,
      });
      if (res.data) setData(res.data);
    } catch (err) {
      console.error('Failed to load quotations', err);
    } finally {
      setIsLoading(false);
    }
  }, [pageNumber, search, selectedStatus]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // Load supporting lookup data
  useEffect(() => {
    const loadLookups = async () => {
      try {
        const [accRes, svcRes, plRes] = await Promise.all([
          accountService.getAccounts({ pageSize: 100 }),
          serviceCatalogService.getServices({ isActive: true }),
          serviceCatalogService.getPriceLists(false),
        ]);
        if (accRes.data?.items) setAccounts(accRes.data.items);
        if (svcRes.data) setServices(svcRes.data);
        if (plRes.data) setPriceLists(plRes.data);
      } catch (err) {
        console.error('Failed to load lookup items', err);
      }
    };
    loadLookups();
  }, []);

  const handleOpenCreate = () => {
    setAccountId(accounts.length > 0 ? accounts[0].id : '');
    setPriceListId('');
    setValidUntil(new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]);
    setTerms('Standard Net 30 payment terms upon deliverable acceptance.');
    setNotes('');

    if (services.length > 0) {
      setItems([
        {
          serviceId: services[0].id,
          serviceCode: services[0].code,
          ServiceName: services[0].name,
          unit: services[0].unit,
          quantity: 1,
          unitPrice: services[0].basePrice,
          discountRate: 0,
          vatRate: services[0].vatRate || 8.0,
          notes: '',
        },
      ]);
    } else {
      setItems([]);
    }
    setIsCreateOpen(true);
  };

  const handleAddItem = () => {
    if (services.length === 0) return;
    setItems([
      ...items,
      {
        serviceId: services[0].id,
        serviceCode: services[0].code,
        ServiceName: services[0].name,
        unit: services[0].unit,
        quantity: 1,
        unitPrice: services[0].basePrice,
        discountRate: 0,
        vatRate: services[0].vatRate || 8.0,
        notes: '',
      },
    ]);
  };

  const handleRemoveItem = (index: number) => {
    setItems(items.filter((_, i) => i !== index));
  };

  const handleItemFieldChange = (index: number, field: keyof QuotationItemInput, value: any) => {
    const updated = [...items];
    updated[index] = { ...updated[index], [field]: value };

    if (field === 'serviceId') {
      const svc = services.find((s) => s.id === value);
      if (svc) {
        updated[index].serviceCode = svc.code;
        updated[index].ServiceName = svc.name;
        updated[index].unit = svc.unit;
        updated[index].unitPrice = svc.basePrice;
        updated[index].vatRate = svc.vatRate || 8.0;
      }
    }
    setItems(updated);
  };

  // Live Summary Math
  const subtotal = items.reduce((sum, it) => sum + it.quantity * it.unitPrice, 0);
  const discountTotal = items.reduce(
    (sum, it) => sum + it.quantity * it.unitPrice * (it.discountRate / 100),
    0
  );
  const vatTotal = items.reduce((sum, it) => {
    const net = it.quantity * it.unitPrice * (1 - it.discountRate / 100);
    return sum + net * (it.vatRate / 100);
  }, 0);
  const grandTotal = subtotal - discountTotal + vatTotal;

  const handleCreateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!accountId) {
      alert('Please select an Account');
      return;
    }
    if (items.length === 0) {
      alert('Please include at least one service item in the quotation.');
      return;
    }

    const payload: CreateQuotationRequest = {
      accountId,
      priceListId: priceListId || undefined,
      validUntil: new Date(validUntil).toISOString(),
      termsAndConditions: terms,
      notes,
      items,
    };

    try {
      const res = await quotationService.createQuotation(payload);
      setIsCreateOpen(false);
      if (res.data) {
        navigate(`/quotations/${res.data.id}`);
      } else {
        loadData();
      }
    } catch (err) {
      console.error('Failed to create quotation', err);
    }
  };

  const handleDownloadPdf = async (e: React.MouseEvent, quote: Quotation) => {
    e.stopPropagation();
    try {
      await quotationService.downloadPdf(quote.id, quote.quotationNumber, quote.revisionNumber);
    } catch (err) {
      console.error('Failed to download PDF', err);
    }
  };

  const handleDelete = async (e: React.MouseEvent, id: string) => {
    e.stopPropagation();
    if (!confirm('Are you sure you want to delete this quotation?')) return;
    try {
      await quotationService.deleteQuotation(id);
      loadData();
    } catch (err) {
      console.error('Failed to delete quotation', err);
    }
  };

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Draft':
        return <span className="px-2 py-0.5 rounded text-xs font-medium bg-slate-100 text-slate-700">Draft</span>;
      case 'Submitted':
        return <span className="px-2 py-0.5 rounded text-xs font-medium bg-amber-100 text-amber-800">Submitted</span>;
      case 'Approved':
        return <span className="px-2 py-0.5 rounded text-xs font-medium bg-emerald-100 text-emerald-800">Approved</span>;
      case 'Rejected':
        return <span className="px-2 py-0.5 rounded text-xs font-medium bg-rose-100 text-rose-800">Rejected</span>;
      case 'Sent':
        return <span className="px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">Sent to Client</span>;
      case 'Accepted':
        return <span className="px-2 py-0.5 rounded text-xs font-medium bg-purple-100 text-purple-800">Accepted</span>;
      default:
        return <span className="px-2 py-0.5 rounded text-xs font-medium bg-slate-100 text-slate-700">{status}</span>;
    }
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 flex items-center gap-2">
            <FileSpreadsheet className="w-7 h-7 text-blue-600" />
            Quotations & CPQ Engine
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            Build formal service proposals with automatic VAT & discount math, multi-revision versions, and PDF generation.
          </p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium text-sm rounded-lg shadow-sm transition-colors"
        >
          <Plus className="w-4 h-4" />
          Create Quotation
        </button>
      </div>

      {/* Filters Bar */}
      <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex flex-wrap gap-4 items-center justify-between">
        <div className="flex flex-wrap items-center gap-3 flex-1 min-w-[280px]">
          <div className="relative flex-1 max-w-md">
            <Search className="w-4 h-4 absolute left-3 top-3 text-slate-400" />
            <input
              type="text"
              placeholder="Search by quote number, account name..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-9 pr-4 py-2 border border-slate-300 rounded-lg text-sm focus:outline-hidden focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <select
            value={selectedStatus}
            onChange={(e) => setSelectedStatus(e.target.value)}
            className="px-3 py-2 border border-slate-300 rounded-lg text-sm bg-white text-slate-700 focus:outline-hidden focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Statuses</option>
            <option value="Draft">Draft</option>
            <option value="Submitted">Submitted</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
            <option value="Sent">Sent</option>
            <option value="Accepted">Accepted</option>
          </select>
        </div>

        <button
          onClick={loadData}
          className="p-2 border border-slate-300 rounded-lg hover:bg-slate-50 text-slate-600 transition-colors"
          title="Refresh quotations"
        >
          <RefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin' : ''}`} />
        </button>
      </div>

      {/* Table */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm text-slate-700">
            <thead className="bg-slate-50 border-b border-slate-200 text-xs uppercase font-semibold text-slate-500 tracking-wider">
              <tr>
                <th className="px-6 py-3.5">Quotation #</th>
                <th className="px-6 py-3.5">Account / Client</th>
                <th className="px-6 py-3.5">Revision</th>
                <th className="px-6 py-3.5 text-right">Total Amount</th>
                <th className="px-6 py-3.5">Valid Until</th>
                <th className="px-6 py-3.5 text-center">Status</th>
                <th className="px-6 py-3.5 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-200">
              {isLoading && data.items.length === 0 ? (
                <tr>
                  <td colSpan={7} className="px-6 py-10 text-center text-slate-400">
                    Loading quotations...
                  </td>
                </tr>
              ) : data.items.length === 0 ? (
                <tr>
                  <td colSpan={7} className="px-6 py-12 text-center text-slate-400">
                    No quotations found matching your filter criteria.
                  </td>
                </tr>
              ) : (
                data.items.map((q) => (
                  <tr
                    key={q.id}
                    onClick={() => navigate(`/quotations/${q.id}`)}
                    className="hover:bg-slate-50/75 cursor-pointer transition-colors"
                  >
                    <td className="px-6 py-4 font-mono font-medium text-blue-600">
                      {q.quotationNumber}
                    </td>
                    <td className="px-6 py-4 font-medium text-slate-900">
                      {q.accountName}
                    </td>
                    <td className="px-6 py-4">
                      <span className="font-mono text-xs px-2 py-0.5 bg-slate-100 text-slate-700 rounded font-semibold">
                        {q.revisionNumber}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-right font-bold text-slate-900">
                      {formatCurrency(q.totalAmount)}
                    </td>
                    <td className="px-6 py-4 text-slate-600">
                      {new Date(q.validUntil).toLocaleDateString()}
                    </td>
                    <td className="px-6 py-4 text-center">
                      {getStatusBadge(q.status)}
                    </td>
                    <td className="px-6 py-4 text-right space-x-2">
                      <button
                        onClick={(e) => handleDownloadPdf(e, q)}
                        title="Download PDF"
                        className="p-1.5 text-slate-400 hover:text-blue-600 transition-colors"
                      >
                        <Download className="w-4 h-4" />
                      </button>
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          navigate(`/quotations/${q.id}`);
                        }}
                        title="View Details"
                        className="p-1.5 text-slate-400 hover:text-indigo-600 transition-colors"
                      >
                        <Eye className="w-4 h-4" />
                      </button>
                      {q.status === 'Draft' && (
                        <button
                          onClick={(e) => handleDelete(e, q.id)}
                          title="Delete Quotation"
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

      {/* Create Quotation Modal */}
      <Modal
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        title="Create New Service Quotation"
      >
        <form onSubmit={handleCreateSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Target Account *</label>
              <select
                required
                value={accountId}
                onChange={(e) => setAccountId(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
              >
                <option value="">-- Select Client Account --</option>
                {accounts.map((a) => (
                  <option key={a.id} value={a.id}>
                    {a.name} ({a.taxId || 'No Tax ID'})
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Rate Card / Price List</label>
              <select
                value={priceListId}
                onChange={(e) => setPriceListId(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
              >
                <option value="">-- Standard Base Price --</option>
                {priceLists.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.name} ({p.currency})
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Valid Until *</label>
              <input
                type="date"
                required
                value={validUntil}
                onChange={(e) => setValidUntil(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Internal Reference Notes</label>
              <input
                type="text"
                placeholder="e.g. Q1 Expansion scope"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>

          {/* Line Items Table Builder */}
          <div className="space-y-3 pt-2 border-t border-slate-200">
            <div className="flex justify-between items-center">
              <h4 className="text-xs font-semibold uppercase text-slate-700 tracking-wider">
                Services & Billable Line Items
              </h4>
              <button
                type="button"
                onClick={handleAddItem}
                className="text-xs text-blue-600 hover:text-blue-800 font-medium flex items-center gap-1"
              >
                <Plus className="w-3.5 h-3.5" /> Add Service Line
              </button>
            </div>

            <div className="space-y-2 max-h-56 overflow-y-auto pr-1">
              {items.map((item, idx) => (
                <div key={idx} className="bg-slate-50 p-2.5 rounded-lg border border-slate-200 space-y-2">
                  <div className="flex items-center gap-2">
                    <select
                      value={item.serviceId}
                      onChange={(e) => handleItemFieldChange(idx, 'serviceId', e.target.value)}
                      className="flex-2 px-2 py-1.5 border border-slate-300 rounded text-xs bg-white"
                    >
                      {services.map((s) => (
                        <option key={s.id} value={s.id}>
                          {s.code} - {s.name}
                        </option>
                      ))}
                    </select>

                    <input
                      type="text"
                      title="Unit"
                      placeholder="Unit"
                      value={item.unit}
                      onChange={(e) => handleItemFieldChange(idx, 'unit', e.target.value)}
                      className="w-16 px-2 py-1.5 border border-slate-300 rounded text-xs bg-white text-center"
                    />

                    <input
                      type="number"
                      min="1"
                      title="Quantity"
                      placeholder="Qty"
                      value={item.quantity}
                      onChange={(e) => handleItemFieldChange(idx, 'quantity', Number(e.target.value))}
                      className="w-16 px-2 py-1.5 border border-slate-300 rounded text-xs bg-white text-center"
                    />

                    <input
                      type="number"
                      step="100000"
                      title="Unit Price"
                      placeholder="Unit Price"
                      value={item.unitPrice}
                      onChange={(e) => handleItemFieldChange(idx, 'unitPrice', Number(e.target.value))}
                      className="w-28 px-2 py-1.5 border border-slate-300 rounded text-xs bg-white text-right"
                    />

                    <input
                      type="number"
                      min="0"
                      max="100"
                      title="Discount %"
                      placeholder="Disc%"
                      value={item.discountRate}
                      onChange={(e) => handleItemFieldChange(idx, 'discountRate', Number(e.target.value))}
                      className="w-16 px-2 py-1.5 border border-slate-300 rounded text-xs bg-white text-center"
                    />

                    <input
                      type="number"
                      min="0"
                      max="100"
                      title="VAT %"
                      placeholder="VAT%"
                      value={item.vatRate}
                      onChange={(e) => handleItemFieldChange(idx, 'vatRate', Number(e.target.value))}
                      className="w-16 px-2 py-1.5 border border-slate-300 rounded text-xs bg-white text-center"
                    />

                    <button
                      type="button"
                      onClick={() => handleRemoveItem(idx)}
                      className="p-1 text-slate-400 hover:text-rose-500"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                </div>
              ))}
            </div>

            {/* Calculations Breakdown */}
            <div className="bg-slate-100 p-3 rounded-lg border border-slate-200 text-xs space-y-1.5">
              <div className="flex justify-between text-slate-600">
                <span>Subtotal:</span>
                <span className="font-mono">{formatCurrency(subtotal)}</span>
              </div>
              <div className="flex justify-between text-rose-600">
                <span>Total Discount:</span>
                <span className="font-mono">-{formatCurrency(discountTotal)}</span>
              </div>
              <div className="flex justify-between text-slate-600">
                <span>VAT / Tax:</span>
                <span className="font-mono">+{formatCurrency(vatTotal)}</span>
              </div>
              <div className="flex justify-between text-slate-900 font-bold text-sm pt-1 border-t border-slate-300">
                <span>Total Proposal Value:</span>
                <span className="font-mono text-blue-600">{formatCurrency(grandTotal)}</span>
              </div>
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Terms & Conditions</label>
            <textarea
              rows={2}
              value={terms}
              onChange={(e) => setTerms(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
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
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium shadow-sm"
            >
              Create Draft Quotation
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
};
