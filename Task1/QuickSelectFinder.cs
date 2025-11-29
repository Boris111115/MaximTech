public class QuickSelectFinder : IDriverFinder
{
    public string AlgorithmName => "QuickSelect";
    
    public List<Driver> FindNearestDrivers(List<Driver> drivers, int targetX, int targetY, int count)
    {
        if (drivers == null || drivers.Count == 0)
            return new List<Driver>();
            
        count = Math.Min(count, drivers.Count);
        var distances = drivers.Select(d => new 
        { 
            Driver = d, 
            Distance = d.DistanceTo(targetX, targetY),
            Id = d.Id
        }).ToArray();
        
        QuickSelect(distances, 0, distances.Length - 1, count - 1);
        
        return distances.Take(count)
                       .OrderBy(x => x.Distance)
                       .ThenBy(x => x.Id)
                       .Select(x => x.Driver)
                       .ToList();
    }
    
    private void QuickSelect(dynamic[] items, int left, int right, int k)
    {
        while (left < right)
        {
            int pivotIndex = Partition(items, left, right);
            
            if (pivotIndex == k)
                return;
            else if (pivotIndex < k)
                left = pivotIndex + 1;
            else
                right = pivotIndex - 1;
        }
    }
    
    private int Partition(dynamic[] items, int left, int right)
    {
        var pivotItem = items[right];
        double pivotDistance = pivotItem.Distance;
        int pivotId = pivotItem.Id;
        int i = left;
        
        for (int j = left; j < right; j++)
        {
            if (items[j].Distance < pivotDistance || 
                (items[j].Distance == pivotDistance && items[j].Id <= pivotId))
            {
                Swap(items, i, j);
                i++;
            }
        }
        
        Swap(items, i, right);
        return i;
    }
    
    private void Swap(dynamic[] items, int i, int j)
    {
        var temp = items[i];
        items[i] = items[j];
        items[j] = temp;
    }
}