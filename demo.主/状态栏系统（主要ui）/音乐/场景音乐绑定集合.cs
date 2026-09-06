using Godot;

[GlobalClass]
public partial class 场景音乐绑定集合 : Resource
{
	[Export] public Godot.Collections.Array<场景音乐绑定条目> 绑定列表 = new();
}
