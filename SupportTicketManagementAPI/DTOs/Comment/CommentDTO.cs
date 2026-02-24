using System.ComponentModel.DataAnnotations;

public class CommentDTO
{
    [Required]
    [MinLength(1)]
    public string comment{get; set;} = null!;
}