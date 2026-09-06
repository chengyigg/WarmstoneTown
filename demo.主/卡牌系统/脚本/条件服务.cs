using Godot;
using 你的项目.Scripts.资源;
using 你的项目.Scripts.管理器;
using 你的项目.Scripts.全局;

public static class 条件服务
{
	// ===================== 辅助方法 =====================
	
public static int 获取道具数量(string 道具ID字符串)
{
	if (!int.TryParse(道具ID字符串, out int 道具ID))
		return 0;
	var 管理器 = 玩家道具管理器.实例;
	if (管理器 == null) return 0;
	foreach (var 道具 in 管理器.玩家道具列表)
	{
		if (道具.道具ID == 道具ID)
			return 道具.数量;
	}
	return 0;
}




// ★ 辅助方法：根据名字查找怪物配置
private static 怪物配置 查找怪物配置(string 怪物名字)
{
	// 方式1：从卡牌数据管理器上下文获取
	var 队伍 = 卡牌数据管理器.上下文?.临时怪物队伍;
	if (队伍 != null)
	{
		foreach (var 配置 in 队伍.怪物列表)
		{
			if (配置 != null && 配置.名字 == 怪物名字)
				return 配置;
		}
	}

	// 方式2：如果上下文没有，尝试从资源加载（备选）
	// 你可以在这里添加从文件夹加载所有怪物配置的逻辑
	
	GD.PrintErr($"[条件服务] 找不到怪物配置: {怪物名字}");
	return null;
}

	// ===================== 获取实际序列 =====================
public static 对话序列 获取实际序列(对话序列 原始序列)
{
	if (原始序列 == null) return null;

	// 1. 检查条件改变列表
	if (原始序列.条件改变列表 != null && 原始序列.条件改变列表.Count > 0)
	{
		foreach (var 条件项 in 原始序列.条件改变列表)
		{
			if (条件项 == null) continue;

			// ★ 如果已执行（只执行一次且已执行），跳过它，继续检查下一个
			if (条件项.只执行一次 && 是否已执行条件改变项(条件项))
			{
				GD.Print($"[条件服务] 条件改变项已执行，跳过: {条件项.ResourcePath}");
				continue; // ← 改成 continue，而不是 return
			}

			bool 条件满足 = 检查条件是否满足(条件项, 原始序列);
			if (条件满足 && 条件项.目标序列 != null)
			{
				bool 执行成功 = 执行消耗奖励(条件项);
				if (执行成功)
				{
					if (条件项.只执行一次)
						标记条件改变项已执行(条件项);
					GD.Print($"[条件服务] 条件满足并执行成功，返回目标序列: {条件项.目标序列.ResourcePath}");
					return 条件项.目标序列;
				}
				else
				{
					GD.Print($"[条件服务] 条件满足但消耗失败，保持原序列");
					return 原始序列;
				}
			}
		}
	}
		if (原始序列.可改变对话)
		{
			string 路径 = 原始序列.ResourcePath;
			var 当前状态 = 存档管理器.实例?.获取当前状态();
			if (当前状态 != null && 当前状态.获取对话改变状态(路径))
			{
				string 目标路径 = 当前状态.获取对话改变目标序列(路径);
				if (!string.IsNullOrEmpty(目标路径))
				{
					var 目标序列 = GD.Load<对话序列>(目标路径);
					if (目标序列 != null)
					{
						GD.Print($"[条件服务] 使用分支选项指定的目标序列: {目标路径}");
						return 目标序列;
					}
				}
				if (原始序列.改变后序列 != null)
				{
					GD.Print($"[条件服务] 使用对话序列自身的改变后序列: {原始序列.改变后序列.ResourcePath}");
					return 原始序列.改变后序列;
				}
			}
		}

		return 原始序列;
	}

private static bool 检查条件是否满足(条件改变项 条件项, 对话序列 原始序列)
{
	// 优先检查关联任务
	if (条件项.关联任务资源 != null)
	{
		var 管理器 = 任务管理器.实例;
		if (管理器 == null) return false;

		// ★ 先检查任务是否已在列表中（已接取）
		var 任务 = 管理器.任务列表.Find(t => t.原始任务ID == 条件项.关联任务资源.任务ID);
		if (任务 == null)
		{
			// 任务未接取：即使背包有道具，也不应触发完成
			// 但如果是已完成任务（已移除），则检查存档
			if (管理器.任务是否已完成(条件项.关联任务资源.任务ID))
				return true;
			// 未接取且未完成 → 条件不满足
			return false;
		}

		// 任务已接取，检查是否完成（背包数量是否达标）
		if (任务.是否完成)
		{
			管理器.完成任务(任务.原始任务ID);
			return true;
		}
		else
		{
			// 任务未完成，直接检查背包是否满足需求
			bool 背包满足 = 检查背包是否满足任务需求(任务);
			if (背包满足)
			{
				// 标记完成并触发
				任务.是否完成 = true;
				管理器.完成任务(任务.原始任务ID);
				return true;
			}
			return false;
		}
	}

		// 然后检查复合条件
		if (条件项.条件需求列表 != null && 条件项.条件需求列表.Count > 0)
		{
			foreach (var 需求 in 条件项.条件需求列表)
			{
				if (需求.需求类型 == 任务类型枚举.收集道具)
				{
					int 拥有数量 = 获取道具数量(需求.目标ID);
					if (拥有数量 < 需求.所需数量) return false;
				}
		else if (需求.需求类型 == 任务类型枚举.击杀怪物)
{
	// 击杀进度现在由任务系统管理
	// 这里需要检查对应的任务是否已完成
	bool 任务已完成 = 检查击杀任务是否完成(需求.目标ID, 需求.所需数量);
	if (!任务已完成) return false;
}
			}
			return true;
		}

		GD.PrintErr($"[条件服务] 条件项没有条件需求列表，无法检查条件。请使用条件需求列表代替旧字段。");
		return false;
	}
private static bool 检查击杀任务是否完成(string 怪物名称, int 所需数量)
{
	var 管理器 = 任务管理器.实例;
	if (管理器 == null) return false;

	// 查找是否有进行中的任务包含这个击杀需求
	foreach (var 任务 in 管理器.任务列表)
	{
		if (任务.是否完成) continue;

		foreach (var 需求 in 任务.需求列表)
		{
			if (需求.需求类型 == 任务类型枚举.击杀怪物 && 需求.目标ID == 怪物名称)
			{
				// 检查进度是否达标
				int 当前进度 = 任务.获取进度(怪物名称);
				if (当前进度 >= 需求.所需数量)
					return true;
			}
		}
	}
	return false;
}
public static bool 检查背包是否满足任务需求(任务数据 任务)
{
	var 管理器 = 玩家道具管理器.实例;
	if (管理器 == null) return false;

	foreach (var 需求 in 任务.需求列表)
	{
		if (需求.需求类型 != 任务类型枚举.收集道具) continue;
		
		int 背包数量 = 0;
		foreach (var 道具 in 管理器.玩家道具列表)
		{
			if (道具.道具ID.ToString() == 需求.目标ID)
			{
				背包数量 = 道具.数量;
				break;
			}
		}
		if (背包数量 < 需求.所需数量)
			return false;
	}
	return true;
}


	// ===================== 执行消耗奖励 =====================
	public static bool 执行消耗奖励(条件改变项 条件项)
	{
		// 1. 如果启用了"消耗所有任务需求"，从关联任务中读取需求并消耗
		if (条件项.消耗所有任务需求 && 条件项.关联任务资源 != null)
		{
			var 任务 = 条件项.关联任务资源;
			if (任务.需求列表 == null || 任务.需求列表.Count == 0)
			{
				GD.Print("[条件服务] 关联任务没有需求，无需消耗");
			}
			else
			{
				var 管理器 = 玩家道具管理器.实例;
				if (管理器 == null)
				{
					GD.PrintErr("[条件服务] 玩家道具管理器不存在");
					return false;
				}

				// 先检查所有需求是否满足
				foreach (var 需求 in 任务.需求列表)
				{
					if (需求.需求类型 != 任务类型枚举.收集道具) continue;
					int 拥有数量 = 0;
					foreach (var 已有道具 in 管理器.玩家道具列表)
					{
						if (已有道具.道具ID.ToString() == 需求.目标ID)
						{
							拥有数量 = 已有道具.数量;
							break;
						}
					}
					if (拥有数量 < 需求.所需数量)
					{
						GD.Print($"[条件服务] 道具不足：需要 {需求.目标ID} x{需求.所需数量}，拥有 {拥有数量}");
						// 触发消耗失败回调，但不传递具体道具，因为这里可能涉及多个道具
						对话播放器.实例?.触发消耗失败(null);
						return false;
					}
				}

				// 然后消耗
				foreach (var 需求 in 任务.需求列表)
				{
					if (需求.需求类型 != 任务类型枚举.收集道具) continue;
					if (int.TryParse(需求.目标ID, out int 道具ID))
						消耗指定数量道具(道具ID, 需求.所需数量);
					else
						GD.PrintErr($"[条件服务] 无效的道具ID：{需求.目标ID}");
				}
			}
		}

		// 2. 完成关联任务（奖励由任务管理器发放）
		if (条件项.关联任务资源 != null)
		{
			任务管理器.实例?.完成任务(条件项.关联任务资源.任务ID);
		}

		return true;
	}

	// ===================== 辅助方法 =====================
	private static void 消耗指定数量道具(int 道具ID, int 数量)
	{
		var 管理器 = 玩家道具管理器.实例;
		if (管理器 == null) return;
		var 道具列表 = 管理器.玩家道具列表;
		for (int i = 道具列表.Count - 1; i >= 0; i--)
		{
			var 道具 = 道具列表[i];
			if (道具.道具ID == 道具ID)
			{
				int 消耗量 = Mathf.Min(数量, 道具.数量);
				道具.数量 -= 消耗量;
				if (道具.数量 <= 0)
					道具列表.RemoveAt(i);
			}
		}
		管理器.EmitSignal(玩家道具管理器.SignalName.道具列表已更新);
	}

	private static bool 是否已执行条件改变项(条件改变项 条件项)
	{
		if (条件项 == null || !条件项.只执行一次) return false;
		string key = 条件项.ResourcePath;
		if (string.IsNullOrEmpty(key)) return false;
		var 状态 = 存档管理器.实例?.获取当前状态();
		if (状态 == null) return false;
		return 状态.获取条件改变项执行状态(key);
	}

	private static void 标记条件改变项已执行(条件改变项 条件项)
	{
		if (条件项 == null || !条件项.只执行一次) return;
		string key = 条件项.ResourcePath;
		if (string.IsNullOrEmpty(key)) return;
		var 状态 = 存档管理器.实例?.获取当前状态();
		状态?.设置条件改变项执行状态(key, true);
	}
}
