using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class 敌人手牌管理器 : Control
{
	[Export] public PackedScene 卡牌UIPrefab;
	[Export] private float 手牌区域宽度 = 400f;
	[Export] private Vector2 卡牌原始尺寸 = new Vector2(120, 180);
	[Export] private float 背面卡牌缩放 = 0.3f;
	[Export] private float 卡牌间距 = -1f;
	[Export] private float 动画时长 = 0.3f;

	public List<卡牌UI> 当前卡牌UI列表 = new();
	private 卡牌战斗单位 所属单位;

	public void 初始化(卡牌战斗单位 单位)
	{
		所属单位 = 单位;
		if (卡牌UIPrefab == null)
			GD.PrintErr("敌人手牌管理器: 卡牌UIPrefab未设置！");
		GD.Print($"[手牌] 初始化，所属单位: {单位?.名字}");
	}

	public void 设置手牌区域宽度(float 宽度)
	{
		手牌区域宽度 = 宽度;
		GD.Print($"[手牌] 设置区域宽度 = {宽度}");
	}

	private 卡牌UI 创建背面卡牌(卡牌实例 真实卡牌)
	{
		if (卡牌UIPrefab == null) return null;
		var 卡牌UI实例 = 卡牌UIPrefab.Instantiate<卡牌UI>();
		if (卡牌UI实例 == null) return null;

		卡牌UI实例.初始化(真实卡牌, true);
		卡牌UI实例.Scale = new Vector2(背面卡牌缩放, 背面卡牌缩放);
		卡牌UI实例.PivotOffset = 卡牌UI实例.Size * 0.5f;
		卡牌UI实例.设置高光启用(false);
		卡牌UI实例.设置禁用状态(true);
		GD.Print($"[手牌] 创建背面卡牌: {真实卡牌?.基础数据?.卡牌名称 ?? "null"}");
		return 卡牌UI实例;
	}

	public void 刷新手牌()
	{
		GD.Print($"[手牌] 刷新手牌，旧UI数量={当前卡牌UI列表.Count}");
		foreach (var ui in 当前卡牌UI列表)
			if (ui != null && IsInstanceValid(ui)) ui.QueueFree();
		当前卡牌UI列表.Clear();

		if (所属单位?.手牌 == null || 所属单位.手牌.Count == 0)
		{
			GD.Print("[手牌] 刷新手牌: 无手牌");
			return;
		}

		int 数量 = 所属单位.手牌.Count;
		GD.Print($"[手牌] 刷新手牌，手牌数量={数量}");
		var 目标位置列表 = 计算卡牌位置列表(数量);
		Vector2 起始生成点 = new Vector2(Position.X, Position.Y - 100);

		for (int i = 0; i < 数量; i++)
		{
			var 真实卡牌 = 所属单位.手牌[i];
			var 卡牌UI = 创建背面卡牌(真实卡牌);
			if (卡牌UI == null) continue;
			AddChild(卡牌UI);
			当前卡牌UI列表.Add(卡牌UI);

			卡牌UI.Position = 起始生成点;
			卡牌UI.Scale = new Vector2(0.1f, 0.1f);
			卡牌UI.Rotation = Mathf.Pi;
			卡牌UI.Modulate = new Color(1, 1, 1, 0.8f);

			var 动画 = 卡牌UI.CreateTween();
			if (i > 0) 动画.TweenInterval(i * 0.1f);
			动画.SetParallel(true);
			Vector2 目标缩放 = new Vector2(背面卡牌缩放, 背面卡牌缩放);
			动画.TweenProperty(卡牌UI, "position", 目标位置列表[i], 0.6f)
				.SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
			动画.TweenProperty(卡牌UI, "scale", 目标缩放, 0.5f)
				.SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
			动画.TweenProperty(卡牌UI, "rotation", 0f, 0.4f)
				.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
			动画.TweenProperty(卡牌UI, "modulate:a", 1f, 0.3f);
		}
		GD.Print("[手牌] 刷新手牌完成，入场动画已启动");
	}

	public void 移除一张卡牌()
	{
		GD.Print($"[手牌] 移除一张卡牌被调用，当前列表数量={当前卡牌UI列表.Count}");
		if (当前卡牌UI列表.Count == 0) return;
		var 卡牌UI = 当前卡牌UI列表[0];
		string 卡牌名 = 卡牌UI.获取卡牌数据()?.基础数据?.卡牌名称 ?? "未知";
		GD.Print($"[手牌] 移除第一张卡牌: {卡牌名}");
		当前卡牌UI列表.RemoveAt(0);
		卡牌UI.QueueFree();
		GD.Print($"[手牌] 已移除，剩余数量={当前卡牌UI列表.Count}，开始重新排列");
		重新排列();
	}

	public void 移除卡牌UI(卡牌实例 卡牌)
	{
		GD.Print($"[手牌] 移除卡牌UI，卡牌={卡牌?.基础数据?.卡牌名称 ?? "null"}");
		var 卡牌UI = 当前卡牌UI列表.Find(ui => ui.获取卡牌数据() == 卡牌);
		if (卡牌UI != null)
		{
			当前卡牌UI列表.Remove(卡牌UI);
			卡牌UI.QueueFree();
			GD.Print($"[手牌] 已移除指定卡牌UI，剩余={当前卡牌UI列表.Count}");
			重新排列();
		}
		else
		{
			GD.Print("[手牌] 未找到对应的卡牌UI");
		}
	}

	public void 添加一张卡牌()
	{
		GD.Print("[手牌] 添加一张卡牌被调用");
		if (所属单位 == null) return;
		刷新手牌();
	}

	public void 清空手牌()
	{
		GD.Print("[手牌] 清空手牌");
		foreach (var ui in 当前卡牌UI列表)
			if (ui != null && IsInstanceValid(ui)) ui.QueueFree();
		当前卡牌UI列表.Clear();
	}

	public void 重新排列()
	{
		int 数量 = 当前卡牌UI列表.Count;
		GD.Print($"[手牌] 重新排列，当前卡牌数量={数量}");
		if (数量 == 0) return;

		var 目标位置列表 = 计算卡牌位置列表(数量);
		for (int i = 0; i < 数量; i++)
		{
			var 卡牌 = 当前卡牌UI列表[i];
			Vector2 目标位置 = 目标位置列表[i];
			卡牌.更新原始位置(目标位置);
			var 动画 = 卡牌.CreateTween();
			动画.TweenProperty(卡牌, "position", 目标位置, 动画时长)
				.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		}
		GD.Print("[手牌] 重新排列动画已启动");
	}

	private List<Vector2> 计算卡牌位置列表(int 数量)
	{
		var 结果 = new List<Vector2>();
		if (数量 == 0) return 结果;

		Vector2 中心点 = Position;
		float 卡牌宽 = 卡牌原始尺寸.X * 背面卡牌缩放;
		float 间距 = 获取最终间距(数量);
		float 总宽 = 数量 * 卡牌宽 + (数量 - 1) * 间距;
		float 起始X = 中心点.X - 总宽 / 2 + 卡牌宽 / 2;
		float y = 中心点.Y;

		for (int i = 0; i < 数量; i++)
		{
			float x = 起始X + i * (卡牌宽 + 间距);
			结果.Add(new Vector2(x, y));
		}
		return 结果;
	}

	private float 获取最终间距(int 数量)
	{
		if (卡牌间距 >= 0) return 卡牌间距;
		return 计算动态间距(数量);
	}

	private float 计算动态间距(int 数量)
	{
		if (数量 <= 1) return 0f;
		float 卡牌宽 = 卡牌原始尺寸.X * 背面卡牌缩放;
		float 理想总宽 = 数量 * 卡牌宽;
		float 可用宽 = 手牌区域宽度 - 理想总宽;
		if (可用宽 <= 0) return 5f;
		float 间距 = 可用宽 / (数量 - 1);
		if (数量 <= 3) return Mathf.Min(80f, 间距);
		if (数量 <= 6) return Mathf.Clamp(间距, 10f, 60f);
		return Mathf.Clamp(间距, 5f, 30f);
	}

	public async Task 播放卡牌展示动画(卡牌实例 卡牌, 卡牌使用记录管理器 记录管理器 = null)
	{
		if (卡牌?.基础数据 == null) return;
		if (卡牌UIPrefab == null) return;

		var 展示UI = 卡牌UIPrefab.Instantiate<卡牌UI>();
		if (展示UI == null) return;

		展示UI.设置为查看模式(卡牌.基础数据);
		展示UI.Scale = new Vector2(1.5f, 1.5f);
		展示UI.PivotOffset = 展示UI.Size * 0.5f;

		var 根节点 = GetTree().Root;
		根节点.AddChild(展示UI);

		var 屏幕大小 = GetTree().Root.GetVisibleRect().Size;
		var 目标位置 = new Vector2(屏幕大小.X / 2, 屏幕大小.Y / 2 - 200);
		展示UI.Position = 目标位置;
		展示UI.ZIndex = 100;
		展示UI.Modulate = new Color(1, 1, 1, 0);
		展示UI.Rotation = Mathf.Pi;

		var 入场动画 = 展示UI.CreateTween();
		入场动画.SetParallel(true);
		入场动画.TweenProperty(展示UI, "scale", new Vector2(1.5f, 1.5f), 0.5f)
			.SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		入场动画.TweenProperty(展示UI, "modulate:a", 1f, 0.3f);
		入场动画.TweenProperty(展示UI, "rotation", 0f, 0.5f)
			.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		入场动画.TweenProperty(展示UI, "position", 目标位置 - new Vector2(0, 30), 0.5f)
			.SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
		await ToSignal(入场动画, "finished");

		await ToSignal(GetTree().CreateTimer(1.5f), "timeout");

		var 退场动画 = 展示UI.CreateTween();
		退场动画.SetParallel(true);
		退场动画.TweenProperty(展示UI, "scale", new Vector2(0.1f, 0.1f), 0.4f)
			.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
		退场动画.TweenProperty(展示UI, "modulate:a", 0f, 0.4f);
		await ToSignal(退场动画, "finished");

		展示UI.QueueFree();

		if (记录管理器 != null && 卡牌.基础数据.类型 != 卡牌数据.卡牌类型.装备)
			记录管理器.添加记录(卡牌, false);
	}
}
