using Godot;

using System.Collections.Generic;

public partial class TurnOrderDisplay : VBoxContainer
{
    [Export]
    private VBoxContainer _turnOrderEntryVBox;
    [Export]
    private PackedScene _turnOrderEntryScene;

    private const int MaxEntries = 9;

    public void Update(List<Combatant> combatants)
    {
        // Remove old entires from TurnOrderVBox
        foreach (Node child in _turnOrderEntryVBox.GetChildren())
        {
            _turnOrderEntryVBox.RemoveChild(child);
            child.QueueFree();
        }
        // Repopulate TurnOrderVBox
        int i = 1;
        foreach (Combatant combatant in combatants)
        {
            // TODO decouple the health bar from the turn order
            TurnOrderEntry turnOrderEntry = _turnOrderEntryScene.Instantiate<TurnOrderEntry>();
            turnOrderEntry.Label.Text = i.ToString();
            turnOrderEntry.TextureRect.Texture = combatant.Sprite2D.Texture;
            turnOrderEntry.HealthBar.TextureProgressBar.MaxValue = combatant.Stats.Health;
            turnOrderEntry.HealthBar.TextureProgressBar.Value = combatant.Status.CurrentHealth;
            _turnOrderEntryVBox.AddChild(turnOrderEntry);
            i++;
            if (i > MaxEntries)
            {
                break;
            }
        }
    }
}
