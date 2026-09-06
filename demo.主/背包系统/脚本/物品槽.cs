using Godot;
using System;

public partial class 物品槽 : Button
{
	[Export] public 物品资源 物品数据 { get; set; }
	public bool 是否为空 => 物品数据 == null;

	[Signal]
	public delegate void 物品移动事件EventHandler(物品槽 来源槽位, 物品槽 目标槽位);
	
	[Signal]
	public delegate void 物品选中事件EventHandler(物品资源 物品);

	private ColorRect 颜色显示;
	private Label 物品名称标签;
	private bool 正在拖拽 = false;

	public override void _Ready()
	{
	 颜色显示 = GetNode<ColorRect>("ColorDisplay");
		物品名称标签 = GetNode<Label>("ItemNameLabel");
		
		颜色显示.MouseFilter = MouseFilterEnum.Ignore;
		物品名称标签.MouseFilter = MouseFilterEnum.Ignore;
		
		CustomMinimumSize = new Vector2(80, 80);
		MouseFilter = MouseFilterEnum.Stop;
		
		Pressed += 处理槽位点击;
		更新显示();
	}

	public void 设置物品(物品资源 物品)
	{
		物品数据 = 物品;
		更新显示();
	}

	private void 更新显示()
	{
		if (物品数据 != null)
		{
			Visible = true;
			颜色显示.Color = 物品数据.物品颜色;
			物品名称标签.Text = 物品数据.物品名称;
			TooltipText = 获取物品提示();
			Disabled = false;
		}
		else
		{
			颜色显示.Color = Colors.Transparent;
			物品名称标签.Text = "";
			TooltipText = "";
			Disabled = true;
		}
	}

	private void 处理槽位点击()
	{
		if (正在拖拽)
			return;
			
		if (!是否为空)
		{
			GD.Print($"物品被点击: {物品数据.物品名称}");
			EmitSignal(SignalName.物品选中事件, 物品数据);
		}
	}

	public override Variant _GetDragData(Vector2 位置)
	{
		if (是否为空)
			return default;

		正在拖拽 = true;
		
		var 预览 = new Label();
		预览.Text = 物品数据.物品名称;
		SetDragPreview(预览);

		return this;
	}
	
	public override void _Notification(int 通知类型)
	{
		if (通知类型 == NotificationDragEnd)
		{
			正在拖拽 = false;
		}
	}

	// 修复：使用正确的变量名 "数据" 而不是 "data"
	public override bool _CanDropData(Vector2 位置, Variant 数据)
	{
		return 数据.Obj is 物品槽;
	}

	public override void _DropData(Vector2 位置, Variant 数据)
	{
		var 拖拽槽位 = 数据.Obj as 物品槽;
		if (拖拽槽位 != null && 拖拽槽位 != this)
		{
			EmitSignal(SignalName.物品移动事件, 拖拽槽位, this);
		}
	}

	private string 获取物品提示()
	{
		if (物品数据 == null) return "";
		return $"名称: {物品数据.物品名称}\n攻击: {物品数据.攻击力}\n防御: {物品数据.防御力}\n生命: {物品数据.生命值}\n速度: {物品数据.速度}";
	}

	public void 与槽位交换(物品槽 其他槽位)
	{
		var 临时物品 = 其他槽位.物品数据;
		其他槽位.设置物品(this.物品数据);
		this.设置物品(临时物品);
	}
}
