namespace Helper.Web.Models.Account;

public class ProfileViewModel
{
    public Guid Id { get; set; }
    
    public string? Username { get; set; }
    
    public string? Email { get; set; }

    public string? City { get; set; }

    public DateTime RegisterDate { get; set; }

    public DateTime LastLoginDate { get; set; }

    public int CreatedJobs { get; set; }

    public int AcceptedJobs { get; set; }

    public int CompletedJobs { get; set; }

    public int FailedJobs { get; set; }
}