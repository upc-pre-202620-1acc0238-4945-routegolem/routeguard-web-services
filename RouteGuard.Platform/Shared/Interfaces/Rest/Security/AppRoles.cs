namespace RouteGuard.Platform.Shared.Interfaces.Rest.Security;

/// <summary>Role names carried in the JWT <c>role</c> claim (same values as the IAM <c>RoleTier</c>).</summary>
public static class AppRoles
{
    public const string Admin = "ADMIN";
    public const string Driver = "DRIVER";
    public const string Parent = "PARENT";

    public const string AdminOrDriver = Admin + "," + Driver;
    public const string AdminOrParent = Admin + "," + Parent;
    public const string Any = Admin + "," + Driver + "," + Parent;
}
