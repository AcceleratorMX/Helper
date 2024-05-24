using Helper.Domain.Entities;

namespace Helper.Web.Models.Home;

public class HomeViewModel
{
    public Job JobModel { get; set; } = null!;
    public IEnumerable<Job> Jobs { get; set; } = null!;
}