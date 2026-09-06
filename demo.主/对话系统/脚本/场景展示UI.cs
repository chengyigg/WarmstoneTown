using Godot;

public partial class 场景展示UI : CanvasLayer
{
	[Signal] public delegate void 场景展示结束EventHandler(对话序列 后续对话序列);
	
	private Control 场景容器;
	private ColorRect 背景遮罩;
	
	private bool _动画完成 = false;
	private bool _已初始化 = false;
	private bool _允许空格关闭 = true;
	private 对话序列 _后续对话序列;
	private Node _当前展示场景;
	
	public override void _Ready()
	{
		场景容器 = GetNodeOrNull<Control>("中心容器/垂直容器/场景容器");
		背景遮罩 = GetNodeOrNull<ColorRect>("背景遮罩");
		
		if (场景容器 == null || 背景遮罩 == null)
		{
			GD.PrintErr("场景展示UI: 无法获取必要的节点引用");
			GD.PrintErr($"场景容器: {场景容器 != null}, 背景遮罩: {背景遮罩 != null}");
			return;
		}
		
		Visible = false;
		_已初始化 = true;
	}
	
	public void 显示场景(string 场景路径, 对话序列 后续对话 = null, bool 允许空格关闭 = true)
	{
		if (!_已初始化)
		{
			GD.PrintErr("场景展示UI: 未正确初始化，无法显示场景");
			return;
		}
		
		_允许空格关闭 = 允许空格关闭;
		_动画完成 = false;
		
		清理当前场景();
		
		var 场景资源 = GD.Load<PackedScene>(场景路径);
		if (场景资源 == null)
		{
			GD.PrintErr($"场景展示UI: 无法加载场景 - {场景路径}");
			return;
		}
		
		_当前展示场景 = 场景资源.Instantiate();
		if (_当前展示场景 == null)
		{
			GD.PrintErr($"场景展示UI: 场景实例化失败 - {场景路径}");
			return;
		}
		
		场景容器.AddChild(_当前展示场景);
		_后续对话序列 = 后续对话;
		
		if (_当前展示场景 is PasswordLock 密码锁)
		{
			密码锁.密码锁关闭 += 当密码锁关闭;
			密码锁.显示密码锁();
		}
		
		显示UI();
	}
	
	private void 显示UI()
	{
		Visible = true;
		var 淡入动画 = CreateTween();
		淡入动画.TweenProperty(背景遮罩, "color:a", 0.8f, 0.3f).From(0f);
		淡入动画.Parallel().TweenProperty(场景容器, "modulate:a", 1f, 0.5f).From(0f);
		淡入动画.Finished += () => _动画完成 = true;
	}

	private void 隐藏场景()
	{
		清理当前场景();
		Visible = false;
		场景容器.Modulate = new Color(1, 1, 1, 1);
		背景遮罩.Color = new Color(0, 0, 0, 0);
		_动画完成 = false;
		EmitSignal(nameof(场景展示结束), _后续对话序列);
		_后续对话序列 = null;
		_允许空格关闭 = true;
	}
	
	private void 清理当前场景()
	{
		if (_当前展示场景 != null)
		{
			if (_当前展示场景 is PasswordLock 密码锁)
				密码锁.密码锁关闭 -= 当密码锁关闭;
			场景容器.RemoveChild(_当前展示场景);
			_当前展示场景.QueueFree();
			_当前展示场景 = null;
		}
	}
	
	private void 当密码锁关闭(bool 密码正确)
	{
		Visible = false;
		场景容器.Modulate = new Color(1, 1, 1, 1);
		背景遮罩.Color = new Color(0, 0, 0, 0);
		_动画完成 = false;
		EmitSignal(nameof(场景展示结束), null);
		_后续对话序列 = null;
	}
	
	public override void _Input(InputEvent @event)
	{
		if (Visible && _动画完成 && @event.IsActionPressed("ui_accept") && _允许空格关闭)
		{
			隐藏场景();
			GetViewport().SetInputAsHandled();
		}
	}
}
