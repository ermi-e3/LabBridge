using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Common.Idempotency;
using LabBridge.Domain.Features.Audit.Entities;
using LabBridge.Domain.Features.DeliveryManagement.Entities;
using LabBridge.Domain.Features.IdentityAccess.Entities;
using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.MessageIngestion.Entities;
using LabBridge.Domain.Features.MessageProcessing.Entities;
using LabBridge.Domain.Features.Monitoring.Entities;
using LabBridge.Domain.Features.OrderResultManagement.Entities;
using LabBridge.Domain.Features.OrganizationManagement.Entities;
using LabBridge.Domain.Features.Reporting.Entities;
using LabBridge.Domain.Features.SubscriptionManagement.Entities;
using LabBridge.Domain.Features.TestCodeMapping.Entities;
using LabBridge.Infrastructure.Identity;
using LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// The single persistence context of LabBridge and the ONLY owner of EF Core migrations.
/// One complete model (83 entity types, one PostgreSQL database). Module boundaries are kept through domain
/// ownership, namespaces and folders, not through separate contexts.
/// Identity: users, roles, user_role_assignments, external_identities, user_claims, role_claims and user_tokens come
/// from IdentityDbContext (explicit type arguments because the user-role join is custom; key type is Guid).
/// </summary>
public class LabBridgeDbContext
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        IdentityUserClaim<Guid>,
        ApplicationUserRole,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>
    >
{
    /// <summary>Used by InboundMessageConfiguration (ReceiptSequence default). Name must not change.</summary>
    public const string InboundMessageReceiptSequence = "inbound_message_receipt_seq";

    public LabBridgeDbContext(DbContextOptions<LabBridgeDbContext> options)
        : base(options) { }

    // Inherited from IdentityDbContext (7): Users, Roles, UserRoles, UserClaims, UserLogins, UserTokens, RoleClaims.

    // ---- Platform kernel (1) ----
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    // ---- Organization and Tenant Management (4) ----
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationSetting> OrganizationSettings => Set<OrganizationSetting>();
    public DbSet<OrganizationStatusHistory> OrganizationStatusHistories => Set<OrganizationStatusHistory>();
    public DbSet<SettingDefinition> SettingDefinitions => Set<SettingDefinition>();

    // ---- Identity and Access: module-specific tables (3) ----
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();

    // ---- Subscription Management (6) ----
    public DbSet<LimitDefinition> LimitDefinitions => Set<LimitDefinition>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<PlanLimit> PlanLimits => Set<PlanLimit>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionHistory> SubscriptionHistories => Set<SubscriptionHistory>();
    public DbSet<UsageCounter> UsageCounters => Set<UsageCounter>();

    // ---- Instrument and Connector Management (12) ----
    public DbSet<AdapterType> AdapterTypes => Set<AdapterType>();
    public DbSet<Connector> Connectors => Set<Connector>();
    public DbSet<ConnectorConfigRevision> ConnectorConfigRevisions => Set<ConnectorConfigRevision>();
    public DbSet<ConnectorHealthState> ConnectorHealthStates => Set<ConnectorHealthState>();
    public DbSet<ConnectorSecretRef> ConnectorSecretRefs => Set<ConnectorSecretRef>();
    public DbSet<ConnectorTestResult> ConnectorTestResults => Set<ConnectorTestResult>();
    public DbSet<ExternalSystem> ExternalSystems => Set<ExternalSystem>();
    public DbSet<Instrument> Instruments => Set<Instrument>();
    public DbSet<InstrumentVerification> InstrumentVerifications => Set<InstrumentVerification>();
    public DbSet<InterfaceProfile> InterfaceProfiles => Set<InterfaceProfile>();
    public DbSet<InterfaceProfileVersion> InterfaceProfileVersions => Set<InterfaceProfileVersion>();
    public DbSet<Route> Routes => Set<Route>();

    // ---- Message Ingestion (6) ----
    public DbSet<DeduplicationKey> DeduplicationKeys => Set<DeduplicationKey>();
    public DbSet<InboundMessage> InboundMessages => Set<InboundMessage>();
    public DbSet<InboundMessageStatusHistory> InboundMessageStatusHistories => Set<InboundMessageStatusHistory>();
    public DbSet<InboundProcessingHandoff> InboundProcessingHandoffs => Set<InboundProcessingHandoff>();
    public DbSet<InboundRefusal> InboundRefusals => Set<InboundRefusal>();
    public DbSet<RawPayload> RawPayloads => Set<RawPayload>();

    // ---- Message Processing (7) ----
    public DbSet<NormalizedHandoff> NormalizedHandoffs => Set<NormalizedHandoff>();
    public DbSet<NormalizedMessage> NormalizedMessages => Set<NormalizedMessage>();
    public DbSet<ParserType> ParserTypes => Set<ParserType>();
    public DbSet<ProcessingReview> ProcessingReviews => Set<ProcessingReview>();
    public DbSet<ProcessingRun> ProcessingRuns => Set<ProcessingRun>();
    public DbSet<ProfileParserBinding> ProfileParserBindings => Set<ProfileParserBinding>();
    public DbSet<ValidationFinding> ValidationFindings => Set<ValidationFinding>();

    // ---- Test Code Mapping (9) ----
    public DbSet<CanonicalConcept> CanonicalConcepts => Set<CanonicalConcept>();
    public DbSet<MappingEntry> MappingEntries => Set<MappingEntry>();
    public DbSet<MappingGap> MappingGaps => Set<MappingGap>();
    public DbSet<MappingGapSubject> MappingGapSubjects => Set<MappingGapSubject>();
    public DbSet<MappingResolution> MappingResolutions => Set<MappingResolution>();
    public DbSet<MappingRun> MappingRuns => Set<MappingRun>();
    public DbSet<MappingSet> MappingSets => Set<MappingSet>();
    public DbSet<MappingSetVersion> MappingSetVersions => Set<MappingSetVersion>();
    public DbSet<UnitConversionRule> UnitConversionRules => Set<UnitConversionRule>();

    // ---- Order and Result Workflow (7) ----
    public DbSet<DeliveryRequest> DeliveryRequests => Set<DeliveryRequest>();
    public DbSet<Observation> Observations => Set<Observation>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<ResultStatusHistory> ResultStatusHistories => Set<ResultStatusHistory>();

    // ---- Delivery (7) ----
    public DbSet<AcknowledgmentRecord> AcknowledgmentRecords => Set<AcknowledgmentRecord>();
    public DbSet<DeliveryAttempt> DeliveryAttempts => Set<DeliveryAttempt>();
    public DbSet<DeliveryItem> DeliveryItems => Set<DeliveryItem>();
    public DbSet<ReconciliationCase> ReconciliationCases => Set<ReconciliationCase>();
    public DbSet<RouteDeliveryPolicy> RouteDeliveryPolicies => Set<RouteDeliveryPolicy>();
    public DbSet<SimulatorBehavior> SimulatorBehaviors => Set<SimulatorBehavior>();
    public DbSet<SimulatorSinkMessage> SimulatorSinkMessages => Set<SimulatorSinkMessage>();

    // ---- Monitoring and Alerting (6) ----
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<AlertRuleSetting> AlertRuleSettings => Set<AlertRuleSetting>();
    public DbSet<AlertStateHistory> AlertStateHistories => Set<AlertStateHistory>();
    public DbSet<ConnectorHealthSnapshot> ConnectorHealthSnapshots => Set<ConnectorHealthSnapshot>();
    public DbSet<MetricSample> MetricSamples => Set<MetricSample>();

    // ---- Audit (5) ----
    public DbSet<AuditActionDefinition> AuditActionDefinitions => Set<AuditActionDefinition>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<AuditIntegrityCheck> AuditIntegrityChecks => Set<AuditIntegrityCheck>();
    public DbSet<AuditSeal> AuditSeals => Set<AuditSeal>();
    public DbSet<AuditSpoolItem> AuditSpoolItems => Set<AuditSpoolItem>();

    // ---- Reporting (3) ----
    public DbSet<DailyFact> DailyFacts => Set<DailyFact>();
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<ReportRun> ReportRuns => Set<ReportRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity's default model first, so the entity configurations below override its names, lengths and
        // delete behavior (same ordering as the former IdentityAccessDbContext).
        base.OnModelCreating(modelBuilder);

        // Required by the citext columns: IdempotencyRecord, Connector, ExternalSystem, Instrument, Route.
        modelBuilder.HasPostgresExtension("citext");

        // Used by InboundMessageConfiguration: HasDefaultValueSql("nextval('inbound_message_receipt_seq')").
        //modelBuilder.HasSequence<long>(InboundMessageReceiptSequence);
        modelBuilder.HasSequence<long>(InboundMessageReceiptSequence, "ingestion");

        // All 83 IEntityTypeConfiguration classes live in this assembly (LabBridge.Infrastructure). No namespace
        // filter: every configuration found is applied, so nothing can be silently excluded.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationConfiguration).Assembly);
    }

    // ---- Persistence guards (consolidated from AuditDbContext and IdentityAccessDbContext) ----
    // All four public overloads (SaveChanges(), SaveChanges(bool), SaveChangesAsync(ct), SaveChangesAsync(bool, ct))
    // funnel into the two overrides below, so every guard runs exactly once per save, before the base implementation.

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnforcePersistenceGuards();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    )
    {
        EnforcePersistenceGuards();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// One pass over the tracked entries, dispatching by entity type. A single ChangeTracker.Entries() call detects
    /// changes once (the former contexts triggered detection once per guarded type).
    /// </summary>
    private void EnforcePersistenceGuards()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.Entity)
            {
                case AuditEvent:
                    GuardAuditEvent(entry);
                    break;
                case AuditSeal:
                    GuardAuditSeal(entry);
                    break;
                case ApplicationUserRole assignment:
                    GuardRoleAssignment(entry, assignment);
                    break;
            }
        }
    }

    // Defense in depth: audit rows are append-only; only the one-time sealing columns of an event may change.
    private static void GuardAuditEvent(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        if (entry.State == EntityState.Deleted)
            throw new BusinessRuleException(
                "AUDIT_EVENT_IMMUTABLE",
                "Audit event immutable",
                "Audit events cannot be deleted."
            );
        if (
            entry.State == EntityState.Modified
            && entry.Properties.Any(p =>
                p.IsModified
                && p.Metadata.Name is not (nameof(AuditEvent.StreamSequence) or nameof(AuditEvent.SealId))
            )
        )
            throw new BusinessRuleException(
                "AUDIT_EVENT_IMMUTABLE",
                "Audit event immutable",
                "Only the sealing columns of an audit event may change."
            );
    }

    private static void GuardAuditSeal(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        if (entry.State is EntityState.Modified or EntityState.Deleted)
            throw new BusinessRuleException(
                "AUDIT_SEAL_IMMUTABLE",
                "Audit seal immutable",
                "Audit seals cannot be changed or deleted."
            );
    }

    // SECURITY: UserManager.AddToRoleAsync creates an ApplicationUserRole through the parameterless constructor, with
    // no AssignedAt/AssignedBy/OrganizationId. That path skips the Domain rules (scope, self-escalation,
    // compatibility). Rows that were not created by ApplicationUser.AssignRole are rejected here.
    private static void GuardRoleAssignment(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
        ApplicationUserRole assignment
    )
    {
        if (entry.State == EntityState.Added && assignment.AssignedAt == default)
            throw new InvalidOperationException(
                "Role assignments must be created with ApplicationUser.AssignRole, not UserManager.AddToRoleAsync."
            );
    }
}