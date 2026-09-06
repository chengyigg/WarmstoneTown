using Godot;
using System;

public partial class 效果_抽牌 : RefCounted, I卡牌效果
{
	public string 效果名称 => "抽牌";

	[Export] public int 抽牌数量 { get; set; } = 1;

	public void 执行效果(卡牌战斗单位 使用者, 卡牌战斗单位 目标, 卡牌实例 卡牌, 卡牌战斗管理器 战斗管理器)
	{
		for (int i = 0; i < 抽牌数量; i++)
		{
			var 抽到的牌 = 使用者.抽牌();
			if (抽到的牌 != null && 使用者 == 战斗管理器.玩家单位)
			{
				var 主场景 = 战斗管理器.GetParent() as 卡牌游戏主场景;
				主场景?.玩家手牌管理器?.添加卡牌到手中排队(抽到的牌, true);
			}
		}
		战斗管理器.添加战斗日志($"抽取 {抽牌数量} 张牌");
	}

	public string 获取效果描述()
	{
		return $"抽 {抽牌数量} 张牌";
	}
}
