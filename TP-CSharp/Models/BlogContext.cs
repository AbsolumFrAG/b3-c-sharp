using Microsoft.EntityFrameworkCore;

namespace TP_CSharp.Models
{
    public class BlogContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public BlogContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));
        }

        public DbSet<Blog> Blogs { get; set; }
    }
}
