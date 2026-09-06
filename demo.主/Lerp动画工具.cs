using Godot;
using System;
using System.Threading.Tasks;

public static class Lerp动画工具
{
	// 为Control节点添加动画方法
	public static async Task 位置Lerp动画(Control 控件, Vector2 起始位置, Vector2 目标位置, float 持续时间)
	{
		var 补间 = 控件.CreateTween();
		补间.TweenProperty(控件, "position", 目标位置, 持续时间)
			.From(起始位置)
			.SetTrans(Tween.TransitionType.Linear);
		
		await 控件.ToSignal(补间, "finished");
	}
	
	// 为Control节点添加缩放动画
	public static async Task 缩放Lerp动画(Control 控件, Vector2 起始缩放, Vector2 目标缩放, float 持续时间)
	{
		var 补间 = 控件.CreateTween();
		补间.TweenProperty(控件, "scale", 目标缩放, 持续时间)
			.From(起始缩放)
			.SetTrans(Tween.TransitionType.Linear);
		
		await 控件.ToSignal(补间, "finished");
	}
	
	// 为Control节点添加旋转动画
	public static async Task 旋转Lerp动画(Control 控件, float 起始旋转, float 目标旋转, float 持续时间)
	{
		var 补间 = 控件.CreateTween();
		补间.TweenProperty(控件, "rotation", 目标旋转, 持续时间)
			.From(起始旋转)
			.SetTrans(Tween.TransitionType.Linear);
		
		await 控件.ToSignal(补间, "finished");
	}
	
	// 通用版本（支持任何Node节点）
	public static async Task 通用位置Lerp动画(Node 节点, Vector2 起始位置, Vector2 目标位置, float 持续时间)
	{
		var 补间 = 节点.CreateTween();
		补间.TweenProperty(节点, "position", 目标位置, 持续时间)
			.From(起始位置)
			.SetTrans(Tween.TransitionType.Linear);
		
		await 节点.ToSignal(补间, "finished");
	}
	
	// Node2D版本（保留原有代码）
	public static async Task 位置Lerp动画(Node2D 节点, Vector2 起始位置, Vector2 目标位置, float 持续时间)
	{
		var 补间 = 节点.CreateTween();
		补间.TweenProperty(节点, "position", 目标位置, 持续时间)
			.From(起始位置)
			.SetTrans(Tween.TransitionType.Linear);
		
		await 节点.ToSignal(补间, "finished");
	}
	
	// 带缓动的Lerp动画
	public static async Task 位置缓动动画(Node2D 节点, Vector2 起始位置, Vector2 目标位置, float 持续时间, Tween.TransitionType 过渡类型 = Tween.TransitionType.Cubic, Tween.EaseType 缓动类型 = Tween.EaseType.Out)
	{
		var 补间 = 节点.CreateTween();
		补间.TweenProperty(节点, "position", 目标位置, 持续时间)
			.From(起始位置)
			.SetTrans(过渡类型)
			.SetEase(缓动类型);
		
		await 节点.ToSignal(补间, "finished");
	}
	
	// Node2D缩放Lerp动画
	public static async Task 缩放Lerp动画(Node2D 节点, Vector2 起始缩放, Vector2 目标缩放, float 持续时间)
	{
		var 补间 = 节点.CreateTween();
		补间.TweenProperty(节点, "scale", 目标缩放, 持续时间)
			.From(起始缩放)
			.SetTrans(Tween.TransitionType.Linear);
		
		await 节点.ToSignal(补间, "finished");
	}
	
	// Node2D旋转Lerp动画
	public static async Task 旋转Lerp动画(Node2D 节点, float 起始旋转, float 目标旋转, float 持续时间)
	{
		var 补间 = 节点.CreateTween();
		补间.TweenProperty(节点, "rotation", 目标旋转, 持续时间)
			.From(起始旋转)
			.SetTrans(Tween.TransitionType.Linear);
		
		await 节点.ToSignal(补间, "finished");
	}
	
	// 完整卡牌抽牌动画：翻转+移动+缩放
	public static async Task 卡牌抽牌动画(Control 卡牌, Vector2 起始位置, Vector2 目标位置, float 持续时间 = 1.0f)
	{
		var 补间 = 卡牌.CreateTween();
		补间.SetParallel(true);
		
		// 位置动画
		补间.TweenProperty(卡牌, "position", 目标位置, 持续时间)
			.From(起始位置)
			.SetTrans(Tween.TransitionType.Quint)
			.SetEase(Tween.EaseType.Out);
		
		// 旋转动画（翻转效果）
		补间.TweenProperty(卡牌, "rotation_degrees", 360f, 持续时间)
			.From(0f)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
		
		// 缩放动画
		补间.TweenProperty(卡牌, "scale", new Vector2(1.0f, 1.0f), 持续时间)
			.From(new Vector2(0.5f, 0.5f))
			.SetTrans(Tween.TransitionType.Elastic)
			.SetEase(Tween.EaseType.Out);
		
		await 卡牌.ToSignal(补间, "finished");
	}
}
