using Godot;
using 你的项目.Scripts.资源;

[GlobalClass]
public partial class 剧情序列资源 : Resource
{
	[Export] public 摄像头动画资源 摄像头动画配置 { get; set; }
	
	// 旧字段（单个动画）保留，用于向后兼容
	[Export] public string 替身动画名称 { get; set; } = "";
	[Export] public NodePath 替身节点路径 { get; set; }
	[Export] public bool 启用退化 { get; set; } = false;
	[Export] public bool 是否为一次性动画 { get; set; }
	[Export] public NodePath 一次性动画节点路径 { get; set; }
	[Export] public bool 动画结束后强制空闲 { get; set; } = true;
	[Export] public bool 是否为替身动画 { get; set; } = true;
	
	// 新增：动画列表（如果列表不为空，则优先使用列表，忽略旧字段）
	[Export] public 替身动画条目[] 动画列表 { get; set; } = new 替身动画条目[0];
	
	[Export] public 对话序列 对话序列 { get; set; }
}
