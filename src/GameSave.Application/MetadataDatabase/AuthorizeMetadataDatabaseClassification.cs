using GameSave.Application.Inspection;

namespace GameSave.Application.MetadataDatabase;

public sealed class AuthorizeMetadataDatabaseClassification
{
    private readonly InspectMetadataDatabase _inspect;
    private readonly IMetadataDatabaseClassificationStore _store;
    private readonly TimeProvider _timeProvider;

    public AuthorizeMetadataDatabaseClassification(
        InspectMetadataDatabase inspect,
        IMetadataDatabaseClassificationStore store,
        TimeProvider? timeProvider = null)
    {
        _inspect = inspect ?? throw new ArgumentNullException(nameof(inspect));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<AuthorizeMetadataDatabaseClassificationResult> ExecuteAsync(
        AuthorizeMetadataDatabaseClassificationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var inspection = await _inspect.ExecuteAsync(cancellationToken);

        if (!string.Equals(
                request.ExpectedInspectionRevision,
                inspection.Context.Revision,
                StringComparison.Ordinal))
        {
            return new AuthorizeMetadataDatabaseClassificationResult(
                AuthorizeMetadataDatabaseClassificationStatus.StaleInspection,
                inspection,
                findings:
                [
                    new InspectionFinding(
                        MetadataDatabaseClassificationFindingCodes.DecisionStale,
                        new Dictionary<string, object?>
                        {
                            ["expected_revision"] =
                                request.ExpectedInspectionRevision,
                            ["current_revision"] =
                                inspection.Context.Revision,
                        }),
                ]);
        }

        if (!inspection.RequiresAdministratorClassification)
        {
            return new AuthorizeMetadataDatabaseClassificationResult(
                AuthorizeMetadataDatabaseClassificationStatus.ClassificationNotRequired,
                inspection);
        }

        if (!inspection.CandidateStates.Contains(request.SelectedState))
        {
            return new AuthorizeMetadataDatabaseClassificationResult(
                AuthorizeMetadataDatabaseClassificationStatus.SelectedStateNotCandidate,
                inspection);
        }

        if (request.SelectedState != inspection.SuggestedState
            && string.IsNullOrWhiteSpace(request.Rationale))
        {
            return new AuthorizeMetadataDatabaseClassificationResult(
                AuthorizeMetadataDatabaseClassificationStatus.RationaleRequired,
                inspection);
        }

        var classification = new AuthorizedMetadataDatabaseClassification(
            Guid.NewGuid(),
            inspection.Context.ResourceIdentity,
            inspection.Context.ResourceLabel,
            inspection.Context.Revision,
            inspection.Context.ClassificationPolicyVersion,
            inspection.CandidateStates,
            inspection.SuggestedState,
            request.SelectedState,
            request.ActorReference,
            request.ActorLabel,
            _timeProvider.GetUtcNow(),
            request.Rationale);

        var writeStatus = await _store.AppendAsync(
            classification,
            cancellationToken);

        return writeStatus switch
        {
            MetadataDatabaseClassificationStoreStatus.Ready =>
                new AuthorizeMetadataDatabaseClassificationResult(
                    AuthorizeMetadataDatabaseClassificationStatus.Recorded,
                    inspection,
                    classification),

            MetadataDatabaseClassificationStoreStatus.Unavailable =>
                new AuthorizeMetadataDatabaseClassificationResult(
                    AuthorizeMetadataDatabaseClassificationStatus.StoreUnavailable,
                    inspection,
                    findings:
                    [
                        new InspectionFinding(
                            MetadataDatabaseClassificationFindingCodes
                                .StoreUnavailable),
                    ]),

            MetadataDatabaseClassificationStoreStatus.Invalid =>
                new AuthorizeMetadataDatabaseClassificationResult(
                    AuthorizeMetadataDatabaseClassificationStatus.StoreInvalid,
                    inspection,
                    findings:
                    [
                        new InspectionFinding(
                            MetadataDatabaseClassificationFindingCodes
                                .StoreInvalid),
                    ]),

            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
