using System;
using System.Linq;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class AppDbContext : DbContext
    {
        // you could login and see db, but that's it, update when db goes down
        private static string ConnectionString =
            "Server=barrel.itcollege.ee;User Id=student;Password=Student.Pass.1;Database=student_jametl_BS;MultipleActiveResultSets=true";
        
        public DbSet<Game> Games { get; set; } = null!;
        public DbSet<GameOption> GameOptions { get; set; } = null!;


        //not recommended - do not hardcore DB conf!
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            

            // remove the cascade delete
            foreach (var relationship in modelBuilder.Model
                .GetEntityTypes()
                .Where(e => !e.IsOwned())
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}