using Godot;

using System;

using System.Collections.Generic;

public partial class Brain : Node
{
    public static Vector2I ChooseMoveLocation(Vector2I currentCoords, List<Vector2I> possibleCoords)
    {
        possibleCoords.Add(currentCoords);
        Random random = new();
        int randomIndex = random.Next(possibleCoords.Count);
        return possibleCoords[randomIndex];
    }
}
