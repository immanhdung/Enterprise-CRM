namespace EnterpriseCRM.Domain.Constants;

public static class Permissions
{
    // Leads Module
    public const string LeadsView = "leads:view";
    public const string LeadsCreate = "leads:create";
    public const string LeadsEdit = "leads:edit";
    public const string LeadsDelete = "leads:delete";
    public const string LeadsAssign = "leads:assign";
    public const string LeadsExport = "leads:export";

    // Customers / Accounts Module
    public const string CustomersView = "customers:view";
    public const string CustomersCreate = "customers:create";
    public const string CustomersEdit = "customers:edit";
    public const string CustomersDelete = "customers:delete";

    // Deals / Opportunities Module
    public const string DealsView = "deals:view";
    public const string DealsCreate = "deals:create";
    public const string DealsEdit = "deals:edit";
    public const string DealsDelete = "deals:delete";
    public const string DealsStageChange = "deals:stage_change";

    // Services & Price Lists Module
    public const string ServicesView = "services:view";
    public const string ServicesManage = "services:manage";

    // Quotations Module
    public const string QuotationsView = "quotations:view";
    public const string QuotationsCreate = "quotations:create";
    public const string QuotationsEdit = "quotations:edit";
    public const string QuotationsDelete = "quotations:delete";
    public const string QuotationsApprove = "quotations:approve";
    public const string QuotationsSend = "quotations:send";

    // Contracts Module
    public const string ContractsView = "contracts:view";
    public const string ContractsCreate = "contracts:create";
    public const string ContractsEdit = "contracts:edit";
    public const string ContractsDelete = "contracts:delete";
    public const string ContractsApprove = "contracts:approve";
    public const string ContractsSign = "contracts:sign";
    public const string ContractTemplatesView = "contracts:templates_view";
    public const string ContractTemplatesManage = "contracts:templates_manage";

    // Projects Module
    public const string ProjectsView = "projects:view";
    public const string ProjectsCreate = "projects:create";
    public const string ProjectsEdit = "projects:edit";
    public const string ProjectsDelete = "projects:delete";

    // Invoices & Payments Module
    public const string InvoicesView = "invoices:view";
    public const string InvoicesCreate = "invoices:create";
    public const string InvoicesEdit = "invoices:edit";
    public const string InvoicesApprove = "invoices:approve";
    public const string InvoicesDelete = "invoices:delete";

    public const string PaymentsView = "payments:view";
    public const string PaymentsCreate = "payments:create";
    public const string PaymentsVerify = "payments:verify";
    public const string PaymentsReverse = "payments:reverse";

    public const string ReceivablesView = "receivables:view";
    public const string ReceivablesManage = "receivables:manage";

    public const string ReconciliationView = "reconciliation:view";
    public const string ReconciliationManage = "reconciliation:manage";

    // Finance, Commissions & Payroll Module
    public const string FinanceView = "finance:view";
    public const string FinanceExport = "finance:export";
    public const string CommissionsView = "commissions:view";
    public const string CommissionsViewOwn = "commissions:view_own";
    public const string CommissionsManage = "commissions:manage";
    public const string CommissionsCalculate = "commissions:calculate";
    public const string CommissionsApprove = "commissions:approve";
    public const string CommissionsAdjust = "commissions:adjust";

    public const string BonusesView = "bonuses:view";
    public const string BonusesManage = "bonuses:manage";
    public const string BonusesApprove = "bonuses:approve";

    public const string PayrollView = "payroll:view";
    public const string PayrollViewOwn = "payroll:view_own";
    public const string PayrollProcess = "payroll:process";
    public const string PayrollApprove = "payroll:approve";
    public const string PayrollFinalize = "payroll:finalize";

    public const string KpisView = "kpis:view";
    public const string KpisManage = "kpis:manage";
    public const string KpisEvaluate = "kpis:evaluate";
    public const string KpisApprove = "kpis:approve";

    public const string PerformanceViewOwn = "performance:view_own";
    public const string PerformanceViewTeam = "performance:view_team";
    public const string PerformanceViewAll = "performance:view_all";

    // Organization & Employees Module
    public const string EmployeesView = "employees:view";
    public const string EmployeesCreate = "employees:create";
    public const string EmployeesEdit = "employees:edit";
    public const string EmployeesDelete = "employees:delete";

    public const string DepartmentsView = "departments:view";
    public const string DepartmentsManage = "departments:manage";

    public const string TeamsView = "teams:view";
    public const string TeamsManage = "teams:manage";

    // Administration & Security Module
    public const string UsersView = "users:view";
    public const string UsersCreate = "users:create";
    public const string UsersEdit = "users:edit";
    public const string UsersDelete = "users:delete";
    public const string UsersManageRoles = "users:manage_roles";

    public const string RolesView = "roles:view";
    public const string RolesManage = "roles:manage";

    public const string SettingsView = "settings:view";
    public const string SettingsManage = "settings:manage";
    public const string AuditView = "audit:view";

    // Workflow Automation & AI Intelligence (Phase 7)
    public const string WorkflowsView = "workflows:view";
    public const string WorkflowsManage = "workflows:manage";
    public const string WorkflowsExecute = "workflows:execute";

    public const string AlertsView = "alerts:view";
    public const string AlertsManage = "alerts:manage";

    public const string AiAssistantUse = "ai:assistant_use";
    public const string AiAnalyticsView = "ai:analytics_view";
    public const string AiQueryExecute = "ai:query_execute";

    public const string ReportsView = "reports:view";
    public const string ReportsGenerate = "reports:generate";
    public const string ReportsSchedule = "reports:schedule";

    public const string DashboardSalesView = "dashboard:sales_view";
    public const string DashboardManagerView = "dashboard:manager_view";
    public const string DashboardExecutiveView = "dashboard:executive_view";

    public static readonly IReadOnlyList<(string Code, string Module, string Description)> AllPermissions = new[]
    {
        (LeadsView, "Leads", "View leads list and details"),
        (LeadsCreate, "Leads", "Create new leads"),
        (LeadsEdit, "Leads", "Edit existing leads"),
        (LeadsDelete, "Leads", "Delete leads"),
        (LeadsAssign, "Leads", "Assign leads to team members"),
        (LeadsExport, "Leads", "Export leads to Excel/CSV"),

        (CustomersView, "Customers", "View accounts and contacts"),
        (CustomersCreate, "Customers", "Create new accounts and contacts"),
        (CustomersEdit, "Customers", "Edit accounts and contacts"),
        (CustomersDelete, "Customers", "Delete accounts and contacts"),

        (DealsView, "Deals", "View deals and sales pipeline"),
        (DealsCreate, "Deals", "Create new deals"),
        (DealsEdit, "Deals", "Edit existing deals"),
        (DealsDelete, "Deals", "Delete deals"),
        (DealsStageChange, "Deals", "Move deals across pipeline stages"),

        (ServicesView, "Services", "View service catalog and price lists"),
        (ServicesManage, "Services", "Manage services, categories and pricing"),

        (QuotationsView, "Quotations", "View quotations"),
        (QuotationsCreate, "Quotations", "Create new quotations"),
        (QuotationsEdit, "Quotations", "Edit quotations"),
        (QuotationsDelete, "Quotations", "Delete quotations"),
        (QuotationsApprove, "Quotations", "Approve quotations"),
        (QuotationsSend, "Quotations", "Send quotations to clients"),

        (ContractsView, "Contracts", "View contracts"),
        (ContractsCreate, "Contracts", "Create new contracts"),
        (ContractsEdit, "Contracts", "Edit contracts"),
        (ContractsDelete, "Contracts", "Delete contracts"),
        (ContractsApprove, "Contracts", "Approve contracts"),
        (ContractsSign, "Contracts", "Initiate and track e-signatures"),
        (ContractTemplatesView, "Contracts", "View contract templates"),
        (ContractTemplatesManage, "Contracts", "Manage contract templates"),

        (ProjectsView, "Projects", "View projects"),
        (ProjectsCreate, "Projects", "Create new projects"),
        (ProjectsEdit, "Projects", "Edit projects"),
        (ProjectsDelete, "Projects", "Delete projects"),

        (InvoicesView, "Invoices", "View invoices"),
        (InvoicesCreate, "Invoices", "Create invoice requests"),
        (InvoicesEdit, "Invoices", "Edit invoices"),
        (InvoicesApprove, "Invoices", "Approve invoices for issuing"),
        (InvoicesDelete, "Invoices", "Delete or cancel invoices"),

        (PaymentsView, "Payments", "View payment records"),
        (PaymentsCreate, "Payments", "Record incoming payments"),
        (PaymentsVerify, "Payments", "Verify and reconcile payments"),
        (PaymentsReverse, "Payments", "Cancel or reverse verified payments"),

        (ReceivablesView, "Receivables", "View accounts receivable and aging reports"),
        (ReceivablesManage, "Receivables", "Manage collection logs and debt follow-ups"),

        (ReconciliationView, "Reconciliation", "View financial reconciliation reports"),
        (ReconciliationManage, "Reconciliation", "Execute reconciliation and resolve discrepancies"),

        (FinanceView, "Finance", "View financial reports and dashboard"),
        (FinanceExport, "Finance", "Export financial statements"),
        (CommissionsView, "Commissions", "View commission calculations"),
        (CommissionsViewOwn, "Commissions", "View own commission records"),
        (CommissionsManage, "Commissions", "Configure commission policies"),
        (CommissionsCalculate, "Commissions", "Run commission calculations"),
        (CommissionsApprove, "Commissions", "Approve calculated commissions"),
        (CommissionsAdjust, "Commissions", "Manually adjust commission amounts"),

        (BonusesView, "Bonuses", "View bonus records and policies"),
        (BonusesManage, "Bonuses", "Configure bonus policies and grant bonuses"),
        (BonusesApprove, "Bonuses", "Approve bonuses"),

        (PayrollView, "Payroll", "View payroll periods and records"),
        (PayrollViewOwn, "Payroll", "View own payroll records and payslips"),
        (PayrollProcess, "Payroll", "Process and calculate payroll runs"),
        (PayrollApprove, "Payroll", "Approve payroll records"),
        (PayrollFinalize, "Payroll", "Finalize and lock payroll runs"),

        (KpisView, "KPIs", "View KPI definitions and assignments"),
        (KpisManage, "KPIs", "Configure and assign KPI targets"),
        (KpisEvaluate, "KPIs", "Evaluate actual KPI performance"),
        (KpisApprove, "KPIs", "Approve KPI evaluations"),

        (PerformanceViewOwn, "Performance", "View own sales performance"),
        (PerformanceViewTeam, "Performance", "View team sales performance"),
        (PerformanceViewAll, "Performance", "View company-wide sales performance"),

        (EmployeesView, "Employees", "View employee directory"),
        (EmployeesCreate, "Employees", "Create new employees"),
        (EmployeesEdit, "Employees", "Edit employee details"),
        (EmployeesDelete, "Employees", "Deactivate/delete employees"),

        (DepartmentsView, "Departments", "View departments"),
        (DepartmentsManage, "Departments", "Manage departments and structure"),

        (TeamsView, "Teams", "View sales and operational teams"),
        (TeamsManage, "Teams", "Manage teams and assignments"),

        (UsersView, "Users", "View user accounts"),
        (UsersCreate, "Users", "Create user accounts"),
        (UsersEdit, "Users", "Edit user accounts"),
        (UsersDelete, "Users", "Deactivate user accounts"),
        (UsersManageRoles, "Users", "Assign roles to users"),

        (RolesView, "Roles", "View system roles and permissions"),
        (RolesManage, "Roles", "Create and modify roles and permissions"),

        (SettingsView, "Settings", "View system settings"),
        (SettingsManage, "Settings", "Modify company profile and configuration"),
        (AuditView, "Audit", "View system audit logs"),

        (WorkflowsView, "Automation", "View automated workflows and executions"),
        (WorkflowsManage, "Automation", "Create and configure workflow definitions"),
        (WorkflowsExecute, "Automation", "Trigger and rerun workflow automations"),

        (AlertsView, "Alerts", "View business alerts and smart reminders"),
        (AlertsManage, "Alerts", "Configure alert rules and acknowledge notifications"),

        (AiAssistantUse, "AI", "Interact with AI business co-pilot and chat"),
        (AiAnalyticsView, "AI", "View AI sales performance insights and recommendations"),
        (AiQueryExecute, "AI", "Execute natural language business queries"),

        (ReportsView, "Reports", "View management reports"),
        (ReportsGenerate, "Reports", "Generate daily, weekly and monthly reports"),
        (ReportsSchedule, "Reports", "Configure report schedules and auto-delivery"),

        (DashboardSalesView, "Dashboards", "View sales representative personal dashboard"),
        (DashboardManagerView, "Dashboards", "View team and department manager dashboard"),
        (DashboardExecutiveView, "Dashboards", "View executive CEO/BOD analytics dashboard")
    };
}

public static class SystemRoles
{
    public const string Admin = "Admin";
    public const string CeoBod = "CEO_BOD";
    public const string Manager = "Manager";
    public const string Sales = "Sales";
    public const string Accountant = "Accountant";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Admin,
        CeoBod,
        Manager,
        Sales,
        Accountant
    };
}
