// EnemyCardUI.cs
using Godot;
using System;

public partial class 敌人卡牌UI : Control
{
	[Export] private TextureRect 卡面;
	[Export] private float 悬停缩放倍数 = 1.05f;
	[Export] private float 动画时长 = 0.2f;
	
	private Vector2 原始位置;
	private Vector2 原始缩放;
	private bool 正在动画中 = false;
	
	public override void _Ready()
	{
		原始位置 = Position;
		原始缩放 = Scale;
		
		// 确保是卡背显示
		if (卡面 != null && 卡面.Texture == null)
		{
			// 可以在这里设置默认卡背纹理
			// 或者由外部传入
		}
	}
	
	// 初始化，只需要卡背纹理
	public void 初始化(Texture2D 卡背纹理)
	{
		if (卡面 != null && 卡背纹理 != null)
		{
			卡面.Texture = 卡背纹理;
		}
	}
	
	// 初始化，使用卡牌实例（可选）
	public void 初始化(卡牌实例 卡牌)
	{
		if (卡牌?.基础数据?.卡背贴图 != null)
		{
			卡面.Texture = 卡牌.基础数据.卡背贴图;
		}
	}
	
	// 执行平滑移动动画
	public void 执行平滑移动动画(Vector2 目标位置, float 目标旋转, int 目标层级, float 时长 = 0.3f, float 延迟 = 0f)
	{
		if (正在动画中) return;
		正在动画中 = true;
		
		var 动画 = CreateTween();
		
		// 如果有延迟，先等待
		if (延迟 > 0)
		{
			动画.TweenInterval(延迟);
		}
		
		动画.SetParallel(true);
		
		// 移动位置
		动画.TweenProperty(this, "position", 目标位置, 时长)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);
		
		// 旋转
		动画.TweenProperty(this, "rotation", 目标旋转, 时长)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);
		
		// 层级
		动画.TweenCallback(Callable.From(() => {
			ZIndex = 目标层级;
		}));
		
		// 动画完成回调
		动画.Connect("finished", Callable.From(() => {
			正在动画中 = false;
			原始位置 = 目标位置; // 更新原始位置
		}));
	}
	
	// 更新原始位置
	public void 更新原始位置(Vector2 新位置)
	{
		原始位置 = 新位置;
	}
	
	// 获取原始缩放
	public Vector2 获取原始缩放()
	{
		return 原始缩放;
	}
	
	// 简单消失动画
	public void 执行消失动画(Action 完成回调 = null)
	{
		if (正在动画中) return;
		正在动画中 = true;
		
		var 动画 = CreateTween();
		动画.SetParallel(true);
		
		// 缩小
		动画.TweenProperty(this, "scale", Vector2.Zero, 0.3f)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
		
		// 透明度
		动画.TweenProperty(this, "modulate:a", 0f, 0.3f);
		
		// 动画完成回调
		动画.Connect("finished", Callable.From(() => {
			正在动画中 = false;
			完成回调?.Invoke();
		}));
	}
	
	// 创建补间动画
	private void 创建补间动画(string 属性, Variant 目标值, float 时长)
	{
		var 补间 = CreateTween();
		补间.SetEase(Tween.EaseType.Out);
		补间.SetTrans(Tween.TransitionType.Quad);
		
		if (属性 == "移动效果")
		{
			补间.TweenProperty(this, "position", 目标值, 时长);
		}
		else if (属性 == "缩放效果")
		{
			补间.TweenProperty(this, "scale", 目标值, 时长);
		}
	}
}
