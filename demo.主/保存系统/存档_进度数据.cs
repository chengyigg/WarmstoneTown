using Godot;
using System.Collections.Generic;

namespace 你的项目.Scripts.资源
{
	[GlobalClass]
	public partial class 存档_进度数据 : Resource
	{
		[Export] public Godot.Collections.Dictionary<string, bool> 已触发动画记录 { get; set; }
			= new Godot.Collections.Dictionary<string, bool>();

		[Export] public Godot.Collections.Dictionary<string, bool> 看法解锁状态 { get; set; }
			= new Godot.Collections.Dictionary<string, bool>();

		[Export] public Godot.Collections.Dictionary<string, bool> 条件状态记录 { get; set; }
			= new Godot.Collections.Dictionary<string, bool>();

		[Export] public Godot.Collections.Dictionary<string, bool> 已触发剧情记录 { get; set; }
			= new Godot.Collections.Dictionary<string, bool>();
// 在 存档_进度数据.cs 中添加
[Export] public Godot.Collections.Array<string> 已播放转场列表 { get; set; } 
	= new Godot.Collections.Array<string>();
	
	[Export] public Godot.Collections.Array<string> 已隐藏触发器名称列表 { get; set; } 
	= new Godot.Collections.Array<string>();

public void 记录触发器隐藏(string 触发器名称)
{
	if (!已隐藏触发器名称列表.Contains(触发器名称))
		已隐藏触发器名称列表.Add(触发器名称);
}

public bool 是否触发器已隐藏(string 触发器名称)
{
	return 已隐藏触发器名称列表.Contains(触发器名称);
}
		// ===== 方法 =====
public bool 是否已播放转场(string 转场ID)
{
	return 已播放转场列表.Contains(转场ID);
}

public void 标记转场已播放(string 转场ID)
{
	if (!已播放转场列表.Contains(转场ID))
		已播放转场列表.Add(转场ID);
}
		public void 记录动画触发(string 唯一标识, bool 已触发)
		{
			已触发动画记录[唯一标识] = 已触发;
		}

		public bool 获取动画触发状态(string 唯一标识)
		{
			return 已触发动画记录.ContainsKey(唯一标识) && 已触发动画记录[唯一标识];
		}

		public void 记录看法解锁状态(string 角色ID, string 主题, bool 已解锁)
		{
			看法解锁状态[$"{角色ID}|{主题}"] = 已解锁;
		}

		public bool 获取看法解锁状态(string 角色ID, string 主题)
		{
			string 键 = $"{角色ID}|{主题}";
			return 看法解锁状态.ContainsKey(键) && 看法解锁状态[键];
		}

		public void 记录条件状态(string 条件名称, bool 已满足)
		{
			条件状态记录[条件名称] = 已满足;
		}

		public bool 获取条件状态(string 条件名称)
		{
			return 条件状态记录.ContainsKey(条件名称) && 条件状态记录[条件名称];
		}

		public void 记录剧情触发(string 剧情资源路径, bool 已触发)
		{
			已触发剧情记录[剧情资源路径] = 已触发;
		}

		public bool 获取剧情触发状态(string 剧情资源路径)
		{
			return 已触发剧情记录.ContainsKey(剧情资源路径) && 已触发剧情记录[剧情资源路径];
		}
	}
}
