using Godot;

using System.Collections.Generic;
using System.Diagnostics;

public partial class Movement : Node
{
    [Export]
    private Combatant _combatant;

    public override void _Input(InputEvent @event)
    {
        if (_combatant.CurrentTurnState == Combatant.TurnState.Waiting)
        {
            return;
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.A)
            {
                TryMove(new Vector2I(-BattleManager.Get().TileSize.X, 0));
            }
            if (keyEvent.Keycode == Key.D)
            {
                TryMove(new Vector2I(BattleManager.Get().TileSize.X, 0));
            }
            if (keyEvent.Keycode == Key.S)
            {
                TryMove(new Vector2I(0, BattleManager.Get().TileSize.Y));
            }
            if (keyEvent.Keycode == Key.W)
            {
                TryMove(new Vector2I(0, -BattleManager.Get().TileSize.Y));
            }
        }
    }

    private bool TryMove(Vector2I movement)
    {
        TileMapLayer tileMapLayer = BattleManager.Get().TileMapLayer;
        Vector2I newTile = tileMapLayer.LocalToMap(_combatant.Position + movement);
        TileData tileData = tileMapLayer.GetCellTileData(newTile);
        bool isTileTraversable = false;
        string customDataLayerName = "Traversable";
        Debug.Assert(tileData.HasCustomData(customDataLayerName));
        if (tileData.HasCustomData(customDataLayerName))
        {
            isTileTraversable = tileData.GetCustomData(customDataLayerName).AsBool();
        }
        if (_combatant.Status.CurrentMovement > 0 && isTileTraversable)
        {
            _combatant.Position = _combatant.Position + movement;
            _combatant.Status.CurrentMovement -= 1;
            return true;
        }
        return false;
    }
}
