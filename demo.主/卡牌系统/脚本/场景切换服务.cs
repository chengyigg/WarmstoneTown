using Godot;
using 你的项目.Scripts.资源;
using 你的项目.Scripts.管理器;
using 你的项目.Scripts.摄像机;

/// <summary>
/// 场景切换服务 - 负责场景切换（传送、转场）
/// </summary>
public static class 场景切换服务
{
	/// <summary>
	/// 执行场景切换（由对话播放器调用）
	/// </summary>
	public static void 切换场景(
		string 目标场景路径,
		转场动画资源 转场动画配置,
		bool 传送后播放摄像头动画 = false,
		摄像头动画资源 摄像头动画配置 = null,
		bool 转场后触发对话 = false,
		对话序列 转场后对话序列 = null
	)
	{
		string 来源场景路径 = GetTree().CurrentScene.SceneFilePath;
		int 当前存档位 = 卡牌数据管理器.当前存档位;

		if (转场管理器.实例 != null)
		{
			转场管理器.实例.开始转场(
				目标场景路径,
				转场动画配置,
				来源场景路径,
				false,
				传送后播放摄像头动画,
				摄像头动画配置,
				转场后触发对话,
				转场后对话序列,
				当前存档位
			);
		}
		else
		{
			GetTree().ChangeSceneToFile(目标场景路径);
		}
	}

	/// <summary>
	/// 执行传送（含返回原地逻辑）
	/// </summary>
	public static void 执行传送(
		string 目标场景路径,
		转场动画资源 转场动画配置,
		bool 返回原地,
		bool 传送后触发对话,
		对话序列 传送后对话序列
	)
	{
		// 返回原地逻辑由选项执行器处理，这里只是包装
		切换场景(
			目标场景路径,
			转场动画配置,
			false,
			null,
			传送后触发对话,
			传送后对话序列
		);
	}

	private static SceneTree GetTree() => Engine.GetMainLoop() as SceneTree;
}
