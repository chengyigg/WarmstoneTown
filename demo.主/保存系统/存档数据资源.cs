using Godot;
using System;
using System.Collections.Generic;

namespace 你的项目.Scripts.资源
{
	[GlobalClass]
	public partial class 存档数据资源 : Resource
	{
		// ==================== 旧字段（保留兼容，标记为过期） ====================
		[Export] public Godot.Collections.Dictionary<string, bool> 已触发动画记录 { get; set; } = new();
		[Export] public string 存档场景路径 { get; set; } = "";
		[Export] public Vector2 存档位置 { get; set; } = Vector2.Zero;
		[Export] public string 存档点名称 { get; set; } = "";
		[Export] public string 玩家名字 { get; set; } = "玩家";
		[Export] public int 金币数量 { get; set; } = 0;
		[Export] public string 存档时间字符串 { get; set; } = "";
		[Export] public Godot.Collections.Dictionary<string, Vector2> 场景存档位置 { get; set; } = new();
		[Export] public Godot.Collections.Dictionary<string, string> 场景存档点 { get; set; } = new();
		[Export] public Godot.Collections.Dictionary<string, bool> 传送后对话触发记录 { get; set; } = new();
		[Export] public Godot.Collections.Dictionary<string, bool> 看法解锁状态 { get; set; } = new();
		[Export] public Godot.Collections.Array<string> 跟随者预制体路径列表 { get; set; } = new();
		[Export] public Godot.Collections.Dictionary<string, bool> 已改变对话序列 { get; set; } = new();
		[Export] public Godot.Collections.Dictionary<string, bool> 条件状态记录 { get; set; } = new();
		[Export] public Godot.Collections.Array<string> 已购买商品路径列表 { get; set; } = new();
		[Export] public Godot.Collections.Array<string> 卡牌路径列表 { get; set; } = new();
		[Export] public Godot.Collections.Dictionary<string, bool> 已触发剧情记录 { get; set; } = new();
		[Export] public Godot.Collections.Dictionary<string, bool> 已执行条件改变项 { get; set; } = new();
		[Export] public Godot.Collections.Dictionary<string, string> 对话改变目标序列映射 { get; set; } = new();
		[Export] public Godot.Collections.Dictionary<string, int> 击杀条件进度 { get; set; } = new();

		// ==================== 新增：子资源（新存档使用） ====================
		[Export] public 存档_基础数据 基础数据 { get; set; } = new();
		[Export] public 存档_场景数据 场景数据 { get; set; } = new();
		[Export] public 存档_对话数据 对话数据 { get; set; } = new();
		[Export] public 存档_进度数据 进度数据 { get; set; } = new();
		[Export] public 存档_收藏数据 收藏数据 { get; set; } = new();
// 在 存档数据资源.cs 的字段区域添加
[Export] public Godot.Collections.Array<任务数据> 任务列表 { get; set; } = new();

// ★ 新增：已完成任务的原始ID列表
[Export] public Godot.Collections.Array<int> 已完成任务ID列表 { get; set; } = new();
		// ==================== 构造函数 ====================
		public 存档数据资源()
		{
			// 初始化子资源（若为空）
			if (基础数据 == null) 基础数据 = new 存档_基础数据();
			if (场景数据 == null) 场景数据 = new 存档_场景数据();
			if (对话数据 == null) 对话数据 = new 存档_对话数据();
			if (进度数据 == null) 进度数据 = new 存档_进度数据();
			if (收藏数据 == null) 收藏数据 = new 存档_收藏数据();
			存档时间 = DateTime.Now;
		}

		// ==================== 兼容性属性（保持外部调用不变） ====================

		public DateTime 存档时间
		{
			get => 基础数据?.存档时间 ?? DateTime.Now;
			set { if (基础数据 != null) 基础数据.存档时间 = value; }
		}

		public string 获取格式化时间() => 基础数据?.获取格式化时间() ?? "";

		public bool 是否有存档() => 场景数据?.是否有存档() ?? !string.IsNullOrEmpty(存档场景路径);

		// ==================== 迁移和同步方法 ====================

		/// <summary>
		/// 从旧字段迁移到子资源（加载旧存档时自动调用）
		/// </summary>
		public void 迁移到子资源()
		{
			// 基础数据
			if (基础数据 != null)
			{
				基础数据.玩家名字 = 玩家名字;
				基础数据.金币数量 = 金币数量;
				基础数据.存档时间字符串 = 存档时间字符串;
			}

			// 场景数据
			if (场景数据 != null)
			{
				场景数据.存档场景路径 = 存档场景路径;
				场景数据.存档位置 = 存档位置;
				场景数据.存档点名称 = 存档点名称;
				场景数据.场景存档位置.Clear();
				foreach (var kvp in 场景存档位置)
					场景数据.场景存档位置[kvp.Key] = kvp.Value;
				场景数据.场景存档点.Clear();
				foreach (var kvp in 场景存档点)
					场景数据.场景存档点[kvp.Key] = kvp.Value;
			}

			// 对话数据
			if (对话数据 != null)
			{
				对话数据.已改变对话序列.Clear();
				foreach (var kvp in 已改变对话序列)
					对话数据.已改变对话序列[kvp.Key] = kvp.Value;

				对话数据.传送后对话触发记录.Clear();
				foreach (var kvp in 传送后对话触发记录)
					对话数据.传送后对话触发记录[kvp.Key] = kvp.Value;

				对话数据.对话改变目标序列映射.Clear();
				foreach (var kvp in 对话改变目标序列映射)
					对话数据.对话改变目标序列映射[kvp.Key] = kvp.Value;

				对话数据.已执行条件改变项.Clear();
				foreach (var kvp in 已执行条件改变项)
					对话数据.已执行条件改变项[kvp.Key] = kvp.Value;

				对话数据.击杀条件进度.Clear();
				foreach (var kvp in 击杀条件进度)
					对话数据.击杀条件进度[kvp.Key] = kvp.Value;
			}

			// 进度数据
			if (进度数据 != null)
			{
				进度数据.已触发动画记录.Clear();
				foreach (var kvp in 已触发动画记录)
					进度数据.已触发动画记录[kvp.Key] = kvp.Value;

				进度数据.看法解锁状态.Clear();
				foreach (var kvp in 看法解锁状态)
					进度数据.看法解锁状态[kvp.Key] = kvp.Value;

				进度数据.条件状态记录.Clear();
				foreach (var kvp in 条件状态记录)
					进度数据.条件状态记录[kvp.Key] = kvp.Value;

				进度数据.已触发剧情记录.Clear();
				foreach (var kvp in 已触发剧情记录)
					进度数据.已触发剧情记录[kvp.Key] = kvp.Value;
			}

			// 收藏数据
			if (收藏数据 != null)
			{
				收藏数据.卡牌路径列表.Clear();
				foreach (var item in 卡牌路径列表)
					收藏数据.卡牌路径列表.Add(item);

				收藏数据.已购买商品路径列表.Clear();
				foreach (var item in 已购买商品路径列表)
					收藏数据.已购买商品路径列表.Add(item);

				收藏数据.跟随者预制体路径列表.Clear();
				foreach (var item in 跟随者预制体路径列表)
					收藏数据.跟随者预制体路径列表.Add(item);
			}
		}

		/// <summary>
		/// 从子资源同步到旧字段（保存时调用，保持兼容性）
		/// </summary>
		public void 同步到旧字段()
		{
			if (基础数据 != null)
			{
				玩家名字 = 基础数据.玩家名字;
				金币数量 = 基础数据.金币数量;
				存档时间字符串 = 基础数据.存档时间字符串;
			}

			if (场景数据 != null)
			{
				存档场景路径 = 场景数据.存档场景路径;
				存档位置 = 场景数据.存档位置;
				存档点名称 = 场景数据.存档点名称;
			}
		}

		// ==================== 原有方法（保持兼容，转发到子资源） ====================

		// ---- 跟随者 ----
		public void 保存跟随者列表(Godot.Collections.Array<string> 路径列表)
		{
			收藏数据?.保存跟随者列表(路径列表);
			跟随者预制体路径列表 = 路径列表;
		}

		public Godot.Collections.Array<string> 获取跟随者列表()
		{
			return 收藏数据?.获取跟随者列表() ?? 跟随者预制体路径列表;
		}

		// ---- 剧情 ----
		public void 记录剧情触发(string 剧情资源路径, bool 已触发)
		{
			进度数据?.记录剧情触发(剧情资源路径, 已触发);
			已触发剧情记录[剧情资源路径] = 已触发;
		}

		public bool 获取剧情触发状态(string 剧情资源路径)
		{
			return 进度数据?.获取剧情触发状态(剧情资源路径) ?? (已触发剧情记录.ContainsKey(剧情资源路径) && 已触发剧情记录[剧情资源路径]);
		}

		// ---- 对话改变 ----
		public bool 获取对话改变状态(string 序列路径)
		{
			return 对话数据?.获取对话改变状态(序列路径) ?? (已改变对话序列.ContainsKey(序列路径) && 已改变对话序列[序列路径]);
		}

		public void 设置对话改变状态(string 序列路径, bool 已改变)
		{
			对话数据?.设置对话改变状态(序列路径, 已改变);
			已改变对话序列[序列路径] = 已改变;
		}

		// ---- 动画 ----
		public void 记录动画触发(string 唯一标识, bool 已触发)
		{
			进度数据?.记录动画触发(唯一标识, 已触发);
			已触发动画记录[唯一标识] = 已触发;
		}

		public bool 获取动画触发状态(string 唯一标识)
		{
			return 进度数据?.获取动画触发状态(唯一标识) ?? (已触发动画记录.ContainsKey(唯一标识) && 已触发动画记录[唯一标识]);
		}

		// ---- 传送后对话 ----
		public bool 获取传送后对话触发状态(string 对话标识)
		{
			return 对话数据?.获取传送后对话触发状态(对话标识) ?? (传送后对话触发记录.ContainsKey(对话标识) && 传送后对话触发记录[对话标识]);
		}

		public void 设置传送后对话触发状态(string 对话标识, bool 已触发)
		{
			对话数据?.设置传送后对话触发状态(对话标识, 已触发);
			传送后对话触发记录[对话标识] = 已触发;
		}

		public void 移除传送后对话触发记录(string 对话标识)
		{
			if (传送后对话触发记录.ContainsKey(对话标识))
				传送后对话触发记录.Remove(对话标识);
			// 子资源同步
			对话数据?.传送后对话触发记录.Remove(对话标识);
		}

		public void 清空传送后对话触发记录()
		{
			传送后对话触发记录.Clear();
			对话数据?.清空传送后对话触发记录();
		}

		public void 清空所有对话记录() => 清空传送后对话触发记录();

		// ---- 看法解锁 ----
		public void 记录看法解锁状态(string 角色ID, string 主题, bool 已解锁)
		{
			进度数据?.记录看法解锁状态(角色ID, 主题, 已解锁);
			看法解锁状态[$"{角色ID}|{主题}"] = 已解锁;
		}

		public bool 获取看法解锁状态(string 角色ID, string 主题)
		{
			string 键 = $"{角色ID}|{主题}";
			return 进度数据?.获取看法解锁状态(角色ID, 主题) ?? (看法解锁状态.ContainsKey(键) && 看法解锁状态[键]);
		}

		public void 应用看法解锁状态()
		{
			if (回忆系统管理器.实例 == null)
			{
				GD.PrintErr("存档数据资源: 回忆系统管理器实例为空，无法应用看法解锁状态");
				return;
			}

			// 从进度数据或旧字典读取
			var 数据 = 进度数据?.看法解锁状态 ?? 看法解锁状态;
			foreach (var 键值对 in 数据)
			{
				string 唯一标识 = 键值对.Key;
				bool 已解锁 = 键值对.Value;
				string[] 部分 = 唯一标识.Split('|');
				if (部分.Length == 2)
				{
					string 角色名称 = 部分[0];
					string 主题 = 部分[1];
					var 角色数据 = 回忆系统管理器.实例.获取角色看法(角色名称);
					if (角色数据 != null)
					{
						foreach (看法条目 看法 in 角色数据.看法列表)
						{
							if (看法.主题 == 主题)
							{
								看法.已解锁 = 已解锁;
								break;
							}
						}
					}
				}
			}
		}

		// ---- 条件状态 ----
		public void 记录条件状态(string 条件名称, bool 已满足)
		{
			进度数据?.记录条件状态(条件名称, 已满足);
			条件状态记录[条件名称] = 已满足;
		}

		public bool 获取条件状态(string 条件名称)
		{
			return 进度数据?.获取条件状态(条件名称) ?? (条件状态记录.ContainsKey(条件名称) && 条件状态记录[条件名称]);
		}

		public void 应用条件状态()
		{
			if (条件管理器.实例 == null)
			{
				GD.PrintErr("存档数据资源: 条件管理器实例为空，无法应用条件状态");
				return;
			}

			var 数据 = 进度数据?.条件状态记录 ?? 条件状态记录;
			foreach (var 键值对 in 数据)
			{
				if (键值对.Value)
					条件管理器.实例.设置条件满足(键值对.Key);
			}
		}

		// ---- 对话改变目标序列映射 ----
		public string 获取对话改变目标序列(string 序列路径)
		{
			return 对话数据?.获取对话改变目标序列(序列路径) ?? (对话改变目标序列映射.ContainsKey(序列路径) ? 对话改变目标序列映射[序列路径] : "");
		}

		public void 设置对话改变目标序列(string 序列路径, string 目标路径)
		{
			对话数据?.设置对话改变目标序列(序列路径, 目标路径);
			对话改变目标序列映射[序列路径] = 目标路径;
		}

		// ---- 条件改变项已执行 ----
		public bool 获取条件改变项执行状态(string 条件项路径)
		{
			return 对话数据?.获取条件改变项执行状态(条件项路径) ?? (已执行条件改变项.ContainsKey(条件项路径) && 已执行条件改变项[条件项路径]);
		}

		public void 设置条件改变项执行状态(string 条件项路径, bool 已执行)
		{
			对话数据?.设置条件改变项执行状态(条件项路径, 已执行);
			已执行条件改变项[条件项路径] = 已执行;
		}

		public void 清空已执行条件改变项记录()
		{
			已执行条件改变项.Clear();
			对话数据?.已执行条件改变项.Clear();
		}

		// ---- 击杀进度 ----
		public int 获取击杀条件进度(string 键)
		{
			return 对话数据?.获取击杀条件进度(键) ?? (击杀条件进度.ContainsKey(键) ? 击杀条件进度[键] : -1);
		}

		public void 设置击杀条件进度(string 键, int 值)
		{
			对话数据?.设置击杀条件进度(键, 值);
			击杀条件进度[键] = 值;
		}

		public bool 击杀条件键存在(string 键)
		{
			return 对话数据?.击杀条件键存在(键) ?? 击杀条件进度.ContainsKey(键);
		}

		// ---- 场景位置 ----
		public Vector2 获取场景存档位置(string 场景路径)
		{
			return 场景数据?.获取场景存档位置(场景路径) ?? (场景存档位置.ContainsKey(场景路径) ? 场景存档位置[场景路径] : Vector2.Zero);
		}

		public void 设置场景存档位置(string 场景路径, Vector2 位置, string 存档点名称 = "")
		{
			场景数据?.设置场景存档位置(场景路径, 位置, 存档点名称);
			场景存档位置[场景路径] = 位置;
			if (!string.IsNullOrEmpty(存档点名称))
				场景存档点[场景路径] = 存档点名称;
			存档场景路径 = 场景路径;
			存档位置 = 位置;
			this.存档点名称 = 存档点名称;
		}

		public bool 场景是否有存档(string 场景路径)
		{
			return 场景数据?.场景是否有存档(场景路径) ?? 场景存档位置.ContainsKey(场景路径);
		}
	}
}
