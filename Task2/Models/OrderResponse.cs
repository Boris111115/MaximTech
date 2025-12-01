namespace Task2.Models;
public class OrderResponse
{
    public int DriverId { get; set; }
    public int DriverX { get; set; }
    public int DriverY { get; set; }
    public double RouteLength { get; set; }
    public List<Coordinate> Route { get; set; } = new();
}

public class Coordinate
{
    public int X { get; set; }
    public int Y { get; set; }
    
    public Coordinate(int x, int y)
    {
        X = x;
        Y = y;
    }
}