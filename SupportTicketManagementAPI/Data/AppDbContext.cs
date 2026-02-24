using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

    public DbSet<User> Users {get; set;}
    public DbSet<Role> Roles {get; set;}
    public DbSet<Ticket> Tickets{get; set;}

    public DbSet<TicketComment> TicketComments{get; set;}
    public DbSet<TicketStatusLog> TicketStatusLogs{get; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().Property(r => r.Name).HasConversion<string>();
        modelBuilder.Entity<Ticket>().Property(t => t.Status).HasConversion<string>();
        modelBuilder.Entity<Ticket>().Property(t => t.Priority).HasConversion<string>();

        modelBuilder.Entity<TicketComment>().HasOne(tc => tc.Ticket)
        .WithMany().HasForeignKey(tc => tc.TicketId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TicketStatusLog>().HasOne(ts => ts.Ticket)
        .WithMany().HasForeignKey(ts => ts.TicketId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Ticket>().HasOne(t => t.CreatingUser).WithMany().
        HasForeignKey(t => t.CreatedBy).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Ticket>().HasOne(t => t.AssignedUser).WithMany().
        HasForeignKey(t => t.AssignedTo).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketStatusLog>().HasOne(ts => ts.User).WithMany().HasForeignKey(ts => ts.ChangedBy).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TicketComment>().HasOne(tc => tc.User).WithMany().HasForeignKey(tc => tc.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Role>().HasData(
            new Role { Id=1, Name=RoleTypes.MANAGER},
            new Role { Id=2, Name=RoleTypes.SUPPORT},
            new Role { Id=3, Name=RoleTypes.USER}
        );
    }
}