using Microsoft.EntityFrameworkCore;
using ChatChallenge.Core.Entities;

namespace ChatChallenge.Infrastructure.Data;

public class ChatDbContext : DbContext
{
  public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
  {
  }

  public DbSet<ChatMessage> ChatMessages { get; set; }
  public DbSet<ChatRoom> ChatRooms { get; set; }
  public DbSet<User> Users { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<ChatMessage>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
      entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
      entity.Property(e => e.CreatedAt).IsRequired();
      entity.Property(e => e.IsStockBot).HasDefaultValue(false);
      
      entity.HasOne(e => e.ChatRoom)
        .WithMany(r => r.Messages)
        .HasForeignKey(e => e.ChatRoomId)
        .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<ChatRoom>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
      entity.Property(e => e.CreatedAt).IsRequired();
      entity.HasIndex(e => e.Name).IsUnique();
    });

    modelBuilder.Entity<User>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
      entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
      entity.Property(e => e.CreatedAt).IsRequired();
      entity.HasIndex(e => e.Email).IsUnique();
      entity.HasIndex(e => e.UserName).IsUnique();
    });

    base.OnModelCreating(modelBuilder);
  }
}
