using Godot;
using 你的项目.Scripts.资源;
using 你的项目.Scripts.管理器;
using 你的项目.Scripts.UI;
using 你的项目.Scripts.交互;
using 你的项目.Scripts.角色;
using System;  // 用于 DateTime

public partial class GlobalSaveManager : Node
{
	private static GlobalSaveManager _实例;
	public static GlobalSaveManager 实例 => _实例;
public static bool 禁止条件设置 { get; private set; } = false;
	public static bool 存档界面打开中 { get; private set; } = false;
	public 存档点 当前激活的存档点 { get; private set; }

	public override void _Ready()
	{
		if (_实例 == null)
		{
			_实例 = this;
			ProcessMode = ProcessModeEnum.Always;
			GD.Print("GlobalSaveManager: 初始化完成");
		}
		else
		{
			QueueFree();
			return;
		}
	}

	public void 打开存档界面(存档点 存档点)
	{
		GD.Print($"GlobalSaveManager: 打开存档界面，存档点: {存档点?.存档点名称}");

   
		var 界面场景 = GD.Load<PackedScene>("res://保存系统/存档选择界面.tscn");
		if (界面场景 == null)
		{
			GD.PrintErr("GlobalSaveManager: 无法加载存档选择界面场景");
			return;
		}

		var 存档选择界面 = 界面场景.Instantiate() as 存档选择界面;
		if (存档选择界面 == null)
		{
			GD.PrintErr("GlobalSaveManager: 无法实例化存档选择界面");
			return;
		}

		GetTree().Root.AddChild(存档选择界面);

		// 连接信号（注意信号需要两个参数：存档位, 是保存）
		存档选择界面.存档选中 += (存档位, 是保存) =>
		{
			if (是保存)
				执行存档(存档位);
		};
		存档选择界面.界面关闭 += 关闭存档界面;

		当前激活的存档点 = 存档点;
		 禁止条件设置 = true;
		存档界面打开中 = true;
		存档选择界面.打开界面(true);  // true表示存档模式
		GetTree().Paused = true;
		GD.Print($"GlobalSaveManager: 游戏已暂停: {GetTree().Paused}");
	}

	public void 关闭存档界面()
	{
		GD.Print("GlobalSaveManager: 关闭存档界面");
  禁止条件设置 = false;

		存档界面打开中 = false;
		当前激活的存档点 = null;

		if (GetTree().Paused)
		{
			GetTree().Paused = false;
			GD.Print($"GlobalSaveManager: 游戏已恢复，暂停状态: {GetTree().Paused}");
		}

		var 玩家组 = GetTree().GetNodesInGroup("玩家");
		GD.Print($"GlobalSaveManager: 找到玩家数量: {玩家组.Count}");

		foreach (var 节点 in 玩家组)
		{
			if (节点 is 玩家控制器 玩家)
			{
				GD.Print($"GlobalSaveManager: 恢复玩家移动能力: {玩家.Name}");
				玩家.设置可移动(true);
			}
		}
	}

	public void 执行存档(int 存档位)
	{
		GD.Print($"[保存] 开始存档位 {存档位} 时间: {DateTime.Now:HH:mm:ss.fff}");

		// 打印保存前的条件状态
		if (条件管理器.实例 != null)
		{
			GD.Print("=== 保存前条件管理器状态 ===");
			条件管理器.实例.检查所有条件状态();
		}
		else
		{
			GD.PrintErr("保存前: 条件管理器实例为空");
		}

		if (当前激活的存档点 != null)
		{
			string 当前场景路径 = GetTree().CurrentScene.SceneFilePath;
			Vector2 存档位置 = 当前激活的存档点.GlobalPosition;
			string 存档点名称 = 当前激活的存档点.存档点名称;

			存档管理器.实例.保存游戏(存档位, 当前场景路径, 存档位置, 存档点名称);
			当前激活的存档点.显示保存成功提示();
		}
		else
		{
			GD.PrintErr("GlobalSaveManager: 当前激活的存档点为null，无法执行存档");
		}

		GD.Print($"[保存] 结束存档位 {存档位} 时间: {DateTime.Now:HH:mm:ss.fff}");

		关闭存档界面();
	}

	// 可选：在存档点保存游戏（如果你的存档点直接调用此方法）
	public void 在存档点保存游戏(int 存档位, 存档点 存档点实例)
	{
		string 当前场景路径 = GetTree().CurrentScene.SceneFilePath;
		Vector2 玩家位置 = 玩家管理器.实例.当前玩家.GlobalPosition;
		string 存档点名称 = 存档点实例.存档点名称;
		存档管理器.实例.保存游戏(存档位, 当前场景路径, 玩家位置, 存档点名称);
	}
}
