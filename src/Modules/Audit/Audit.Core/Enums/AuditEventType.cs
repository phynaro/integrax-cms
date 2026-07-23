namespace Audit.Core.Enums;

public enum AuditEventType
{
    Create,
    Update,
    Delete,
    Login,
    Logout,
    LoginFailed,
    RoleChange,
    UserActivate,
    UserDeactivate,
    Export,
    Import
}
