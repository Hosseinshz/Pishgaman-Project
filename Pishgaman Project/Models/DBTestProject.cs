using Microsoft.EntityFrameworkCore;

namespace Pishgaman_Project.Models
{
    public class DBTestProject : DbContext
    {
        public DBTestProject(DbContextOptions<DBTestProject> options) : base(options)
        {
        }

        public DbSet<UserInfo> UserInfo { get; set; }
    }
}
