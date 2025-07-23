namespace OzonBankTestProject.Infrastructure;

public class InfoEntity
{
    public Guid Id { get; set; }

    public string IdProduct { get; set; } = null!;

    public string IdFigure { get; set; } = null!;

    public int Viewers { get; set; }
    
    public int Sellers { get; set; }
    
    public DateTime Date { get; set; }
}