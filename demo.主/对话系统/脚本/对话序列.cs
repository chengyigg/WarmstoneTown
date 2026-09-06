using Godot;

[GlobalClass]
public partial class 对话序列 : Resource
{
	[Export] public Godot.Collections.Array<对话句子> Sentences { get; set; } 
		= new Godot.Collections.Array<对话句子>();
	
	[Export] public Godot.Collections.Array<分支选项> Options { get; set; } 
		= new Godot.Collections.Array<分支选项>();
	
	// 原有：可改变对话（基于存档标记）
	[Export] public bool 可改变对话 { get; set; } = false;
	[Export] public 对话序列 改变后序列 { get; set; }

	// ★ 新增：条件改变列表（优先级高于可改变对话）
	[Export] public Godot.Collections.Array<条件改变项> 条件改变列表 { get; set; } 
		= new Godot.Collections.Array<条件改变项>();

	public 对话序列() {}
}
