using Godot;

public partial class 效果_影袭 : RefCounted, I卡牌效果
{
	public string 效果名称 => "影袭";
	public void 执行效果(卡牌战斗单位 使用者, 卡牌战斗单位 目标, 卡牌实例 卡牌, 卡牌战斗管理器 战斗管理器)
	{
		int 伤害 = 4;
		战斗管理器.造成伤害(使用者, 目标, 伤害);
		战斗管理器.添加战斗日志($"影袭造成{伤害}点伤害");
	}
	public string 获取效果描述() => "造成4点伤害";
}
