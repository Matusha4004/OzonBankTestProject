namespace OzonBankTestProject.Infrastructure;

public class ReportEntity
{
    public Guid Id { get; set; }
    public string ProductId { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string Status { get; set; } = null!;
    
    public double Ratio  { get; set; }
}