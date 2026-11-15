export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  message: string | null;
  errors: string[] | null;
  timestamp: string;
}

export interface DatabaseHealthDto {
  isConnected: boolean;
  provider: string;
  host: string;
  databaseName: string;
  responseTimeMs: number;
  serverVersion?: string;
  message?: string;
}

export interface SystemHealthDto {
  status: string;
  environment: string;
  version: string;
  serverTimeUtc: string;
  uptime: string;
  database: DatabaseHealthDto;
}
