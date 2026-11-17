import { apiClient } from './apiClient';
import type { ApiResponse } from '../types/api';
import type { PagedResult } from '../types/crm';
import type {
  PaymentSchedule,
  CreatePaymentScheduleRequest,
  GenerateInstallmentSchedulesRequest,
  Payment,
  CreatePaymentRequest,
  ConfirmPaymentRequest,
  ReversePaymentRequest,
  ContractFinancialSummary,
  Invoice,
  IssueInvoiceRequest,
  CancelInvoiceRequest,
  InvoiceRequest,
  CreateInvoiceRequestInput,
  AccountsReceivableItem,
  ReceivableAgingSummary,
  ReceivableCollectionLog,
  CreateReceivableCollectionLogRequest,
  RevenueAnalyticsResponse,
  FinancialReconciliationRecord,
  RunReconciliationRequest,
  ResolveDiscrepancyRequest,
  ReconciliationItem,
} from '../types/finance';

export const financeService = {
  // ==========================================
  // 1. Payment Schedules
  // ==========================================
  async getContractSchedules(contractId: string): Promise<PaymentSchedule[]> {
    const res = await apiClient.get<ApiResponse<PaymentSchedule[]>>(`/payment-schedules/contract/${contractId}`);
    return res.data.data!;
  },

  async createSchedule(contractId: string, req: CreatePaymentScheduleRequest): Promise<PaymentSchedule> {
    const res = await apiClient.post<ApiResponse<PaymentSchedule>>(`/payment-schedules/contract/${contractId}`, req);
    return res.data.data!;
  },

  async generateContractSchedules(contractId: string, req: GenerateInstallmentSchedulesRequest): Promise<PaymentSchedule[]> {
    const res = await apiClient.post<ApiResponse<PaymentSchedule[]>>(`/payment-schedules/contract/${contractId}/generate`, req);
    return res.data.data!;
  },

  async deleteSchedule(id: string): Promise<boolean> {
    const res = await apiClient.delete<ApiResponse<boolean>>(`/payment-schedules/${id}`);
    return res.data.data!;
  },

  // ==========================================
  // 2. Payments
  // ==========================================
  async getPayments(params?: {
    pageNumber?: number;
    pageSize?: number;
    searchTerm?: string;
    status?: string;
    method?: number;
    contractId?: string;
    accountId?: string;
    fromDate?: string;
    toDate?: string;
  }): Promise<PagedResult<Payment>> {
    const res = await apiClient.get<ApiResponse<PagedResult<Payment>>>('/payments', { params });
    return res.data.data!;
  },

  async getPaymentById(id: string): Promise<Payment> {
    const res = await apiClient.get<ApiResponse<Payment>>(`/payments/${id}`);
    return res.data.data!;
  },

  async createPayment(req: CreatePaymentRequest): Promise<Payment> {
    const res = await apiClient.post<ApiResponse<Payment>>('/payments', req);
    return res.data.data!;
  },

  async confirmPayment(id: string, req: ConfirmPaymentRequest): Promise<Payment> {
    const res = await apiClient.post<ApiResponse<Payment>>(`/payments/${id}/confirm`, req);
    return res.data.data!;
  },

  async reversePayment(id: string, req: ReversePaymentRequest): Promise<Payment> {
    const res = await apiClient.post<ApiResponse<Payment>>(`/payments/${id}/reverse`, req);
    return res.data.data!;
  },

  async getContractFinancialSummary(contractId: string): Promise<ContractFinancialSummary> {
    const res = await apiClient.get<ApiResponse<ContractFinancialSummary>>(`/payments/contract/${contractId}/summary`);
    return res.data.data!;
  },

  // ==========================================
  // 3. Invoices
  // ==========================================
  async getInvoices(params?: {
    pageNumber?: number;
    pageSize?: number;
    searchTerm?: string;
    status?: number;
    contractId?: string;
    accountId?: string;
    fromDate?: string;
    toDate?: string;
  }): Promise<PagedResult<Invoice>> {
    const res = await apiClient.get<ApiResponse<PagedResult<Invoice>>>('/invoices', { params });
    return res.data.data!;
  },

  async getInvoiceById(id: string): Promise<Invoice> {
    const res = await apiClient.get<ApiResponse<Invoice>>(`/invoices/${id}`);
    return res.data.data!;
  },

  async issueInvoice(req: IssueInvoiceRequest): Promise<Invoice> {
    const res = await apiClient.post<ApiResponse<Invoice>>('/invoices', req);
    return res.data.data!;
  },

  async cancelInvoice(id: string, req: CancelInvoiceRequest): Promise<Invoice> {
    const res = await apiClient.post<ApiResponse<Invoice>>(`/invoices/${id}/cancel`, req);
    return res.data.data!;
  },

  async syncInvoiceStatus(id: string): Promise<Invoice> {
    const res = await apiClient.post<ApiResponse<Invoice>>(`/invoices/${id}/sync`);
    return res.data.data!;
  },

  async downloadInvoicePdf(id: string): Promise<Blob> {
    const res = await apiClient.get(`/invoices/${id}/pdf`, { responseType: 'blob' });
    return res.data;
  },

  async getInvoiceRequests(params?: {
    pageNumber?: number;
    pageSize?: number;
    status?: string;
    contractId?: string;
    accountId?: string;
  }): Promise<PagedResult<InvoiceRequest>> {
    const res = await apiClient.get<ApiResponse<PagedResult<InvoiceRequest>>>('/invoices/requests', { params });
    return res.data.data!;
  },

  async createInvoiceRequest(req: CreateInvoiceRequestInput): Promise<InvoiceRequest> {
    const res = await apiClient.post<ApiResponse<InvoiceRequest>>('/invoices/requests', req);
    return res.data.data!;
  },

  async approveInvoiceRequest(id: string): Promise<Invoice> {
    const res = await apiClient.post<ApiResponse<Invoice>>(`/invoices/requests/${id}/approve`);
    return res.data.data!;
  },

  async rejectInvoiceRequest(id: string, reason: string): Promise<boolean> {
    const res = await apiClient.post<ApiResponse<boolean>>(`/invoices/requests/${id}/reject`, reason, {
      headers: { 'Content-Type': 'application/json' },
    });
    return res.data.data!;
  },

  // ==========================================
  // 4. Receivables & Debt Aging
  // ==========================================
  async getAgingSummary(): Promise<ReceivableAgingSummary> {
    const res = await apiClient.get<ApiResponse<ReceivableAgingSummary>>('/finance/receivables/summary');
    return res.data.data!;
  },

  async getReceivablesList(params?: {
    searchTerm?: string;
    agingBracket?: string;
    accountId?: string;
    onlyOverdue?: boolean;
  }): Promise<AccountsReceivableItem[]> {
    const res = await apiClient.get<ApiResponse<AccountsReceivableItem[]>>('/finance/receivables', { params });
    return res.data.data!;
  },

  async getCollectionLogs(params?: {
    accountId?: string;
    contractId?: string;
  }): Promise<ReceivableCollectionLog[]> {
    const res = await apiClient.get<ApiResponse<ReceivableCollectionLog[]>>('/finance/collection-logs', { params });
    return res.data.data!;
  },

  async addCollectionLog(req: CreateReceivableCollectionLogRequest): Promise<ReceivableCollectionLog> {
    const res = await apiClient.post<ApiResponse<ReceivableCollectionLog>>('/finance/collection-logs', req);
    return res.data.data!;
  },

  // ==========================================
  // 5. Revenue Management
  // ==========================================
  async getRevenueAnalytics(params?: {
    fromDate?: string;
    toDate?: string;
    periodType?: string;
    customerId?: string;
    salesEmployeeId?: string;
    contractId?: string;
  }): Promise<RevenueAnalyticsResponse> {
    const res = await apiClient.get<ApiResponse<RevenueAnalyticsResponse>>('/finance/revenue', { params });
    return res.data.data!;
  },

  // ==========================================
  // 6. Financial Reconciliation
  // ==========================================
  async getReconciliationHistory(): Promise<FinancialReconciliationRecord[]> {
    const res = await apiClient.get<ApiResponse<FinancialReconciliationRecord[]>>('/finance/reconciliation');
    return res.data.data!;
  },

  async getReconciliationById(id: string): Promise<FinancialReconciliationRecord> {
    const res = await apiClient.get<ApiResponse<FinancialReconciliationRecord>>(`/finance/reconciliation/${id}`);
    return res.data.data!;
  },

  async runReconciliation(req: RunReconciliationRequest): Promise<FinancialReconciliationRecord> {
    const res = await apiClient.post<ApiResponse<FinancialReconciliationRecord>>('/finance/reconciliation/run', req);
    return res.data.data!;
  },

  async resolveDiscrepancy(itemId: string, req: ResolveDiscrepancyRequest): Promise<ReconciliationItem> {
    const res = await apiClient.post<ApiResponse<ReconciliationItem>>(`/finance/reconciliation/items/${itemId}/resolve`, req);
    return res.data.data!;
  },
};
