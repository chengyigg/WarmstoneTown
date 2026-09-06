using Godot;

[GlobalClass]
public partial class 看法条目 : Resource
{
	[Export] public string 主题 { get; set; } = "";
	[Export] public string 内容 { get; set; } = "";
	[Export] public bool 已解锁 { get; set; } = false;
	
	// 新增：解锁条件
	[Export] public string 解锁条件 { get; set; } = "";
	
	// 新增：检查是否满足条件的方法
	public bool 是否满足条件()
{
	// 如果解锁条件为空，直接返回true
	if (string.IsNullOrEmpty(解锁条件))
	{
		return true;
	}
	
	// 如果解锁条件是"测试"，检查条件管理器
	if (解锁条件 == "测试")
	{
		if (条件管理器.实例 != null)
		{
			bool 满足 = 条件管理器.实例.检查条件("测试");
			GD.Print($"看法条目: 检查条件'测试' = {满足}");
			return 满足;
		}
		return false;
	}
	
	// 其他条件使用条件管理器检查
	if (条件管理器.实例 != null)
	{
		return 条件管理器.实例.检查条件(解锁条件);
	}
	
	return false; // 没有条件管理器，默认不满足
}
}
