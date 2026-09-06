using Godot;
using System.Collections.Generic;

public partial class 玩家道具管理器 : Node
{
	private static 玩家道具管理器 _实例;
	public static 玩家道具管理器 实例 => _实例;

	public List<道具数据> 玩家道具列表 = new List<道具数据>();

	public override void _Ready()
	{
		if (_实例 == null)
		{
			_实例 = this;
			ProcessMode = ProcessModeEnum.Always;
			

	
			EmitSignal(SignalName.道具列表已更新);
		}
		else
		{
			QueueFree();
		}
		// ★ 删除下面这行，它重复添加且变量作用域错误
		// 玩家道具列表.Add(初始道具.克隆());
	}

public void 添加道具(道具数据 新道具, bool 显示提示 = true)
{
	var 已有 = 玩家道具列表.Find(p => p.道具ID == 新道具.道具ID);
	if (已有 != null)
	{
		已有.数量 += 新道具.数量;
	}
	else
	{
		玩家道具列表.Add(新道具);
	}

	// ★ 通知任务管理器更新收集进度
	if (任务管理器.实例 != null)
	{
		任务管理器.实例.更新收集进度(新道具.道具ID, 新道具.数量);
	}

	if (显示提示 && 新道具 != null)
	{
		任务提示UI.显示道具获得提示(新道具.名称);
	}

	EmitSignal(SignalName.道具列表已更新);
}

	public void 使用道具(道具数据 道具)
	{
		道具.数量--;
		if (道具.数量 <= 0)
		{
			玩家道具列表.Remove(道具);
		}
		EmitSignal(SignalName.道具列表已更新);
	}

	[Signal]
	public delegate void 道具列表已更新EventHandler();
}
