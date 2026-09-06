using Godot;
using System.Collections.Generic;

public partial class 装备管理器 : Node
{
	private static 装备管理器 _实例;
	public static 装备管理器 实例 => _实例;
	
	private Dictionary<卡牌战斗单位, List<装备状态>> 单位装备 = new();
	
	public override void _EnterTree()
	{
		if (_实例 == null) _实例 = this;
	}
	
	public class 装备状态
	{
		public string 装备名称;
		public 卡牌实例 装备卡牌;
		public Dictionary<string, object> 装备效果;
		
		public 装备状态(string 名称, 卡牌实例 卡牌)
		{
			装备名称 = 名称;
			装备卡牌 = 卡牌;
			装备效果 = new Dictionary<string, object>();
		}
		
		public void 设置效果(string 键, object 值) => 装备效果[键] = 值;
		public T 获取效果<T>(string 键) => 装备效果.ContainsKey(键) ? (T)装备效果[键] : default(T);
	}

	public void 添加装备(卡牌战斗单位 单位, 卡牌实例 装备卡牌, string 装备名称)
	{
		if (!单位装备.ContainsKey(单位)) 单位装备[单位] = new List<装备状态>();
		var 新装备 = new 装备状态(装备名称, 装备卡牌);
		单位装备[单位].Add(新装备);
		触发装备生效(单位, 新装备);
	}
	
	public bool 有装备(卡牌战斗单位 单位, string 装备名称)
	{
		if (!单位装备.ContainsKey(单位)) return false;
		foreach (var 装备 in 单位装备[单位])
			if (装备.装备名称 == 装备名称) return true;
		return false;
	}
	
	public 装备状态 获取装备(卡牌战斗单位 单位, string 装备名称)
	{
		if (!单位装备.ContainsKey(单位)) return null;
		foreach (var 装备 in 单位装备[单位])
			if (装备.装备名称 == 装备名称) return 装备;
		return null;
	}
	
	private void 触发装备生效(卡牌战斗单位 单位, 装备状态 装备) { }
	
	public void 通知攻击使用(卡牌战斗单位 单位)
	{
		if (!单位装备.ContainsKey(单位)) return;
		foreach (var 装备 in 单位装备[单位])
		{
			if (装备.装备名称 == "勇气徽章")
			{
				int 当前计数 = 装备.获取效果<int>("攻击计数") + 1;
				装备.设置效果("攻击计数", 当前计数);
				if (当前计数 >= 3)
				{
					装备.设置效果("攻击计数", 0);
					触发抽牌效果(单位);
				}
			}
		}
	}

	private void 触发抽牌效果(卡牌战斗单位 单位)
	{
		EmitSignal(SignalName.装备触发抽牌, 单位);
	}
	
	public void 处理回合开始(卡牌战斗单位 单位, 卡牌战斗管理器 战斗管理器)
	{
		if (!单位装备.ContainsKey(单位)) return;
		foreach (var 装备 in 单位装备[单位])
		{
			if (装备.装备名称 == "坚固盾牌")
			{
				单位.护盾值 += 3;
				战斗管理器?.添加战斗日志("坚固盾牌效果：获得3点护甲");
				战斗管理器?.更新UI();
			}
		}
	}
	
	public int 获取防御牌护甲加成(卡牌战斗单位 单位)
	{
		if (!单位装备.ContainsKey(单位)) return 0;
		foreach (var 装备 in 单位装备[单位])
			if (装备.装备名称 == "坚固盾牌") return 3;
		return 0;
	}
	
	public bool 有坚固盾牌(卡牌战斗单位 单位) => 有装备(单位, "坚固盾牌");
	
	[Signal] public delegate void 装备触发抽牌EventHandler(卡牌战斗单位 单位);
}
