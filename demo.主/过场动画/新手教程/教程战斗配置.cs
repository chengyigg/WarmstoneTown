using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class 教程战斗配置 : Resource
{
	[Export] public string 教程ID = "新手教程";
	
	// 玩家预设抽牌序列（按顺序抽这些卡，抽完后回归正常随机）
	[Export] public Godot.Collections.Array<卡牌数据> 玩家抽牌序列;
	
	// 敌人预设抽牌序列
	[Export] public Godot.Collections.Array<卡牌数据> 敌人抽牌序列;
	
	// 战斗中显示的教程对话序列（包含高亮、抽牌、打牌等交互句子）
	[Export] public 对话序列 教程对话序列;
}
