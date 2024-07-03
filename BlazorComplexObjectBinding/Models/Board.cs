namespace BlazorComplexObjectBinding.Models;

public class Board
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool Synced { get; set; }
    public List<WorkItem> WorkItems { get; set; }

    public Board(Guid id, string name, bool synced, List<WorkItem> workItems)
    {
        Id = id;
        Name = name;
        Synced = synced;
        WorkItems = workItems;
    }
}
