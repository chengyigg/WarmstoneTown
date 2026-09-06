using Godot;
using System.Collections.Generic;

public partial class 背包界面 : Control
{
	public static 背包界面 Instance { get; private set; }
	
	[Export] private PackedScene 物品槽场景;
	[Export] private Godot.Collections.Array<物品资源> 初始物品 = new Godot.Collections.Array<物品资源>();

	private GridContainer 物品网格;
	private Label 信息标签;
	private Label 选中物品标签;
	
	private List<物品槽> 所有物品槽 = new List<物品槽>();

	public override void _Ready()
	{
		// 设置单例实例
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			QueueFree();
			return;
		}
		
		// 设置高Z索引确保在最前面
		ZIndex = 1000;
		
		// 获取子节点
		物品网格 = GetNode<GridContainer>("MainContainer/ItemGrid");
		信息标签 = GetNode<Label>("MainContainer/InfoLabel");
		选中物品标签 = GetNode<Label>("MainContainer/SelectedItemLabel");

		// 初始隐藏背包
		Visible = false;
		
		初始化背包();
		更新信息文本();
	}

	public override void _Input(InputEvent @event)
	{
		// 处理背包开关逻辑
		if (@event.IsActionPressed("打开背包"))
		{
			切换背包显示();
			GetViewport().SetInputAsHandled();
		}
	}

	private void 切换背包显示()
	{
		Visible = !Visible;
		
		if (Visible)
		{
			// 显示背包时暂停游戏
			GetTree().Paused = true;
			ProcessMode = ProcessModeEnum.WhenPaused;
			GrabFocus();  // 确保背包获得焦点
		}
		else
		{
			// 隐藏背包时恢复游戏
			GetTree().Paused = false;
			ProcessMode = ProcessModeEnum.Inherit;
		}
	}

	private void 初始化背包()
	{
		// 清理现有槽位
		foreach (Node 子节点 in 物品网格.GetChildren())
		{
			子节点.QueueFree();
		}
		所有物品槽.Clear();

		int 总槽位数 = 8;
		
		// 创建新槽位
		for (int i = 0; i < 总槽位数; i++)
		{
			物品槽 新槽位 = 物品槽场景.Instantiate<物品槽>();
			物品网格.AddChild(新槽位);
			所有物品槽.Add(新槽位);

			新槽位.物品移动事件 += 处理物品移动;
			新槽位.物品选中事件 += 处理物品选中;

			// 设置初始物品
			if (i < 初始物品.Count && 初始物品[i] != null)
			{
				新槽位.设置物品(初始物品[i]);
			}
			else
			{
				新槽位.设置物品(null);
			}
		}
		更新信息文本();
	}

	private void 处理物品移动(物品槽 来源槽位, 物品槽 目标槽位)
	{
		来源槽位.与槽位交换(目标槽位);
		更新信息文本();
	}
	
	private void 处理物品选中(物品资源 物品)
	{
		选中物品标签.Text = $"选中物品: {物品.物品名称}\n" +
						$"攻击力: {物品.攻击力}\n" +
						$"防御力: {物品.防御力}\n" +
						$"生命值: {物品.生命值}\n" +
						$"速度: {物品.速度}";
	}

	private void 更新信息文本()
	{
		int 物品数量 = 0;
		foreach (var 槽位 in 所有物品槽)
		{
			if (!槽位.是否为空) 物品数量++;
		}
		信息标签.Text = $"背包物品: {物品数量}/{所有物品槽.Count}";
	}
	
	// 公共接口方法
	public void 打开背包()
	{
		Visible = true;
		GetTree().Paused = true;
		ProcessMode = ProcessModeEnum.WhenPaused;
		ZIndex = 1000;
		GrabFocus();
	}
	
	public void 关闭背包()
	{
		Visible = false;
		GetTree().Paused = false;
		ProcessMode = ProcessModeEnum.Inherit;
	}
	
	public void 切换背包()
	{
		切换背包显示();
	}
}
