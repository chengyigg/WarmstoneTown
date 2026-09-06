using Godot;
using System;

public partial class 道具槽 : Panel
{
	[Export] public TextureRect 图标节点;
	[Export] public Label 名称节点;
	[Export] public Label 数量节点;
	
	// 新增：两个可拖入的样式资源
	[Export] private StyleBox 普通样式;
	[Export] private StyleBox 高亮样式;

	private 道具数据 绑定的道具;
	private bool 是否高亮 = false;

	public override void _Ready()
	{
		// 如果没有手动拖入普通样式，尝试获取默认样式（可选）
		if (普通样式 == null)
			普通样式 = GetThemeStylebox("panel");
	}

	public void 绑定数据(道具数据 道具)
	{
		绑定的道具 = 道具;
		图标节点.Texture = 道具.图标;
		名称节点.Text = 道具.名称;
		数量节点.Text = $"x{道具.数量}";
	}

public void 设置高亮(bool 高亮)
{
	GD.Print($"设置高亮: {高亮}, 高亮样式存在: {高亮样式 != null}");
	是否高亮 = 高亮;
	if (高亮 && 高亮样式 != null)
		AddThemeStyleboxOverride("panel", 高亮样式);
	else if (普通样式 != null)
		AddThemeStyleboxOverride("panel", 普通样式);
	else
		RemoveThemeStyleboxOverride("panel");
}

	[Signal]
	public delegate void 道具槽点击EventHandler(道具数据 道具);

	public override void _GuiInput(InputEvent 事件)
	{
		if (事件 is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			EmitSignal(SignalName.道具槽点击, 绑定的道具);
			AcceptEvent();
		}
	}
}
