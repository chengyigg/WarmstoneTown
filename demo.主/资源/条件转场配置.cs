using Godot;
using 你的项目.Scripts.资源;             // ★ 新增（解决 转场动画资源 未找到）
[GlobalClass]
public partial class 条件转场配置 : Resource
{
	[Export] public string 条件名称 { get; set; } = "";           // 条件管理器中的条件名
	[Export] public 转场动画资源 转场动画 { get; set; }           // 满足条件时播放的转场
	[Export] public bool 只播放一次 { get; set; } = false;       // 是否只播放一次
}
