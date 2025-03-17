using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PhotoWebApp.Models;
using PhotoWebApp.Controllers;
using System.Runtime.Serialization;

namespace PhotoWebApp.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comment { get; set; }

    public virtual DbSet<Photo> Photo { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //If you face the migration errors
        // First: 
        // delete the whole migration folder
        // open package manager console and type remove-migrations too
        // delete the database
        // Second:
        // now run "Add-Migration InitialMigrationCreateTables"
        // and run "Update-Database"
        // it will fix the migration problem

        modelBuilder.Entity<Comment>(entity =>
         {
             entity.HasIndex(e => e.PhotoId, "IX_Comment_PhotoId");

             entity.Property(e => e.commentValue).HasColumnName("commentValue");

             entity.HasOne(d => d.Photo).WithMany(p => p.Comment).HasForeignKey(d => d.PhotoId);
         });

         modelBuilder.Entity<Photo>(entity =>
         {
             entity.Property(e => e.photoId).HasColumnName("photoId");
             entity.Property(e => e.description).HasColumnName("description");
             entity.Property(e => e.photoTitle).HasColumnName("photoTitle");
             entity.Property(e => e.tags).HasColumnName("tags");
             entity.Property(e => e.userId).HasColumnName("userId");
         });

        modelBuilder.Entity<Users>().HasKey(u => u.UserId);

        modelBuilder.Entity<Users>(entity =>
         {
             entity.HasKey(e => e.UserId);
             entity.Property(e => e.Email).HasMaxLength(80);
             entity.Property(e => e.Username).HasMaxLength(15);

             entity.HasMany(u => u.Photos).WithOne(p => p.User).HasForeignKey(p => p.userId).OnDelete(DeleteBehavior.Cascade);
         });
        
        
        modelBuilder.Entity<Users>().HasData(
                new Users { UserId = 1, Username = "admin", Email = "admin@photowebapp.com", Role = (UserRole)1, TokenExpiry = DateTime.Parse("2016-05-02"), Token = "abcdefgh" },
                new Users { UserId = 2, Username = "ccuser", Email = "ccuser@photowebapp.com", Role = (UserRole)2, TokenExpiry = DateTime.Parse("2016-05-02"), Token = "abcdefgh" });

        modelBuilder.Entity<Photo>().HasData(
                new Photo { photoId = 1, photoTitle = "Photo 1", description = "description 1", userId = 2, tags = "#tag1", CommentMode = true, LikesCount = 200, IsPublic = true },
                new Photo { photoId = 2, photoTitle = "Photo 2", description = "description 2", userId = 2, tags = "#tag2", CommentMode = false, LikesCount = 10, IsPublic = false }
         );

        modelBuilder.Entity<Comment>().HasData(
                new Comment { CommentId = 1, PhotoId = 1, commentValue = "Comment1", Flagged = true, DatePosted = DateTime.Parse("2016-05-02") },
                new Comment { CommentId = 2, PhotoId = 2, commentValue = "Comment2", Flagged = false, DatePosted = DateTime.Parse("2016-05-02") }
         );
        


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
