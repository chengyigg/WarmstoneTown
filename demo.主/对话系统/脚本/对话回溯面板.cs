using Godot;
using System.Collections.Generic;

public partial class 对话回溯面板 : Control
{
	[Signal]
	public delegate void 面板关闭EventHandler();

	[Export] private VBoxContainer 内容列表;
	[Export] private Control 背景遮罩;

	private List<历史条目> _历史数据;

	public override void _Ready()
	{
		if (背景遮罩 != null)
			背景遮罩.GuiInput += On背景遮罩输入;
		this.VisibilityChanged += On可见性改变;
	}

	private void On可见性改变()
	{
		if (Visible)
			GrabFocus();
	}

	private void On背景遮罩输入(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
			关闭();
	}

	public override void _Input(InputEvent @event)
	{
		if (Visible && @event.IsActionPressed("ui_cancel"))
		{
			关闭();
			AcceptEvent();
		}
	}

	public void 设置历史数据(List<历史条目> 数据)
	{
		_历史数据 = 数据;
		刷新界面();
	}

	private void 刷新界面()
	{
		foreach (Node child in 内容列表.GetChildren())
			child.QueueFree();

		if (_历史数据 == null) return;

		foreach (var 条目 in _历史数据)
		{
			HBoxContainer 行 = new HBoxContainer();
			行.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			行.AddThemeConstantOverride("separation", 15);

			Label 名字标签 = new Label();
			名字标签.Text = 条目.说话人 + "：";
			名字标签.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
			名字标签.CustomMinimumSize = new Vector2(80, 0);
			名字标签.HorizontalAlignment = HorizontalAlignment.Right;

			Label 内容标签 = new Label();
			内容标签.Text = 条目.文本;
			内容标签.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			内容标签.AutowrapMode = TextServer.AutowrapMode.Word;
			内容标签.HorizontalAlignment = HorizontalAlignment.Left;

			行.AddChild(名字标签);
			行.AddChild(内容标签);
			内容列表.AddChild(行);
		}
	}

	public  void 关闭()
	{
		EmitSignal(SignalName.面板关闭);
		QueueFree();
	}
}

public struct 历史条目
{
	public string 说话人;
	public string 文本;
}
