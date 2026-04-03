using Godot;

using System.Collections.Generic;

public partial class UtilityDisplay : PanelContainer
{
    public List<Combatant> Combatants = []; // updated by BattleManager

    [Export]
    private Label _name;
    [Export]
    private Label _rangeValue;
    [Export]
    private TextureRect _portrait;
    [Export]
    private Button _combatantLeftButton;
    [Export]
    private Button _combatantRightButton;
    [Export]
    private Button _rangeLeftButton;
    [Export]
    private Button _rangeRightButton;
    [Export]
    private Slider _friendlyTerritorySlider;
    [Export]
    private Slider _oppositionTerritorySlider;
    [Export]
    private Slider _contestedTerritorySlider;
    [Export]
    private Slider _maintainRangeSlider;
    [Export]
    private Slider _greatestDamageSlider;
    [Export]
    private Slider _eliminateOppositionSlider;
    [Export]
    private Slider _targetFrailestSlider;
    [Export]
    private Slider _targetDeadliestSlider;

    private Combatant _combatant = null;
    private int _currentCombatantIndex = 0;

    public override void _Ready()
    {
        Visible = false;

        _combatantLeftButton.Pressed += OnCombatantLeftButtonPressed;
        _combatantRightButton.Pressed += OnCombatantRightButtonPressed;
        _rangeLeftButton.Pressed += OnRangeLeftButtonPressed;
        _rangeRightButton.Pressed += OnRangeRightButtonPressed;
        _friendlyTerritorySlider.DragEnded += OnFriendlyTerritorySliderDragEnded;
        _oppositionTerritorySlider.DragEnded += OnOppositionTerritorySliderDragEnded;
        _contestedTerritorySlider.DragEnded += OnContestedTerritorySliderDragEnded;
        _maintainRangeSlider.DragEnded += OnMaintainRangeSliderDragEnded;
        _greatestDamageSlider.DragEnded += OnGreatestDamageSliderDragEnded;
        _eliminateOppositionSlider.DragEnded += OnEliminateOppositionSliderDragEnded;
        _targetFrailestSlider.DragEnded += OnTargetFrailestSliderDragEnded;
        _targetDeadliestSlider.DragEnded += OnTargetDeadliestSliderDragEnded;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                Visible = !Visible;
            }
        }
    }

    public void RefreshDisplay()
    {
        _combatant = Combatants[_currentCombatantIndex];
        _name.Text = _combatant.DisplayName;
        _portrait.Texture = _combatant.Sprite2D.Texture;
        if (_combatant != null)
        {
            foreach (MovementUtility movementUtility in _combatant.Brain.MovementUtilities)
            {
                if (movementUtility is MoveToFriendlyTerritory moveToFriendlyTerritory)
                {
                    _friendlyTerritorySlider.Value = moveToFriendlyTerritory.Weight;
                }
                else if (movementUtility is MoveToOppositionTerritory moveToOppositionTerritory)
                {
                    _oppositionTerritorySlider.Value = moveToOppositionTerritory.Weight;
                }
                else if (movementUtility is MoveToContestedTerritory moveToContestedTerritory)
                {
                    _contestedTerritorySlider.Value = moveToContestedTerritory.Weight;
                }
                else if (movementUtility is RangeFromOpposition rangeFromOpposition)
                {
                    _maintainRangeSlider.Value = rangeFromOpposition.Weight;
                    _rangeValue.Text = rangeFromOpposition.Range.ToString();
                }
            }
            foreach (AbilityUtility abilityUtility in _combatant.Brain.AbilityUtilities)
            {
                if (abilityUtility is GreatestDamage greatestDamage)
                {
                    _greatestDamageSlider.Value = greatestDamage.Weight;
                }
                else if (abilityUtility is EliminateOpposition eliminateOpposition)
                {
                    _eliminateOppositionSlider.Value = eliminateOpposition.Weight;
                }
                else if (abilityUtility is TargetFrailest targetFrailest)
                {
                    _targetFrailestSlider.Value = targetFrailest.Weight;
                }
                else if (abilityUtility is TargetDeadliest targetDeadliest)
                {
                    _targetDeadliestSlider.Value = targetDeadliest.Weight;
                }
            }
        }
    }

    private void OnCombatantLeftButtonPressed()
    {
        _currentCombatantIndex -= 1;
        if (_currentCombatantIndex < 0)
        {
            _currentCombatantIndex = Combatants.Count - 1;
        }
        _combatant = Combatants[_currentCombatantIndex];
        // TODO add null checking
        RefreshDisplay();
    }

    private void OnCombatantRightButtonPressed()
    {
        _currentCombatantIndex = (_currentCombatantIndex + 1) % Combatants.Count;
        _combatant = Combatants[_currentCombatantIndex];
        // TODO add null checking
        RefreshDisplay();
    }

    private void OnRangeLeftButtonPressed()
    {
        if (_combatant != null)
        {
            foreach (MovementUtility movementUtility in _combatant.Brain.MovementUtilities)
            {
                if (movementUtility is RangeFromOpposition rangeFromOpposition)
                {
                    rangeFromOpposition.Range--;
                    if (rangeFromOpposition.Range < 1)
                    {
                        rangeFromOpposition.Range = 1;
                    }
                    _rangeValue.Text = rangeFromOpposition.Range.ToString();
                    break;
                }
            }
        }
    }

    private void OnRangeRightButtonPressed()
    {
        if (_combatant != null)
        {
            foreach (MovementUtility movementUtility in _combatant.Brain.MovementUtilities)
            {
                if (movementUtility is RangeFromOpposition rangeFromOpposition)
                {
                    rangeFromOpposition.Range++;
                    _rangeValue.Text = rangeFromOpposition.Range.ToString();
                    break;
                }
            }
        }
    }

    private void OnFriendlyTerritorySliderDragEnded(bool valueChanged)
    {
        if (valueChanged && _combatant != null)
        {
            foreach (MovementUtility movementUtility in _combatant.Brain.MovementUtilities)
            {
                if (movementUtility is MoveToFriendlyTerritory moveToFriendlyTerritory)
                {
                    moveToFriendlyTerritory.Weight = (float)_friendlyTerritorySlider.Value;
                    break;
                }
            }
        }
    }

    private void OnOppositionTerritorySliderDragEnded(bool valueChanged)
    {
        if (valueChanged && _combatant != null)
        {
            foreach (MovementUtility movementUtility in _combatant.Brain.MovementUtilities)
            {
                if (movementUtility is MoveToOppositionTerritory moveToOppositionTerritory)
                {
                    moveToOppositionTerritory.Weight = (float)_oppositionTerritorySlider.Value;
                    break;
                }
            }
        }
    }

    private void OnContestedTerritorySliderDragEnded(bool valueChanged)
    {
        if (valueChanged && _combatant != null)
        {
            foreach (MovementUtility movementUtility in _combatant.Brain.MovementUtilities)
            {
                if (movementUtility is MoveToContestedTerritory moveToContestedTerritory)
                {
                    moveToContestedTerritory.Weight = (float)_contestedTerritorySlider.Value;
                    break;
                }
            }
        }
    }

    private void OnMaintainRangeSliderDragEnded(bool valueChanged)
    {
        if (valueChanged && _combatant != null)
        {
            foreach (MovementUtility movementUtility in _combatant.Brain.MovementUtilities)
            {
                if (movementUtility is RangeFromOpposition rangeFromOpposition)
                {
                    rangeFromOpposition.Weight = (float)_maintainRangeSlider.Value;
                    break;
                }
            }
        }
    }

    private void OnGreatestDamageSliderDragEnded(bool valueChanged)
    {
        if (valueChanged && _combatant != null)
        {
            foreach (AbilityUtility abilityUtility in _combatant.Brain.AbilityUtilities)
            {
                if (abilityUtility is GreatestDamage greatestDamage)
                {
                    greatestDamage.Weight = (float)_greatestDamageSlider.Value;
                    break;
                }
            }
        }
    }

    private void OnEliminateOppositionSliderDragEnded(bool valueChanged)
    {
        if (valueChanged && _combatant != null)
        {
            foreach (AbilityUtility abilityUtility in _combatant.Brain.AbilityUtilities)
            {
                if (abilityUtility is EliminateOpposition eliminateOpposition)
                {
                    eliminateOpposition.Weight = (float)_eliminateOppositionSlider.Value;
                    break;
                }
            }
        }
    }

    private void OnTargetFrailestSliderDragEnded(bool valueChanged)
    {
        if (valueChanged && _combatant != null)
        {
            foreach (AbilityUtility abilityUtility in _combatant.Brain.AbilityUtilities)
            {
                if (abilityUtility is TargetFrailest targetFrailest)
                {
                    targetFrailest.Weight = (float)_targetFrailestSlider.Value;
                    break;
                }
            }
        }
    }

    private void OnTargetDeadliestSliderDragEnded(bool valueChanged)
    {
        if (valueChanged && _combatant != null)
        {
            foreach (AbilityUtility abilityUtility in _combatant.Brain.AbilityUtilities)
            {
                if (abilityUtility is TargetDeadliest targetDeadliest)
                {
                    targetDeadliest.Weight = (float)_targetDeadliestSlider.Value;
                    break;
                }
            }
        }
    }
}
