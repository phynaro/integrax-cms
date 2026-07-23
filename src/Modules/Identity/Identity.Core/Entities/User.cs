using Shared.Kernel.Entities;

namespace Identity.Core.Entities;

public class User : AuditableEntity
{
    public string KeycloakId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public Guid RoleId { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public virtual Role Role { get; set; } = null!;

    public string FullName => $"{FirstName} {LastName}".Trim();
}
