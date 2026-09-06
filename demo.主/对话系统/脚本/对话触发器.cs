using Godot;
using 你的项目.Scripts.管理器;
using 你的项目.Scripts.资源;

// 在文件顶部（类外）添加
public enum 条件逻辑模式
{
	全部满足,   // AND
	任意满足    // OR
}

public partial class 对话触发器 : Area2D
{
	[Export] public 对话序列 对话序列资源 { get; set; }
	
	[ExportGroup("首次对话设置")]
	[Export] public bool 启用首次对话 = false;
	[Export] public 对话序列 首次对话序列 { get; set; }
	[Export] public 对话序列 后续对话序列 { get; set; }
	[Export] public string 首次对话条件名 { get; set; } = "";
	private string _首次对话条件名缓存;

	[Export] public string 交互提示文本 { get; set; } = "按 F 对话";
	
	[ExportGroup("条件设置")]
	// ---- 新条件系统 ----
	[Export] public Godot.Collections.Array<string> 显示条件列表 { get; set; } 
		= new Godot.Collections.Array<string>();
	[Export] public 条件逻辑模式 显示条件逻辑 { get; set; } = 条件逻辑模式.全部满足;

	[Export] public Godot.Collections.Array<string> 隐藏条件列表 { get; set; } 
		= new Godot.Collections.Array<string>();
	[Export] public 条件逻辑模式 隐藏条件逻辑 { get; set; } = 条件逻辑模式.任意满足;

	[Export] public StaticBody2D 阻挡墙体 { get; set; }
	[Export] public Godot.Collections.Array<任务数据> 关联任务列表 { get; set; } 
		= new Godot.Collections.Array<任务数据>();

	private Label 提示标签;
	private bool 玩家在范围内 = false;
	private bool 可以触发对话 = true;
	private bool 上次条件满足 = true;

	private Label _任务标记;

	// 保存原始碰撞层/掩码，以便恢复
	private uint _原始碰撞层;
	private uint _原始碰撞掩码;

	public override async void _Ready()
	{
		// 保存原始碰撞值
		_原始碰撞层 = CollisionLayer;
		_原始碰撞掩码 = CollisionMask;

		提示标签 = GetNode<Label>("提示标签");
		提示标签.Text = 交互提示文本;
		提示标签.Visible = false;

		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		if (对话播放器.实例 != null)
		{
			对话播放器.实例.对话开始 += On对话开始;
			对话播放器.实例.对话结束 += On对话结束;
		}

		设置墙体启用(true);
		设置碰撞启用(true);
		SetDeferred("monitoring", true);
		Visible = true;
		上次条件满足 = true;

		_初始化任务标记();

		// 首次对话初始化
		if (启用首次对话)
		{
			if (string.IsNullOrEmpty(首次对话条件名))
				_首次对话条件名缓存 = $"首次对话_{GetPath().GetHashCode()}";
			else
				_首次对话条件名缓存 = 首次对话条件名;

			bool 已触发 = 条件管理器.实例?.检查条件(_首次对话条件名缓存) ?? false;
			if (已触发)
			{
				if (后续对话序列 != null)
					对话序列资源 = 后续对话序列;
				GD.Print($"[对话触发器] 首次对话已触发，使用后续对话序列");
			}
			else
			{
				if (首次对话序列 != null)
					对话序列资源 = 首次对话序列;
				GD.Print($"[对话触发器] 首次对话未触发，使用首次对话序列");
			}
		}

		await ToSignal(GetTree(), "process_frame");
		强制刷新条件();
	}

	private void _初始化任务标记()
	{
		if (关联任务列表 == null || 关联任务列表.Count == 0) return;

		_任务标记 = GetNodeOrNull<Label>("任务标记");
		if (_任务标记 == null)
		{
			_任务标记 = new Label();
			_任务标记.Name = "任务标记";
			_任务标记.Position = new Vector2(-4, -60);
			_任务标记.AddThemeFontSizeOverride("font_size", 20);
			_任务标记.AddThemeConstantOverride("outline_size", 2);
			_任务标记.AddThemeColorOverride("font_outline_color", Colors.Black);
			_任务标记.HorizontalAlignment = HorizontalAlignment.Center;
			AddChild(_任务标记);
		}

		if (任务管理器.实例 != null)
		{
			任务管理器.实例.任务列表更新 += 强制刷新标记;
			任务管理器.实例.任务移除 += _当任务移除;
		}
		强制刷新标记();
	}

	private void _当任务移除(int 原始任务ID)
	{
		// 检查移除的任务是否在关联列表中
		foreach (var 关联任务 in 关联任务列表)
		{
			if (关联任务 != null && 关联任务.任务ID == 原始任务ID)
			{
				强制刷新标记();
				return;
			}
		}
	}

	public override void _ExitTree()
	{
		if (对话播放器.实例 != null)
		{
			对话播放器.实例.对话开始 -= On对话开始;
			对话播放器.实例.对话结束 -= On对话结束;
		}
		if (任务管理器.实例 != null)
		{
			任务管理器.实例.任务列表更新 -= 强制刷新标记;
			任务管理器.实例.任务移除 -= _当任务移除;
		}
		base._ExitTree();
	}

	private void On对话开始() => 可以触发对话 = false;
	private void On对话结束()
	{
		可以触发对话 = true;
		if (玩家在范围内 && 条件允许交互())
			提示标签.Visible = true;
		强制刷新标记();
	}

	public override void _Process(double delta)
	{
		应用条件状态();
		if (Input.IsKeyPressed(Key.F2))
			强制重置();

		if (条件允许交互() && 玩家在范围内 && Input.IsActionJustPressed("交互") && 可以触发对话 && 对话序列资源 != null)
			触发对话();
	}

	public void 强制刷新标记()
	{
		if (!IsInstanceValid(this) || _任务标记 == null || !IsInstanceValid(_任务标记))
			return;

		if (关联任务列表 == null || 关联任务列表.Count == 0)
		{
			if (_任务标记 != null) _任务标记.Visible = false;
			return;
		}

		var 管理器 = 任务管理器.实例;
		if (管理器 == null)
		{
			if (_任务标记 != null) _任务标记.Visible = false;
			return;
		}

		// 检查所有关联任务的状态
		bool 有未接取 = false;
		bool 有进行中 = false;
		bool 有可交付 = false;
		bool 有已完成 = false;

		foreach (var 关联任务 in 关联任务列表)
		{
			if (关联任务 == null) continue;

			// 检查是否已完成
			if (管理器.任务是否已完成(关联任务.任务ID))
			{
				有已完成 = true;
				continue;
			}

			// 查找进行中的任务
			var 任务 = 管理器.任务列表.Find(t => t.原始任务ID == 关联任务.任务ID);
			if (任务 == null)
			{
				// 未接取
				有未接取 = true;
			}
			else if (任务.所有需求已完成())
			{
				// 进度达标，可交付
				有可交付 = true;
			}
			else
			{
				// 进行中，进度未达标
				有进行中 = true;
			}
		}

		// 综合判断显示什么标记
		if (有可交付)
		{
			// 优先显示绿色 ?（有任务可交付）
			_任务标记.Text = "?";
			_任务标记.AddThemeColorOverride("font_color", Colors.Green);
			_任务标记.Visible = true;
		}
		else if (有进行中)
		{
			// 有进行中的任务，显示灰色 ?
			_任务标记.Text = "?";
			_任务标记.AddThemeColorOverride("font_color", Colors.Gray);
			_任务标记.Visible = true;
		}
		else if (有未接取)
		{
			// 有未接取的任务，显示黄色 !
			_任务标记.Text = "!";
			_任务标记.AddThemeColorOverride("font_color", Colors.Yellow);
			_任务标记.Visible = true;
		}
		else if (有已完成 && !有未接取 && !有进行中 && !有可交付)
		{
			// 所有任务都已完成，隐藏标记
			_任务标记.Visible = false;
		}
		else
		{
			// 默认隐藏
			_任务标记.Visible = false;
		}
	}

	// ★★★ 核心条件判断（只使用新系统）★★★
	private bool 条件允许交互()
	{
		bool 正向满足 = true;   // 默认满足（如果未设置显示条件）
		bool 反向满足 = false;  // 默认不满足（如果未设置隐藏条件）

		// 检查显示条件
		if (显示条件列表 != null && 显示条件列表.Count > 0)
		{
			var 管理器 = 条件管理器.实例;
			if (管理器 == null) return false;

			bool 全部满足 = true;
			bool 任意满足 = false;
			foreach (string 条件名 in 显示条件列表)
			{
				bool 状态 = 管理器.检查条件(条件名);
				全部满足 &= 状态;
				任意满足 |= 状态;
			}
			正向满足 = (显示条件逻辑 == 条件逻辑模式.全部满足) ? 全部满足 : 任意满足;
		}

		// 检查隐藏条件
		if (隐藏条件列表 != null && 隐藏条件列表.Count > 0)
		{
			var 管理器 = 条件管理器.实例;
			if (管理器 == null) return false;

			bool 全部满足 = true;
			bool 任意满足 = false;
			foreach (string 条件名 in 隐藏条件列表)
			{
				bool 状态 = 管理器.检查条件(条件名);
				全部满足 &= 状态;
				任意满足 |= 状态;
			}
			反向满足 = (隐藏条件逻辑 == 条件逻辑模式.全部满足) ? 全部满足 : 任意满足;
		}

		// 最终判定：显示条件满足 且 隐藏条件不满足
		return 正向满足 && !反向满足;
	}

	// 应用状态：使用 条件允许交互 控制显示和碰撞
	private void 应用条件状态()
	{
		bool 当前满足 = 条件允许交互();
		if (当前满足 == 上次条件满足) 
			return;
		上次条件满足 = 当前满足;

		if (当前满足)
		{
			Visible = true;
			设置碰撞启用(true);
			if (玩家在范围内)
				提示标签.Visible = true;
		}
		else
		{
			Visible = false;
			设置碰撞启用(false);
			提示标签.Visible = false;
			玩家在范围内 = false;
		}
	}
	
	public void 强制刷新条件()
	{
		bool 当前满足 = 条件允许交互();
		if (当前满足 == 上次条件满足) return;
		上次条件满足 = 当前满足;

		if (当前满足)
		{
			Visible = true;
			设置碰撞启用(true);
			提示标签.Visible = 玩家在范围内;
		}
		else
		{
			Visible = false;
			设置碰撞启用(false);
			提示标签.Visible = false;
			玩家在范围内 = false;
		}
	}
	
	// 核心方法：彻底启用/禁用碰撞（包括碰撞层、掩码、形状、监测）
	public void 设置碰撞启用(bool 启用)
	{
		// 1. 禁用/启用 Area2D 自身的碰撞层和掩码（这是物理碰撞的核心）
		if (启用)
		{
			CollisionLayer = _原始碰撞层;
			CollisionMask = _原始碰撞掩码;
		}
		else
		{
			CollisionLayer = 0;
			CollisionMask = 0;
		}

		// 2. 禁用/启用 Area2D 的监测（影响信号触发）
		SetDeferred("monitoring", 启用);
		SetDeferred("monitorable", 启用);

		// 3. 禁用/启用 Area2D 自身的碰撞形状（CollisionShape2D/CollisionPolygon2D）
		foreach (Node child in GetChildren())
		{
			if (child is CollisionShape2D shape)
				shape.Disabled = !启用;
			else if (child is CollisionPolygon2D poly)
				poly.Disabled = !启用;
		}
		
		// 4. 如果有阻挡墙体，同样处理它的碰撞形状和碰撞层
		if (阻挡墙体 != null)
		{
			foreach (Node child in 阻挡墙体.GetChildren())
			{
				if (child is CollisionShape2D shape)
					shape.Disabled = !启用;
				else if (child is CollisionPolygon2D poly)
					poly.Disabled = !启用;
			}
		}
	}

	// 原有方法，保留用于强制重置（但实际已由设置碰撞启用替代，此处简化）
	private void 设置墙体启用(bool 启用)
	{
		if (阻挡墙体 == null) return;
		foreach (Node child in 阻挡墙体.GetChildren())
		{
			if (child is CollisionShape2D shape)
				shape.Disabled = !启用;
			else if (child is CollisionPolygon2D poly)
				poly.Disabled = !启用;
		}
	}
	
	private void 检测并设置玩家在区域内()
	{
		if (!条件允许交互()) return;
		var 重叠区域 = GetOverlappingBodies();
		foreach (var body in 重叠区域)
		{
			if (body.IsInGroup("玩家") || body.Name.ToString().Contains("玩家"))
			{
				玩家在范围内 = true;
				提示标签.Visible = true;
				break;
			}
		}
	}
	
	private void OnBodyEntered(Node2D body)
	{
		if (!条件允许交互()) return;
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

	private void 触发对话()
	{
		if (!条件允许交互()) 
		{
			GD.Print($"[对话触发器] 触发对话但条件不满足: {Name}");
			return;
		}
		GD.Print($"[对话触发器] 开始触发对话: {Name}, 序列: {对话序列资源?.ResourcePath}, 场景: {GetTree().CurrentScene.Name}");
		if (对话播放器.实例 == null)
		{
			GD.PrintErr("对话播放器实例为空，无法开始对话！");
			return;
		}

		// 保存当前要播放的对话（在切换之前）
		对话序列 要播放的对话 = 对话序列资源;

		// 首次对话处理：标记条件，并切换为后续对话供下次使用
		if (启用首次对话 && 对话序列资源 == 首次对话序列 && 首次对话序列 != null)
		{
			if (条件管理器.实例 != null)
			{
				条件管理器.实例.设置条件满足(_首次对话条件名缓存);
				GD.Print($"[对话触发器] 首次对话已触发，标记条件 {_首次对话条件名缓存}");
			}
			if (后续对话序列 != null)
			{
				对话序列资源 = 后续对话序列;
				GD.Print($"[对话触发器] 已切换到后续对话序列");
			}
		}

		提示标签.Visible = false;
		// 播放保存的对话，而不是被切换后的 对话序列资源
		对话播放器.实例.开始对话(要播放的对话);
	}
	
	public void 强制重置()
	{
		可以触发对话 = true;
		玩家在范围内 = false;
		提示标签.Visible = false;
		SetDeferred("monitoring", true);
		Visible = true;
		上次条件满足 = !条件允许交互();
		应用条件状态();
	}
}
