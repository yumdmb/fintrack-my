namespace Fintrack.Api.Data;

public static class AppRoles
{
    public const string Admin = "admin";
    public const string Accountant = "accountant";
    public const string Staff = "staff";

    public static readonly string[] All = [Admin, Accountant, Staff];
}
