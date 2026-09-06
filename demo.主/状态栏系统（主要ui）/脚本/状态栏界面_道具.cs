using Godot;
using System;
using 你的项目.Scripts.管理器;
using System.Collections.Generic;

public partial class 状态栏界面
{
	// ======================== 道具相关 ========================
private void 刷新道具UI()
{
	if (道具格子容器 == null) { GD.PrintErr("道具格子容器 未在编辑器中赋值"); return; }
	foreach (Node child in 道具格子容器.GetChildren()) child.QueueFree();
	所有道具槽.Clear();

	var 道具列表 = 玩家道具管理器.实例.玩家道具列表;
	foreach (var 道具 in 道具列表)
	{
		var 槽 = GD.Load<PackedScene>("res://任务系统/道具/道具槽.tscn").Instantiate<道具槽>();
		槽.绑定数据(道具);
		槽.道具槽点击 += 当道具槽被点击;
		道具格子容器.AddChild(槽);
		所有道具槽.Add(槽);
	}

	当前选中行 = 0;
	当前选中列 = 0;
	更新选中高亮和描述();

	// ★ 不要在这里设置 Visible，交给标签切换控制
	// 如果道具列表为空，可以清空描述文字
	if (所有道具槽.Count == 0 && 道具描述标签 != null)
		道具描述标签.Text = "没有道具";
}

private void 更新选中高亮和描述()
{
	foreach (var 槽 in 所有道具槽)
		槽.设置高亮(false);

	int 索引 = 当前选中行 * 2 + 当前选中列;
	if (索引 < 所有道具槽.Count)
	{
		所有道具槽[索引].设置高亮(true);
		var 当前道具 = 玩家道具管理器.实例.玩家道具列表[索引];
		if (道具描述标签 != null)
			道具描述标签.Text = 当前道具.描述;
	}
	else
	{
		if (道具描述标签 != null)
			道具描述标签.Text = "没有选中道具";
	}
	// 不要修改 道具描述框.Visible
}

	private void 当道具槽被点击(道具数据 道具)
	{
		var 道具列表 = 玩家道具管理器.实例.玩家道具列表;
		int 索引 = 道具列表.IndexOf(道具);
		if (索引 >= 0)
		{
			当前选中行 = 索引 / 2;
			当前选中列 = 索引 % 2;
			更新选中高亮和描述();
		}
	}

	private void 初始化道具对话框()
	{
		使用确认对话框 = new ConfirmationDialog();
		使用确认对话框.Title = "使用道具";
		使用确认对话框.DialogText = "确定要使用该道具吗？";
		使用确认对话框.Confirmed += 执行使用道具;
		AddChild(使用确认对话框);
	}

	private void 使用当前选中的道具()
	{
		int 索引 = 当前选中行 * 2 + 当前选中列;
		if (索引 >= 所有道具槽.Count) return;
		var 道具 = 玩家道具管理器.实例.玩家道具列表[索引];
		使用确认对话框.DialogText = $"确定要使用 {道具.名称} 吗？";
		使用确认对话框.PopupCentered();
	}

private void 执行使用道具()
{
	int 索引 = 当前选中行 * 2 + 当前选中列;
	if (索引 >= 所有道具槽.Count) return;
	var 道具 = 玩家道具管理器.实例.玩家道具列表[索引];
	GD.Print($"使用道具：{道具.名称}，剩余数量：{道具.数量 - 1}");
	
	玩家道具管理器.实例.使用道具(道具);
	
	// 立即刷新UI（信号也会触发，但直接调用更及时且安全）
	刷新道具UI();
}

	private void 修正选中索引()
	{
		if (所有道具槽.Count == 0)
		{
			道具标签激活 = false;
			道具面板.Visible = false;
			return;
		}
		int 最大索引 = 所有道具槽.Count - 1;
		int 当前索引 = 当前选中行 * 2 + 当前选中列;
		if (当前索引 > 最大索引)
		{
			当前索引 = 最大索引;
			当前选中行 = 当前索引 / 2;
			当前选中列 = 当前索引 % 2;
		}
		更新选中高亮和描述();
	}
}
