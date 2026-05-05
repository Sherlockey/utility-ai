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
    private Label _levelLabel;
    [Export]
    private Label _knowledgePointsLabel;
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
        FreeBuyUtilityControls();

        if (_partyIndex > Game.Instance.Party.Count - 1)
        {
            return;
        }
        Combatant combatant = Game.Instance.Party[_partyIndex];
        _nameLabel.Text = combatant.DisplayName;
        _portrait.Texture = combatant.Sprite2D.Texture;
        _levelLabel.Text = "LV: " + combatant.Stats.Level;
        _knowledgePointsLabel.Text = "KP: " + combatant.Stats.KnowledgePoints.ToString();

        // Refresh Movement Utility Functions
        foreach (PackedScene packedScene in _movementUtilities.Scenes)
        {
            MovementUtilityFunction tempMuf = packedScene.Instantiate<MovementUtilityFunction>();
            bool learned = false;
            // TODO this is very inefficient. Clean up later
            foreach (MovementUtilityFunction muf in combatant.Brain.MovementUtilities)
            {
                if (tempMuf.DisplayName == muf.DisplayName)
                {
                    learned = true;
                    break;
                }
            }

            // make a hboxcontainer with the button and the cost label
            HBoxContainer hboxContainer = new();
            _movementUtilitiesParent.AddChild(hboxContainer);

            Label label = new();
            if (learned)
            {
                label.Text = "Learned  ";
            }
            else
            {
                label.Text = "Cost: " + tempMuf.Cost.ToString();
            }
            label.AddThemeFontSizeOverride("font_size", 6);

            Button button = new();
            button.Text = tempMuf.DisplayName;
            button.AddThemeFontSizeOverride("font_size", 6);

            hboxContainer.AddChild(label);
            hboxContainer.AddChild(button);

            if (learned)
            {
                button.Disabled = true;
            }
            else
            {
                button.Pressed += () => OnMovementUtilityPressed(packedScene, combatant, tempMuf.Cost, button);
            }
            tempMuf.Free();
        }

        // Refresh Ability Utility Functions
        foreach (PackedScene packedScene in _abilityUtilities.Scenes)
        {
            AbilityUtilityFunction tempAuf = packedScene.Instantiate<AbilityUtilityFunction>();
            bool learned = false;
            // TODO this is very inefficient. Clean up later
            foreach (AbilityUtilityFunction auf in combatant.Brain.AbilityUtilities)
            {
                if (tempAuf.DisplayName == auf.DisplayName)
                {
                    learned = true;
                    break;
                }
            }
            // make a hboxcontainer with the button and the cost label
            HBoxContainer hboxContainer = new();
            _abilityUtilitiesParent.AddChild(hboxContainer);

            Label label = new();
            if (learned)
            {
                label.Text = "Learned  ";
            }
            else
            {
                label.Text = "Cost: " + tempAuf.Cost.ToString();
            }
            label.AddThemeFontSizeOverride("font_size", 6);

            Button button = new();
            button.Text = tempAuf.DisplayName;
            button.AddThemeFontSizeOverride("font_size", 6);

            hboxContainer.AddChild(label);
            hboxContainer.AddChild(button);

            if (learned)
            {
                button.Disabled = true;
            }
            else
            {
                button.Pressed += () => OnAbilityUtilityPressed(packedScene, combatant, tempAuf.Cost, button);
            }
            tempAuf.Free();
        }
    }

    private void FreeBuyUtilityControls()
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
    private void OnMovementUtilityPressed(PackedScene packedScene, Combatant combatant, int cost, Button button)
    {
        // check if combatant has enough knowledge points to buy the utility function

        if (combatant.Stats.KnowledgePoints >= cost)
        {
            button.Disabled = true;
            combatant.Stats.KnowledgePoints -= cost;
            _knowledgePointsLabel.Text = "KP: " + combatant.Stats.KnowledgePoints.ToString();
            MovementUtilityFunction muf = packedScene.Instantiate<MovementUtilityFunction>();
            combatant.Brain.MovementUtilityParent.AddChild(muf);
            combatant.Brain.AddMovementUtility(muf);
        }
        else
        {
            // TODO add error noise and/or error messaging
        }
    }

    private void OnAbilityUtilityPressed(PackedScene packedScene, Combatant combatant, int cost, Button button)
    {
        // check if combatant has enough knowledge points to buy the utility function

        if (combatant.Stats.KnowledgePoints >= cost)
        {
            button.Disabled = true;
            combatant.Stats.KnowledgePoints -= cost;
            _knowledgePointsLabel.Text = "KP: " + combatant.Stats.KnowledgePoints.ToString();
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
