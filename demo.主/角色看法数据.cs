using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class 角色看法数据 : Resource
{
	[Export] public string 角色名称 { get; set; } = "";
	[Export] public string 角色头像路径 { get; set; } = "";
	[Export] public Godot.Collections.Array<看法条目> 看法列表 { get; set; } 
		= new Godot.Collections.Array<看法条目>();
}
