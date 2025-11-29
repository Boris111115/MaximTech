public class AreaSearchFinder : IDriverFinder
{
    public string AlgorithmName => "Area Search";
    
    public List<Driver> FindNearestDrivers(List<Driver> drivers, int targetX, int targetY, int count)
    {
        if (drivers == null || drivers.Count == 0)
            return new List<Driver>();
            
        var result = new List<Driver>();
        int maxRadius = Math.Max(targetX, Math.Max(targetY, 
                          Math.Max(drivers.Max(d => d.X), drivers.Max(d => d.Y))));
        
        for (int radius = 0; radius <= maxRadius && result.Count < count; radius++)
        {
            var driversInRadius = drivers
                .Where(d => Math.Abs(d.X - targetX) <= radius && 
                           Math.Abs(d.Y - targetY) <= radius)
                .Where(d => !result.Contains(d))
                .Select(d => new { Driver = d, Distance = d.DistanceTo(targetX, targetY) })
                .OrderBy(x => x.Distance)
                .ThenBy(x => x.Driver.Id)
                .Take(count - result.Count)
                .Select(x => x.Driver);
                
            result.AddRange(driversInRadius);
        }
        
        return result.Take(count).ToList();
    }
}