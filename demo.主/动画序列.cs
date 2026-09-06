using Godot;
using System;
[GlobalClass]
	public partial class 动画序列 : Resource
	{
		[Export] public Vector2 目标位置偏移 { get; set; } = Vector2.Zero;
		[Export] public float 移动时长 { get; set; } = 1.0f;
		[Export] public float 停留时间 { get; set; } = 1.0f;
		[Export] public Tween.EaseType 缓动类型 { get; set; } = Tween.EaseType.InOut;
		[Export] public Tween.TransitionType 过渡类型 { get; set; } = Tween.TransitionType.Quad;
		
		// 描述这个动画步骤
		[Export] public string 描述 { get; set; } = "";
	}
