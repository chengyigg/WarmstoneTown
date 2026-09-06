using Godot;
using System.Collections.Generic;
using 你的项目.Scripts.资源;

[GlobalClass]
public partial class 分支选项 : Resource
{
	// ==================== 基本设置 ====================
	[ExportGroup("基本设置")]
	[Export] public string 选项文本 { get; set; } = "";
	[Export] public string 选项ID { get; set; } = "";
	[Export] public 对话序列 目标序列 { get; set; }

	// ==================== 场景展示 ====================
	[ExportGroup("场景展示")]
	[Export] public bool 显示场景 { get; set; } = false;
	[Export] public string 场景路径 { get; set; } = "";
	[Export] public 对话序列 场景后对话序列 { get; set; }

	// ==================== 场景切换（传送） ====================
	[ExportGroup("场景切换")]
	[ExportSubgroup("传送设置")]
	[Export] public bool 触发场景切换 { get; set; } = false;
	[Export] public string 目标场景路径 { get; set; } = "";
	[Export] public string 目标传送点名称 { get; set; } = "";
	[Export] public 转场动画资源 转场动画配置 { get; set; }
	[Export] public bool 返回原地 { get; set; } = false;

	[ExportSubgroup("传送后对话")]
	[Export] public bool 传送后触发对话 { get; set; } = false;
	[Export] public 对话序列 传送后对话序列 { get; set; }
	[Export] public bool 是否只能触发一次 { get; set; } = false;
	[Export] public bool 是否回到选项 { get; set; } = false;

	// ==================== 战斗设置 ====================
	[ExportGroup("战斗设置")]
	[ExportSubgroup("触发战斗")]
	[Export] public bool 对话后触发战斗 { get; set; } = false;
	[Export] public 怪物队伍配置 怪物队伍 { get; set; }
	[Export] public 教程战斗配置 教程战斗配置 { get; set; }

	[ExportSubgroup("战斗后切换场景")]
	[Export] public bool 对话后触发场景切换 { get; set; } = false;
	[Export] public string 对话后目标场景路径 { get; set; } = "";

	[ExportSubgroup("战斗后改变对话")]
	[Export] public 对话序列 战斗胜利后改变对话序列 { get; set; }
	[Export] public bool 战斗胜利后改变当前对话 { get; set; } = false;

	[ExportSubgroup("战斗胜利返回")]
	[Export] public 对话序列 战斗胜利后返回对话序列 { get; set; }

	// ==================== 奖励 ====================
	[ExportGroup("奖励")]
	[Export] public int 战斗胜利金币奖励 { get; set; } = 0;
	[Export] public 生成卡组资源 奖励卡组 { get; set; }
	[Export] public Godot.Collections.Array<道具数据> 胜利后获得道具列表 { get; set; }
	[Export] public Godot.Collections.Array<道具数据> 选择后获得道具列表 { get; set; }

	// ==================== 条件与存档 ====================
	[ExportGroup("条件与存档")]
	[Export] public string 触发条件名称 { get; set; } = "";
	[Export] public bool 允许存档 { get; set; } = false;

	// ==================== 选项行为 ====================
	[ExportGroup("选项行为")]
	[Export] public string 选项触发后改变对话序列路径 { get; set; } = "";
	[Export] public 对话序列 选项触发后目标改变序列 { get; set; }

	// ★ 接取任务
	[Export] public 任务数据 接取任务资源 { get; set; }
	
	[ExportGroup("任务完成行为")]
	[Export] public 对话序列 任务完成后目标改变序列 { get; set; }
	[Export] public string 任务完成后要改变的源序列路径 { get; set; } = "";

	// ★ 新增：战斗胜利后自动隐藏对话触发器（使用名称）
	[ExportGroup("战斗胜利后自动隐藏")]
	[Export] public bool 战斗胜利后消失 { get; set; } = false;
	[Export] public string 要隐藏的触发器名称 { get; set; } = "";   // 填触发器的 Name

	public 分支选项() { }
}
