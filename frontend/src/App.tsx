import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AppShell } from '@/components/layout/AppShell';
import { DashboardPage } from '@/features/dashboard/DashboardPage';
import { FeaturePlaceholder } from '@/components/common/FeaturePlaceholder';
import { LoginPage } from '@/features/auth/LoginPage';
import { ForbiddenPage } from '@/features/auth/ForbiddenPage';
import { ProtectedRoute } from '@/routes/ProtectedRoute';

// Phase 3 Core CRM Pages
import { LeadsPage } from '@/features/leads/LeadsPage';
import { LeadDetailPage } from '@/features/leads/LeadDetailPage';
import { AccountsPage } from '@/features/accounts/AccountsPage';
import { AccountDetailPage } from '@/features/accounts/AccountDetailPage';
import { ContactsPage } from '@/features/contacts/ContactsPage';
import { DealsPage } from '@/features/deals/DealsPage';
import { DealDetailPage } from '@/features/deals/DealDetailPage';
import { PipelineKanbanPage } from '@/features/deals/PipelineKanbanPage';
import { TasksPage } from '@/features/tasks/TasksPage';
import { AppointmentsPage } from '@/features/appointments/AppointmentsPage';
import { ActivitiesPage } from '@/features/activities/ActivitiesPage';

// Phase 4 Quotation & Contract Management Pages
import { ServicesPage } from '@/features/services/ServicesPage';
import { PriceListsPage } from '@/features/services/PriceListsPage';
import { QuotationsPage } from '@/features/quotations/QuotationsPage';
import { QuotationDetailPage } from '@/features/quotations/QuotationDetailPage';
import { ContractsPage } from '@/features/contracts/ContractsPage';
import { ContractDetailPage } from '@/features/contracts/ContractDetailPage';
import { ContractTemplatesPage } from '@/features/contracts/ContractTemplatesPage';

// Phase 5 Finance & Project Management Pages
import { FinanceDashboardPage } from '@/features/finance/FinanceDashboardPage';
import { ContractFinancePage } from '@/features/finance/ContractFinancePage';
import { PaymentsPage } from '@/features/finance/PaymentsPage';
import { InvoicesPage } from '@/features/finance/InvoicesPage';
import { ReceivablesPage } from '@/features/finance/ReceivablesPage';
import { CustomerReceivableDetailPage } from '@/features/finance/CustomerReceivableDetailPage';
import { RevenueAnalyticsPage } from '@/features/finance/RevenueAnalyticsPage';
import { ReconciliationPage } from '@/features/finance/ReconciliationPage';

// Phase 6 Sales Performance, KPI, Commission & Payroll Pages
import { SalesPerformanceDashboardPage } from '@/features/performance/SalesPerformanceDashboardPage';
import { KpisPage } from '@/features/performance/KpisPage';
import { KpiDetailPage } from '@/features/performance/KpiDetailPage';
import { KpiAssignmentsPage } from '@/features/performance/KpiAssignmentsPage';
import { EmployeePerformancePage } from '@/features/performance/EmployeePerformancePage';
import { TeamPerformancePage } from '@/features/performance/TeamPerformancePage';
import { CommissionsPage } from '@/features/performance/CommissionsPage';
import { CommissionPoliciesPage } from '@/features/performance/CommissionPoliciesPage';
import { CommissionPolicyDetailPage } from '@/features/performance/CommissionPolicyDetailPage';
import { CommissionAdjustmentsPage } from '@/features/performance/CommissionAdjustmentsPage';
import { BonusesPage } from '@/features/performance/BonusesPage';
import { BonusPoliciesPage } from '@/features/performance/BonusPoliciesPage';
import { PayrollDashboardPage } from '@/features/performance/PayrollDashboardPage';
import { PayrollPeriodDetailPage } from '@/features/performance/PayrollPeriodDetailPage';
import { EmployeePayslipPage } from '@/features/performance/EmployeePayslipPage';
import { PayrollApprovalsPage } from '@/features/performance/PayrollApprovalsPage';

// Phase 7 Workflow Automation, AI Intelligence, Role Dashboards & Reports
import { WorkflowsPage } from '@/features/automation/WorkflowsPage';
import { WorkflowBuilderPage } from '@/features/automation/WorkflowBuilderPage';
import { WorkflowExecutionsPage } from '@/features/automation/WorkflowExecutionsPage';
import { LeadDistributionDashboardPage } from '@/features/automation/LeadDistributionDashboardPage';
import { AlertsCenterPage } from '@/features/automation/AlertsCenterPage';

import { AiAssistantPage } from '@/features/ai/AiAssistantPage';
import { AiQueryPage } from '@/features/ai/AiQueryPage';
import { AiSettingsPage } from '@/features/ai/AiSettingsPage';
import { AiAnalyticsOverviewPage } from '@/features/ai/AiAnalyticsOverviewPage';
import { RevenueForecastPage } from '@/features/ai/RevenueForecastPage';
import { AnomalyDetectionPage } from '@/features/ai/AnomalyDetectionPage';
import { BusinessRecommendationsPage } from '@/features/ai/BusinessRecommendationsPage';

import { ReportsDashboardPage } from '@/features/reports/ReportsDashboardPage';
import { SalesRoleDashboardPage } from '@/features/dashboard/SalesRoleDashboardPage';
import { ManagerRoleDashboardPage } from '@/features/dashboard/ManagerRoleDashboardPage';
import { ExecutiveRoleDashboardPage } from '@/features/dashboard/ExecutiveRoleDashboardPage';

// Phase 8 System Administration & Enterprise Management Pages
import { UsersAdminPage } from '@/features/admin/UsersAdminPage';
import { OrganizationAdminPage } from '@/features/admin/OrganizationAdminPage';
import { RolesAdminPage } from '@/features/admin/RolesAdminPage';
import { PermissionMatrixPage } from '@/features/admin/PermissionMatrixPage';
import { AuditLogsPage } from '@/features/admin/AuditLogsPage';
import { DataManagementPage } from '@/features/admin/DataManagementPage';
import { IntegrationsPage } from '@/features/admin/IntegrationsPage';
import { BackupsPage } from '@/features/admin/BackupsPage';
import { SystemSettingsPage } from '@/features/admin/SystemSettingsPage';
import { NotificationCenterPage } from '@/features/notifications/NotificationCenterPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60,
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          {/* Public Auth Routes */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/forbidden" element={<ForbiddenPage />} />

          {/* Protected Organization Routes */}
          <Route element={<ProtectedRoute />}>
            <Route element={<AppShell />}>
              <Route path="/" element={<DashboardPage />} />

              {/* Module A – Lead Management */}
              <Route path="/leads" element={<LeadsPage />} />
              <Route path="/leads/:id" element={<LeadDetailPage />} />

              {/* Module B – Account Management */}
              <Route path="/accounts" element={<AccountsPage />} />
              <Route path="/accounts/:id" element={<AccountDetailPage />} />

              {/* Module C – Contact Management */}
              <Route path="/contacts" element={<ContactsPage />} />

              {/* Module D – Deal & Pipeline Kanban Management */}
              <Route path="/pipeline" element={<PipelineKanbanPage />} />
              <Route path="/deals" element={<DealsPage />} />
              <Route path="/deals/:id" element={<DealDetailPage />} />

              {/* Module E – Tasks, Activities & Appointments */}
              <Route path="/tasks" element={<TasksPage />} />
              <Route path="/appointments" element={<AppointmentsPage />} />
              <Route path="/activities" element={<ActivitiesPage />} />

              {/* Phase 4 – Quotation & Contract Management */}
              <Route path="/services" element={<ServicesPage />} />
              <Route path="/price-lists" element={<PriceListsPage />} />
              <Route path="/quotations" element={<QuotationsPage />} />
              <Route path="/quotations/:id" element={<QuotationDetailPage />} />
              <Route path="/contracts" element={<ContractsPage />} />
              <Route path="/contracts/:id" element={<ContractDetailPage />} />
              <Route path="/contract-templates" element={<ContractTemplatesPage />} />

              {/* Future Phases Placeholders */}
              <Route
                path="/projects"
                element={
                  <FeaturePlaceholder
                    title="Projects & Delivery"
                    description="Milestone tracking linked to signed contracts, service delivery phases, and progress monitoring."
                    phase="Phase 5 (Projects)"
                  />
                }
              />

              {/* Phase 5 Finance & Project Management */}
              <Route
                path="/finance"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Accountant', 'Manager']}>
                    <FinanceDashboardPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/finance/contracts"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Accountant', 'Manager']}>
                    <ContractFinancePage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/finance/payments"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Accountant', 'Manager']}>
                    <PaymentsPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/finance/invoices"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Accountant', 'Manager']}>
                    <InvoicesPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/finance/receivables"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Accountant', 'Manager']}>
                    <ReceivablesPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/finance/receivables/:id"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Accountant', 'Manager']}>
                    <CustomerReceivableDetailPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/finance/revenue"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Accountant', 'Manager']}>
                    <RevenueAnalyticsPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/finance/reconciliation"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Accountant']}>
                    <ReconciliationPage />
                  </ProtectedRoute>
                }
              />
              {/* Phase 6 – Sales Performance, KPI, Commission & Payroll */}
              <Route path="/performance/dashboard" element={<SalesPerformanceDashboardPage />} />
              <Route path="/performance/kpis" element={<KpisPage />} />
              <Route path="/performance/kpis/:id" element={<KpiDetailPage />} />
              <Route path="/performance/assignments" element={<KpiAssignmentsPage />} />
              <Route path="/performance/rankings" element={<EmployeePerformancePage />} />
              <Route path="/performance/employees" element={<EmployeePerformancePage />} />
              <Route path="/performance/teams" element={<TeamPerformancePage />} />
              <Route path="/performance/departments" element={<TeamPerformancePage />} />

              <Route path="/commissions" element={<CommissionsPage />} />
              <Route path="/commissions/calculations" element={<CommissionsPage />} />
              <Route path="/commissions/policies" element={<CommissionPoliciesPage />} />
              <Route path="/commissions/policies/:id" element={<CommissionPolicyDetailPage />} />
              <Route path="/commissions/adjustments" element={<CommissionAdjustmentsPage />} />

              <Route path="/bonuses" element={<BonusesPage />} />
              <Route path="/bonuses/adjustments" element={<BonusesPage />} />
              <Route path="/bonuses/policies" element={<BonusPoliciesPage />} />

              <Route
                path="/payroll"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager', 'Accountant']}>
                    <PayrollDashboardPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/payroll/periods"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager', 'Accountant']}>
                    <PayrollDashboardPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/payroll/periods/:id"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager', 'Accountant']}>
                    <PayrollPeriodDetailPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/payroll/employees/:id"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager', 'Accountant', 'Sales']}>
                    <EmployeePayslipPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/payroll/approvals"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager']}>
                    <PayrollApprovalsPage />
                  </ProtectedRoute>
                }
              />
              {/* Role-Based Analytical Dashboards */}
              <Route path="/dashboard/sales" element={<SalesRoleDashboardPage />} />
              <Route
                path="/dashboard/manager"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager']}>
                    <ManagerRoleDashboardPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/dashboard/executive"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD']}>
                    <ExecutiveRoleDashboardPage />
                  </ProtectedRoute>
                }
              />

              {/* Phase 7 – Workflow Automation */}
              <Route
                path="/workflows"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager']}>
                    <WorkflowsPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/automation/workflows"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager']}>
                    <WorkflowsPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/automation/workflows/create"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager']}>
                    <WorkflowBuilderPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/automation/workflows/:id"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager']}>
                    <WorkflowBuilderPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/automation/executions"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager']}>
                    <WorkflowExecutionsPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/automation/lead-distribution"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager']}>
                    <LeadDistributionDashboardPage />
                  </ProtectedRoute>
                }
              />
              <Route path="/automation/alerts" element={<AlertsCenterPage />} />

              {/* Phase 7 – AI Intelligence & Assistant */}
              <Route path="/ai-assistant" element={<AiAssistantPage />} />
              <Route path="/ai-assistant/query" element={<AiQueryPage />} />
              <Route
                path="/ai-assistant/settings"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD']}>
                    <AiSettingsPage />
                  </ProtectedRoute>
                }
              />
              <Route path="/ai-analytics/overview" element={<AiAnalyticsOverviewPage />} />
              <Route path="/ai-analytics/forecast" element={<RevenueForecastPage />} />
              <Route path="/ai-analytics/anomalies" element={<AnomalyDetectionPage />} />
              <Route path="/ai-analytics/recommendations" element={<BusinessRecommendationsPage />} />

              {/* Phase 7 – Management Reports */}
              <Route
                path="/reports"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager', 'Accountant']}>
                    <ReportsDashboardPage />
                  </ProtectedRoute>
                }
              />

              {/* Phase 8 – Notification Center */}
              <Route path="/notifications" element={<NotificationCenterPage />} />

              {/* Phase 8 – System Administration & Enterprise Governance */}
              <Route
                path="/admin"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD']}>
                    <Navigate to="/admin/users" replace />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/users"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD']}>
                    <UsersAdminPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/organization"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD']}>
                    <OrganizationAdminPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/roles"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD']}>
                    <RolesAdminPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/permission-matrix"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD']}>
                    <PermissionMatrixPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/audit-logs"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD']}>
                    <AuditLogsPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/data-management"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD']}>
                    <DataManagementPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/integrations"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD']}>
                    <IntegrationsPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/backups"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD']}>
                    <BackupsPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/settings"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD']}>
                    <SystemSettingsPage />
                  </ProtectedRoute>
                }
              />
            </Route>
          </Route>

          {/* Catch-all redirect */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
