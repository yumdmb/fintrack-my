using Microsoft.AspNetCore.Identity;

namespace Fintrack.Api.Data;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
