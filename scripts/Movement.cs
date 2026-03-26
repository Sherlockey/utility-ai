using Godot;

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class Movement : Node
{
    public event EventHandler<Combatant> Moved;

    [Export]
    private Combatant _combatant;
    [Export]
    private Timer _walkWaitTimer;

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

    public async Task WalkTo(Vector2I endCoords, Dictionary<Vector2I, Vector2I> prev)
    {
        TileMapLayer tileMapLayer = BattleManager.Get().TileMapLayer;
        Vector2I startCoords = tileMapLayer.LocalToMap(_combatant.Position);
        List<Vector2I> steps = [];
        Vector2I coords = endCoords;
        while (coords != startCoords)
        {
            steps.Add(coords);
            coords = prev[coords];
        }
        steps.Reverse();
        foreach (Vector2I step in steps)
        {
            Vector2 position = tileMapLayer.MapToLocal(step);
            _combatant.Position = position;
            Moved?.Invoke(this, _combatant);
            _combatant.Status.CurrentMovement -= 1;
            _walkWaitTimer.Start();
            await ToSignal(_walkWaitTimer, Timer.SignalName.Timeout);
        }
    }
}
