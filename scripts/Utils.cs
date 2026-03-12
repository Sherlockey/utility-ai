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
}
