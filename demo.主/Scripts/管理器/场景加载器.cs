using Godot;
using 你的项目.Scripts.资源;

namespace 你的项目.Scripts.管理器
{
	public partial class 场景加载器 : Node
	{
		private static 场景加载器 _实例;
		public static 场景加载器 实例 => _实例;
		
		[Export] public string 开始场景路径 { get; set; } = "res://Scenes/UI/StartMenu.tscn";
		[Export] public string 游戏场景路径 { get; set; } = "res://Scenes/Game/房屋场景/卧室1.tscn";
		
		// 默认转场配置
		[Export] public 转场动画资源 默认转场动画 { get; set; }

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
			}
			
			// 创建默认转场动画资源（如果没有设置）
			if (默认转场动画 == null)
			{
				默认转场动画 = 转场动画资源.创建默认();
			}
		}

		public void 加载开始菜单()
		{
			加载场景(开始场景路径, 默认转场动画);
		}

		public void 加载游戏场景()
		{
			加载场景(游戏场景路径, 默认转场动画);
		}

	  public void 加载场景(string 场景路径, 转场动画资源 转场配置 = null)
{
	// 如果有转场管理器，使用转场管理器
	if (转场管理器.实例 != null)
	{
		转场管理器.实例.开始转场(场景路径, 转场配置);
	}
	else
	{
		// 备用方案：直接加载场景
		直接加载场景(场景路径);
	}
}

	  public void 直接加载场景(string 路径)
{
	var 场景 = GD.Load<PackedScene>(路径);
	if (场景 != null)
	{
		GetTree().ChangeSceneToPacked(场景);
	}
	else
	{
		GD.PrintErr($"无法加载场景: {路径}");
	}
}

		public void 退出游戏()
		{
			GetTree().Quit();
		}
	}
}
