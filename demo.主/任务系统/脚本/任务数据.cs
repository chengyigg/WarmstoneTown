using Godot;
using 你的项目.Scripts.资源;

// ★ 将枚举移到类外部，方便任务需求引用
public enum 任务类型枚举
{
	击杀怪物,
	收集道具
}

[GlobalClass]
public partial class 任务数据 : Resource
{
	[Export] public int 原始任务ID { get; set; } = -1;
	[Export] public int 任务ID { get; set; }
	[Export] public string 任务名称 { get; set; } = "";
	[Export] public string 任务描述 { get; set; } = "";
	[Export] public Texture2D 任务图标 { get; set; }

	// ★ 运行时状态（检查器中可见，但请勿手动修改）
	[Export] public bool 是否完成 { get; set; } = false;
	[Export] public Godot.Collections.Dictionary<string, int> 进度 { get; set; } 
		= new Godot.Collections.Dictionary<string, int>();

	// 奖励
	[Export] public int 奖励金币 { get; set; } = 0;
	[Export] public Godot.Collections.Array<道具数据> 奖励道具列表 { get; set; }
	[Export] public 生成卡组资源 奖励卡组 { get; set; }

	[Export] public string 来源NPC { get; set; } = "";
	[Export] public AudioStream 完成音效;

	// ★ 新增：任务完成时自动改变对话序列
	[Export] public string 完成时源序列路径 { get; set; } = "";
	[Export] public string 完成时目标序列路径 { get; set; } = "";

	// ★ 需求列表
	[Export] public Godot.Collections.Array<任务需求> 需求列表 { get; set; } 
		= new Godot.Collections.Array<任务需求>();

	public 任务数据 克隆()
	{
		return new 任务数据
		{
			任务ID = this.任务ID,
			任务名称 = this.任务名称,
			任务描述 = this.任务描述,
			任务图标 = this.任务图标,
			是否完成 = this.是否完成,
			进度 = new Godot.Collections.Dictionary<string, int>(this.进度),
			奖励金币 = this.奖励金币,
			奖励道具列表 = this.奖励道具列表,
			奖励卡组 = this.奖励卡组,
			来源NPC = this.来源NPC,
			完成音效 = this.完成音效,
			需求列表 = this.需求列表,
			// ★ 新增：复制这两个字段
			完成时源序列路径 = this.完成时源序列路径,
			完成时目标序列路径 = this.完成时目标序列路径
		};
	}

	public bool 所有需求已完成()
	{
		foreach (var 需求 in 需求列表)
		{
			int 当前 = 进度.ContainsKey(需求.目标ID) ? 进度[需求.目标ID] : 0;
			if (当前 < 需求.所需数量) return false;
		}
		return true;
	}

	public void 更新进度(string 目标ID, int 增量 = 1)
	{
		if (!进度.ContainsKey(目标ID)) 进度[目标ID] = 0;
		进度[目标ID] += 增量;
		foreach (var 需求 in 需求列表)
		{
			if (需求.目标ID == 目标ID && 进度[目标ID] > 需求.所需数量)
				进度[目标ID] = 需求.所需数量;
		}
	}

	public int 获取进度(string 目标ID)
	{
		return 进度.ContainsKey(目标ID) ? 进度[目标ID] : 0;
	}
}
