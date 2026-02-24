using System.ComponentModel.DataAnnotations;

public class CreateUserDto
{
    [Required]
    [MaxLength(255)]
    public string name {get; set;} = null!;

    [Required]
    [EmailAddress]
    public string email {get; set;} = null!;

    [Required]
    [MinLength(6)]
    public string password {get; set;} = null!;

    [Required]
    public RoleTypes role {get; set;}
}