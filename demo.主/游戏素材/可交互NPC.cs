using Godot;
using 你的项目.Scripts.管理器;
using 你的项目.Scripts.资源;

public partial class 可交互NPC : Area2D
{
	[Export] public 对话序列 对话资源 { get; set; }
	[Export] public string 交互提示文本 { get; set; } = "点击对话";

	// ★ 新增：关联的任务
	[Export] public 任务数据 关联任务 { get; set; }

	private Label _提示标签;
	private bool _鼠标悬停 = false;

	// ★ 任务标记相关
	private Label _任务标记;
	private bool _已接取过 = false;

	public override void _Ready()
	{
		InputPickable = true;

		_提示标签 = new Label();
		_提示标签.Text = 交互提示文本;
		_提示标签.Visible = false;
		_提示标签.ZIndex = 100;
		AddChild(_提示标签);

		MouseEntered += () => 显示提示(true);
		MouseExited += () => 显示提示(false);
		InputEvent += (viewport, @event, shapeIdx) =>
		{
			if (@event is InputEventMouseButton mouseEvent &&
				mouseEvent.ButtonIndex == MouseButton.Left &&
				mouseEvent.Pressed)
			{
				触发对话();
			}
		};

		// ★ 初始化任务标记
		_初始化任务标记();
	}

	private void _初始化任务标记()
	{
		if (关联任务 == null) return;

		_任务标记 = GetNodeOrNull<Label>("任务标记");
		if (_任务标记 == null)
		{
			_任务标记 = new Label();
			_任务标记.Name = "任务标记";
			_任务标记.Position = new Vector2(0, -35);
			_任务标记.AddThemeFontSizeOverride("font_size", 28);
			_任务标记.AddThemeConstantOverride("outline_size", 2);
			_任务标记.AddThemeColorOverride("font_outline_color", Colors.Black);
			_任务标记.HorizontalAlignment = HorizontalAlignment.Center;
			AddChild(_任务标记);
		}

		if (任务管理器.实例 != null)
		{
			任务管理器.实例.任务列表更新 += 更新标记;
			任务管理器.实例.任务移除 += _当任务移除;
		}

		更新标记();
	}

	private void _当任务移除(int 原始任务ID)
	{
		if (关联任务 == null) return;
		if (关联任务.任务ID == 原始任务ID)
		{
			if (_任务标记 != null)
				_任务标记.Visible = false;
			_已接取过 = true;
		}
	}

	private void 更新标记()
	{
		if (_任务标记 == null || 关联任务 == null)
		{
			if (_任务标记 != null) _任务标记.Visible = false;
			return;
		}

		var 管理器 = 任务管理器.实例;
		if (管理器 == null)
		{
			_任务标记.Visible = false;
			return;
		}

		var 任务 = 管理器.任务列表.Find(t => t.原始任务ID == 关联任务.任务ID);
		if (任务 == null)
		{
			if (_已接取过)
			{
				_任务标记.Visible = false;
			}
			else
			{
				_任务标记.Text = "!";
				_任务标记.AddThemeColorOverride("font_color", Colors.Yellow);
				_任务标记.Visible = true;
			}
			return;
		}
		else
		{
			_已接取过 = true;
			if (任务.是否完成)
			{
				_任务标记.Text = "?";
				_任务标记.AddThemeColorOverride("font_color", Colors.Green);
				_任务标记.Visible = true;
			}
			else
			{
				_任务标记.Text = "?";
				_任务标记.AddThemeColorOverride("font_color", Colors.Gray);
				_任务标记.Visible = true;
			}
		}
	}

	public override void _ExitTree()
	{
		if (任务管理器.实例 != null)
		{
			任务管理器.实例.任务列表更新 -= 更新标记;
			任务管理器.实例.任务移除 -= _当任务移除;
		}
		base._ExitTree();
	}

	private void 显示提示(bool 显示)
	{
		_鼠标悬停 = 显示;
		_提示标签.Visible = 显示;
		if (显示)
		{
			_提示标签.Position = GetGlobalMousePosition() - GlobalPosition + new Vector2(10, -20);
		}
	}

	private void 触发对话()
	{
		if (对话播放器.实例?.是否禁止NPC交互() ?? false) return;

		if (对话资源 == null)
		{
			GD.PrintErr($"NPC {Name} 未设置对话资源");
			return;
		}

		var 对话播放器实例 = 对话播放器.实例;
		if (对话播放器实例 == null)
		{
			GD.PrintErr("对话播放器实例不存在");
			return;
		}

		对话播放器实例.开始对话(对话资源);

		// ★ 对话后刷新标记
		CallDeferred(nameof(更新标记));
	}
}
