using Godot;
using 你的项目.Scripts.角色;
using System.Collections.Generic;

namespace 你的项目.Scripts.管理器
{
	public partial class 玩家管理器 : Node
	{
		private static 玩家管理器 _实例;
		public static 玩家管理器 实例 => _实例;
		
		public 玩家控制器 当前玩家 { get; private set; }
		
		[Export] public string 玩家场景路径 { get; set; } = "res://Scenes/Game/玩家.tscn";
		[Export] public PackedScene 跟随者预制体 { get; set; }
		
		private List<跟随者控制器> _跟随者列表 = new List<跟随者控制器>();
		
		public override void _Ready()
		{
			if (_实例 == null)
			{
				_实例 = this;
				ProcessMode = ProcessModeEnum.Always;
				GD.Print("玩家管理器已初始化");
			}
			else
			{
				QueueFree();
			}
		}
		
		public 玩家控制器 创建玩家(Vector2 位置 = default)
		{
			if (当前玩家 != null && IsInstanceValid(当前玩家))
			{
				GD.Print("已存在玩家实例，先销毁旧实例");
				销毁玩家();
			}
			
			var 玩家场景资源 = GD.Load<PackedScene>(玩家场景路径);
			if (玩家场景资源 == null)
			{
				GD.PrintErr($"无法加载玩家场景: {玩家场景路径}");
				return null;
			}
			
			当前玩家 = 玩家场景资源.Instantiate<玩家控制器>();
			
			if (位置 != default)
			{
				当前玩家.GlobalPosition = 位置;
			}
			
			GD.Print($"玩家已创建，位置: {位置}");
			return 当前玩家;
		}
		
		public void 销毁玩家()
		{
			if (当前玩家 != null && IsInstanceValid(当前玩家))
			{
				当前玩家.QueueFree();
				当前玩家 = null;
				GD.Print("玩家已销毁");
			}
		}
		
		public bool 玩家存在()
		{
			return 当前玩家 != null && IsInstanceValid(当前玩家);
		}
		
		public Vector2 获取玩家位置()
		{
			return 玩家存在() ? 当前玩家.GlobalPosition : Vector2.Zero;
		}
		
		public void 设置玩家位置(Vector2 位置)
		{
			if (玩家存在())
			{
				当前玩家.GlobalPosition = 位置;
				GD.Print($"玩家位置已设置: {位置}");
			}
		}
		
public void 添加跟随者(int 滞后格子数 = 2)
{
	if (!玩家存在())
	{
		GD.PrintErr("玩家不存在，无法添加跟随者");
		return;
	}
	if (跟随者预制体 == null)
	{
		GD.PrintErr("跟随者预制体未设置！");
		return;
	}

	var 跟随者 = 跟随者预制体.Instantiate<跟随者控制器>();
	GetTree().CurrentScene.AddChild(跟随者);
	跟随者.GlobalPosition = 当前玩家.GlobalPosition;

	_跟随者列表.Add(跟随者);
	GD.Print($"已添加跟随者，当前总数：{_跟随者列表.Count}");
}

public void 添加跟随者(PackedScene 指定预制体, int 滞后格子数 = 2)
{
	if (!玩家存在())
	{
		GD.PrintErr("玩家不存在，无法添加跟随者");
		return;
	}
	if (指定预制体 == null)
	{
		GD.PrintErr("跟随者预制体为空！");
		return;
	}

	var 跟随者 = 指定预制体.Instantiate<跟随者控制器>();
	GetTree().CurrentScene.AddChild(跟随者);
	跟随者.GlobalPosition = 当前玩家.GlobalPosition;

	_跟随者列表.Add(跟随者);
	GD.Print($"已添加跟随者，当前总数：{_跟随者列表.Count}");
}
		
		public void 移除跟随者(跟随者控制器 跟随者)
		{
			if (_跟随者列表.Contains(跟随者))
			{
				_跟随者列表.Remove(跟随者);
				跟随者.QueueFree();
				GD.Print("已移除一个跟随者");
			}
		}
		
		public void 清除所有跟随者()
		{
			foreach (var 跟随者 in _跟随者列表)
			{
				if (IsInstanceValid(跟随者))
					跟随者.QueueFree();
			}
			_跟随者列表.Clear();
			GD.Print("所有跟随者已清除");
		}
		
		public int 获取跟随者数量() => _跟随者列表.Count;
	}
}
