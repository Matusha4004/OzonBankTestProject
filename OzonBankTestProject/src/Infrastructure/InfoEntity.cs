namespace OzonBankTestProject.Infrastructure;

public class InfoEntity
{
    public Guid Id { get; set; }
    
    public string IdProduct { get; set; }
    
    public string IdFigure { get; set; }

    public int Viewers { get; set; }
    
    public int Sellers { get; set; }
    
    public DateTime Date { get; set; }
}