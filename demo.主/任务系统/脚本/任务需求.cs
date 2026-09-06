using Godot;

[GlobalClass]
public partial class 任务需求 : Resource
{
	[Export] public 任务类型枚举 需求类型;  // 现在可以找到了
	[Export] public string 目标ID;
	[Export] public int 所需数量;
}
