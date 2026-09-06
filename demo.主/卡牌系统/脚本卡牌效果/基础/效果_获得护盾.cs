using Godot;
using System;

public partial class 效果_获得护盾 : RefCounted, I卡牌效果
{
	public string 效果名称 => "获得护盾";

	public void 执行效果(卡牌战斗单位 使用者, 卡牌战斗单位 目标, 卡牌实例 卡牌, 卡牌战斗管理器 战斗管理器)
	{
		int 护盾值 = 卡牌.基础数据.防御值;
		战斗管理器.获得护盾(使用者, 护盾值);
		战斗管理器.添加战斗日志($"{卡牌.基础数据.卡牌名称} 获得 {护盾值} 点护盾");
	}

	public string 获取效果描述()
	{
		return "获得护盾";  // 通用描述
	}
}
