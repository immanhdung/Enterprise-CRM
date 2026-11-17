import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  DollarSign,
  Receipt,
  CreditCard,
  AlertTriangle,
  Percent,
  Plus,
  ArrowRight,
  ShieldAlert,
} from 'lucide-react';
import { FinanceNavigation } from './FinanceNavigation';
import { financeService } from '@/services/financeService';
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  Legend,
  CartesianGrid,
} from 'recharts';

export const FinanceDashboardPage: React.FC = () => {
  const { data: revenueData, isLoading: isRevLoading } = useQuery({
    queryKey: ['finance', 'revenue-analytics'],
    queryFn: () => financeService.getRevenueAnalytics({ periodType: 'monthly' }),
  });

  const { data: agingSummary, isLoading: isAgingLoading } = useQuery({
    queryKey: ['finance', 'aging-summary'],
    queryFn: () => financeService.getAgingSummary(),
  });

  const { data: recentPayments } = useQuery({
    queryKey: ['finance', 'recent-payments'],
    queryFn: () => financeService.getPayments({ pageSize: 5 }),
  });

  const { data: overdueList } = useQuery({
    queryKey: ['finance', 'overdue-receivables'],
    queryFn: () => financeService.getReceivablesList({ onlyOverdue: true }),
  });

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const summary = revenueData?.summary;

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <FinanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Header Title & Quick Actions */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Financial Management Dashboard</h1>
            <p className="text-sm text-slate-500 mt-1">
              Multi-installment contract schedules, cash collection, debt aging & electronic invoicing.
            </p>
          </div>
          <div className="flex items-center gap-2.5">
            <Link
              to="/finance/payments"
              className="inline-flex items-center gap-1.5 px-3.5 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 shadow-xs transition"
            >
              <Plus className="w-4 h-4" />
              <span>Record Payment</span>
            </Link>
            <Link
              to="/finance/invoices"
              className="inline-flex items-center gap-1.5 px-3.5 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-200 rounded-lg hover:bg-slate-50 shadow-xs transition"
            >
              <Receipt className="w-4 h-4" />
              <span>Issue Invoice</span>
            </Link>
            <Link
              to="/finance/reconciliation"
              className="inline-flex items-center gap-1.5 px-3.5 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-200 rounded-lg hover:bg-slate-50 shadow-xs transition"
            >
              <span>Reconcile</span>
            </Link>
          </div>
        </div>

        {/* 6 Key Financial KPI Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4">
          {/* Card 1: Contract Value */}
          <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-slate-500 mb-2">
              <span className="text-xs font-semibold uppercase tracking-wider">Total Contract Value</span>
              <DollarSign className="w-4 h-4 text-blue-600" />
            </div>
            <div className="text-lg font-bold text-slate-900">
              {isRevLoading ? '...' : formatVnd(summary?.totalContractValue)}
            </div>
            <div className="text-xs text-slate-500 mt-1">Active signed contracts</div>
          </div>

          {/* Card 2: Invoiced Revenue */}
          <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-slate-500 mb-2">
              <span className="text-xs font-semibold uppercase tracking-wider">Invoiced Revenue</span>
              <Receipt className="w-4 h-4 text-indigo-600" />
            </div>
            <div className="text-lg font-bold text-slate-900">
              {isRevLoading ? '...' : formatVnd(summary?.totalInvoicedAmount)}
            </div>
            <div className="text-xs text-indigo-600 font-medium mt-1">
              {summary ? `${summary.invoicedPercentage}% of contract value` : '0%'}
            </div>
          </div>

          {/* Card 3: Collected Cash */}
          <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-slate-500 mb-2">
              <span className="text-xs font-semibold uppercase tracking-wider">Collected Cash</span>
              <CreditCard className="w-4 h-4 text-emerald-600" />
            </div>
            <div className="text-lg font-bold text-emerald-600">
              {isRevLoading ? '...' : formatVnd(summary?.totalCollectedCash)}
            </div>
            <div className="text-xs text-emerald-700 font-medium mt-1">Only confirmed payments</div>
          </div>

          {/* Card 4: Outstanding Receivables */}
          <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-slate-500 mb-2">
              <span className="text-xs font-semibold uppercase tracking-wider">Total Receivables</span>
              <AlertTriangle className="w-4 h-4 text-amber-500" />
            </div>
            <div className="text-lg font-bold text-amber-600">
              {isRevLoading ? '...' : formatVnd(summary?.totalOutstandingReceivables)}
            </div>
            <div className="text-xs text-slate-500 mt-1">Pending + Overdue balance</div>
          </div>

          {/* Card 5: Cash Collection Rate */}
          <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-slate-500 mb-2">
              <span className="text-xs font-semibold uppercase tracking-wider">Collection Rate</span>
              <Percent className="w-4 h-4 text-purple-600" />
            </div>
            <div className="text-lg font-bold text-purple-700">
              {isRevLoading ? '...' : `${summary?.cashCollectionRate || 0}%`}
            </div>
            <div className="text-xs text-slate-500 mt-1">Cash / Invoiced ratio</div>
          </div>

          {/* Card 6: Overdue Debt */}
          <div className="bg-red-50/50 p-4 rounded-xl border border-red-200 shadow-xs">
            <div className="flex items-center justify-between text-red-600 mb-2">
              <span className="text-xs font-semibold uppercase tracking-wider">Overdue Debt</span>
              <ShieldAlert className="w-4 h-4 text-red-600" />
            </div>
            <div className="text-lg font-bold text-red-700">
              {isAgingLoading ? '...' : formatVnd(agingSummary?.totalOverdue)}
            </div>
            <div className="text-xs text-red-600 font-medium mt-1">
              {agingSummary?.overdueContractsCount || 0} contracts overdue
            </div>
          </div>
        </div>

        {/* Charts & Analytics Section */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Main Trend Bar Chart */}
          <div className="lg:col-span-2 bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between mb-4">
              <div>
                <h3 className="font-semibold text-slate-900">Revenue & Cash Collection Trend</h3>
                <p className="text-xs text-slate-500">Contract Incurred vs Invoiced vs Confirmed Cash</p>
              </div>
              <Link to="/finance/revenue" className="text-xs font-medium text-blue-600 hover:text-blue-700 flex items-center gap-1">
                Full analytics <ArrowRight className="w-3.5 h-3.5" />
              </Link>
            </div>
            <div className="h-72 w-full">
              {revenueData?.trendByPeriod && revenueData.trendByPeriod.length > 0 ? (
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={revenueData.trendByPeriod} margin={{ top: 10, right: 10, left: 10, bottom: 20 }}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                    <XAxis dataKey="periodLabel" stroke="#94a3b8" fontSize={11} tickLine={false} />
                    <YAxis
                      stroke="#94a3b8"
                      fontSize={11}
                      tickLine={false}
                      tickFormatter={(val) => `${(val / 1000000).toFixed(0)}M`}
                    />
                    <Tooltip
                      formatter={(val: any) => formatVnd(Number(val))}
                      contentStyle={{ backgroundColor: '#fff', borderRadius: '8px', border: '1px solid #e2e8f0' }}
                    />
                    <Legend wrapperStyle={{ fontSize: '12px' }} />
                    <Bar dataKey="contractValue" name="Contract Value" fill="#94a3b8" radius={[4, 4, 0, 0]} />
                    <Bar dataKey="invoicedAmount" name="Invoiced Revenue" fill="#6366f1" radius={[4, 4, 0, 0]} />
                    <Bar dataKey="collectedCash" name="Collected Cash" fill="#10b981" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              ) : (
                <div className="h-full flex items-center justify-center text-slate-400 text-sm">
                  No revenue trend data recorded for selected period.
                </div>
              )}
            </div>
          </div>

          {/* Aging Distribution Breakdown */}
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs flex flex-col justify-between">
            <div>
              <div className="flex items-center justify-between mb-4">
                <h3 className="font-semibold text-slate-900">Receivable Aging Buckets</h3>
                <Link to="/finance/receivables" className="text-xs font-medium text-blue-600 hover:text-blue-700 flex items-center gap-1">
                  View all <ArrowRight className="w-3.5 h-3.5" />
                </Link>
              </div>

              <div className="space-y-3">
                <div>
                  <div className="flex justify-between text-xs font-medium text-slate-700 mb-1">
                    <span className="flex items-center gap-1.5">
                      <span className="w-2.5 h-2.5 rounded-full bg-emerald-500"></span>
                      Current / Due Soon
                    </span>
                    <span>{formatVnd(agingSummary?.currentAmount)}</span>
                  </div>
                </div>

                <div>
                  <div className="flex justify-between text-xs font-medium text-slate-700 mb-1">
                    <span className="flex items-center gap-1.5">
                      <span className="w-2.5 h-2.5 rounded-full bg-amber-400"></span>
                      1–7 Days Overdue
                    </span>
                    <span>{formatVnd(agingSummary?.overdue1To7Days)}</span>
                  </div>
                </div>

                <div>
                  <div className="flex justify-between text-xs font-medium text-slate-700 mb-1">
                    <span className="flex items-center gap-1.5">
                      <span className="w-2.5 h-2.5 rounded-full bg-amber-500"></span>
                      8–30 Days Overdue
                    </span>
                    <span>{formatVnd(agingSummary?.overdue8To30Days)}</span>
                  </div>
                </div>

                <div>
                  <div className="flex justify-between text-xs font-medium text-slate-700 mb-1">
                    <span className="flex items-center gap-1.5">
                      <span className="w-2.5 h-2.5 rounded-full bg-orange-500"></span>
                      31–60 Days Overdue
                    </span>
                    <span>{formatVnd(agingSummary?.overdue31To60Days)}</span>
                  </div>
                </div>

                <div>
                  <div className="flex justify-between text-xs font-medium text-slate-700 mb-1">
                    <span className="flex items-center gap-1.5">
                      <span className="w-2.5 h-2.5 rounded-full bg-red-600"></span>
                      &gt;60 Days Overdue
                    </span>
                    <span className="text-red-700 font-semibold">{formatVnd(agingSummary?.overdueMoreThan60Days)}</span>
                  </div>
                </div>
              </div>
            </div>

            <div className="mt-6 p-3 bg-slate-50 rounded-lg border border-slate-200">
              <div className="text-xs text-slate-600 flex items-center justify-between">
                <span>Total Debtors:</span>
                <span className="font-semibold text-slate-900">{agingSummary?.accountsWithDebtCount || 0} Accounts</span>
              </div>
              <div className="text-xs text-slate-600 flex items-center justify-between mt-1">
                <span>Total Overdue:</span>
                <span className="font-semibold text-red-600">{formatVnd(agingSummary?.totalOverdue)}</span>
              </div>
            </div>
          </div>
        </div>

        {/* Tables Row: Recent Transactions & Urgent Overdue Debtors */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Recent Payments */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
            <div className="p-4 border-b border-slate-100 flex items-center justify-between">
              <div>
                <h3 className="font-semibold text-slate-900">Recent Payment Transactions</h3>
                <p className="text-xs text-slate-500">Latest bank transfers & cash settlements</p>
              </div>
              <Link to="/finance/payments" className="text-xs font-medium text-blue-600 hover:text-blue-700 flex items-center gap-1">
                All payments <ArrowRight className="w-3.5 h-3.5" />
              </Link>
            </div>
            <div className="divide-y divide-slate-100">
              {recentPayments?.items && recentPayments.items.length > 0 ? (
                recentPayments.items.map((p) => (
                  <div key={p.id} className="p-4 flex items-center justify-between hover:bg-slate-50 transition">
                    <div>
                      <div className="font-medium text-sm text-slate-900">{p.accountName || 'Client'}</div>
                      <div className="text-xs text-slate-500 mt-0.5">
                        {p.paymentNumber} • {new Date(p.paymentDate).toLocaleDateString('vi-VN')}
                      </div>
                    </div>
                    <div className="text-right">
                      <div className="text-sm font-semibold text-slate-900">{formatVnd(p.amount)}</div>
                      <span
                        className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium mt-0.5 ${
                          p.status === 'Confirmed'
                            ? 'bg-emerald-100 text-emerald-800'
                            : p.status === 'Pending'
                            ? 'bg-amber-100 text-amber-800'
                            : p.status === 'Reversed'
                            ? 'bg-purple-100 text-purple-800'
                            : 'bg-slate-100 text-slate-700'
                        }`}
                      >
                        {p.status}
                      </span>
                    </div>
                  </div>
                ))
              ) : (
                <div className="p-8 text-center text-slate-400 text-sm">No payment records found.</div>
              )}
            </div>
          </div>

          {/* Overdue Receivables Requiring Action */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
            <div className="p-4 border-b border-slate-100 flex items-center justify-between">
              <div>
                <h3 className="font-semibold text-slate-900">Overdue Installments Requiring Follow-up</h3>
                <p className="text-xs text-slate-500">Unsettled milestones past payment due date</p>
              </div>
              <Link to="/finance/receivables" className="text-xs font-medium text-blue-600 hover:text-blue-700 flex items-center gap-1">
                Manage collection <ArrowRight className="w-3.5 h-3.5" />
              </Link>
            </div>
            <div className="divide-y divide-slate-100">
              {overdueList && overdueList.length > 0 ? (
                overdueList.slice(0, 5).map((item) => (
                  <div key={item.paymentScheduleId} className="p-4 flex items-center justify-between hover:bg-slate-50 transition">
                    <div>
                      <div className="font-medium text-sm text-slate-900">{item.accountName}</div>
                      <div className="text-xs text-slate-500 mt-0.5">
                        {item.contractNumber} • {item.milestoneTitle}
                      </div>
                      <div className="text-xs text-red-600 font-medium mt-1">
                        Overdue by {item.overdueDays} days ({item.dueDate})
                      </div>
                    </div>
                    <div className="text-right">
                      <div className="text-sm font-semibold text-red-700">{formatVnd(item.outstandingAmount)}</div>
                      <Link
                        to={`/finance/receivables/${item.accountId}`}
                        className="inline-flex items-center gap-1 text-xs text-blue-600 hover:text-blue-700 mt-1 font-medium"
                      >
                        Action log <ArrowRight className="w-3 h-3" />
                      </Link>
                    </div>
                  </div>
                ))
              ) : (
                <div className="p-8 text-center text-slate-400 text-sm">No overdue installments! All debts are current.</div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
