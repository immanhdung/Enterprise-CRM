import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import dayjs from 'dayjs';
import {
  ArrowLeft,
  Building2,
  DollarSign,
  TrendingUp,
  Calendar,
  Clock,
  History,
  CheckCircle,
  FileText,
} from 'lucide-react';
import { dealService } from '../../services/dealService';
import { timelineService } from '../../services/timelineService';
import { StatusBadge } from '../../components/common/StatusBadge';
import { ActivityTimeline } from '../../components/common/ActivityTimeline';
import type { DealDto, DealStageHistoryDto, TimelineEventDto } from '../../types/crm';

export const DealDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [deal, setDeal] = useState<DealDto | null>(null);
  const [stageHistories, setStageHistories] = useState<DealStageHistoryDto[]>([]);
  const [timeline, setTimeline] = useState<TimelineEventDto[]>([]);
  const [stages, setStages] = useState<{ id: string; name: string }[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [notification, setNotification] = useState<string | null>(null);

  const loadDealData = useCallback(async () => {
    if (!id) return;
    try {
      setIsLoading(true);
      setError(null);
      const [dealRes, histRes, timeRes, kanbanRes] = await Promise.all([
        dealService.getDealById(id),
        dealService.getDealStageHistory(id),
        timelineService.getEntityTimeline('Deal', id),
        dealService.getKanbanBoard(),
      ]);

      if (dealRes.success && dealRes.data) {
        setDeal(dealRes.data);
      } else {
        setError(dealRes.message || 'Deal not found.');
      }

      if (histRes.success && histRes.data) {
        setStageHistories(histRes.data);
      }

      if (timeRes.success && timeRes.data) {
        setTimeline(timeRes.data);
      }

      if (kanbanRes.success && kanbanRes.data) {
        setStages(kanbanRes.data.columns.map((c) => ({ id: c.stageId, name: c.stageName })));
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error loading deal details.');
    } finally {
      setIsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    loadDealData();
  }, [loadDealData]);

  const handleStageChange = async (newStageId: string) => {
    if (!id || !deal) return;
    try {
      const targetStage = stages.find((s) => s.id === newStageId);
      const res = await dealService.updateDealStage(id, {
        stageId: newStageId,
        notes: `Manual stage transition to ${targetStage?.name || 'new stage'}.`,
      });

      if (res.success && res.data) {
        setDeal(res.data);
        setNotification(`Stage changed to ${res.data.stageName}.`);
        loadDealData();
        setTimeout(() => setNotification(null), 3000);
      } else {
        setError(res.message || 'Failed to update stage.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Stage update failed.');
    }
  };

  if (isLoading) {
    return (
      <div className="py-20 text-center text-slate-400">
        <Clock className="w-8 h-8 animate-spin mx-auto mb-3 text-blue-500" />
        <p>Loading deal record...</p>
      </div>
    );
  }

  if (error || !deal) {
    return (
      <div className="space-y-4">
        <button
          onClick={() => navigate('/deals')}
          className="text-xs text-slate-500 hover:text-slate-800 flex items-center gap-1"
        >
          <ArrowLeft className="w-3.5 h-3.5" /> Back to Deals
        </button>
        <div className="p-4 bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 rounded-lg text-sm text-red-600 dark:text-red-400">
          {error || 'Deal could not be found.'}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Back button */}
      <div className="flex items-center justify-between">
        <button
          onClick={() => navigate('/deals')}
          className="text-xs text-slate-500 hover:text-slate-800 dark:hover:text-slate-200 flex items-center gap-1 transition-colors"
        >
          <ArrowLeft className="w-3.5 h-3.5" /> Back to Deals
        </button>

        <button
          onClick={() => navigate('/pipeline')}
          className="text-xs text-blue-600 font-medium hover:underline flex items-center gap-1"
        >
          View in Pipeline Kanban
        </button>
      </div>

      {notification && (
        <div className="p-3 bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 dark:border-emerald-800 rounded-lg text-xs text-emerald-800 dark:text-emerald-300 flex items-center gap-2">
          <CheckCircle className="w-4 h-4 shrink-0 text-emerald-600" />
          <span>{notification}</span>
        </div>
      )}

      {/* Main Deal Header Card */}
      <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 pb-6 border-b border-slate-100 dark:border-slate-700/60">
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
                {deal.title}
              </h1>
              <StatusBadge status={deal.status} />
              <span className="text-xs font-mono text-slate-400">
                {deal.code}
              </span>
            </div>
            <div className="flex flex-wrap items-center gap-4 mt-2 text-xs text-slate-500 dark:text-slate-400">
              <button
                onClick={() => navigate(`/accounts/${deal.accountId}`)}
                className="flex items-center gap-1 text-blue-600 hover:underline font-medium"
              >
                <Building2 className="w-3.5 h-3.5" />
                {deal.accountName}
              </button>
              {deal.contactName && <span>• Key Contact: {deal.contactName}</span>}
              <span>• Source: {deal.source || 'Direct'}</span>
            </div>
          </div>

          {/* Quick Stage Selector */}
          <div className="flex items-center gap-2">
            <span className="text-xs font-medium text-slate-500">Stage:</span>
            <select
              value={deal.stageId}
              onChange={(e) => handleStageChange(e.target.value)}
              className="text-xs px-3 py-1.5 border rounded-lg bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-800 dark:text-slate-200 font-semibold"
            >
              {stages.map((stg) => (
                <option key={stg.id} value={stg.id}>
                  {stg.name}
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* Financial & Forecast Metrics */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 pt-6 text-xs">
          <div className="p-4 bg-slate-50 dark:bg-slate-900/40 rounded-xl border border-slate-100 dark:border-slate-800">
            <div className="text-slate-400 mb-1 flex items-center gap-1">
              <DollarSign className="w-3.5 h-3.5" /> Total Contract Value
            </div>
            <div className="text-xl font-bold text-slate-900 dark:text-white">
              ${Number(deal.amount).toLocaleString()}
            </div>
          </div>

          <div className="p-4 bg-slate-50 dark:bg-slate-900/40 rounded-xl border border-slate-100 dark:border-slate-800">
            <div className="text-slate-400 mb-1 flex items-center gap-1">
              <TrendingUp className="w-3.5 h-3.5" /> Win Probability
            </div>
            <div className="text-xl font-bold text-blue-600 dark:text-blue-400">
              {deal.effectiveProbability}%
            </div>
          </div>

          <div className="p-4 bg-slate-50 dark:bg-slate-900/40 rounded-xl border border-slate-100 dark:border-slate-800">
            <div className="text-slate-400 mb-1 flex items-center gap-1">
              <DollarSign className="w-3.5 h-3.5" /> Expected Revenue
            </div>
            <div className="text-xl font-bold text-emerald-600 dark:text-emerald-400">
              ${Number(deal.expectedRevenue).toLocaleString()}
            </div>
            <div className="text-[10px] text-slate-400 mt-0.5">Value × Probability</div>
          </div>

          <div className="p-4 bg-slate-50 dark:bg-slate-900/40 rounded-xl border border-slate-100 dark:border-slate-800">
            <div className="text-slate-400 mb-1 flex items-center gap-1">
              <Calendar className="w-3.5 h-3.5" /> Target Close Date
            </div>
            <div className="text-xl font-bold text-slate-800 dark:text-slate-200">
              {deal.expectedClosingDate ? dayjs(deal.expectedClosingDate).format('MMM D, YYYY') : 'Not set'}
            </div>
          </div>
        </div>

        {deal.description && (
          <div className="mt-4 pt-4 border-t border-slate-100 dark:border-slate-700/60 text-xs">
            <div className="font-semibold text-slate-700 dark:text-slate-300 mb-1 flex items-center gap-1">
              <FileText className="w-3.5 h-3.5" /> Scope & Commercial Notes:
            </div>
            <p className="text-slate-600 dark:text-slate-300 leading-relaxed whitespace-pre-line p-3 bg-slate-50 dark:bg-slate-900/40 rounded-lg">
              {deal.description}
            </p>
          </div>
        )}
      </div>

      {/* Stage Progression History & Timeline Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Stage Transitions Card */}
        <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-4">
          <div className="flex items-center gap-2 pb-3 border-b border-slate-100 dark:border-slate-700">
            <History className="w-4 h-4 text-indigo-600" />
            <h3 className="font-bold text-sm text-slate-900 dark:text-white">
              Stage Velocity & History
            </h3>
          </div>

          {stageHistories.length === 0 ? (
            <p className="text-xs text-slate-400 py-6 text-center">No stage transitions logged yet.</p>
          ) : (
            <div className="space-y-3">
              {stageHistories.map((h) => (
                <div
                  key={h.id}
                  className="p-3 rounded-lg border border-slate-100 dark:border-slate-700 bg-slate-50/50 dark:bg-slate-900/30 text-xs flex items-center justify-between"
                >
                  <div>
                    <div className="font-semibold text-slate-800 dark:text-slate-200">
                      {h.fromStageName ? `${h.fromStageName} → ` : 'Started at '}
                      <span className="text-blue-600 dark:text-blue-400">{h.toStageName}</span>
                    </div>
                    {h.notes && <div className="text-[11px] text-slate-500 mt-0.5">{h.notes}</div>}
                    <div className="text-[10px] text-slate-400 mt-1">
                      By {h.changedByName} • {dayjs(h.changedAt).format('MMM D, YYYY HH:mm')}
                    </div>
                  </div>
                  <div className="text-right text-[11px] font-medium text-slate-600 dark:text-slate-400 shrink-0">
                    {h.durationDays} days in stage
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Customer Touchpoint Timeline */}
        <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-4">
          <div className="flex items-center gap-2 pb-3 border-b border-slate-100 dark:border-slate-700">
            <Clock className="w-4 h-4 text-blue-600" />
            <h3 className="font-bold text-sm text-slate-900 dark:text-white">
              Opportunity Activity Timeline
            </h3>
          </div>

          <ActivityTimeline events={timeline} emptyMessage="No activities logged on this deal." />
        </div>
      </div>
    </div>
  );
};
