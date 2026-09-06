// CameraAnimationResource.cs
using Godot;
using System;

namespace 你的项目.Scripts.资源
{
[GlobalClass]
public partial class 摄像头动画资源 : Resource
{
	[Export] public bool 启用动画 { get; set; } = false;
	
	// 动画序列定义
	[Export] public 动画序列[] 序列 { get; set; } = new 动画序列[0];
	
	// 总动画时长（秒）
	public float 总时长
	{
		get
		{
			float 时长 = 0f;
			foreach (var 动画 in 序列)
			{
				时长 += 动画.移动时长 + 动画.停留时间;
			}
			return 时长;
		}
	}
	
	// 新增：调试信息
	public void 打印调试信息()
	{
		GD.Print($"摄像头动画资源调试:");
		GD.Print($"  启用动画: {启用动画}");
		GD.Print($"  序列数量: {序列.Length}");
		GD.Print($"  总时长: {总时长}秒");
		
		for (int i = 0; i < 序列.Length; i++)
		{
			var 动画 = 序列[i];
			GD.Print($"  序列[{i}]: 偏移={动画.目标位置偏移}, 移动={动画.移动时长}秒, 停留={动画.停留时间}秒");
		}
	}
}
}
