namespace Module.Auth.Domain;

public static class BranchFeatureKeysResolver
{
    public static List<string> Resolve(
        List<string> planAllowedKeys,
        BranchType branchType,
        IEnumerable<FeatureModuleInfo> features)
    {
        var allowedModules = branchType == BranchType.Warehouse
            ? new[] { Module.Inventory }
            : new[] { Module.Inventory, Module.Sales };

        return features
            .Where(f => planAllowedKeys.Contains(f.Key) && allowedModules.Contains(f.Module))
            .Select(f => f.Key)
            .ToList();
    }
}

public readonly record struct FeatureModuleInfo(string Key, Module Module);