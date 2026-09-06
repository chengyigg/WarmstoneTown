using Godot;
using System.Collections.Generic;

/// <summary>
/// 任务提示弹窗 - 屏幕右上角显示任务接取/完成通知
/// </summary>
public partial class 任务提示UI : CanvasLayer
{
	private static 任务提示UI _实例;

	[ExportGroup("位置与样式")]
	[Export] private Vector2 位置偏移 = new Vector2(-20, 20);
	[Export] private float 停留时间 = 2.5f;
	[Export] private float 淡入时间 = 0.2f;
	[Export] private float 淡出时间 = 0.3f;
	[Export] private int 字体大小 = 20;
	[Export] private int 最大同时显示 = 1;

	private const string 接取图标 = "📜";
	private const string 完成图标 = "✅";

	private Queue<提示信息> _提示队列 = new Queue<提示信息>();
	private bool _正在显示 = false;

	private Panel _当前面板;
	private Label _当前图标;
	private Label _当前文字;

	public override void _Ready()
	{
		if (_实例 != null)
		{
			QueueFree();
			return;
		}
		_实例 = this;
		ProcessMode = ProcessModeEnum.Always;
		Layer = 2000;
		// CanvasLayer 本身不处理鼠标事件，无需设置 MouseFilter
	}

	public static void 显示接取提示(string 任务名称)
	{
		_实例?.加入队列(接取图标, $"接受任务：{任务名称}", Colors.LightGreen);
	}

	public static void 显示完成提示(string 任务名称)
	{
		_实例?.加入队列(完成图标, $"任务完成：{任务名称}", Colors.LightYellow);
	}

	public  void 加入队列(string 图标, string 文本, Color 颜色)
	{
		_提示队列.Enqueue(new 提示信息 { 图标 = 图标, 文本 = 文本, 颜色 = 颜色 });
		if (!_正在显示)
			显示下一个();
	}

	private async void 显示下一个()
	{
		if (_提示队列.Count == 0)
		{
			_正在显示 = false;
			return;
		}

		_正在显示 = true;
		var 信息 = _提示队列.Dequeue();

		_创建提示UI(信息);

		_当前面板.Modulate = new Color(1, 1, 1, 0);
		var 淡入Tween = CreateTween();
		淡入Tween.TweenProperty(_当前面板, "modulate:a", 1, 淡入时间)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);

		await ToSignal(GetTree().CreateTimer(淡入时间), "timeout");

		await ToSignal(GetTree().CreateTimer(停留时间), "timeout");

		var 淡出Tween = CreateTween();
		淡出Tween.TweenProperty(_当前面板, "modulate:a", 0, 淡出时间)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In);

		await ToSignal(GetTree().CreateTimer(淡出时间), "timeout");

		if (_当前面板 != null && IsInstanceValid(_当前面板))
			_当前面板.QueueFree();

		显示下一个();
	}

	private void _创建提示UI(提示信息 信息)
	{
		var 视口 = GetViewport().GetVisibleRect().Size;

		_当前面板 = new Panel();
		_当前面板.MouseFilter = Control.MouseFilterEnum.Ignore; // 让面板不阻挡点击

		var 样式 = new StyleBoxFlat();
		样式.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.85f);
		样式.BorderColor = new Color(0.3f, 0.3f, 0.5f);
		样式.BorderWidthLeft = 2;
		样式.BorderWidthTop = 2;
		样式.BorderWidthRight = 2;
		样式.BorderWidthBottom = 2;
		样式.CornerRadiusTopLeft = 10;
		样式.CornerRadiusTopRight = 10;
		样式.CornerRadiusBottomRight = 10;
		样式.CornerRadiusBottomLeft = 10;
		样式.ShadowSize = 10;
		样式.ShadowColor = new Color(0, 0, 0, 0.3f);
		_当前面板.AddThemeStyleboxOverride("panel", 样式);

		float 面板宽度 = 320;
		float 面板高度 = 60;

		float x = 视口.X - 面板宽度 + 位置偏移.X;
		float y = 位置偏移.Y;
		_当前面板.Position = new Vector2(x, y);
		_当前面板.Size = new Vector2(面板宽度, 面板高度);

		_当前图标 = new Label();
		_当前图标.Text = 信息.图标;
		_当前图标.AddThemeFontSizeOverride("font_size", 30);
		_当前图标.Position = new Vector2(10, 10);
		_当前图标.Size = new Vector2(40, 40);
		_当前面板.AddChild(_当前图标);

		_当前文字 = new Label();
		_当前文字.Text = 信息.文本;
		_当前文字.AddThemeFontSizeOverride("font_size", 字体大小);
		_当前文字.AddThemeColorOverride("font_color", Colors.White);
		_当前文字.AddThemeConstantOverride("outline_size", 1);
		_当前文字.AddThemeColorOverride("font_outline_color", Colors.Black);
		_当前文字.Position = new Vector2(55, 15);
		_当前文字.Size = new Vector2(面板宽度 - 70, 面板高度 - 10);
		_当前文字.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		_当前文字.HorizontalAlignment = HorizontalAlignment.Left;
		_当前文字.VerticalAlignment = VerticalAlignment.Center;
		_当前面板.AddChild(_当前文字);

		AddChild(_当前面板);
		MoveChild(_当前面板, 0);
	}
// 在任务提示UI.cs 中添加
public static void 显示道具获得提示(string 道具名称)
{
	_实例?.加入队列("🎁", $"获得道具：{道具名称}", Colors.LightBlue);
}
	private struct 提示信息
	{
		public string 图标;
		public string 文本;
		public Color 颜色;
	}
}
