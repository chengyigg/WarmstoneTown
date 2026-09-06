using Godot;

[GlobalClass]
public partial class 条件改变项 : Resource
{
	public enum 条件类型枚举
	{
		拥有道具,
		击杀数量,
	}

	// 旧字段（兼容旧存档，建议逐步迁移到条件需求列表）
	[Export] public 条件类型枚举 条件类型 { get; set; } = 条件类型枚举.拥有道具;
	[Export] public string 条件参数 { get; set; } = "";
	[Export] public string 目标怪物ID { get; set; } = "";
	[Export] public int 所需击杀数量 { get; set; } = 1;

	[Export] public 对话序列 目标序列 { get; set; }
	// ❌ 删除：public Godot.Collections.Array<道具数据> 消耗道具列表 { get; set; }
	[Export] public bool 只执行一次 = true;
	[Export] public 任务数据 关联任务资源 { get; set; }

	// ★ 新增：复合条件需求列表（新，优先使用）
	[ExportGroup("复合条件（新，优先使用）")]
	[Export] public Godot.Collections.Array<任务需求> 条件需求列表 { get; set; } 
		= new Godot.Collections.Array<任务需求>();

	// ★ 新增：消耗所有任务需求（关联任务时有效）
	[ExportGroup("消耗设置")]
	[Export] public bool 消耗所有任务需求 { get; set; } = false;
}
