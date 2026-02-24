using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[Index(nameof(Email),IsUnique = true)]
public class User
{
    public int Id {get; set;}
    
    [Required]
    [MaxLength(255)]
    public string Name {get; set;} = null!;

    [Required]
    [MaxLength(255)]
    public string Email {get; set;} = null!;

    [Required]
    public string Password {get; set;} = null!;

    [Required]
    public int RoleId {get; set;}

    public Role Role{get; set;} = null!;
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
}