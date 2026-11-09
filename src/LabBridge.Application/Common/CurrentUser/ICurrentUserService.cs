using System;
using System.Collections.Generic;
using System.Text;

namespace LabBridge.Application.Common;


public interface ICurrentUserService
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }
    string? Email { get; }
    string? Name { get; }
    IReadOnlyCollection<string> Roles { get; }
}