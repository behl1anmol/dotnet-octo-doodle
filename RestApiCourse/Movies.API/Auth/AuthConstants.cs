namespace Movies.API.Auth;

public static class AuthConstants
{
    public const string AdminUserPolicyName = "Admin";
    public const string AdminUserClaimName = "admin";
    public const string TrustedUserPolicyName = "TrustedUser";
    public const string TrustedUserClaimName = "trusted_member";
    public const string TrustedorAdminUserPolicyName = "TrustedUser";
    public const string ApiKeyHeaderName = "x-api-key";
}
