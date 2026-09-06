using Godot;
using System.Collections.Generic;
using 你的项目.Scripts.资源;

public partial class 卡牌数据管理器 : Node
{
	private static int _当前存档位 = -1;
	public static 卡牌数据管理器 实例 { get; private set; }
	public Godot.Collections.Array<卡牌数据> 玩家初始卡组 { get; set; }

	// ★ 当前战斗上下文（每次战斗时创建新实例）
	private 战斗上下文 _当前战斗上下文 = new 战斗上下文();
	public static 战斗上下文 上下文 => 实例?._当前战斗上下文;

	// ★ 为了方便，提供一个快速创建新上下文的方法
public static void 开始新战斗()
{
	if (实例 != null)
		实例._当前战斗上下文 = new 战斗上下文(); // 直接 new
}

	// ★ 战斗结束后重置上下文
	public static void 结束战斗()
	{
		if (实例 != null)
		   实例._当前战斗上下文.重置();
	}

	public static int 当前存档位
	{
		get => _当前存档位;
		set
		{
			if (_当前存档位 != value)
			{
				GD.Print($"[卡牌数据管理器] 当前存档位 从 {_当前存档位} 改为 {value}");
				GD.Print($"调用堆栈: {System.Environment.StackTrace}");
				_当前存档位 = value;
			}
		}
	}

	public override void _EnterTree()
	{
		if (实例 != null)
		{
			QueueFree();
			return;
		}
		实例 = this;
	}

	public override void _Ready()
	{
		if (玩家初始卡组 == null)
		{
			玩家初始卡组 = new Godot.Collections.Array<卡牌数据>();
			var 测试卡牌1 = new 卡牌数据();
			测试卡牌1.卡牌名称 = "测试卡牌1";
			玩家初始卡组.Add(测试卡牌1);
			var 测试卡牌2 = new 卡牌数据();
			测试卡牌2.卡牌名称 = "测试卡牌2";
			玩家初始卡组.Add(测试卡牌2);
		}
	}
}
