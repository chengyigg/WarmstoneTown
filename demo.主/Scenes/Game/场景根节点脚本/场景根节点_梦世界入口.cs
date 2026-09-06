using Godot;
using 你的项目.Scripts.管理器;
using System.Threading.Tasks;

public partial class 场景根节点_梦世界入口 : Node2D
{
	public override async void _Ready()
	{
		await ToSignal(GetTree(), "process_frame");
		刷新所有触发器();
	}

	// 新增：公共刷新方法
	public void 刷新所有触发器()
	{
		var 触发器列表 = GetTree().GetNodesInGroup("对话触发器");
		foreach (Node 节点 in 触发器列表)
		{
			if (节点 is 对话触发器 触发器)
			{
				触发器.强制刷新条件();
				GD.Print($"刷新触发器: {触发器.Name}");
			}
		}
	}
}
