import React, { useState, useEffect, useCallback } from 'react';
import {
  RefreshCw,
  AlertCircle,
  Activity,
} from 'lucide-react';
import { timelineService } from '../../services/timelineService';
import { ActivityTimeline } from '../../components/common/ActivityTimeline';
import type { TimelineEventDto } from '../../types/crm';

export const ActivitiesPage: React.FC = () => {
  const [events, setEvents] = useState<TimelineEventDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filterType, setFilterType] = useState<string>('');

  const fetchTimeline = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const res = await timelineService.getRecentTimeline(50);
      if (res.success && res.data) {
        setEvents(res.data);
      } else {
        setError(res.message || 'Failed to fetch activity stream.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error loading timeline.');
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchTimeline();
  }, [fetchTimeline]);

  const filteredEvents = filterType
    ? events.filter((e) => e.type.toLowerCase() === filterType.toLowerCase())
    : events;

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white flex items-center gap-2">
            <Activity className="w-6 h-6 text-blue-600" />
            <span>Activity Stream & Audit Log</span>
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400">
            Real-time feed of all customer engagements, meetings, calls, stage movements, and deal conversions.
          </p>
        </div>

        <div className="flex items-center gap-2">
          <select
            value={filterType}
            onChange={(e) => setFilterType(e.target.value)}
            className="px-3 py-2 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300 font-medium"
          >
            <option value="">All Interaction Types</option>
            <option value="Call">Calls</option>
            <option value="Meeting">Meetings</option>
            <option value="Email">Emails</option>
            <option value="Note">Notes</option>
            <option value="StageChange">Pipeline Moves</option>
            <option value="DealConverted">Conversions</option>
            <option value="TaskCompleted">Tasks Completed</option>
          </select>

          <button
            onClick={fetchTimeline}
            title="Refresh Feed"
            className="p-2 border rounded-lg text-slate-500 hover:text-slate-800 dark:hover:text-slate-200 border-slate-300 dark:border-slate-700 transition-colors"
          >
            <RefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin text-blue-500' : ''}`} />
          </button>
        </div>
      </div>

      {error && (
        <div className="p-3 bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 rounded-lg text-xs text-red-600 dark:text-red-400 flex items-center gap-2">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs">
        <ActivityTimeline
          events={filteredEvents}
          isLoading={isLoading}
          emptyMessage="No recent customer touchpoints or activity events found."
        />
      </div>
    </div>
  );
};
