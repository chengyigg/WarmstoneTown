using Godot;
using 你的项目.Scripts.管理器;

namespace 你的项目.Scripts.UI
{
	public partial class 名字输入界面 : Control  // 是的，类型是Control
	{
		private LineEdit _名字输入框;
		private Button _确认按钮;
		private Label _提示标签;
		private ColorRect _背景;
		
		[Export] public int 最大名字长度 { get; set; } = 10;
		[Export] public int 最小名字长度 { get; set; } = 2;

		public override void _Ready()
		{
			// 根据你的节点结构，这些节点都是当前Control的直接子节点
			_名字输入框 = GetNode<LineEdit>("名字输入框");
			_确认按钮 = GetNode<Button>("确认按钮");
			_提示标签 = GetNode<Label>("提示标签");
			_背景 = GetNode<ColorRect>("背景");
			
			设置UI();
			连接信号();
		}

		private void 设置UI()
		{
			_提示标签.Text = "请输入你的名字";
			_确认按钮.Text = "确认";
			_名字输入框.PlaceholderText = $"请输入{最小名字长度}-{最大名字长度}个字符";
			_名字输入框.MaxLength = 最大名字长度;
			
			// 设置样式
			_背景.Color = new Color(0.1f, 0.1f, 0.2f, 0.9f);
			
			var 输入框样式 = new StyleBoxFlat();
			输入框样式.BgColor = new Color(0.2f, 0.2f, 0.3f);
			输入框样式.BorderColor = new Color(0.4f, 0.4f, 0.5f);
			输入框样式.CornerRadiusTopLeft = 5;
			输入框样式.CornerRadiusTopRight = 5;
			输入框样式.CornerRadiusBottomRight = 5;
			输入框样式.CornerRadiusBottomLeft = 5;
			_名字输入框.AddThemeStyleboxOverride("normal", 输入框样式);
			
			// 初始状态
			_确认按钮.Disabled = true;
			
			// 自动聚焦
			Callable.From(() => _名字输入框.GrabFocus()).CallDeferred();
		}

		private void 连接信号()
		{
			_确认按钮.Pressed += 确认按钮按下;
			_名字输入框.TextSubmitted += 名字输入提交;
			_名字输入框.TextChanged += 名字输入改变;
		}

		private void 名字输入改变(string 新文本)
		{
			// 实时验证名字长度
			bool 有效 = 新文本.Length >= 最小名字长度 && 新文本.Length <= 最大名字长度;
			_确认按钮.Disabled = !有效;
			
			if (!有效 && 新文本.Length > 0)
			{
				_提示标签.Text = $"名字长度必须在{最小名字长度}-{最大名字长度}个字符之间";
				_提示标签.AddThemeColorOverride("font_color", new Color(1, 0.5f, 0.5f));
			}
			else
			{
				_提示标签.Text = "请输入你的名字";
				_提示标签.AddThemeColorOverride("font_color", new Color(1, 1, 1));
			}
		}

		private void 名字输入提交(string 文本)
		{
			// 当玩家按回车时提交
			if (!_确认按钮.Disabled)
			{
				确认按钮按下();
			}
		}

		private void 确认按钮按下()
		{
			string 输入名字 = _名字输入框.Text.Trim();
			
			if (输入名字.Length < 最小名字长度 || 输入名字.Length > 最大名字长度)
			{
				// 显示错误提示
				var 错误动画 = CreateTween();
				错误动画.TweenProperty(_名字输入框, "modulate", new Color(1, 0.5f, 0.5f), 0.1f);
				错误动画.TweenProperty(_名字输入框, "modulate", new Color(1, 1, 1), 0.1f);
				return;
			}
			
			// 设置玩家名字
			if (玩家数据管理器.实例 != null)
			{
				玩家数据管理器.实例.玩家名字 = 输入名字;
				GD.Print($"玩家名字设置为: {输入名字}");
			}
			
			// 发出确认信号
			EmitSignal(nameof(名字确认), 输入名字);
			
			// 隐藏界面
			Hide();
		}

		[Signal] public delegate void 名字确认EventHandler(string 玩家名字);
		
		// 公共方法：显示名字输入界面
		public void 显示输入界面(string 默认名字 = "")
		{
			if (!string.IsNullOrEmpty(默认名字))
			{
				_名字输入框.Text = 默认名字;
			}
			else
			{
				_名字输入框.Text = "";
			}
			
			Show();
			// 延迟获取焦点，确保界面已经完全显示
			Callable.From(() => _名字输入框.GrabFocus()).CallDeferred();
		}
		public override void _UnhandledInput(InputEvent @event)
{
	if (@event.IsActionPressed("ui_cancel"))
	{
		GD.Print("名字输入界面 按 ESC");
		var 主界面 = GetParent<开始界面UI>();
		if (主界面 != null)
		{
			主界面.返回主菜单();
		}
		Hide();
		AcceptEvent(); // 阻止事件继续传递
	}
}
public override void _GuiInput(InputEvent @event)
{
	if (@event is InputEventKey keyEvent && keyEvent.Pressed)
	{
		if (keyEvent.Keycode == Key.Escape)
		{
			// 返回主菜单
			var 主界面 = GetParent<开始界面UI>();
			if (主界面 != null)
			{
				主界面.返回主菜单();
			}
			Hide();
		}
	}
}

	}
}
