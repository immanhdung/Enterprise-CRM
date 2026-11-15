import React, { useState } from 'react';
import { Loader2, Phone, Video, Mail, FileText, AlertCircle } from 'lucide-react';
import { Modal } from '../../../components/common/Modal';
import { leadService } from '../../../services/leadService';

interface LogActivityModalProps {
  isOpen: boolean;
  onClose: () => void;
  leadId: string;
  leadName: string;
  onSuccess: () => void;
}

export const LogActivityModal: React.FC<LogActivityModalProps> = ({
  isOpen,
  onClose,
  leadId,
  leadName,
  onSuccess,
}) => {
  const [type, setType] = useState<'Call' | 'Meeting' | 'Email' | 'Note'>('Call');
  const [subject, setSubject] = useState('');
  const [notes, setNotes] = useState('');
  const [scheduledAt, setScheduledAt] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!subject.trim()) {
      setError('Please provide a subject for this activity.');
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);

      const res = await leadService.logLeadActivity(leadId, {
        type,
        subject: subject.trim(),
        notes: notes.trim() || undefined,
        scheduledAt: scheduledAt ? new Date(scheduledAt).toISOString() : undefined,
      });

      if (res.success) {
        onSuccess();
        onClose();
        setSubject('');
        setNotes('');
        setScheduledAt('');
      } else {
        setError(res.message || 'Failed to record activity.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error recording activity.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Log Customer Interaction"
      description={`Record touchpoint or activity with ${leadName}.`}
      maxWidth="md"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && (
          <div className="p-3 bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 rounded-lg text-xs text-red-600 dark:text-red-400 flex items-center gap-2">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{error}</span>
          </div>
        )}

        <div>
          <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-2">
            Interaction Type
          </label>
          <div className="grid grid-cols-4 gap-2">
            <button
              type="button"
              onClick={() => setType('Call')}
              className={`p-2.5 text-center rounded-lg border text-xs font-medium transition-all ${
                type === 'Call'
                  ? 'border-emerald-600 bg-emerald-50 text-emerald-900 dark:bg-emerald-950/50 dark:border-emerald-500 dark:text-emerald-100 ring-2 ring-emerald-500/20'
                  : 'border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300'
              }`}
            >
              <Phone className="w-4 h-4 mx-auto mb-1 text-emerald-600" />
              <span>Call</span>
            </button>

            <button
              type="button"
              onClick={() => setType('Meeting')}
              className={`p-2.5 text-center rounded-lg border text-xs font-medium transition-all ${
                type === 'Meeting'
                  ? 'border-purple-600 bg-purple-50 text-purple-900 dark:bg-purple-950/50 dark:border-purple-500 dark:text-purple-100 ring-2 ring-purple-500/20'
                  : 'border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300'
              }`}
            >
              <Video className="w-4 h-4 mx-auto mb-1 text-purple-600" />
              <span>Meeting</span>
            </button>

            <button
              type="button"
              onClick={() => setType('Email')}
              className={`p-2.5 text-center rounded-lg border text-xs font-medium transition-all ${
                type === 'Email'
                  ? 'border-blue-600 bg-blue-50 text-blue-900 dark:bg-blue-950/50 dark:border-blue-500 dark:text-blue-100 ring-2 ring-blue-500/20'
                  : 'border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300'
              }`}
            >
              <Mail className="w-4 h-4 mx-auto mb-1 text-blue-600" />
              <span>Email</span>
            </button>

            <button
              type="button"
              onClick={() => setType('Note')}
              className={`p-2.5 text-center rounded-lg border text-xs font-medium transition-all ${
                type === 'Note'
                  ? 'border-amber-600 bg-amber-50 text-amber-900 dark:bg-amber-950/50 dark:border-amber-500 dark:text-amber-100 ring-2 ring-amber-500/20'
                  : 'border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300'
              }`}
            >
              <FileText className="w-4 h-4 mx-auto mb-1 text-amber-600" />
              <span>Note</span>
            </button>
          </div>
        </div>

        <div>
          <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
            Subject / Headline *
          </label>
          <input
            type="text"
            value={subject}
            onChange={(e) => setSubject(e.target.value)}
            placeholder="e.g. Discovery call regarding enterprise cloud migration"
            className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
            Detailed Notes / Discussion Points
          </label>
          <textarea
            rows={3}
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="Key objections raised, scope discussed, next action items..."
            className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-xs font-medium text-slate-700 dark:text-slate-300 mb-1">
            Scheduled / Performed Date
          </label>
          <input
            type="datetime-local"
            value={scheduledAt}
            onChange={(e) => setScheduledAt(e.target.value)}
            className="w-full px-3 py-2 border rounded-lg text-sm bg-white dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div className="flex justify-end gap-3 pt-3 border-t border-slate-100 dark:border-slate-800">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={isSubmitting}
            className="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 rounded-lg flex items-center gap-2 transition-colors shadow-sm"
          >
            {isSubmitting ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin" />
                <span>Recording...</span>
              </>
            ) : (
              <span>Save Activity</span>
            )}
          </button>
        </div>
      </form>
    </Modal>
  );
};
