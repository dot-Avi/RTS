using Godot;
using System;

[GlobalClass]
public partial class MapLayer : TileMapLayer
{
	public bool isReady = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* foreach (Vector2I tile in GetUsedCells())
		{
			if (tile.X < 0 || tile.Y < 0)
			{
				EraseCell(tile);
			}
		} */

		isReady = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
