import React from 'react';
import { cn } from '@/utils/cn';

export type BadgeVariant = 'healthy' | 'warning' | 'error' | 'info' | string;

interface StatusBadgeProps {
  status: BadgeVariant;
  children?: React.ReactNode;
  className?: string;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status, children, className }) => {
  const normalizeVariant = (val: string): 'healthy' | 'warning' | 'error' | 'info' => {
    switch (val.toLowerCase()) {
      case 'healthy':
      case 'active':
      case 'won':
      case 'completed':
      case 'qualified':
      case 'converted':
        return 'healthy';
      case 'warning':
      case 'pending':
      case 'inprogress':
      case 'in progress':
      case 'scheduled':
      case 'stagnant':
      case 'contacted':
        return 'warning';
      case 'error':
      case 'inactive':
      case 'lost':
      case 'unqualified':
      case 'cancelled':
        return 'error';
      default:
        return 'info';
    }
  };

  const variant = normalizeVariant(status);

  const styles = {
    healthy: 'bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-300 dark:border-emerald-800',
    warning: 'bg-amber-50 text-amber-700 border-amber-200 dark:bg-amber-950/40 dark:text-amber-300 dark:border-amber-800',
    error: 'bg-rose-50 text-rose-700 border-rose-200 dark:bg-rose-950/40 dark:text-rose-300 dark:border-rose-800',
    info: 'bg-blue-50 text-blue-700 border-blue-200 dark:bg-blue-950/40 dark:text-blue-300 dark:border-blue-800',
  };

  const dots = {
    healthy: 'bg-emerald-500',
    warning: 'bg-amber-500',
    error: 'bg-rose-500',
    info: 'bg-blue-500',
  };

  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium border select-none',
        styles[variant],
        className
      )}
    >
      <span className={cn('w-1.5 h-1.5 rounded-full animate-pulse shrink-0', dots[variant])} />
      {children || status}
    </span>
  );
};
