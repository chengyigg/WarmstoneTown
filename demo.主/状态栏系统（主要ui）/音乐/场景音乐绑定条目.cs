using Godot;

[GlobalClass]
public partial class 场景音乐绑定条目 : Resource
{
	[Export] public string 场景路径;   // 你可以拖入 .tscn 文件，会自动填充路径
	[Export] public AudioStream 音乐;  // 直接拖入音频资源
}
