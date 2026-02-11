using Godot;

using System;

public partial class Level : Node2D
{
	[Export]
	private TileMapLayer _tileMapLayer;
	[Export]
	private BattleManager _battleManager;

	public override void _Ready()
	{
		_battleManager.InitializeCombatantsTileSize(_tileMapLayer.TileSet.TileSize);
	}
}
