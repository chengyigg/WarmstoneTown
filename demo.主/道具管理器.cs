using Godot;
using System.Collections.Generic;

public partial class 道具管理器 : Node
{
	private static 道具管理器 _实例;
	public static 道具管理器 实例 => _实例;
	
	// 玩家拥有的道具（等待使用的道具）
	private List<道具数据> 待使用道具 = new List<道具数据>();
	
	// 道具替换记录：记录哪个道具替换了哪个选项
	private Dictionary<string, string> 道具替换记录 = new Dictionary<string, string>();
	
	// 已读选项记录：选项ID -> 是否已读
	private Dictionary<string, bool> 已读选项记录 = new Dictionary<string, bool>();
	
	public override void _Ready()
	{
		if (_实例 == null)
		{
			_实例 = this;
			ProcessMode = ProcessModeEnum.Always;
		}
		else
		{
			QueueFree();
		}
	}
	
	// 道具数据结构
	public class 道具数据
	{
		public string 道具ID;
		public string 道具名称;
		public 分支选项 替换选项; // 这个道具提供的新的选项
		
		public 道具数据(string id, string name, 分支选项 选项)
		{
			道具ID = id;
			道具名称 = name;
			替换选项 = 选项;
		}
	}
	
	// 添加道具
	public void 添加道具(string 道具ID, string 道具名称, 分支选项 替换选项)
	{
		var 新道具 = new 道具数据(道具ID, 道具名称, 替换选项);
		待使用道具.Add(新道具);
		GD.Print($"获得道具: {道具名称}，等待替换已读选项");
		
		// 尝试立即使用道具（如果有已读选项）
		尝试使用道具();
	}
	
	// 标记选项为已读
	public void 标记选项已读(string 选项ID)
	{
		if (!已读选项记录.ContainsKey(选项ID))
		{
			已读选项记录[选项ID] = true;
			GD.Print($"选项已读: {选项ID}");
			
			// 有新的已读选项，尝试使用待使用的道具
			尝试使用道具();
		}
	}
	
	// 尝试使用待使用的道具
	private void 尝试使用道具()
	{
		if (待使用道具.Count == 0) return;
		
		// 获取所有已读选项
		List<string> 已读选项列表 = new List<string>();
		foreach (var 记录 in 已读选项记录)
		{
			if (记录.Value) // 如果已读
			{
				已读选项列表.Add(记录.Key);
			}
		}
		
		if (已读选项列表.Count == 0)
		{
			GD.Print("没有已读选项，道具继续等待");
			return;
		}
		
		// 随机选择一个道具使用
		int 随机道具索引 = GD.RandRange(0, 待使用道具.Count - 1);
		var 使用的道具 = 待使用道具[随机道具索引];
		
		// 随机选择一个已读选项进行替换
		int 随机选项索引 = GD.RandRange(0, 已读选项列表.Count - 1);
		string 被替换的选项ID = 已读选项列表[随机选项索引];
		
		// 记录替换关系
		道具替换记录[被替换的选项ID] = 使用的道具.道具ID;
		
		// 从待使用道具中移除
		待使用道具.RemoveAt(随机道具索引);
		
		GD.Print($"道具 [{使用的道具.道具名称}] 替换了选项 [{被替换的选项ID}]");
	}
	
	// 获取经过道具替换后的选项列表
	public Godot.Collections.Array<分支选项> 应用道具替换(Godot.Collections.Array<分支选项> 原始选项)
	{
		var 替换后选项 = new Godot.Collections.Array<分支选项>();
		
		foreach (var 选项 in 原始选项)
		{
			// 检查这个选项是否被道具替换了
			if (道具替换记录.ContainsKey(选项.选项ID))
			{
				// 找到替换这个选项的道具
				string 道具ID = 道具替换记录[选项.选项ID];
				var 替换道具 = 查找道具(道具ID);
				
				if (替换道具 != null)
				{
					替换后选项.Add(替换道具.替换选项);
					GD.Print($"选项 [{选项.选项ID}] 被道具 [{替换道具.道具名称}] 替换");
				}
				else
				{
					// 如果找不到道具，使用原选项
					替换后选项.Add(选项);
				}
			}
			else
			{
				// 没有被替换，使用原选项
				替换后选项.Add(选项);
			}
		}
		
		return 替换后选项;
	}
	
	// 查找道具数据
	private 道具数据 查找道具(string 道具ID)
	{
		// 首先在待使用道具中找
		foreach (var 道具 in 待使用道具)
		{
			if (道具.道具ID == 道具ID) return 道具;
		}
		
		// 如果找不到，说明这个道具已经被使用了，我们需要重新创建（这种情况不应该发生）
		GD.PrintErr($"找不到道具: {道具ID}");
		return null;
	}
	
	// 获取待使用道具数量
	public int 获取待使用道具数量()
	{
		return 待使用道具.Count;
	}
	
	// 获取道具替换信息（用于UI显示）
	public Dictionary<string, string> 获取道具替换信息()
	{
		var 信息 = new Dictionary<string, string>();
		foreach (var 记录 in 道具替换记录)
		{
			var 道具 = 查找道具(记录.Value);
			if (道具 != null)
			{
				信息[记录.Key] = $"被 [{道具.道具名称}] 替换";
			}
		}
		return 信息;
	}
	
	// 重置所有记录（开始新游戏时使用）
	public void 重置()
	{
		待使用道具.Clear();
		道具替换记录.Clear();
		已读选项记录.Clear();
	}
}
