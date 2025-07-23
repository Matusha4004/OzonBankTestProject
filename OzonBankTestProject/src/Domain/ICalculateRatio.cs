using OzonBankTestProject.Infrastructure;

namespace OzonBankTestProject.Domain;

public interface ICalculateRatio
{
    Task<Tuple<double, int>> CalculateRatio(ReportEntity reportEntity);
}