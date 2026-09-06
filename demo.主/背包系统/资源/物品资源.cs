using Godot;

[GlobalClass]
public partial class 物品资源 : Resource
{
	[Export] public Color 物品颜色 { get; set; } = Colors.White;
	[Export] public string 物品名称 { get; set; } = "物品";
	[Export] public int 攻击力 { get; set; } = 0;
	[Export] public int 防御力 { get; set; } = 0;
	[Export] public int 生命值 { get; set; } = 0; // 类字段
	[Export] public int 速度 { get; set; } = 0;
	
	public 物品资源() { }
	
	// 修复：修改冲突的参数名称
	public 物品资源(Color 颜色值, string 名称值, int 攻击值, int 防御值, int 生命值参数, int 速度值)
	{
		物品颜色 = 颜色值;
		物品名称 = 名称值;
		攻击力 = 攻击值;
		防御力 = 防御值;
		生命值 = 生命值参数; // 正确赋值给字段
		速度 = 速度值;
	}
}
