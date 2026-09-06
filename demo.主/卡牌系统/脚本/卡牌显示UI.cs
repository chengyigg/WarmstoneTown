using Godot;

public partial class 卡牌显示UI : Control
{
	[Export] private TextureRect 卡面;
	[Export] private Label 名称标签;
	[Export] private Label 描述标签;
	[Export] private Label 伤害标签;
	[Export] private Label 速度标签;

	public void 初始化(卡牌数据 卡牌)
	{
		if (卡牌 == null) return;

		名称标签.Text = 卡牌.卡牌名称;
		描述标签.Text = 卡牌.卡牌描述;
		伤害标签.Text = 卡牌.基础伤害.ToString(); // 改为基础伤害
	

		if (卡牌.卡面贴图 != null)
			卡面.Texture = 卡牌.卡面贴图;
	}
}
