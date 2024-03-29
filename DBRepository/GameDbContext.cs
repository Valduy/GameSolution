﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;

namespace Context
{
    public sealed class GameDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Friendship> FriendLists { get; set; }

        public GameDbContext(DbContextOptions<GameDbContext> options) 
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(UserConfigure);
            modelBuilder.Entity<Friendship>(FriendshipConfigure);
        }

        public void UserConfigure(EntityTypeBuilder<User> builder)
        {
            builder.HasAlternateKey(u => u.Login);
            builder.Property(u => u.Login).IsRequired().HasMaxLength(20);
            builder.Property(u => u.Password).IsRequired();
        }

        public void FriendshipConfigure(EntityTypeBuilder<Friendship> builder)
        {
            builder
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(f => f.Friend)
                .WithMany(u => u.FriendList)
                .HasForeignKey(f => f.FriendId);

            builder.HasKey(f => new {f.UserId, f.FriendId});
        }
    }
}
