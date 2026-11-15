import React from 'react';
import { Navigate, useLocation, Outlet } from 'react-router-dom';
import { useAuthStore } from '@/stores/authStore';

interface ProtectedRouteProps {
  requiredRoles?: string[];
  requiredPermissions?: string[];
  children?: React.ReactNode;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  requiredRoles,
  requiredPermissions,
  children,
}) => {
  const location = useLocation();
  const { isAuthenticated, user, hasRole, hasPermission } = useAuthStore();

  if (!isAuthenticated || !user) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Check Role Requirements
  if (requiredRoles && requiredRoles.length > 0) {
    const hasAnyRequiredRole = requiredRoles.some((role) => hasRole(role));
    if (!hasAnyRequiredRole) {
      return <Navigate to="/forbidden" replace />;
    }
  }

  // Check Permission Requirements
  if (requiredPermissions && requiredPermissions.length > 0) {
    const hasAllPermissions = requiredPermissions.every((perm) => hasPermission(perm));
    if (!hasAllPermissions) {
      return <Navigate to="/forbidden" replace />;
    }
  }

  return children ? <>{children}</> : <Outlet />;
};
