import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  Download,
  Mail,
  Send,
  CheckCircle,
  XCircle,
  Copy,
  Clock,
  FileCheck,
  Building,
  Calendar,
  Layers,
} from 'lucide-react';
import { quotationService } from '../../services/quotationService';
import { contractService } from '../../services/contractService';
import type { Quotation } from '../../types/billing';
import { Modal } from '../../components/common/Modal';

export const QuotationDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [quotation, setQuotation] = useState<Quotation | null>(null);
  const [versions, setVersions] = useState<Quotation[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Modals state
  const [isSubmitOpen, setIsSubmitOpen] = useState(false);
  const [submitNotes, setSubmitNotes] = useState('Submitted for formal review.');

  const [isApproveOpen, setIsApproveOpen] = useState(false);
  const [approvalNotes, setApprovalNotes] = useState('Commercial terms and deliverables approved.');

  const [isRejectOpen, setIsRejectOpen] = useState(false);
  const [rejectionReason, setRejectionReason] = useState('');

  const [isRevisionOpen, setIsRevisionOpen] = useState(false);
  const [revisionNotes, setRevisionNotes] = useState('');

  const [isEmailOpen, setIsEmailOpen] = useState(false);
  const [recipientEmail, setRecipientEmail] = useState('');
  const [emailSubject, setEmailSubject] = useState('');
  const [emailBody, setEmailBody] = useState('');

  const loadQuotation = useCallback(async () => {
    if (!id) return;
    setIsLoading(true);
    try {
      const [res, versRes] = await Promise.all([
        quotationService.getQuotationById(id),
        quotationService.getVersions(id),
      ]);
      if (res.data) {
        setQuotation(res.data);
        setEmailSubject(`Service Quotation ${res.data.quotationNumber} - ${res.data.accountName}`);
        setEmailBody(
          `Dear Client,\n\nPlease find attached our formal service quotation ${res.data.quotationNumber} (${res.data.revisionNumber}) for your review and approval.\n\nBest regards,\nSales & Account Management Team`
        );
      }
      if (versRes.data) setVersions(versRes.data);
    } catch (err) {
      console.error('Failed to load quotation details', err);
    } finally {
      setIsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    loadQuotation();
  }, [loadQuotation]);

  const handleSubmitForApproval = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;
    try {
      await quotationService.submitForApproval(id, submitNotes);
      setIsSubmitOpen(false);
      loadQuotation();
    } catch (err) {
      console.error('Failed to submit quotation', err);
    }
  };

  const handleApprove = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;
    try {
      await quotationService.approveQuotation(id, approvalNotes);
      setIsApproveOpen(false);
      loadQuotation();
    } catch (err) {
      console.error('Failed to approve quotation', err);
    }
  };

  const handleReject = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id || !rejectionReason.trim()) return;
    try {
      await quotationService.rejectQuotation(id, rejectionReason);
      setIsRejectOpen(false);
      loadQuotation();
    } catch (err) {
      console.error('Failed to reject quotation', err);
    }
  };

  const handleCreateRevision = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;
    try {
      const res = await quotationService.createRevision(id, revisionNotes || 'Customer scope adjustment');
      setIsRevisionOpen(false);
      if (res.data) {
        navigate(`/quotations/${res.data.id}`);
      } else {
        loadQuotation();
      }
    } catch (err) {
      console.error('Failed to create revision', err);
    }
  };

  const handleDownloadPdf = async () => {
    if (!quotation) return;
    try {
      await quotationService.downloadPdf(quotation.id, quotation.quotationNumber, quotation.revisionNumber);
    } catch (err) {
      console.error('Failed to download PDF', err);
    }
  };

  const handleSendEmail = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id || !recipientEmail) return;
    try {
      await quotationService.sendEmail(id, recipientEmail, emailSubject, emailBody);
      setIsEmailOpen(false);
      loadQuotation();
      alert('Quotation dispatched to client email successfully.');
    } catch (err) {
      console.error('Failed to send email', err);
    }
  };

  const handleGenerateContract = async () => {
    if (!quotation) return;
    try {
      const res = await contractService.createFromQuotation({
        quotationId: quotation.id,
        title: `Service Agreement - ${quotation.accountName}`,
      });
      if (res.data) {
        navigate(`/contracts/${res.data.id}`);
      }
    } catch (err) {
      console.error('Failed to generate contract', err);
    }
  };

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Draft':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-slate-100 text-slate-700">Draft</span>;
      case 'Submitted':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-amber-100 text-amber-800">Pending BOD Approval</span>;
      case 'Approved':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-emerald-100 text-emerald-800">Approved</span>;
      case 'Rejected':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-rose-100 text-rose-800">Rejected</span>;
      case 'Sent':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-blue-100 text-blue-800">Dispatched to Client</span>;
      case 'Accepted':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-purple-100 text-purple-800">Accepted by Client</span>;
      default:
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-slate-100 text-slate-700">{status}</span>;
    }
  };

  if (isLoading || !quotation) {
    return (
      <div className="p-12 text-center text-slate-500">
        Loading quotation details...
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Top Navigation */}
      <div className="flex items-center justify-between">
        <button
          onClick={() => navigate('/quotations')}
          className="flex items-center gap-2 text-sm text-slate-600 hover:text-slate-900 font-medium"
        >
          <ArrowLeft className="w-4 h-4" /> Back to Quotations
        </button>

        {/* Versions Selector */}
        {versions.length > 1 && (
          <div className="flex items-center gap-2">
            <span className="text-xs font-medium text-slate-500">Revisions History:</span>
            <select
              value={quotation.id}
              onChange={(e) => navigate(`/quotations/${e.target.value}`)}
              className="px-3 py-1 border border-slate-300 rounded-lg text-xs bg-white font-mono font-medium"
            >
              {versions.map((v) => (
                <option key={v.id} value={v.id}>
                  {v.revisionNumber} - {v.status} ({new Date(v.createdAt).toLocaleDateString()})
                </option>
              ))}
            </select>
          </div>
        )}
      </div>

      {/* Main Header Card */}
      <div className="bg-white border border-slate-200 rounded-xl p-6 shadow-xs flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
        <div className="space-y-2">
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold text-slate-900 font-mono">
              {quotation.quotationNumber}
            </h1>
            <span className="px-2.5 py-0.5 rounded font-mono text-xs font-bold bg-blue-100 text-blue-700">
              {quotation.revisionNumber}
            </span>
            {getStatusBadge(quotation.status)}
          </div>

          <div className="flex flex-wrap items-center gap-4 text-xs text-slate-500">
            <span className="flex items-center gap-1.5 font-medium text-slate-700">
              <Building className="w-4 h-4 text-slate-400" />
              {quotation.accountName}
            </span>
            <span>•</span>
            <span className="flex items-center gap-1.5">
              <Calendar className="w-4 h-4 text-slate-400" />
              Valid Until: {new Date(quotation.validUntil).toLocaleDateString()}
            </span>
            {quotation.priceListName && (
              <>
                <span>•</span>
                <span className="flex items-center gap-1.5">
                  <Layers className="w-4 h-4 text-slate-400" />
                  Rate Card: {quotation.priceListName}
                </span>
              </>
            )}
          </div>
        </div>

        {/* Action Buttons Toolbar */}
        <div className="flex flex-wrap items-center gap-2">
          {quotation.status === 'Draft' && (
            <button
              onClick={() => setIsSubmitOpen(true)}
              className="flex items-center gap-1.5 px-3.5 py-2 bg-blue-600 hover:bg-blue-700 text-white text-xs font-semibold rounded-lg shadow-sm"
            >
              <Send className="w-4 h-4" /> Submit for Approval
            </button>
          )}

          {quotation.status === 'Submitted' && (
            <>
              <button
                onClick={() => setIsApproveOpen(true)}
                className="flex items-center gap-1.5 px-3.5 py-2 bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-semibold rounded-lg shadow-sm"
              >
                <CheckCircle className="w-4 h-4" /> Approve Quotation
              </button>
              <button
                onClick={() => setIsRejectOpen(true)}
                className="flex items-center gap-1.5 px-3.5 py-2 bg-rose-600 hover:bg-rose-700 text-white text-xs font-semibold rounded-lg shadow-sm"
              >
                <XCircle className="w-4 h-4" /> Reject
              </button>
            </>
          )}

          {quotation.status === 'Approved' && (
            <button
              onClick={handleGenerateContract}
              className="flex items-center gap-1.5 px-3.5 py-2 bg-purple-600 hover:bg-purple-700 text-white text-xs font-semibold rounded-lg shadow-sm"
            >
              <FileCheck className="w-4 h-4" /> Generate Contract
            </button>
          )}

          <button
            onClick={handleDownloadPdf}
            className="flex items-center gap-1.5 px-3 py-2 border border-slate-300 hover:bg-slate-50 text-slate-700 text-xs font-semibold rounded-lg"
          >
            <Download className="w-4 h-4 text-blue-600" /> PDF Document
          </button>

          {(quotation.status === 'Approved' || quotation.status === 'Sent') && (
            <button
              onClick={() => setIsEmailOpen(true)}
              className="flex items-center gap-1.5 px-3 py-2 border border-slate-300 hover:bg-slate-50 text-slate-700 text-xs font-semibold rounded-lg"
            >
              <Mail className="w-4 h-4 text-indigo-600" /> Send Email
            </button>
          )}

          {(quotation.status === 'Approved' || quotation.status === 'Sent' || quotation.status === 'Rejected') && (
            <button
              onClick={() => setIsRevisionOpen(true)}
              className="flex items-center gap-1.5 px-3 py-2 border border-slate-300 hover:bg-slate-50 text-slate-700 text-xs font-semibold rounded-lg"
            >
              <Copy className="w-4 h-4 text-amber-600" /> Create Revision
            </button>
          )}
        </div>
      </div>

      {/* Grid: Line Items & Summary */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Line Items Table (2 cols) */}
        <div className="lg:col-span-2 space-y-6">
          <div className="bg-white border border-slate-200 rounded-xl overflow-hidden shadow-xs">
            <div className="p-4 bg-slate-50 border-b border-slate-200 font-semibold text-sm text-slate-800">
              Deliverable Scope & Billable Items ({quotation.items.length})
            </div>

            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs text-slate-700">
                <thead className="bg-slate-50/75 border-b border-slate-200 uppercase font-semibold text-slate-500">
                  <tr>
                    <th className="px-4 py-3">Service</th>
                    <th className="px-3 py-3 text-center">Unit</th>
                    <th className="px-3 py-3 text-center">Qty</th>
                    <th className="px-4 py-3 text-right">Unit Price</th>
                    <th className="px-3 py-3 text-center">Disc%</th>
                    <th className="px-3 py-3 text-center">VAT%</th>
                    <th className="px-4 py-3 text-right">Line Total</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-200">
                  {quotation.items.map((it) => (
                    <tr key={it.id} className="hover:bg-slate-50/50">
                      <td className="px-4 py-3">
                        <div className="font-semibold text-slate-900">{it.serviceName}</div>
                        <div className="font-mono text-[11px] text-slate-400">{it.serviceCode}</div>
                        {it.notes && <div className="text-[11px] italic text-slate-500">{it.notes}</div>}
                      </td>
                      <td className="px-3 py-3 text-center text-slate-600">{it.unit}</td>
                      <td className="px-3 py-3 text-center font-semibold text-slate-800">{it.quantity}</td>
                      <td className="px-4 py-3 text-right font-mono">{formatCurrency(it.unitPrice)}</td>
                      <td className="px-3 py-3 text-center text-rose-600">{it.discountRate}%</td>
                      <td className="px-3 py-3 text-center text-slate-600">{it.vatRate}%</td>
                      <td className="px-4 py-3 text-right font-mono font-bold text-slate-900">
                        {formatCurrency(it.lineTotal)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Terms & Conditions */}
          <div className="bg-white border border-slate-200 rounded-xl p-5 shadow-xs space-y-3">
            <h3 className="font-semibold text-sm text-slate-800">Commercial Terms & Conditions</h3>
            <p className="text-xs text-slate-600 whitespace-pre-wrap leading-relaxed">
              {quotation.termsAndConditions || 'Standard payment terms apply.'}
            </p>
            {quotation.notes && (
              <div className="pt-3 border-t border-slate-100 text-xs text-slate-500">
                <span className="font-medium text-slate-700">Internal Reference:</span> {quotation.notes}
              </div>
            )}
          </div>
        </div>

        {/* Sidebar: Financial Summary & Approval History */}
        <div className="space-y-6">
          {/* Summary Card */}
          <div className="bg-white border border-slate-200 rounded-xl p-5 shadow-xs space-y-3">
            <h3 className="font-semibold text-sm text-slate-800 border-b border-slate-100 pb-2">
              Financial Breakdown
            </h3>

            <div className="space-y-2 text-xs">
              <div className="flex justify-between text-slate-600">
                <span>Subtotal (Net):</span>
                <span className="font-mono">{formatCurrency(quotation.subtotal)}</span>
              </div>
              <div className="flex justify-between text-rose-600">
                <span>Total Discount:</span>
                <span className="font-mono">-{formatCurrency(quotation.discountAmount)}</span>
              </div>
              <div className="flex justify-between text-slate-600">
                <span>Value Added Tax (VAT):</span>
                <span className="font-mono">+{formatCurrency(quotation.vatAmount)}</span>
              </div>
              <div className="pt-2 border-t border-slate-200 flex justify-between items-center">
                <span className="font-bold text-slate-900 text-sm">Grand Total:</span>
                <span className="font-mono font-bold text-base text-blue-600">
                  {formatCurrency(quotation.totalAmount)}
                </span>
              </div>
            </div>
          </div>

          {/* Approval History Timeline */}
          <div className="bg-white border border-slate-200 rounded-xl p-5 shadow-xs space-y-4">
            <h3 className="font-semibold text-sm text-slate-800 border-b border-slate-100 pb-2 flex items-center gap-2">
              <Clock className="w-4 h-4 text-slate-400" />
              Approval & Dispatch Audit Trail
            </h3>

            <div className="space-y-3">
              {quotation.approvalHistories.length === 0 ? (
                <div className="text-xs text-slate-400 text-center py-4">No audit logs recorded yet.</div>
              ) : (
                quotation.approvalHistories.map((h) => (
                  <div key={h.id} className="border-l-2 border-blue-500 pl-3 py-1 space-y-0.5 text-xs">
                    <div className="flex justify-between items-center">
                      <span className="font-semibold text-slate-800">{h.action}</span>
                      <span className="text-[11px] text-slate-400">
                        {new Date(h.timestamp).toLocaleString()}
                      </span>
                    </div>
                    <div className="text-slate-500 text-[11px]">By: {h.actorName}</div>
                    {h.comments && <div className="text-slate-600 italic mt-0.5">{h.comments}</div>}
                  </div>
                ))
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Submit Modal */}
      <Modal isOpen={isSubmitOpen} onClose={() => setIsSubmitOpen(false)} title="Submit Quotation for BOD Approval">
        <form onSubmit={handleSubmitForApproval} className="space-y-4">
          <p className="text-xs text-slate-600">
            Submitting locks the quotation draft into "Submitted" status and alerts leadership for commercial authorization.
          </p>
          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Submission Notes</label>
            <textarea
              rows={3}
              value={submitNotes}
              onChange={(e) => setSubmitNotes(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
            />
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => setIsSubmitOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
              Cancel
            </button>
            <button type="submit" className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-semibold">
              Confirm Submission
            </button>
          </div>
        </form>
      </Modal>

      {/* Approve Modal */}
      <Modal isOpen={isApproveOpen} onClose={() => setIsApproveOpen(false)} title="Authorize Quotation">
        <form onSubmit={handleApprove} className="space-y-4">
          <p className="text-xs text-slate-600">
            Approving authorizes this quote for client dispatch and activates contract generation capabilities.
          </p>
          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Approval Comments</label>
            <input
              type="text"
              value={approvalNotes}
              onChange={(e) => setApprovalNotes(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
            />
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => setIsApproveOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
              Cancel
            </button>
            <button type="submit" className="px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-sm font-semibold">
              Approve Quotation
            </button>
          </div>
        </form>
      </Modal>

      {/* Reject Modal */}
      <Modal isOpen={isRejectOpen} onClose={() => setIsRejectOpen(false)} title="Reject Quotation">
        <form onSubmit={handleReject} className="space-y-4">
          <p className="text-xs text-slate-600">
            Please specify the commercial or technical rationale for declining this proposal.
          </p>
          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Rejection Reason *</label>
            <textarea
              rows={3}
              required
              value={rejectionReason}
              onChange={(e) => setRejectionReason(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
            />
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => setIsRejectOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
              Cancel
            </button>
            <button type="submit" className="px-4 py-2 bg-rose-600 hover:bg-rose-700 text-white rounded-lg text-sm font-semibold">
              Reject Quotation
            </button>
          </div>
        </form>
      </Modal>

      {/* Create Revision Modal */}
      <Modal isOpen={isRevisionOpen} onClose={() => setIsRevisionOpen(false)} title={`Create Revision from ${quotation.revisionNumber}`}>
        <form onSubmit={handleCreateRevision} className="space-y-4">
          <p className="text-xs text-slate-600">
            Creates a new sequential revision (e.g. {quotation.revisionNumber === 'REV-01' ? 'REV-02' : 'REV-03'}) preserving history while allowing line item adjustments.
          </p>
          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Scope Change Summary</label>
            <textarea
              rows={3}
              required
              placeholder="e.g. Revised SLA deliverables, added enterprise security audit tier"
              value={revisionNotes}
              onChange={(e) => setRevisionNotes(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
            />
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => setIsRevisionOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
              Cancel
            </button>
            <button type="submit" className="px-4 py-2 bg-amber-600 hover:bg-amber-700 text-white rounded-lg text-sm font-semibold">
              Generate Revision
            </button>
          </div>
        </form>
      </Modal>

      {/* Send Email Modal */}
      <Modal isOpen={isEmailOpen} onClose={() => setIsEmailOpen(false)} title="Dispatch Quotation via Email">
        <form onSubmit={handleSendEmail} className="space-y-4">
          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Recipient Email *</label>
            <input
              type="email"
              required
              placeholder="procurement@clientcorp.vn"
              value={recipientEmail}
              onChange={(e) => setRecipientEmail(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Subject</label>
            <input
              type="text"
              required
              value={emailSubject}
              onChange={(e) => setEmailSubject(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-slate-700 mb-1">Email Body</label>
            <textarea
              rows={4}
              value={emailBody}
              onChange={(e) => setEmailBody(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"
            />
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={() => setIsEmailOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
              Cancel
            </button>
            <button type="submit" className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-semibold">
              Send Email
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
};
