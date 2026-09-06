using Godot;
using System;

public partial class 初始值 : ColorRect
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		   // 强制设置初始位置为你想要的值（位置1）
	Position = new Vector2(152, 519);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
