import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import dayjs from 'dayjs';
import {
  Plus,
  Search,
  Download,
  Upload,
  UserCheck,
  ArrowRightLeft,
  Eye,
  Trash2,
  RefreshCw,
  AlertCircle,
  FileSpreadsheet,
  CheckCircle,
} from 'lucide-react';
import { leadService } from '../../services/leadService';
import { employeeService, type EmployeeSimpleDto } from '../../services/employeeService';
import { CreateLeadModal } from './components/CreateLeadModal';
import { AssignLeadModal } from './components/AssignLeadModal';
import { ConvertLeadModal } from './components/ConvertLeadModal';
import { StatusBadge } from '../../components/common/StatusBadge';
import type { LeadDto, PagedResult } from '../../types/crm';

export const LeadsPage: React.FC = () => {
  const navigate = useNavigate();
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Data state
  const [data, setData] = useState<PagedResult<LeadDto>>({
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

  // Filters & Sorting
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [source, setSource] = useState('');
  const [priority, setPriority] = useState('');
  const [assignedEmployeeId, setAssignedEmployeeId] = useState('');
  const [sortBy, setSortBy] = useState('CreatedAt');
  const [sortDescending, setSortDescending] = useState(true);
  const [pageNumber, setPageNumber] = useState(1);

  // Modals state
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [assignLead, setAssignLead] = useState<LeadDto | null>(null);
  const [convertLead, setConvertLead] = useState<LeadDto | null>(null);

  const fetchLeads = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const res = await leadService.getLeads({
        pageNumber,
        pageSize: 10,
        search: search.trim() || undefined,
        status: status || undefined,
        source: source || undefined,
        priority: priority || undefined,
        assignedEmployeeId: assignedEmployeeId || undefined,
        sortBy,
        sortDescending,
      });

      if (res.success && res.data) {
        setData(res.data);
      } else {
        setError(res.message || 'Failed to load leads.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error fetching leads.');
    } finally {
      setIsLoading(false);
    }
  }, [pageNumber, search, status, source, priority, assignedEmployeeId, sortBy, sortDescending]);

  useEffect(() => {
    fetchLeads();
  }, [fetchLeads]);

  useEffect(() => {
    employeeService.getEmployees().then((res) => {
      if (res.data) setEmployees(res.data);
    }).catch(console.error);
  }, []);

  const handleSort = (column: string) => {
    if (sortBy === column) {
      setSortDescending(!sortDescending);
    } else {
      setSortBy(column);
      setSortDescending(false);
    }
  };

  const handleExport = async () => {
    try {
      setNotification('Exporting leads data to CSV...');
      const blob = await leadService.exportLeads({
        search: search || undefined,
        status: status || undefined,
        source: source || undefined,
        assignedEmployeeId: assignedEmployeeId || undefined,
      });

      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Leads_Export_${dayjs().format('YYYYMMDD_HHmm')}.csv`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
      setNotification('Leads successfully exported.');
      setTimeout(() => setNotification(null), 3000);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error exporting leads.');
    }
  };

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    try {
      setNotification('Importing leads from CSV...');
      const res = await leadService.importLeads(file);
      if (res.success && res.data) {
        setNotification(`Successfully imported ${res.data.importedCount} leads!`);
        fetchLeads();
      } else {
        setError(res.message || 'Failed to import leads.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Import failed.');
    } finally {
      if (fileInputRef.current) fileInputRef.current.value = '';
      setTimeout(() => setNotification(null), 4000);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`Are you sure you want to delete lead "${name}"?`)) return;
    try {
      const res = await leadService.deleteLead(id);
      if (res.success) {
        setNotification(`Lead "${name}" deleted.`);
        fetchLeads();
        setTimeout(() => setNotification(null), 3000);
      } else {
        setError(res.message || 'Failed to delete lead.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error deleting lead.');
    }
  };

  return (
    <div className="space-y-6">
      {/* Top Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white">
            Lead Management
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400">
            Track, qualify, assign, and convert prospective enterprise opportunities.
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <input
            type="file"
            ref={fileInputRef}
            onChange={handleFileChange}
            accept=".csv"
            className="hidden"
          />
          <button
            onClick={() => fileInputRef.current?.click()}
            className="px-3 py-2 text-xs font-medium text-slate-700 dark:text-slate-200 bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 flex items-center gap-1.5 transition-colors"
          >
            <Upload className="w-3.5 h-3.5 text-slate-500" />
            Import CSV
          </button>

          <button
            onClick={handleExport}
            className="px-3 py-2 text-xs font-medium text-slate-700 dark:text-slate-200 bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 flex items-center gap-1.5 transition-colors"
          >
            <Download className="w-3.5 h-3.5 text-slate-500" />
            Export CSV
          </button>

          <button
            onClick={() => setIsCreateOpen(true)}
            className="px-3.5 py-2 text-xs font-semibold text-white bg-blue-600 hover:bg-blue-700 rounded-lg flex items-center gap-1.5 shadow-sm transition-colors"
          >
            <Plus className="w-4 h-4" />
            New Lead
          </button>
        </div>
      </div>

      {/* Notifications and Errors */}
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

      {/* Search & Filters Bar */}
      <div className="p-4 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs space-y-3">
        <div className="flex flex-col md:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-2.5" />
            <input
              type="text"
              placeholder="Search leads by name, email, phone, company..."
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
              value={status}
              onChange={(e) => {
                setStatus(e.target.value);
                setPageNumber(1);
              }}
              className="px-2.5 py-2 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
            >
              <option value="">All Statuses</option>
              <option value="New">New</option>
              <option value="Contacted">Contacted</option>
              <option value="Qualified">Qualified</option>
              <option value="Unqualified">Unqualified</option>
              <option value="Converted">Converted</option>
              <option value="Stagnant">Stagnant</option>
            </select>

            <select
              value={priority}
              onChange={(e) => {
                setPriority(e.target.value);
                setPageNumber(1);
              }}
              className="px-2.5 py-2 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
            >
              <option value="">All Priorities</option>
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
              <option value="Urgent">Urgent</option>
            </select>

            <select
              value={source}
              onChange={(e) => {
                setSource(e.target.value);
                setPageNumber(1);
              }}
              className="px-2.5 py-2 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
            >
              <option value="">All Sources</option>
              <option value="Website">Website</option>
              <option value="Referral">Referral</option>
              <option value="ColdOutreach">Cold Outreach</option>
              <option value="InboundCall">Inbound Call</option>
              <option value="LinkedIn">LinkedIn</option>
              <option value="Conference">Conference</option>
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
                setStatus('');
                setSource('');
                setPriority('');
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

      {/* Leads Table */}
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs text-slate-600 dark:text-slate-300">
            <thead className="bg-slate-50 dark:bg-slate-900/50 text-[11px] uppercase tracking-wider text-slate-500 dark:text-slate-400 border-b border-slate-200 dark:border-slate-700">
              <tr>
                <th
                  onClick={() => handleSort('FullName')}
                  className="px-4 py-3 font-semibold cursor-pointer hover:text-slate-900 dark:hover:text-white"
                >
                  Name & Title {sortBy === 'FullName' && (sortDescending ? '↓' : '↑')}
                </th>
                <th
                  onClick={() => handleSort('CompanyName')}
                  className="px-4 py-3 font-semibold cursor-pointer hover:text-slate-900 dark:hover:text-white"
                >
                  Company {sortBy === 'CompanyName' && (sortDescending ? '↓' : '↑')}
                </th>
                <th className="px-4 py-3 font-semibold">Contact Info</th>
                <th
                  onClick={() => handleSort('Status')}
                  className="px-4 py-3 font-semibold cursor-pointer hover:text-slate-900 dark:hover:text-white"
                >
                  Status {sortBy === 'Status' && (sortDescending ? '↓' : '↑')}
                </th>
                <th
                  onClick={() => handleSort('Priority')}
                  className="px-4 py-3 font-semibold cursor-pointer hover:text-slate-900 dark:hover:text-white"
                >
                  Priority {sortBy === 'Priority' && (sortDescending ? '↓' : '↑')}
                </th>
                <th
                  onClick={() => handleSort('ExpectedBudget')}
                  className="px-4 py-3 font-semibold cursor-pointer hover:text-slate-900 dark:hover:text-white"
                >
                  Budget {sortBy === 'ExpectedBudget' && (sortDescending ? '↓' : '↑')}
                </th>
                <th className="px-4 py-3 font-semibold">Assigned Rep</th>
                <th
                  onClick={() => handleSort('CreatedAt')}
                  className="px-4 py-3 font-semibold cursor-pointer hover:text-slate-900 dark:hover:text-white"
                >
                  Created {sortBy === 'CreatedAt' && (sortDescending ? '↓' : '↑')}
                </th>
                <th className="px-4 py-3 font-semibold text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-700/60">
              {isLoading ? (
                <tr>
                  <td colSpan={9} className="py-12 text-center text-slate-400">
                    <RefreshCw className="w-6 h-6 animate-spin mx-auto mb-2 text-blue-500" />
                    <span>Loading leads catalog...</span>
                  </td>
                </tr>
              ) : data.items.length === 0 ? (
                <tr>
                  <td colSpan={9} className="py-12 text-center text-slate-400">
                    <FileSpreadsheet className="w-8 h-8 mx-auto mb-2 opacity-40" />
                    <p className="font-medium text-slate-600 dark:text-slate-300">No leads found</p>
                    <p className="text-xs text-slate-400 mt-0.5">Try adjusting your search criteria or register a new lead.</p>
                  </td>
                </tr>
              ) : (
                data.items.map((lead) => (
                  <tr
                    key={lead.id}
                    className="hover:bg-slate-50/70 dark:hover:bg-slate-700/30 transition-colors"
                  >
                    <td className="px-4 py-3">
                      <button
                        onClick={() => navigate(`/leads/${lead.id}`)}
                        className="font-medium text-slate-900 dark:text-white hover:text-blue-600 dark:hover:text-blue-400 text-left"
                      >
                        {lead.fullName}
                      </button>
                      {lead.position && (
                        <div className="text-[11px] text-slate-400">{lead.position}</div>
                      )}
                    </td>
                    <td className="px-4 py-3 font-medium text-slate-700 dark:text-slate-200">
                      {lead.companyName || '—'}
                    </td>
                    <td className="px-4 py-3 text-[11px]">
                      <div>{lead.email || '—'}</div>
                      <div className="text-slate-400">{lead.phone || ''}</div>
                    </td>
                    <td className="px-4 py-3">
                      <StatusBadge status={lead.status} />
                    </td>
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold ${
                          lead.priority === 'Urgent'
                            ? 'bg-rose-100 text-rose-700 dark:bg-rose-950/60 dark:text-rose-400'
                            : lead.priority === 'High'
                            ? 'bg-orange-100 text-orange-700 dark:bg-orange-950/60 dark:text-orange-400'
                            : lead.priority === 'Medium'
                            ? 'bg-blue-100 text-blue-700 dark:bg-blue-950/60 dark:text-blue-400'
                            : 'bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-400'
                        }`}
                      >
                        {lead.priority}
                      </span>
                    </td>
                    <td className="px-4 py-3 font-medium text-slate-800 dark:text-slate-200">
                      {lead.expectedBudget
                        ? `$${Number(lead.expectedBudget).toLocaleString()}`
                        : '—'}
                    </td>
                    <td className="px-4 py-3">
                      {lead.assignedEmployeeName ? (
                        <span className="font-medium text-slate-700 dark:text-slate-300">
                          {lead.assignedEmployeeName}
                        </span>
                      ) : (
                        <span className="text-slate-400 italic">Unassigned</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-slate-400 text-[11px]">
                      {dayjs(lead.createdAt).format('MMM D, YYYY')}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex items-center justify-end gap-1">
                        <button
                          title="View Details"
                          onClick={() => navigate(`/leads/${lead.id}`)}
                          className="p-1.5 text-slate-500 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-slate-100 dark:hover:bg-slate-700 rounded transition-colors"
                        >
                          <Eye className="w-3.5 h-3.5" />
                        </button>

                        <button
                          title="Assign Lead"
                          onClick={() => setAssignLead(lead)}
                          className="p-1.5 text-slate-500 hover:text-indigo-600 dark:hover:text-indigo-400 hover:bg-slate-100 dark:hover:bg-slate-700 rounded transition-colors"
                        >
                          <UserCheck className="w-3.5 h-3.5" />
                        </button>

                        {lead.status !== 'Converted' && (
                          <button
                            title="Convert Lead"
                            onClick={() => setConvertLead(lead)}
                            className="p-1.5 text-slate-500 hover:text-emerald-600 dark:hover:text-emerald-400 hover:bg-slate-100 dark:hover:bg-slate-700 rounded transition-colors"
                          >
                            <ArrowRightLeft className="w-3.5 h-3.5" />
                          </button>
                        )}

                        <button
                          title="Delete Lead"
                          onClick={() => handleDelete(lead.id, lead.fullName)}
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
            <span className="font-semibold">{data.totalCount}</span> total leads
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

      {/* Modals */}
      <CreateLeadModal
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        onSuccess={() => {
          setNotification('New Lead created successfully.');
          fetchLeads();
          setTimeout(() => setNotification(null), 3000);
        }}
      />

      <AssignLeadModal
        isOpen={!!assignLead}
        onClose={() => setAssignLead(null)}
        lead={assignLead}
        onSuccess={() => {
          setNotification('Lead assigned successfully.');
          fetchLeads();
          setTimeout(() => setNotification(null), 3000);
        }}
      />

      <ConvertLeadModal
        isOpen={!!convertLead}
        onClose={() => setConvertLead(null)}
        lead={convertLead}
        onSuccess={(result) => {
          setNotification(`Lead converted! Created Account "${result.accountName}" & Deal "${result.dealTitle}".`);
          fetchLeads();
          setTimeout(() => setNotification(null), 5000);
        }}
      />
    </div>
  );
};
