using Godot;
using System.Collections.Generic;

namespace 你的项目.Scripts.资源
{
	[GlobalClass]
	public partial class 存档_对话数据 : Resource
	{
		// ===== 旧字典（保留兼容） =====
		[Export] public Godot.Collections.Dictionary<string, bool> 已改变对话序列 { get; set; }
			= new Godot.Collections.Dictionary<string, bool>();

		[Export] public Godot.Collections.Dictionary<string, bool> 传送后对话触发记录 { get; set; }
			= new Godot.Collections.Dictionary<string, bool>();

		[Export] public Godot.Collections.Dictionary<string, string> 对话改变目标序列映射 { get; set; }
			= new Godot.Collections.Dictionary<string, string>();

		[Export] public Godot.Collections.Dictionary<string, bool> 已执行条件改变项 { get; set; }
			= new Godot.Collections.Dictionary<string, bool>();

		// ★ 旧击杀进度（基于 ResourcePath，保留兼容）
		[Export] public Godot.Collections.Dictionary<string, int> 击杀条件进度 { get; set; }
			= new Godot.Collections.Dictionary<string, int>();

		// ===== ★ 新增：基于怪物ID的新字典 =====
		// 键 = 怪物ID，值 = 击杀数量
		[Export] public Godot.Collections.Dictionary<int, int> 击杀条件进度_按怪物ID { get; set; }
			= new Godot.Collections.Dictionary<int, int>();

		// ===== 方法 =====

		// ===== 对话改变状态 =====
		public bool 获取对话改变状态(string 序列路径)
		{
			return 已改变对话序列.ContainsKey(序列路径) && 已改变对话序列[序列路径];
		}

		public void 设置对话改变状态(string 序列路径, bool 已改变)
		{
			已改变对话序列[序列路径] = 已改变;
		}

		// ===== 传送后对话 =====
		public bool 获取传送后对话触发状态(string 对话标识)
		{
			return 传送后对话触发记录.ContainsKey(对话标识) && 传送后对话触发记录[对话标识];
		}

		public void 设置传送后对话触发状态(string 对话标识, bool 已触发)
		{
			传送后对话触发记录[对话标识] = 已触发;
		}

		public void 清空传送后对话触发记录()
		{
			传送后对话触发记录.Clear();
		}

		// ===== 对话改变目标序列映射 =====
		public string 获取对话改变目标序列(string 序列路径)
		{
			return 对话改变目标序列映射.ContainsKey(序列路径) ? 对话改变目标序列映射[序列路径] : "";
		}

		public void 设置对话改变目标序列(string 序列路径, string 目标路径)
		{
			对话改变目标序列映射[序列路径] = 目标路径;
		}

		// ===== 条件改变项已执行 =====
		public bool 获取条件改变项执行状态(string 条件项路径)
		{
			return 已执行条件改变项.ContainsKey(条件项路径) && 已执行条件改变项[条件项路径];
		}

		public void 设置条件改变项执行状态(string 条件项路径, bool 已执行)
		{
			已执行条件改变项[条件项路径] = 已执行;
		}

		// ===== 兼容性方法：写入击杀进度 =====
		public void 设置击杀条件进度(string 旧键, int 值, int? 怪物ID = null)
		{
			// 写入旧字典（兼容旧版本）
			击杀条件进度[旧键] = 值;

			// 如果提供了怪物ID，写入新字典
			if (怪物ID.HasValue)
				击杀条件进度_按怪物ID[怪物ID.Value] = 值;
		}

		// ===== 兼容性方法：读取击杀进度（优先新字典） =====
		public int 获取击杀条件进度(string 旧键, int? 怪物ID = null)
		{
			// 1. 优先从新字典读取（基于怪物ID）
			if (怪物ID.HasValue && 击杀条件进度_按怪物ID.ContainsKey(怪物ID.Value))
				return 击杀条件进度_按怪物ID[怪物ID.Value];

			// 2. 回退到旧字典（基于 ResourcePath）
			if (击杀条件进度.ContainsKey(旧键))
				return 击杀条件进度[旧键];

			// 3. 都不存在
			return -1;
		}

		// ===== 兼容性方法：检查键是否存在 =====
		public bool 击杀条件键存在(string 旧键, int? 怪物ID = null)
		{
			if (怪物ID.HasValue && 击杀条件进度_按怪物ID.ContainsKey(怪物ID.Value))
				return true;
			return 击杀条件进度.ContainsKey(旧键);
		}

		// ===== 迁移方法：将旧字典数据迁移到新字典 =====
		public void 迁移击杀进度(怪物配置[] 所有怪物配置)
		{
			if (所有怪物配置 == null || 所有怪物配置.Length == 0) return;

			// 构建名字 → ID 映射
			var 名字到ID = new Dictionary<string, int>();
			foreach (var 配置 in 所有怪物配置)
			{
				if (配置 != null && !string.IsNullOrEmpty(配置.名字))
					名字到ID[配置.名字] = 配置.怪物ID;
			}

			// 遍历旧字典，迁移到新字典
			var 待迁移 = new List<KeyValuePair<string, int>>();
			foreach (var 旧键值 in 击杀条件进度)
				待迁移.Add(旧键值);

			foreach (var 旧键值 in 待迁移)
			{
				string 旧键 = 旧键值.Key;
				int 值 = 旧键值.Value;

				// 旧键格式："res://...|怪物名"
				string[] 部分 = 旧键.Split('|');
				if (部分.Length != 2) continue;

				string 怪物名字 = 部分[1];
				if (名字到ID.TryGetValue(怪物名字, out int 怪物ID))
				{
					// 如果新字典还没有这个ID，则迁移
					if (!击杀条件进度_按怪物ID.ContainsKey(怪物ID))
					{
						击杀条件进度_按怪物ID[怪物ID] = 值;
						GD.Print($"[存档迁移] 击杀进度迁移: 怪物ID={怪物ID} ({怪物名字}) -> {值}");
					}
				}
			}
		}
	}
}
