using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class 生成卡组资源 : Resource
{
	[Export] public Godot.Collections.Array<卡牌数据> 卡组 = new();
}
