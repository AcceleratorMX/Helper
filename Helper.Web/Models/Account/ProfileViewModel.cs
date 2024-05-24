using System.ComponentModel.DataAnnotations;

namespace Helper.Web.Models.Account;

public class ProfileViewModel
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    
    [EmailAddress(ErrorMessage = "Некоректна адреса електронної пошти!")]
    [StringLength(30, MinimumLength = 0, ErrorMessage = "Адреса не може бути довшою за {1} символів!")]
    public string? Email { get; set; }
    
    [StringLength(30, MinimumLength = 0, ErrorMessage = "Назва міста може бути не довшою {1} символів!")]
    public string? City { get; set; }
    public DateTime RegisterDate { get; set; }
    public DateTime LastLoginDate { get; set; }
    public int CreatedJobs { get; set; }
    public int AcceptedJobs { get; set; }
    public int CompletedJobs { get; set; }
    public int FailedJobs { get; set; }
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? RepeatPassword { get; set; }
}