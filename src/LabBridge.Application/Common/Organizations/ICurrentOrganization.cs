using System;
using System.Collections.Generic;
using System.Text;

namespace LabBridge.Application.Common.Organizations;

public interface ICurrentOrganization
{
    Guid? OrganizationId { get; }

    bool IsOrganizationContextAvailable { get; }
}