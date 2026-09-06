using Godot;
using 你的项目.Scripts.UI;
using 你的项目.Scripts.角色;

namespace 你的项目.Scripts.管理器
{
	public partial class 游戏管理器 : Node
	{
		private static 游戏管理器 _实例;
		public static 游戏管理器 实例 => _实例;
private bool _允许交互 = true;  // 默认允许交互
public bool 允许交互 => _允许交互;
		public enum 游戏状态
		{
			主菜单,
			游戏中,
			已暂停,
			游戏结束
		}

		private 游戏状态 _当前状态 = 游戏状态.主菜单;
		public 游戏状态 当前状态 
		{ 
			get => _当前状态; 
			set
			{
				_当前状态 = value;
				游戏状态变化?.Invoke(value);
			}
		}
public void 禁止玩家交互()
{
	_允许交互 = false;
	GD.Print("[游戏管理器] 禁止玩家交互（自动触发器将被阻止）");
}

public void 允许玩家交互()
{
	_允许交互 = true;
	GD.Print("[游戏管理器] 允许玩家交互（自动触发器已恢复）");
}
		public delegate void 游戏状态变化处理程序(游戏状态 新状态);
		public event 游戏状态变化处理程序 游戏状态变化;

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



		public void 开始游戏()
		{
			GD.Print("游戏管理器.开始游戏() 被调用");
			当前状态 = 游戏状态.游戏中;
			
			if (场景加载器.实例 == null)
			{
				GD.PrintErr("场景加载器实例为空！");
				return;
			}
			
			GD.Print("准备加载游戏场景");
			场景加载器.实例.加载游戏场景();
		}

		// 继续游戏：只应用一次状态，场景加载完成后会在转场管理器中再次应用，但为避免重复此处不再调用
		public void 继续游戏(int 存档位 = 0)
		{
			GD.Print($"游戏管理器.继续游戏() 被调用，存档位: {存档位}");
			当前状态 = 游戏状态.游戏中;
			
			if (场景加载器.实例 == null)
			{
				GD.PrintErr("场景加载器实例为空！");
				return;
			}
			
			// 注意：存档状态的应用已交由转场管理器统一处理，此处不再重复调用
			GD.Print("准备加载游戏场景（继续游戏）");
			场景加载器.实例.加载游戏场景();
		}

		public void 返回主菜单()
		{
			当前状态 = 游戏状态.主菜单;
			场景加载器.实例.加载开始菜单();
		}

		public void 切换暂停状态()
		{
			if (当前状态 == 游戏状态.游戏中)
			{
				当前状态 = 游戏状态.已暂停;
				GetTree().Paused = true;
			}
			else if (当前状态 == 游戏状态.已暂停)
			{
				当前状态 = 游戏状态.游戏中;
				GetTree().Paused = false;
			}
		}

		public void 场景加载完成(int 存档位 = 0)
		{
			GD.Print($"游戏管理器: 场景加载完成，存档位: {存档位}");
			
			// 注意：存档条件状态已在转场管理器中应用，此处不再重复应用
			// 仅做与位置恢复、跟随者相关的逻辑
			
			bool 当前场景需要玩家 = GetTree().CurrentScene?.IsInGroup("需要玩家") ?? false;
			if (!当前场景需要玩家)
			{
				GD.Print("游戏管理器: 当前场景不需要玩家，跳过位置恢复");
				return;
			}
			
			if (卡牌数据管理器.上下文.返回玩家位置 != Vector2.Zero)
			{
				Callable.From(() => {
					Node2D 玩家节点 = null;
					var 玩家组 = GetTree().GetNodesInGroup("玩家");
					if (玩家组.Count > 0)
						玩家节点 = 玩家组[0] as Node2D;
					if (玩家节点 == null)
						玩家节点 = GetTree().CurrentScene.GetNodeOrNull<Node2D>("玩家");
					if (玩家节点 != null)
					{
						玩家节点.GlobalPosition = 卡牌数据管理器.上下文.返回玩家位置;
						GD.Print($"游戏管理器恢复玩家位置到: {卡牌数据管理器.上下文.返回玩家位置}");
						卡牌数据管理器.上下文.从战斗返回 = false;
					}
					else
					{
						GD.PrintErr("游戏管理器: 未找到玩家节点，无法恢复位置！");
					}
				}).CallDeferred();
			}
			
			Callable.From(() => {
				if (!(GetTree().CurrentScene?.IsInGroup("需要玩家") ?? false))
					return;
					
				var 玩家组 = GetTree().GetNodesInGroup("玩家");
				if (玩家组.Count == 0) return;
				var 玩家 = 玩家组[0] as 玩家控制器;
				if (玩家 == null) return;
				
				if (条件管理器.实例 != null && 条件管理器.实例.检查条件("已获得跟随者"))
				{
					var 跟随者组 = GetTree().GetNodesInGroup("跟随者");
					if (跟随者组.Count == 0 && 玩家.跟随者预制体1 != null)
					{
						玩家.添加跟随者(玩家.跟随者预制体1, 2);
						GD.Print("游戏管理器: 自动添加跟随者（场景加载完成后）");
					}
				}
			}).CallDeferred();
		}
	}
}
