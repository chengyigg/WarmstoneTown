using Godot;

[GlobalClass]
public partial class 怪物队伍配置 : Resource
{
	[Export] public Godot.Collections.Array<怪物配置> 怪物列表 = new();
}
