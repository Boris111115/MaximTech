using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;

[MemoryDiagnoser]
[RankColumn]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[Config(typeof(Config))]
public class DriverFinderBenchmarks
{
    private List<Driver> drivers;
    private List<IDriverFinder> finders;
    
    [Params(100, 1000, 5000)]
    public int DriverCount { get; set; }
    
    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        drivers = new List<Driver>();
        
        for (int i = 0; i < DriverCount; i++)
        {
            drivers.Add(new Driver(i, random.Next(0, 1000), random.Next(0, 1000)));
        }
        
        finders = new List<IDriverFinder>
        {
            new SimpleSortFinder(),
            new PriorityQueueFinder(),
            new QuickSelectFinder(),
            new AreaSearchFinder()
        };
    }
    
    [Benchmark(Baseline = true)]
    public void SimpleSort() => RunBenchmark(finders[0]);
    
    [Benchmark]
    public void PriorityQueue() => RunBenchmark(finders[1]);
    
    [Benchmark]
    public void QuickSelect() => RunBenchmark(finders[2]);
    
    [Benchmark]
    public void AreaSearch() => RunBenchmark(finders[3]);
    
    private void RunBenchmark(IDriverFinder finder)
    {
        var result = finder.FindNearestDrivers(drivers, 500, 500, 5);
        if (result.Count == 0) throw new InvalidOperationException("No results");
    }
    private class Config : ManualConfig
    {
        public Config()
        {
            WithOptions(ConfigOptions.DisableOptimizationsValidator);
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        
        var summary = BenchmarkRunner.Run<DriverFinderBenchmarks>();
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}