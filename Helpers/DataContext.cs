using AuthSystem.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AuthSystem.Helpers
{
    public class DataContext : DbContext
    {
        protected readonly IConfiguration _configuration;

        public DataContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to sql server database
            options.UseSqlServer(_configuration.GetConnectionString("AuthSystemDB"));
        }

        public DbSet<User> Users { get; set; }
    }
}
