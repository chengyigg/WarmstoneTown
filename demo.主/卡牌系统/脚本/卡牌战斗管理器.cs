using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using 你的项目.Scripts.管理器;
using 你的项目.Scripts.资源;

// ★ 枚举定义（必须放在类之前）
public enum 战斗阶段
{
	空闲,          // 行动条累积中，无人行动
	玩家回合,      // 玩家可以操作
	敌人回合,      // 敌人在行动
	过场动画,      // 显示过场文字/转场
	战斗结束       // 战斗已结束
}

public partial class 卡牌战斗管理器 : Node
{
	// ----- 原有所有字段、属性、事件（不变）-----
	public 卡牌战斗单位 玩家单位 { get; private set; }
	public List<卡牌战斗单位> 怪物队伍 { get; private set; } = new();
	public bool 允许强制玩家动作 = false;
	public bool 禁止结束玩家行动临时 = false;
	public bool 战斗进行中 { get; private set; } = false;
	public static 卡牌战斗管理器 实例 { get; private set; }

	[Export] public CanvasLayer 提示层;
	[Export] private Font 过场文字字体;
	[Export] private int 过场文字字号 = 36;
	[Export] public 敌人手牌管理器 敌人手牌管理器;
	[Export] private Button 强化模式按钮;
	[Export] public 卡牌使用记录管理器 卡牌使用记录管理器;

	public Func<卡牌战斗单位, 卡牌实例> 自定义抽牌逻辑 { get; set; } = null;

	[Export] private Label 玩家生命值显示;
	[Export] private Label 玩家护盾显示;
	[Export] private Label 玩家手牌数量显示;
	[Export] private Label 玩家卡组数量显示;
	[Export] private ProgressBar 玩家行动条显示;
	[Export] private Label 战斗日志;
	[Export] private Button 结束回合按钮;
	[Export] private HBoxContainer 能量槽容器;
	[Export] private PackedScene 能量格预制体;

	public Action<卡牌战斗单位> 抽牌事件;
	public Action<卡牌战斗单位, 卡牌实例> 打出攻击牌事件;
	public Action<卡牌战斗单位> 敌方行动事件;
	public Action<bool> 强化模式切换事件;
	public Action<卡牌战斗单位, int> 单位受伤事件;

	public Action<卡牌战斗单位> 请求更新手牌UI;
	public Func<卡牌战斗单位, 卡牌实例, Task> 请求播放卡牌动画;
	public Action<卡牌战斗单位, 卡牌实例> 请求移除单张卡牌UI;

	private Timer _行动条计时器;
	private 战斗阶段 _当前阶段 = 战斗阶段.空闲;
	private int 当前能量 = 0;
	private const int 最大能量 = 3;
	private bool _是否强化模式 = false;
	public bool 是否强化模式 => _是否强化模式;

	private List<ColorRect> 能量格列表 = new();
	private readonly Color 未激活颜色 = new Color(1, 1, 0);
	private readonly Color 已激活颜色 = new Color(1, 0, 0);
	private readonly Color 灰色颜色 = new Color(0.5f, 0.5f, 0.5f);

	[Signal] public delegate void 战斗结束EventHandler(bool 玩家胜利);

	// ----- 原有 _Ready、初始化等（不变）-----
	public override void _Ready()
	{
		if (提示层 == null)
			GD.PrintErr("提示层未设置，请在检查器中拖入一个 CanvasLayer 节点");
		初始化能量槽();
		实例 = this;
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

	public void 设置战斗单位(卡牌战斗单位 玩家, List<卡牌战斗单位> 怪物列表)
	{
		玩家单位 = 玩家;
		怪物队伍 = 怪物列表;
	}

	private async void 显示过场文字(string 文字, Color 文字颜色, Action 完成回调)
	{
		try
		{
			if (!战斗进行中) return;
			if (提示层 == null)
			{
				GD.PrintErr("提示层未设置，无法显示过场文字！");
				完成回调?.Invoke();
				return;
			}

			GD.Print($"[过场] 显示文字: {文字}");
			_当前阶段 = 战斗阶段.过场动画;

			var 标签 = new Label();
			标签.Text = 文字;
			标签.HorizontalAlignment = HorizontalAlignment.Center;
			标签.VerticalAlignment = VerticalAlignment.Center;
			if (过场文字字体 != null)
				标签.AddThemeFontOverride("font", 过场文字字体);
			标签.AddThemeFontSizeOverride("font_size", 过场文字字号);
			标签.AddThemeColorOverride("font_color", 文字颜色);
			标签.AddThemeColorOverride("font_outline_color", Colors.Black);
			标签.AddThemeConstantOverride("outline_size", 2);
			标签.MouseFilter = Control.MouseFilterEnum.Ignore;

			提示层.AddChild(标签);
			标签.Size = 标签.GetMinimumSize();

			var 视口大小 = GetViewport().GetVisibleRect().Size;
			Vector2 目标位置 = new Vector2(
				(视口大小.X - 标签.Size.X) / 2,
				(视口大小.Y - 标签.Size.Y) / 2 - 100
			);
			Vector2 初始位置 = 目标位置 + new Vector2(0, 40);
			标签.Position = 初始位置;
			标签.Modulate = new Color(1, 1, 1, 0);
			标签.ZIndex = 100;

			var 入场动画 = CreateTween();
			入场动画.SetParallel(true);
			入场动画.TweenProperty(标签, "position", 目标位置, 0.3f)
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out);
			入场动画.TweenProperty(标签, "modulate:a", 1f, 0.2f);
			await ToSignal(入场动画, Tween.SignalName.Finished);

			await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

			var 退场动画 = CreateTween();
			退场动画.TweenProperty(标签, "modulate:a", 0f, 0.3f);
			await ToSignal(退场动画, Tween.SignalName.Finished);

			标签.QueueFree();
			_当前阶段 = 战斗阶段.空闲;
			GD.Print($"[过场] 文字结束: {文字}");
			完成回调?.Invoke();
		}
		catch (Exception e)
		{
			GD.PrintErr($"[过场文字] 发生严重错误: {e.Message}\n{e.StackTrace}");
			_当前阶段 = 战斗阶段.空闲;
			完成回调?.Invoke();
		}
	}

	public void 开始战斗()
	{
		GD.Print("=== 开始战斗 ===");
		战斗进行中 = true;
		_当前阶段 = 战斗阶段.空闲;

		玩家单位.抽起始手牌(2);
		foreach (var 怪物 in 怪物队伍)
			GD.Print($"{怪物.名字} 初始无手牌");

		var 主场景 = GetParent() as 卡牌游戏主场景;
		主场景?.玩家手牌管理器?.更新手牌显示();

		_行动条计时器 = new Timer();
		_行动条计时器.WaitTime = 0.05f;
		_行动条计时器.Timeout += 更新所有单位行动条;
		AddChild(_行动条计时器);
		_行动条计时器.Start();

		设置能量(1);
		_是否强化模式 = false;
		卡牌使用记录管理器?.清空记录();
		清空战斗日志();
		添加战斗日志("战斗开始！行动条决定行动顺序。");
		更新玩家UI();
	}

	private void 更新玩家UI()
	{
		if (玩家生命值显示 != null)
			玩家生命值显示.Text = $"生命值: {玩家单位.生命值}";
		if (玩家护盾显示 != null)
			玩家护盾显示.Text = $"护盾: {玩家单位.护盾值}";
		if (玩家手牌数量显示 != null)
			玩家手牌数量显示.Text = $"手牌: {玩家单位.手牌.Count}";
		if (玩家卡组数量显示 != null)
			玩家卡组数量显示.Text = $"卡组: {玩家单位.抽牌堆.Count}";
		if (玩家行动条显示 != null)
			玩家行动条显示.Value = Mathf.Clamp(玩家单位.行动条, 0, 100);
	}

	private void 更新行动条显示()
	{
		if (玩家行动条显示 != null)
			玩家行动条显示.Value = Mathf.Clamp(玩家单位.行动条, 0, 100);
	}

	public void 更新UI()
	{
		更新玩家UI();
	}

	public bool 玩家是否可以行动()
	{
		return _当前阶段 == 战斗阶段.玩家回合;
	}

	private void 更新所有单位行动条()
	{
		if (!战斗进行中) return;
		if (_当前阶段 != 战斗阶段.空闲) return;

		玩家单位.行动条 += 玩家单位.速度值 * 0.05f;
		foreach (var 怪物 in 怪物队伍)
			if (怪物.生命值 > 0)
				怪物.行动条 += 怪物.速度值 * 0.05f;

		更新行动条显示();

		var 主场景 = GetParent() as 卡牌游戏主场景;
		if (主场景 != null)
		{
			for (int i = 0; i < 怪物队伍.Count && i < 主场景.当前怪物视觉列表.Count; i++)
			{
				var 视觉 = 主场景.当前怪物视觉列表[i];
				if (视觉.行动条 != null)
					视觉.行动条.Value = 怪物队伍[i].行动条;
			}
		}

		卡牌战斗单位 行动者 = null;
		if (玩家单位.行动条 >= 100) 行动者 = 玩家单位;
		else
		{
			foreach (var 怪物 in 怪物队伍)
			{
				if (怪物.行动条 >= 100)
				{
					行动者 = 怪物;
					break;
				}
			}
		}

		if (行动者 != null)
		{
			GD.Print($"[行动条] 即将显示过场文字, 行动者={行动者.名字}");
			if (行动者 == 玩家单位)
			{
				显示过场文字("你的回合", Colors.LimeGreen, () => {
					清空战斗日志();
					_当前阶段 = 战斗阶段.玩家回合;
					GD.Print($"[行动条] 玩家获得行动权，行动条={玩家单位.行动条}");
					添加战斗日志("轮到玩家行动！");
					通知玩家卡牌交互启用(true);

				});
			}
			else
			{
				string 敌人名 = 行动者.名字;
				显示过场文字($"{敌人名}的回合", Colors.Red, () => {
					清空战斗日志();
					_当前阶段 = 战斗阶段.敌人回合;
					GD.Print($"[行动条] {敌人名} 获得行动权，行动条={行动者.行动条}");
					_ = 执行敌人单个动作(行动者);
				});
			}
		}
	}

	public void 设置教程模式(bool 启用)
	{
		if (结束回合按钮 != null)
			结束回合按钮.Disabled = 启用;
		if (强化模式按钮 != null)
			强化模式按钮.Disabled = 启用;
	}

	public void 设置过场动画中(bool 是否动画)
	{
		_当前阶段 = 是否动画 ? 战斗阶段.过场动画 : 战斗阶段.空闲;
	}

	public void 设置玩家交互启用(bool 启用)
	{
		通知玩家卡牌交互启用(启用);
	}

	public void 禁止结束回合(bool 禁止)
	{
		if (结束回合按钮 != null)
			结束回合按钮.Disabled = 禁止;
	}

	public void 禁止强化模式(bool 禁止)
	{
		if (强化模式按钮 != null)
			强化模式按钮.Disabled = 禁止;
	}

	private async Task 执行敌人单个动作(卡牌战斗单位 怪物)
	{
		try
		{
			敌方行动事件?.Invoke(怪物);
			var 随机 = new Random();
			bool 执行了动作 = false;

			if (怪物.手牌.Count == 0)
			{
				执行了动作 = await 尝试抽牌动作(怪物);
			}
			else
			{
				var 可用牌 = new List<卡牌实例>();
				foreach (var 卡牌 in 怪物.手牌)
					if (卡牌.基础数据.行动值消耗 <= 怪物.行动条)
						可用牌.Add(卡牌);
				if (可用牌.Count > 0)
				{
					var 选中的牌 = 可用牌[随机.Next(可用牌.Count)];
					执行了动作 = await 尝试打出卡牌(怪物, 选中的牌);
				}
				else
				{
					执行了动作 = await 尝试抽牌动作(怪物);
				}
			}

			if (!执行了动作)
				添加战斗日志($"{怪物.名字} 无法行动，跳过");

			if (怪物.手牌.Count > 5)
			{
				int 弃牌数 = 怪物.手牌.Count - 5;
				for (int i = 0; i < 弃牌数; i++)
				{
					int 随机索引 = 随机.Next(怪物.手牌.Count);
					var 弃牌 = 怪物.手牌[随机索引];
					怪物.弃牌(弃牌);
					添加战斗日志($"{怪物.名字} 弃置 {弃牌.基础数据.卡牌名称} (手牌超过5张)");
				}
				请求更新手牌UI?.Invoke(怪物);
			}

			_当前阶段 = 战斗阶段.空闲;
		}
		catch (Exception e)
		{
			GD.PrintErr($"[敌人动作] {怪物.名字} 执行失败: {e.Message}\n{e.StackTrace}");
			_当前阶段 = 战斗阶段.空闲;
			请求更新手牌UI?.Invoke(怪物);
		}
	}

	private async Task<bool> 尝试抽牌动作(卡牌战斗单位 单位)
	{
		int 消耗 = 抽牌动作.行动值消耗;
		if (单位.行动条 < 消耗)
		{
			GD.Print($"[抽牌] {单位.名字} 行动值不足");
			return false;
		}
		GD.Print($"[抽牌] {单位.名字} 行动条 {单位.行动条} -> 扣除{消耗}");
		单位.行动条 -= 消耗;
		if (单位 == 玩家单位)
			更新行动条显示();
		抽牌事件?.Invoke(单位);
		bool 成功 = 抽牌动作.执行(单位, this);
		if (成功)
		{
			添加战斗日志($"{单位.名字} 抽了一张牌");
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		}

		if (单位 == 玩家单位 && 单位.行动条 < 100)
			结束玩家行动();

		return 成功;
	}

	private async Task<bool> 尝试打出卡牌(卡牌战斗单位 单位, 卡牌实例 卡牌)
	{
		// 仅由敌人AI调用
		int 消耗 = 卡牌.基础数据.行动值消耗;
		if (单位.行动条 < 消耗)
		{
			GD.Print($"[出牌] {单位.名字} 行动值不足");
			return false;
		}
		GD.Print($"[出牌] {单位.名字} 行动条 {单位.行动条} -> 扣除{消耗}");
		单位.行动条 -= 消耗;

		await 请求播放卡牌动画?.Invoke(单位, 卡牌);

		执行卡牌效果(单位, 玩家单位, 卡牌);
		单位.使用卡牌(卡牌);
		请求移除单张卡牌UI?.Invoke(单位, 卡牌);

		return true;
	}

	public void 玩家主动打出卡牌(卡牌实例 卡牌)
	{
		执行玩家使用卡牌(卡牌, null);
	}

	public async void 玩家主动抽牌()
	{
		try
		{
			if (!玩家是否可以行动())
				return;
			if (玩家单位.行动条 < 抽牌动作.行动值消耗)
			{
				GD.Print($"[抽牌] 行动值不足");
				return;
			}
			await 尝试抽牌动作(玩家单位);
			if (!禁止结束玩家行动临时 && 玩家单位.行动条 < 100)
				结束玩家行动();
		}
		catch (Exception e)
		{
			GD.PrintErr($"[玩家抽牌] 发生错误: {e.Message}\n{e.StackTrace}");
			if (_当前阶段 == 战斗阶段.玩家回合)
				结束玩家行动();
		}
	}

	public void 结束玩家行动()
	{
		if (_当前阶段 == 战斗阶段.玩家回合)
		{
			_当前阶段 = 战斗阶段.空闲;
			通知玩家卡牌交互启用(false);
			添加战斗日志("玩家结束行动，行动条恢复累加");
		}
	}

	// 兼容旧调用
	public void 结束玩家回合()
	{
		结束玩家行动();
	}

	public void 结束怪物回合()
	{
		if (_当前阶段 == 战斗阶段.敌人回合)
			_当前阶段 = 战斗阶段.空闲;
	}

	public void 暂停自动行动条()
	{
		if (_行动条计时器 != null && _行动条计时器.IsInsideTree())
			_行动条计时器.Stop();
	}

	public void 恢复自动行动条()
	{
		if (_行动条计时器 != null && !_行动条计时器.IsInsideTree())
			AddChild(_行动条计时器);
		if (_行动条计时器 != null && !_行动条计时器.IsStopped())
			_行动条计时器.Start();
	}

	public void 通知玩家卡牌交互启用(bool 启用)
	{
		GD.Print($"[DEBUG] 通知玩家卡牌交互启用: {启用}");
		var 主场景 = GetParent() as 卡牌游戏主场景;
		if (主场景?.玩家手牌管理器 == null)
		{
			GD.PrintErr("[DEBUG] 主场景或玩家手牌管理器为空");
			return;
		}
		int 卡牌数量 = 主场景.玩家手牌管理器.当前卡牌UI列表.Count;
		GD.Print($"[DEBUG] 找到 {卡牌数量} 张卡牌UI，设置禁用状态为 {!启用}");
		foreach (var ui in 主场景.玩家手牌管理器.当前卡牌UI列表)
			ui.设置禁用状态(!启用);
	}

	public void 添加战斗日志(string 日志)
	{
		GD.Print($"[战斗日志] {日志}");
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

	private void 清空战斗日志()
	{
		if (战斗日志 != null)
			战斗日志.Text = "";
	}

	private void 设置能量(int 新值)
	{
		当前能量 = Mathf.Clamp(新值, 0, 最大能量);
		for (int i = 0; i < 能量格列表.Count; i++)
		{
			if (i < 当前能量)
				能量格列表[i].Color = _是否强化模式 ? 已激活颜色 : 未激活颜色;
			else
				能量格列表[i].Color = 灰色颜色;
		}
		var 主场景 = GetParent() as 卡牌游戏主场景;
		主场景?.玩家手牌管理器?.刷新所有卡牌高光();
	}

	public void 增加能量(int 数量) => 设置能量(当前能量 + 数量);
	private void 消耗能量(int 数量) => 设置能量(当前能量 - 数量);

	private void 执行玩家使用卡牌(卡牌实例 卡牌, 卡牌战斗单位 目标 = null)
	{
		if (!玩家是否可以行动())
		{
			GD.Print("[执行玩家使用卡牌] 当前不可行动，忽略");
			return;
		}

		if (玩家单位.行动条 < 卡牌.基础数据.行动值消耗)
		{
			添加战斗日志("行动值不足！");
			return;
		}

		if (目标 == null)
		{
			foreach (var 怪物 in 怪物队伍)
			{
				if (怪物.生命值 > 0)
				{
					目标 = 怪物;
					break;
				}
			}
			if (目标 == null)
			{
				添加战斗日志("没有可攻击的目标！");
				return;
			}
		}

		玩家单位.行动条 -= 卡牌.基础数据.行动值消耗;
		更新行动条显示();

		执行卡牌效果(玩家单位, 目标, 卡牌);

		if (_是否强化模式)
			消耗能量(1);

		卡牌使用记录管理器?.添加记录(卡牌, true);

		var 主场景 = GetParent() as 卡牌游戏主场景;

		if (卡牌.基础数据.类型 == 卡牌数据.卡牌类型.装备)
		{
			if (玩家单位.手牌.Contains(卡牌))
				玩家单位.手牌.Remove(卡牌);
			装备管理器.实例?.添加装备(玩家单位, 卡牌, 卡牌.基础数据.卡牌名称);
			主场景?.玩家手牌管理器?.移除卡牌(卡牌);
		}
		else
		{
			玩家单位.使用卡牌(卡牌);
		}

		if (卡牌.基础数据.类型 == 卡牌数据.卡牌类型.攻击)
		{
			装备管理器.实例?.通知攻击使用(玩家单位);
			打出攻击牌事件?.Invoke(玩家单位, 卡牌);
		}

		更新UI();
		主场景?.玩家手牌管理器?.播放使用卡牌动画(卡牌);

		if (玩家单位.行动条 < 100)
			结束玩家行动();

		if (所有怪物死亡())
			结束战斗(true);
	}

	public void 切换强化模式()
	{
		if (!玩家是否可以行动())
		{
			GD.Print("[切换强化模式] 当前不可操作");
			return;
		}
		if (!战斗进行中) return;
		if (_是否强化模式)
			关闭强化模式();
		else
		{
			if (当前能量 <= 0)
			{
				添加战斗日志("能量不足，无法启动强化模式！");
				return;
			}
			开启强化模式();
		}
	}

	private void 开启强化模式()
	{
		_是否强化模式 = true;
		添加战斗日志("★ 强化模式启动！卡牌效果已增强 ★");
		if (强化模式按钮 != null)
			强化模式按钮.Disabled = false;
		var 主场景 = GetParent() as 卡牌游戏主场景;
		主场景?.玩家手牌管理器?.刷新所有卡牌描述(true);
		主场景?.玩家手牌管理器?.刷新所有卡牌高光();
		设置能量(当前能量);
		强化模式切换事件?.Invoke(true);
	}

	public void 关闭强化模式()
	{
		if (!_是否强化模式) return;
		_是否强化模式 = false;
		添加战斗日志("强化模式已关闭。");
		if (强化模式按钮 != null)
			强化模式按钮.Disabled = false;
		var 主场景 = GetParent() as 卡牌游戏主场景;
		主场景?.玩家手牌管理器?.刷新所有卡牌描述(false);
		主场景?.玩家手牌管理器?.刷新所有卡牌高光();
		设置能量(当前能量);
	}

	public void 玩家使用卡牌(卡牌实例 卡牌, 卡牌战斗单位 指定目标 = null)
	{
		执行玩家使用卡牌(卡牌, 指定目标);
	}

	public void 执行卡牌效果(卡牌战斗单位 使用者, 卡牌战斗单位 目标, 卡牌实例 卡牌)
	{
		if (卡牌效果管理器.实例 != null)
			卡牌效果管理器.实例.执行卡牌效果(卡牌, 使用者, 目标, this);
		else
			GD.PrintErr("卡牌效果管理器实例为空！");
	}

  // ★★★★★ 修改2：造成伤害（增加参数，统一处理伤害、护盾、Buff、下限）★★★★★
	public void 造成伤害(卡牌战斗单位 使用者, 卡牌战斗单位 目标, int 伤害, bool 无视护甲 = false, bool 无视减伤 = false)
	{
		// 1. 应用观察弱点加成（仅玩家使用时）
		if (使用者 == 玩家单位 && 观察弱点状态.玩家下次攻击伤害加成 > 0)
		{
			伤害 += 观察弱点状态.玩家下次攻击伤害加成;
			添加战斗日志($"观察弱点加成 +{观察弱点状态.玩家下次攻击伤害加成} 伤害");
			观察弱点状态.重置(); // 重置加成
		}

		// 2. 处理减伤（Buff）
		var buff系统 = GetNodeOrNull<Buff系统>("Buff系统");
		if (!无视减伤 && buff系统 != null)
		{
			if (buff系统.有Buff(目标, "消散"))
			{
				伤害 = Mathf.FloorToInt(伤害 / 2f);
				buff系统.消耗Buff(目标, "消散");
				添加战斗日志("消散效果：伤害减半");
			}
			if (buff系统.有Buff(目标, "架势"))
			{
				伤害 = Mathf.FloorToInt(伤害 / 2f);
				添加战斗日志("架势效果：伤害减半");
			}
		}

		// 3. 处理护盾（若无视护甲则跳过护盾吸收）
		int 实际伤害 = 伤害;
		if (!无视护甲 && 目标.护盾值 > 0)
		{
			int 剩余护盾 = 目标.护盾值 - 伤害;
			if (剩余护盾 >= 0)
			{
				目标.护盾值 = 剩余护盾;
				实际伤害 = 0;
				添加战斗日志($"护盾吸收了全部 {伤害} 点伤害");
			}
			else
			{
				实际伤害 = -剩余护盾; // 伤害减去护盾值
				目标.护盾值 = 0;
				添加战斗日志($"护盾吸收了 {伤害 - 实际伤害} 点伤害，剩余 {实际伤害} 点穿透");
			}
		}

		// 4. 造成实际伤害（强制下限为0）
		if (实际伤害 > 0)
		{
			目标.生命值 -= 实际伤害;
			if (目标.生命值 < 0) 目标.生命值 = 0;
			添加战斗日志($"对 {目标.名字} 造成 {实际伤害} 点伤害");
		}

		// 5. 触发受伤事件
		单位受伤事件?.Invoke(目标, 实际伤害);

		// 6. 更新UI
		var 主场景 = GetParent() as 卡牌游戏主场景;
		主场景?.更新所有怪物UI();
		更新UI();
	}

	public void 强制暂停战斗()
	{
		_当前阶段 = 战斗阶段.过场动画;
		if (_行动条计时器 != null)
			_行动条计时器.Stop();
		通知玩家卡牌交互启用(false);
		设置教程模式(true);
	}

	public void 强制恢复战斗()
	{
		_当前阶段 = 战斗阶段.空闲;
		if (_行动条计时器 != null)
			_行动条计时器.Start();
		通知玩家卡牌交互启用(true);
		设置教程模式(false);
	}

	public void 强制开始玩家回合()
	{
		GD.Print($"[DEBUG] 强制开始玩家回合 - 当前阶段={_当前阶段}");
		_当前阶段 = 战斗阶段.玩家回合;
		通知玩家卡牌交互启用(true);
		添加战斗日志("轮到玩家行动！(强制)");
	}

	public void 获得护盾(卡牌战斗单位 单位, int 护盾值)
	{
		单位.护盾值 += 护盾值;
		添加战斗日志($"{单位.名字} 获得 {护盾值} 点护盾");
		if (单位 == 玩家单位)
			更新UI();
		else
		{
			var 主场景 = GetParent() as 卡牌游戏主场景;
			主场景?.更新所有怪物UI();
		}
	}

	private bool 所有怪物死亡()
	{
		foreach (var 怪物 in 怪物队伍)
			if (怪物.生命值 > 0) return false;
		return true;
	}

 // ★★★★★ 修改1：结束战斗（移除隐藏触发器查找）★★★★★
	public void 结束战斗(bool 玩家胜利)
	{
		战斗进行中 = false;
		_当前阶段 = 战斗阶段.战斗结束;
		_行动条计时器?.Stop();
		添加战斗日志(玩家胜利 ? "战斗胜利！" : "战斗失败...");

		if (!玩家胜利)
		{
			EmitSignal(SignalName.战斗结束, false);
			return;
		}

		// ★ 移除所有查找触发器的代码（已移至对话播放器）

		_处理胜利奖励();
		_处理对话序列改变();
		_处理击杀进度();
		_处理返回对话();

		EmitSignal(SignalName.战斗结束, true);
	}

	private void _处理胜利奖励()
	{
		var 奖励道具 = 卡牌数据管理器.上下文.待奖励道具列表;
		if (奖励道具 == null || 奖励道具.Count == 0) return;

		foreach (var 道具 in 奖励道具)
		{
			if (道具 == null) continue;
			var 克隆 = 道具.克隆();
			玩家道具管理器.实例?.添加道具(克隆);
			GD.Print($"[战斗管理器] 添加奖励道具: {克隆.名称} x{克隆.数量}");
		}
		卡牌数据管理器.上下文.待奖励道具列表 = null;
	}

	private void _处理对话序列改变()
	{
		var 待改变序列 = 卡牌数据管理器.上下文.待改变对话序列;
		if (待改变序列 == null) return;

		var 当前状态 = 存档管理器.实例?.获取当前状态();
		if (当前状态 != null)
		{
			当前状态.设置对话改变状态(待改变序列.ResourcePath, true);
			GD.Print($"[战斗管理器] 标记对话序列为已改变: {待改变序列.ResourcePath}");
		}
		卡牌数据管理器.上下文.待改变对话序列 = null;
	}

	private void _处理击杀进度()
	{
		foreach (var 怪物 in 怪物队伍)
		{
			if (怪物.生命值 > 0) continue;
			任务管理器.实例?.更新击杀进度(怪物.名字);
		}
	}

	private void _处理返回对话()
	{
		var 返回对话 = 卡牌数据管理器.上下文.战斗后返回对话序列;
		if (返回对话 == null) return;

		卡牌数据管理器.上下文.返回后自动触发对话序列 = 返回对话;
		卡牌数据管理器.上下文.战斗后返回对话序列 = null;
		GD.Print($"[战斗管理器] 设置返回后自动触发对话: {返回对话.ResourcePath}");
	}

	public bool 卡牌是否可用(卡牌实例 卡牌)
	{
		if (_当前阶段 != 战斗阶段.玩家回合) return false;
		if (玩家单位.行动条 < 卡牌.基础数据.行动值消耗) return false;
		if (_是否强化模式 && 当前能量 <= 0) return false;
		return true;
	}
}
