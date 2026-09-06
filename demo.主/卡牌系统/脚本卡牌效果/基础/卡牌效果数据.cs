using Godot;

[GlobalClass]
public partial class 卡牌效果数据 : Resource
{
	[Export] public string 效果名称 = "";
	[Export] public string 效果描述 = "";
	[Export] public Texture2D 效果图标;
	
	// 最重要的：效果类名
	[Export] public string 效果类名 = ""; // 例如 "效果_二连击"
}
