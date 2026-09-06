using Godot;
using 你的项目.Scripts.资源;

/// <summary>
/// 战斗上下文 - 封装一次战斗中所有临时数据
/// 每次触发战斗时创建新实例，战斗结束后销毁
/// </summary>
public class 战斗上下文
{
	// 奖励相关
	public Godot.Collections.Array<道具数据> 待奖励道具列表 { get; set; }
	public int 战斗胜利金币奖励 { get; set; } = 0;
	public 生成卡组资源 奖励卡组 { get; set; }

	// 怪物队伍
	public 怪物队伍配置 临时怪物队伍 { get; set; }
	public 生成卡组资源 临时敌人卡组 { get; set; }

	// 对话序列改变
	public 对话序列 待改变对话序列 { get; set; }
	public 对话序列 返回后自动触发对话序列 { get; set; }
	public 对话序列 战斗后返回对话序列 { get; set; }

	// 战斗触发
	public string 待触发战斗场景路径 { get; set; } = "";
	public 转场动画资源 待触发战斗转场动画 { get; set; }

	// ★ 战斗胜利后隐藏触发器名称
	public string 战斗胜利后隐藏触发器名称 { get; set; } = "";

	// 教程战斗
	public 教程战斗配置 待启动的教程战斗配置 { get; set; }
	public bool 是否启动教程战斗 => 待启动的教程战斗配置 != null;

	// 返回场景信息
	public bool 从战斗返回 { get; set; } = false;
	public string 返回场景路径 { get; set; } = "res://Scenes/Game/主游戏场景.tscn";
	public Vector2 返回玩家位置 { get; set; } = Vector2.Zero;
	public bool 使用动画结束位置 { get; set; } = false;
	public Vector2 动画结束位置 { get; set; } = Vector2.Zero;

	/// <summary>
	/// 重置所有字段到默认值（战斗结束后调用）
	/// </summary>
	public void 重置()
	{
		待奖励道具列表 = null;
		战斗胜利金币奖励 = 0;
		奖励卡组 = null;
		临时怪物队伍 = null;
		临时敌人卡组 = null;
		待改变对话序列 = null;
		返回后自动触发对话序列 = null;
		战斗后返回对话序列 = null;
		待触发战斗场景路径 = "";
		待触发战斗转场动画 = null;
		战斗胜利后隐藏触发器名称 = "";
		待启动的教程战斗配置 = null;
		从战斗返回 = false;
		返回场景路径 = "res://Scenes/Game/主游戏场景.tscn";
		返回玩家位置 = Vector2.Zero;
		使用动画结束位置 = false;
		动画结束位置 = Vector2.Zero;
	}
}
