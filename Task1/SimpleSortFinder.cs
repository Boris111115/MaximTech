public class SimpleSortFinder : IDriverFinder
{
    public string AlgorithmName => "Simple Sort";
    
    public List<Driver> FindNearestDrivers(List<Driver> drivers, int targetX, int targetY, int count)
    {
        if (drivers == null || drivers.Count == 0)
            return new List<Driver>();
            
        var sorted = drivers.OrderBy(d => d.DistanceTo(targetX, targetY)).ThenBy(d => d.Id)
                           .Take(count)
                           .ToList();
        return sorted;
    }
}