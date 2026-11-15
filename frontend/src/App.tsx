import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AppShell } from '@/components/layout/AppShell';
import { DashboardPage } from '@/features/dashboard/DashboardPage';
import { FeaturePlaceholder } from '@/components/common/FeaturePlaceholder';
import { LoginPage } from '@/features/auth/LoginPage';
import { ForbiddenPage } from '@/features/auth/ForbiddenPage';
import { ProtectedRoute } from '@/routes/ProtectedRoute';

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
            <Route
              element={
                <AppShell />
              }
            >
              <Route path="/" element={<DashboardPage />} />
              <Route
                path="/leads"
                element={
                  <FeaturePlaceholder
                    title="Lead Management"
                    description="Omnichannel capture (Facebook, Google, TikTok Ads), automatic round-robin assignment, duplicate detection and conversion pipeline."
                    phase="Phase 3 (Core CRM)"
                  />
                }
              />
              <Route
                path="/accounts"
                element={
                  <FeaturePlaceholder
                    title="Customer Accounts"
                    description="360-degree customer profile, corporate tax registry, segmentation, and full transaction history."
                    phase="Phase 3 (Core CRM)"
                  />
                }
              />
              <Route
                path="/contacts"
                element={
                  <FeaturePlaceholder
                    title="Contact Registry"
                    description="Individual decision-makers, direct phone numbers, email logs, and account associations."
                    phase="Phase 3 (Core CRM)"
                  />
                }
              />
              <Route
                path="/deals"
                element={
                  <FeaturePlaceholder
                    title="Sales Pipeline (Kanban)"
                    description="Drag-and-drop opportunity tracking, win probabilities, stage customization, and expected revenue forecast."
                    phase="Phase 3 (Core CRM)"
                  />
                }
              />
              <Route
                path="/activities"
                element={
                  <FeaturePlaceholder
                    title="Activities, Tasks & Appointments"
                    description="Task scheduler, calendar synchronization, follow-up reminders, and overdue alerts."
                    phase="Phase 3 (Core CRM)"
                  />
                }
              />
              <Route
                path="/quotations"
                element={
                  <FeaturePlaceholder
                    title="Quotations & CPQ"
                    description="Service catalog pricing, automated calculation with VAT/discounts, approval workflows, and branded PDF generation."
                    phase="Phase 4 (Operations)"
                  />
                }
              />
              <Route
                path="/contracts"
                element={
                  <FeaturePlaceholder
                    title="Contracts & E-Signatures"
                    description="Contract lifecycle, contract appendices, expiration reminders, and electronic signature integration adapters."
                    phase="Phase 4 (Operations)"
                  />
                }
              />
              <Route
                path="/projects"
                element={
                  <FeaturePlaceholder
                    title="Projects & Delivery"
                    description="Milestone tracking linked to signed contracts, service delivery phases, and progress monitoring."
                    phase="Phase 4 (Operations)"
                  />
                }
              />

              {/* Role Restricted Routes */}
              <Route
                path="/finance"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Accountant']}>
                    <FeaturePlaceholder
                      title="Finance & Invoices"
                      description="Multi-installment payment schedules, cash collection reconciliation, debt aging reports, and E-Invoice provider adapters."
                      phase="Phase 4 (Operations)"
                    />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/payroll"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager', 'Accountant']}>
                    <FeaturePlaceholder
                      title="Commissions & Payroll"
                      description="Multi-tiered sales commission policies, KPI performance tracking, salary adjustments, and approval workflows."
                      phase="Phase 5 (Performance & Payroll)"
                    />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/workflows"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD', 'Manager']}>
                    <FeaturePlaceholder
                      title="Workflow Automation"
                      description="Hangfire background engine, stagnant lead/deal detection, overdue debt alerts, and automated reporting."
                      phase="Phase 6 (Automation & AI)"
                    />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/ai-assistant"
                element={
                  <FeaturePlaceholder
                    title="AI Business Co-pilot"
                    description="Natural language business data querying, revenue forecasting, anomaly detection, and executive summaries."
                    phase="Phase 6 (Automation & AI)"
                  />
                }
              />
              <Route
                path="/admin"
                element={
                  <ProtectedRoute requiredRoles={['Admin', 'CEO_BOD']}>
                    <FeaturePlaceholder
                      title="System Administration"
                      description="Role-based access control (RBAC), user and department hierarchy, audit logging, and integration settings."
                      phase="Phase 7 (Governance & Admin)"
                    />
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
