export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// ----------------- Lead Types -----------------
export interface LeadDto {
  id: string;
  code: string;
  fullName: string;
  companyName?: string;
  phone?: string;
  email?: string;
  position?: string;
  source?: string;
  campaign?: string;
  description?: string;
  notes?: string;
  status: string;
  qualityScore: number;
  priority: string;
  expectedBudget?: number;
  nextFollowUpDate?: string;
  assignedEmployeeId?: string;
  assignedEmployeeName?: string;
  convertedAccountId?: string;
  convertedAccountName?: string;
  convertedContactId?: string;
  convertedContactName?: string;
  convertedDealId?: string;
  convertedDealTitle?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateLeadRequest {
  fullName: string;
  companyName?: string;
  phone?: string;
  email?: string;
  position?: string;
  source?: string;
  campaign?: string;
  description?: string;
  priority?: string;
  expectedBudget?: number;
  nextFollowUpDate?: string;
  assignedEmployeeId?: string;
}

export interface UpdateLeadRequest {
  fullName: string;
  companyName?: string;
  phone?: string;
  email?: string;
  position?: string;
  source?: string;
  campaign?: string;
  description?: string;
  status: string;
  qualityScore: number;
  priority: string;
  expectedBudget?: number;
  nextFollowUpDate?: string;
  assignedEmployeeId?: string;
}

export interface AssignLeadRequest {
  mode: 'Manual' | 'RoundRobin' | 'WorkloadBased';
  targetEmployeeId?: string;
  notes?: string;
}

export interface ConvertLeadRequest {
  accountName?: string;
  existingAccountId?: string;
  existingContactId?: string;
  dealTitle: string;
  dealAmount: number;
  expectedClosingDate?: string;
  stageId?: string;
}

export interface DuplicateCheckResult {
  hasDuplicate: boolean;
  message: string;
  matchedField?: string;
  existingLeadId?: string;
  existingLeadName?: string;
  existingLeadEmail?: string;
  existingLeadPhone?: string;
}

export interface LeadActivityDto {
  id: string;
  type: string;
  subject: string;
  notes?: string;
  scheduledAt?: string;
  completedAt?: string;
  performedByName: string;
  createdAt: string;
}

// ----------------- Account Types -----------------
export interface AccountDto {
  id: string;
  code: string;
  name: string;
  industry?: string;
  classification?: string;
  status: string;
  taxId?: string;
  phone?: string;
  email?: string;
  website?: string;
  address?: string;
  city?: string;
  country?: string;
  description?: string;
  assignedEmployeeId?: string;
  assignedEmployeeName?: string;
  contactsCount: number;
  dealsCount: number;
  activeContractsCount: number;
  totalDealsValue: number;
  createdAt: string;
  updatedAt?: string;
}

export interface AccountContactItemDto {
  id: string;
  fullName: string;
  email?: string;
  phone?: string;
  position?: string;
  isPrimary: boolean;
  status: string;
}

export interface AccountDealItemDto {
  id: string;
  code: string;
  title: string;
  stageName: string;
  stageColor?: string;
  amount: number;
  expectedRevenue: number;
  status: string;
  expectedClosingDate?: string;
}

export interface AccountContractItemDto {
  id: string;
  contractNumber: string;
  title: string;
  status: string;
  totalValue: number;
  startDate: string;
  endDate?: string;
}

export interface AccountProjectItemDto {
  id: string;
  code: string;
  name: string;
  status: string;
  progressPercent: number;
  startDate?: string;
  endDate?: string;
}

export interface AccountDetailDto {
  id: string;
  code: string;
  name: string;
  industry?: string;
  classification?: string;
  status: string;
  taxId?: string;
  phone?: string;
  email?: string;
  website?: string;
  address?: string;
  city?: string;
  country?: string;
  description?: string;
  assignedEmployeeId?: string;
  assignedEmployeeName?: string;
  createdAt: string;
  updatedAt?: string;
  contacts: AccountContactItemDto[];
  deals: AccountDealItemDto[];
  contracts: AccountContractItemDto[];
  projects: AccountProjectItemDto[];
  activities: TimelineEventDto[];
  timeline: TimelineEventDto[];
}

export interface CreateAccountRequest {
  name: string;
  industry?: string;
  classification?: string;
  status?: string;
  taxId?: string;
  phone?: string;
  email?: string;
  website?: string;
  address?: string;
  city?: string;
  country?: string;
  description?: string;
  assignedEmployeeId?: string;
}

export interface UpdateAccountRequest {
  name: string;
  industry?: string;
  classification?: string;
  status: string;
  taxId?: string;
  phone?: string;
  email?: string;
  website?: string;
  address?: string;
  city?: string;
  country?: string;
  description?: string;
  assignedEmployeeId?: string;
}

// ----------------- Contact Types -----------------
export interface ContactDto {
  id: string;
  accountId: string;
  accountName: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email?: string;
  phone?: string;
  position?: string;
  department?: string;
  isPrimary: boolean;
  status: string;
  notes?: string;
  dealsCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateContactRequest {
  accountId: string;
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  position?: string;
  department?: string;
  isPrimary?: boolean;
  notes?: string;
}

export interface UpdateContactRequest {
  accountId: string;
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  position?: string;
  department?: string;
  isPrimary: boolean;
  status: string;
  notes?: string;
}

// ----------------- Deal & Pipeline Types -----------------
export interface DealDto {
  id: string;
  code: string;
  title: string;
  accountId: string;
  accountName: string;
  contactId?: string;
  contactName?: string;
  stageId: string;
  stageName: string;
  stageColor?: string;
  probability: number;
  probabilityOverride?: number;
  effectiveProbability: number;
  amount: number;
  expectedRevenue: number;
  expectedClosingDate?: string;
  status: string;
  source?: string;
  description?: string;
  assignedEmployeeId?: string;
  assignedEmployeeName?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateDealRequest {
  title: string;
  accountId: string;
  contactId?: string;
  stageId: string;
  amount: number;
  probabilityOverride?: number;
  expectedClosingDate?: string;
  source?: string;
  description?: string;
  assignedEmployeeId?: string;
}

export interface UpdateDealRequest {
  title: string;
  contactId?: string;
  stageId: string;
  amount: number;
  probabilityOverride?: number;
  expectedClosingDate?: string;
  status: string;
  source?: string;
  description?: string;
  assignedEmployeeId?: string;
}

export interface UpdateDealStageRequest {
  stageId: string;
  notes?: string;
}

export interface KanbanDealCardDto {
  id: string;
  code: string;
  title: string;
  accountId: string;
  accountName: string;
  stageId: string;
  contactName?: string;
  amount: number;
  probability: number;
  expectedRevenue: number;
  expectedClosingDate?: string;
  status: string;
  assignedEmployeeName?: string;
  updatedAt?: string;
}

export interface KanbanStageColumnDto {
  stageId: string;
  stageName: string;
  orderIndex: number;
  probability: number;
  colorCode?: string;
  isWonStage: boolean;
  isLostStage: boolean;
  dealsCount: number;
  totalAmount: number;
  totalExpectedRevenue: number;
  deals: KanbanDealCardDto[];
}

export interface KanbanBoardDto {
  totalDealsCount: number;
  totalPipelineValue: number;
  totalExpectedRevenue: number;
  columns: KanbanStageColumnDto[];
}

export interface DealStageHistoryDto {
  id: string;
  fromStageName?: string;
  toStageName: string;
  changedByName: string;
  changedAt: string;
  durationDays: number;
  notes?: string;
}

// ----------------- Task Types -----------------
export interface TaskDto {
  id: string;
  title: string;
  description?: string;
  priority: string;
  status: string;
  startDate?: string;
  dueDate?: string;
  completedAt?: string;
  assignedEmployeeId?: string;
  assignedEmployeeName?: string;
  leadId?: string;
  leadName?: string;
  accountId?: string;
  accountName?: string;
  contactId?: string;
  contactName?: string;
  dealId?: string;
  dealTitle?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  priority: string;
  startDate?: string;
  dueDate?: string;
  assignedEmployeeId?: string;
  leadId?: string;
  accountId?: string;
  contactId?: string;
  dealId?: string;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  priority: string;
  status: string;
  startDate?: string;
  dueDate?: string;
  assignedEmployeeId?: string;
  leadId?: string;
  accountId?: string;
  contactId?: string;
  dealId?: string;
}

// ----------------- Appointment Types -----------------
export interface AppointmentDto {
  id: string;
  title: string;
  description?: string;
  startTime: string;
  endTime: string;
  location?: string;
  meetingLink?: string;
  status: string;
  employeeId: string;
  employeeName: string;
  dealId?: string;
  dealTitle?: string;
  accountId?: string;
  accountName?: string;
  contactId?: string;
  contactFullName?: string;
  leadId?: string;
  leadFullName?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateAppointmentRequest {
  title: string;
  description?: string;
  startTime: string;
  endTime: string;
  location?: string;
  meetingLink?: string;
  employeeId: string;
  dealId?: string;
  accountId?: string;
  contactId?: string;
  leadId?: string;
}

export interface UpdateAppointmentRequest {
  title: string;
  description?: string;
  startTime: string;
  endTime: string;
  location?: string;
  meetingLink?: string;
  status: string;
  employeeId: string;
  dealId?: string;
  accountId?: string;
  contactId?: string;
  leadId?: string;
}

// ----------------- Timeline Types -----------------
export interface TimelineEventDto {
  id: string;
  type: string;
  title: string;
  description?: string;
  timestamp: string;
  performedByName: string;
  entityType?: string;
  entityId?: string;
  metadata?: Record<string, unknown>;
}
