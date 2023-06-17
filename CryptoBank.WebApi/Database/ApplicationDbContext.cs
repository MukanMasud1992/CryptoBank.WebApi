﻿using CryptoBank.WebApi.Features.Users.Domain;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.WebApi.Database;

public class ApplicationDbContext:DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
  
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        MapUsers(modelBuilder);
        MapRoles(modelBuilder);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    private void MapUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(x => x.Id);

            user.Property(u => u.Email)
                .IsRequired();

            user.Property(x => x.PasswordHashAndSalt)
                .IsRequired();

            user.Property(x => x.MemorySize)
                .IsRequired();

            user.Property(x => x.Parallelism)
                .IsRequired();

            user.Property(x => x.Iterations)
                .IsRequired();

            user.Property(x => x.BirthDate)
                .IsRequired();

            user.Property(x => x.CreatedAt)
                .IsRequired();

            user.Property(x => x.UpdatedAt);
        });
    }

    private void MapRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(role =>
        {
            role.HasKey(x => x.Id);

            role.Property(x => x.UserId)
                .IsRequired();

            role.Property(x => x.Name)
                .IsRequired();

            role.Property(x => x.CreatedAt)
                .IsRequired();

            role.HasOne(r => r.User)
                .WithMany(u => u.Roles)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}