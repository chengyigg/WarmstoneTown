using Godot;

[GlobalClass]
public partial class 怪物配置 : Resource
{
	// ★ 新增：固定唯一ID（创建后不要修改）
	[Export] public int 怪物ID { get; set; } = 0;
	
	[Export] public string 名字 = "怪物";
	[Export] public int 最大生命值 = 30;
	[Export] public int 初始护盾 = 0;
	[Export] public Texture2D 怪物图片;
	[Export] public 生成卡组资源 卡组;
	[Export] public int 速度值 = 3; // 预留
}
