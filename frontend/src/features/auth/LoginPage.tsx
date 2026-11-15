import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import {
  ShieldCheck,
  Lock,
  Mail,
  Eye,
  EyeOff,
  AlertCircle,
  Loader2,
  CheckCircle2,
  Building2,
  UserCheck
} from 'lucide-react';
import { useAuthStore } from '@/stores/authStore';

const loginSchema = z.object({
  email: z.string().min(1, 'Email is required').email('Invalid email address format'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
});

type LoginFormData = z.infer<typeof loginSchema>;

export const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { login } = useAuthStore();

  const [showPassword, setShowPassword] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const from = (location.state as { from?: { pathname: string } })?.from?.pathname || '/';

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: 'admin@enterprisecrm.local',
      password: 'EnterpriseCRM@2026!',
    },
  });

  const onSubmit = async (data: LoginFormData) => {
    setErrorMessage(null);
    setIsSubmitting(true);

    try {
      await login(data);
      navigate(from, { replace: true });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Invalid credentials. Please verify your email and password.';
      setErrorMessage(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleQuickLogin = (email: string) => {
    setValue('email', email);
    setValue('password', 'EnterpriseCRM@2026!');
    setErrorMessage(null);
  };

  return (
    <div className="min-h-screen bg-slate-950 flex flex-col justify-center py-12 sm:px-6 lg:px-8 relative overflow-hidden font-sans">
      {/* Background Decorative Gradients */}
      <div className="absolute top-0 left-1/4 w-96 h-96 bg-blue-600/10 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute bottom-0 right-1/4 w-96 h-96 bg-indigo-600/10 rounded-full blur-3xl pointer-events-none" />

      <div className="sm:mx-auto sm:w-full sm:max-w-md z-10">
        <div className="flex justify-center items-center gap-3 mb-3">
          <div className="w-12 h-12 rounded-xl bg-gradient-to-tr from-blue-600 to-indigo-500 flex items-center justify-center shadow-lg shadow-blue-500/20 text-white">
            <Building2 className="w-6 h-6" />
          </div>
          <div>
            <h1 className="text-2xl font-bold tracking-tight text-white">
              Enterprise<span className="text-blue-500">CRM</span>
            </h1>
            <p className="text-xs text-slate-400 font-medium">Service Business Suite</p>
          </div>
        </div>
        <h2 className="text-center text-xl font-semibold text-slate-200 mt-2">
          Sign in to your organization
        </h2>
        <p className="text-center text-xs text-slate-400 mt-1">
          Authorized personnel only. Protected by RBAC & Session Security.
        </p>
      </div>

      <div className="mt-6 sm:mx-auto sm:w-full sm:max-w-md z-10 px-4 sm:px-0">
        <div className="bg-slate-900/90 backdrop-blur-md py-8 px-6 sm:px-10 shadow-2xl rounded-2xl border border-slate-800">
          {errorMessage && (
            <div className="mb-6 p-4 rounded-lg bg-red-950/50 border border-red-800/60 flex items-start gap-3 text-red-300 text-sm animate-in fade-in duration-200">
              <AlertCircle className="w-5 h-5 text-red-400 shrink-0 mt-0.5" />
              <div>
                <p className="font-medium text-red-200">Authentication Failed</p>
                <p className="text-xs text-red-300/90 mt-0.5">{errorMessage}</p>
              </div>
            </div>
          )}

          <form className="space-y-5" onSubmit={handleSubmit(onSubmit)}>
            {/* Email Field */}
            <div>
              <label className="block text-xs font-medium text-slate-300 mb-1.5">
                Work Email
              </label>
              <div className="relative rounded-lg shadow-sm">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
                  <Mail className="h-4 w-4" />
                </div>
                <input
                  type="email"
                  autoComplete="email"
                  placeholder="name@company.com"
                  {...register('email')}
                  className={`block w-full pl-10 pr-3 py-2.5 bg-slate-950/60 border rounded-lg text-sm text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors ${
                    errors.email ? 'border-red-500/80 focus:border-red-500' : 'border-slate-800 focus:border-blue-500'
                  }`}
                />
              </div>
              {errors.email && (
                <p className="mt-1.5 text-xs text-red-400 flex items-center gap-1">
                  <AlertCircle className="w-3.5 h-3.5" />
                  {errors.email.message}
                </p>
              )}
            </div>

            {/* Password Field */}
            <div>
              <div className="flex items-center justify-between mb-1.5">
                <label className="block text-xs font-medium text-slate-300">
                  Password
                </label>
                <span className="text-xs text-slate-500 cursor-not-allowed">
                  Forgot password?
                </span>
              </div>
              <div className="relative rounded-lg shadow-sm">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
                  <Lock className="h-4 w-4" />
                </div>
                <input
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="current-password"
                  placeholder="••••••••••••"
                  {...register('password')}
                  className={`block w-full pl-10 pr-10 py-2.5 bg-slate-950/60 border rounded-lg text-sm text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors ${
                    errors.password ? 'border-red-500/80 focus:border-red-500' : 'border-slate-800 focus:border-blue-500'
                  }`}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute inset-y-0 right-0 pr-3 flex items-center text-slate-400 hover:text-slate-200 transition-colors"
                >
                  {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              </div>
              {errors.password && (
                <p className="mt-1.5 text-xs text-red-400 flex items-center gap-1">
                  <AlertCircle className="w-3.5 h-3.5" />
                  {errors.password.message}
                </p>
              )}
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full flex justify-center items-center gap-2 py-2.5 px-4 border border-transparent rounded-lg shadow-sm text-sm font-semibold text-white bg-blue-600 hover:bg-blue-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-blue-500/25"
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="w-4 h-4 animate-spin" />
                  Authenticating...
                </>
              ) : (
                <>
                  <ShieldCheck className="w-4 h-4" />
                  Sign In
                </>
              )}
            </button>
          </form>

          {/* Quick Demo Role Selector */}
          <div className="mt-8 pt-6 border-t border-slate-800">
            <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-3 flex items-center gap-1.5">
              <UserCheck className="w-3.5 h-3.5 text-blue-400" />
              Quick Demo Accounts (RBAC)
            </p>
            <div className="grid grid-cols-2 gap-2 text-xs">
              <button
                type="button"
                onClick={() => handleQuickLogin('admin@enterprisecrm.local')}
                className="p-2 text-left bg-slate-950/60 hover:bg-blue-950/40 border border-slate-800 hover:border-blue-700/60 rounded-lg transition-all text-slate-300 hover:text-white"
              >
                <span className="font-semibold text-blue-400 block">Admin</span>
                <span className="text-[10px] text-slate-400">Full System Access</span>
              </button>

              <button
                type="button"
                onClick={() => handleQuickLogin('ceo@enterprisecrm.local')}
                className="p-2 text-left bg-slate-950/60 hover:bg-blue-950/40 border border-slate-800 hover:border-blue-700/60 rounded-lg transition-all text-slate-300 hover:text-white"
              >
                <span className="font-semibold text-purple-400 block">CEO / BOD</span>
                <span className="text-[10px] text-slate-400">Executive Approvals</span>
              </button>

              <button
                type="button"
                onClick={() => handleQuickLogin('manager@enterprisecrm.local')}
                className="p-2 text-left bg-slate-950/60 hover:bg-blue-950/40 border border-slate-800 hover:border-blue-700/60 rounded-lg transition-all text-slate-300 hover:text-white"
              >
                <span className="font-semibold text-amber-400 block">Manager</span>
                <span className="text-[10px] text-slate-400">Dept & Leads Scope</span>
              </button>

              <button
                type="button"
                onClick={() => handleQuickLogin('sales@enterprisecrm.local')}
                className="p-2 text-left bg-slate-950/60 hover:bg-blue-950/40 border border-slate-800 hover:border-blue-700/60 rounded-lg transition-all text-slate-300 hover:text-white"
              >
                <span className="font-semibold text-emerald-400 block">Sales Rep</span>
                <span className="text-[10px] text-slate-400">Deals & Quotes</span>
              </button>

              <button
                type="button"
                onClick={() => handleQuickLogin('accountant@enterprisecrm.local')}
                className="col-span-2 p-2 text-left bg-slate-950/60 hover:bg-blue-950/40 border border-slate-800 hover:border-blue-700/60 rounded-lg transition-all text-slate-300 hover:text-white"
              >
                <span className="font-semibold text-cyan-400 block">Accountant</span>
                <span className="text-[10px] text-slate-400">Invoices, Payments & Payroll</span>
              </button>
            </div>
            <p className="text-[10px] text-slate-500 mt-2 text-center">
              Password for all seeded demo accounts: <code className="text-slate-400 bg-slate-950 px-1 py-0.5 rounded">EnterpriseCRM@2026!</code>
            </p>
          </div>
        </div>

        <div className="mt-6 flex items-center justify-center gap-6 text-xs text-slate-500">
          <span className="flex items-center gap-1.5">
            <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500" />
            Supabase PostgreSQL SSL
          </span>
          <span className="flex items-center gap-1.5">
            <ShieldCheck className="w-3.5 h-3.5 text-blue-500" />
            JWT Token Rotation
          </span>
        </div>
      </div>
    </div>
  );
};
