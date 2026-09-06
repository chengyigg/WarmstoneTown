using Godot;
using System.Collections.Generic;

public partial class Buff系统 : Node
{
	private Dictionary<卡牌战斗单位, List<Buff>> 单位Buff列表 = new();

	public class Buff
	{
		public string 名称;
		public int 剩余次数;    // -1 表示无限次数（整回合）
		public float 持续时间;  // 秒，暂未用
		public System.Action<卡牌战斗单位> 生效回调;
		public System.Action<卡牌战斗单位> 移除回调;

		public Buff(string 名称, int 次数 = -1)
		{
			this.名称 = 名称;
			this.剩余次数 = 次数;
		}
	}

	public void 添加Buff(卡牌战斗单位 单位, string 名称, int 次数 = -1)
	{
		if (!单位Buff列表.ContainsKey(单位)) 单位Buff列表[单位] = new List<Buff>();
		var 已有 = 单位Buff列表[单位].Find(b => b.名称 == 名称);
		if (已有 != null) 已有.剩余次数 = 次数;
		else 单位Buff列表[单位].Add(new Buff(名称, 次数));
	}

	public bool 有Buff(卡牌战斗单位 单位, string 名称)
	{
		return 单位Buff列表.ContainsKey(单位) && 单位Buff列表[单位].Exists(b => b.名称 == 名称 && b.剩余次数 != 0);
	}

	public void 消耗Buff(卡牌战斗单位 单位, string 名称)
	{
		if (!单位Buff列表.ContainsKey(单位)) return;
		var buff = 单位Buff列表[单位].Find(b => b.名称 == 名称);
		if (buff != null && buff.剩余次数 > 0)
		{
			buff.剩余次数--;
			if (buff.剩余次数 == 0) 单位Buff列表[单位].Remove(buff);
		}
	}

	public void 清除回合Buff(卡牌战斗单位 单位)
	{
		if (单位Buff列表.ContainsKey(单位))
			单位Buff列表[单位].RemoveAll(b => b.名称 == "架势");
	}
}
