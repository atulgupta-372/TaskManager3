using Microsoft.EntityFrameworkCore;
using TaskManager3.Models;

namespace TaskManager3.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Board> Boards { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
    }
}
