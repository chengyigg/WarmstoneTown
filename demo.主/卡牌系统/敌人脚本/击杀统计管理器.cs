using Godot;
using System.Collections.Generic;

public partial class 击杀统计管理器 : Node
{
	private static 击杀统计管理器 _实例;
	public static 击杀统计管理器 实例 => _实例;

	// 存储怪物ID → 击杀数量
	private Dictionary<string, int> _击杀记录 = new Dictionary<string, int>();

	public override void _Ready()
	{
		if (_实例 == null)
			_实例 = this;
		else
			QueueFree();
	}

	// 增加击杀数
public void 增加击杀(string 怪物ID, int 数量 = 1)
{
	if (!_击杀记录.ContainsKey(怪物ID))
		_击杀记录[怪物ID] = 0;
	_击杀记录[怪物ID] += 数量;
	GD.Print($"[击杀统计] {怪物ID} 击杀数 +{数量}，当前总数 {_击杀记录[怪物ID]}");

	// ★ 新增：通知任务管理器更新击杀任务进度
	任务管理器.实例?.更新击杀进度(怪物ID);
}

	// 获取击杀数
	public int 获取击杀数(string 怪物ID)
	{
		return _击杀记录.ContainsKey(怪物ID) ? _击杀记录[怪物ID] : 0;
	}

	// 重置所有（新游戏时）
	public void 重置()
	{
		_击杀记录.Clear();
	}
}
