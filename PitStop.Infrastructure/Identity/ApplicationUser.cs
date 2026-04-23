using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PitStop.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ProfilePhoto { get; set; }
}