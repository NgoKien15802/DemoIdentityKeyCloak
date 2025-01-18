using DemoIdentity.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace DemoIdentity.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Tự động áp dụng các cấu hình thực thể từ các lớp triển khai IEntityTypeConfiguration<TEntity>.
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
