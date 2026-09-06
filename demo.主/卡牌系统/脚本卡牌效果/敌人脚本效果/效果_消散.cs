using Godot;

public partial class 效果_消散 : RefCounted, I卡牌效果
{
	public string 效果名称 => "消散";
	public void 执行效果(卡牌战斗单位 使用者, 卡牌战斗单位 目标, 卡牌实例 卡牌, 卡牌战斗管理器 战斗管理器)
	{
		// 给使用者（敌人）添加一个单次伤害减半的buff
		var buff系统 = 战斗管理器.GetNodeOrNull<Buff系统>("Buff系统");
		if (buff系统 == null)
		{
			buff系统 = new Buff系统();
			战斗管理器.AddChild(buff系统);
		}
		buff系统.添加Buff(使用者, "消散", 1);
		战斗管理器.添加战斗日志($"{使用者.名字} 进入消散状态，下次受到伤害减半");
	}
	public string 获取效果描述() => "本回合内受到的下一次攻击伤害减半";
}
