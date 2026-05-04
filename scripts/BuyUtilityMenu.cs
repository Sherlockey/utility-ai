using Godot;

using System;

using System.Collections.Generic;

public partial class BuyUtilityMenu : PanelContainer
{
    [Export]
    private Label _nameLabel;
    [Export]
    private TextureRect _portrait;
    [Export]
    private Button _combatantLeftButton;
    [Export]
    private Button _combatantRightButton;
    [Export]
    private Control _movementUtilitiesParent;
    [Export]
    private Control _abilityUtilitiesParent;
    [Export]
    private SceneArray _movementUtilities;
    [Export]
    private SceneArray _abilityUtilities;

    private int _partyIndex = 0;

    public override void _Ready()
    {
        if (_partyIndex > Game.Instance.Party.Count - 1)
        {
            return;
        }

        RefreshDisplay();

        _combatantLeftButton.Pressed += OnCombatantLeftButtonPressed;
        _combatantRightButton.Pressed += OnCombatantRightButtonPressed;
    }

    public void RefreshDisplay()
    {
        FreeSetUtilityControls();

        if (_partyIndex > Game.Instance.Party.Count - 1)
        {
            return;
        }
        Combatant combatant = Game.Instance.Party[_partyIndex];
        _nameLabel.Text = combatant.DisplayName;
        _portrait.Texture = combatant.Sprite2D.Texture;

        // Refresh Movement Utility Functions
        foreach (PackedScene packedScene in _movementUtilities.Scenes)
        {
            MovementUtilityFunction tempMuf = packedScene.Instantiate<MovementUtilityFunction>();
            // make a button with the kvp.key string
            Button button = new();
            button.Text = tempMuf.DisplayName;
            button.AddThemeFontSizeOverride("font_size", 6);
            _movementUtilitiesParent.AddChild(button);

            // TODO this is very inefficient. Clean up later
            foreach (MovementUtilityFunction muf in combatant.Brain.MovementUtilities)
            {
                if (tempMuf.DisplayName == muf.DisplayName)
                {
                    button.Disabled = true;
                    break;
                }
            }

            if (!button.Disabled)
            {
                button.Pressed += () => OnMovementUtilityPressed(packedScene, combatant, tempMuf.Cost, button);
            }
            tempMuf.Free();
        }

        // Refresh Ability Utility Functions
        foreach (PackedScene packedScene in _abilityUtilities.Scenes)
        {
            AbilityUtilityFunction tempAuf = packedScene.Instantiate<AbilityUtilityFunction>();
            // make a button with the kvp.key string
            Button button = new();
            button.Text = tempAuf.DisplayName;
            button.AddThemeFontSizeOverride("font_size", 6);
            _abilityUtilitiesParent.AddChild(button);

            // TODO this is very inefficient. Clean up later
            foreach (AbilityUtilityFunction auf in combatant.Brain.AbilityUtilities)
            {
                if (tempAuf.DisplayName == auf.DisplayName)
                {
                    button.Disabled = true;
                    break;
                }
            }

            if (!button.Disabled)
            {
                button.Pressed += () => OnAbilityUtilityPressed(packedScene, combatant, tempAuf.Cost, button);
            }
            tempAuf.Free();
        }
    }

    private void FreeSetUtilityControls()
    {
        foreach (Node child in _movementUtilitiesParent.GetChildren())
        {
            child.Free();
        }
        foreach (Node child in _abilityUtilitiesParent.GetChildren())
        {
            child.Free();
        }
    }

    private void OnCombatantLeftButtonPressed()
    {
        int before = _partyIndex;
        _partyIndex--;
        if (_partyIndex < 0)
        {
            _partyIndex = Game.Instance.Party.Count - 1;
        }
        if (_partyIndex != before)
        {
            RefreshDisplay();
        }
    }

    private void OnCombatantRightButtonPressed()
    {
        int before = _partyIndex;
        _partyIndex++;
        if (_partyIndex > Game.Instance.Party.Count - 1)
        {
            _partyIndex = 0;
        }
        if (_partyIndex != before)
        {
            RefreshDisplay();
        }
    }

    // TODO clean up clear duplication here (once there is a super class for utility functions)
    private static void OnMovementUtilityPressed(PackedScene packedScene, Combatant combatant, int cost, Button button)
    {
        // check if combatant has enough knowledge points to buy the utility function

        if (combatant.Stats.KnowledgePoints >= cost)
        {
            button.Disabled = true;
            combatant.Stats.KnowledgePoints -= cost;
            MovementUtilityFunction muf = packedScene.Instantiate<MovementUtilityFunction>();
            combatant.Brain.MovementUtilityParent.AddChild(muf);
            combatant.Brain.AddMovementUtility(muf);
        }
        else
        {
            // TODO add error noise and/or error messaging
        }
    }

    private static void OnAbilityUtilityPressed(PackedScene packedScene, Combatant combatant, int cost, Button button)
    {
        // check if combatant has enough knowledge points to buy the utility function

        if (combatant.Stats.KnowledgePoints >= cost)
        {
            button.Disabled = true;
            combatant.Stats.KnowledgePoints -= cost;
            AbilityUtilityFunction auf = packedScene.Instantiate<AbilityUtilityFunction>();
            combatant.Brain.MovementUtilityParent.AddChild(auf);
            combatant.Brain.AddAbilityUtility(auf);
        }
        else
        {
            // TODO add error noise and/or error messaging
        }
    }
}
