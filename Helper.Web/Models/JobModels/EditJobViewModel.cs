namespace Helper.Web.Models.JobModels;

public class EditJobViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Location { get; set; } = null!;
    public int CategoryId { get; set; }
}

