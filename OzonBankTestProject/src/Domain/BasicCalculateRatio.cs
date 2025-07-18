using Microsoft.EntityFrameworkCore;
using OzonBankTestProject.Infrastructure;

namespace OzonBankTestProject.Domain;

public class BasicCalculateRatio : ICalculateRatio
{
    
    private readonly InfoContext _infoContext;

    public BasicCalculateRatio(InfoContext context)
    {
        _infoContext = context;
    }
    public async Task<Tuple<double, int>?> CalculateRatio(ReportEntity reportEntity)
    {
        
        
        var start = reportEntity.PeriodStart.ToUniversalTime();
        var end   = reportEntity.PeriodEnd.ToUniversalTime();

        var data = await _infoContext.Info
            .Where(i => i.IdProduct == reportEntity.ProductId &&
                        i.IdFigure == reportEntity.OrderId &&
                        i.Date >= start &&
                        i.Date < end)
            .ToListAsync();

        if (!data.Any())
            return Tuple.Create(0d, 0);

        int totalViewers = data.Sum(d => d.Viewers);
        int totalPayments = data.Sum(d => d.Sellers);

        double ratio = totalViewers == 0 ? 0d : (double)totalPayments / totalViewers;

        return Tuple.Create(ratio, totalPayments);
    }
}