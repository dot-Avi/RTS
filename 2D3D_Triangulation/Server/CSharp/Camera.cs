using Godot;
using System;

public partial class Camera : Node2D
{
	public bool moveKeyHeld = false;
	[Export] public float minXPosition = 0f;
	[Export] public float maxXPosition = 51200f;
	[Export] public float minYPosition = 0f;
	[Export] public float maxYPosition = 51200f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Camera Loaded");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsAction("moveCamera"))
		{
			GD.Print("Middle Mouse Button");
			if (!moveKeyHeld && @event.IsActionPressed("moveCamera"))
			{
				GD.Print("Pressed");
				moveKeyHeld = true;
			}
			if (moveKeyHeld && @event.IsActionReleased("moveCamera"))
			{
				GD.Print("Released");
				moveKeyHeld = false;
			}
		}

		if (moveKeyHeld && @event is InputEventMouseMotion)
		{
			MoveCamera((InputEventMouseMotion)@event);
		}
	}

	public void MoveCamera(InputEventMouseMotion translation)
	{
		Position -= translation.ScreenRelative * Transform.X.X;
		if (Position.X < minXPosition)
		{
			Position = new Vector2(minXPosition, Position.Y);
		}
		if (Position.Y < minYPosition)
		{
			Position = new Vector2(Position.X, minYPosition);
		}
		if (Position.X > maxXPosition)
		{
			Position = new Vector2(maxXPosition, Position.Y);
		}
		if (Position.Y > maxYPosition)
		{
			Position = new Vector2(Position.X, maxXPosition);
		}
	}
}
