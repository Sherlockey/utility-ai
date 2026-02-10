using Godot;

using System;

public partial class Combatant : Node2D
{
	public event EventHandler TurnEnded;

	public enum TurnState
	{
		Active,
		Waiting,
	}

	[Export]
	private int _speed = 6;
	[Export]
	private int _movement = 4;

	private TurnState _turnState = TurnState.Waiting;
	private int _accumulated_speed = 0;
	private int _current_movement = 0;

	public void EndTurn()
	{
		if (_turnState == TurnState.Active)
		{
			_accumulated_speed %= BattleManager.TURN_THRESHOLD;
			_turnState = TurnState.Waiting;
			TurnEnded?.Invoke(this, EventArgs.Empty);
		}
	}

	public void IncrementAccumulatedSpeed()
	{
		_accumulated_speed += _speed;
	}

	public int GetSpeed()
	{
		return _speed;
	}

	public int GetAccumulatedSpeed()
	{
		return _accumulated_speed;
	}

	public void SetAccumulatedSpeed(int value)
	{
		_accumulated_speed = value;
	}

	public TurnState GetTurnState()
	{
		return _turnState;
	}

	public void SetTurnState(TurnState turnState)
	{
		_turnState = turnState;
	}

	public int GetCurrentMovement()
	{
		return _current_movement;
	}

	public void SetCurrentMovement(int value)
	{
		_current_movement = value;
	}

	public int GetMovement()
	{
		return _movement;
	}
}
