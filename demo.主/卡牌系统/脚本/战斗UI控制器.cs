using Godot;
using System.Collections.Generic;

public partial class 战斗UI控制器 : Node
{
	private Label 玩家生命值显示;
	private Label 怪物生命值显示;
	private Label 玩家护盾显示;
	private Label 怪物护盾显示;
	private Label 玩家手牌数量显示;
	private Label 怪物手牌数量显示;
	private Label 玩家卡组数量显示;
	private Label 怪物卡组数量显示;
	private ProgressBar 玩家行动条显示;
	private ProgressBar 怪物行动条显示;
	private Label 战斗日志;
	private Button 结束回合按钮;
	private Control 卡牌生成位置;
	private Control 玩家手牌位置;
	private HBoxContainer 能量槽容器;
	private PackedScene 能量格预制体;
	private List<ColorRect> 能量格列表 = new List<ColorRect>();
	private 卡牌战斗管理器 _battleManager;

	private readonly Color 未激活颜色 = new Color(1, 1, 0);
	private readonly Color 已激活颜色 = new Color(1, 0, 0);
	private readonly Color 灰色颜色 = new Color(0.5f, 0.5f, 0.5f);

	public void 初始化(
		Label 玩家生命, Label 怪物生命,
		Label 玩家护盾, Label 怪物护盾,
		Label 玩家手牌数, Label 怪物手牌数,
		Label 玩家卡组数, Label 怪物卡组数,
		ProgressBar 玩家行动条, ProgressBar 怪物行动条,
		Label 日志, Button 结束回合,
		Control 卡牌生成点, Control 手牌位置,
		HBoxContainer 能量槽, PackedScene 能量格, 卡牌战斗管理器 battleManager)
	{
		玩家生命值显示 = 玩家生命;
		怪物生命值显示 = 怪物生命;
		玩家护盾显示 = 玩家护盾;
		怪物护盾显示 = 怪物护盾;
		玩家手牌数量显示 = 玩家手牌数;
		怪物手牌数量显示 = 怪物手牌数;
		玩家卡组数量显示 = 玩家卡组数;
		怪物卡组数量显示 = 怪物卡组数;
		玩家行动条显示 = 玩家行动条;
		怪物行动条显示 = 怪物行动条;
		战斗日志 = 日志;
		结束回合按钮 = 结束回合;
		卡牌生成位置 = 卡牌生成点;
		玩家手牌位置 = 手牌位置;
		能量槽容器 = 能量槽;
		能量格预制体 = 能量格;
		_battleManager = battleManager;

		初始化能量槽();
	}

	private void 初始化能量槽()
	{
		if (能量槽容器 == null) return;
		foreach (Node child in 能量槽容器.GetChildren())
			if (child is ColorRect cr)
				能量格列表.Add(cr);
		while (能量格列表.Count < 3)
		{
			ColorRect 新格子 = new ColorRect();
			新格子.Size = new Vector2(40, 40);
			能量槽容器.AddChild(新格子);
			能量格列表.Add(新格子);
		}
	}
public void 更新行动条显示(float 玩家行动条, float 怪物行动条)
{
	if (玩家行动条显示 != null)
		玩家行动条显示.Value = Mathf.Clamp(玩家行动条, 0, 100);
	if (怪物行动条显示 != null)
		怪物行动条显示.Value = Mathf.Clamp(怪物行动条, 0, 100);
}
	public void 更新能量显示(int 当前能量, bool 是否强化模式)
	{
		for (int i = 0; i < 能量格列表.Count; i++)
		{
			if (i < 当前能量)
				能量格列表[i].Color = 是否强化模式 ? 已激活颜色 : 未激活颜色;
			else
				能量格列表[i].Color = 灰色颜色;
		}
	}

	public async void 播放强化特效(Viewport 视口)
	{
		var 特效层 = new ColorRect();
		特效层.Color = new Color(1, 1, 1, 0);
		特效层.Size = 视口.GetVisibleRect().Size;
		特效层.MouseFilter = Control.MouseFilterEnum.Ignore;
		_battleManager.AddChild(特效层);
		var tween = _battleManager.CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(特效层, "color:a", 1.0f, 0.2f);
		tween.TweenProperty(特效层, "color:a", 0.0f, 0.4f).SetDelay(0.2f);
		await ToSignal(tween, Tween.SignalName.Finished);
		特效层.QueueFree();
	}

	public void 更新UI(卡牌战斗单位 玩家, 卡牌战斗单位 怪物)
	{
		if (玩家生命值显示 != null) 玩家生命值显示.Text = $"生命值: {玩家?.生命值 ?? 0}";
		if (怪物生命值显示 != null) 怪物生命值显示.Text = $"生命值: {怪物?.生命值 ?? 0}";
		if (玩家护盾显示 != null) 玩家护盾显示.Text = $"护盾: {玩家?.护盾值 ?? 0}";
		if (怪物护盾显示 != null) 怪物护盾显示.Text = $"护盾: {怪物?.护盾值 ?? 0}";
		if (玩家手牌数量显示 != null) 玩家手牌数量显示.Text = $"手牌: {玩家?.手牌?.Count ?? 0}";
		if (怪物手牌数量显示 != null) 怪物手牌数量显示.Text = $"手牌: {怪物?.手牌?.Count ?? 0}";
		if (玩家卡组数量显示 != null) 玩家卡组数量显示.Text = $"卡组: {玩家?.抽牌堆?.Count ?? 0}";
		if (怪物卡组数量显示 != null) 怪物卡组数量显示.Text = $"卡组: {怪物?.抽牌堆?.Count ?? 0}";
	}

	public void 添加战斗日志(string 日志)
	{
		if (战斗日志 != null)
		{
			战斗日志.Text += $"\n{日志}";
			if (战斗日志.GetLineCount() > 20)
			{
				var 文本 = 战斗日志.Text;
				var 索引 = 文本.IndexOf('\n') + 1;
				战斗日志.Text = 文本.Substring(索引);
			}
		}
	}

	public void 通知玩家卡牌禁用状态(bool 禁用, 卡牌游戏主场景 主场景)
	{
		if (主场景?.玩家手牌管理器 == null) return;
		foreach (var 卡牌UI in 主场景.玩家手牌管理器.当前卡牌UI列表)
			卡牌UI.Modulate = 禁用 ? new Color(0.6f, 0.6f, 0.6f, 0.8f) : new Color(1, 1, 1, 1);
	}

public void 通知手牌管理器更新(卡牌游戏主场景 主场景)
{
	主场景?.玩家手牌管理器?.更新手牌显示();
	// 改为更新第一个怪物（或循环更新所有怪物UI）
	var 第一个怪物 = _battleManager.怪物队伍.Count > 0 ? _battleManager.怪物队伍[0] : null;
	更新UI(_battleManager.玩家单位, 第一个怪物);
}

	public void 显示抽牌动画(卡牌实例 卡牌, Node 父节点)
	{
		if (卡牌生成位置 == null || 玩家手牌位置 == null) return;
		var 临时卡牌UI = new Control();
		临时卡牌UI.Size = new Vector2(120, 180);
		临时卡牌UI.Position = 卡牌生成位置.GlobalPosition;
		临时卡牌UI.Scale = new Vector2(0.1f, 0.1f);
		临时卡牌UI.Rotation = Mathf.Pi;
		临时卡牌UI.PivotOffset = 临时卡牌UI.Size / 2;
		var 背景 = new Panel();
		背景.Size = 临时卡牌UI.Size;
		var 样式 = new StyleBoxFlat();
		样式.BgColor = new Color(0.1f, 0.1f, 0.1f);
		背景.AddThemeStyleboxOverride("panel", 样式);
		临时卡牌UI.AddChild(背景);
		父节点.AddChild(临时卡牌UI);
		var 目标位置 = 玩家手牌位置.GlobalPosition;
		var 动画 = 临时卡牌UI.CreateTween();
		动画.SetParallel(true);
		动画.TweenProperty(临时卡牌UI, "position", 目标位置, 1.0f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		动画.TweenProperty(临时卡牌UI, "rotation", 0f, 0.8f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		动画.TweenProperty(临时卡牌UI, "scale", new Vector2(1.0f, 1.0f), 1.0f).SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
		临时卡牌UI.Modulate = new Color(1, 1, 1, 0);
		动画.TweenProperty(临时卡牌UI, "modulate:a", 1f, 0.5f);
		动画.TweenCallback(Callable.From(() => { 临时卡牌UI.QueueFree(); 通知手牌管理器更新(_battleManager.GetParent() as 卡牌游戏主场景); }));
	}
}
