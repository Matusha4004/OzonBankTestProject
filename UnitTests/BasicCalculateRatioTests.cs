using Microsoft.EntityFrameworkCore;
using OzonBankTestProject.Domain;
using OzonBankTestProject.Infrastructure;
using Xunit;
using Assert = Xunit.Assert;

namespace UnitTests
{
    public class BasicCalculateRatioTests
    {
        private InfoContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<InfoContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new InfoContext(options);
        }

        [Fact]
        public async Task CalculateRatio_ReturnsZero_WhenNoData()
        {
            // Arrange
            var context = CreateInMemoryContext("TestDb1");
            var service = new BasicCalculateRatio(context);

            var report = new ReportEntity
            {
                ProductId = "P1",
                OrderId = "O1",
                PeriodStart = DateTime.UtcNow.AddDays(-1),
                PeriodEnd = DateTime.UtcNow
            };

            // Act
            var result = await service.CalculateRatio(report);

            // Assert
            Assert.Equal(0d, result.Item1);
            Assert.Equal(0, result.Item2);
        }

        [Fact]
        public async Task CalculateRatio_ReturnsCorrectRatio_WhenDataExists()
        {
            // Arrange
            var context = CreateInMemoryContext("TestDb2");

            context.Info.Add(new InfoEntity
            {
                Id = Guid.NewGuid(),
                IdProduct = "P1",
                IdFigure = "O1",
                Viewers = 100,
                Sellers = 20,
                Date = DateTime.UtcNow.AddMinutes(-30)
            });

            await context.SaveChangesAsync();

            var service = new BasicCalculateRatio(context);

            var report = new ReportEntity
            {
                ProductId = "P1",
                OrderId = "O1",
                PeriodStart = DateTime.UtcNow.AddHours(-1),
                PeriodEnd = DateTime.UtcNow
            };

            // Act
            var result = await service.CalculateRatio(report);

            // Assert
            Assert.Equal(0.2d, result.Item1); // 20 / 100
            Assert.Equal(20, result.Item2);
        }

        [Fact]
        public async Task CalculateRatio_IgnoresDataOutsidePeriod()
        {
            // Arrange
            var context = CreateInMemoryContext("TestDb3");

            context.Info.Add(new InfoEntity
            {
                Id = Guid.NewGuid(),
                IdProduct = "P1",
                IdFigure = "O1",
                Viewers = 50,
                Sellers = 10,
                Date = DateTime.UtcNow.AddHours(-5) // вне периода
            });

            await context.SaveChangesAsync();

            var service = new BasicCalculateRatio(context);

            var report = new ReportEntity
            {
                ProductId = "P1",
                OrderId = "O1",
                PeriodStart = DateTime.UtcNow.AddHours(-1),
                PeriodEnd = DateTime.UtcNow
            };

            // Act
            var result = await service.CalculateRatio(report);

            // Assert
            Assert.Equal(0d, result.Item1);
            Assert.Equal(0, result.Item2);
        }
    }
}
