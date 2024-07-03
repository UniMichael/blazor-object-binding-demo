using System.ComponentModel.DataAnnotations;

namespace BlazorComplexObjectBinding.Models;

public class Project
{
    [Required(AllowEmptyStrings = false), MaxLength(10)]
    public string Name { get; set; }
    
    public List<Board> Boards { get; set; }

    public Project(string name, List<Board> boards)
    {
        Name = name;
        Boards = boards;
    }
}
