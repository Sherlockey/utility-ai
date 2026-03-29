using Godot;

using System;
using System.Collections.Generic;

public partial class DebugTileMapLayer : TileMapLayer
{
    private readonly Dictionary<Vector2I, Color> _updatedCells = [];

    public override bool _UseTileDataRuntimeUpdate(Vector2I coords)
    {
        return _updatedCells.ContainsKey(coords);
    }

    public override void _TileDataRuntimeUpdate(Vector2I coords, TileData tileData)
    {
        if (_updatedCells.TryGetValue(coords, out Color targetColor))
        {
            tileData.Modulate = targetColor;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.F1)
            {
                if (!Visible)
                {
                    BattleManager.Get().DrawInfluenceMap();
                }
                Visible = !Visible;
            }
        }
    }

    public void Initialize(TileMapLayer reference)
    {
        foreach (Vector2I coords in reference.GetUsedCellsById(0)) // gets all "background" cells
        {
            SetCell(coords, 0, new(0, 0), 0); // 0 is white-tile
            SetCellModulate(coords, Colors.Black);
        }
    }

    public void SetCellModulate(Vector2I coords, Color color)
    {
        _updatedCells[coords] = color;
        NotifyRuntimeTileDataUpdate();
    }
}
