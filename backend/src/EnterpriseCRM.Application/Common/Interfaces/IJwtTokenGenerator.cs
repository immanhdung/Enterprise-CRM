using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;

namespace EnterpriseCRM.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions, Guid? departmentId = null, Guid? teamId = null, DataScope dataScope = DataScope.Own);
    string GenerateRefreshToken();
}
