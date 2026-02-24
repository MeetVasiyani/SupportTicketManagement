using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public enum RoleTypes
{
    MANAGER,SUPPORT,USER
}
[Index(nameof(Name),IsUnique = true)]
public class Role
{
    public int Id {get; set;}

    [Required]
    public RoleTypes Name {get; set;}

    public ICollection<User> Users{get; set;} = new List<User>();
}