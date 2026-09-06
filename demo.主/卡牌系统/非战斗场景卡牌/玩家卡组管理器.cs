using Godot;
using System.Collections.Generic;

public partial class 玩家卡组管理器 : Node
{
	public static 玩家卡组管理器 实例 { get; private set; }
	public Godot.Collections.Array<卡牌数据> 玩家初始卡组 { get; set; } = new();

	public override void _EnterTree()
	{
		if (实例 == null)
		{
			实例 = this;
			ProcessMode = ProcessModeEnum.Always;
		}
		else
			QueueFree();
	}
	
	public void 添加卡牌(卡牌数据 新卡牌)
	{
		玩家初始卡组.Add(新卡牌);
	}

	public void 添加卡牌列表(Godot.Collections.Array<卡牌数据> 卡牌列表)
	{
		foreach (var 卡牌 in 卡牌列表)
			if (卡牌 != null) 玩家初始卡组.Add(卡牌);
	}

	public void 从路径列表重建卡组(Godot.Collections.Array<string> 路径列表)
	{
		玩家初始卡组.Clear();
		foreach (string 路径 in 路径列表)
		{
			if (ResourceLoader.Exists(路径))
			{
				var 卡牌 = ResourceLoader.Load<卡牌数据>(路径);
				if (卡牌 != null)
					玩家初始卡组.Add(卡牌);
				else
					GD.PrintErr($"无法加载卡牌资源：{路径}");
			}
			else
			{
				GD.PrintErr($"卡牌资源文件不存在：{路径}");
			}
		}
	}

	public Godot.Collections.Array<string> 获取卡牌路径列表()
	{
		var 路径列表 = new Godot.Collections.Array<string>();
		foreach (var 卡牌 in 玩家初始卡组)
		{
			if (卡牌 != null && ResourceLoader.Exists(卡牌.ResourcePath))
				路径列表.Add(卡牌.ResourcePath);
			else
				GD.PrintErr($"卡牌资源路径无效：{卡牌?.卡牌名称 ?? "null"}");
		}
		return 路径列表;
	}

public override void _Ready()
{
	// 不再自动加载默认卡组，卡牌完全由存档恢复或游戏内获得
	if (玩家初始卡组 == null)
		玩家初始卡组 = new Godot.Collections.Array<卡牌数据>();
	
	// 如果希望从存档恢复，应在此处调用 从路径列表重建卡组(存档数据.卡牌路径列表)
	// 但存档恢复通常由存档管理器在加载存档时调用，因此这里保持空即可
}

	public Godot.Collections.Array<卡牌实例> 创建卡牌实例列表()
	{
		var 实例列表 = new Godot.Collections.Array<卡牌实例>();
		foreach (var 数据 in 玩家初始卡组)
			if (数据 != null) 实例列表.Add(new 卡牌实例(数据));
		return 实例列表;
	}
}
