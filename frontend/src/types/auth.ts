export type UserRole = 'Admin' | 'CEO_BOD' | 'Manager' | 'Sales' | 'Accountant';

export type DataScope = 'Own' | 'Team' | 'Department' | 'All';

export interface UserProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  isActive: boolean;
  lastLoginAt: string | null;
  roles: string[];
  permissions: string[];
  departmentId: string | null;
  departmentName: string | null;
  teamId: string | null;
  teamName: string | null;
  dataScope: DataScope;
  employeeCode: string | null;
  position: string | null;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresInSeconds: number;
  user: UserProfile;
}

export interface LoginCredentials {
  email: string;
  password: string;
}
