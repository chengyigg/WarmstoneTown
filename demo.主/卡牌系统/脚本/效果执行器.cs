using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class 效果执行器 : Node
{
	private static 效果执行器 _实例;
	public static 效果执行器 实例 => _实例;
	
	public override void _EnterTree()
	{
		if (_实例 == null) _实例 = this;
	}
	
	public async void 执行观察弱点效果(卡牌战斗单位 使用者, 卡牌战斗单位 目标, 卡牌实例 卡牌, 卡牌战斗管理器 战斗管理器, 手牌管理器 手牌管理器)
	{
		await 执行普通抽牌流程(使用者, 战斗管理器, 手牌管理器);
		await ToSignal(GetTree().CreateTimer(0.6f), "timeout");
		await 执行普通抽牌流程(使用者, 战斗管理器, 手牌管理器);
		观察弱点状态.玩家下次攻击伤害加成 = 3;
		战斗管理器.添加战斗日志("观察弱点效果：发现了敌人的弱点！下次攻击伤害+3");
		战斗管理器.更新UI();
	}
	
	private async Task 执行普通抽牌流程(卡牌战斗单位 使用者, 卡牌战斗管理器 战斗管理器, 手牌管理器 手牌管理器)
	{
		if (使用者.抽牌堆.Count == 0 && 使用者.弃牌堆.Count > 0)
			使用者.回合开始准备();
		if (使用者.抽牌堆.Count > 0)
		{
			var 抽到的卡牌 = 使用者.抽牌();
			if (抽到的卡牌 != null)
			{
				战斗管理器.添加战斗日志($"抽到了：{抽到的卡牌.基础数据?.卡牌名称}");
				手牌管理器.添加卡牌到手中(抽到的卡牌, true);
			}
		}
		else
		{
			战斗管理器.添加战斗日志("牌库已空，无法抽牌");
		}
	}
}
