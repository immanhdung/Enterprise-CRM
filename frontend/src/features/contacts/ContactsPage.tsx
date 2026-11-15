import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Users,
  Plus,
  Search,
  Mail,
  Phone,
  Building2,
  Trash2,
  RefreshCw,
  AlertCircle,
  CheckCircle,
  Star,
} from 'lucide-react';
import { contactService } from '../../services/contactService';
import { accountService } from '../../services/accountService';
import { CreateContactModal } from './components/CreateContactModal';
import type { ContactDto, PagedResult, AccountDto } from '../../types/crm';

export const ContactsPage: React.FC = () => {
  const navigate = useNavigate();

  const [data, setData] = useState<PagedResult<ContactDto>>({
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 10,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  });
  const [accounts, setAccounts] = useState<AccountDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [notification, setNotification] = useState<string | null>(null);

  // Filters
  const [search, setSearch] = useState('');
  const [accountId, setAccountId] = useState('');
  const [isPrimary, setIsPrimary] = useState<string>('');
  const [pageNumber, setPageNumber] = useState(1);
  const [isCreateOpen, setIsCreateOpen] = useState(false);

  const fetchContacts = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const res = await contactService.getContacts({
        pageNumber,
        pageSize: 10,
        search: search.trim() || undefined,
        accountId: accountId || undefined,
        isPrimary: isPrimary === 'true' ? true : isPrimary === 'false' ? false : undefined,
      });

      if (res.success && res.data) {
        setData(res.data);
      } else {
        setError(res.message || 'Failed to fetch contacts.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error loading contacts.');
    } finally {
      setIsLoading(false);
    }
  }, [pageNumber, search, accountId, isPrimary]);

  useEffect(() => {
    fetchContacts();
  }, [fetchContacts]);

  useEffect(() => {
    accountService.getAccounts({ pageSize: 50 }).then((res) => {
      if (res.data?.items) setAccounts(res.data.items);
    }).catch(console.error);
  }, []);

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`Are you sure you want to delete contact "${name}"?`)) return;
    try {
      const res = await contactService.deleteContact(id);
      if (res.success) {
        setNotification(`Contact "${name}" deleted.`);
        fetchContacts();
        setTimeout(() => setNotification(null), 3000);
      } else {
        setError(res.message || 'Failed to delete contact.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error deleting contact.');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white">
            Contact Directory
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400">
            Maintain executive and operational stakeholders across all corporate client accounts.
          </p>
        </div>

        <button
          onClick={() => setIsCreateOpen(true)}
          className="px-3.5 py-2 text-xs font-semibold text-white bg-blue-600 hover:bg-blue-700 rounded-lg flex items-center gap-1.5 shadow-sm transition-colors"
        >
          <Plus className="w-4 h-4" />
          New Contact
        </button>
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
              placeholder="Search contacts by name, email, title, phone..."
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
              value={isPrimary}
              onChange={(e) => {
                setIsPrimary(e.target.value);
                setPageNumber(1);
              }}
              className="px-2.5 py-2 border rounded-lg text-xs bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300"
            >
              <option value="">All Roles</option>
              <option value="true">Primary Contacts Only</option>
              <option value="false">Secondary Contacts</option>
            </select>

            <button
              onClick={() => {
                setSearch('');
                setAccountId('');
                setIsPrimary('');
                setPageNumber(1);
              }}
              className="px-2.5 py-2 text-xs text-slate-500 hover:text-slate-700 dark:hover:text-slate-300 transition-colors"
            >
              Reset
            </button>
          </div>
        </div>
      </div>

      {/* Contacts Table */}
      <div className="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700/60 shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs text-slate-600 dark:text-slate-300">
            <thead className="bg-slate-50 dark:bg-slate-900/50 text-[11px] uppercase tracking-wider text-slate-500 dark:text-slate-400 border-b border-slate-200 dark:border-slate-700">
              <tr>
                <th className="px-4 py-3 font-semibold">Contact Name</th>
                <th className="px-4 py-3 font-semibold">Company Account</th>
                <th className="px-4 py-3 font-semibold">Position & Department</th>
                <th className="px-4 py-3 font-semibold">Email & Phone</th>
                <th className="px-4 py-3 font-semibold">Primary Role</th>
                <th className="px-4 py-3 font-semibold">Active Deals</th>
                <th className="px-4 py-3 font-semibold text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-700/60">
              {isLoading ? (
                <tr>
                  <td colSpan={7} className="py-12 text-center text-slate-400">
                    <RefreshCw className="w-6 h-6 animate-spin mx-auto mb-2 text-blue-500" />
                    <span>Loading contact directory...</span>
                  </td>
                </tr>
              ) : data.items.length === 0 ? (
                <tr>
                  <td colSpan={7} className="py-12 text-center text-slate-400">
                    <Users className="w-8 h-8 mx-auto mb-2 opacity-40" />
                    <p className="font-medium text-slate-600 dark:text-slate-300">No contacts found</p>
                    <p className="text-xs text-slate-400 mt-0.5">Register a contact or adjust filter parameters.</p>
                  </td>
                </tr>
              ) : (
                data.items.map((contact) => (
                  <tr
                    key={contact.id}
                    className="hover:bg-slate-50/70 dark:hover:bg-slate-700/30 transition-colors"
                  >
                    <td className="px-4 py-3">
                      <div className="font-semibold text-slate-900 dark:text-white">
                        {contact.fullName}
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <button
                        onClick={() => navigate(`/accounts/${contact.accountId}`)}
                        className="font-medium text-blue-600 dark:text-blue-400 hover:underline flex items-center gap-1"
                      >
                        <Building2 className="w-3.5 h-3.5 shrink-0" />
                        <span>{contact.accountName}</span>
                      </button>
                    </td>
                    <td className="px-4 py-3">
                      <div className="font-medium text-slate-800 dark:text-slate-200">
                        {contact.position || '—'}
                      </div>
                      {contact.department && (
                        <div className="text-[11px] text-slate-400">{contact.department}</div>
                      )}
                    </td>
                    <td className="px-4 py-3 text-[11px]">
                      {contact.email && (
                        <div className="flex items-center gap-1 text-slate-700 dark:text-slate-300">
                          <Mail className="w-3 h-3 text-slate-400 shrink-0" />
                          <a href={`mailto:${contact.email}`} className="hover:underline">
                            {contact.email}
                          </a>
                        </div>
                      )}
                      {contact.phone && (
                        <div className="flex items-center gap-1 text-slate-500 mt-0.5">
                          <Phone className="w-3 h-3 text-slate-400 shrink-0" />
                          <a href={`tel:${contact.phone}`} className="hover:underline">
                            {contact.phone}
                          </a>
                        </div>
                      )}
                    </td>
                    <td className="px-4 py-3">
                      {contact.isPrimary ? (
                        <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold bg-blue-100 text-blue-700 dark:bg-blue-950/60 dark:text-blue-300">
                          <Star className="w-2.5 h-2.5 fill-current" />
                          Primary Contact
                        </span>
                      ) : (
                        <span className="text-slate-400 text-[11px]">Secondary</span>
                      )}
                    </td>
                    <td className="px-4 py-3 font-medium text-slate-700 dark:text-slate-300">
                      {contact.dealsCount} deals
                    </td>
                    <td className="px-4 py-3 text-right">
                      <button
                        title="Delete Contact"
                        onClick={() => handleDelete(contact.id, contact.fullName)}
                        className="p-1.5 text-slate-500 hover:text-red-600 dark:hover:text-red-400 hover:bg-slate-100 dark:hover:bg-slate-700 rounded transition-colors"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>
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
            <span className="font-semibold">{data.totalCount}</span> total contacts
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

      <CreateContactModal
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        onSuccess={() => {
          setNotification('Contact created successfully.');
          fetchContacts();
          setTimeout(() => setNotification(null), 3000);
        }}
      />
    </div>
  );
};
