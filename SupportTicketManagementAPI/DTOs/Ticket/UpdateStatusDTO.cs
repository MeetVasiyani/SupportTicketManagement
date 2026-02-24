using System.ComponentModel.DataAnnotations;

public class UpdateStatusDTO
{
    [Required]
    public TicketsStatus status {get; set;}
}