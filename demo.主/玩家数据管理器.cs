// PlayerDataManager.cs
using Godot;
using System;

namespace 你的项目.Scripts.管理器
{
	public partial class 玩家数据管理器 : Node
	{
		private static 玩家数据管理器 _实例;
		public static 玩家数据管理器 实例 => _实例;

		private string _玩家名字 = "冒险者"; // 默认名字

		public string 玩家名字
		{
			get 
			{ 
				GD.Print($"[玩家数据管理器] 获取玩家名字: {_玩家名字}");
				return _玩家名字; 
			}
			set
			{
				GD.Print($"[玩家数据管理器] 设置玩家名字: 从 '{_玩家名字}' 改为 '{value}'");
				_玩家名字 = value;
				EmitSignal(nameof(玩家名字改变), _玩家名字);
			}
		}

		[Signal] public delegate void 玩家名字改变EventHandler(string 新名字);

		public override void _Ready()
		{
			if (_实例 == null)
			{
				_实例 = this;
				ProcessMode = ProcessModeEnum.Always;
				GD.Print("[玩家数据管理器] 初始化完成，当前名字: " + 玩家名字);
			}
			else
			{
				QueueFree();
			}
		}
		
		public void 重置数据()
		{
			GD.Print("[玩家数据管理器] 重置数据");
			玩家名字 = "冒险者";
		}
	}
}
