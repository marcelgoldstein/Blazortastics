using Blazortastics.DB.Tables;
using Microsoft.EntityFrameworkCore;

namespace Blazortastics.DB
{
    public class BlazorDbContext : DbContext
    {
        public BlazorDbContext(DbContextOptions<BlazorDbContext> options) : base(options)
        {

        }

        public DbSet<Ranking> Rankings { get; set; }
    }
}
