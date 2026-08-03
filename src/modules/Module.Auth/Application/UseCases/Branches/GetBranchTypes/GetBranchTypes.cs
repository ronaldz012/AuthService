using Common.Utilities;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Branches.GetBranchTypes;

public class GetBranchTypes
{
    public Task<Result<List<BranchTypeResponse>>> Execute()
    {
        var types = Enum.GetValues(typeof(BranchType))
            .Cast<BranchType>()
            .Select(t => new BranchTypeResponse
            {
                Value = (int)t,
                Name = t.ToString()
            })
            .ToList();

        return Task.FromResult<Result<List<BranchTypeResponse>>>(types);
    }
}
