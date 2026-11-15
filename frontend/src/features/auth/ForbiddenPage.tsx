import React from 'react';
import { Link } from 'react-router-dom';
import { ShieldAlert, ArrowLeft, Home } from 'lucide-react';
import { useAuthStore } from '@/stores/authStore';

export const ForbiddenPage: React.FC = () => {
  const { user } = useAuthStore();

  return (
    <div className="min-h-screen bg-slate-950 flex flex-col items-center justify-center p-6 text-center">
      <div className="w-16 h-16 rounded-2xl bg-amber-500/10 border border-amber-500/30 flex items-center justify-center text-amber-400 mb-6 shadow-xl shadow-amber-500/10">
        <ShieldAlert className="w-8 h-8" />
      </div>

      <h1 className="text-3xl font-bold text-white tracking-tight">403 - Access Forbidden</h1>
      <p className="text-slate-400 text-sm max-w-md mt-2">
        You do not have the required role or permission to access this resource. Your current roles:{' '}
        <span className="text-slate-200 font-semibold">{user?.roles.join(', ') || 'None'}</span>
      </p>

      <div className="mt-8 flex items-center gap-3">
        <Link
          to="/"
          className="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-blue-600 hover:bg-blue-500 text-white text-sm font-medium transition-colors shadow-lg shadow-blue-600/20"
        >
          <Home className="w-4 h-4" />
          Back to Dashboard
        </Link>
        <button
          onClick={() => window.history.back()}
          className="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-slate-900 hover:bg-slate-800 text-slate-300 border border-slate-800 text-sm font-medium transition-colors"
        >
          <ArrowLeft className="w-4 h-4" />
          Previous Page
        </button>
      </div>
    </div>
  );
};
