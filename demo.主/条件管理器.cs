using Godot;
using System.Collections.Generic;
using Godot.Collections; // 用于覆盖条件

public partial class 条件管理器 : Node
{
	private static 条件管理器 _实例;
	public static 条件管理器 实例 => _实例;
	
	private HashSet<string> 已满足条件 = new HashSet<string>();
	
	private List<string> 测试条件列表 = new List<string> { "条件1", "条件2", "条件3", "条件4", "条件5" };
	private int 当前测试条件索引 = 0;
	// 条件管理器.cs
[Signal] public delegate void 条件更新EventHandler(string 条件名称, bool 新状态);
	public override void _Ready()
	{
		if (_实例 == null)
		{
			_实例 = this;
			ProcessMode = ProcessModeEnum.Always;
		}
		else
		{
			QueueFree();
			return;
		}
		GD.Print("条件管理器: 初始化完成");
	}
	
	public bool 检查条件(string 条件名称)
	{
		return 已满足条件.Contains(条件名称);
	}
	
	public void 覆盖条件(Godot.Collections.Dictionary<string, bool> 条件字典)
	{
		已满足条件.Clear();
		foreach (var kvp in 条件字典)
		{
			if (kvp.Value)
				已满足条件.Add(kvp.Key);
		}
		GD.Print($"[条件管理器] 已覆盖条件，共 {已满足条件.Count} 个条件");
	}
	
	public void 设置条件满足(string 条件名称)
	{
		已满足条件.Add(条件名称);
		EmitSignal(SignalName.条件更新, 条件名称, true);  // ★ 发射信号
	}
	
	public void 移除条件(string 条件名称)
	{
		已满足条件.Remove(条件名称);
		EmitSignal(SignalName.条件更新, 条件名称, false); // ★ 发射信号
	}
	
	public string 获取下一个测试条件()
	{
		if (测试条件列表.Count == 0) return "";
		当前测试条件索引 = (当前测试条件索引 + 1) % 测试条件列表.Count;
		return 测试条件列表[当前测试条件索引];
	}

	public string 获取当前测试条件()
	{
		if (测试条件列表.Count == 0) return "";
		return 测试条件列表[当前测试条件索引];
	}
	
	public void 清空所有条件()
	{
		已满足条件.Clear();
	}
	
	public string[] 获取所有已满足条件()
	{
		string[] 条件数组 = new string[已满足条件.Count];
		已满足条件.CopyTo(条件数组);
		return 条件数组;
	}

	public void 检查所有条件状态()
	{
		GD.Print("=== 条件管理器状态检查 ===");
		GD.Print($"已满足条件数量: {已满足条件.Count}");
		foreach (string 条件 in 已满足条件)
		{
			GD.Print($"  已满足条件: {条件}");
		}
		GD.Print($"条件'测试'的状态: {检查条件("测试")}");
	}
}
