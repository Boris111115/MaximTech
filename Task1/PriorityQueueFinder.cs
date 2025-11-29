public class PriorityQueueFinder : IDriverFinder
{
    public string AlgorithmName => "Priority Queue";
    
    public List<Driver> FindNearestDrivers(List<Driver> drivers, int targetX, int targetY, int count)
    {
        if (drivers == null || drivers.Count == 0)
            return new List<Driver>();
            
        var pq = new PriorityQueue<Driver, (double Distance, int Id)>();
        
        foreach (var driver in drivers)
        {
            double distance = driver.DistanceTo(targetX, targetY);
            pq.Enqueue(driver, (distance, driver.Id));
        }
        
        var result = new List<Driver>();
        int takeCount = Math.Min(count, drivers.Count);
        for (int i = 0; i < takeCount; i++)
        {
            result.Add(pq.Dequeue());
        }
        
        return result;
    }
}