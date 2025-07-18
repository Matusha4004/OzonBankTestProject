using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using OzonBankTestProject.Domain;
using OzonBankTestProject.Infrastructure;
using ReportService.Api;

namespace OzonBankTestProject.API;

public class GrpcService : Report.ReportBase
{
    private readonly ReportCache _reportCache;
    private readonly ReportsContext _dbContext;
    private readonly ICalculateRatio _calculator;

    public GrpcService(ReportsContext dbContext, ReportCache reportCache, ICalculateRatio calculator)
    {
        _dbContext = dbContext;
        _reportCache = reportCache;
        _calculator = calculator;
    }

    public override async Task<CreateReportResponse> CreateReport(CreateReportRequest request, ServerCallContext context)
    {
        
        string hashKey = $"{request.ProductId}|{request.OrderId}|{DateTime.Parse(request.PeriodStart):o}|{DateTime.Parse(request.PeriodEnd):o}";

        
        if (!_reportCache.TryGet(hashKey, out Tuple<double,int>? reportAnswer))
        {
            var newReport = new ReportEntity
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                OrderId = request.OrderId,
                PeriodStart = DateTime.Parse(request.PeriodStart),
                PeriodEnd = DateTime.Parse(request.PeriodEnd),
                CreatedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            var answer = await _calculator.CalculateRatio(newReport);
            _dbContext.Reports.Add(newReport);
            newReport.Status = "Completed"; 
            newReport.Ratio = answer.Item1;
            await _dbContext.SaveChangesAsync();
            _reportCache.Set(hashKey, answer);
        }

        _reportCache.TryGet(hashKey, out Tuple<double, int>? report);
        if (report == null) throw new NullReferenceException();
        return new CreateReportResponse
        {
            Ratio = report.Item1,
            Payments = report.Item2
        };
    }
}