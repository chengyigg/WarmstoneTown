using Godot;
using System.Collections.Generic;

namespace 你的项目.Scripts.管理器
{
	[GlobalClass]
	public partial class 一次性动画管理器 : Node
	{
		private static 一次性动画管理器 _实例;
		public static 一次性动画管理器 实例 => _实例;

		private Dictionary<string, Dictionary<string, string>> _注册表 = new();

		public override void _Ready()
		{
			if (_实例 != null)
			{
				QueueFree();
				return;
			}
			_实例 = this;
			ProcessMode = ProcessModeEnum.Always;
		}

		public void 注册一次性动画(NodePath 节点路径, string 场景路径 = null)
		{
			if (场景路径 == null)
				场景路径 = GetTree().CurrentScene.SceneFilePath;

			string 唯一ID = $"{场景路径}|{节点路径}";
			if (!_注册表.ContainsKey(场景路径))
				_注册表[场景路径] = new Dictionary<string, string>();

			if (!_注册表[场景路径].ContainsKey(节点路径))
				_注册表[场景路径][节点路径] = 唯一ID;
		}

public void 标记动画已触发(NodePath 节点路径, string 场景路径 = null)
{
	// 如果正在存档界面操作，禁止记录（防止覆盖保存时意外写入）
	if (GlobalSaveManager.禁止条件设置)
	{
		GD.Print("[一次性动画管理器] 存档界面打开中，禁止记录动画触发");
		return;
	}

	if (场景路径 == null)
		场景路径 = GetTree().CurrentScene.SceneFilePath;

	string 唯一ID = $"{场景路径}|{节点路径}";

	// ★ 改为使用当前状态，不再依赖存档位
	var 当前状态 = 存档管理器.实例?.获取当前状态();
	if (当前状态 != null)
	{
		当前状态.记录动画触发(唯一ID, true);
		GD.Print($"[一次性动画管理器] 记录动画触发到当前状态: {唯一ID}");
	}
	else
	{
		GD.PrintErr($"[一次性动画管理器] 无法获取当前状态，记录失败");
		// 即使记录失败，也继续执行节点隐藏（但这种情况极少发生）
	}

	// 隐藏节点（无论记录是否成功，业务上应当执行）
	var 节点 = GetTree().CurrentScene.GetNodeOrNull<Node2D>(节点路径);
	if (节点 != null)
	{
		节点.Visible = false;
	}
	else
	{
		GD.PrintErr($"[一次性动画管理器] 未找到节点: {节点路径}");
	}
}

public void 恢复当前场景一次性动画()
{
	string 当前场景路径 = GetTree().CurrentScene.SceneFilePath;
	if (!_注册表.ContainsKey(当前场景路径))
		return;

	// ★ 改为从当前状态读取，不再依赖存档位
	var 当前状态 = 存档管理器.实例?.获取当前状态();
	if (当前状态 == null)
	{
		GD.PrintErr($"[一次性动画管理器] 无法获取当前状态，不能恢复动画状态");
		return;
	}

	foreach (var kvp in _注册表[当前场景路径])
	{
		string 节点路径字符串 = kvp.Key;
		string 唯一ID = kvp.Value;
		bool 已触发 = 当前状态.获取动画触发状态(唯一ID);
		var 节点 = GetTree().CurrentScene.GetNodeOrNull<Node2D>(节点路径字符串);
		if (节点 != null)
		{
			bool 新可见性 = !已触发;
			节点.Visible = 新可见性;
			GD.Print($"[一次性动画管理器] 节点 {节点路径字符串} 已触发={已触发}，设置 Visible={新可见性}（从当前状态读取）");
		}
		else
		{
			GD.PrintErr($"[一次性动画管理器] 未找到节点: {节点路径字符串}");
		}
	}
}


		public void 清理注册表()
		{
			_注册表.Clear();
		}
	}
}
