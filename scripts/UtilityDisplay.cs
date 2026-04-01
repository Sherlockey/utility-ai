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
    private Slider _keepRangeSlider;
    [Export]
    private Slider _greatestDamageSlider;
    [Export]
    private Slider _eliminateOppositionSlider;
    [Export]
    private Slider _targetWeakestSlider;
    [Export]
    private Slider _avoidFriendlyDamageSlider;

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
        _keepRangeSlider.DragEnded += OnKeepRangeSliderDragEnded;
        _greatestDamageSlider.DragEnded += OnGreatestDamageSliderDragEnded;
        _eliminateOppositionSlider.DragEnded += OnEliminateOppositionSliderDragEnded;
        _targetWeakestSlider.DragEnded += OnTargetWeakestSliderDragEnded;
        _avoidFriendlyDamageSlider.DragEnded += OnAvoidFriendlyDamageSliderDragEnded;
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
                    _keepRangeSlider.Value = rangeFromOpposition.Weight;
                    _rangeValue.Text = rangeFromOpposition.Range.ToString();
                }
                // TODO add the four values for Abilities
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
        if (_combatant != null)
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
        if (_combatant != null)
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
        if (_combatant != null)
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

    private void OnKeepRangeSliderDragEnded(bool valueChanged)
    {
        if (_combatant != null)
        {
            foreach (MovementUtility movementUtility in _combatant.Brain.MovementUtilities)
            {
                if (movementUtility is RangeFromOpposition rangeFromOpposition)
                {
                    rangeFromOpposition.Weight = (float)_keepRangeSlider.Value;
                    break;
                }
            }
        }
    }

    private void OnGreatestDamageSliderDragEnded(bool valueChanged)
    {

    }

    private void OnEliminateOppositionSliderDragEnded(bool valueChanged)
    {

    }

    private void OnTargetWeakestSliderDragEnded(bool valueChanged)
    {

    }

    private void OnAvoidFriendlyDamageSliderDragEnded(bool valueChanged)
    {

    }
}
