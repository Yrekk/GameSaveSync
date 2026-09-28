using GameSave.Application.Inspection;

namespace GameSave.Application.MetadataDatabase;

public sealed class ResolveMetadataDatabaseClassification(
    IMetadataDatabaseClassificationStore store)
{
    private readonly IMetadataDatabaseClassificationStore _store =
        store ?? throw new ArgumentNullException(nameof(store));

    public async Task<MetadataDatabaseClassificationResolution> ExecuteAsync(
        MetadataDatabaseInspection inspection,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inspection);

        if (!inspection.RequiresAdministratorClassification)
        {
            return MetadataDatabaseClassificationResolution.Deterministic(inspection);
        }

        var snapshot = await _store.ReadAsync(cancellationToken);

        if (snapshot.Status != MetadataDatabaseClassificationStoreStatus.Ready)
        {
            var code = snapshot.Status switch
            {
                MetadataDatabaseClassificationStoreStatus.Unavailable =>
                    MetadataDatabaseClassificationFindingCodes.StoreUnavailable,
                MetadataDatabaseClassificationStoreStatus.Invalid =>
                    MetadataDatabaseClassificationFindingCodes.StoreInvalid,
                _ => throw new ArgumentOutOfRangeException(),
            };

            return MetadataDatabaseClassificationResolution.Unresolved(
                inspection,
                [new InspectionFinding(code)]);
        }

        var compatible = snapshot.History
            .LastOrDefault(classification =>
                IsCompatible(classification, inspection));

        if (compatible is null)
        {
            return MetadataDatabaseClassificationResolution.Unresolved(
                inspection,
                [
                    new InspectionFinding(
                        MetadataDatabaseReadinessFindingCodes.ClassificationRequired),
                ]);
        }

        return MetadataDatabaseClassificationResolution.Authorized(
            inspection,
            compatible);
    }

    private static bool IsCompatible(
        AuthorizedMetadataDatabaseClassification classification,
        MetadataDatabaseInspection inspection)
    {
        if (!string.Equals(
                classification.ResourceIdentity,
                inspection.Context.ResourceIdentity,
                StringComparison.Ordinal)
            || !string.Equals(
                classification.InspectionRevision,
                inspection.Context.Revision,
                StringComparison.Ordinal)
            || classification.ClassificationPolicyVersion
                != inspection.Context.ClassificationPolicyVersion
            || classification.SuggestedState != inspection.SuggestedState
            || !inspection.CandidateStates.Contains(classification.SelectedState))
        {
            return false;
        }

        return classification.CandidateStates
            .OrderBy(state => state)
            .SequenceEqual(
                inspection.CandidateStates.OrderBy(state => state));
    }
}
