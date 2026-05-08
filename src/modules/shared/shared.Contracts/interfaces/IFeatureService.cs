
using shared.Contracts.dtos;

namespace shared.Contracts.interfaces;

public interface IFeatureService
{
    Task<List<FeatureWithModuleDto>> GetFeaturesByIdsAsync(IEnumerable<int> featureIds);
}