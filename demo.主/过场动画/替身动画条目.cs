using Godot;

[GlobalClass]
public partial class 替身动画条目 : Resource
{
	[Export] public string 动画名称 { get; set; } = "";
	[Export] public float 播放前延迟 { get; set; } = 0f;   // 延迟多少秒后再播放这个动画
	[Export] public bool 等待动画结束 { get; set; } = true; // 是否等待动画播放完再继续
}
