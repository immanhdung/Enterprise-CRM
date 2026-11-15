import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Briefcase,
  Plus,
  Search,
  Building2,
  Trash2,
  RefreshCw,
  AlertCircle,
  CheckCircle,
  Kanban,
  Eye,
} from 'lucide-react';
import { dealService } from '../../services/dealService';
import { accountService } from '../../services/accountService';
import { employeeService, type EmployeeSimpleDto } from '../../services/employeeService';
import { CreateDealModal } from './components/CreateDealModal';
import { StatusBadge } from '../../components/common/StatusBadge';
import type { DealDto, PagedResult, AccountDto } from '../../types/crm';

export const DealsPage: React.FC = () => {
  const navigate = useNavigate();

  const [data, setData] = useState<PagedResult<DealDto>>({
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 10,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  });
  const [employees, setEmployees] = useState<EmployeeSimpleDto[]>([]);
  const [accounts, setAccounts] = useState<AccountDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [notification, setNotification] = useState<string | null>(null);

  // Filters
  const [search, setSearch] = useState('');
  const [accountId, setAccountId] = useState('');
  const [status, setStatus] = useState('');
  const [assignedEmployeeId, setAssignedEmployeeId] = useState('');
  const [pageNumber, setPageNumber] = useState(1);
  const [isCreateOpen, setIsCreateOpen] = useState(false);

  const fetchDeals = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const res = await dealService.getDeals({
        pageNumber,
        pageSize: 10,
        search: search.trim() || undefined,
        accountId: accountId || undefined,
        status: status || undefined,
        assignedEmployeeId: assignedEmployeeId || undefined,
      });

      if (res.success && res.data) {
        setData(res.data);
      } else {
        setError(res.message || 'Failed to fetch deals.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error loading deals.');
    } finally {
      setIsLoading(false);
    }
  }, [pageNumber, search, accountId, status, assignedEmployeeId]);

  useEffect(() => {
    fetchDeals();
  }, [fetchDeals]);

  useEffect(() => {
    employeeService.getEmployees().then((res) => {
      if (res.data) setEmployees(res.data);
    }).catch(console.error);

    accountService.getAccounts({ pageSize: 50 }).then((res) => {
      if (res.data?.items) setAccounts(res.data.items);
    }).catch(console.error);
  }, []);

  const handleDelete = async (id: string, title: string) => {
    if (!window.confirm(`Are you sure you want to delete deal "${title}"?`)) return;
    try {
      const res = await dealService.deleteDeal(id);
      if (res.success) {
        setNotification(`Deal "${title}" deleted.`);
        fetchDeals();
        setTimeout(() => setNotification(null), 3000);
      } else {
        setError(res.message || 'Failed to delete deal.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error deleting deal.');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white">
            Deals & Opportunities
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400">
            Pipeline deal records, commercial contracts, and expected revenue tracking.
          </p>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={() => navigate('/pipeline')}
            className="px-3 py-2 text-xs font-medium text-slate-700 dark:text-slate-200 bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 flex items-center gap-1.5 transition-colors"
          >
            <Kanban className="w-3.5 h-3.5 text-blue-600" />
            Kanban Board
          </button>

          <button
            onClick={() => setIsCreateOpen(true)}
            className="px-3.5 py-2 text-xs font-semibold text-white bg-blue-600 hover:bg-blue-700 rounded-lg flex items-center gap-1.5 shadow-sm transition-colors"
          >
            <Plus className="w-4 h-4" />
            New Deal
          </button>
        </div>
      </div>

      {notification && (
        <div className="p-3 bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 dark:border-emerald-800 rounded-lg text-xs text-emerald-800 dark:text-emerald-300 flex items-center gap-2">
          <CheckCircle className="w-4 h-4 shrink-0 text-emerald-600" />
          <span>{notification}</span>
        </div>
      )}

      {error && (
        <div className="p-3 bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 rounded-lg text-xs text-red-600 dark:text-red-400 flex items-center gap-2">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      {/* Filter Bar */}
      <div className="p-4 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-3">
        <div className="flex flex-col md:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-2.5" />
            <input
              type="text"
              placeholder="Search deals by title, code, client name..."
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setPageNumber(1);
              }}
              className="w-full pl-9 pr-3 py-2 border rounded-lg text-xs bg-slate-50 dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-900 dark:text-white focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div className="flex flex-wrap items-center gap-2">
            <select
              value={accountId}
              onChange={(e) => {
                setAccountId(e.target.value);
                setPageNumber(1);
              }}
              className="px-2.5 py-2 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
            >
              <option value="">All Accounts</option>
              {accounts.map((acc) => (
                <option key={acc.id} value={acc.id}>
                  {acc.name}
                </option>
              ))}
            </select>

            <select
              value={status}
              onChange={(e) => {
                setStatus(e.target.value);
                setPageNumber(1);
              }}
              className="px-2.5 py-2 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
            >
              <option value="">All Statuses</option>
              <option value="Open">Open</option>
              <option value="Won">Won</option>
              <option value="Lost">Lost</option>
            </select>

            <select
              value={assignedEmployeeId}
              onChange={(e) => {
                setAssignedEmployeeId(e.target.value);
                setPageNumber(1);
              }}
              className="px-2.5 py-2 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
            >
              <option value="">All Representatives</option>
              {employees.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.fullName}
                </option>
              ))}
            </select>

            <button
              onClick={() => {
                setSearch('');
                setAccountId('');
                setStatus('');
                setAssignedEmployeeId('');
                setPageNumber(1);
              }}
              className="px-2.5 py-2 text-xs text-slate-500 hover:text-slate-700 dark:hover:text-slate-300 transition-colors"
            >
              Reset
            </button>
          </div>
        </div>
      </div>

      {/* Deals Table */}
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs text-slate-600 dark:text-slate-300">
            <thead className="bg-slate-50 dark:bg-slate-900/50 text-[11px] uppercase tracking-wider text-slate-500 dark:text-slate-400 border-b border-slate-200 dark:border-slate-700">
              <tr>
                <th className="px-4 py-3 font-semibold">Deal Title / Code</th>
                <th className="px-4 py-3 font-semibold">Account & Contact</th>
                <th className="px-4 py-3 font-semibold">Pipeline Stage</th>
                <th className="px-4 py-3 font-semibold">Amount</th>
                <th className="px-4 py-3 font-semibold">Probability</th>
                <th className="px-4 py-3 font-semibold">Expected Revenue</th>
                <th className="px-4 py-3 font-semibold">Status</th>
                <th className="px-4 py-3 font-semibold">Assigned Rep</th>
                <th className="px-4 py-3 font-semibold text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-700/60">
              {isLoading ? (
                <tr>
                  <td colSpan={9} className="py-12 text-center text-slate-400">
                    <RefreshCw className="w-6 h-6 animate-spin mx-auto mb-2 text-blue-500" />
                    <span>Loading opportunities...</span>
                  </td>
                </tr>
              ) : data.items.length === 0 ? (
                <tr>
                  <td colSpan={9} className="py-12 text-center text-slate-400">
                    <Briefcase className="w-8 h-8 mx-auto mb-2 opacity-40" />
                    <p className="font-medium text-slate-600 dark:text-slate-300">No deals found</p>
                    <p className="text-xs text-slate-400 mt-0.5">Register a deal or switch filters.</p>
                  </td>
                </tr>
              ) : (
                data.items.map((deal) => (
                  <tr
                    key={deal.id}
                    className="hover:bg-slate-50/70 dark:hover:bg-slate-700/30 transition-colors"
                  >
                    <td className="px-4 py-3">
                      <button
                        onClick={() => navigate(`/deals/${deal.id}`)}
                        className="font-semibold text-slate-900 dark:text-white hover:text-blue-600 dark:hover:text-blue-400 text-left"
                      >
                        {deal.title}
                      </button>
                      <div className="text-[11px] text-slate-400">{deal.code}</div>
                    </td>
                    <td className="px-4 py-3">
                      <button
                        onClick={() => navigate(`/accounts/${deal.accountId}`)}
                        className="font-medium text-blue-600 dark:text-blue-400 hover:underline flex items-center gap-1"
                      >
                        <Building2 className="w-3.5 h-3.5 shrink-0" />
                        <span>{deal.accountName}</span>
                      </button>
                      {deal.contactName && (
                        <div className="text-[11px] text-slate-400">{deal.contactName}</div>
                      )}
                    </td>
                    <td className="px-4 py-3">
                      <span className="inline-flex items-center gap-1.5 font-medium text-slate-800 dark:text-slate-200">
                        <span
                          className="w-2 h-2 rounded-full"
                          style={{ backgroundColor: deal.stageColor || '#3b82f6' }}
                        />
                        {deal.stageName}
                      </span>
                    </td>
                    <td className="px-4 py-3 font-bold text-slate-900 dark:text-white">
                      ${Number(deal.amount).toLocaleString()}
                    </td>
                    <td className="px-4 py-3 font-medium text-slate-600 dark:text-slate-400">
                      {deal.effectiveProbability}%
                    </td>
                    <td className="px-4 py-3 font-semibold text-emerald-600 dark:text-emerald-400">
                      ${Number(deal.expectedRevenue).toLocaleString()}
                    </td>
                    <td className="px-4 py-3">
                      <StatusBadge status={deal.status} />
                    </td>
                    <td className="px-4 py-3">
                      {deal.assignedEmployeeName ? (
                        <span className="font-medium text-slate-700 dark:text-slate-300">
                          {deal.assignedEmployeeName}
                        </span>
                      ) : (
                        <span className="text-slate-400 italic">Unassigned</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex items-center justify-end gap-1">
                        <button
                          title="View Deal"
                          onClick={() => navigate(`/deals/${deal.id}`)}
                          className="p-1.5 text-slate-500 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-slate-100 dark:hover:bg-slate-700 rounded transition-colors"
                        >
                          <Eye className="w-3.5 h-3.5" />
                        </button>
                        <button
                          title="Delete Deal"
                          onClick={() => handleDelete(deal.id, deal.title)}
                          className="p-1.5 text-slate-500 hover:text-red-600 dark:hover:text-red-400 hover:bg-slate-100 dark:hover:bg-slate-700 rounded transition-colors"
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination Footer */}
        <div className="px-4 py-3 bg-slate-50 dark:bg-slate-900/40 border-t border-slate-200 dark:border-slate-700/60 flex items-center justify-between text-xs text-slate-500">
          <div>
            Showing <span className="font-semibold">{data.items.length}</span> of{' '}
            <span className="font-semibold">{data.totalCount}</span> total deals
          </div>

          <div className="flex items-center gap-2">
            <button
              disabled={!data.hasPreviousPage}
              onClick={() => setPageNumber(pageNumber - 1)}
              className="px-3 py-1.5 rounded border border-slate-300 dark:border-slate-700 disabled:opacity-40 hover:bg-white dark:hover:bg-slate-800 transition-colors"
            >
              Previous
            </button>
            <span className="font-medium text-slate-700 dark:text-slate-300">
              Page {data.pageNumber} of {data.totalPages || 1}
            </span>
            <button
              disabled={!data.hasNextPage}
              onClick={() => setPageNumber(pageNumber + 1)}
              className="px-3 py-1.5 rounded border border-slate-300 dark:border-slate-700 disabled:opacity-40 hover:bg-white dark:hover:bg-slate-800 transition-colors"
            >
              Next
            </button>
          </div>
        </div>
      </div>

      <CreateDealModal
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        onSuccess={() => {
          setNotification('Deal created successfully.');
          fetchDeals();
          setTimeout(() => setNotification(null), 3000);
        }}
      />
    </div>
  );
};
