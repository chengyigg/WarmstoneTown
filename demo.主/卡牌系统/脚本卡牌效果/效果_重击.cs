using Godot;

public partial class 效果_重击 : RefCounted, I卡牌效果
{
	public string 效果名称 => "重击";

	public void 执行效果(卡牌战斗单位 使用者, 卡牌战斗单位 目标, 卡牌实例 卡牌, 卡牌战斗管理器 战斗管理器)
	{
		int 基础伤害 = 12;
		// 调用统一伤害方法，无视护甲和减伤
		战斗管理器.造成伤害(使用者, 目标, 基础伤害, 无视护甲: true, 无视减伤: true);
		// 日志已在造成伤害中输出，此处可省略
	}

	public string 获取效果描述() => "造成12点伤害，无视护甲，无视伤害减半类效果";
}
