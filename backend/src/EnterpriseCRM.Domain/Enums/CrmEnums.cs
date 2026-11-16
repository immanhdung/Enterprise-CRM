namespace EnterpriseCRM.Domain.Enums;

public enum UserRoleType
{
    Admin = 1,
    CEO_BOD = 2,
    Manager = 3,
    Sales = 4,
    Accountant = 5
}

public enum DataScope
{
    All = 1,
    Department = 2,
    Team = 3,
    Own = 4
}

public enum LeadStatus
{
    New = 1,
    Contacted = 2,
    Qualified = 3,
    Unqualified = 4,
    Converted = 5,
    Stagnant = 6
}

public enum DealStatus
{
    Open = 1,
    Won = 2,
    Lost = 3,
    Stagnant = 4
}

public enum ContractStatus
{
    Draft = 1,
    InReview = 2,
    Approved = 3,
    Signed = 4,
    Active = 5,
    Expired = 6,
    Terminated = 7,
    Renewed = 8
}

public enum InvoiceStatus
{
    Draft = 1,
    Published = 2,
    SentToCustomer = 3,
    Cancelled = 4
}

public enum PaymentMethod
{
    BankTransfer = 1,
    CreditCard = 2,
    Cash = 3,
    Cheque = 4
}
