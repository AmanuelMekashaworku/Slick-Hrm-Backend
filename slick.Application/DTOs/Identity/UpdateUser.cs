
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slick.Application.DTOs.Identity
{
    public class UpdateUser 
    {
        public required string Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string UserName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public  bool IsActive { get; set; }
        public  bool IsVerified { get; set; }
        public DateOnly? DateofBirth { get; set; }
        public string? Position { get; set; }
        public string? EmployementIdNumber { get; set; }
        public string? TinNumber { get; set; }
        public required string RoleId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
