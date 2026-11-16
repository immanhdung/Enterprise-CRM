import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  Download,
  Send,
  RefreshCw,
  Plus,
  RotateCw,
  FileText,
  Clock,
  Building,
  Calendar,
  ExternalLink,
  ShieldCheck,
  FileSpreadsheet,
} from 'lucide-react';
import { contractService } from '../../services/contractService';
import { eSignatureService } from '../../services/eSignatureService';
import type { Contract, ContractStatus } from '../../types/billing';
import { Modal } from '../../components/common/Modal';

export const ContractDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [contract, setContract] = useState<Contract | null>(null);
  const [renderedContent, setRenderedContent] = useState<string>('');
  const [activeTab, setActiveTab] = useState<'scope' | 'content' | 'appendices' | 'esign' | 'history'>('scope');
  const [isLoading, setIsLoading] = useState(true);

  // Modals state
  const [isEsignOpen, setIsEsignOpen] = useState(false);
  const [signerName, setSignerName] = useState('');
  const [signerEmail, setSignerEmail] = useState('');
  const [isSendingEsign, setIsSendingEsign] = useState(false);

  const [isAppendixOpen, setIsAppendixOpen] = useState(false);
  const [appendixTitle, setAppendixTitle] = useState('');
  const [appendixDesc, setAppendixDesc] = useState('');
  const [appendixAdjustedValue, setAppendixAdjustedValue] = useState(0);
  const [appendixDate, setAppendixDate] = useState(new Date().toISOString().split('T')[0]);

  const [isRenewOpen, setIsRenewOpen] = useState(false);
  const [renewStartDate, setRenewStartDate] = useState('');
  const [renewEndDate, setRenewEndDate] = useState('');
  const [renewTotalValue, setRenewTotalValue] = useState<number | undefined>(undefined);
  const [renewNotes, setRenewNotes] = useState('Annual contract renewal with current scope.');

  const loadContract = useCallback(async () => {
    if (!id) return;
    setIsLoading(true);
    try {
      const [res, contentRes] = await Promise.all([
        contractService.getContractById(id),
        contractService.getContractContent(id).catch(() => ({ data: '' })),
      ]);
      if (res.data) {
        setContract(res.data);
        setSignerName(res.data.contactName || res.data.accountName);
        setSignerEmail(res.data.signedByClientEmail || '');
        setRenewTotalValue(res.data.totalValue);

        const currentEnd = new Date(res.data.endDate);
        const nextStart = new Date(currentEnd.getTime() + 24 * 60 * 60 * 1000);
        const nextEnd = new Date(nextStart.getTime() + 365 * 24 * 60 * 60 * 1000);
        setRenewStartDate(nextStart.toISOString().split('T')[0]);
        setRenewEndDate(nextEnd.toISOString().split('T')[0]);
      }
      if (contentRes.data) setRenderedContent(contentRes.data);
    } catch (err) {
      console.error('Failed to load contract details', err);
    } finally {
      setIsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    loadContract();
  }, [loadContract]);

  const handleDownloadPdf = async () => {
    if (!contract) return;
    try {
      await contractService.downloadPdf(contract.id, contract.contractNumber);
    } catch (err) {
      console.error('Failed to download PDF', err);
    }
  };

  const handleSendEsign = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!contract || !signerEmail || !signerName) return;
    setIsSendingEsign(true);
    try {
      const res = await eSignatureService.sendSignatureRequest({
        contractId: contract.id,
        signerName,
        signerEmail,
      });
      setIsEsignOpen(false);
      loadContract();
      if (res.data?.signingUrl) {
        alert(`E-Signature request dispatched via ${res.data.provider}.\nSigner test portal URL: ${res.data.signingUrl}`);
      }
    } catch (err) {
      console.error('Failed to dispatch e-signature', err);
    } finally {
      setIsSendingEsign(false);
    }
  };

  const handleSyncStatus = async (sigId: string) => {
    try {
      await eSignatureService.syncStatus(sigId);
      loadContract();
    } catch (err) {
      console.error('Failed to sync signature status', err);
    }
  };

  const handleAddAppendix = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!contract) return;
    try {
      await contractService.addAppendix(contract.id, {
        title: appendixTitle,
        description: appendixDesc,
        adjustedValue: appendixAdjustedValue,
        effectiveDate: new Date(appendixDate).toISOString(),
        status: 'Active',
      });
      setIsAppendixOpen(false);
      setAppendixTitle('');
      setAppendixDesc('');
      setAppendixAdjustedValue(0);
      loadContract();
    } catch (err) {
      console.error('Failed to add appendix', err);
    }
  };

  const handleRenewSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!contract) return;
    try {
      const res = await contractService.renewContract(contract.id, {
        newStartDate: new Date(renewStartDate).toISOString(),
        newEndDate: new Date(renewEndDate).toISOString(),
        newTotalValue: renewTotalValue,
        renewalNotes: renewNotes,
      });
      setIsRenewOpen(false);
      if (res.data) {
        navigate(`/contracts/${res.data.id}`);
      } else {
        loadContract();
      }
    } catch (err) {
      console.error('Failed to renew contract', err);
    }
  };

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const getStatusBadge = (status: ContractStatus) => {
    switch (status) {
      case 'Draft':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-slate-100 text-slate-700">Draft</span>;
      case 'Approved':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-blue-100 text-blue-800">Approved</span>;
      case 'PendingSignature':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-amber-100 text-amber-800">Pending Signature</span>;
      case 'Signed':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-emerald-100 text-emerald-800">Digitally Signed</span>;
      case 'Active':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-emerald-100 text-emerald-800">Active</span>;
      case 'Renewed':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-purple-100 text-purple-800">Renewed</span>;
      case 'Expired':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-rose-100 text-rose-800">Expired</span>;
      default:
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-slate-100 text-slate-700">{status}</span>;
    }
  };

  if (isLoading || !contract) {
    return <div className="p-12 text-center text-slate-500">Loading contract details...</div>;
  }

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Back button */}
      <div className="flex items-center justify-between">
        <button
          onClick={() => navigate('/contracts')}
          className="flex items-center gap-2 text-sm text-slate-600 hover:text-slate-900 font-medium"
        >
          <ArrowLeft className="w-4 h-4" /> Back to Contracts
        </button>

        {contract.quotationNumber && (
          <button
            onClick={() => navigate(`/quotations/${contract.quotationId}`)}
            className="flex items-center gap-1.5 text-xs text-blue-600 hover:text-blue-800 font-medium bg-blue-50 px-2.5 py-1 rounded-lg"
          >
            <FileSpreadsheet className="w-3.5 h-3.5" /> Source Quotation: {contract.quotationNumber}
          </button>
        )}
      </div>

      {/* Main Header Card */}
      <div className="bg-white border border-slate-200 rounded-xl p-6 shadow-xs flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
        <div className="space-y-2">
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold text-slate-900 font-mono">
              {contract.contractNumber}
            </h1>
            {getStatusBadge(contract.status)}
            {contract.esignStatus === 'Signed' && (
              <span className="flex items-center gap-1 text-xs font-semibold px-2 py-0.5 rounded bg-emerald-50 text-emerald-700 border border-emerald-200">
                <ShieldCheck className="w-3.5 h-3.5" /> E-Signed
              </span>
            )}
          </div>

          <div className="font-semibold text-slate-800 text-base">{contract.title}</div>

          <div className="flex flex-wrap items-center gap-4 text-xs text-slate-500">
            <span className="flex items-center gap-1.5 font-medium text-slate-700">
              <Building className="w-4 h-4 text-slate-400" />
              {contract.accountName}
            </span>
            <span>•</span>
            <span className="flex items-center gap-1.5">
              <Calendar className="w-4 h-4 text-slate-400" />
              Period: {new Date(contract.startDate).toLocaleDateString()} -{' '}
              {new Date(contract.endDate).toLocaleDateString()}
            </span>
            <span>•</span>
            <span className="font-mono font-bold text-indigo-600 text-sm">
              {formatCurrency(contract.totalValue)}
            </span>
          </div>
        </div>

        {/* Action Toolbar */}
        <div className="flex flex-wrap items-center gap-2">
          <button
            onClick={handleDownloadPdf}
            className="flex items-center gap-1.5 px-3.5 py-2 border border-slate-300 hover:bg-slate-50 text-slate-700 text-xs font-semibold rounded-lg"
          >
            <Download className="w-4 h-4 text-indigo-600" /> PDF Agreement
          </button>

          {contract.status !== 'Signed' && (
            <button
              onClick={() => setIsEsignOpen(true)}
              className="flex items-center gap-1.5 px-3.5 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold rounded-lg shadow-sm"
            >
              <Send className="w-4 h-4" /> Send E-Signature
            </button>
          )}

          <button
            onClick={() => setIsAppendixOpen(true)}
            className="flex items-center gap-1.5 px-3.5 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs font-semibold rounded-lg"
          >
            <Plus className="w-4 h-4" /> Add Appendix
          </button>

          <button
            onClick={() => setIsRenewOpen(true)}
            className="flex items-center gap-1.5 px-3.5 py-2 bg-purple-600 hover:bg-purple-700 text-white text-xs font-semibold rounded-lg shadow-sm"
          >
            <RotateCw className="w-4 h-4" /> Renew Contract
          </button>
        </div>
      </div>

      {/* Navigation Tabs */}
      <div className="border-b border-slate-200 flex gap-6 text-sm font-medium">
        <button
          onClick={() => setActiveTab('scope')}
          className={`pb-3 border-b-2 transition-colors ${
            activeTab === 'scope'
              ? 'border-indigo-600 text-indigo-600'
              : 'border-transparent text-slate-500 hover:text-slate-800'
          }`}
        >
          Scope & Billable Items ({contract.items.length})
        </button>

        <button
          onClick={() => setActiveTab('content')}
          className={`pb-3 border-b-2 transition-colors ${
            activeTab === 'content'
              ? 'border-indigo-600 text-indigo-600'
              : 'border-transparent text-slate-500 hover:text-slate-800'
          }`}
        >
          Rendered Legal Agreement
        </button>

        <button
          onClick={() => setActiveTab('appendices')}
          className={`pb-3 border-b-2 transition-colors ${
            activeTab === 'appendices'
              ? 'border-indigo-600 text-indigo-600'
              : 'border-transparent text-slate-500 hover:text-slate-800'
          }`}
        >
          Contract Appendices ({contract.appendices.length})
        </button>

        <button
          onClick={() => setActiveTab('esign')}
          className={`pb-3 border-b-2 transition-colors ${
            activeTab === 'esign'
              ? 'border-indigo-600 text-indigo-600'
              : 'border-transparent text-slate-500 hover:text-slate-800'
          }`}
        >
          E-Signature Requests ({contract.signatureRequests.length})
        </button>

        <button
          onClick={() => setActiveTab('history')}
          className={`pb-3 border-b-2 transition-colors ${
            activeTab === 'history'
              ? 'border-indigo-600 text-indigo-600'
              : 'border-transparent text-slate-500 hover:text-slate-800'
          }`}
        >
          Audit History ({contract.histories.length})
        </button>
      </div>

      {/* Tab Panels */}
      {activeTab === 'scope' && (
        <div className="space-y-6">
          <div className="bg-white border border-slate-200 rounded-xl overflow-hidden shadow-xs">
            <div className="p-4 bg-slate-50 border-b border-slate-200 font-semibold text-sm text-slate-800">
              Contract Deliverables & Items
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs text-slate-700">
                <thead className="bg-slate-50/75 border-b border-slate-200 uppercase font-semibold text-slate-500">
                  <tr>
                    <th className="px-4 py-3">Deliverable Service</th>
                    <th className="px-3 py-3 text-center">Qty</th>
                    <th className="px-4 py-3 text-right">Unit Price</th>
                    <th className="px-4 py-3 text-right">Line Total</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-200">
                  {contract.items.map((it) => (
                    <tr key={it.id}>
                      <td className="px-4 py-3">
                        <div className="font-semibold text-slate-900">{it.serviceName}</div>
                        {it.serviceCode && <div className="font-mono text-[11px] text-slate-400">{it.serviceCode}</div>}
                        {it.notes && <div className="text-[11px] italic text-slate-500">{it.notes}</div>}
                      </td>
                      <td className="px-3 py-3 text-center font-semibold text-slate-800">{it.quantity}</td>
                      <td className="px-4 py-3 text-right font-mono">{formatCurrency(it.unitPrice)}</td>
                      <td className="px-4 py-3 text-right font-mono font-bold text-slate-900">
                        {formatCurrency(it.lineTotal)}
                      </td>
                    </tr>
                  ))}
                  {contract.items.length === 0 && (
                    <tr>
                      <td colSpan={4} className="px-4 py-6 text-center text-slate-400">
                        No itemized lines specified for this contract.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="bg-white border border-slate-200 rounded-xl p-5 shadow-xs space-y-2">
              <h3 className="font-semibold text-sm text-slate-800">Scope of Work</h3>
              <p className="text-xs text-slate-600 whitespace-pre-wrap leading-relaxed">
                {contract.scopeOfWork || 'Standard deliverables as per quotation.'}
              </p>
            </div>

            <div className="bg-white border border-slate-200 rounded-xl p-5 shadow-xs space-y-2">
              <h3 className="font-semibold text-sm text-slate-800">Payment Terms</h3>
              <p className="text-xs text-slate-600 whitespace-pre-wrap leading-relaxed">
                {contract.paymentTerms || 'Net 30 days.'}
              </p>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'content' && (
        <div className="bg-white border border-slate-200 rounded-xl p-6 shadow-xs space-y-4">
          <div className="flex justify-between items-center border-b border-slate-100 pb-3">
            <h3 className="font-semibold text-sm text-slate-800 flex items-center gap-2">
              <FileText className="w-4 h-4 text-indigo-600" />
              Dynamic Merged Agreement Document
            </h3>
            <span className="text-xs text-slate-400">Merged with Account & Quotation metadata</span>
          </div>

          <div className="prose max-w-none text-xs text-slate-700 whitespace-pre-wrap bg-slate-50 p-6 rounded-lg border border-slate-200 font-serif leading-relaxed">
            {renderedContent || 'No template assigned to render.'}
          </div>
        </div>
      )}

      {activeTab === 'appendices' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h3 className="font-semibold text-sm text-slate-800">
              Contract Appendices & Scope Amendments
            </h3>
            <button
              onClick={() => setIsAppendixOpen(true)}
              className="flex items-center gap-1.5 px-3 py-1.5 bg-indigo-600 text-white text-xs font-semibold rounded-lg"
            >
              <Plus className="w-3.5 h-3.5" /> New Appendix
            </button>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {contract.appendices.length === 0 ? (
              <div className="col-span-full py-8 text-center text-xs text-slate-400 bg-white border border-slate-200 rounded-xl">
                No appendices added yet. Add an appendix to adjust contract value or scope of work.
              </div>
            ) : (
              contract.appendices.map((app) => (
                <div key={app.id} className="bg-white border border-slate-200 rounded-xl p-4 shadow-xs space-y-2">
                  <div className="flex justify-between items-center">
                    <span className="font-mono text-xs font-bold text-indigo-600 px-2 py-0.5 bg-indigo-50 rounded">
                      {app.appendixNumber}
                    </span>
                    <span className="text-xs text-slate-400">
                      Effective: {new Date(app.effectiveDate).toLocaleDateString()}
                    </span>
                  </div>

                  <h4 className="font-semibold text-sm text-slate-900">{app.title}</h4>
                  {app.description && <p className="text-xs text-slate-600">{app.description}</p>}

                  <div className="pt-2 border-t border-slate-100 flex justify-between items-center text-xs">
                    <span className="text-slate-500">Value Adjustment:</span>
                    <span
                      className={`font-mono font-bold ${
                        app.adjustedValue >= 0 ? 'text-emerald-600' : 'text-rose-600'
                      }`}
                    >
                      {app.adjustedValue >= 0 ? '+' : ''}
                      {formatCurrency(app.adjustedValue)}
                    </span>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      )}

      {activeTab === 'esign' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h3 className="font-semibold text-sm text-slate-800">
              Electronic Signature Requests & Status Synchronization
            </h3>
            <button
              onClick={() => setIsEsignOpen(true)}
              className="flex items-center gap-1.5 px-3 py-1.5 bg-indigo-600 text-white text-xs font-semibold rounded-lg"
            >
              <Send className="w-3.5 h-3.5" /> Dispatch Signature
            </button>
          </div>

          <div className="bg-white border border-slate-200 rounded-xl overflow-hidden shadow-xs">
            <table className="w-full text-left text-xs text-slate-700">
              <thead className="bg-slate-50 border-b border-slate-200 uppercase font-semibold text-slate-500">
                <tr>
                  <th className="px-4 py-3">Signer Name & Email</th>
                  <th className="px-4 py-3">Provider</th>
                  <th className="px-4 py-3">Envelope ID</th>
                  <th className="px-4 py-3 text-center">Status</th>
                  <th className="px-4 py-3">Sent At</th>
                  <th className="px-4 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200">
                {contract.signatureRequests.map((req) => (
                  <tr key={req.id}>
                    <td className="px-4 py-3">
                      <div className="font-semibold text-slate-900">{req.signerName}</div>
                      <div className="text-slate-500 text-[11px]">{req.signerEmail}</div>
                    </td>
                    <td className="px-4 py-3 font-medium text-slate-700">{req.provider}</td>
                    <td className="px-4 py-3 font-mono text-[11px] text-slate-500">
                      {req.externalEnvelopeId || 'Pending'}
                    </td>
                    <td className="px-4 py-3 text-center">
                      <span
                        className={`px-2 py-0.5 rounded text-[11px] font-semibold ${
                          req.status === 'Signed'
                            ? 'bg-emerald-100 text-emerald-800'
                            : 'bg-amber-100 text-amber-800'
                        }`}
                      >
                        {req.status}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-slate-500">
                      {new Date(req.sentAt).toLocaleString()}
                    </td>
                    <td className="px-4 py-3 text-right space-x-2">
                      {req.signingUrl && (
                        <a
                          href={req.signingUrl}
                          target="_blank"
                          rel="noreferrer"
                          className="inline-flex items-center gap-1 text-indigo-600 hover:text-indigo-800 font-medium"
                        >
                          Sign Portal <ExternalLink className="w-3 h-3" />
                        </a>
                      )}
                      <button
                        onClick={() => handleSyncStatus(req.id)}
                        title="Sync Status with Provider"
                        className="p-1 text-slate-400 hover:text-indigo-600"
                      >
                        <RefreshCw className="w-3.5 h-3.5" />
                      </button>
                    </td>
                  </tr>
                ))}
                {contract.signatureRequests.length === 0 && (
                  <tr>
                    <td colSpan={6} className="px-4 py-6 text-center text-slate-400">
                      No electronic signature sessions initiated for this contract yet.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'history' && (
        <div className="bg-white border border-slate-200 rounded-xl p-6 shadow-xs space-y-4">
          <h3 className="font-semibold text-sm text-slate-800 flex items-center gap-2">
            <Clock className="w-4 h-4 text-slate-400" />
            Contract Audit Trail & Status History
          </h3>

          <div className="space-y-3">
            {contract.histories.map((h) => (
              <div key={h.id} className="border-l-2 border-indigo-500 pl-4 py-1.5 space-y-1 text-xs">
                <div className="flex justify-between items-center">
                  <span className="font-semibold text-slate-800">{h.action}</span>
                  <span className="text-[11px] text-slate-400">
                    {new Date(h.timestamp).toLocaleString()}
                  </span>
                </div>
                <div className="text-slate-500 text-[11px]">
                  Actor: <span className="text-slate-700">{h.actorName}</span>
                  {h.oldStatus && h.newStatus && (
                    <span className="ml-2 font-mono">
                      ({h.oldStatus} → {h.newStatus})
                    </span>
                  )}
                </div>
                {h.notes && <div className="text-slate-600 italic mt-0.5">{h.notes}</div>}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Send E-Sign Modal */}
      <Modal isOpen={isEsignOpen} onClose={() => setIsEsignOpen(false)} title="Initiate Electronic Signature Request">
        <form onSubmit={handleSendEsign} className="space-y-4">
          <p className="text-xs text-slate-600">
            Dispatches a formal digital signing package via configured provider (DocuSign/AdobeSign/Mock Provider) to the authorized signatory.
          </p>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Signatory Legal Name *</label>
            <input
              type="text"
              required
              placeholder="e.g. Nguyen Van A"
              value={signerName}
              onChange={(e) => setSignerName(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Signatory Email *</label>
            <input
              type="email"
              required
              placeholder="signer@clientcorp.vn"
              value={signerEmail}
              onChange={(e) => setSignerEmail(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
            />
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => setIsEsignOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSendingEsign}
              className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-semibold disabled:opacity-50"
            >
              {isSendingEsign ? 'Dispatching...' : 'Send for Signature'}
            </button>
          </div>
        </form>
      </Modal>

      {/* Add Appendix Modal */}
      <Modal isOpen={isAppendixOpen} onClose={() => setIsAppendixOpen(false)} title="Add Contract Appendix / Variation">
        <form onSubmit={handleAddAppendix} className="space-y-4">
          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Appendix Title *</label>
            <input
              type="text"
              required
              placeholder="e.g. Scope Addendum - Additional Cloud Database Nodes"
              value={appendixTitle}
              onChange={(e) => setAppendixTitle(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Adjusted Value (VND) *</label>
              <input
                type="number"
                required
                step="500000"
                placeholder="Positive or negative change"
                value={appendixAdjustedValue}
                onChange={(e) => setAppendixAdjustedValue(Number(e.target.value))}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
              />
            </div>

            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Effective Date *</label>
              <input
                type="date"
                required
                value={appendixDate}
                onChange={(e) => setAppendixDate(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Scope Variation Details</label>
            <textarea
              rows={3}
              value={appendixDesc}
              onChange={(e) => setAppendixDesc(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
            />
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => setIsAppendixOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
              Cancel
            </button>
            <button type="submit" className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-semibold">
              Save Appendix
            </button>
          </div>
        </form>
      </Modal>

      {/* Renew Contract Modal */}
      <Modal isOpen={isRenewOpen} onClose={() => setIsRenewOpen(false)} title="Renew Service Contract">
        <form onSubmit={handleRenewSubmit} className="space-y-4">
          <p className="text-xs text-slate-600">
            Generates a new sequential contract linked as a renewal to this agreement, and updates the current contract status to "Renewed".
          </p>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Renewal Start Date *</label>
              <input
                type="date"
                required
                value={renewStartDate}
                onChange={(e) => setRenewStartDate(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
              />
            </div>

            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Renewal End Date *</label>
              <input
                type="date"
                required
                value={renewEndDate}
                onChange={(e) => setRenewEndDate(e.target.value)}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">New Total Value (VND)</label>
            <input
              type="number"
              step="500000"
              value={renewTotalValue || 0}
              onChange={(e) => setRenewTotalValue(Number(e.target.value))}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Renewal Notes</label>
            <textarea
              rows={2}
              value={renewNotes}
              onChange={(e) => setRenewNotes(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
            />
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => setIsRenewOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
              Cancel
            </button>
            <button type="submit" className="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg text-sm font-semibold">
              Confirm Renewal
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
};
