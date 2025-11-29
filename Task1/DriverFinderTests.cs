using NUnit.Framework;
using System.Text;

[TestFixture]
public class DriverFinderTests
{
    private List<Driver> testDrivers;
    private List<IDriverFinder> finders;
    
    [SetUp]
    public void Setup()
    {
        testDrivers = new List<Driver>
        {
            new Driver(1, 0, 0),
            new Driver(2, 3, 4),
            new Driver(3, 1, 1),
            new Driver(4, 5, 5),
            new Driver(5, 2, 2),
            new Driver(6, 10, 10),
            new Driver(7, 1, 0),
            new Driver(8, 0, 1),
            new Driver(9, 4, 3),
            new Driver(10, 2, 3)
        };
        
        finders = new List<IDriverFinder>
        {
            new SimpleSortFinder(),
            new PriorityQueueFinder(),
            new QuickSelectFinder(),
            new AreaSearchFinder()
        };
    }

    private void PrintResults(string testName, List<Driver> result, IDriverFinder finder, int targetX, int targetY)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"\n{testName} - {finder.AlgorithmName}:");
        sb.AppendLine($"Target: ({targetX}, {targetY})");
        sb.AppendLine($"Found {result.Count} drivers:");
        
        foreach (var driver in result)
        {
            double distance = driver.DistanceTo(targetX, targetY);
            sb.AppendLine($"  Driver {driver.Id} at ({driver.X}, {driver.Y}) - Distance: {distance:F2}");
        }
        
        TestContext.WriteLine(sb.ToString());
    }
    
    [Test]
    public void FindNearestDrivers_ShouldReturnCorrectCount()
    {
        foreach (var finder in finders)
        {
            var result = finder.FindNearestDrivers(testDrivers, 0, 0, 3);
            PrintResults("Correct Count Test", result, finder, 0, 0);
            Assert.AreEqual(3, result.Count, $"{finder.AlgorithmName} returned wrong count");
        }
    }
    
    [Test]
    public void FindNearestDrivers_ShouldReturnCorrectOrder()
    {
        foreach (var finder in finders)
        {
            var result = finder.FindNearestDrivers(testDrivers, 0, 0, 4);
            PrintResults("Correct Order Test", result, finder, 0, 0);
            
            Assert.AreEqual(1, result[0].Id, $"{finder.AlgorithmName} wrong first driver");
            Assert.AreEqual(7, result[1].Id, $"{finder.AlgorithmName} wrong second driver");
            Assert.AreEqual(8, result[2].Id, $"{finder.AlgorithmName} wrong third driver");
            Assert.AreEqual(3, result[3].Id, $"{finder.AlgorithmName} wrong fourth driver");
        }
    }
    
    [Test]
    public void FindNearestDrivers_WithEmptyList_ShouldReturnEmpty()
    {
        foreach (var finder in finders)
        {
            var result = finder.FindNearestDrivers(new List<Driver>(), 0, 0, 5);
            PrintResults("Empty List Test", result, finder, 0, 0);
            Assert.AreEqual(0, result.Count, $"{finder.AlgorithmName} should return empty list");
        }
    }
    
    [Test]
    public void FindNearestDrivers_RequestMoreThanAvailable_ShouldReturnAll()
    {
        foreach (var finder in finders)
        {
            var smallList = testDrivers.Take(3).ToList();
            var result = finder.FindNearestDrivers(smallList, 0, 0, 10);
            PrintResults("More Than Available Test", result, finder, 0, 0);
            Assert.AreEqual(3, result.Count, $"{finder.AlgorithmName} should return all available drivers");
        }
    }
    
    [Test]
    public void AllAlgorithms_ShouldReturnSameResults()
    {
        var targetX = 2;
        var targetY = 2;
        var count = 6;
        
        var results = new List<List<Driver>>();
        
        foreach (var finder in finders)
        {
            var result = finder.FindNearestDrivers(testDrivers, targetX, targetY, count);
            results.Add(result);
            PrintResults("All Algorithms Same Results", result, finder, targetX, targetY);
        }
        
        for (int i = 1; i < results.Count; i++)
        {
            CollectionAssert.AreEqual(
                results[0].Select(d => d.Id),
                results[i].Select(d => d.Id),
                $"{finders[0].AlgorithmName} and {finders[i].AlgorithmName} returned different results"
            );
        }
    }
    
    [Test]
    public void FindNearestDrivers_EdgeCase_SingleDriver()
    {
        foreach (var finder in finders)
        {
            var singleDriverList = new List<Driver> { new Driver(1, 5, 5) };
            var result = finder.FindNearestDrivers(singleDriverList, 0, 0, 3);
            PrintResults("Single Driver Test", result, finder, 0, 0);
            
            Assert.AreEqual(1, result.Count, $"{finder.AlgorithmName} should return single driver");
            Assert.AreEqual(1, result[0].Id, $"{finder.AlgorithmName} wrong driver ID");
        }
    }
    
    [Test]
    public void FindNearestDrivers_EdgeCase_SameCoordinates()
    {
        var sameCoordDrivers = new List<Driver>
        {
            new Driver(1, 3, 3),
            new Driver(2, 3, 3),
            new Driver(3, 3, 3),
            new Driver(4, 2, 2)
        };
        
        foreach (var finder in finders)
        {
            var result = finder.FindNearestDrivers(sameCoordDrivers, 3, 3, 3);
            PrintResults("Same Coordinates Test", result, finder, 3, 3);
            
            Assert.AreEqual(3, result.Count, $"{finder.AlgorithmName} wrong count for same coordinates");
            Assert.IsTrue(result.All(d => d.X == 3 && d.Y == 3), 
                $"{finder.AlgorithmName} should return drivers with same coordinates first");
        }
    }
    
    [Test]
    public void FindNearestDrivers_PerformanceTest_LargeDataset()
    {
        var largeDrivers = new List<Driver>();
        var random = new Random(42);
        
        for (int i = 1; i <= 100; i++)
        {
            largeDrivers.Add(new Driver(i, random.Next(0, 100), random.Next(0, 100)));
        }
        
        foreach (var finder in finders)
        {
            var result = finder.FindNearestDrivers(largeDrivers, 50, 50, 10);
            PrintResults("Large Dataset Test", result, finder, 50, 50);
            
            Assert.AreEqual(10, result.Count, $"{finder.AlgorithmName} wrong count for large dataset");
            Assert.IsTrue(result.All(d => d != null), $"{finder.AlgorithmName} returned null drivers");
        }
    }
    
    [Test]
    public void FindNearestDrivers_DifferentTargetPoints()
    {
        var testPoints = new[] { (0, 0), (5, 5), (10, 10), (2, 3), (7, 1) };
        
        foreach (var (targetX, targetY) in testPoints)
        {
            TestContext.WriteLine($"\n=== Testing target point ({targetX}, {targetY}) ===");
            
            var results = new List<List<Driver>>();
            foreach (var finder in finders)
            {
                var result = finder.FindNearestDrivers(testDrivers, targetX, targetY, 4);
                results.Add(result);
                PrintResults($"Target Point ({targetX}, {targetY})", result, finder, targetX, targetY);
            }
            
            for (int i = 1; i < results.Count; i++)
            {
                CollectionAssert.AreEqual(
                    results[0].Select(d => d.Id),
                    results[i].Select(d => d.Id),
                    $"{finders[0].AlgorithmName} and {finders[i].AlgorithmName} returned different results for point ({targetX}, {targetY})"
                );
            }
        }
    }
    
    [Test]
    public void FindNearestDrivers_ZeroCount_ShouldReturnEmpty()
    {
        foreach (var finder in finders)
        {
            var result = finder.FindNearestDrivers(testDrivers, 0, 0, 0);
            PrintResults("Zero Count Test", result, finder, 0, 0);
            Assert.AreEqual(0, result.Count, $"{finder.AlgorithmName} should return empty list for count 0");
        }
    }
}