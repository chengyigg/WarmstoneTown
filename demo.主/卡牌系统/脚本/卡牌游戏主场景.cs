using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class 卡牌游戏主场景 : Node2D
{
	[Export] private 卡牌战斗管理器 战斗管理器;
	[Export] public 手牌管理器 玩家手牌管理器;
	[Export] private 生成卡组资源 玩家卡组资源;
	[Export] private PackedScene 怪物视觉预制体;
	[Export] private PackedScene 敌人手牌管理器预制体;
	[Export] private 目标选择器 目标选择器;
	[Export] public 教程战斗控制器 教程战斗控制器;

	public List<怪物视觉数据> 当前怪物视觉列表 = new();
	private 卡牌战斗单位 玩家单位;
	private List<卡牌战斗单位> _怪物单位列表;
	public class 怪物视觉数据
	{
		public 卡牌战斗单位 战斗单位;
		public Control 视觉节点;
		public 敌人手牌管理器 手牌管理器;
		public ProgressBar 血条;
		public Label 护盾标签;
		public Label 生命值标签;
		public int 最大生命值;
		public ProgressBar 行动条;
	}

public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.R)
		{
			if (战斗管理器 != null && 战斗管理器.战斗进行中)
				战斗管理器.玩家主动抽牌();
		}
	}

public override async void _Ready()
{  // ★ 调试：确认战斗场景分组
	GD.Print($"[战斗场景] 是否在战斗场景组: {IsInGroup("战斗场景")}");
	try
	{
		GD.Print("=== 卡牌游戏主场景 _Ready 开始 ===");

		if (玩家卡组资源 == null)
		{
			GD.PrintErr("玩家卡组资源为空！");
			return;
		}

		var 玩家卡组数组 = 玩家卡组资源.卡组;
		var 队伍 = 卡牌数据管理器.上下文.临时怪物队伍;
		if (队伍 == null || 队伍.怪物列表.Count == 0)
		{
			GD.PrintErr("没有怪物队伍配置！");
			return;
		}

		// 创建玩家单位
		玩家单位 = new 卡牌战斗单位();
		玩家单位.名字 = "玩家";
		玩家单位.生命值 = 50;
		玩家单位.速度值 = 80;
		玩家单位.初始化卡组(玩家卡组数组);
		玩家单位.战斗管理器 = 战斗管理器;   // ★ 新增这一行
		玩家手牌管理器?.初始化(玩家单位, 战斗管理器);

		// 创建怪物单位列表
		_怪物单位列表 = new List<卡牌战斗单位>();
		foreach (var 配置 in 队伍.怪物列表)
		{
			var 怪物单位 = new 卡牌战斗单位();
			怪物单位.名字 = 配置.名字;
			怪物单位.生命值 = 配置.最大生命值;
			怪物单位.护盾值 = 配置.初始护盾;
			怪物单位.速度值 = 65;
			怪物单位.初始化卡组(配置.卡组.卡组);
			怪物单位.战斗管理器 = 战斗管理器;   // ★ 新增
			_怪物单位列表.Add(怪物单位);
		}

		if (_怪物单位列表.Count == 0)
		{
			GD.PrintErr("没有有效的怪物单位");
			return;
		}

		// 生成UI
		动态生成怪物UI(_怪物单位列表, 队伍.怪物列表);

		var 敌人视觉列表 = new List<(卡牌战斗单位, Node)>();
		foreach (var 视觉 in 当前怪物视觉列表)
			敌人视觉列表.Add((视觉.战斗单位, 视觉.视觉节点));
		目标选择器?.初始化(敌人视觉列表, 目标选择器);

		// 注册委托
		战斗管理器.请求更新手牌UI = (怪物) =>
		{
			var 视觉 = 当前怪物视觉列表.Find(v => v.战斗单位 == 怪物);
			视觉?.手牌管理器.刷新手牌();
		};
		战斗管理器.请求移除单张卡牌UI = (怪物, 卡牌) =>
		{
			var 视觉 = 当前怪物视觉列表.Find(v => v.战斗单位 == 怪物);
			视觉?.手牌管理器.移除卡牌UI(卡牌);
		};
		战斗管理器.请求播放卡牌动画 = async (怪物, 卡牌) =>
		{
			var 视觉 = 当前怪物视觉列表.Find(v => v.战斗单位 == 怪物);
			if (视觉?.手牌管理器 != null)
				await 视觉.手牌管理器.播放卡牌展示动画(卡牌, 战斗管理器.卡牌使用记录管理器);
		};

		// 设置战斗单位
		战斗管理器.设置战斗单位(玩家单位, _怪物单位列表);
	}
	catch (Exception e)
	{
		GD.PrintErr($"初始化异常: {e.Message}\n{e.StackTrace}");
		return;
	}

	// 根据是否需要教程来决定是否立即开始战斗
	if (卡牌数据管理器.上下文.是否启动教程战斗 && 教程战斗控制器 != null)
	{
		// 教程战斗：由教程控制器内部调用开始战斗并暂停
		教程战斗控制器.开始教程战斗(卡牌数据管理器.上下文.待启动的教程战斗配置, 玩家单位, _怪物单位列表);
		卡牌数据管理器.上下文.待启动的教程战斗配置 = null;
	}
	else
	{
		// 普通战斗：直接开始
		战斗管理器.开始战斗();
		// ★ 强制初始手牌显示高光循环动画
if (玩家手牌管理器 != null)
{
	await ToSignal(GetTree(), "process_frame"); // 等待一帧确保手牌UI已创建
	玩家手牌管理器.强制启用所有高光();
}
	}


}

	public void 更新所有怪物UI()
	{
		foreach (var 视觉 in 当前怪物视觉列表)
		{
			视觉.血条.Value = 视觉.战斗单位.生命值;
			视觉.护盾标签.Text = $"🛡️ {视觉.战斗单位.护盾值}";
			if (视觉.生命值标签 != null)
				视觉.生命值标签.Text = $"{视觉.战斗单位.生命值}/{视觉.最大生命值}";
		}
	}

	private void 动态生成怪物UI(List<卡牌战斗单位> 怪物单位列表, Godot.Collections.Array<怪物配置> 配置列表)
	{
		int 数量 = 怪物单位列表.Count;
		float 屏幕宽 = GetViewportRect().Size.X;
		float 容器宽度 = 200f;
		float 间距 = 250f;
		float 总宽度 = 数量 * 容器宽度 + (数量 - 1) * 间距;
		float 起始X = 屏幕宽 / 2 - 总宽度 / 2;
		if (数量 == 1) 起始X = 屏幕宽 / 2 - 容器宽度 / 2;

		for (int i = 0; i < 数量; i++)
		{
			var 单位 = 怪物单位列表[i];
			var 配置 = 配置列表[i];

			var 容器 = 怪物视觉预制体.Instantiate<Control>();
			容器.Position = new Vector2(起始X + i * (容器宽度 + 间距), 100);
			AddChild(容器);

			var 图片 = 容器.GetNode<TextureRect>("图片");
			图片.Texture = 配置.怪物图片;
			var 名字标签 = 容器.GetNode<Label>("名字");
			名字标签.Text = 配置.名字;
			var 血条 = 容器.GetNode<ProgressBar>("血条");
			血条.MaxValue = 单位.生命值;
			血条.Value = 单位.生命值;
			var 血量数字 = 容器.GetNode<Label>("血量数字");
			血量数字.Text = $"{单位.生命值}/{单位.生命值}";
			var 护盾标签 = 容器.GetNode<Label>("护盾标签");
			护盾标签.Text = $"🛡️ {单位.护盾值}";

			var 行动条节点 = 容器.GetNode<ProgressBar>("行动条");
			if (行动条节点 != null)
			{
				行动条节点.MaxValue = 100;
				行动条节点.Value = 单位.行动条;
			}

			var 手牌锚点 = 容器.GetNode<Control>("手牌锚点");
			var 手牌管理器实例 = 敌人手牌管理器预制体.Instantiate<敌人手牌管理器>();
			手牌管理器实例.初始化(单位);
			手牌锚点.AddChild(手牌管理器实例);
			手牌管理器实例.Position = Vector2.Zero;
			float 锚点宽 = 手牌锚点.Size.X == 0 ? 200 : 手牌锚点.Size.X;
			手牌管理器实例.设置手牌区域宽度(锚点宽);

			当前怪物视觉列表.Add(new 怪物视觉数据
			{
				战斗单位 = 单位,
				视觉节点 = 容器,
				手牌管理器 = 手牌管理器实例,
				血条 = 血条,
				护盾标签 = 护盾标签,
				生命值标签 = 血量数字,
				最大生命值 = 单位.生命值,
				行动条 = 行动条节点
			});
		}
	}
}
