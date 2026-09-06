using Godot;
using 你的项目.Scripts.UI;
using 你的项目.Scripts.管理器;

public partial class 全局存档UI管理器 : Node
{
	public static 全局存档UI管理器 实例 { get; private set; }
	
	private 存档选择界面 _当前界面 = null;
	
	public override void _Ready()
	{
		if (实例 == null)
		{
			实例 = this;
			ProcessMode = ProcessModeEnum.Always;
		}
		else
		{
			QueueFree();
		}
	}

	public async System.Threading.Tasks.Task<bool> 请求保存并等待()
	{
		if (_当前界面 != null)
			return false;

		string 场景路径 = "res://保存系统/存档选择界面.tscn";
		if (!ResourceLoader.Exists(场景路径))
		{
			GD.PrintErr($"[全局存档UI管理器] 场景文件不存在: {场景路径}");
			return false;
		}

		var 场景 = GD.Load<PackedScene>(场景路径);
		if (场景 == null)
		{
			GD.PrintErr("[全局存档UI管理器] 加载场景失败，返回 null");
			return false;
		}

		_当前界面 = 场景.Instantiate<存档选择界面>();
		if (_当前界面 == null)
		{
			GD.PrintErr("[全局存档UI管理器] 实例化存档选择界面失败");
			return false;
		}

		GetTree().CurrentScene.AddChild(_当前界面);
		_当前界面.打开界面(true);

		bool 已保存 = false;
		int 选中存档位 = -1;

	_当前界面.存档选中 += (int 存档位, bool 是保存) =>
{
	if (是保存)
	{
		选中存档位 = 存档位;
		已保存 = true;
		_当前界面.QueueFree();
	}
	else
	{
		GD.PrintErr("存档UI管理器不应在保存模式下收到加载信号");
	}
};

		_当前界面.界面关闭 += () =>
		{
			已保存 = false;
			_当前界面.QueueFree();
		};

		await ToSignal(_当前界面, "tree_exited");
		_当前界面 = null;

		if (已保存 && 选中存档位 != -1)
		{
			卡牌数据管理器.当前存档位 = 选中存档位;
			var 玩家位置 = 获取玩家全局位置();
			var 当前场景路径 = GetTree().CurrentScene.SceneFilePath;
			if (存档管理器.实例 == null)
			{
				GD.PrintErr("[全局存档UI管理器] 存档管理器实例为空，无法保存");
				return false;
			}
			存档管理器.实例.保存游戏(选中存档位, 当前场景路径, 玩家位置, "对话选项存档");
			return true;
		}
		return false;
	}
	
	private Vector2 获取玩家全局位置()
	{
		var 玩家节点 = GetTree().GetFirstNodeInGroup("玩家");
		if (玩家节点 is Node2D 玩家2D)
			return 玩家2D.GlobalPosition;
		return Vector2.Zero;
	}
}
