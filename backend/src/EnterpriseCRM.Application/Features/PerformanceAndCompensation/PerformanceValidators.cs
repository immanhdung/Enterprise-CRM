using FluentValidation;

namespace EnterpriseCRM.Application.Features.PerformanceAndCompensation;

public class CreateKpiDefinitionRequestValidator : AbstractValidator<CreateKpiDefinitionRequest>
{
    public CreateKpiDefinitionRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã KPI không được để trống.")
            .MaximumLength(50).WithMessage("Mã KPI tối đa 50 ký tự.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên KPI không được để trống.")
            .MaximumLength(150).WithMessage("Tên KPI tối đa 150 ký tự.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Nhóm KPI không được để trống.")
            .Must(c => c is "Sales" or "Activity" or "Conversion")
            .WithMessage("Nhóm KPI phải là 'Sales', 'Activity' hoặc 'Conversion'.");

        RuleFor(x => x.MetricType)
            .NotEmpty().WithMessage("Loại chỉ số không được để trống.");

        RuleFor(x => x.DefaultTarget)
            .GreaterThanOrEqualTo(0).WithMessage("Chỉ tiêu mặc định không được âm.");
    }
}

public class CreateKpiAssignmentRequestValidator : AbstractValidator<CreateKpiAssignmentRequest>
{
    public CreateKpiAssignmentRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Nhân viên được giao KPI không được để trống.");

        RuleFor(x => x.PeriodMonth)
            .InclusiveBetween(1, 12).WithMessage("Tháng đánh giá phải từ 1 đến 12.");

        RuleFor(x => x.PeriodYear)
            .InclusiveBetween(2020, 2050).WithMessage("Năm đánh giá không hợp lệ.");

        RuleFor(x => x.TargetRevenue)
            .GreaterThanOrEqualTo(0).WithMessage("Chỉ tiêu doanh thu không được âm.");

        RuleFor(x => x.WeightPercent)
            .InclusiveBetween(1, 100).WithMessage("Trọng số KPI phải từ 1% đến 100%.");
    }
}

public class CreateCommissionPolicyRequestValidator : AbstractValidator<CreateCommissionPolicyRequest>
{
    public CreateCommissionPolicyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên chính sách hoa hồng không được để trống.")
            .MaximumLength(150).WithMessage("Tên chính sách tối đa 150 ký tự.");

        RuleFor(x => x.CalculationBasis)
            .NotEmpty().WithMessage("Cơ sở tính hoa hồng không được để trống.")
            .Must(b => b is "ContractValue" or "CollectedCash" or "ServiceBased" or "CustomerTypeBased" or "TargetTierBased")
            .WithMessage("Cơ sở tính hoa hồng không hợp lệ.");

        RuleFor(x => x.RatePercent)
            .InclusiveBetween(0, 100).WithMessage("Tỷ lệ hoa hồng phải từ 0% đến 100%.");

        RuleFor(x => x.MinThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Hạn mức tối thiểu không được âm.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("Ngày hiệu lực không được để trống.");

        When(x => x.EffectiveTo.HasValue, () =>
        {
            RuleFor(x => x.EffectiveTo!.Value)
                .GreaterThanOrEqualTo(x => x.EffectiveFrom)
                .WithMessage("Ngày kết thúc hiệu lực phải sau hoặc bằng ngày bắt đầu.");
        });
    }
}

public class CommissionAdjustmentRequestValidator : AbstractValidator<CommissionAdjustmentRequest>
{
    public CommissionAdjustmentRequestValidator()
    {
        RuleFor(x => x.CommissionId)
            .NotEmpty().WithMessage("Mã hoa hồng không được để trống.");

        RuleFor(x => x.AdjustmentAmount)
            .NotEqual(0).WithMessage("Số tiền điều chỉnh không được bằng 0.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Bắt buộc nhập lý do điều chỉnh hoa hồng.")
            .MinimumLength(5).WithMessage("Lý do điều chỉnh tối thiểu 5 ký tự.");
    }
}

public class CreateBonusPolicyRequestValidator : AbstractValidator<CreateBonusPolicyRequest>
{
    public CreateBonusPolicyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên chính sách thưởng không được để trống.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Loại thưởng không được để trống.");

        RuleFor(x => x.BonusAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Mức tiền thưởng không được âm.");
    }
}

public class CreateBonusRecordRequestValidator : AbstractValidator<CreateBonusRecordRequest>
{
    public CreateBonusRecordRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Nhân viên được nhận thưởng không được để trống.");

        RuleFor(x => x.PeriodMonth)
            .InclusiveBetween(1, 12).WithMessage("Tháng thưởng phải từ 1 đến 12.");

        RuleFor(x => x.PeriodYear)
            .InclusiveBetween(2020, 2050).WithMessage("Năm thưởng không hợp lệ.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Số tiền thưởng phải lớn hơn 0.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Bắt buộc nhập lý do khen thưởng.");
    }
}

public class CreatePayrollPeriodRequestValidator : AbstractValidator<CreatePayrollPeriodRequest>
{
    public CreatePayrollPeriodRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên kỳ lương không được để trống.");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Tháng phải từ 1 đến 12.");

        RuleFor(x => x.Year)
            .InclusiveBetween(2020, 2050).WithMessage("Năm không hợp lệ.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("Ngày kết thúc kỳ lương phải sau ngày bắt đầu.");
    }
}

public class AddPayrollAdjustmentRequestValidator : AbstractValidator<AddPayrollAdjustmentRequest>
{
    public AddPayrollAdjustmentRequestValidator()
    {
        RuleFor(x => x.PayrollRecordId)
            .NotEmpty().WithMessage("Mã bảng lương nhân viên không được để trống.");

        RuleFor(x => x.Amount)
            .NotEqual(0).WithMessage("Số tiền điều chỉnh không được bằng 0.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Bắt buộc nhập lý do điều chỉnh lương.")
            .MinimumLength(5).WithMessage("Lý do điều chỉnh tối thiểu 5 ký tự.");
    }
}
