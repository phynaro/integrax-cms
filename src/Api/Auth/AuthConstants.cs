namespace Api.Auth;

public static class AuthConstants
{
    public static class Roles
    {
        public const string SystemAdmin = "SystemAdmin";
        public const string Manager = "Manager";
        public const string Agent = "Agent";
        public const string Viewer = "Viewer";
    }

    public static class Policies
    {
        public const string RequireSystemAdmin = "RequireSystemAdmin";
        public const string RequireManagerOrAbove = "RequireManagerOrAbove";
        public const string RequireAuthenticated = "RequireAuthenticated";
    }

    public static class Permissions
    {
        public const string ClientsRead = "clients:read";
        public const string ClientsWrite = "clients:write";
        public const string PortfoliosRead = "portfolios:read";
        public const string PortfoliosWrite = "portfolios:write";
        public const string UsersRead = "users:read";
        public const string UsersWrite = "users:write";
        public const string AuditRead = "audit:read";
        public const string All = "*";
    }
}
