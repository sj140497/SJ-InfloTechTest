using System;
using System.ComponentModel.DataAnnotations;
using UserManagement.Web.Models.Logs;

namespace UserManagement.Web.Models.Users;

public class UserDetailsViewModel
{
    public long Id { get; set; }

    [Display(Name = "First Name")]
    public string Forename { get; set; } = string.Empty;

    [Display(Name = "Last Name")]
    public string Surname { get; set; } = string.Empty;

    [Display(Name = "Full Name")]
    public string FullName => $"{Forename} {Surname}";

    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Date of Birth")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Age")]
    public int? Age 
    { 
        get 
        {
            if (!DateOfBirth.HasValue) return null;
            
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Value.Year;
            
            // Subtract one year if birthday hasn't occurred this year
            if (DateOfBirth.Value.Date > today.AddYears(-age))
                age--;
                
            return age;
        } 
    }

    [Display(Name = "Account Status")]
    public bool IsActive { get; set; }

    [Display(Name = "Account Status")]
    public string AccountStatus => IsActive ? "Active" : "Inactive";
    
    // Recent activity logs for this user
    public List<UserLogListItemViewModel> RecentLogs { get; set; } = new();
}
