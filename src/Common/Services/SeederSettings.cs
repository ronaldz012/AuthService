namespace Common.Services;

public class SeederSettings
{
    public const string SectionName = "Seeder";

    public bool Enabled { get; set; } = true;
    public string AdminEmail { get; set; } = "admin@drivecore.com";
    public string AdminPassword { get; set; } = "DriveCore@2026";
}
