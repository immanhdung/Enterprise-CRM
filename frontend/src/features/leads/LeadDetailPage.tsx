import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import dayjs from 'dayjs';
import {
  ArrowLeft,
  Building2,
  Mail,
  Phone,
  Calendar,
  DollarSign,
  UserCheck,
  ArrowRightLeft,
  PlusCircle,
  Clock,
  CheckCircle,
  FileText,
} from 'lucide-react';
import { leadService } from '../../services/leadService';
import { timelineService } from '../../services/timelineService';
import { StatusBadge } from '../../components/common/StatusBadge';
import { ActivityTimeline } from '../../components/common/ActivityTimeline';
import { AssignLeadModal } from './components/AssignLeadModal';
import { ConvertLeadModal } from './components/ConvertLeadModal';
import { LogActivityModal } from './components/LogActivityModal';
import type { LeadDto, TimelineEventDto } from '../../types/crm';

export const LeadDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [lead, setLead] = useState<LeadDto | null>(null);
  const [timeline, setTimeline] = useState<TimelineEventDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [notification, setNotification] = useState<string | null>(null);

  // Modals
  const [isAssignOpen, setIsAssignOpen] = useState(false);
  const [isConvertOpen, setIsConvertOpen] = useState(false);
  const [isLogActivityOpen, setIsLogActivityOpen] = useState(false);

  const loadLeadData = useCallback(async () => {
    if (!id) return;
    try {
      setIsLoading(true);
      setError(null);
      const [leadRes, timelineRes] = await Promise.all([
        leadService.getLeadById(id),
        timelineService.getEntityTimeline('Lead', id),
      ]);

      if (leadRes.success && leadRes.data) {
        setLead(leadRes.data);
      } else {
        setError(leadRes.message || 'Lead not found.');
      }

      if (timelineRes.success && timelineRes.data) {
        setTimeline(timelineRes.data);
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error loading lead details.');
    } finally {
      setIsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    loadLeadData();
  }, [loadLeadData]);

  const handleStatusChange = async (newStatus: string) => {
    if (!id || !lead) return;
    try {
      const res = await leadService.updateLeadStatus(id, newStatus);
      if (res.success && res.data) {
        setLead(res.data);
        setNotification(`Lead status updated to ${newStatus}.`);
        loadLeadData();
        setTimeout(() => setNotification(null), 3000);
      } else {
        setError(res.message || 'Failed to update status.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Status update failed.');
    }
  };

  if (isLoading) {
    return (
      <div className="py-20 text-center text-slate-400">
        <Clock className="w-8 h-8 animate-spin mx-auto mb-3 text-blue-500" />
        <p>Loading lead record...</p>
      </div>
    );
  }

  if (error || !lead) {
    return (
      <div className="space-y-4">
        <button
          onClick={() => navigate('/leads')}
          className="text-xs text-slate-500 hover:text-slate-800 flex items-center gap-1"
        >
          <ArrowLeft className="w-3.5 h-3.5" /> Back to Leads
        </button>
        <div className="p-4 bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 rounded-lg text-sm text-red-600 dark:text-red-400">
          {error || 'Lead could not be found.'}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Back Button */}
      <div className="flex items-center justify-between">
        <button
          onClick={() => navigate('/leads')}
          className="text-xs text-slate-500 hover:text-slate-800 dark:hover:text-slate-200 flex items-center gap-1 transition-colors"
        >
          <ArrowLeft className="w-3.5 h-3.5" /> Back to Leads
        </button>

        <div className="flex items-center gap-2">
          <button
            onClick={() => setIsLogActivityOpen(true)}
            className="px-3 py-1.5 text-xs font-medium text-slate-700 dark:text-slate-200 bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 flex items-center gap-1.5 transition-colors"
          >
            <PlusCircle className="w-3.5 h-3.5 text-blue-500" />
            Log Activity
          </button>

          <button
            onClick={() => setIsAssignOpen(true)}
            className="px-3 py-1.5 text-xs font-medium text-slate-700 dark:text-slate-200 bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 flex items-center gap-1.5 transition-colors"
          >
            <UserCheck className="w-3.5 h-3.5 text-indigo-500" />
            Reassign
          </button>

          {lead.status !== 'Converted' ? (
            <button
              onClick={() => setIsConvertOpen(true)}
              className="px-3.5 py-1.5 text-xs font-semibold text-white bg-emerald-600 hover:bg-emerald-700 rounded-lg flex items-center gap-1.5 transition-colors shadow-sm"
            >
              <ArrowRightLeft className="w-3.5 h-3.5" />
              Convert Lead
            </button>
          ) : (
            <div className="px-3 py-1.5 bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 dark:border-emerald-800 text-xs text-emerald-800 dark:text-emerald-300 rounded-lg flex items-center gap-1.5">
              <CheckCircle className="w-3.5 h-3.5 text-emerald-600" />
              <span>Converted to {lead.convertedAccountName || 'Account'}</span>
            </div>
          )}
        </div>
      </div>

      {notification && (
        <div className="p-3 bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 dark:border-emerald-800 rounded-lg text-xs text-emerald-800 dark:text-emerald-300 flex items-center gap-2">
          <CheckCircle className="w-4 h-4 shrink-0 text-emerald-600" />
          <span>{notification}</span>
        </div>
      )}

      {/* Main Profile Header Card */}
      <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 pb-6 border-b border-slate-100 dark:border-slate-700/60">
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
                {lead.fullName}
              </h1>
              <StatusBadge status={lead.status} />
              <span className="text-xs px-2.5 py-0.5 rounded-full font-medium bg-slate-100 dark:bg-slate-700 text-slate-600 dark:text-slate-300">
                Score: {lead.qualityScore}/100
              </span>
            </div>
            <div className="flex flex-wrap items-center gap-4 mt-2 text-xs text-slate-500 dark:text-slate-400">
              {lead.companyName && (
                <span className="flex items-center gap-1 text-slate-700 dark:text-slate-300 font-medium">
                  <Building2 className="w-3.5 h-3.5 text-blue-500" />
                  {lead.companyName}
                </span>
              )}
              {lead.position && <span>• {lead.position}</span>}
              <span>• Code: {lead.code}</span>
              <span>• Source: {lead.source || 'Website'}</span>
              {lead.campaign && <span>• Campaign: {lead.campaign}</span>}
            </div>
          </div>

          {/* Quick status selector */}
          <div className="flex items-center gap-2">
            <span className="text-xs font-medium text-slate-500">Status:</span>
            <select
              value={lead.status}
              onChange={(e) => handleStatusChange(e.target.value)}
              className="text-xs px-2.5 py-1.5 border rounded-lg bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-800 dark:text-slate-200 font-medium"
            >
              <option value="New">New</option>
              <option value="Contacted">Contacted</option>
              <option value="Qualified">Qualified</option>
              <option value="Unqualified">Unqualified</option>
              <option value="Converted">Converted</option>
              <option value="Stagnant">Stagnant</option>
            </select>
          </div>
        </div>

        {/* Lead Details Grid */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 pt-6 text-xs">
          <div className="p-3 bg-slate-50 dark:bg-slate-900/40 rounded-lg border border-slate-100 dark:border-slate-800">
            <div className="text-slate-400 mb-1 flex items-center gap-1">
              <Mail className="w-3.5 h-3.5" /> Email
            </div>
            <div className="font-semibold text-slate-800 dark:text-slate-200 break-all">
              {lead.email || '—'}
            </div>
          </div>

          <div className="p-3 bg-slate-50 dark:bg-slate-900/40 rounded-lg border border-slate-100 dark:border-slate-800">
            <div className="text-slate-400 mb-1 flex items-center gap-1">
              <Phone className="w-3.5 h-3.5" /> Phone
            </div>
            <div className="font-semibold text-slate-800 dark:text-slate-200">
              {lead.phone || '—'}
            </div>
          </div>

          <div className="p-3 bg-slate-50 dark:bg-slate-900/40 rounded-lg border border-slate-100 dark:border-slate-800">
            <div className="text-slate-400 mb-1 flex items-center gap-1">
              <DollarSign className="w-3.5 h-3.5" /> Expected Budget
            </div>
            <div className="font-semibold text-emerald-600 dark:text-emerald-400 text-sm">
              {lead.expectedBudget ? `$${Number(lead.expectedBudget).toLocaleString()}` : '—'}
            </div>
          </div>

          <div className="p-3 bg-slate-50 dark:bg-slate-900/40 rounded-lg border border-slate-100 dark:border-slate-800">
            <div className="text-slate-400 mb-1 flex items-center gap-1">
              <Calendar className="w-3.5 h-3.5" /> Next Follow-Up
            </div>
            <div className="font-semibold text-slate-800 dark:text-slate-200">
              {lead.nextFollowUpDate ? dayjs(lead.nextFollowUpDate).format('MMM D, YYYY') : 'None scheduled'}
            </div>
          </div>
        </div>

        {/* Assigned Rep & Description */}
        <div className="mt-4 pt-4 border-t border-slate-100 dark:border-slate-700/60 grid grid-cols-1 md:grid-cols-2 gap-4 text-xs">
          <div>
            <span className="font-medium text-slate-500">Assigned Representative:</span>{' '}
            <span className="font-semibold text-slate-800 dark:text-slate-200">
              {lead.assignedEmployeeName || 'Unassigned'}
            </span>
          </div>

          <div>
            <span className="font-medium text-slate-500">Priority Level:</span>{' '}
            <span className="font-semibold text-slate-800 dark:text-slate-200">
              {lead.priority}
            </span>
          </div>

          {lead.description && (
            <div className="md:col-span-2 mt-2">
              <div className="font-medium text-slate-500 mb-1 flex items-center gap-1">
                <FileText className="w-3.5 h-3.5" /> Description & Scope Notes:
              </div>
              <div className="p-3 rounded-lg bg-slate-50 dark:bg-slate-900/40 text-slate-700 dark:text-slate-300 whitespace-pre-line leading-relaxed">
                {lead.description}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Activities & Timeline Section */}
      <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs">
        <div className="flex items-center justify-between pb-4 mb-4 border-b border-slate-100 dark:border-slate-700/60">
          <div>
            <h2 className="text-base font-bold text-slate-900 dark:text-white">
              Activity History & Customer Timeline
            </h2>
            <p className="text-xs text-slate-500">
              Chronological log of customer touchpoints, calls, meetings, notes, and pipeline transitions.
            </p>
          </div>

          <button
            onClick={() => setIsLogActivityOpen(true)}
            className="px-3 py-1.5 text-xs font-semibold text-white bg-blue-600 hover:bg-blue-700 rounded-lg flex items-center gap-1.5 transition-colors shadow-xs"
          >
            <PlusCircle className="w-3.5 h-3.5" />
            Log Touchpoint
          </button>
        </div>

        <ActivityTimeline events={timeline} emptyMessage="No activities or interactions recorded for this lead yet." />
      </div>

      {/* Modals */}
      <AssignLeadModal
        isOpen={isAssignOpen}
        onClose={() => setIsAssignOpen(false)}
        lead={lead}
        onSuccess={() => {
          setNotification('Lead reassigned successfully.');
          loadLeadData();
          setTimeout(() => setNotification(null), 3000);
        }}
      />

      <ConvertLeadModal
        isOpen={isConvertOpen}
        onClose={() => setIsConvertOpen(false)}
        lead={lead}
        onSuccess={(result) => {
          setNotification(`Lead converted into Account "${result.accountName}" & Deal "${result.dealTitle}".`);
          loadLeadData();
          setTimeout(() => setNotification(null), 5000);
        }}
      />

      <LogActivityModal
        isOpen={isLogActivityOpen}
        onClose={() => setIsLogActivityOpen(false)}
        leadId={lead.id}
        leadName={lead.fullName}
        onSuccess={() => {
          setNotification('Activity logged.');
          loadLeadData();
          setTimeout(() => setNotification(null), 3000);
        }}
      />
    </div>
  );
};
