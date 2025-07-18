using Microsoft.EntityFrameworkCore;

namespace OzonBankTestProject.Infrastructure;

public class InfoContext : DbContext
{
    public InfoContext(DbContextOptions<InfoContext> options) : base(options)
    {
        
    }
    
    public DbSet<InfoEntity> Info { get; set; }
}