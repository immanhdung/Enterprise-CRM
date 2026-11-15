import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Building2,
  Plus,
  Search,
  Eye,
  Trash2,
  RefreshCw,
  AlertCircle,
  CheckCircle,
  Briefcase,
  Users,
} from 'lucide-react';
import { accountService } from '../../services/accountService';
import { employeeService, type EmployeeSimpleDto } from '../../services/employeeService';
import { CreateAccountModal } from './components/CreateAccountModal';
import { StatusBadge } from '../../components/common/StatusBadge';
import type { AccountDto, PagedResult } from '../../types/crm';

export const AccountsPage: React.FC = () => {
  const navigate = useNavigate();

  const [data, setData] = useState<PagedResult<AccountDto>>({
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 10,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  });
  const [employees, setEmployees] = useState<EmployeeSimpleDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [notification, setNotification] = useState<string | null>(null);

  // Filters
  const [search, setSearch] = useState('');
  const [classification, setClassification] = useState('');
  const [status, setStatus] = useState('');
  const [industry, setIndustry] = useState('');
  const [assignedEmployeeId, setAssignedEmployeeId] = useState('');
  const [pageNumber, setPageNumber] = useState(1);
  const [isCreateOpen, setIsCreateOpen] = useState(false);

  const fetchAccounts = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const res = await accountService.getAccounts({
        pageNumber,
        pageSize: 10,
        search: search.trim() || undefined,
        classification: classification || undefined,
        status: status || undefined,
        industry: industry || undefined,
        assignedEmployeeId: assignedEmployeeId || undefined,
      });

      if (res.success && res.data) {
        setData(res.data);
      } else {
        setError(res.message || 'Failed to fetch accounts.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error loading accounts.');
    } finally {
      setIsLoading(false);
    }
  }, [pageNumber, search, classification, status, industry, assignedEmployeeId]);

  useEffect(() => {
    fetchAccounts();
  }, [fetchAccounts]);

  useEffect(() => {
    employeeService.getEmployees().then((res) => {
      if (res.data) setEmployees(res.data);
    }).catch(console.error);
  }, []);

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`Are you sure you want to delete account "${name}"?`)) return;
    try {
      const res = await accountService.deleteAccount(id);
      if (res.success) {
        setNotification(`Account "${name}" deleted.`);
        fetchAccounts();
        setTimeout(() => setNotification(null), 3000);
      } else {
        setError(res.message || 'Failed to delete account.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error deleting account.');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white">
            Enterprise Accounts
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400">
            Manage client corporate entities, service contracts, projects, and key relationships.
          </p>
        </div>

        <button
          onClick={() => setIsCreateOpen(true)}
          className="px-3.5 py-2 text-xs font-semibold text-white bg-blue-600 hover:bg-blue-700 rounded-lg flex items-center gap-1.5 shadow-sm transition-colors"
        >
          <Plus className="w-4 h-4" />
          New Account
        </button>
      </div>

      {/* Notifications */}
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

      {/* Search and Filters */}
      <div className="p-4 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-3">
        <div className="flex flex-col md:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-2.5" />
            <input
              type="text"
              placeholder="Search accounts by name, code, tax ID, email, website..."
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
              value={classification}
              onChange={(e) => {
                setClassification(e.target.value);
                setPageNumber(1);
              }}
              className="px-2.5 py-2 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
            >
              <option value="">All Tiers</option>
              <option value="VIP">VIP</option>
              <option value="Enterprise">Enterprise</option>
              <option value="Standard">Standard</option>
              <option value="SMB">SMB</option>
            </select>

            <select
              value={industry}
              onChange={(e) => {
                setIndustry(e.target.value);
                setPageNumber(1);
              }}
              className="px-2.5 py-2 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
            >
              <option value="">All Industries</option>
              <option value="Technology">Technology</option>
              <option value="Finance">Finance</option>
              <option value="Healthcare">Healthcare</option>
              <option value="Manufacturing">Manufacturing</option>
              <option value="Retail">Retail</option>
              <option value="Consulting">Consulting</option>
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
              <option value="Active">Active</option>
              <option value="Inactive">Inactive</option>
            </select>

            <select
              value={assignedEmployeeId}
              onChange={(e) => {
                setAssignedEmployeeId(e.target.value);
                setPageNumber(1);
              }}
              className="px-2.5 py-2 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
            >
              <option value="">All Managers</option>
              {employees.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.fullName}
                </option>
              ))}
            </select>

            <button
              onClick={() => {
                setSearch('');
                setClassification('');
                setStatus('');
                setIndustry('');
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

      {/* Accounts Table */}
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs text-slate-600 dark:text-slate-300">
            <thead className="bg-slate-50 dark:bg-slate-900/50 text-[11px] uppercase tracking-wider text-slate-500 dark:text-slate-400 border-b border-slate-200 dark:border-slate-700">
              <tr>
                <th className="px-4 py-3 font-semibold">Account / Code</th>
                <th className="px-4 py-3 font-semibold">Industry</th>
                <th className="px-4 py-3 font-semibold">Classification</th>
                <th className="px-4 py-3 font-semibold">Status</th>
                <th className="px-4 py-3 font-semibold">Contacts</th>
                <th className="px-4 py-3 font-semibold">Deals</th>
                <th className="px-4 py-3 font-semibold">Pipeline Value</th>
                <th className="px-4 py-3 font-semibold">Account Manager</th>
                <th className="px-4 py-3 font-semibold text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-700/60">
              {isLoading ? (
                <tr>
                  <td colSpan={9} className="py-12 text-center text-slate-400">
                    <RefreshCw className="w-6 h-6 animate-spin mx-auto mb-2 text-blue-500" />
                    <span>Loading corporate accounts...</span>
                  </td>
                </tr>
              ) : data.items.length === 0 ? (
                <tr>
                  <td colSpan={9} className="py-12 text-center text-slate-400">
                    <Building2 className="w-8 h-8 mx-auto mb-2 opacity-40" />
                    <p className="font-medium text-slate-600 dark:text-slate-300">No accounts found</p>
                    <p className="text-xs text-slate-400 mt-0.5">Create your first corporate account or adjust filters.</p>
                  </td>
                </tr>
              ) : (
                data.items.map((acc) => (
                  <tr
                    key={acc.id}
                    className="hover:bg-slate-50/70 dark:hover:bg-slate-700/30 transition-colors"
                  >
                    <td className="px-4 py-3">
                      <button
                        onClick={() => navigate(`/accounts/${acc.id}`)}
                        className="font-medium text-slate-900 dark:text-white hover:text-blue-600 dark:hover:text-blue-400 text-left flex items-center gap-1.5"
                      >
                        <Building2 className="w-3.5 h-3.5 text-blue-500 shrink-0" />
                        <span>{acc.name}</span>
                      </button>
                      <div className="text-[11px] text-slate-400">{acc.code}</div>
                    </td>
                    <td className="px-4 py-3 text-slate-700 dark:text-slate-300 font-medium">
                      {acc.industry || '—'}
                    </td>
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold ${
                          acc.classification === 'VIP'
                            ? 'bg-purple-100 text-purple-700 dark:bg-purple-950/60 dark:text-purple-400'
                            : acc.classification === 'Enterprise'
                            ? 'bg-blue-100 text-blue-700 dark:bg-blue-950/60 dark:text-blue-400'
                            : 'bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-400'
                        }`}
                      >
                        {acc.classification || 'Standard'}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <StatusBadge status={acc.status} />
                    </td>
                    <td className="px-4 py-3">
                      <span className="flex items-center gap-1 text-slate-700 dark:text-slate-300">
                        <Users className="w-3.5 h-3.5 text-slate-400" />
                        {acc.contactsCount}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <span className="flex items-center gap-1 text-slate-700 dark:text-slate-300">
                        <Briefcase className="w-3.5 h-3.5 text-slate-400" />
                        {acc.dealsCount}
                      </span>
                    </td>
                    <td className="px-4 py-3 font-semibold text-emerald-600 dark:text-emerald-400">
                      {acc.totalDealsValue
                        ? `$${Number(acc.totalDealsValue).toLocaleString()}`
                        : '$0'}
                    </td>
                    <td className="px-4 py-3">
                      {acc.assignedEmployeeName ? (
                        <span className="font-medium text-slate-700 dark:text-slate-300">
                          {acc.assignedEmployeeName}
                        </span>
                      ) : (
                        <span className="text-slate-400 italic">Unassigned</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex items-center justify-end gap-1">
                        <button
                          title="View Account Profile"
                          onClick={() => navigate(`/accounts/${acc.id}`)}
                          className="p-1.5 text-slate-500 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-slate-100 dark:hover:bg-slate-700 rounded transition-colors"
                        >
                          <Eye className="w-3.5 h-3.5" />
                        </button>
                        <button
                          title="Delete Account"
                          onClick={() => handleDelete(acc.id, acc.name)}
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
            <span className="font-semibold">{data.totalCount}</span> total accounts
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

      <CreateAccountModal
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        onSuccess={() => {
          setNotification('Account created successfully.');
          fetchAccounts();
          setTimeout(() => setNotification(null), 3000);
        }}
      />
    </div>
  );
};
