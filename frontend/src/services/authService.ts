import { apiClient } from './apiClient';
import type { ApiResponse } from '@/types/api';
import type { AuthResponse, LoginCredentials, UserProfile } from '@/types/auth';

export const authService = {
  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const response = await apiClient.post<ApiResponse<AuthResponse>>('/auth/login', credentials);
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || 'Login failed.');
    }
    return response.data.data;
  },

  async refreshToken(refreshToken: string): Promise<AuthResponse> {
    const response = await apiClient.post<ApiResponse<AuthResponse>>('/auth/refresh-token', {
      refreshToken,
    });
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || 'Failed to refresh token.');
    }
    return response.data.data;
  },

  async logout(): Promise<void> {
    try {
      await apiClient.post('/auth/logout');
    } catch {
      // Ignore network errors on logout to ensure local cleanup
    }
  },

  async getCurrentUser(): Promise<UserProfile> {
    const response = await apiClient.get<ApiResponse<UserProfile>>('/auth/me');
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || 'Failed to load user profile.');
    }
    return response.data.data;
  },

  async changePassword(passwords: { currentPassword: string; newPassword: string; confirmNewPassword: string }): Promise<void> {
    const response = await apiClient.post<ApiResponse<boolean>>('/auth/change-password', passwords);
    if (!response.data.success) {
      throw new Error(response.data.message || 'Failed to change password.');
    }
  },
};
