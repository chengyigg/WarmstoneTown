using Godot;
using System;
using System.Collections.Generic;

public partial class 卡牌效果管理器 : Node
{
	private static 卡牌效果管理器 _实例;
	public static 卡牌效果管理器 实例 => _实例;
	
	private Dictionary<string, I卡牌效果> 效果缓存 = new();
	
	public override void _EnterTree()
	{
		if (_实例 == null)
		{
			_实例 = this;
		}
	}
	
	public I卡牌效果 获取效果(string 效果类名)
	{
		if (效果缓存.ContainsKey(效果类名))
		{
			return 效果缓存[效果类名];
		}

		// 动态创建效果实例
		Type 类型 = Type.GetType(效果类名);
		if (类型 != null)
		{
			try
			{
				var 效果实例 = Activator.CreateInstance(类型) as I卡牌效果;
				if (效果实例 != null)
				{
					效果缓存[效果类名] = 效果实例;
					return 效果实例;
				}
			}
			catch (Exception e)
			{
				GD.PrintErr($"创建效果实例失败: {效果类名}, 错误: {e.Message}");
			}
		}
		
		GD.PrintErr($"无法创建效果: {效果类名}");
		return null;
	}
	
public void 执行卡牌效果(卡牌实例 卡牌, 卡牌战斗单位 使用者, 卡牌战斗单位 目标, 卡牌战斗管理器 战斗管理器)
{
	if (卡牌?.基础数据 == null)
	{
		GD.PrintErr("卡牌或基础数据为空");
		return;
	}

	var 效果列表 = 卡牌.基础数据.效果数据列表;
	if (效果列表 == null || 效果列表.Count == 0)
	{
		GD.PrintErr($"卡牌 {卡牌.基础数据.卡牌名称} 没有配置任何效果！");
		return;
	}

	foreach (var 效果数据 in 效果列表)
	{
		if (string.IsNullOrEmpty(效果数据.效果类名)) continue;
		var 效果 = 获取效果(效果数据.效果类名);
		效果?.执行效果(使用者, 目标, 卡牌, 战斗管理器);
	}
}
}
