import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  TrendingUp,
  DollarSign,
  Receipt,
  CreditCard,
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

export const RevenueAnalyticsPage: React.FC = () => {
  const [periodType, setPeriodType] = useState('monthly');
  const [activeDimension, setActiveDimension] = useState<'customer' | 'contract' | 'sales' | 'service'>('customer');

  const { data: analyticsData } = useQuery({
    queryKey: ['finance', 'revenue-analytics-full', periodType],
    queryFn: () => financeService.getRevenueAnalytics({ periodType }),
  });

  const formatVnd = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const summary = analyticsData?.summary;

  return (
    <div className="flex-1 flex flex-col min-h-screen bg-slate-50">
      <FinanceNavigation />

      <div className="p-6 space-y-6 max-w-7xl mx-auto w-full">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Revenue Analytics & Dimensions</h1>
            <p className="text-sm text-slate-500 mt-1">
              Strict accounting separation: Contract Value Incurred vs. Invoiced Revenue vs. Confirmed Collected Cash.
            </p>
          </div>

          <div className="flex items-center gap-2">
            <span className="text-xs font-semibold text-slate-500">Period:</span>
            <select
              value={periodType}
              onChange={(e) => setPeriodType(e.target.value)}
              className="text-xs border border-slate-300 rounded-lg px-3 py-1.5 bg-white text-slate-700 font-medium focus:ring-1 focus:ring-blue-500"
            >
              <option value="monthly">Monthly Breakdown</option>
              <option value="quarterly">Quarterly Breakdown</option>
              <option value="yearly">Annual Breakdown</option>
            </select>
          </div>
        </div>

        {/* 4 Core Financial Totals */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-slate-500 mb-2">
              <span className="text-xs font-semibold uppercase tracking-wider">Total Contract Value</span>
              <DollarSign className="w-4 h-4 text-slate-400" />
            </div>
            <div className="text-xl font-bold text-slate-900">{formatVnd(summary?.totalContractValue)}</div>
            <div className="text-xs text-slate-500 mt-1">Total value of all active signed contracts</div>
          </div>

          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-indigo-600 mb-2">
              <span className="text-xs font-semibold uppercase tracking-wider">Invoiced Revenue</span>
              <Receipt className="w-4 h-4 text-indigo-600" />
            </div>
            <div className="text-xl font-bold text-indigo-700">{formatVnd(summary?.totalInvoicedAmount)}</div>
            <div className="text-xs text-indigo-600 font-medium mt-1">
              {summary ? `${summary.invoicedPercentage}% invoiced of contract value` : '0%'}
            </div>
          </div>

          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-emerald-600 mb-2">
              <span className="text-xs font-semibold uppercase tracking-wider">Confirmed Collected Cash</span>
              <CreditCard className="w-4 h-4 text-emerald-600" />
            </div>
            <div className="text-xl font-bold text-emerald-700">{formatVnd(summary?.totalCollectedCash)}</div>
            <div className="text-xs text-emerald-600 font-medium mt-1">
              {summary ? `${summary.cashCollectionRate}% collection rate vs invoiced` : '0%'}
            </div>
          </div>

          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-xs">
            <div className="flex items-center justify-between text-amber-600 mb-2">
              <span className="text-xs font-semibold uppercase tracking-wider">Outstanding Receivables</span>
              <TrendingUp className="w-4 h-4 text-amber-500" />
            </div>
            <div className="text-xl font-bold text-amber-700">
              {formatVnd(summary?.totalOutstandingReceivables)}
            </div>
            <div className="text-xs text-slate-500 mt-1">Remaining to be settled</div>
          </div>
        </div>

        {/* Main Chart: Timeline Comparison */}
        <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-xs space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="font-bold text-slate-900 text-base">Timeline Financial Progression</h3>
              <p className="text-xs text-slate-500 mt-0.5">
                Contract Incurred vs Invoiced Revenue vs Confirmed Cash by {periodType}
              </p>
            </div>
          </div>

          <div className="h-80 w-full">
            {analyticsData?.trendByPeriod && analyticsData.trendByPeriod.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={analyticsData.trendByPeriod} margin={{ top: 10, right: 20, left: 20, bottom: 20 }}>
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
                  <Bar dataKey="contractValue" name="Contract Value" fill="#cbd5e1" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="invoicedAmount" name="Invoiced Revenue" fill="#6366f1" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="collectedCash" name="Collected Cash" fill="#10b981" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-full flex items-center justify-center text-slate-400 text-sm">
                No financial records for the selected period.
              </div>
            )}
          </div>
        </div>

        {/* Dimension Breakdown Tabs */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
          <div className="p-4 border-b border-slate-200 flex flex-wrap items-center justify-between gap-4">
            <h3 className="font-bold text-slate-900 text-sm">Dimensional Revenue Distribution</h3>

            <div className="flex items-center gap-1.5 bg-slate-100 p-1 rounded-lg text-xs">
              <button
                onClick={() => setActiveDimension('customer')}
                className={`px-3 py-1.5 rounded-md font-medium transition ${
                  activeDimension === 'customer'
                    ? 'bg-white text-blue-600 font-semibold shadow-xs'
                    : 'text-slate-600 hover:text-slate-900'
                }`}
              >
                By Customer
              </button>
              <button
                onClick={() => setActiveDimension('contract')}
                className={`px-3 py-1.5 rounded-md font-medium transition ${
                  activeDimension === 'contract'
                    ? 'bg-white text-blue-600 font-semibold shadow-xs'
                    : 'text-slate-600 hover:text-slate-900'
                }`}
              >
                By Contract
              </button>
              <button
                onClick={() => setActiveDimension('sales')}
                className={`px-3 py-1.5 rounded-md font-medium transition ${
                  activeDimension === 'sales'
                    ? 'bg-white text-blue-600 font-semibold shadow-xs'
                    : 'text-slate-600 hover:text-slate-900'
                }`}
              >
                By Sales Representative
              </button>
              <button
                onClick={() => setActiveDimension('service')}
                className={`px-3 py-1.5 rounded-md font-medium transition ${
                  activeDimension === 'service'
                    ? 'bg-white text-blue-600 font-semibold shadow-xs'
                    : 'text-slate-600 hover:text-slate-900'
                }`}
              >
                By Service Category
              </button>
            </div>
          </div>

          <div className="overflow-x-auto">
            {activeDimension === 'customer' && (
              <table className="w-full text-left border-collapse text-xs">
                <thead>
                  <tr className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
                    <th className="py-3 px-4">Customer Name</th>
                    <th className="py-3 px-4">Contracts Count</th>
                    <th className="py-3 px-4">Total Contract Value</th>
                    <th className="py-3 px-4">Invoiced Amount</th>
                    <th className="py-3 px-4">Collected Cash</th>
                    <th className="py-3 px-4">Outstanding Receivables</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {analyticsData?.byCustomer?.map((c) => (
                    <tr key={c.accountId} className="hover:bg-slate-50 transition">
                      <td className="py-3 px-4 font-bold text-slate-900">{c.customerName}</td>
                      <td className="py-3 px-4 font-semibold text-slate-600">{c.contractCount}</td>
                      <td className="py-3 px-4 font-mono font-semibold text-slate-800">
                        {formatVnd(c.totalContractValue)}
                      </td>
                      <td className="py-3 px-4 font-mono text-indigo-700">{formatVnd(c.invoicedAmount)}</td>
                      <td className="py-3 px-4 font-mono text-emerald-600 font-bold">{formatVnd(c.collectedCash)}</td>
                      <td className="py-3 px-4 font-mono text-red-600">{formatVnd(c.outstandingAmount)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}

            {activeDimension === 'contract' && (
              <table className="w-full text-left border-collapse text-xs">
                <thead>
                  <tr className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
                    <th className="py-3 px-4">Contract #</th>
                    <th className="py-3 px-4">Customer Name</th>
                    <th className="py-3 px-4">Contract Value</th>
                    <th className="py-3 px-4">Invoiced Amount</th>
                    <th className="py-3 px-4">Collected Cash</th>
                    <th className="py-3 px-4">Remaining Balance</th>
                    <th className="py-3 px-4">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {analyticsData?.byContract?.map((c) => (
                    <tr key={c.contractId} className="hover:bg-slate-50 transition">
                      <td className="py-3 px-4 font-bold text-slate-900 font-mono">{c.contractNumber}</td>
                      <td className="py-3 px-4 text-slate-800">{c.customerName}</td>
                      <td className="py-3 px-4 font-mono font-semibold text-slate-900">{formatVnd(c.totalValue)}</td>
                      <td className="py-3 px-4 font-mono text-indigo-700">{formatVnd(c.invoicedAmount)}</td>
                      <td className="py-3 px-4 font-mono text-emerald-600 font-bold">{formatVnd(c.collectedCash)}</td>
                      <td className="py-3 px-4 font-mono text-amber-600">{formatVnd(c.outstandingAmount)}</td>
                      <td className="py-3 px-4">
                        <span className="px-2 py-0.5 rounded text-[11px] font-semibold bg-blue-50 text-blue-700">
                          {c.status}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}

            {activeDimension === 'sales' && (
              <table className="w-full text-left border-collapse text-xs">
                <thead>
                  <tr className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
                    <th className="py-3 px-4">Sales Representative</th>
                    <th className="py-3 px-4">Signed Contracts</th>
                    <th className="py-3 px-4">Total Contract Value</th>
                    <th className="py-3 px-4">Invoiced Revenue</th>
                    <th className="py-3 px-4">Actual Collected Cash</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {analyticsData?.bySales?.map((s, idx) => (
                    <tr key={idx} className="hover:bg-slate-50 transition">
                      <td className="py-3 px-4 font-bold text-slate-900">{s.employeeName}</td>
                      <td className="py-3 px-4 font-semibold text-slate-600">{s.contractCount}</td>
                      <td className="py-3 px-4 font-mono font-semibold text-slate-800">
                        {formatVnd(s.totalContractValue)}
                      </td>
                      <td className="py-3 px-4 font-mono text-indigo-700">{formatVnd(s.invoicedAmount)}</td>
                      <td className="py-3 px-4 font-mono text-emerald-600 font-bold">{formatVnd(s.collectedCash)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}

            {activeDimension === 'service' && (
              <table className="w-full text-left border-collapse text-xs">
                <thead>
                  <tr className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
                    <th className="py-3 px-4">Service Description</th>
                    <th className="py-3 px-4">Quantity Sold</th>
                    <th className="py-3 px-4">Invoiced Amount</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {analyticsData?.byService?.map((s, idx) => (
                    <tr key={idx} className="hover:bg-slate-50 transition">
                      <td className="py-3 px-4 font-bold text-slate-900">{s.serviceName}</td>
                      <td className="py-3 px-4 font-semibold text-slate-700 font-mono">{s.quantitySold}</td>
                      <td className="py-3 px-4 font-mono font-bold text-indigo-700">
                        {formatVnd(s.invoicedAmount)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
