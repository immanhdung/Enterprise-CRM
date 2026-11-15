import React from 'react';
import { cn } from '@/utils/cn';

interface StatusBadgeProps {
  status: 'healthy' | 'warning' | 'error' | 'info';
  children: React.ReactNode;
  className?: string;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status, children, className }) => {
  const styles = {
    healthy: 'bg-emerald-50 text-emerald-700 border-emerald-200',
    warning: 'bg-amber-50 text-amber-700 border-amber-200',
    error: 'bg-rose-50 text-rose-700 border-rose-200',
    info: 'bg-blue-50 text-blue-700 border-blue-200',
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
        'inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium border',
        styles[status],
        className
      )}
    >
      <span className={cn('w-1.5 h-1.5 rounded-full animate-pulse', dots[status])} />
      {children}
    </span>
  );
};
