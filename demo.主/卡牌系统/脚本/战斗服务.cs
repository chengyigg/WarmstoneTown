using Godot;
using 你的项目.Scripts.资源;
using 你的项目.Scripts.管理器;

/// <summary>
/// 战斗服务 - 负责触发战斗、战斗场景切换
/// </summary>
public static class 战斗服务
{
	/// <summary>
	/// 触发战斗（由对话播放器调用）
	/// </summary>
	public static void 触发战斗(string 战斗场景路径, 转场动画资源 转场动画)
	{
		GD.Print($"[战斗服务] 触发战斗: {战斗场景路径}");

		if (转场管理器.实例 != null)
		{
			int 当前存档位 = 卡牌数据管理器.当前存档位;
			转场管理器.实例.开始转场(
				战斗场景路径,
				转场动画,
				null,
				false,
				false,
				null,
				false,
				null,
				当前存档位
			);
		}
		else
		{
			// 备用：直接切换
			GetTree().ChangeSceneToFile(战斗场景路径);
		}
	}

	/// <summary>
	/// 战斗胜利后返回场景（由战斗胜利界面调用）
	/// </summary>
	public static void 返回场景(
		string 返回场景路径,
		bool 是否触发对话,
		对话序列 自动对话,
		int 当前存档位
	)
	{
		if (转场管理器.实例 != null)
		{
			转场管理器.实例.开始转场(
				返回场景路径,
				null,
				null,
				false,
				false,
				null,
				是否触发对话,
				自动对话,
				当前存档位
			);
		}
		else
		{
			GetTree().ChangeSceneToFile(返回场景路径);
		}
	}

	/// <summary>
	/// 获取当前场景的根节点（辅助方法）
	/// </summary>
	private static SceneTree GetTree() => Engine.GetMainLoop() as SceneTree;
}
