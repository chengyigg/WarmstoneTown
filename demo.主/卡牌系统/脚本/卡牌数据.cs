using Godot;
using System;

[GlobalClass]
public partial class 卡牌数据 : Resource
{
	// 在[GlobalClass]声明下方添加这些属性
	[Export] public Godot.Collections.Array<卡牌效果数据> 效果数据列表 = new();
	
	[Export] public string 卡牌名称 = "";
	[Export] public string 卡牌描述 = "";
	 [Export] public string 强化描述 = ""; // 强化后的描述，为空则显示原描述
	[Export] public Texture2D 卡面贴图;
	[Export] public Texture2D 卡背贴图;
	[Export] public Texture2D 卡牌图标; // 小图标，用于使用记录
	// 战斗属性
	[Export] public 卡牌类型 类型 = 卡牌类型.攻击;
	[Export] public int 基础伤害 = 0;
	[Export] public int 防御值 = 0;  

	[Export] public int 行动值消耗 = 15;   // 默认15，可单独配置

	
	public enum 卡牌类型
	{
		攻击,
		防御,
		特殊,
		情绪,
		装备  // 添加装备类型
	}
}
