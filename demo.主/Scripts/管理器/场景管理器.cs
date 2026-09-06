using Godot;
using System.Collections.Generic;
using 你的项目.Scripts.交互; // 确保这一行存在

namespace 你的项目.Scripts.管理器
{
	public partial class 场景管理器 : Node
	{
		private static 场景管理器 _实例;
		public static 场景管理器 实例 => _实例;
		
		// 存储各场景的默认传送点
		private Dictionary<string, 传送点> 场景默认传送点 = new Dictionary<string, 传送点>();
		
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
		}
		
		// 注册场景的默认传送点
		public void 注册场景默认传送点(string 场景路径, 传送点 传送点实例)
		{
			场景默认传送点[场景路径] = 传送点实例;
			GD.Print($"注册场景 {场景路径} 的默认传送点: {传送点实例.传送点名称}");
		}
		
		// 获取场景的默认传送点
		public 传送点 获取场景默认传送点(string 场景路径)
		{
			if (场景默认传送点.ContainsKey(场景路径))
			{
				return 场景默认传送点[场景路径];
			}
			return null;
		}
		
		// 激活当前场景的默认传送点
		public void 激活当前场景默认传送点()
		{
			string 当前场景路径 = GetTree().CurrentScene.SceneFilePath;
			var 传送点 = 获取场景默认传送点(当前场景路径);
			
			if (传送点 != null)
			{
				传送点.激活传送点();
			}
			else
			{
				GD.PrintErr($"警告: 场景 {当前场景路径} 没有找到默认传送点");
			}
		}
	}
}
