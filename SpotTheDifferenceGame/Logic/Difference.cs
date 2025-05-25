public class Difference
{
    public int CenterX { get; set; }
    public int CenterY { get; set; }
    public int Radius { get; set; } = 15;

    public bool IsNear(int x, int y)
    {
        double distance = Math.Sqrt(Math.Pow(x - CenterX, 2) + Math.Pow(y - CenterY, 2));
        return distance <= Radius;
    }
}
