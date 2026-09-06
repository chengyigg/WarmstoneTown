using Godot;
using System.Collections.Generic;

public partial class 效果_锋利匕首 : Node, I卡牌效果
{
	public string 效果名称 => "锋利匕首";

	private static Dictionary<卡牌战斗单位, int> 常驻加成 = new();
	private static Dictionary<卡牌战斗单位, bool> 首次已用 = new();

	public string 获取效果描述()
	{
		return "攻击力+1，每回合首次攻击额外造成3点伤害。";
	}

	public void 执行效果(卡牌战斗单位 使用者, 卡牌战斗单位 目标, 卡牌实例 卡牌, 卡牌战斗管理器 战斗管理器)
	{
		if (使用者 == null) return;

		if (!常驻加成.ContainsKey(使用者))
			常驻加成[使用者] = 0;
		常驻加成[使用者] += 1;

		首次已用[使用者] = false;

		装备管理器.实例?.添加装备(使用者, 卡牌, "锋利匕首");

		战斗管理器.添加战斗日志($"{使用者} 装备了锋利匕首，攻击力+1，每回合首次攻击额外+3");
	}

	public static int 获取攻击加成(卡牌战斗单位 单位)
	{
		int 加成 = 0;
		if (常驻加成.ContainsKey(单位))
			加成 += 常驻加成[单位];
		if (首次已用.ContainsKey(单位) && !首次已用[单位])
		{
			加成 += 3;
			首次已用[单位] = true;
			GD.Print($"{单位} 触发锋利匕首首次额外伤害 +3");
		}
		return 加成;
	}

	public static void 重置回合标记(卡牌战斗单位 单位)
	{
		if (首次已用.ContainsKey(单位))
		{
			首次已用[单位] = false;
			GD.Print($"{单位} 的锋利匕首首次攻击标记已重置");
		}
	}

	public static void 移除(卡牌战斗单位 单位)
	{
		常驻加成.Remove(单位);
		首次已用.Remove(单位);
	}
}
