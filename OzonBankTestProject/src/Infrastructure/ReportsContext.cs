using Microsoft.EntityFrameworkCore;

namespace OzonBankTestProject.Infrastructure;

public class ReportsContext : DbContext
{
    public ReportsContext(DbContextOptions<ReportsContext> options) : base(options)
    {
        
    }
    
    public DbSet<ReportEntity> Reports { get; set; }
}