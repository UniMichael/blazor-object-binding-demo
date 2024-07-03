namespace BlazorComplexObjectBinding.Models;

public class WorkItem
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Done { get; set; }

    public WorkItem(Guid id, string name, string description = "", bool done = false)
    {
        Id = id;
        Name = name;
        Description = description;
        Done = done;
    }
}
