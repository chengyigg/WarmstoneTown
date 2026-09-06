using Godot;

public partial class 抽牌动作 : RefCounted
{
	public const int 行动值消耗 = 10;

	public static bool 执行(卡牌战斗单位 单位, 卡牌战斗管理器 战斗管理器)
	{
		if (单位.抽牌堆.Count == 0 && 单位.弃牌堆.Count > 0)
			单位.回合开始准备();

		if (单位.抽牌堆.Count == 0)
		{
			战斗管理器.添加战斗日志($"{单位.名字} 牌库已空，无法抽牌！");
			return false;
		}

		var 抽到的牌 = 单位.抽牌();
		if (抽到的牌 == null) return false;

		战斗管理器.添加战斗日志($"{单位.名字} 消耗10行动值抽了一张牌: {抽到的牌.基础数据.卡牌名称}");

		// 如果是玩家单位，更新手牌UI
		if (单位 == 战斗管理器.玩家单位)
		{
			var 主场景 = 战斗管理器.GetParent() as 卡牌游戏主场景;
			主场景?.玩家手牌管理器?.添加卡牌到手中排队(抽到的牌, true);
		}
		else
		{
			// 敌人抽牌，通知其手牌管理器刷新
			战斗管理器.请求更新手牌UI?.Invoke(单位);
		}
		return true;
	}
}
