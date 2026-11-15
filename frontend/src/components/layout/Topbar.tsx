import React from 'react';
import { Search, Bell, ExternalLink, User } from 'lucide-react';
import { useUIStore } from '@/stores/uiStore';

export const Topbar: React.FC = () => {
  const { searchQuery, setSearchQuery } = useUIStore();

  return (
    <header className="h-16 bg-white border-b border-slate-200 px-6 flex items-center justify-between sticky top-0 z-20 shadow-xs">
      {/* Global Search Bar */}
      <div className="flex items-center gap-4 flex-1 max-w-md">
        <div className="relative w-full">
          <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder="Search leads, deals, accounts, contracts (Ctrl+K)..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full bg-slate-50 border border-slate-200 rounded-lg pl-9 pr-4 py-1.5 text-sm text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:bg-white transition-all"
          />
        </div>
      </div>

      {/* Right Controls */}
      <div className="flex items-center gap-3">
        {/* API Docs Button */}
        <a
          href="http://localhost:5000/scalar/v1"
          target="_blank"
          rel="noreferrer"
          className="hidden sm:inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-slate-100 hover:bg-slate-200 text-slate-700 transition-colors"
        >
          <span>Scalar / Swagger API</span>
          <ExternalLink className="w-3.5 h-3.5 text-slate-500" />
        </a>

        {/* Notifications */}
        <button className="relative p-2 rounded-lg text-slate-500 hover:bg-slate-100 hover:text-slate-700 transition-colors">
          <Bell className="w-4 h-4" />
          <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-blue-600 rounded-full" />
        </button>

        <div className="h-5 w-px bg-slate-200 mx-1" />

        {/* User Profile Capsule */}
        <div className="flex items-center gap-2.5 pl-1">
          <div className="w-8 h-8 rounded-full bg-blue-100 border border-blue-200 flex items-center justify-center text-blue-700 font-semibold text-xs">
            <User className="w-4 h-4" />
          </div>
          <div className="hidden md:flex flex-col text-left">
            <span className="text-xs font-semibold text-slate-900 leading-tight">Senior Administrator</span>
            <span className="text-[10px] text-blue-600 font-medium">CEO / Admin</span>
          </div>
        </div>
      </div>
    </header>
  );
};
