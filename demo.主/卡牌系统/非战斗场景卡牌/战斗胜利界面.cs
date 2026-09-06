using Godot;
using 你的项目.Scripts.管理器;
using 你的项目.Scripts.全局;

public partial class 战斗胜利界面 : CanvasLayer
{
	[Export] private string 返回场景路径 = "res://Scenes/Game/主游戏场景.tscn";
	[Export] private NodePath 战斗管理器路径;
	[Export] private bool 自动连接战斗管理器 = true;
	[Export] private Button 返回按钮;
	[Export] public int 奖励金币数量 = 10;

	private bool 已显示 = false;

	public override void _Ready()
	{
		if (返回按钮 != null)
		{
			返回按钮.Pressed += On返回按钮Pressed;
		}
		else
		{
			GD.PrintErr("战斗胜利界面: 未找到返回按钮，请检查节点路径");
		}

		if (自动连接战斗管理器)
			连接战斗管理器信号();
	}

	private void 连接战斗管理器信号()
	{
		卡牌战斗管理器 战斗管理器 = null;
		if (!string.IsNullOrEmpty(战斗管理器路径))
			战斗管理器 = GetNode<卡牌战斗管理器>(战斗管理器路径);
		else
			战斗管理器 = GetTree().Root.FindChild("卡牌战斗管理器", true, false) as 卡牌战斗管理器;

		if (战斗管理器 != null)
		{
			战斗管理器.战斗结束 -= On战斗结束; // 避免重复
			战斗管理器.战斗结束 += On战斗结束;
		}
		else
		{
			GD.PrintErr("战斗胜利界面: 未找到战斗管理器节点");
		}
	}

	private void On战斗结束(bool 玩家胜利)
	{
		if (玩家胜利 && !已显示)
		{
		  int 奖励 = 卡牌数据管理器.上下文.战斗胜利金币奖励;  // ★ 改这里
		卡牌数据管理器.上下文.战斗胜利金币奖励 = 0;        // ★ 改这里
			if (金币管理器.实例 != null)
				金币管理器.实例.增加金币(奖励);
			显示胜利界面();
		}
	}

	private void 显示胜利界面()
	{
		已显示 = true;
		Visible = true;
	}

	private void On返回按钮Pressed()
{
	var 自动对话 = 卡牌数据管理器.上下文.返回后自动触发对话序列;
	卡牌数据管理器.上下文.返回后自动触发对话序列 = null;

	战斗服务.返回场景(
		卡牌数据管理器.上下文.返回场景路径,
		自动对话 != null,
		自动对话,
		卡牌数据管理器.当前存档位
	);

	Visible = false;
}
}
