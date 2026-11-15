import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Activity,
  Database,
  Server,
  RefreshCw,
  TrendingUp,
  Users,
  Briefcase,
  DollarSign,
  Plus,
  ArrowUpRight,
  Clock,
  Sparkles,
} from 'lucide-react';
import { healthService } from '@/services/healthService';
import { StatusBadge } from '@/components/common/StatusBadge';
import type { DatabaseHealthDto } from '@/types/api';

export const DashboardPage: React.FC = () => {
  const [dbTestResult, setDbTestResult] = useState<DatabaseHealthDto | null>(null);
  const [isTestingDb, setIsTestingDb] = useState(false);
  const [dbTestError, setDbTestError] = useState<string | null>(null);

  // Live Query for backend health
  const { data: healthData, isLoading, isError, refetch } = useQuery({
    queryKey: ['systemHealth'],
    queryFn: healthService.getSystemHealth,
    refetchInterval: 30000,
  });

  const handleTestDatabase = async () => {
    setIsTestingDb(true);
    setDbTestError(null);
    try {
      const result = await healthService.getDatabaseHealth();
      setDbTestResult(result);
    } catch (err: any) {
      setDbTestError(err.message || 'Database connection error.');
    } finally {
      setIsTestingDb(false);
    }
  };

  return (
    <div className="space-y-8 animate-in fade-in duration-300">
      {/* Header & Quick Actions */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Enterprise Dashboard</h1>
          <p className="text-sm text-slate-500 mt-1">
            Service Business Operations, Live Pipeline & Cloud Connectivity.
          </p>
        </div>
        <div className="flex items-center gap-3">
          <button
            onClick={() => refetch()}
            className="inline-flex items-center gap-2 px-3.5 py-2 rounded-lg text-xs font-semibold bg-white border border-slate-200 text-slate-700 hover:bg-slate-50 transition-colors shadow-xs"
          >
            <RefreshCw className="w-3.5 h-3.5 text-slate-500" />
            Refresh
          </button>
          <button className="inline-flex items-center gap-2 px-4 py-2 rounded-lg text-xs font-semibold bg-blue-600 hover:bg-blue-700 text-white transition-colors shadow-sm shadow-blue-500/20">
            <Plus className="w-3.5 h-3.5" />
            Quick Lead
          </button>
        </div>
      </div>

      {/* Cloud & Architecture Health Diagnostic Widget */}
      <div className="bg-white rounded-xl border border-slate-200 p-6 shadow-xs">
        <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-6 pb-6 border-b border-slate-100">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-blue-50 border border-blue-100 flex items-center justify-center text-blue-600">
              <Activity className="w-5 h-5" />
            </div>
            <div>
              <div className="flex items-center gap-2.5">
                <h3 className="font-semibold text-slate-900 text-sm">System Health & Cloud Infrastructure</h3>
                {isLoading ? (
                  <StatusBadge status="warning">Checking...</StatusBadge>
                ) : isError ? (
                  <StatusBadge status="error">API Offline</StatusBadge>
                ) : (
                  <StatusBadge status="healthy">API Online (.NET 10)</StatusBadge>
                )}
              </div>
              <p className="text-xs text-slate-500 mt-0.5">
                Live communication between React Frontend and ASP.NET Core Clean Architecture API.
              </p>
            </div>
          </div>

          <button
            onClick={handleTestDatabase}
            disabled={isTestingDb}
            className="inline-flex items-center justify-center gap-2 px-4 py-2 rounded-lg text-xs font-semibold bg-slate-900 hover:bg-slate-800 text-white disabled:opacity-50 transition-colors shadow-xs"
          >
            {isTestingDb ? (
              <RefreshCw className="w-3.5 h-3.5 animate-spin" />
            ) : (
              <Database className="w-3.5 h-3.5 text-blue-400" />
            )}
            Test Supabase DB Connection
          </button>
        </div>

        {/* Diagnostic Metadata Grid */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 pt-6">
          <div className="flex items-start gap-3">
            <Server className="w-4 h-4 text-blue-600 mt-0.5 shrink-0" />
            <div className="space-y-1">
              <span className="text-xs font-medium text-slate-500">Backend Server</span>
              <p className="text-sm font-semibold text-slate-800">
                ASP.NET Core Web API 10.0
              </p>
              <p className="text-[11px] text-slate-400 font-mono">
                Environment: {healthData?.environment || 'Development'}
              </p>
            </div>
          </div>

          <div className="flex items-start gap-3">
            <Database className="w-4 h-4 text-emerald-600 mt-0.5 shrink-0" />
            <div className="space-y-1">
              <span className="text-xs font-medium text-slate-500">Database Engine</span>
              <p className="text-sm font-semibold text-slate-800">
                Supabase PostgreSQL
              </p>
              <p className="text-[11px] text-slate-400 font-mono">
                Host: db.ehmjgxwrmzlccnivqqxb.supabase.co
              </p>
            </div>
          </div>

          <div className="flex items-start gap-3">
            <Clock className="w-4 h-4 text-indigo-600 mt-0.5 shrink-0" />
            <div className="space-y-1">
              <span className="text-xs font-medium text-slate-500">Server Time (UTC)</span>
              <p className="text-sm font-semibold text-slate-800 font-mono">
                {healthData?.serverTimeUtc
                  ? new Date(healthData.serverTimeUtc).toLocaleTimeString() + ' UTC'
                  : 'Awaiting API response...'}
              </p>
              <p className="text-[11px] text-slate-400">
                SSL Required / Port 5432
              </p>
            </div>
          </div>
        </div>

        {/* Live DB Test Results Banner */}
        {dbTestResult && (
          <div className="mt-6 p-4 rounded-lg bg-emerald-50 border border-emerald-200 flex items-start gap-3">
            <div className="w-2 h-2 rounded-full bg-emerald-500 mt-1.5 shrink-0" />
            <div className="text-xs text-emerald-900 space-y-1">
              <p className="font-semibold">{dbTestResult.message}</p>
              <p className="text-emerald-700 font-mono">
                Response Time: {dbTestResult.responseTimeMs.toFixed(1)}ms | Host: {dbTestResult.host}
              </p>
            </div>
          </div>
        )}

        {dbTestError && (
          <div className="mt-6 p-4 rounded-lg bg-amber-50 border border-amber-200 flex items-start gap-3">
            <div className="w-2 h-2 rounded-full bg-amber-500 mt-1.5 shrink-0" />
            <div className="text-xs text-amber-900 space-y-1">
              <p className="font-semibold">Supabase Connection Notice</p>
              <p className="text-amber-700">{dbTestError}</p>
              <p className="text-[11px] text-amber-600 mt-1">
                Tip: Configure <code className="bg-amber-100 px-1 py-0.5 rounded font-mono">SUPABASE_DB_PASSWORD</code> in your environment or secrets to verify live query execution against Supabase.
              </p>
            </div>
          </div>
        )}
      </div>

      {/* CRM Business KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
        <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-medium text-slate-500">Active Leads</span>
            <div className="w-8 h-8 rounded-lg bg-blue-50 text-blue-600 flex items-center justify-center">
              <Users className="w-4 h-4" />
            </div>
          </div>
          <div className="mt-4 flex items-baseline gap-2">
            <span className="text-2xl font-bold text-slate-900">48</span>
            <span className="text-xs font-semibold text-emerald-600 flex items-center">
              <TrendingUp className="w-3 h-3 mr-0.5" /> +12%
            </span>
          </div>
          <p className="text-[11px] text-slate-400 mt-1">Omnichannel Ads & Inbound</p>
        </div>

        <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-medium text-slate-500">Pipeline Deals</span>
            <div className="w-8 h-8 rounded-lg bg-indigo-50 text-indigo-600 flex items-center justify-center">
              <Briefcase className="w-4 h-4" />
            </div>
          </div>
          <div className="mt-4 flex items-baseline gap-2">
            <span className="text-2xl font-bold text-slate-900">24</span>
            <span className="text-xs font-semibold text-emerald-600 flex items-center">
              <TrendingUp className="w-3 h-3 mr-0.5" /> +5
            </span>
          </div>
          <p className="text-[11px] text-slate-400 mt-1">6 Active Kanban Stages</p>
        </div>

        <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-medium text-slate-500">Weighted Revenue</span>
            <div className="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-600 flex items-center justify-center">
              <DollarSign className="w-4 h-4" />
            </div>
          </div>
          <div className="mt-4 flex items-baseline gap-2">
            <span className="text-2xl font-bold text-slate-900">$284,500</span>
            <span className="text-xs font-semibold text-emerald-600 flex items-center">
              <TrendingUp className="w-3 h-3 mr-0.5" /> +18.4%
            </span>
          </div>
          <p className="text-[11px] text-slate-400 mt-1">Forecast for current quarter</p>
        </div>

        <div className="bg-white rounded-xl border border-slate-200 p-5 shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-medium text-slate-500">AI Co-pilot</span>
            <div className="w-8 h-8 rounded-lg bg-purple-50 text-purple-600 flex items-center justify-center">
              <Sparkles className="w-4 h-4" />
            </div>
          </div>
          <div className="mt-4 flex items-baseline gap-2">
            <span className="text-2xl font-bold text-slate-900">Active</span>
            <span className="text-xs font-semibold text-purple-600">Smart Insights</span>
          </div>
          <p className="text-[11px] text-slate-400 mt-1">Stagnant deal alerts ready</p>
        </div>
      </div>

      {/* Pipeline Preview and Roadmap Status */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Pipeline Overview */}
        <div className="lg:col-span-2 bg-white rounded-xl border border-slate-200 p-6 shadow-xs">
          <div className="flex items-center justify-between mb-6">
            <div>
              <h3 className="font-semibold text-slate-900 text-sm">Deal Pipeline Distribution</h3>
              <p className="text-xs text-slate-500">Deal velocity across service stages</p>
            </div>
            <button className="text-xs font-semibold text-blue-600 hover:text-blue-700 flex items-center gap-1">
              View Kanban <ArrowUpRight className="w-3.5 h-3.5" />
            </button>
          </div>

          <div className="grid grid-cols-5 gap-3">
            {[
              { name: 'Lead In', count: 8, val: '$42k', color: 'bg-slate-100 border-slate-200' },
              { name: 'Qualified', count: 6, val: '$68k', color: 'bg-blue-50 border-blue-200' },
              { name: 'Proposal Sent', count: 4, val: '$95k', color: 'bg-amber-50 border-amber-200' },
              { name: 'Negotiation', count: 3, val: '$52k', color: 'bg-purple-50 border-purple-200' },
              { name: 'Won (Signed)', count: 3, val: '$74k', color: 'bg-emerald-50 border-emerald-200' },
            ].map((stage, idx) => (
              <div key={idx} className={`p-3 rounded-lg border ${stage.color} flex flex-col justify-between`}>
                <span className="text-[11px] font-semibold text-slate-600 truncate">{stage.name}</span>
                <div className="mt-3">
                  <span className="text-base font-bold text-slate-900">{stage.count} deals</span>
                  <p className="text-[11px] text-slate-500 font-medium">{stage.val}</p>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Phase Implementation Status */}
        <div className="bg-white rounded-xl border border-slate-200 p-6 shadow-xs">
          <h3 className="font-semibold text-slate-900 text-sm mb-4">Phase Progress</h3>
          <div className="space-y-4">
            <div className="flex items-center justify-between p-3 rounded-lg bg-emerald-50/60 border border-emerald-200">
              <div className="space-y-0.5">
                <span className="text-xs font-bold text-emerald-900">Phase 1: Foundation</span>
                <p className="text-[11px] text-emerald-700">Arch, Clean API & Vite TS</p>
              </div>
              <span className="text-[11px] font-bold px-2 py-0.5 rounded-full bg-emerald-200 text-emerald-800">
                COMPLETED
              </span>
            </div>

            <div className="flex items-center justify-between p-3 rounded-lg bg-blue-50/60 border border-blue-200">
              <div className="space-y-0.5">
                <span className="text-xs font-bold text-blue-900">Phase 2: DB & Auth</span>
                <p className="text-[11px] text-blue-700">ERD, Migrations & RBAC</p>
              </div>
              <span className="text-[11px] font-bold px-2 py-0.5 rounded-full bg-blue-200 text-blue-800">
                NEXT
              </span>
            </div>

            <div className="flex items-center justify-between p-3 rounded-lg bg-slate-50 border border-slate-200">
              <div className="space-y-0.5">
                <span className="text-xs font-bold text-slate-700">Phase 3: Core CRM</span>
                <p className="text-[11px] text-slate-500">Leads, Deals & Kanban</p>
              </div>
              <span className="text-[11px] font-medium px-2 py-0.5 rounded-full bg-slate-200 text-slate-600">
                PENDING
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
