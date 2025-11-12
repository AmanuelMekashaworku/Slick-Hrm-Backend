using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slick.Application.DTOs.Identity
{
    public class GetUser : BaseModel
    {
        public string? Id {  get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string ConfirmPassword { get; set; }
        public required string UserName { get; set; }
        public required bool IsActive { get; set; }
        public required bool IsVerified { get; set; }
        public required string PhoneNumber { get; set; }
        public DateOnly? DateofBirth { get; set; }
        public String? Position { get; set; }
        public String? EmployementIdNumber { get; set; }
        public String? TinNumber { get; set; }
        public required String RoleId { get; set; }
        public Guid? CompanyId { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public Guid? BranchId { get; set; }
        public string? CompanyName { get; set; }
        public string? BranchName { get; set; }
        public string? RoleName { get; set; }
        public int DaysUntilHardDelete { get; set; }
    }
}
