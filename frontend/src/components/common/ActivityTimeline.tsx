import React from 'react';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';
import {
  PhoneCall,
  Video,
  FileText,
  Mail,
  ArrowRightCircle,
  CheckCircle2,
  Calendar,
  Clock,
  Sparkles,
} from 'lucide-react';
import type { TimelineEventDto } from '../../types/crm';

dayjs.extend(relativeTime);

interface ActivityTimelineProps {
  events: TimelineEventDto[];
  isLoading?: boolean;
  emptyMessage?: string;
}

export const ActivityTimeline: React.FC<ActivityTimelineProps> = ({
  events,
  isLoading = false,
  emptyMessage = 'No activities recorded yet.',
}) => {
  if (isLoading) {
    return (
      <div className="space-y-4 py-4">
        {[1, 2, 3].map((i) => (
          <div key={i} className="flex gap-4 animate-pulse">
            <div className="w-8 h-8 rounded-full bg-slate-200 dark:bg-slate-700" />
            <div className="flex-1 space-y-2">
              <div className="h-4 bg-slate-200 dark:bg-slate-700 rounded w-1/3" />
              <div className="h-3 bg-slate-100 dark:bg-slate-800 rounded w-2/3" />
            </div>
          </div>
        ))}
      </div>
    );
  }

  if (!events || events.length === 0) {
    return (
      <div className="py-8 text-center text-slate-500 dark:text-slate-400">
        <Clock className="w-8 h-8 mx-auto mb-2 opacity-50" />
        <p className="text-sm">{emptyMessage}</p>
      </div>
    );
  }

  const getEventIcon = (type: string) => {
    switch (type.toLowerCase()) {
      case 'call':
        return <PhoneCall className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />;
      case 'meeting':
        return <Video className="w-4 h-4 text-purple-600 dark:text-purple-400" />;
      case 'email':
        return <Mail className="w-4 h-4 text-blue-600 dark:text-blue-400" />;
      case 'note':
        return <FileText className="w-4 h-4 text-amber-600 dark:text-amber-400" />;
      case 'stagechange':
        return <ArrowRightCircle className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />;
      case 'dealconverted':
        return <Sparkles className="w-4 h-4 text-green-600 dark:text-green-400" />;
      case 'taskcompleted':
        return <CheckCircle2 className="w-4 h-4 text-teal-600 dark:text-teal-400" />;
      case 'appointment':
        return <Calendar className="w-4 h-4 text-sky-600 dark:text-sky-400" />;
      default:
        return <Clock className="w-4 h-4 text-slate-500" />;
    }
  };

  const getEventBadgeColor = (type: string) => {
    switch (type.toLowerCase()) {
      case 'call':
        return 'bg-emerald-50 border-emerald-200 dark:bg-emerald-950/40 dark:border-emerald-800';
      case 'meeting':
        return 'bg-purple-50 border-purple-200 dark:bg-purple-950/40 dark:border-purple-800';
      case 'email':
        return 'bg-blue-50 border-blue-200 dark:bg-blue-950/40 dark:border-blue-800';
      case 'note':
        return 'bg-amber-50 border-amber-200 dark:bg-amber-950/40 dark:border-amber-800';
      case 'stagechange':
        return 'bg-indigo-50 border-indigo-200 dark:bg-indigo-950/40 dark:border-indigo-800';
      case 'dealconverted':
        return 'bg-green-50 border-green-200 dark:bg-green-950/40 dark:border-green-800';
      case 'taskcompleted':
        return 'bg-teal-50 border-teal-200 dark:bg-teal-950/40 dark:border-teal-800';
      default:
        return 'bg-slate-50 border-slate-200 dark:bg-slate-800 dark:border-slate-700';
    }
  };

  return (
    <div className="relative pl-6 space-y-6 before:absolute before:left-3 before:top-2 before:bottom-2 before:w-0.5 before:bg-slate-200 dark:before:bg-slate-700">
      {events.map((ev) => (
        <div key={ev.id} className="relative group">
          {/* Timeline node icon */}
          <div
            className={`absolute -left-6 top-1 w-6 h-6 rounded-full border flex items-center justify-center bg-white dark:bg-slate-900 shadow-sm ${getEventBadgeColor(
              ev.type
            )}`}
          >
            {getEventIcon(ev.type)}
          </div>

          <div className="bg-white dark:bg-slate-800 p-3.5 rounded-lg border border-slate-200 dark:border-slate-700 shadow-xs hover:border-slate-300 dark:hover:border-slate-600 transition-all">
            <div className="flex flex-wrap items-center justify-between gap-2 mb-1.5">
              <span className="font-semibold text-sm text-slate-800 dark:text-slate-100">
                {ev.title}
              </span>
              <span className="text-xs text-slate-400 dark:text-slate-500 flex items-center gap-1">
                <Clock className="w-3 h-3" />
                {dayjs(ev.timestamp).fromNow()} ({dayjs(ev.timestamp).format('MMM D, YYYY HH:mm')})
              </span>
            </div>

            {ev.description && (
              <p className="text-xs text-slate-600 dark:text-slate-300 whitespace-pre-line leading-relaxed mb-2">
                {ev.description}
              </p>
            )}

            <div className="flex items-center justify-between text-xs text-slate-400 dark:text-slate-500 pt-1 border-t border-slate-100 dark:border-slate-700/50">
              <span className="capitalize text-[11px] font-medium px-2 py-0.5 rounded-full bg-slate-100 dark:bg-slate-700 text-slate-700 dark:text-slate-300">
                {ev.type}
              </span>
              <span>By {ev.performedByName || 'System'}</span>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
};
