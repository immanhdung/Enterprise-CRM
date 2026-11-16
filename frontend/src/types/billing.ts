export interface ServiceCategory {
  id: string;
  code: string;
  name: string;
  description?: string;
  orderIndex: number;
  isActive: boolean;
  servicesCount?: number;
}

export interface ServiceItem {
  id: string;
  code: string;
  name: string;
  categoryId?: string;
  categoryName?: string;
  category: string;
  unit: string;
  basePrice: number;
  costPrice: number;
  vatRate: number;
  description?: string;
  isActive: boolean;
}

export interface ServicePriceHistory {
  id: string;
  serviceId: string;
  oldPrice: number;
  newPrice: number;
  changedById?: string;
  changedByName: string;
  effectiveDate: string;
  reason?: string;
}

export interface PriceListItem {
  id: string;
  priceListId: string;
  serviceId: string;
  serviceCode: string;
  serviceName: string;
  unitPrice: number;
  minQuantity: number;
  maxDiscountRate: number;
}

export interface PriceList {
  id: string;
  code: string;
  name: string;
  description?: string;
  currency: string;
  startDate?: string;
  endDate?: string;
  isDefault: boolean;
  isActive: boolean;
  items: PriceListItem[];
}

export interface QuotationItem {
  id: string;
  serviceId: string;
  serviceCode: string;
  serviceName: string;
  unit: string;
  quantity: number;
  unitPrice: number;
  discountRate: number;
  discountAmount: number;
  vatRate: number;
  vatAmount: number;
  lineTotal: number;
  notes?: string;
}

export interface QuotationApprovalHistory {
  id: string;
  quotationId: string;
  action: string;
  actorUserId?: string;
  actorName: string;
  comments?: string;
  timestamp: string;
}

export type QuotationStatus = 'Draft' | 'Submitted' | 'Approved' | 'Rejected' | 'Sent' | 'Expired' | 'Accepted';

export interface Quotation {
  id: string;
  quotationNumber: string;
  version: number;
  revisionNumber: string;
  parentQuotationId?: string;
  dealId?: string;
  dealTitle?: string;
  accountId: string;
  accountName: string;
  contactId?: string;
  contactName?: string;
  priceListId?: string;
  priceListName?: string;
  validUntil: string;
  subtotal: number;
  discountAmount: number;
  vatAmount: number;
  totalAmount: number;
  status: QuotationStatus;
  termsAndConditions?: string;
  notes?: string;
  pdfUrl?: string;
  submittedAt?: string;
  submittedById?: string;
  approvedBy?: string;
  approvedAt?: string;
  approvalNotes?: string;
  rejectedBy?: string;
  rejectedAt?: string;
  rejectionReason?: string;
  sentAt?: string;
  sentToEmail?: string;
  sentById?: string;
  createdAt: string;
  updatedAt: string;
  items: QuotationItem[];
  approvalHistories: QuotationApprovalHistory[];
}

export interface ContractTemplate {
  id: string;
  code: string;
  name: string;
  description?: string;
  contentTemplate: string;
  version: number;
  isActive: boolean;
}

export interface ContractItem {
  id: string;
  contractId: string;
  serviceId?: string;
  serviceName: string;
  serviceCode?: string;
  unit?: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  notes?: string;
}

export interface ContractAppendix {
  id: string;
  contractId: string;
  appendixNumber: string;
  title: string;
  description?: string;
  adjustedValue: number;
  effectiveDate: string;
  status: string;
  documentUrl?: string;
  createdAt: string;
}

export interface ContractHistory {
  id: string;
  contractId: string;
  action: string;
  oldStatus?: string;
  newStatus?: string;
  actorUserId?: string;
  actorName: string;
  notes?: string;
  timestamp: string;
}

export interface SignatureRequest {
  id: string;
  contractId: string;
  provider: string;
  externalEnvelopeId?: string;
  signerName: string;
  signerEmail: string;
  status: string; // Sent, Delivered, Signed, Declined, Expired
  sentAt: string;
  signedAt?: string;
  signingUrl?: string;
}

export type ContractStatus = 
  | 'Draft' 
  | 'PendingReview' 
  | 'Approved' 
  | 'PendingSignature' 
  | 'Signed' 
  | 'Active' 
  | 'Expired' 
  | 'Terminated' 
  | 'Renewed';

export interface Contract {
  id: string;
  contractNumber: string;
  title: string;
  accountId: string;
  accountName: string;
  contactId?: string;
  contactName?: string;
  dealId?: string;
  quotationId?: string;
  quotationNumber?: string;
  templateId?: string;
  templateName?: string;
  parentContractId?: string;
  status: ContractStatus;
  startDate: string;
  endDate: string;
  signedAt?: string;
  totalValue: number;
  scopeOfWork?: string;
  paymentTerms?: string;
  specialTerms?: string;
  documentUrl?: string;
  esignStatus: string;
  signedByClientName?: string;
  signedByClientEmail?: string;
  signerIpAddress?: string;
  createdAt: string;
  updatedAt: string;
  items: ContractItem[];
  appendices: ContractAppendix[];
  histories: ContractHistory[];
  signatureRequests: SignatureRequest[];
}

export interface SignatureSessionResult {
  signatureRequestId: string;
  externalEnvelopeId: string;
  signingUrl: string;
  provider: string;
}
