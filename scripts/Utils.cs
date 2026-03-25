using Godot;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

    public static (Dictionary<Vector2I, int>, Dictionary<Vector2I, Vector2I>) WalkableCoordsDistAndPrev(Vector2I source, int movementDist, Combatant.Team sourceTeam)
    {
        Dictionary<Vector2I, int> dist = [];
        Dictionary<Vector2I, Vector2I> prev = [];
        PriorityQueue<Vector2I, int> queue = new();

        dist[source] = 0;
        queue.Enqueue(source, 0);

        while (queue.Count > 0)
        {
            Vector2I current = queue.Dequeue();
            List<Vector2I> neighbors = GetValidNeighbors(current, sourceTeam);
            foreach (Vector2I neighbor in neighbors)
            {
                int altDist = dist[current] + MovementRequired(current, neighbor);
                int neighborDist = dist.GetValueOrDefault(neighbor, int.MaxValue);
                if (altDist < neighborDist && altDist <= movementDist)
                {
                    dist[neighbor] = altDist;
                    prev[neighbor] = current;
                    queue.Enqueue(neighbor, altDist);
                }
            }
        }
        RemoveOccupiedCoords(dist);
        return (dist, prev);
    }

    // Returns a tuple with a distance Dictionary and a previous cell Dictionary.
    // If prev[i] == Vector2I(int.MaxValue, int.MaxValue) it is invalid.
    // TODO replace above with an Option type?
    public static (Dictionary<Vector2I, int>, Dictionary<Vector2I, Vector2I>) Dijkstra(List<Vector2I> graph, Vector2I source)
    {
        Dictionary<Vector2I, int> dist = [];
        Dictionary<Vector2I, Vector2I> prev = [];
        Dictionary<Vector2I, int> queue = [];

        foreach (Vector2I coords in graph)
        {
            prev[coords] = new(int.MaxValue, int.MaxValue);
            if (coords == source)
            {
                dist[coords] = 0;
                queue[coords] = 0;
            }
            else
            {
                dist[coords] = int.MaxValue;
                queue[coords] = int.MaxValue;
            }
        }

        while (queue.Count > 0)
        {
            Vector2I current = queue.First().Key;
            int currentMin = int.MaxValue;
            foreach (KeyValuePair<Vector2I, int> kvp in queue)
            {
                if (kvp.Value <= currentMin)
                {
                    current = kvp.Key;
                    currentMin = kvp.Value;
                }
            }
            queue.Remove(current);
            List<Vector2I> neighbors = GetNeighborsInQueue(current, queue);
            foreach (Vector2I neighbor in neighbors)
            {
                int altDist = dist[current] + MovementRequired(current, neighbor);
                if (altDist < dist[neighbor])
                {
                    dist[neighbor] = altDist;
                    prev[neighbor] = current;
                    queue[neighbor] = altDist;
                }
            }
        }
        return (dist, prev);
    }

    public static List<Vector2I> TraversableCoords(List<Vector2I> coordsList)
    {
        List<Vector2I> result = [];
        TileMapLayer tileMapLayer = BattleManager.Get().TileMapLayer;
        foreach (Vector2I coords in coordsList)
        {
            if (tileMapLayer.GetCellSourceId(coords) != -1)
            {
                TileData tileData = tileMapLayer.GetCellTileData(coords);
                bool isTileTraversable;
                string customDataLayerName = "Traversable";
                Debug.Assert(tileData.HasCustomData(customDataLayerName));
                if (tileData.HasCustomData(customDataLayerName))
                {
                    isTileTraversable = tileData.GetCustomData(customDataLayerName).AsBool();
                    if (isTileTraversable)
                    {
                        result.Add(coords);
                    }
                }
            }
        }
        return result;
    }

    private static List<Vector2I> GetNeighbors(Vector2I current)
    {
        List<Vector2I> result = [
            new(current.X - 1, current.Y),
            new(current.X + 1, current.Y),
            new(current.X, current.Y - 1),
            new(current.X, current.Y + 1)
        ];
        return result;
    }

    private static List<Vector2I> GetValidNeighbors(Vector2I current, Combatant.Team sourceTeam)
    {
        List<Vector2I> result = [];
        BattleManager battleManager = BattleManager.Get();
        TileMapLayer tileMapLayer = battleManager.TileMapLayer;
        List<Vector2I> neighbors = GetNeighbors(current);
        foreach (Vector2I neighbor in neighbors)
        {
            if (tileMapLayer.GetCellSourceId(neighbor) != -1)
            {
                TileData tileData = tileMapLayer.GetCellTileData(neighbor);
                string customDataLayerName = "Traversable";
                Debug.Assert(tileData.HasCustomData(customDataLayerName));
                if (tileData.HasCustomData(customDataLayerName))
                {
                    if (tileData.GetCustomData(customDataLayerName).AsBool())
                    {
                        bool occupiedByEnemy = false;
                        foreach (Combatant combatant in battleManager.Combatants)
                        {
                            if (combatant.MyTeam != sourceTeam)
                            {
                                Vector2I combatantCoords = battleManager.TileMapLayer.LocalToMap(combatant.Position);
                                if (combatantCoords == neighbor)
                                {
                                    occupiedByEnemy = true;
                                    break;
                                }
                            }
                        }
                        if (!occupiedByEnemy)
                        {
                            result.Add(neighbor);
                        }
                    }
                }
            }
        }
        return result;
    }

    private static List<Vector2I> GetNeighborsInQueue(Vector2I current, Dictionary<Vector2I, int> free)
    {
        List<Vector2I> result = [];
        List<Vector2I> neighbors = GetNeighbors(current);
        for (int i = 0; i < neighbors.Count; i++)
        {
            if (free.ContainsKey(neighbors[i]))
            {
                result.Add(neighbors[i]);
            }
        }
        return result;
    }

    private static int MovementRequired(Vector2I start, Vector2I end)
    {
        int xDist = Mathf.Abs(end.X - start.X);
        int yDist = Mathf.Abs(end.Y - start.Y);
        return xDist + yDist;
    }

    private static void RemoveOccupiedCoords(Dictionary<Vector2I, int> source)
    {
        BattleManager battleManager = BattleManager.Get();
        List<Vector2I> occupiedCoords = [];
        foreach (Combatant combatant in battleManager.Combatants)
        {
            Vector2I occupiedCoord = battleManager.TileMapLayer.LocalToMap(combatant.Position);
            occupiedCoords.Add(occupiedCoord);
            source.Remove(occupiedCoord);
        }
    }
}
