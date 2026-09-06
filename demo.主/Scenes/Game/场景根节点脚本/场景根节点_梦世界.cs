using Godot;
using 你的项目.Scripts.管理器;
using 你的项目.Scripts.资源;
using System.Threading.Tasks;

public partial class 场景根节点_梦世界 : Node2D
{
	[Export] public NodePath 一次性动画节点Path;  // 保留备用（实际不再使用）

	public override async void _Ready()
	{
		GD.Print("梦世界根节点_Ready 执行了！");

		// 等待一帧，确保条件管理器已准备就绪
		await ToSignal(GetTree(), "process_frame");

		// 根据条件管理器中的剧情触发状态决定节点可见性
		bool 剧情已触发 = 条件管理器.实例 != null && 条件管理器.实例.检查条件("剧情_初见玩家");

		// 尝试获取“初见玩家动画”父节点（这是您场景中实际控制动画的容器）
		var 动画父节点 = GetNodeOrNull<Node2D>("初见玩家动画");
		if (动画父节点 != null)
		{
		
			动画父节点.Visible = !剧情已触发;
			GD.Print($"[场景根节点] 初见玩家动画 剧情已触发={剧情已触发}，设置 Visible={!剧情已触发}");
		}
		else
		{
			// 备选：如果找不到父节点，尝试通过导出路径查找（向后兼容）
			if (一次性动画节点Path != null && !一次性动画节点Path.IsEmpty)
			{
				var 动画节点 = GetNodeOrNull<Node2D>(一次性动画节点Path);
				if (动画节点 != null)
				{
					动画节点.Visible = !剧情已触发;
					GD.Print($"[场景根节点] 一次性动画节点 剧情已触发={剧情已触发}，设置 Visible={!剧情已触发}");
				}
				else
				{
					GD.PrintErr("[场景根节点] 未找到任何可控制的动画节点！");
				}
			}
			else
			{
				GD.PrintErr("[场景根节点] 未找到“初见玩家动画”节点，且未指定一次性动画节点路径");
			}
		}
	}
}
