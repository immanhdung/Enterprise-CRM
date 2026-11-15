import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import dayjs from 'dayjs';
import {
  ArrowLeft,
  Building2,
  Globe,
  Mail,
  Phone,
  MapPin,
  Users,
  Briefcase,
  FileText,
  FolderGit2,
  Clock,
  ExternalLink,
  Plus,
} from 'lucide-react';
import { accountService } from '../../services/accountService';
import { StatusBadge } from '../../components/common/StatusBadge';
import { ActivityTimeline } from '../../components/common/ActivityTimeline';
import type { AccountDetailDto } from '../../types/crm';

export const AccountDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [account, setAccount] = useState<AccountDetailDto | null>(null);
  const [activeTab, setActiveTab] = useState<
    'overview' | 'contacts' | 'deals' | 'activities' | 'contracts' | 'projects' | 'timeline'
  >('overview');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadAccount = useCallback(async () => {
    if (!id) return;
    try {
      setIsLoading(true);
      setError(null);
      const res = await accountService.getAccountById(id);
      if (res.success && res.data) {
        setAccount(res.data);
      } else {
        setError(res.message || 'Account not found.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error loading account details.');
    } finally {
      setIsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    loadAccount();
  }, [loadAccount]);

  if (isLoading) {
    return (
      <div className="py-20 text-center text-slate-400">
        <Clock className="w-8 h-8 animate-spin mx-auto mb-3 text-blue-500" />
        <p>Loading enterprise account profile...</p>
      </div>
    );
  }

  if (error || !account) {
    return (
      <div className="space-y-4">
        <button
          onClick={() => navigate('/accounts')}
          className="text-xs text-slate-500 hover:text-slate-800 flex items-center gap-1"
        >
          <ArrowLeft className="w-3.5 h-3.5" /> Back to Accounts
        </button>
        <div className="p-4 bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 rounded-lg text-sm text-red-600 dark:text-red-400">
          {error || 'Account could not be loaded.'}
        </div>
      </div>
    );
  }

  const tabs = [
    { id: 'overview', label: '1. Overview', icon: Building2 },
    { id: 'contacts', label: `2. Contacts (${account.contacts.length})`, icon: Users },
    { id: 'deals', label: `3. Deals (${account.deals.length})`, icon: Briefcase },
    { id: 'activities', label: `4. Activities (${account.activities.length})`, icon: FileText },
    { id: 'contracts', label: `5. Contracts (${account.contracts.length})`, icon: FileText },
    { id: 'projects', label: `6. Projects (${account.projects.length})`, icon: FolderGit2 },
    { id: 'timeline', label: `7. Timeline (${account.timeline.length})`, icon: Clock },
  ] as const;

  return (
    <div className="space-y-6">
      {/* Header bar */}
      <div className="flex items-center justify-between">
        <button
          onClick={() => navigate('/accounts')}
          className="text-xs text-slate-500 hover:text-slate-800 dark:hover:text-slate-200 flex items-center gap-1 transition-colors"
        >
          <ArrowLeft className="w-3.5 h-3.5" /> Back to Accounts
        </button>
      </div>

      {/* Main Account Banner */}
      <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div className="flex items-start gap-4">
            <div className="w-12 h-12 rounded-xl bg-blue-50 dark:bg-blue-950/50 border border-blue-200 dark:border-blue-800 flex items-center justify-center shrink-0">
              <Building2 className="w-6 h-6 text-blue-600 dark:text-blue-400" />
            </div>
            <div>
              <div className="flex flex-wrap items-center gap-3">
                <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
                  {account.name}
                </h1>
                <StatusBadge status={account.status} />
                <span className="text-xs px-2.5 py-0.5 rounded-full font-medium bg-purple-50 border border-purple-200 text-purple-700 dark:bg-purple-950/40 dark:border-purple-800 dark:text-purple-300">
                  {account.classification || 'Standard'}
                </span>
              </div>
              <div className="flex flex-wrap items-center gap-4 mt-2 text-xs text-slate-500 dark:text-slate-400">
                <span>Code: {account.code}</span>
                <span>• Industry: {account.industry || 'General'}</span>
                {account.taxId && <span>• Tax ID: {account.taxId}</span>}
                {account.website && (
                  <a
                    href={account.website.startsWith('http') ? account.website : `https://${account.website}`}
                    target="_blank"
                    rel="noreferrer"
                    className="flex items-center gap-1 text-blue-600 hover:underline"
                  >
                    <Globe className="w-3 h-3" />
                    {account.website}
                    <ExternalLink className="w-2.5 h-2.5" />
                  </a>
                )}
              </div>
            </div>
          </div>

          <div className="text-right">
            <div className="text-xs text-slate-400">Account Manager</div>
            <div className="font-semibold text-sm text-slate-800 dark:text-slate-200">
              {account.assignedEmployeeName || 'Unassigned'}
            </div>
          </div>
        </div>

        {/* 7 Tabs Navigation */}
        <div className="flex flex-wrap gap-1 border-b border-slate-200 dark:border-slate-700 mt-6 pt-2">
          {tabs.map((tab) => {
            const Icon = tab.icon;
            const isActive = activeTab === tab.id;
            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id as typeof activeTab)}
                className={`flex items-center gap-1.5 px-4 py-2.5 text-xs font-semibold border-b-2 transition-all ${
                  isActive
                    ? 'border-blue-600 text-blue-600 dark:text-blue-400 bg-blue-50/50 dark:bg-blue-950/20'
                    : 'border-transparent text-slate-500 hover:text-slate-700 dark:hover:text-slate-300 hover:border-slate-300'
                }`}
              >
                <Icon className="w-3.5 h-3.5" />
                <span>{tab.label}</span>
              </button>
            );
          })}
        </div>
      </div>

      {/* Tab 1: Overview */}
      {activeTab === 'overview' && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="md:col-span-2 space-y-6">
            <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-4">
              <h3 className="text-sm font-bold text-slate-900 dark:text-white">
                About Organization
              </h3>
              <p className="text-xs text-slate-600 dark:text-slate-300 leading-relaxed whitespace-pre-line">
                {account.description || 'No detailed description recorded for this account.'}
              </p>

              <div className="pt-4 border-t border-slate-100 dark:border-slate-700 grid grid-cols-2 gap-4 text-xs">
                <div>
                  <div className="text-slate-400 mb-1 flex items-center gap-1">
                    <Mail className="w-3.5 h-3.5" /> Primary Email
                  </div>
                  <div className="font-medium text-slate-800 dark:text-slate-200">
                    {account.email || '—'}
                  </div>
                </div>
                <div>
                  <div className="text-slate-400 mb-1 flex items-center gap-1">
                    <Phone className="w-3.5 h-3.5" /> Phone Number
                  </div>
                  <div className="font-medium text-slate-800 dark:text-slate-200">
                    {account.phone || '—'}
                  </div>
                </div>
                <div className="col-span-2">
                  <div className="text-slate-400 mb-1 flex items-center gap-1">
                    <MapPin className="w-3.5 h-3.5" /> Headquarters Address
                  </div>
                  <div className="font-medium text-slate-800 dark:text-slate-200">
                    {[account.address, account.city, account.country].filter(Boolean).join(', ') || '—'}
                  </div>
                </div>
              </div>
            </div>

            {/* Recent Deals snippet */}
            <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-3">
              <h3 className="text-sm font-bold text-slate-900 dark:text-white flex items-center justify-between">
                <span>Active Business Deals</span>
                <button
                  onClick={() => setActiveTab('deals')}
                  className="text-xs text-blue-600 font-normal hover:underline"
                >
                  View all ({account.deals.length})
                </button>
              </h3>
              {account.deals.length === 0 ? (
                <p className="text-xs text-slate-400 py-3">No deals associated with this account yet.</p>
              ) : (
                <div className="divide-y divide-slate-100 dark:divide-slate-700">
                  {account.deals.slice(0, 3).map((deal) => (
                    <div key={deal.id} className="py-2.5 flex items-center justify-between text-xs">
                      <div>
                        <div className="font-semibold text-slate-800 dark:text-slate-200">{deal.title}</div>
                        <div className="text-slate-400 text-[11px]">{deal.stageName} • {deal.code}</div>
                      </div>
                      <div className="text-right">
                        <div className="font-semibold text-emerald-600">${Number(deal.amount).toLocaleString()}</div>
                        <div className="text-slate-400 text-[10px]">Exp: ${Number(deal.expectedRevenue).toLocaleString()}</div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>

          <div className="space-y-6">
            <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-3">
              <h3 className="text-sm font-bold text-slate-900 dark:text-white">
                Account Metrics
              </h3>
              <div className="space-y-3 text-xs">
                <div className="flex justify-between py-1.5 border-b border-slate-100 dark:border-slate-700">
                  <span className="text-slate-500">Total Key Contacts</span>
                  <span className="font-bold text-slate-800 dark:text-slate-200">{account.contacts.length}</span>
                </div>
                <div className="flex justify-between py-1.5 border-b border-slate-100 dark:border-slate-700">
                  <span className="text-slate-500">Pipeline Opportunities</span>
                  <span className="font-bold text-slate-800 dark:text-slate-200">{account.deals.length}</span>
                </div>
                <div className="flex justify-between py-1.5 border-b border-slate-100 dark:border-slate-700">
                  <span className="text-slate-500">Active Service Contracts</span>
                  <span className="font-bold text-slate-800 dark:text-slate-200">{account.contracts.length}</span>
                </div>
                <div className="flex justify-between py-1.5 border-b border-slate-100 dark:border-slate-700">
                  <span className="text-slate-500">Delivery Projects</span>
                  <span className="font-bold text-slate-800 dark:text-slate-200">{account.projects.length}</span>
                </div>
                <div className="flex justify-between py-1.5">
                  <span className="text-slate-500">Customer Since</span>
                  <span className="font-medium text-slate-700 dark:text-slate-300">
                    {dayjs(account.createdAt).format('MMM D, YYYY')}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Tab 2: Contacts */}
      {activeTab === 'contacts' && (
        <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-4">
          <div className="flex items-center justify-between">
            <h3 className="text-base font-bold text-slate-900 dark:text-white">
              Associated Corporate Contacts
            </h3>
            <button
              onClick={() => navigate('/contacts')}
              className="text-xs px-3 py-1.5 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 flex items-center gap-1"
            >
              <Plus className="w-3.5 h-3.5" />
              Manage Contacts
            </button>
          </div>

          {account.contacts.length === 0 ? (
            <p className="text-xs text-slate-400 py-8 text-center">No contacts associated with this account.</p>
          ) : (
            <div className="divide-y divide-slate-100 dark:divide-slate-700">
              {account.contacts.map((c) => (
                <div key={c.id} className="py-3 flex items-center justify-between text-xs">
                  <div>
                    <div className="font-semibold text-slate-900 dark:text-white flex items-center gap-2">
                      <span>{c.fullName}</span>
                      {c.isPrimary && (
                        <span className="px-1.5 py-0.5 rounded text-[10px] bg-blue-100 text-blue-700 dark:bg-blue-950/60 dark:text-blue-300 font-semibold">
                          Primary Contact
                        </span>
                      )}
                    </div>
                    <div className="text-slate-400 mt-0.5">{c.position || 'Contact'}</div>
                  </div>
                  <div className="text-right text-slate-600 dark:text-slate-300">
                    <div>{c.email || 'No email'}</div>
                    <div className="text-slate-400">{c.phone || ''}</div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Tab 3: Deals */}
      {activeTab === 'deals' && (
        <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-4">
          <div className="flex items-center justify-between">
            <h3 className="text-base font-bold text-slate-900 dark:text-white">
              Opportunities & Deals
            </h3>
            <button
              onClick={() => navigate('/deals')}
              className="text-xs px-3 py-1.5 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 flex items-center gap-1"
            >
              <Plus className="w-3.5 h-3.5" />
              Manage Deals
            </button>
          </div>

          {account.deals.length === 0 ? (
            <p className="text-xs text-slate-400 py-8 text-center">No deals in pipeline for this account.</p>
          ) : (
            <div className="divide-y divide-slate-100 dark:divide-slate-700">
              {account.deals.map((d) => (
                <div key={d.id} className="py-3 flex items-center justify-between text-xs">
                  <div>
                    <div className="font-semibold text-slate-900 dark:text-white">{d.title}</div>
                    <div className="text-slate-400 mt-0.5">
                      Code: {d.code} • Stage: <span className="font-medium text-slate-700 dark:text-slate-300">{d.stageName}</span>
                    </div>
                  </div>
                  <div className="text-right">
                    <div className="font-bold text-emerald-600 text-sm">
                      ${Number(d.amount).toLocaleString()}
                    </div>
                    <div className="text-slate-400 text-[11px]">
                      Expected: ${Number(d.expectedRevenue).toLocaleString()}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Tab 4: Activities */}
      {activeTab === 'activities' && (
        <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-4">
          <h3 className="text-base font-bold text-slate-900 dark:text-white">
            Customer Activities
          </h3>
          <ActivityTimeline events={account.activities} emptyMessage="No activities recorded for this account." />
        </div>
      )}

      {/* Tab 5: Contracts */}
      {activeTab === 'contracts' && (
        <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-4">
          <h3 className="text-base font-bold text-slate-900 dark:text-white">
            Service Contracts
          </h3>
          {account.contracts.length === 0 ? (
            <p className="text-xs text-slate-400 py-8 text-center">No active contracts found for this account.</p>
          ) : (
            <div className="divide-y divide-slate-100 dark:divide-slate-700">
              {account.contracts.map((c) => (
                <div key={c.id} className="py-3 flex items-center justify-between text-xs">
                  <div>
                    <div className="font-semibold text-slate-900 dark:text-white">{c.title}</div>
                    <div className="text-slate-400 mt-0.5">
                      Number: {c.contractNumber} • Period: {dayjs(c.startDate).format('MMM YYYY')} -{' '}
                      {c.endDate ? dayjs(c.endDate).format('MMM YYYY') : 'Ongoing'}
                    </div>
                  </div>
                  <div className="text-right">
                    <div className="font-bold text-slate-900 dark:text-white">
                      ${Number(c.totalValue).toLocaleString()}
                    </div>
                    <StatusBadge status={c.status} />
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Tab 6: Projects */}
      {activeTab === 'projects' && (
        <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-4">
          <h3 className="text-base font-bold text-slate-900 dark:text-white">
            Delivery Projects
          </h3>
          {account.projects.length === 0 ? (
            <p className="text-xs text-slate-400 py-8 text-center">No delivery projects linked to this account.</p>
          ) : (
            <div className="space-y-4">
              {account.projects.map((p) => (
                <div key={p.id} className="p-4 rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50/50 dark:bg-slate-900/30 text-xs space-y-2">
                  <div className="flex items-center justify-between">
                    <div>
                      <div className="font-bold text-slate-900 dark:text-white text-sm">{p.name}</div>
                      <div className="text-slate-400">Code: {p.code}</div>
                    </div>
                    <StatusBadge status={p.status} />
                  </div>
                  <div className="space-y-1">
                    <div className="flex justify-between text-[11px] text-slate-500">
                      <span>Progress</span>
                      <span>{p.progressPercent}%</span>
                    </div>
                    <div className="w-full bg-slate-200 dark:bg-slate-700 h-2 rounded-full overflow-hidden">
                      <div
                        className="bg-blue-600 h-2 rounded-full transition-all"
                        style={{ width: `${p.progressPercent}%` }}
                      />
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Tab 7: Timeline */}
      {activeTab === 'timeline' && (
        <div className="p-6 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-4">
          <h3 className="text-base font-bold text-slate-900 dark:text-white">
            Full Account Audit & Event Timeline
          </h3>
          <ActivityTimeline events={account.timeline} emptyMessage="No timeline events recorded." />
        </div>
      )}
    </div>
  );
};
