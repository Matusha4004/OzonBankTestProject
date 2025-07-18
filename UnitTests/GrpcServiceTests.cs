using Microsoft.EntityFrameworkCore;
using Moq;
using OzonBankTestProject.API;
using OzonBankTestProject.Domain;
using OzonBankTestProject.Infrastructure;
using ReportService.Api;
using Xunit;
using Assert = Xunit.Assert;

public class GrpcServiceTests
{
    [Fact]
    public async Task CreateReport_SavesToDb_AndReturnsExpectedValues()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ReportsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var reportsContext = new ReportsContext(options);
        var reportCache = new ReportCache();

        var mockCalculator = new Mock<ICalculateRatio>();
        mockCalculator
            .Setup(c => c.CalculateRatio(It.IsAny<ReportEntity>()))
            .ReturnsAsync(Tuple.Create(0.5, 10));

        var grpcService = new GrpcService(reportsContext, reportCache, mockCalculator.Object);

        var request = new CreateReportRequest
        {
            ProductId = "P1",
            OrderId = "O1",
            PeriodStart = DateTime.UtcNow.AddDays(-1).ToString("O"),
            PeriodEnd = DateTime.UtcNow.ToString("O")
        };

        // Act
        var response = await grpcService.CreateReport(request, null);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(0.5, response.Ratio, 3);
        Assert.Equal(10, response.Payments);

        // Check DB saved
        var dbReport = await reportsContext.Reports.FirstOrDefaultAsync();
        Assert.NotNull(dbReport);
        Assert.Equal("Completed", dbReport.Status);
        Assert.Equal(0.5, dbReport.Ratio);

        // Check cache
        var hashKey = $"{request.ProductId}|{request.OrderId}|{DateTime.Parse(request.PeriodStart):o}|{DateTime.Parse(request.PeriodEnd):o}";
        var cacheExists = reportCache.TryGet(hashKey, out var cachedResult);
        Assert.True(cacheExists);
        Assert.Equal(0.5, cachedResult.Item1);
        Assert.Equal(10, cachedResult.Item2);
    }
}