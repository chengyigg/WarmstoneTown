using Godot;
using System.Collections.Generic;
using System.Linq;
using 你的项目.Scripts.资源;
using 你的项目.Scripts.全局;
using 你的项目.Scripts.管理器;

public partial class 任务管理器 : Node
{
	private static 任务管理器 _实例;
	public static 任务管理器 实例 => _实例;

	public List<任务数据> 任务列表 { get; private set; } = new List<任务数据>();
	private int _下一个任务ID = 1;
	private Dictionary<int, 任务数据> _任务配置表 = new Dictionary<int, 任务数据>();

	[Signal] public delegate void 任务列表更新EventHandler();
	[Signal] public delegate void 任务进度更新EventHandler(任务数据 任务);
	[Signal] public delegate void 任务移除EventHandler(int 原始任务ID);

	public override void _Ready()
	{
		if (_实例 != null) { QueueFree(); return; }
		_实例 = this;
		ProcessMode = ProcessModeEnum.Always;
		加载任务配置();
	}

	private void 加载任务配置()
	{
		string 任务文件夹 = "res://任务系统/数据/";
		using var dir = DirAccess.Open(任务文件夹);
		if (dir == null)
		{
			GD.PrintErr($"[任务管理器] 无法打开任务文件夹: {任务文件夹}");
			return;
		}
		dir.ListDirBegin();
		string 文件名;
		int 加载数量 = 0;
		while ((文件名 = dir.GetNext()) != "")
		{
			if (!文件名.EndsWith(".tres")) continue;
			string 完整路径 = 任务文件夹 + 文件名;
			var 任务 = GD.Load<任务数据>(完整路径);
			if (任务 != null)
			{
				_任务配置表[任务.任务ID] = 任务;
				GD.Print($"[任务管理器] 加载任务: {任务.任务名称} (ID: {任务.任务ID})");
				加载数量++;
			}
			else GD.PrintErr($"[任务管理器] 加载任务失败: {完整路径}");
		}
		dir.ListDirEnd();
		GD.Print($"[任务管理器] 共加载 {加载数量} 个任务配置");
	}

public void 接取任务(任务数据 任务模板, string 完成时源序列路径 = "", string 完成时目标序列路径 = "")
{
	if (任务模板 == null) return;
	if (任务是否已完成(任务模板.任务ID))
	{
		GD.Print($"[任务管理器] 任务 {任务模板.任务名称} 已完成，不能重复接取");
		return;
	}
	if (任务列表.Any(t => t.原始任务ID == 任务模板.任务ID))
	{
		GD.Print($"[任务管理器] 任务 {任务模板.任务名称} 已存在，不重复接取");
		return;
	}

	var 新任务 = 任务模板.克隆();
	新任务.原始任务ID = 任务模板.任务ID;
	新任务.任务ID = _下一个任务ID++;

	新任务.完成时源序列路径 = 完成时源序列路径;
	新任务.完成时目标序列路径 = 完成时目标序列路径;

	新任务.进度.Clear();
	foreach (var 需求 in 任务模板.需求列表)
	{
		if (需求 == null) continue;
		if (需求.需求类型 == 任务类型枚举.收集道具)
		{
			int 背包数量 = 条件服务.获取道具数量(需求.目标ID);
			新任务.进度[需求.目标ID] = 背包数量;
			GD.Print($"[任务管理器] 任务 '{任务模板.任务名称}' 需求 '{需求.目标ID}' 初始背包数量: {背包数量}");
		}
		else
		{
			新任务.进度[需求.目标ID] = 0;
		}
	}

	任务列表.Add(新任务);
	GD.Print($"[任务管理器] 接取任务: {新任务.任务名称} (原始ID: {新任务.原始任务ID})");

	// ★ 删除自动完成检测（不在此处完成）
	// if (新任务.所有需求已完成())
	// {
	//     完成内部任务(新任务);
	//     GD.Print($"[任务管理器] 任务 '{新任务.任务名称}' 接取时已满足需求，立即完成");
	// }
	// else
	// {
		任务提示UI.显示接取提示(新任务.任务名称);
	// }

	EmitSignal(SignalName.任务列表更新);
}

	public void 更新进度(string 目标ID, int 增量 = 1)
{
	bool 有更新 = false;
	foreach (var 任务 in 任务列表)
	{
		if (任务.是否完成) continue;

		bool 匹配 = false;
		foreach (var 需求 in 任务.需求列表)
		{
			if (需求.目标ID == 目标ID)
			{
				匹配 = true;
				break;
			}
		}
		if (!匹配) continue;

		任务.更新进度(目标ID, 增量);
		有更新 = true;

		// ★ 删除自动完成检测和调用
		// if (任务.所有需求已完成() && !任务.是否完成)
		// {
		//     完成内部任务(任务);
		//     GD.Print($"[任务管理器] 任务完成: {任务.任务名称}");
		// }
	}
	if (有更新)
	{
		EmitSignal(SignalName.任务列表更新);
		EmitSignal(SignalName.任务进度更新, 任务列表.Find(t => !t.是否完成 && t.需求列表.Any(r => r.目标ID == 目标ID)));
	}
}



	// 内部完成方法，避免递归
private void 完成内部任务(任务数据 任务)
{
	// ★ 移除任务
	任务列表.Remove(任务);
	GD.Print($"[任务管理器] 任务完成并移除: {任务.任务名称} (原始ID: {任务.原始任务ID})");

	// 发放奖励
	_发放任务奖励(任务);
	任务提示UI.显示完成提示(任务.任务名称);

	// 记录已完成
	var 状态 = 存档管理器.实例?.获取当前状态();
	if (状态 != null && !状态.已完成任务ID列表.Contains(任务.原始任务ID))
	{
		状态.已完成任务ID列表.Add(任务.原始任务ID);
		GD.Print($"[任务管理器] 记录任务已完成: {任务.任务名称} (原始ID: {任务.原始任务ID})");
	}

	// 播放音效
	if (任务.完成音效 != null && 全局背景音乐管理器.实例 != null)
		全局背景音乐管理器.实例.播放音效(任务.完成音效);

	// ★ 修改：直接从任务数据读取目标改变
	if (!string.IsNullOrEmpty(任务.完成时源序列路径) && !string.IsNullOrEmpty(任务.完成时目标序列路径))
	{
		var 当前状态 = 存档管理器.实例?.获取当前状态();
		if (当前状态 != null)
		{
			当前状态.设置对话改变状态(任务.完成时源序列路径, true);
			当前状态.设置对话改变目标序列(任务.完成时源序列路径, 任务.完成时目标序列路径);
			GD.Print($"[任务管理器] 应用任务完成目标改变: {任务.完成时源序列路径} → {任务.完成时目标序列路径}");
		}
	}

	EmitSignal(SignalName.任务列表更新);
}

	// 外部调用的完成任务（保留兼容性，但内部逻辑已统一）
public void 完成任务(int 原始任务ID)
{
	var 任务 = 任务列表.Find(t => t.原始任务ID == 原始任务ID);
	if (任务 == null)
	{
		GD.PrintErr($"[任务管理器] 未找到要完成的任务 (原始ID: {原始任务ID})");
		return;
	}
	// ★ 如果任务已经完成，说明只是进度达标，尚未发放奖励，需要补发
	if (任务.是否完成)
	{
		完成内部任务(任务);
		return;
	}
	// 强制完成（即使需求未满足，用于特殊场合）
	任务.是否完成 = true;
	完成内部任务(任务);
}

	

private void _发放任务奖励(任务数据 任务)
{
	if (任务.奖励金币 > 0)
	{
		if (金币管理器.实例 != null)
		{
			金币管理器.实例.增加金币(任务.奖励金币);
			// ★ 删除或注释掉强制刷新金币UI，让信号自动更新
			// 强制刷新金币UI();
		}
		else GD.PrintErr("[任务管理器] 金币管理器实例为空");
	}

		if (任务.奖励卡组 != null && 任务.奖励卡组.卡组.Count > 0)
			玩家卡组管理器.实例?.添加卡牌列表(任务.奖励卡组.卡组);

		if (任务.奖励道具列表 != null && 任务.奖励道具列表.Count > 0)
		{
			foreach (var 道具 in 任务.奖励道具列表)
			{
				if (道具 != null)
				{
					var 克隆 = 道具.克隆();
					玩家道具管理器.实例?.添加道具(克隆);
				}
			}
		}
	}

private void 强制刷新金币UI()
{
	var 金币UI节点 = GetTree().Root.FindChild("金币UI", true, false) as CanvasLayer;
	if (金币UI节点 == null)
	{
		GD.PrintErr("[任务管理器] 找不到金币UI节点");
		return;
	}
	Label label = 金币UI节点.FindChild("Label", true, false) as Label;
	if (label == null)
	{
		GD.PrintErr("[任务管理器] 找不到金币Label节点");
		return;
	}
	if (金币管理器.实例 != null)
	{
		label.Text = 金币管理器.实例.金币数量.ToString();
		GD.Print($"[任务管理器] 金币UI已更新为: {金币管理器.实例.金币数量}");
	}
}

	// 兼容旧接口：更新击杀进度
	public void 更新击杀进度(string 怪物名称)
	{
		更新进度(怪物名称, 1);
	}

public void 更新收集进度(int 道具ID, int 增加数量 = 1)
{
	// 直接调用更新进度
	更新进度(道具ID.ToString(), 增加数量);
}

	public List<任务数据> 获取所有任务() => 任务列表;
	public List<任务数据> 获取进行中任务() => 任务列表.FindAll(t => !t.是否完成);
	public List<任务数据> 获取已完成任务() => 任务列表.FindAll(t => t.是否完成);

	public bool 任务是否已完成(int 原始任务ID)
	{
		var 状态 = 存档管理器.实例?.获取当前状态();
		if (状态 == null) return false;
		return 状态.已完成任务ID列表.Contains(原始任务ID);
	}

	public void 重置()
	{
		任务列表.Clear();
		_下一个任务ID = 1;
		EmitSignal(SignalName.任务列表更新);
		GD.Print("[任务管理器] 已重置所有任务");
	}

	public void 从存档恢复(Godot.Collections.Array<任务数据> 存档任务列表)
{
	任务列表.Clear();
	if (存档任务列表 == null) return;

	foreach (var 任务 in 存档任务列表)
	{
		if (任务 != null)
		{
			// 如果需求列表为空，但进度不为空，则可能是旧格式（但旧字段已删除，不会发生）
			// 为了安全，如果需求列表为空，我们尝试从进度重建？但无法知道目标数量。
			// 所以我们只添加有需求的任务，其他丢弃并记录警告。
			if (任务.需求列表.Count == 0)
			{
				GD.PrintErr($"[任务管理器] 丢弃任务 '{任务.任务名称}'，因为需求列表为空。可能是旧存档，请重新接取。");
				continue;
			}
			任务列表.Add(任务);
			if (任务.任务ID >= _下一个任务ID)
				_下一个任务ID = 任务.任务ID + 1;
		}
	}
	GD.Print($"[任务管理器] 从存档恢复 {任务列表.Count} 个任务");
	EmitSignal(SignalName.任务列表更新);
}

	public Godot.Collections.Array<任务数据> 导出到存档()
	{
		var 数组 = new Godot.Collections.Array<任务数据>();
		foreach (var 任务 in 任务列表)
			数组.Add(任务.克隆());
		return 数组;
	}
}
