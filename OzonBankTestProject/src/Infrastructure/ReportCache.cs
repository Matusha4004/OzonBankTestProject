using System.Collections.Concurrent;

namespace OzonBankTestProject.Infrastructure;

public class ReportCache
{
    private readonly ConcurrentDictionary<string, Tuple<double,int>> _cache = new();

    public bool TryGet(string key, out Tuple<double,int>? reportAnswer) => _cache.TryGetValue(key, out reportAnswer);

    public void Set(string key, Tuple<double,int> reportAnswer) => _cache[key] = reportAnswer;
}
