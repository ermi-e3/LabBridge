
```
LabBridge
├─ LabBridge.slnx
├─ src
│  ├─ LabBridge.Api
│  │  ├─ appsettings.Development.json
│  │  ├─ appsettings.json
│  │  ├─ Controllers
│  │  │  └─ WeatherForecastController.cs
│  │  ├─ LabBridge.Api.csproj
│  │  ├─ LabBridge.Api.http
│  │  ├─ Program.cs
│  │  ├─ Properties
│  │  │  └─ launchSettings.json
│  │  └─ WeatherForecast.cs
│  ├─ LabBridge.Application
│  │  └─ LabBridge.Application.csproj
│  ├─ LabBridge.Domain
│  │  ├─ Common
│  │  │  ├─ AggregateRoot.cs
│  │  │  ├─ AuditedAggregateRoot.cs
│  │  │  ├─ AuditedEntity.cs
│  │  │  ├─ Entity.cs
│  │  │  ├─ Enums
│  │  │  │  ├─ MessageDirection.cs
│  │  │  │  └─ MessageKind.cs
│  │  │  ├─ Events
│  │  │  │  └─ IDomainEvent.cs
│  │  │  ├─ Exceptions
│  │  │  │  ├─ AppException.cs
│  │  │  │  ├─ AuthenticationException.cs
│  │  │  │  ├─ BusinessRuleException.cs
│  │  │  │  ├─ ConcurrencyException.cs
│  │  │  │  ├─ ConflictException.cs
│  │  │  │  ├─ DuplicateResourceException.cs
│  │  │  │  ├─ ForbiddenException.cs
│  │  │  │  ├─ ResourceGoneException.cs
│  │  │  │  ├─ ResourceNotFoundException.cs
│  │  │  │  └─ ValidationException.cs
│  │  │  ├─ Guard.cs
│  │  │  ├─ Idempotency
│  │  │  │  └─ IdempotencyRecord.cs
│  │  │  ├─ IOrganizationOwned.cs
│  │  │  └─ ValueObjects
│  │  │     ├─ ActorReference.cs
│  │  │     ├─ EmailAddress.cs
│  │  │     └─ Sha256Hash.cs
│  │  ├─ Features
│  │  │  ├─ Audit
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ AuditActionDefinition.cs
│  │  │  │  │  ├─ AuditEvent.cs
│  │  │  │  │  ├─ AuditIntegrityCheck.cs
│  │  │  │  │  ├─ AuditSeal.cs
│  │  │  │  │  └─ AuditSpoolItem.cs
│  │  │  │  └─ Enums
│  │  │  │     ├─ AuditActionClass.cs
│  │  │  │     ├─ AuditActorType.cs
│  │  │  │     ├─ AuditOutcome.cs
│  │  │  │     ├─ AuditScope.cs
│  │  │  │     ├─ EmissionMode.cs
│  │  │  │     ├─ IntegrityCheckStatus.cs
│  │  │  │     └─ SpoolStatus.cs
│  │  │  ├─ DeliveryManagement
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ AcknowledgmentRecord.cs
│  │  │  │  │  ├─ DeliveryAttempt.cs
│  │  │  │  │  ├─ DeliveryItem.cs
│  │  │  │  │  ├─ ReconciliationCase.cs
│  │  │  │  │  ├─ RouteDeliveryPolicy.cs
│  │  │  │  │  ├─ SimulatorBehavior.cs
│  │  │  │  │  └─ SimulatorSinkMessage.cs
│  │  │  │  └─ Enums
│  │  │  │     ├─ AcceptanceOutcome.cs
│  │  │  │     ├─ AckMatchStatus.cs
│  │  │  │     ├─ AckOutcome.cs
│  │  │  │     ├─ AttemptResult.cs
│  │  │  │     ├─ DeliveryHoldReason.cs
│  │  │  │     ├─ DeliveryState.cs
│  │  │  │     ├─ JitterMode.cs
│  │  │  │     ├─ ReconciliationCaseStatus.cs
│  │  │  │     ├─ ReconciliationResolution.cs
│  │  │  │     ├─ SimulatorMode.cs
│  │  │  │     └─ TransportOutcome.cs
│  │  │  ├─ IdentityAccess
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ Permission.cs
│  │  │  │  │  ├─ RolePermission.cs
│  │  │  │  │  └─ UserInvitation.cs
│  │  │  │  ├─ Enums
│  │  │  │  │  ├─ PermissionScope.cs
│  │  │  │  │  ├─ RoleScope.cs
│  │  │  │  │  └─ UserStatus.cs
│  │  │  │  ├─ RoleCompatibilityPolicy.cs
│  │  │  │  ├─ RoleSnapshot.cs
│  │  │  │  ├─ SystemRoleCodes.cs
│  │  │  │  └─ UserAccessPolicy.cs
│  │  │  ├─ InstrumentManagement
│  │  │  │  ├─ ConnectorEnabled.cs
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ AdapterType.cs
│  │  │  │  │  ├─ Connector.cs
│  │  │  │  │  ├─ ConnectorConfigRevision.cs
│  │  │  │  │  ├─ ConnectorHealthState.cs
│  │  │  │  │  ├─ ConnectorSecretRef.cs
│  │  │  │  │  ├─ ConnectorTestResult.cs
│  │  │  │  │  ├─ ExternalSystem.cs
│  │  │  │  │  ├─ Instrument.cs
│  │  │  │  │  ├─ InstrumentVerification.cs
│  │  │  │  │  ├─ InterfaceProfile.cs
│  │  │  │  │  ├─ InterfaceProfileVersion.cs
│  │  │  │  │  └─ Route.cs
│  │  │  │  └─ Enums
│  │  │  │     ├─ ActiveStatus.cs
│  │  │  │     ├─ CompatibilityClass.cs
│  │  │  │     ├─ ConnectorDirection.cs
│  │  │  │     ├─ ConnectorHealthStatus.cs
│  │  │  │     ├─ ConnectorStatus.cs
│  │  │  │     ├─ ConnectorTestOutcome.cs
│  │  │  │     ├─ ExternalSystemType.cs
│  │  │  │     ├─ InstrumentTopology.cs
│  │  │  │     ├─ ProfileVersionStatus.cs
│  │  │  │     ├─ RouteStatus.cs
│  │  │  │     ├─ VerificationKind.cs
│  │  │  │     ├─ VerificationOutcome.cs
│  │  │  │     └─ VerificationStatus.cs
│  │  │  ├─ MessageIngestion
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ DeduplicationKey.cs
│  │  │  │  │  ├─ InboundMessage.cs
│  │  │  │  │  ├─ InboundMessageStatusHistory.cs
│  │  │  │  │  ├─ InboundProcessingHandoff.cs
│  │  │  │  │  ├─ InboundRefusal.cs
│  │  │  │  │  └─ RawPayload.cs
│  │  │  │  ├─ Enums
│  │  │  │  │  ├─ DedupKeySource.cs
│  │  │  │  │  ├─ HandoffStatus.cs
│  │  │  │  │  ├─ MessageStatus.cs
│  │  │  │  │  ├─ ReceiptOutcome.cs
│  │  │  │  │  └─ RefusalReason.cs
│  │  │  │  ├─ InboundMessageReceived.cs
│  │  │  │  └─ MessageStatusTransitions.cs
│  │  │  ├─ MessageProcessing
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ NormalizedHandoff.cs
│  │  │  │  │  ├─ NormalizedMessage.cs
│  │  │  │  │  ├─ ParserType.cs
│  │  │  │  │  ├─ ProcessingReview.cs
│  │  │  │  │  ├─ ProcessingRun.cs
│  │  │  │  │  ├─ ProfileParserBinding.cs
│  │  │  │  │  └─ ValidationFinding.cs
│  │  │  │  └─ Enums
│  │  │  │     ├─ BindingStatus.cs
│  │  │  │     ├─ ConsumerKind.cs
│  │  │  │     ├─ FindingCategory.cs
│  │  │  │     ├─ FindingSeverity.cs
│  │  │  │     ├─ NormalizedHandoffStatus.cs
│  │  │  │     ├─ ProcessingOutcome.cs
│  │  │  │     ├─ ProcessingRunStatus.cs
│  │  │  │     ├─ ProcessingTrigger.cs
│  │  │  │     └─ ReviewDecision.cs
│  │  │  ├─ Monitoring
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ Alert.cs
│  │  │  │  │  ├─ AlertRule.cs
│  │  │  │  │  ├─ AlertRuleSetting.cs
│  │  │  │  │  ├─ AlertStateHistory.cs
│  │  │  │  │  ├─ ConnectorHealthSnapshot.cs
│  │  │  │  │  └─ MetricSample.cs
│  │  │  │  └─ Enums
│  │  │  │     ├─ AlertScope.cs
│  │  │  │     ├─ AlertSeverity.cs
│  │  │  │     ├─ AlertStatus.cs
│  │  │  │     ├─ MetricKind.cs
│  │  │  │     ├─ ResolveReason.cs
│  │  │  │     └─ RuleKind.cs
│  │  │  ├─ OrderResultManagement
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ DeliveryRequest.cs
│  │  │  │  │  ├─ Observation.cs
│  │  │  │  │  ├─ Order.cs
│  │  │  │  │  ├─ OrderLine.cs
│  │  │  │  │  ├─ OrderStatusHistory.cs
│  │  │  │  │  ├─ Result.cs
│  │  │  │  │  └─ ResultStatusHistory.cs
│  │  │  │  ├─ Enums
│  │  │  │  │  ├─ CorrelationMethod.cs
│  │  │  │  │  ├─ CorrelationStatus.cs
│  │  │  │  │  ├─ DeliveryRequestStatus.cs
│  │  │  │  │  ├─ DeliverySubjectType.cs
│  │  │  │  │  ├─ OrderStatus.cs
│  │  │  │  │  ├─ QuarantineReason.cs
│  │  │  │  │  ├─ ResultStatus.cs
│  │  │  │  │  ├─ StatusActorType.cs
│  │  │  │  │  └─ StatusSource.cs
│  │  │  │  ├─ OrderData.cs
│  │  │  │  ├─ ResultData.cs
│  │  │  │  └─ StatusTransitionTable.cs
│  │  │  ├─ OrganizationManagement
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ Organization.cs
│  │  │  │  │  ├─ OrganizationSetting.cs
│  │  │  │  │  ├─ OrganizationStatusHistory.cs
│  │  │  │  │  └─ SettingDefinition.cs
│  │  │  │  ├─ Enums
│  │  │  │  │  ├─ OrganizationStatus.cs
│  │  │  │  │  ├─ SettingEditableBy.cs
│  │  │  │  │  ├─ SettingEditorScope.cs
│  │  │  │  │  ├─ SettingValueType.cs
│  │  │  │  │  └─ SuspensionMode.cs
│  │  │  │  ├─ OrganizationCode.cs
│  │  │  │  ├─ OrganizationOperatingPolicy.cs
│  │  │  │  └─ OrganizationStatusChanged.cs
│  │  │  ├─ Reporting
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ DailyFact.cs
│  │  │  │  │  ├─ ReportDefinition.cs
│  │  │  │  │  └─ ReportRun.cs
│  │  │  │  └─ Enums
│  │  │  │     └─ ReportRunStatus.cs
│  │  │  ├─ SubscriptionManagement
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ LimitDefinition.cs
│  │  │  │  │  ├─ Plan.cs
│  │  │  │  │  ├─ PlanLimit.cs
│  │  │  │  │  ├─ Subscription.cs
│  │  │  │  │  ├─ SubscriptionHistory.cs
│  │  │  │  │  └─ UsageCounter.cs
│  │  │  │  └─ Enums
│  │  │  │     ├─ EnforcementMode.cs
│  │  │  │     ├─ LimitKind.cs
│  │  │  │     ├─ PlanStatus.cs
│  │  │  │     └─ SubscriptionStatus.cs
│  │  │  └─ TestCodeMapping
│  │  │     ├─ Entities
│  │  │     │  ├─ CanonicalConcept.cs
│  │  │     │  ├─ MappingEntry.cs
│  │  │     │  ├─ MappingGap.cs
│  │  │     │  ├─ MappingGapSubject.cs
│  │  │     │  ├─ MappingResolution.cs
│  │  │     │  ├─ MappingRun.cs
│  │  │     │  ├─ MappingSet.cs
│  │  │     │  ├─ MappingSetVersion.cs
│  │  │     │  └─ UnitConversionRule.cs
│  │  │     └─ Enums
│  │  │        ├─ CounterpartyKind.cs
│  │  │        ├─ GapStatus.cs
│  │  │        ├─ MappingDirection.cs
│  │  │        ├─ MappingDomain.cs
│  │  │        ├─ MappingGapKind.cs
│  │  │        ├─ MappingRunStatus.cs
│  │  │        ├─ MappingRunTrigger.cs
│  │  │        ├─ MappingSubjectType.cs
│  │  │        ├─ ResolutionType.cs
│  │  │        ├─ RoundingMode.cs
│  │  │        ├─ RuleStatus.cs
│  │  │        ├─ UnmatchedPolicy.cs
│  │  │        └─ VersionStatus.cs
│  │  └─ LabBridge.Domain.csproj
│  └─ LabBridge.Infrastructure
│     ├─ Features
│     ├─ Identity
│     │  ├─ ApplicationRole.cs
│     │  ├─ ApplicationUser.cs
│     │  └─ ApplicationUserRole.cs
│     ├─ LabBridge.Infrastructure.csproj
│     └─ Persistence
│        ├─ Configurations
│        │  ├─ Audit
│        │  │  ├─ AuditActionDefinitionConfiguration.cs
│        │  │  ├─ AuditEventConfiguration.cs
│        │  │  ├─ AuditIntegrityCheckConfiguration.cs
│        │  │  ├─ AuditSealConfiguration.cs
│        │  │  └─ AuditSpoolItemConfiguration.cs
│        │  ├─ Common
│        │  │  ├─ EntityTypeBuilderExtensions.cs
│        │  │  └─ IdempotencyRecordConfiguration.cs
│        │  ├─ DeliveryManagement
│        │  │  ├─ AcknowledgmentRecordConfiguration.cs
│        │  │  ├─ DeliveryAttemptConfiguration.cs
│        │  │  ├─ DeliveryItemConfiguration.cs
│        │  │  ├─ ReconciliationCaseConfiguration.cs
│        │  │  ├─ RouteDeliveryPolicyConfiguration.cs
│        │  │  ├─ SimulatorBehaviorConfiguration.cs
│        │  │  └─ SimulatorSinkMessageConfiguration.cs
│        │  ├─ IdentityAccess
│        │  │  ├─ ApplicationRoleConfiguration.cs
│        │  │  ├─ ApplicationUserConfiguration.cs
│        │  │  ├─ ApplicationUserRoleConfiguration.cs
│        │  │  ├─ ExternalLoginConfiguration.cs
│        │  │  ├─ PermissionConfiguration.cs
│        │  │  ├─ RoleClaimConfiguration.cs
│        │  │  ├─ RolePermissionConfiguration.cs
│        │  │  ├─ UserClaimConfiguration.cs
│        │  │  ├─ UserInvitationConfiguration.cs
│        │  │  └─ UserTokenConfiguration.cs
│        │  ├─ InstrumentManagement
│        │  │  ├─ AdapterTypeConfiguration.cs
│        │  │  ├─ ConnectorConfigRevisionConfiguration.cs
│        │  │  ├─ ConnectorConfiguration.cs
│        │  │  ├─ ConnectorHealthStateConfiguration.cs
│        │  │  ├─ ConnectorSecretRefConfiguration.cs
│        │  │  ├─ ConnectorTestResultConfiguration.cs
│        │  │  ├─ ExternalSystemConfiguration.cs
│        │  │  ├─ InstrumentConfiguration.cs
│        │  │  ├─ InstrumentVerificationConfiguration.cs
│        │  │  ├─ InterfaceProfileConfiguration.cs
│        │  │  ├─ InterfaceProfileVersionConfiguration.cs
│        │  │  └─ RouteConfiguration.cs
│        │  ├─ MessageIngestion
│        │  │  ├─ DeduplicationKeyConfiguration.cs
│        │  │  ├─ InboundMessageConfiguration.cs
│        │  │  ├─ InboundMessageStatusHistoryConfiguration.cs
│        │  │  ├─ InboundProcessingHandoffConfiguration.cs
│        │  │  ├─ InboundRefusalConfiguration.cs
│        │  │  └─ RawPayloadConfiguration.cs
│        │  ├─ MessageProcessing
│        │  │  ├─ NormalizedHandoffConfiguration.cs
│        │  │  ├─ NormalizedMessageConfiguration.cs
│        │  │  ├─ ParserTypeConfiguration.cs
│        │  │  ├─ ProcessingReviewConfiguration.cs
│        │  │  ├─ ProcessingRunConfiguration.cs
│        │  │  ├─ ProfileParserBindingConfiguration.cs
│        │  │  └─ ValidationFindingConfiguration.cs
│        │  ├─ Monitoring
│        │  │  ├─ AlertConfiguration.cs
│        │  │  ├─ AlertRuleConfiguration.cs
│        │  │  ├─ AlertRuleSettingConfiguration.cs
│        │  │  ├─ AlertStateHistoryConfiguration.cs
│        │  │  ├─ ConnectorHealthSnapshotConfiguration.cs
│        │  │  └─ MetricSampleConfiguration.cs
│        │  ├─ OrderResultManagement
│        │  │  ├─ DeliveryRequestConfiguration.cs
│        │  │  ├─ ObservationConfiguration.cs
│        │  │  ├─ OrderConfiguration.cs
│        │  │  ├─ OrderLineConfiguration.cs
│        │  │  ├─ OrderStatusHistoryConfiguration.cs
│        │  │  ├─ ResultConfiguration.cs
│        │  │  └─ ResultStatusHistoryConfiguration.cs
│        │  ├─ OrganizationManagement
│        │  │  ├─ OrganizationConfiguration.cs
│        │  │  ├─ OrganizationSettingConfiguration.cs
│        │  │  ├─ OrganizationStatusHistoryConfiguration.cs
│        │  │  └─ SettingDefinitionConfiguration.cs
│        │  ├─ Reporting
│        │  │  ├─ DailyFactConfiguration.cs
│        │  │  ├─ ReportDefinitionConfiguration.cs
│        │  │  └─ ReportRunConfiguration.cs
│        │  ├─ SubscriptionManagement
│        │  │  ├─ LimitDefinitionConfiguration.cs
│        │  │  ├─ PlanConfiguration.cs
│        │  │  ├─ PlanLimitConfiguration.cs
│        │  │  ├─ SubscriptionConfiguration.cs
│        │  │  ├─ SubscriptionHistoryConfiguration.cs
│        │  │  └─ UsageCounterConfiguration.cs
│        │  └─ TestCodeMapping
│        │     ├─ CanonicalConceptConfiguration.cs
│        │     ├─ MappingEntryConfiguration.cs
│        │     ├─ MappingGapConfiguration.cs
│        │     ├─ MappingGapSubjectConfiguration.cs
│        │     ├─ MappingResolutionConfiguration.cs
│        │     ├─ MappingRunConfiguration.cs
│        │     ├─ MappingSetConfiguration.cs
│        │     ├─ MappingSetVersionConfiguration.cs
│        │     └─ UnitConversionRuleConfiguration.cs
│        ├─ DbContexts
│        │  ├─ AuditDbContext.cs
│        │  ├─ DeliveryManagementDbContext.cs
│        │  ├─ IdentityAccessDbContext.cs
│        │  ├─ InstrumentManagementDbContext.cs
│        │  ├─ MessageIngestionDbContext.cs
│        │  ├─ MessageProcessingDbContext.cs
│        │  ├─ MonitoringDbContext.cs
│        │  ├─ OrderResultManagementDbContext.cs
│        │  ├─ OrganizationManagementDbContext.cs
│        │  ├─ PlatformKernelDbContext.cs
│        │  ├─ ReportingDbContext.cs
│        │  ├─ SubscriptionManagementDbContext.cs
│        │  └─ TestCodeMappingDbContext.cs
│        ├─ Migrations
│        └─ Seed
└─ tests

```