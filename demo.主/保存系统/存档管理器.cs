using Godot;
using 你的项目.Scripts.资源;
using System;
using System.Collections.Generic;
using 你的项目.Scripts.全局;

namespace 你的项目.Scripts.管理器
{
	public partial class 存档管理器 : Node
	{
		private static 存档管理器 _实例;
		public static 存档管理器 实例 => _实例;
		
		private const int 存档数量 = 7;
		private 存档数据资源[] _存档数组 = new 存档数据资源[存档数量];
		
		// 新增：当前游戏状态（所有累积进度）
		private 存档数据资源 _当前状态;

		public override void _Ready()
		{
			if (_实例 == null)
			{
				_实例 = this;
				ProcessMode = ProcessModeEnum.Always;
				_当前状态 = new 存档数据资源(); // 初始化空状态
				加载所有存档();
			}
			else QueueFree();
		}

		/// <summary>
		/// 获取当前游戏状态，供外部写入累积记录
		/// </summary>
		public 存档数据资源 获取当前状态() => _当前状态;

public void 保存游戏(int 存档位, string 场景路径, Vector2 位置, string 存档点名称 = "")
{
	if (存档位 < 0 || 存档位 >= 存档数量)
	{
		GD.PrintErr($"存档管理器: 存档位 {存档位} 无效！");
		return;
	}

	if (场景路径.Contains("黑屏过渡场景") || 场景路径.Contains("过渡场景"))
	{
		GD.PrintErr($"存档管理器: 不允许在过渡场景中保存游戏: {场景路径}");
		return;
	}

	if (_存档数组[存档位] == null)
		_存档数组[存档位] = new 存档数据资源();

	var 存档数据 = _存档数组[存档位];

	// ===== 第一步：复制当前状态的累积数据（子资源） =====
	if (_当前状态 != null)
	{
		存档数据.基础数据 = _当前状态.基础数据.Duplicate() as 存档_基础数据;
		存档数据.场景数据 = _当前状态.场景数据.Duplicate() as 存档_场景数据;
		存档数据.对话数据 = _当前状态.对话数据.Duplicate() as 存档_对话数据;
		存档数据.进度数据 = _当前状态.进度数据.Duplicate() as 存档_进度数据;
		存档数据.收藏数据 = _当前状态.收藏数据.Duplicate() as 存档_收藏数据;

		// ★★★ 关键修复1：显式复制已完成任务ID列表（从当前状态） ★★★
		存档数据.已完成任务ID列表 = new Godot.Collections.Array<int>(_当前状态.已完成任务ID列表);
		GD.Print($"[存档管理器] 保存前复制已完成任务ID列表，数量: {存档数据.已完成任务ID列表.Count}");
	}
	else
	{
		// 如果当前状态为空，则创建新的
		存档数据.已完成任务ID列表 = new Godot.Collections.Array<int>();
	}

	// ★★★ 关键修复2：从任务管理器直接获取已完成任务ID列表（确保数据最新） ★★★
	// 因为任务管理器可能比 _当前状态 更实时，我们合并两者（取并集）
	if (任务管理器.实例 != null)
	{
		// 假设任务管理器有一个公开方法或列表来获取已完成ID，但当前没有，我们只能从 _当前状态 取
		// 为了保险，我们再从 _当前状态 取一次（已经复制过，但如果 _当前状态 没更新，这里可以补充）
		// 但我们可以在任务管理器完成时立即更新 _当前状态，所以这里不必重复
	}

	// ===== 第二步：保存动态数据 =====
	存档数据.设置场景存档位置(场景路径, 位置, 存档点名称);
	存档数据.存档时间 = DateTime.Now;

	if (玩家数据管理器.实例 != null)
		存档数据.基础数据.玩家名字 = 玩家数据管理器.实例.玩家名字;

	// 保存各管理器状态
	保存看法解锁状态(存档数据);
	保存条件状态(存档数据);
	存档数据.基础数据.金币数量 = 金币管理器.实例?.金币数量 ?? 0;

	if (玩家卡组管理器.实例 != null)
	{
		存档数据.收藏数据.卡牌路径列表 = 玩家卡组管理器.实例.获取卡牌路径列表();

		var 商店场景 = GetTree().CurrentScene;
		if (商店场景 != null && 商店场景.SceneFilePath.Contains("商店"))
		{
			var 商店管理器 = 商店场景.GetNodeOrNull<商店管理器>("商店管理器");
			if (商店管理器 != null)
			{
				存档数据.收藏数据.已购买商品路径列表.Clear();
				foreach (var 商品 in 商店管理器.所有商品)
				{
					if (商品.购买按钮 != null && 商品.购买按钮.Disabled)
					{
						var 卡牌路径 = 商品.获取卡牌数据()?.ResourcePath;
						if (!string.IsNullOrEmpty(卡牌路径))
							存档数据.收藏数据.已购买商品路径列表.Add(卡牌路径);
					}
				}
			}
		}
	}

	if (玩家管理器.实例?.当前玩家 != null)
	{
		var 路径列表 = 玩家管理器.实例.当前玩家.获取跟随者预制体路径列表();
		存档数据.收藏数据.保存跟随者列表(路径列表);
	}

	// 保存进行中的任务列表
	存档数据.任务列表 = 任务管理器.实例?.导出到存档() ?? new Godot.Collections.Array<任务数据>();

	// 同步到旧字段（兼容性）
	存档数据.同步到旧字段();

	// 写入文件
	string 存档路径 = 获取存档路径(存档位);
	DirAccess.RemoveAbsolute(存档路径);
	var 错误 = ResourceSaver.Save(存档数据, 存档路径);
	if (错误 == Error.Ok)
	{
		GD.Print($"存档管理器: 游戏已保存到存档位 {存档位}");
		// 重新加载以确保内容一致
		_存档数组[存档位] = ResourceLoader.Load<存档数据资源>(存档路径);
		// 更新当前状态为保存后的数据副本
		_当前状态 = _存档数组[存档位].Duplicate() as 存档数据资源;
		if (_当前状态 == null) _当前状态 = new 存档数据资源();
		GD.Print($"[存档管理器] 保存后当前状态中已完成任务ID列表数量: {_当前状态.已完成任务ID列表.Count}");
	}
	else
		GD.PrintErr($"存档管理器: 保存失败，错误码 {错误}");
}

public void 重新加载存档(int 存档位)
{
	if (存档位 < 0 || 存档位 >= 存档数量) return;
	string 路径 = 获取存档路径(存档位);

	if (ResourceLoader.Exists(路径))
	{
		_存档数组[存档位] = ResourceLoader.Load<存档数据资源>(路径);
		_存档数组[存档位]?.迁移到子资源();

		var 存档数据 = _存档数组[存档位];
		if (存档数据 != null)
		{
			// 恢复任务列表（进行中的任务）
			任务管理器.实例?.从存档恢复(存档数据.任务列表);
			// ★ 触发任务列表更新，刷新所有UI和标记
			任务管理器.实例?.EmitSignal(任务管理器.SignalName.任务列表更新);

			_当前状态 = 存档数据.Duplicate() as 存档数据资源;
			if (_当前状态 == null) _当前状态 = new 存档数据资源();
			GD.Print($"[存档管理器] 加载后当前状态中已完成任务ID列表数量: {_当前状态.已完成任务ID列表.Count}");

			// 应用各管理器状态
			应用条件状态(存档位);
			应用看法解锁状态(存档位);
			应用金币状态(存档位);
			应用卡组状态(存档位);
		}
	}
	else
	{
		_存档数组[存档位] = null;
		_当前状态 = new 存档数据资源();
		任务管理器.实例?.重置();
	}
	
	 // ★ 加载完成后，强制刷新所有对话触发器的标记
	CallDeferred(nameof(延迟刷新所有触发器));
}
		
		public void 应用金币状态(int 存档位)
		{
			var 存档数据 = _存档数组[存档位];
			if (存档数据 != null && 金币管理器.实例 != null)
				金币管理器.实例.金币数量 = 存档数据.金币数量;
		}

		public void 应用卡组状态(int 存档位)
		{
			var 存档数据 = _存档数组[存档位];
			if (存档数据 != null && 玩家卡组管理器.实例 != null)
				玩家卡组管理器.实例.从路径列表重建卡组(存档数据.卡牌路径列表);
		}

		private void 保存看法解锁状态(存档数据资源 存档数据)
		{
			if (回忆系统管理器.实例 == null)
			{
				GD.PrintErr("存档管理器: 回忆系统管理器实例为空，无法保存看法解锁状态");
				return;
			}

			string[] 所有角色ID = 回忆系统管理器.实例.获取所有角色ID();
			foreach (string 角色ID in 所有角色ID)
			{
				var 角色数据 = 回忆系统管理器.实例.获取角色看法(角色ID);
				if (角色数据 != null)
				{
					foreach (看法条目 看法 in 角色数据.看法列表)
					{
						if (看法 != null)
							存档数据.记录看法解锁状态(角色数据.角色名称, 看法.主题, 看法.已解锁);
					}
				}
			}
		}
		
		// 在 存档管理器.cs 中添加
public void 重置内存状态到磁盘(int 存档位)
{
	if (存档位 < 0 || 存档位 >= 存档数量) return;
	string 路径 = 获取存档路径(存档位);
	if (ResourceLoader.Exists(路径))
	{
		var 存档数据 = ResourceLoader.Load<存档数据资源>(路径, "", ResourceLoader.CacheMode.Replace);
		if (存档数据 != null)
		{
			// 手动深拷贝所有子资源
			_当前状态 = new 存档数据资源();
			
			// 基础数据
			if (存档数据.基础数据 != null)
			{
				_当前状态.基础数据 = new 存档_基础数据();
				_当前状态.基础数据.玩家名字 = 存档数据.基础数据.玩家名字;
				_当前状态.基础数据.金币数量 = 存档数据.基础数据.金币数量;
				_当前状态.基础数据.存档时间字符串 = 存档数据.基础数据.存档时间字符串;
			}
			
			// 场景数据
			if (存档数据.场景数据 != null)
			{
				_当前状态.场景数据 = new 存档_场景数据();
				_当前状态.场景数据.存档场景路径 = 存档数据.场景数据.存档场景路径;
				_当前状态.场景数据.存档位置 = 存档数据.场景数据.存档位置;
				_当前状态.场景数据.存档点名称 = 存档数据.场景数据.存档点名称;
				_当前状态.场景数据.场景存档位置 = new Godot.Collections.Dictionary<string, Vector2>(存档数据.场景数据.场景存档位置);
				_当前状态.场景数据.场景存档点 = new Godot.Collections.Dictionary<string, string>(存档数据.场景数据.场景存档点);
			}
			
			// 对话数据
			if (存档数据.对话数据 != null)
			{
				_当前状态.对话数据 = new 存档_对话数据();
				_当前状态.对话数据.已改变对话序列 = new Godot.Collections.Dictionary<string, bool>(存档数据.对话数据.已改变对话序列);
				_当前状态.对话数据.传送后对话触发记录 = new Godot.Collections.Dictionary<string, bool>(存档数据.对话数据.传送后对话触发记录);
				_当前状态.对话数据.对话改变目标序列映射 = new Godot.Collections.Dictionary<string, string>(存档数据.对话数据.对话改变目标序列映射);
				_当前状态.对话数据.已执行条件改变项 = new Godot.Collections.Dictionary<string, bool>(存档数据.对话数据.已执行条件改变项);
				_当前状态.对话数据.击杀条件进度 = new Godot.Collections.Dictionary<string, int>(存档数据.对话数据.击杀条件进度);
			}
			
			// 进度数据
			if (存档数据.进度数据 != null)
			{
				_当前状态.进度数据 = new 存档_进度数据();
				_当前状态.进度数据.已触发动画记录 = new Godot.Collections.Dictionary<string, bool>(存档数据.进度数据.已触发动画记录);
				_当前状态.进度数据.看法解锁状态 = new Godot.Collections.Dictionary<string, bool>(存档数据.进度数据.看法解锁状态);
				_当前状态.进度数据.条件状态记录 = new Godot.Collections.Dictionary<string, bool>(存档数据.进度数据.条件状态记录);
				_当前状态.进度数据.已触发剧情记录 = new Godot.Collections.Dictionary<string, bool>(存档数据.进度数据.已触发剧情记录);
			}
			
			// ★ 收藏数据（手动深拷贝，重点是已拾取道具路径列表）
			if (存档数据.收藏数据 != null)
			{
				_当前状态.收藏数据 = new 存档_收藏数据();
				_当前状态.收藏数据.卡牌路径列表 = new Godot.Collections.Array<string>(存档数据.收藏数据.卡牌路径列表);
				_当前状态.收藏数据.已购买商品路径列表 = new Godot.Collections.Array<string>(存档数据.收藏数据.已购买商品路径列表);
				_当前状态.收藏数据.跟随者预制体路径列表 = new Godot.Collections.Array<string>(存档数据.收藏数据.跟随者预制体路径列表);
				_当前状态.收藏数据.已拾取道具路径列表 = new Godot.Collections.Array<string>(存档数据.收藏数据.已拾取道具路径列表);
			}
			
			// 已完成任务ID列表
			_当前状态.已完成任务ID列表 = new Godot.Collections.Array<int>(存档数据.已完成任务ID列表);
			
			GD.Print($"[存档管理器] 已将内存状态重置为存档位 {存档位} 的磁盘版本");
			GD.Print($"[存档管理器] 已拾取道具路径列表数量: {_当前状态.收藏数据.已拾取道具路径列表.Count}");
			foreach (string 路径项 in _当前状态.收藏数据.已拾取道具路径列表)
				GD.Print($"  路径: {路径项}");
		}
	}
	else
	{
		_当前状态 = new 存档数据资源();
		GD.Print($"[存档管理器] 存档位 {存档位} 不存在，重置为空状态");
	}
}
		
private void 延迟刷新所有触发器()
{
	var 触发器列表 = GetTree().GetNodesInGroup("对话触发器");
	foreach (Node 节点 in 触发器列表)
	{
		if (节点 is 对话触发器 触发器)
		{
			触发器.强制刷新标记();
		}
	}
	GD.Print($"[存档管理器] 已刷新 {触发器列表.Count} 个对话触发器标记");
}
		private void 保存条件状态(存档数据资源 存档数据)
		{
			if (条件管理器.实例 == null)
			{
				GD.PrintErr("存档管理器: 条件管理器实例为空，无法保存条件状态");
				return;
			}

			string[] 所有已满足条件 = 条件管理器.实例.获取所有已满足条件();
			foreach (string 条件 in 所有已满足条件)
			{
				bool 状态 = 条件管理器.实例.检查条件(条件);
				存档数据.记录条件状态(条件, 状态);
			}
		}
		
		public void 应用看法解锁状态(int 存档位)
		{
			if (存档位 < 0 || 存档位 >= 存档数量)
			{
				GD.PrintErr($"存档管理器: 存档位 {存档位} 无效！");
				return;
			}

			var 存档数据 = _存档数组[存档位];
			if (存档数据 == null)
			{
				GD.PrintErr($"存档管理器: 存档位 {存档位} 无存档数据！");
				return;
			}
			
			存档数据.应用看法解锁状态();
		}

		public void 应用条件状态(int 存档位)
		{
			if (存档位 < 0 || 存档位 >= 存档数量)
			{
				GD.PrintErr($"存档管理器: 存档位 {存档位} 无效！");
				return;
			}

			var 存档数据 = _存档数组[存档位];
			if (存档数据 == null)
			{
				GD.PrintErr($"存档管理器: 存档位 {存档位} 无存档数据！");
				return;
			}
			
			if (条件管理器.实例 != null)
				条件管理器.实例.覆盖条件(存档数据.条件状态记录);
		}
		
		public 存档数据资源 获取场景存档数据(int 存档位, string 场景路径)
		{
			if (存档位 < 0 || 存档位 >= 存档数量)
				return null;
			var 存档数据 = _存档数组[存档位];
			return (存档数据 != null && 存档数据.场景是否有存档(场景路径)) ? 存档数据 : null;
		}
		
		public bool 场景是否有存档(int 存档位, string 场景路径)
		{
			if (存档位 < 0 || 存档位 >= 存档数量)
				return false;
			return _存档数组[存档位]?.场景是否有存档(场景路径) ?? false;
		}
		
		public 存档数据资源 获取存档数据(int 存档位)
		{
			if (存档位 < 0 || 存档位 >= 存档数量)
				return null;
			return _存档数组[存档位];
		}
		
		public 存档数据资源[] 获取所有存档数据() => _存档数组;
		
		public bool 是否有存档(int 存档位)
		{
			if (存档位 < 0 || 存档位 >= 存档数量)
				return false;
			return _存档数组[存档位]?.是否有存档() ?? false;
		}
		
		public bool 是否有任何存档()
		{
			for (int i = 0; i < 存档数量; i++)
			{
				if (是否有存档(i))
					return true;
			}
			return false;
		}
		
		public void 同步玩家名字()
		{
			if (玩家数据管理器.实例 != null)
			{
				for (int i = 0; i < _存档数组.Length; i++)
				{
					if (_存档数组[i] != null && _存档数组[i].是否有存档())
					{
						玩家数据管理器.实例.玩家名字 = _存档数组[i].玩家名字;
						break;
					}
				}
			}
		}



// 存档管理器.cs - 加载所有存档()
public void 加载所有存档()
{
	for (int i = 0; i < 存档数量; i++)
	{
		_存档数组[i] = null;
		string 路径 = 获取存档路径(i);
		if (ResourceLoader.Exists(路径))
		{
			_存档数组[i] = ResourceLoader.Load<存档数据资源>(路径);
			
			// ★ 确保这里有迁移调用
			if (_存档数组[i]?.对话数据 != null)
			{
				var 所有怪物配置 = 加载所有怪物配置(); // 需要你实现
				_存档数组[i].对话数据.迁移击杀进度(所有怪物配置);
			}
		}
	}
}

// ★ 新增：迁移击杀进度的方法
private void 迁移存档击杀进度(存档数据资源 存档数据)
{
	if (存档数据?.对话数据 == null) return;

	// 获取所有怪物配置（从资源中加载）
	var 所有怪物配置 = 加载所有怪物配置(); // 需要实现
	存档数据.对话数据.迁移击杀进度(所有怪物配置);
}

private 怪物配置[] 加载所有怪物配置()
{
	var 列表 = new List<怪物配置>();
	
	string 文件夹路径 = "res://怪物配置/"; // 改为你的实际路径
	
var dir = DirAccess.Open(文件夹路径);
if (dir != null)
{
	dir.ListDirBegin();
	string 文件名 = dir.GetNext();
	while (!string.IsNullOrEmpty(文件名))
	{
		if (!dir.CurrentIsDir() && 文件名.EndsWith(".tres"))
		{
			string 完整路径 = 文件夹路径 + 文件名;
			var 配置 = ResourceLoader.Load<怪物配置>(完整路径);
			if (配置 != null)
				列表.Add(配置);
		}
		文件名 = dir.GetNext();
	}
	dir.ListDirEnd();
}
else
{
	GD.PrintErr($"[存档管理器] 怪物配置文件夹不存在: {文件夹路径}");
}
	
	return 列表.ToArray();
}
		
		private string 获取存档路径(int 存档位) => $"user://游戏存档_{存档位}.res";
	}
}
