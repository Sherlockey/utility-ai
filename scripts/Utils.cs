using Godot;

using System.Collections.Generic;

public partial class Utils : Node
{
    public static List<Vector2I> CoordsInDist(Vector2I startCoords, int dist, bool ignoreStartCoords = false)
    {
        List<Vector2I> result = [];
        for (int y = -dist; y <= dist; y++)
        {
            for (int x = Mathf.Abs(y) - dist; x <= Mathf.Abs(Mathf.Abs(y) - dist); x++)
            {
                if (ignoreStartCoords && x == y)
                {
                    continue;
                }
                Vector2I coords = new(startCoords.X + x, startCoords.Y + y);
                result.Add(coords);
                GD.Print(coords);
            }
        }
        return result;
    }

    // Bresenham's line algorithm
    public static List<Vector2I> PlotLine(Vector2I start, Vector2I end)
    {
        if (Mathf.Abs(end.Y - start.Y) < Mathf.Abs(end.X - start.X))
        {
            if (start.X > end.X)
            {
                return PlotLineLow(end.X, end.Y, start.X, start.Y);
            }
            else
            {
                return PlotLineLow(start.X, start.Y, end.X, end.Y);
            }
        }
        else
        {
            if (start.Y > end.Y)
            {
                return PlotLineHigh(end.X, end.Y, start.X, start.Y);
            }
            else
            {
                return PlotLineHigh(start.X, start.Y, end.X, end.Y);
            }
        }
    }

    private static List<Vector2I> PlotLineLow(int startX, int startY, int endX, int endY)
    {
        List<Vector2I> result = [];
        int distX = endX - startX;
        int distY = endY - startY;
        int iY = 1;
        if (distY < 0)
        {
            iY = -1;
            distY = -distY;
        }
        int distance = (2 * distY) - distX;
        int y = startY;
        for (int x = startX; x <= endX; x++)
        {
            result.Add(new Vector2I(x, y));
            if (distance > 0)
            {
                y += iY;
                distance += 2 * (distY - distX);
            }
            else
            {
                distance += 2 * distY;
            }
        }
        return result;
    }

    private static List<Vector2I> PlotLineHigh(int startX, int startY, int endX, int endY)
    {
        List<Vector2I> result = [];
        int distX = endX - startX;
        int distY = endY - startY;
        int iX = 1;
        if (distX < 0)
        {
            iX = -1;
            distX = -distX;
        }
        int distance = (2 * distX) - distY;
        int x = startX;
        for (int y = startY; y <= endY; y++)
        {
            result.Add(new Vector2I(x, y));
            if (distance > 0)
            {
                x += iX;
                distance += 2 * (distX - distY);
            }
            else
            {
                distance += 2 * distX;
            }
        }
        return result;
    }
}
