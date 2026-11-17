export interface PaymentSchedule {
  id: string;
  contractId: string;
  contractNumber?: string;
  accountId?: string;
  accountName?: string;
  installmentNumber: number;
  milestoneTitle?: string;
  percentage: number;
  dueDate: string;
  amount: number;
  paidAmount: number;
  status: 'Pending' | 'PartiallyPaid' | 'Paid' | 'Overdue';
  notes?: string;
  paidAt?: string;
  createdAt: string;
}

export interface CreatePaymentScheduleRequest {
  milestoneTitle: string;
  installmentNumber: number;
  percentage: number;
  dueDate: string;
  amount: number;
  notes?: string;
}

export interface InstallmentPlanItemInput {
  milestoneTitle: string;
  percentage: number;
  dueDate: string;
  notes?: string;
}

export interface GenerateInstallmentSchedulesRequest {
  installments: InstallmentPlanItemInput[];
}

export interface Payment {
  id: string;
  paymentNumber: string;
  contractId: string;
  contractNumber?: string;
  accountId: string;
  accountName?: string;
  paymentScheduleId?: string;
  milestoneTitle?: string;
  amount: number;
  paymentDate: string;
  method: number; // 1 = BankTransfer, 2 = CreditCard, 3 = Cash, 4 = Cheque
  transactionReference?: string;
  bankName?: string;
  bankAccountNumber?: string;
  senderName?: string;
  notes?: string;
  attachmentUrl?: string;
  status: 'Pending' | 'Confirmed' | 'Failed' | 'Cancelled' | 'Reversed';
  confirmedAt?: string;
  confirmedById?: string;
  confirmedByName?: string;
  cancelledAt?: string;
  cancelledById?: string;
  reversalReason?: string;
  createdAt: string;
}

export interface CreatePaymentRequest {
  contractId: string;
  accountId?: string;
  paymentScheduleId?: string;
  amount: number;
  paymentDate: string;
  method: number;
  transactionReference?: string;
  bankName?: string;
  bankAccountNumber?: string;
  senderName?: string;
  notes?: string;
  attachmentUrl?: string;
}

export interface ConfirmPaymentRequest {
  notes?: string;
}

export interface ReversePaymentRequest {
  reversalReason: string;
}

export interface ContractFinancialSummary {
  contractId: string;
  contractNumber: string;
  contractTitle: string;
  accountId: string;
  accountName: string;
  totalValue: number;
  paidAmount: number;
  outstandingAmount: number;
  remainingContractValue: number;
  totalSchedulesCount: number;
  paidSchedulesCount: number;
  overdueSchedulesCount: number;
  schedules: PaymentSchedule[];
  payments: Payment[];
}

export interface InvoiceItem {
  id: string;
  invoiceId: string;
  serviceId?: string;
  serviceName?: string;
  lineNumber: number;
  description: string;
  unit: string;
  quantity: number;
  unitPrice: number;
  taxRate: number;
  taxAmount: number;
  lineTotal: number;
}

export interface InvoiceItemInput {
  serviceId?: string;
  description: string;
  unit?: string;
  quantity: number;
  unitPrice: number;
  taxRate?: number;
}

export interface Invoice {
  id: string;
  invoiceNumber: string;
  invoiceSeries: string;
  contractId: string;
  contractNumber?: string;
  accountId: string;
  accountName?: string;
  customerName: string;
  customerTaxCode?: string;
  customerAddress?: string;
  customerEmail?: string;
  issueDate: string;
  subtotal: number;
  taxRate: number;
  taxAmount: number;
  totalAmount: number;
  status: number; // 1 = Draft, 2 = Published, 3 = SentToCustomer, 4 = Cancelled
  electronicInvoiceProvider?: string;
  electronicInvoiceId?: string;
  lookupCode?: string;
  lookupUrl?: string;
  errorMessage?: string;
  retryCount: number;
  cancellationReason?: string;
  cancelledAt?: string;
  createdAt: string;
  items?: InvoiceItem[];
}

export interface IssueInvoiceRequest {
  contractId: string;
  accountId?: string;
  invoiceRequestId?: string;
  paymentId?: string;
  customerName: string;
  customerTaxCode?: string;
  customerAddress?: string;
  customerEmail?: string;
  issueDate: string;
  electronicInvoiceProvider?: string;
  items: InvoiceItemInput[];
}

export interface CancelInvoiceRequest {
  cancellationReason: string;
}

export interface InvoiceRequest {
  id: string;
  contractId: string;
  contractNumber?: string;
  accountId: string;
  accountName?: string;
  paymentId?: string;
  paymentNumber?: string;
  amount: number;
  status: string;
  requestedByUserId: string;
  requestedByName?: string;
  requestDate: string;
  notes?: string;
  issuedInvoiceId?: string;
  issuedInvoiceNumber?: string;
  createdAt: string;
}

export interface CreateInvoiceRequestInput {
  contractId: string;
  accountId?: string;
  paymentId?: string;
  amount: number;
  notes?: string;
}

export interface AccountsReceivableItem {
  accountId: string;
  accountName: string;
  contactPerson?: string;
  contactPhone?: string;
  contactEmail?: string;
  contractId: string;
  contractNumber: string;
  paymentScheduleId: string;
  milestoneTitle: string;
  dueDate: string;
  totalAmount: number;
  paidAmount: number;
  outstandingAmount: number;
  overdueDays: number;
  agingBracket: 'Current' | '1-7 Days' | '8-30 Days' | '31-60 Days' | '>60 Days';
  lastCollectionActivityDate?: string;
  lastCollectionStatus?: string;
  assignedSalesEmployeeId?: string;
  assignedSalesEmployeeName?: string;
}

export interface ReceivableAgingSummary {
  totalReceivables: number;
  totalOverdue: number;
  currentAmount: number;
  overdue1To7Days: number;
  overdue8To30Days: number;
  overdue31To60Days: number;
  overdueMoreThan60Days: number;
  accountsWithDebtCount: number;
  overdueContractsCount: number;
}

export interface ReceivableCollectionLog {
  id: string;
  accountId: string;
  accountName?: string;
  contractId: string;
  contractNumber?: string;
  paymentScheduleId?: string;
  activityType: string;
  notes: string;
  loggedById: string;
  loggedByName: string;
  loggedAt: string;
  nextFollowUpDate?: string;
  assignedToUserId?: string;
  assignedToUserName?: string;
  status: string;
  createdAt: string;
}

export interface CreateReceivableCollectionLogRequest {
  accountId: string;
  contractId: string;
  paymentScheduleId?: string;
  activityType: string;
  notes: string;
  nextFollowUpDate?: string;
  status?: string;
}

export interface RevenueSummary {
  totalContractValue: number;
  totalInvoicedAmount: number;
  totalCollectedCash: number;
  totalOutstandingReceivables: number;
  invoicedPercentage: number;
  cashCollectionRate: number;
}

export interface RevenuePeriodItem {
  periodLabel: string;
  startDate: string;
  endDate: string;
  contractValue: number;
  invoicedAmount: number;
  collectedCash: number;
  outstandingAmount: number;
}

export interface RevenueByCustomer {
  accountId: string;
  customerName: string;
  contractCount: number;
  totalContractValue: number;
  invoicedAmount: number;
  collectedCash: number;
  outstandingAmount: number;
}

export interface RevenueByContract {
  contractId: string;
  contractNumber: string;
  customerName: string;
  totalValue: number;
  invoicedAmount: number;
  collectedCash: number;
  outstandingAmount: number;
  status: string;
}

export interface RevenueBySales {
  employeeId: string;
  employeeName: string;
  contractCount: number;
  totalContractValue: number;
  invoicedAmount: number;
  collectedCash: number;
}

export interface RevenueByService {
  serviceId?: string;
  serviceName: string;
  quantitySold: number;
  invoicedAmount: number;
}

export interface RevenueAnalyticsResponse {
  summary: RevenueSummary;
  trendByPeriod: RevenuePeriodItem[];
  byCustomer: RevenueByCustomer[];
  byContract: RevenueByContract[];
  bySales: RevenueBySales[];
  byService: RevenueByService[];
}

export interface FinancialReconciliationRecord {
  id: string;
  period: string;
  reconciliationDate: string;
  totalContractValue: number;
  totalExpectedRevenue: number;
  totalCollectedRevenue: number;
  totalInvoicedRevenue: number;
  discrepancyAmount: number;
  status: 'Balanced' | 'DiscrepancyDetected' | 'Reconciled' | 'Adjusted';
  notes?: string;
  reconciledById: string;
  reconciledByName: string;
  approvedById?: string;
  approvedByName?: string;
  approvedAt?: string;
  createdAt: string;
  items?: ReconciliationItem[];
}

export interface ReconciliationItem {
  id: string;
  reconciliationRecordId: string;
  contractId?: string;
  contractNumber?: string;
  paymentId?: string;
  paymentNumber?: string;
  invoiceId?: string;
  invoiceNumber?: string;
  discrepancyType: 'MissingPayment' | 'UnmatchedInvoice' | 'AmountMismatch' | 'DuplicateTransaction';
  expectedAmount: number;
  actualAmount: number;
  difference: number;
  resolutionStatus: 'Pending' | 'Confirmed' | 'Adjusted' | 'Dismissed';
  resolutionNotes?: string;
}

export interface RunReconciliationRequest {
  periodStart: string;
  periodEnd: string;
  notes?: string;
}

export interface ResolveDiscrepancyRequest {
  action: 'ConfirmAdjustment' | 'Dismiss';
  resolutionNotes: string;
}
