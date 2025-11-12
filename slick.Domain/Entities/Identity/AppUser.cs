using slick.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using slick.Domain.Models;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateOnly? DateofBirth { get; set; }
    public string? Position { get; set; }
    public string? EmployementIdNumber { get; set; }
    public string? TinNumber { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public bool IsDeleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    // Changed from int to Guid
    public Guid CreatedBy { get; set; }          // Not nullable (required)
    public Guid? ModifiedBy { get; set; }        // Nullable
    public Guid? DeletedBy { get; set; }         // Nullable
    public DateTime? DeletedDate { get; set; } // Track when soft-delete occurred// Nullable
    public int DaysUntilHardDelete =>
       IsDeleted ? (45 - (DateTime.Now - DeletedDate.GetValueOrDefault()).Days) : 0;
}
