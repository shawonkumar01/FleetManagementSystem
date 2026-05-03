using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets will go here later (Vehicles, Drivers, etc.)
    }
}