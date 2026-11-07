using System;
using System.Collections.Generic;
using System.Text;
using LabBridge.Domain.Features.IdentityAccess.Enums;

namespace LabBridge.Domain.Features.IdentityAccess;


/// <summary>
/// Immutable view of the role facts that domain rules need (identifier, stable code, scope, additive flag).
/// ARCHITECTURE: the Role entity is now an ASP.NET Core Identity type that lives in Infrastructure. Domain policies
/// must not depend on it, so they work on this framework-independent snapshot instead.
/// </summary>
public sealed record RoleSnapshot(Guid Id, string Code, RoleScope Scope, bool IsAdditive);