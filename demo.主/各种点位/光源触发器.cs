using Godot;

public partial class 光源触发器 : Area2D
{
	[Export]
	public 点光源 目标光源 { get; set; }
	
	[Export]
	public string 提示文字 { get; set; } = "点击 F 打开灯";
	
	[Export]
	public bool 是否启动后开始对话 { get; set; } = false;
	
	[Export]
	public 对话序列 开灯后对话序列 { get; set; }
	
	[Export]
	public 对话序列 关灯后对话序列 { get; set; }
	
	[Export]
	public float 对话延迟时间 { get; set; } = 0.5f;

	// ✅ 新增：在检查器中拖入 CollisionShape2D 节点
	[Export]
	public CollisionShape2D 碰撞形状 { get; set; }

	private Label _提示标签;
	private bool _玩家在范围内 = false;
	private bool _按键已按下 = false;
	private bool _已触发开灯对话 = false;
	private Timer _对话延迟计时器;
	private bool _上次光源状态 = false;
	
	public override void _Ready()
	{
		// 处理碰撞形状：如果未在检查器中指定，则尝试按名称查找
		if (碰撞形状 == null)
		{
			碰撞形状 = GetNode<CollisionShape2D>("CollisionShape2D");
			if (碰撞形状 == null)
			{
				GD.PrintErr("光源触发器: 未找到 CollisionShape2D，请手动拖入检查器或添加子节点 CollisionShape2D");
			}
		}
		
		// 获取场景中的提示标签节点
		_提示标签 = GetNode<Label>("提示标签");
		_提示标签.Text = 提示文字;
		_提示标签.Visible = false;
		
		// 连接信号
		BodyEntered += 当物体进入;
		BodyExited += 当物体离开;
		
		// 设置碰撞层
		CollisionLayer = 2;
		CollisionMask = 1;
		
		更新提示文字();
		
		_对话延迟计时器 = new Timer();
		AddChild(_对话延迟计时器);
		_对话延迟计时器.OneShot = true;
		_对话延迟计时器.Timeout += 延迟后触发对话;
		
		if (目标光源 != null)
		{
			_上次光源状态 = 目标光源.是否开启();
		}
		
		if (对话播放器.实例 == null)
		{
			GD.PrintErr("光源触发器: 对话播放器单例为空，请确保已添加到自动加载");
		}
	}
	
public override void _Process(double delta)
{
	_提示标签.Visible = _玩家在范围内;
	
	if (_玩家在范围内 && Input.IsActionJustPressed("交互"))
	{
		// ✅ 新增：对话进行中时禁止触发光源
		if (对话播放器.实例 != null && 对话播放器.实例.对话进行中)
			return;
		
		if (!_按键已按下)
		{
			切换光源状态();
			_按键已按下 = true;
		}
	}
	else
	{
		_按键已按下 = false;
	}
}
	
	private void 当物体进入(Node2D 物体)
	{
		if (物体.IsInGroup("玩家"))
		{
			_玩家在范围内 = true;
			GD.Print("玩家进入光源范围");
			更新提示文字();
		}
	}
	
	private void 当物体离开(Node2D 物体)
	{
		if (物体.IsInGroup("玩家"))
		{
			_玩家在范围内 = false;
			GD.Print("玩家离开光源范围");
		}
	}
	
	private void 切换光源状态()
	{
		if (目标光源 != null)
		{
			bool 当前光源状态 = 目标光源.是否开启();
			目标光源.切换光源();
			bool 新光源状态 = 目标光源.是否开启();
			
			GD.Print($"光源状态切换: {当前光源状态} -> {新光源状态}");
			
			if (当前光源状态 != 新光源状态)
			{
				if (新光源状态)
				{
					if (是否启动后开始对话 && 开灯后对话序列 != null && !_已触发开灯对话)
					{
						_对话延迟计时器.Start(对话延迟时间);
						_已触发开灯对话 = true;
						GD.Print($"启动开灯对话延迟计时器: {对话延迟时间}秒");
					}
				}
				else
				{
					if (关灯后对话序列 != null)
					{
						_对话延迟计时器.Start(对话延迟时间);
						GD.Print($"启动关灯对话延迟计时器: {对话延迟时间}秒");
					}
				}
			}
			
			更新提示文字();
			_上次光源状态 = 新光源状态;
		}
		else
		{
			GD.PrintErr("目标光源未设置！");
		}
	}
	
	private void 延迟后触发对话()
	{
		GD.Print("延迟结束，准备触发对话");
		
		if (目标光源 == null) return;
		
		bool 当前光源状态 = 目标光源.是否开启();
		
		if (当前光源状态)
		{
			if (开灯后对话序列 != null)
			{
				GD.Print("触发开灯后对话");
				触发对话(开灯后对话序列);
			}
		}
		else
		{
			if (关灯后对话序列 != null)
			{
				GD.Print("触发关灯后对话");
				触发对话(关灯后对话序列);
			}
		}
	}
	
	private void 触发对话(对话序列 对话序列)
	{
		if (对话播放器.实例 == null)
		{
			GD.PrintErr("对话播放器实例为空，无法触发对话");
			return;
		}
		
		if (对话序列 == null)
		{
			GD.PrintErr("对话序列为空");
			return;
		}
		
		Callable.From(() => 安全开始对话(对话序列)).CallDeferred();
	}
	
	private void 安全开始对话(对话序列 对话序列)
	{
		try
		{
			对话播放器.实例.开始对话(对话序列);
			GD.Print($"成功开始对话: {对话序列.ResourcePath}");
		}
		catch (System.Exception e)
		{
			GD.PrintErr($"触发对话时发生错误: {e.Message}");
		}
	}
	
	private void 更新提示文字()
	{
		if (目标光源 != null)
		{
			string 当前状态 = 目标光源.是否开启() ? "关闭" : "打开";
			_提示标签.Text = $"按 F {当前状态}灯";
		}
	}
	
	public void 设置范围大小(Vector2 大小)
	{
		if (碰撞形状 != null && 碰撞形状.Shape is RectangleShape2D 矩形形状)
		{
			矩形形状.Size = 大小;
		}
		else
		{
			GD.PrintErr("设置范围大小失败: 碰撞形状未设置或不是矩形形状");
		}
	}
	
	public void 重置触发器()
	{
		_已触发开灯对话 = false;
		if (目标光源 != null)
		{
			_上次光源状态 = 目标光源.是否开启();
		}
		GD.Print("光源触发器已重置");
	}
}
