using Godot;
using 你的项目.Scripts.管理器;

/// <summary>
/// 挂在场景中的道具节点上，玩家按交互键拾取
/// </summary>
public partial class 可拾取道具 : Area2D
{
	[ExportGroup("道具配置")]
	[Export] public 道具数据 道具资源; // 必须指定（拾取后获得的道具）
	[Export] public string 交互提示文本 = "按 Z / 空格 拾取";
// 不需要 [Export]，自动生成
private string _唯一标识 = "";
	[ExportGroup("行为设置")]
	[Export] public bool 拾取后消失 = true; // 拾取后是否删除节点
	[Export] public bool 是否只能拾取一次 = true;
// ★ 新增：可见条件
[ExportGroup("可见条件")]
[Export] public bool 启用可见条件 = false;          // 是否启用条件显示
[Export] public string 可见条件名称 = "";            // 条件名（如 "已获得钥匙"）
	// ===== ★ 新增：条件拾取 =====
	[ExportGroup("条件拾取")]
	[Export] public bool 需要拥有道具才能拾取 { get; set; } = false;
	[Export] public int 所需道具ID { get; set; } = 0;
	[Export] public bool 拾取时消耗所需道具 { get; set; } = true; // true=消耗, false=只检查拥有

	// ===== ★ 新增：条件不足提示 =====
	[ExportGroup("提示设置")]
	[Export] public string 条件不足提示文本 { get; set; } = "条件不足，无法拾取";
	[Export] public float 提示显示时长 { get; set; } = 1.5f;

	[ExportGroup("UI引用（可选）")]
	[Export] public Label 提示标签; // 如果场景中已有 Label，拖入；否则自动创建


[ExportGroup("音效")]
[Export] public AudioStream 拾取音效;

	private bool 玩家在范围内 = false;
	private bool 已拾取 = false;

	// ★ 屏幕提示相关
	private CanvasLayer _提示图层;
	private Label _浮动提示标签;
	private Tween _提示动画;

	public override void _Ready()
	{
		// 初始化提示标签
		if (提示标签 == null)
		{
			提示标签 = new Label();
			提示标签.Text = 交互提示文本;
			提示标签.Visible = false;
			AddChild(提示标签);
			提示标签.Position = new Vector2(-20, -40);
		}
		else
		{
			提示标签.Text = 交互提示文本;
			提示标签.Visible = false;
		}
  // ★ 生成唯一标识（使用节点路径）
	_唯一标识 = GetPath() + "|" + (道具资源?.名称 ?? "未知道具");
	GD.Print($"[可拾取道具] 唯一标识: {_唯一标识}");

	应用存档状态();
	刷新可见性();

	// ★ 订阅条件更新信号
	if (条件管理器.实例 != null)
		条件管理器.实例.条件更新 += 当条件更新;
		// ★ 初始化屏幕提示图层
		_提示图层 = new CanvasLayer();
		_提示图层.Layer = 1000;
		GetTree().Root.AddChild(_提示图层);
		
		_浮动提示标签 = new Label();
		_浮动提示标签.Text = 条件不足提示文本;
		_浮动提示标签.AddThemeFontSizeOverride("font_size", 32);
		_浮动提示标签.AddThemeColorOverride("font_color", Colors.White);
		_浮动提示标签.AddThemeConstantOverride("outline_size", 2);
		_浮动提示标签.AddThemeColorOverride("font_outline_color", Colors.Black);
		_浮动提示标签.HorizontalAlignment = HorizontalAlignment.Center;
		_浮动提示标签.VerticalAlignment = VerticalAlignment.Center;
		_浮动提示标签.Modulate = new Color(1, 1, 1, 0);
		_提示图层.AddChild(_浮动提示标签);
		
		var 视口大小 = GetViewport().GetVisibleRect().Size;
		_浮动提示标签.Position = (视口大小 - _浮动提示标签.GetMinimumSize()) / 2;

		// 连接碰撞信号
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
		 应用存档状态();
		 刷新可见性();    // ★ 新增：应用条件显示
	}

private void 应用存档状态()
{
	if (是否已拾取())
	{
		已拾取 = true;
		if (提示标签 != null)
			提示标签.Visible = false;
		if (拾取后消失)
			QueueFree();
		else
		{
			SetDeferred("monitoring", false);
			Visible = false;
		}
		GD.Print($"[可拾取道具] 该道具已拾取过，隐藏");
	}
}
private void 刷新可见性()
{
	if (!启用可见条件 || string.IsNullOrEmpty(可见条件名称))
	{
		// 如果未启用条件，确保道具可见（但若已拾取则仍隐藏）
		if (!已拾取)
		{
			Visible = true;
			SetDeferred("monitoring", true);
		}
		return;
	}

	// 检查条件是否满足
	bool 条件满足 = 条件管理器.实例?.检查条件(可见条件名称) ?? false;

	if (条件满足 && !已拾取)
	{
		// 条件满足且未拾取 → 显示
		Visible = true;
		SetDeferred("monitoring", true);
	}
	else
	{
		// 条件不满足或已拾取 → 隐藏
		Visible = false;
		SetDeferred("monitoring", false);
	}
}
	public override void _ExitTree()
{
	if (条件管理器.实例 != null)
		条件管理器.实例.条件更新 -= 当条件更新;

	if (_提示图层 != null && IsInstanceValid(_提示图层))
		_提示图层.QueueFree();
}

	// ★ 显示屏幕中央渐淡提示
	private void 显示浮动提示(string 文本, Color? 颜色 = null)
	{
		if (_浮动提示标签 == null || !IsInstanceValid(_浮动提示标签)) return;
		if (_提示动画 != null && _提示动画.IsRunning())
			_提示动画.Kill();

		_浮动提示标签.Modulate = new Color(1, 1, 1, 1);
		if (颜色.HasValue)
			_浮动提示标签.Modulate = 颜色.Value;
		_浮动提示标签.Text = 文本;

		var 视口大小 = GetViewport().GetVisibleRect().Size;
		_浮动提示标签.Position = (视口大小 - _浮动提示标签.GetMinimumSize()) / 2;

		_提示动画 = CreateTween();
		_提示动画.TweenProperty(_浮动提示标签, "modulate:a", 0f, 提示显示时长)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (已拾取) return;
		if (body.IsInGroup("玩家") || body.Name.ToString().Contains("玩家"))
		{
			玩家在范围内 = true;
			提示标签.Visible = true;
		}
	}

	private void OnBodyExited(Node2D body)
	{
		if (body.IsInGroup("玩家") || body.Name.ToString().Contains("玩家"))
		{
			玩家在范围内 = false;
			提示标签.Visible = false;
		}
	}

	public override void _Process(double delta)
	{
		if (已拾取) return;
		if (玩家在范围内 && Input.IsActionJustPressed("交互"))
		{
			拾取();
		}
	}

private void 拾取()
{
	if (已拾取) return;
	if (道具资源 == null)
	{
		GD.PrintErr("[可拾取道具] 未设置道具资源！");
		return;
	}

	// ★ 检查是否已经拾取过（从存档读取）
	if (是否已拾取())
	{
		GD.Print($"[可拾取道具] 该道具已拾取过，忽略");
		已拾取 = true;
		提示标签.Visible = false;
		if (拾取后消失) QueueFree();
		else { SetDeferred("monitoring", false); Visible = false; }
		return;
	}

	// ★ 获取玩家道具管理器（一次性）
	var 管理器 = 玩家道具管理器.实例;
	if (管理器 == null)
	{
		GD.PrintErr("[可拾取道具] 玩家道具管理器不存在！");
		return;
	}
  // ★ 播放拾取音效
	if (拾取音效 != null && 全局背景音乐管理器.实例 != null)
	{
		全局背景音乐管理器.实例.播放音效(拾取音效);
	}
	// ★ 条件拾取检查
	if (需要拥有道具才能拾取)
	{
		// 检查是否拥有所需道具
		bool 拥有所需道具 = false;
		道具数据 所需道具对象 = null;
		foreach (var 道具 in 管理器.玩家道具列表)
		{
			if (道具.道具ID == 所需道具ID)
			{
				拥有所需道具 = true;
				所需道具对象 = 道具;
				break;
			}
		}

		if (!拥有所需道具)
		{
			GD.Print($"[可拾取道具] 条件不足：需要道具ID {所需道具ID}");
			显示浮动提示(条件不足提示文本);
			return;
		}

		// ★ 如果设置了消耗，则消耗所需道具
		if (拾取时消耗所需道具 && 所需道具对象 != null)
		{
			所需道具对象.数量--;
			if (所需道具对象.数量 <= 0)
				管理器.玩家道具列表.Remove(所需道具对象);
			管理器.EmitSignal(玩家道具管理器.SignalName.道具列表已更新);
			GD.Print($"[可拾取道具] 消耗了所需道具ID {所需道具ID}，剩余数量 {所需道具对象?.数量 ?? 0}");
		}
	}

	// ★ 克隆并添加道具（使用同一个管理器）
	var 克隆 = 道具资源.克隆();
	管理器.添加道具(克隆);
	GD.Print($"[可拾取道具] 拾取了 {克隆.名称} x{克隆.数量}");

	// ★ 记录已拾取到存档
	记录已拾取();

	已拾取 = true;
	提示标签.Visible = false;

	if (拾取后消失)
	{
		QueueFree();
	}
	else
	{
		SetDeferred("monitoring", false);
		Visible = false;
	}
}
private bool 是否已拾取()
{
	if (string.IsNullOrEmpty(_唯一标识)) return false;

	var 状态 = 存档管理器.实例?.获取当前状态();
	if (状态 == null) return false;

	// ★ 使用唯一标识而不是资源路径
	return 状态.收藏数据.已拾取道具路径列表.Contains(_唯一标识);
}

private void 记录已拾取()
{
	if (string.IsNullOrEmpty(_唯一标识)) return;

	var 状态 = 存档管理器.实例?.获取当前状态();
	if (状态 != null && !状态.收藏数据.已拾取道具路径列表.Contains(_唯一标识))
	{
		状态.收藏数据.已拾取道具路径列表.Add(_唯一标识);
		GD.Print($"[可拾取道具] 记录已拾取: {_唯一标识}");
	}
}
private void 当条件更新(string 条件名称, bool 新状态)
{
	if (启用可见条件 && 条件名称 == 可见条件名称)
	{
		GD.Print($"[可拾取道具] 条件 '{条件名称}' 变为 {新状态}，刷新可见性");
		刷新可见性();
	}
}

}
