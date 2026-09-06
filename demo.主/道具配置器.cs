using Godot;
using System;
// 道具配置器.cs - 用于在编辑器中配置道具
[GlobalClass]
public partial class 道具配置器 : Resource
{
	[Export] public Godot.Collections.Dictionary<string, 道具配置> 道具配置表 
		= new Godot.Collections.Dictionary<string, 道具配置>();
}

[GlobalClass]
public partial class 道具配置 : Resource
{
	[Export] public string 道具名称 { get; set; } = "";
	[Export] public string 道具描述 { get; set; } = "";
	[Export] public Texture2D 道具图标 { get; set; }
	[Export] public 分支选项 替换选项 { get; set; }
}
