using System.ComponentModel.DataAnnotations;
using Helper.Domain.Entities;

namespace Helper.Web.Models.Jobs;

public class JobViewModel
{
    public Job JobModel { get; set; }
    
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    
    public IEnumerable<Job> Jobs { get; set; } = null!;
}

