public interface IDriverFinder
{
    List<Driver> FindNearestDrivers(List<Driver> drivers, int targetX, int targetY, int count);
    string AlgorithmName { get; }
}