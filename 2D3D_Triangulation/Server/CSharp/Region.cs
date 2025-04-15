using Godot;
using Godot.NativeInterop;
using System;

[GlobalClass]
public partial class Region : Polygon2D
{
	public Godot.Collections.Array<Vector2> points = new Godot.Collections.Array<Vector2>();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
