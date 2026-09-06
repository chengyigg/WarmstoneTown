using Godot;

public partial class 字体描边 : Control
{
	public override void _Ready()
	{
		// 1. 加载字体
		string fontPath = "res://fonts/ZCOOLKuaiLe-Regular.ttf";
		FontFile font = GD.Load<FontFile>(fontPath);
		
		if (font == null)
		{
			GD.PrintErr("字体加载失败，请检查路径: " + fontPath);
			return;
		}
		
		GD.Print("✅ 字体加载成功");
		
		// 2. 应用描边效果到所有UI元素
		ApplyOutlineToAll(this, font);
	}
	
	private void ApplyOutlineToAll(Node node, FontFile font)
	{
		// 递归处理所有子节点
		foreach (Node child in node.GetChildren())
		{
			// 处理Label节点
			if (child is Label label)
			{
				SetupLabelWithOutline(label, font);
			}
			// 处理Button节点
			else if (child is Button button)
			{
				SetupButtonWithOutline(button, font);
			}
			
			// 递归处理子节点
			ApplyOutlineToAll(child, font);
		}
	}
	
	private void SetupLabelWithOutline(Label label, FontFile font)
	{
		// 应用字体
		label.AddThemeFontOverride("font", font);
		
		// 设置字体大小（根据节点类型）
		int fontSize = 32; // 默认大小
		
		if (label.Name.ToString().Contains("标题") || label.Name.ToString().Contains("Title"))
		{
			fontSize = 56;
			// 标题用更亮的颜色
			label.AddThemeColorOverride("font_color", new Color("#A3FF6B"));
		}
		else
		{
			fontSize = 34;
			label.AddThemeColorOverride("font_color", new Color("#E8FFE8"));
		}
		
		label.AddThemeFontSizeOverride("font_size", fontSize);
		
		// 添加描边效果（关键步骤！）
		label.AddThemeColorOverride("font_outline_color", new Color("#0F1A12")); // 深绿色描边
		label.AddThemeConstantOverride("outline_size", 2); // 2像素描边
		
		GD.Print($"✅ 设置Label: {label.Text} (大小: {fontSize}px)");
	}
	
	private void SetupButtonWithOutline(Button button, FontFile font)
	{
		// 应用字体
		button.AddThemeFontOverride("font", font);
		button.AddThemeFontSizeOverride("font_size", 28);
		
		// 设置按钮文字颜色
		if (button.Text.ToString().Contains("返回") || button.Text.ToString().Contains("Return"))
		{
			// 返回按钮用亮绿色
			button.AddThemeColorOverride("font_color", new Color("#4AFF9C"));
		}
		else
		{
			button.AddThemeColorOverride("font_color", new Color("#D9F3E2"));
		}
		
		// 添加描边效果
		button.AddThemeColorOverride("font_outline_color", new Color("#0F1A12"));
		button.AddThemeConstantOverride("outline_size", 2);
		
		GD.Print($"✅ 设置Button: {button.Text}");
	}
}
