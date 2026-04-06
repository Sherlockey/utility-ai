using Godot;

using System;

public partial class CoordsDisplay : PanelContainer
{
    [Export]
    private Label _coordsLabel;

    public override void _Process(double delta)
    {
        Vector2 mousePos = GetViewport().CanvasTransform.AffineInverse() *
            GetViewport().GetMousePosition();
        Vector2I selectedCoords = BattleManager.Get().TileMapLayer.LocalToMap(mousePos);
        string x = selectedCoords.X.ToString("D2");
        string y = selectedCoords.Y.ToString("D2");
        _coordsLabel.Text = "X: " + x + " Y: " + y;
    }
}
