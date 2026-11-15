import { create } from 'zustand';
import type { UserProfile, LoginCredentials } from '@/types/auth';
import { authService } from '@/services/authService';

interface AuthState {
  user: UserProfile | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginCredentials) => Promise<void>;
  logout: () => Promise<void>;
  hasRole: (role: string) => boolean;
  hasPermission: (permission: string) => boolean;
  hasAnyRole: (roles: string[]) => boolean;
  refreshProfile: () => Promise<void>;
}

// Initial state from localStorage for persistence across reloads
const storedToken = localStorage.getItem('crm_access_token');
const storedRefreshToken = localStorage.getItem('crm_refresh_token');
let storedUser: UserProfile | null = null;

try {
  const userJson = localStorage.getItem('crm_user');
  if (userJson) {
    storedUser = JSON.parse(userJson);
  }
} catch {
  storedUser = null;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: storedUser,
  accessToken: storedToken,
  refreshToken: storedRefreshToken,
  isAuthenticated: !!storedToken && !!storedUser,
  isLoading: false,

  login: async (credentials: LoginCredentials) => {
    set({ isLoading: true });
    try {
      const response = await authService.login(credentials);

      localStorage.setItem('crm_access_token', response.accessToken);
      localStorage.setItem('crm_refresh_token', response.refreshToken);
      localStorage.setItem('crm_user', JSON.stringify(response.user));

      set({
        user: response.user,
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
        isAuthenticated: true,
        isLoading: false,
      });
    } catch (error) {
      set({ isLoading: false });
      throw error;
    }
  },

  logout: async () => {
    try {
      await authService.logout();
    } finally {
      localStorage.removeItem('crm_access_token');
      localStorage.removeItem('crm_refresh_token');
      localStorage.removeItem('crm_user');

      set({
        user: null,
        accessToken: null,
        refreshToken: null,
        isAuthenticated: false,
        isLoading: false,
      });
    }
  },

  hasRole: (role: string) => {
    const user = get().user;
    if (!user) return false;
    if (user.roles.includes('Admin')) return true; // SuperAdmin bypass
    return user.roles.includes(role);
  },

  hasAnyRole: (roles: string[]) => {
    const user = get().user;
    if (!user) return false;
    if (user.roles.includes('Admin')) return true;
    return roles.some((r) => user.roles.includes(r));
  },

  hasPermission: (permission: string) => {
    const user = get().user;
    if (!user) return false;
    if (user.roles.includes('Admin')) return true;
    return user.permissions.includes(permission);
  },

  refreshProfile: async () => {
    if (!get().isAuthenticated) return;
    try {
      const profile = await authService.getCurrentUser();
      localStorage.setItem('crm_user', JSON.stringify(profile));
      set({ user: profile });
    } catch {
      // If profile fetch fails due to 401, the interceptor handles refresh or logout
    }
  },
}));
