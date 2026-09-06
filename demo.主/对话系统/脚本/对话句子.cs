using Godot;
using System;

[GlobalClass]
public partial class 对话句子 : Resource
{
	[Export] public string Text { get; set; } = "";
	[Export] public string SpeakerName { get; set; } = "";
	[Export] public Texture2D Avatar { get; set; }
	[Export] public Godot.Collections.Dictionary<string, Variant> StyleProperties { get; set; }
	[Export] public Godot.Collections.Array<PackedScene> 添加的跟随者预制体列表 { get; set; }
	[Export] public bool 移除所有跟随者 { get; set; } = false;

	public enum 交互类型枚举
	{
		无,
		高亮或动画,
		等待行动条满
	}

	[Export] public 交互类型枚举 交互类型 = 交互类型枚举.无;

	// ===== 高亮或动画 专用字段 =====
	[Export] public string 高亮控件路径 { get; set; } = "";
	[Export] public string 预定义动画名称 { get; set; } = "";
	[Export] public string 高亮提示文本 { get; set; } = "";

	public 对话句子() { }
}
