using GameSave.Application.Inspection;

namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Pure readiness policy over an H2.1C inspection result.
/// It never executes initialization, migration, recovery or other mutations.
/// </summary>
public sealed class EvaluateMetadataDatabaseReadiness
{
    private static readonly MetadataDatabaseCapability[] DiagnosticCapabilities =
    [
        MetadataDatabaseCapability.ViewStatus,
        MetadataDatabaseCapability.ViewDiagnostics,
        MetadataDatabaseCapability.RetryInspection,
    ];

    public MetadataDatabaseReadiness Execute(
        MetadataDatabaseInspection inspection,
        MetadataRecoveryAvailability recoveryAvailability =
            MetadataRecoveryAvailability.Unknown)
    {
        ArgumentNullException.ThrowIfNull(inspection);

        if (inspection.RequiresAdministratorClassification)
        {
            return Build(
                MetadataDatabaseOperationalMode.Maintenance,
                requiresAdministratorClassification: true,
                [MetadataDatabaseCapability.ResolveClassification],
                [
                    new InspectionFinding(
                        MetadataDatabaseReadinessFindingCodes.ClassificationRequired,
                        new Dictionary<string, object?>
                        {
                            ["candidate_count"] = inspection.CandidateStates.Count,
                        }),
                ]);
        }

        var state = inspection.CandidateStates[0];

        return state switch
        {
            MetadataDatabaseState.Ready => Build(
                MetadataDatabaseOperationalMode.Normal,
                additionalCapabilities:
                [
                    MetadataDatabaseCapability.UseMetadataAuthority,
                ]),

            MetadataDatabaseState.Missing
                or MetadataDatabaseState.Uninitialized => BuildMaintenanceWithRecovery(
                    MetadataDatabaseCapability.InitializeMetadataDatabase,
                    recoveryAvailability),

            MetadataDatabaseState.MigrationRequired => Build(
                MetadataDatabaseOperationalMode.Maintenance,
                additionalCapabilities:
                [
                    MetadataDatabaseCapability.ApplyPendingMigrations,
                ]),

            MetadataDatabaseState.Invalid
                or MetadataDatabaseState.TooNew
                or MetadataDatabaseState.Unavailable =>
                    BuildRecoveryOnly(recoveryAvailability),

            _ => throw new ArgumentOutOfRangeException(
                nameof(inspection),
                state,
                "Unsupported metadata database state."),
        };
    }

    private static MetadataDatabaseReadiness BuildMaintenanceWithRecovery(
        MetadataDatabaseCapability maintenanceCapability,
        MetadataRecoveryAvailability recoveryAvailability)
    {
        var additional = new List<MetadataDatabaseCapability>
        {
            maintenanceCapability,
        };

        AddRecoveryCapabilities(additional, recoveryAvailability);

        return Build(
            MetadataDatabaseOperationalMode.Maintenance,
            additionalCapabilities: additional);
    }

    private static MetadataDatabaseReadiness BuildRecoveryOnly(
        MetadataRecoveryAvailability recoveryAvailability)
    {
        return recoveryAvailability switch
        {
            MetadataRecoveryAvailability.Available => Build(
                MetadataDatabaseOperationalMode.RestrictedRecovery,
                additionalCapabilities:
                [
                    MetadataDatabaseCapability.DiscoverRecoveryCandidates,
                    MetadataDatabaseCapability.RestoreMetadataSnapshot,
                ]),

            MetadataRecoveryAvailability.Unknown => Build(
                MetadataDatabaseOperationalMode.RestrictedRecovery,
                additionalCapabilities:
                [
                    MetadataDatabaseCapability.DiscoverRecoveryCandidates,
                ],
                findings:
                [
                    new InspectionFinding(
                        MetadataDatabaseReadinessFindingCodes
                            .RecoveryAvailabilityUnknown),
                ]),

            MetadataRecoveryAvailability.Unavailable => Build(
                MetadataDatabaseOperationalMode.OutOfService,
                findings:
                [
                    new InspectionFinding(
                        MetadataDatabaseReadinessFindingCodes.RecoveryUnavailable),
                ]),

            _ => throw new ArgumentOutOfRangeException(
                nameof(recoveryAvailability),
                recoveryAvailability,
                "Unsupported recovery availability."),
        };
    }

    private static void AddRecoveryCapabilities(
        ICollection<MetadataDatabaseCapability> capabilities,
        MetadataRecoveryAvailability recoveryAvailability)
    {
        if (recoveryAvailability is MetadataRecoveryAvailability.Unknown
            or MetadataRecoveryAvailability.Available)
        {
            capabilities.Add(
                MetadataDatabaseCapability.DiscoverRecoveryCandidates);
        }

        if (recoveryAvailability == MetadataRecoveryAvailability.Available)
        {
            capabilities.Add(
                MetadataDatabaseCapability.RestoreMetadataSnapshot);
        }
    }

    private static MetadataDatabaseReadiness Build(
        MetadataDatabaseOperationalMode mode,
        bool requiresAdministratorClassification = false,
        IEnumerable<MetadataDatabaseCapability>? additionalCapabilities = null,
        IEnumerable<InspectionFinding>? findings = null)
    {
        var capabilities = new List<MetadataDatabaseCapability>(
            DiagnosticCapabilities);

        if (additionalCapabilities is not null)
        {
            capabilities.AddRange(additionalCapabilities);
        }

        return new MetadataDatabaseReadiness(
            mode,
            metadataAuthorityAvailable:
                mode == MetadataDatabaseOperationalMode.Normal,
            requiresAdministratorClassification,
            capabilities,
            findings ?? []);
    }
}
