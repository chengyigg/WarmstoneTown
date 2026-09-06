using Godot;
using System.Collections.Generic;

public partial class 回忆系统管理器 : Node
{
	private static 回忆系统管理器 _实例;
	public static 回忆系统管理器 实例 => _实例;
	
	[Signal] public delegate void 回忆界面打开EventHandler();
	[Signal] public delegate void 回忆界面关闭EventHandler();
	
	private 回忆主界面 主界面;
	private bool 界面打开 = false;
	
	// 添加公共只读属性来暴露界面状态
	public bool 界面打开中 => 界面打开;
	
	// 存储所有角色的看法数据
	private Dictionary<string, 角色看法数据> 角色看法字典 = new Dictionary<string, 角色看法数据>();
	
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
		
		GD.Print("回忆系统管理器: 开始初始化...");
		
		// 加载回忆界面场景
		var 界面场景 = GD.Load<PackedScene>("res://回忆主界面.tscn");
		if (界面场景 != null)
		{
			GD.Print("回忆系统管理器: 成功加载界面场景");
			主界面 = 界面场景.Instantiate() as 回忆主界面;
			if (主界面 != null)
			{
				AddChild(主界面);
				主界面.Visible = false;
				GD.Print("回忆系统管理器: 成功添加主界面到场景树");
			}
			else
			{
				GD.PrintErr("回忆系统管理器: 实例化主界面失败");
			}
		}
		else
		{
			GD.PrintErr("回忆系统管理器: 无法加载回忆界面场景，请检查路径: res://回忆主界面.tscn");
		}
		
		// 移除初始化测试数据的调用
		// 初始化测试数据();
		
		GD.Print("回忆系统管理器: 初始化完成");
	}
	
public override void _Input(InputEvent @event)
{
	// 暂时禁用 Tab 键打开回忆界面
	// if (@event.IsActionPressed("ui_tab") && !界面打开)
	// {
	//     GD.Print("回忆系统管理器: 检测到Tab键按下，尝试打开界面");
	//     GetViewport().SetInputAsHandled();
	//     打开回忆界面();
	// }
	// else if (@event.IsActionPressed("ui_cancel") && 界面打开)
	// {
	//     GD.Print("回忆系统管理器: 检测到Esc键按下，尝试关闭界面");
	//     GetViewport().SetInputAsHandled();
	//     关闭回忆界面();
	// }
}
	
	private void 打开回忆界面()
	{
		if (主界面 == null) 
		{
			GD.PrintErr("回忆系统管理器: 主界面为空，无法打开");
			return;
		}
		
		界面打开 = true;
		主界面.Visible = true;
		主界面.打开界面();
		
		GD.Print($"回忆系统管理器: 界面可见性设置为 {主界面.Visible}");
		
		// 禁止玩家移动
		设置玩家移动(false);
		
		EmitSignal(nameof(回忆界面打开));
		GD.Print("回忆系统: 打开界面完成");
	}
	
	// 改为公共方法
	public void 关闭回忆界面()
	{
		if (主界面 == null) 
		{
			GD.PrintErr("回忆系统管理器: 主界面为空，无法关闭");
			return;
		}
		
		界面打开 = false;
		主界面.Visible = false;
		
		GD.Print($"回忆系统管理器: 界面可见性设置为 {主界面.Visible}");
		
		// 恢复玩家移动
		设置玩家移动(true);
		
		EmitSignal(nameof(回忆界面关闭));
		GD.Print("回忆系统: 关闭界面完成");
	}
	
	private void 设置玩家移动(bool 可移动)
	{
		var 玩家组 = GetTree().GetNodesInGroup("玩家");
		GD.Print($"回忆系统管理器: 找到 {玩家组.Count} 个玩家节点");
		
		foreach (var 节点 in 玩家组)
		{
			// 使用动态类型检查而不是具体类型
			if (节点 is Godot.Node 玩家节点 && 玩家节点.HasMethod("设置可移动"))
			{
				玩家节点.Call("设置可移动", 可移动);
				GD.Print($"回忆系统管理器: 设置玩家移动状态为 {可移动}");
			}
			else
			{
				GD.PrintErr("回忆系统管理器: 玩家节点没有'设置可移动'方法");
			}
		}
	}
	
	// 添加角色看法数据
	public void 添加角色看法(string 角色ID, 角色看法数据 数据)
	{
		角色看法字典[角色ID] = 数据;
		GD.Print($"回忆系统管理器: 添加角色看法 - {角色ID}: {数据?.角色名称}");
	}
	
	// 获取角色看法数据
	public 角色看法数据 获取角色看法(string 角色ID)
	{
		// 如果本地字典没有，尝试从主界面获取
		if (!角色看法字典.ContainsKey(角色ID) && 主界面 != null)
		{
			GD.Print($"回忆系统管理器: 本地字典没有角色 {角色ID}，尝试从主界面获取");
			return 主界面.获取角色看法(角色ID);
		}
		
		if (角色看法字典.ContainsKey(角色ID))
		{
			return 角色看法字典[角色ID];
		}
		GD.PrintErr($"回忆系统管理器: 未找到角色看法 - {角色ID}");
		return null;
	}
	
	// 获取所有角色ID
	public string[] 获取所有角色ID()
	{
		// 如果本地字典为空，尝试从主界面获取
		if (角色看法字典.Count == 0 && 主界面 != null)
		{
			GD.Print("回忆系统管理器: 本地字典为空，尝试从主界面获取角色ID");
			return 主界面.获取所有角色ID();
		}
		
		string[] 角色数组 = new string[角色看法字典.Keys.Count];
		角色看法字典.Keys.CopyTo(角色数组, 0);
		GD.Print($"回忆系统管理器: 获取到 {角色数组.Length} 个角色ID");
		return 角色数组;
	}
	
	// 添加解锁条件看法方法
	public void 解锁条件看法(string 条件名称)
	{
		int 解锁数量 = 0;
		
		// 如果本地字典为空，使用主界面的数据
		if (角色看法字典.Count == 0 && 主界面 != null)
		{
			GD.Print("回忆系统管理器: 使用主界面数据解锁条件看法");
			解锁数量 = 主界面.解锁条件看法(条件名称);
		}
		else
		{
			foreach (var 键值对 in 角色看法字典)
			{
				string 角色ID = 键值对.Key;
				角色看法数据 角色数据 = 键值对.Value;
				
				if (角色数据 != null)
				{
					foreach (看法条目 看法 in 角色数据.看法列表)
					{
						// 检查看法的解锁条件是否匹配
						if (!string.IsNullOrEmpty(看法.解锁条件) && 看法.解锁条件 == 条件名称)
						{
							看法.已解锁 = true;
							解锁数量++;
							GD.Print($"回忆系统管理器: 解锁看法 - {角色数据.角色名称} - {看法.主题}");
						}
					}
				}
			}
		}
		
		GD.Print($"回忆系统管理器: 总共解锁了 {解锁数量} 个条件为 '{条件名称}' 的看法");
		
		// 如果界面打开，强制刷新
		if (界面打开 && 主界面 != null)
		{
			GD.Print("回忆系统管理器: 界面打开中，强制刷新");
			主界面.打开界面();
		}
	}
	// 新增：调试方法，输出所有看法的当前状态
public void 输出所有看法状态()
{
	GD.Print("=== 回忆系统管理器: 所有看法状态 ===");
	
	string[] 所有角色ID = 获取所有角色ID();
	GD.Print($"总角色数: {所有角色ID.Length}");
	
	foreach (string 角色ID in 所有角色ID)
	{
		var 角色数据 = 获取角色看法(角色ID);
		if (角色数据 != null)
		{
			GD.Print($"角色: {角色数据.角色名称}");
			foreach (看法条目 看法 in 角色数据.看法列表)
			{
				GD.Print($"  看法: {看法.主题}, 已解锁: {看法.已解锁}, 条件: '{看法.解锁条件}'");
			}
		}
	}
	GD.Print("=================================");
}
	// 获取所有角色数据（用于外部访问）
	public 角色看法数据[] 获取所有角色数据()
	{
		角色看法数据[] 角色数组 = new 角色看法数据[角色看法字典.Values.Count];
		角色看法字典.Values.CopyTo(角色数组, 0);
		return 角色数组;
	}
	
	private void 初始化测试数据()
	{
		GD.Print("回忆系统管理器: 开始初始化测试数据");
		
		// 测试数据 - 角色1
		var 角色1数据 = new 角色看法数据
		{
			角色名称 = "莉莉丝",
			角色头像路径 = "res://角色头像/lilith.png",
			// 使用 Godot.Collections.Array 而不是 List
			看法列表 = new Godot.Collections.Array<看法条目>
			{
				new 看法条目 { 主题 = "关于主角", 内容 = "我觉得你是个很有趣的人，虽然有时候有点冒失，但心地很善良。" },
				new 看法条目 { 主题 = "关于村庄", 内容 = "这个村庄已经存在很久了，大家都很友好，希望你能喜欢这里。" },
				new 看法条目 { 主题 = "关于任务", 内容 = "如果你需要帮助，随时可以来找我。我知道很多关于这里的事情。" }
			}
		};
		
		// 测试数据 - 角色2
		var 角色2数据 = new 角色看法数据
		{
			角色名称 = "阿尔伯特",
			角色头像路径 = "res://角色头像/albert.png",
			// 使用 Godot.Collections.Array 而不是 List
			看法列表 = new Godot.Collections.Array<看法条目>
			{
				new 看法条目 { 主题 = "关于主角", 内容 = "新来的冒险者？希望你有足够的实力应对这里的挑战。" },
				new 看法条目 { 主题 = "关于战斗", 内容 = "记住，在战斗中保持冷静比力量更重要。" },
				new 看法条目 { 主题 = "关于宝藏", 内容 = "据说森林深处有古代遗迹，但那里很危险，要小心。" }
			}
		};
		
		添加角色看法("lilith", 角色1数据);
		添加角色看法("albert", 角色2数据);
		
		GD.Print("回忆系统管理器: 测试数据初始化完成");
	}
}
