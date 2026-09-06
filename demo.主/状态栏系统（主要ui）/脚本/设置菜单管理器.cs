using Godot;
using System;
using System.Threading.Tasks;

public partial class 设置菜单管理器 : Control
{
	[Export] private Button 返回游戏按钮;
	[Export] private Button 音量按钮;
	[Export] private Button 返回主菜单按钮;
	[Export] private Button 游戏按键按钮;
	
public bool 音量面板打开 { get; private set; } = false;

	private bool 正在播放音量动画 = false;
	private bool 激活中 = false;

	public override void _Ready()
	{
		GD.Print("========================================");
		GD.Print("设置菜单管理器初始化开始");
		GD.Print($"节点名称: {Name}");
		GD.Print($"节点路径: {GetPath()}");
		GD.Print($"父节点: {GetParent()?.Name}");
		GD.Print("========================================");

		Visible = false;
		GD.Print($"初始Visible设置为: {Visible}");

		初始化按钮();
		GD.Print("设置菜单管理器初始化完成");
	
	}

	private void 初始化按钮()
	{
		GD.Print("=== 初始化按钮 ===");
		尝试自动查找按钮();

		GD.Print($"返回游戏按钮: {返回游戏按钮 != null}");
		GD.Print($"音量按钮: {音量按钮 != null}");
		GD.Print($"返回主菜单按钮: {返回主菜单按钮 != null}");
		GD.Print($"游戏按键按钮: {游戏按键按钮 != null}");

		if (返回游戏按钮 != null)
		{
			返回游戏按钮.FocusMode = FocusModeEnum.All;
			GD.Print($"设置返回游戏按钮焦点模式: {返回游戏按钮.FocusMode}");
		}
		if (音量按钮 != null)
		{
			音量按钮.FocusMode = FocusModeEnum.All;
			GD.Print($"设置音量按钮焦点模式: {音量按钮.FocusMode}");
		}
		if (返回主菜单按钮 != null)
		{
			返回主菜单按钮.FocusMode = FocusModeEnum.All;
			GD.Print($"设置返回主菜单按钮焦点模式: {返回主菜单按钮.FocusMode}");
		}
		if (游戏按键按钮 != null)
		{
			游戏按键按钮.FocusMode = FocusModeEnum.All;
			GD.Print($"设置游戏按键按钮焦点模式: {游戏按键按钮.FocusMode}");
		}

		设置垂直导航();
		连接按钮事件();
	}

	private void 尝试自动查找按钮()
	{
		GD.Print("=== 尝试自动查找按钮 ===");
		var 动画容器 = GetNodeOrNull<Control>("动画容器");
		if (动画容器 != null)
		{
			GD.Print($"找到动画容器: {动画容器.Name}");
			if (返回游戏按钮 == null)
			{
				返回游戏按钮 = 动画容器.GetNodeOrNull<Button>("返回游戏按钮");
				if (返回游戏按钮 != null) GD.Print("找到返回游戏按钮");
			}
			if (音量按钮 == null)
			{
				音量按钮 = 动画容器.GetNodeOrNull<Button>("音量按钮");
				if (音量按钮 != null) GD.Print("找到音量按钮");
			}
			if (返回主菜单按钮 == null)
			{
				返回主菜单按钮 = 动画容器.GetNodeOrNull<Button>("返回主菜单按钮");
				if (返回主菜单按钮 != null) GD.Print("找到返回主菜单按钮");
			}
			if (游戏按键按钮 == null)
			{
				游戏按键按钮 = 动画容器.GetNodeOrNull<Button>("游戏按键按钮");
				if (游戏按键按钮 != null) GD.Print("找到游戏按键按钮");
			}
		}
		else
		{
			GD.PrintErr("找不到动画容器节点");
			返回游戏按钮 = GetNodeOrNull<Button>("返回游戏按钮") ?? FindChild("返回游戏", true, false) as Button;
			音量按钮 = GetNodeOrNull<Button>("音量按钮") ?? FindChild("音量", true, false) as Button;
			返回主菜单按钮 = GetNodeOrNull<Button>("返回主菜单按钮") ?? FindChild("返回主菜单", true, false) as Button;
			游戏按键按钮 = GetNodeOrNull<Button>("游戏按键按钮") ?? FindChild("游戏按键", true, false) as Button;
		}
	}
 // 新增：打开音量面板的方法（替代原来的部分逻辑）
	public void 打开音量面板()
	{
		if (音量面板打开) return;
		音量面板打开 = true;
		// 这里放原来音量按钮点击后淡入音量面板的代码（包括播放动画、显示面板等）
		// 为了复用，可以将原来音量按钮按下时的核心逻辑提取到这里
	}

	// 新增：关闭音量面板的方法
// 在设置菜单管理器类中
public async Task 关闭音量面板()
{
	if (!音量面板打开) return;
	if (正在播放音量动画) return;

	正在播放音量动画 = true;

	// 1. 立即隐藏音量控制面板
	Node 父节点 = GetParent();
	Node 爷节点 = 父节点?.GetParent();
	Control 音量面板节点 = 爷节点?.GetNodeOrNull<Control>("音量控制面板");
	if (音量面板节点 != null)
	{
		音量面板节点.Visible = false;
		音量面板节点.MouseFilter = Control.MouseFilterEnum.Ignore;
	}
	GD.Print("音量面板已立即隐藏");

	// 2. 播放音量界面关闭动画（如果有）
	AnimationPlayer 音量动画器 = 音量按钮?.GetNodeOrNull<AnimationPlayer>("动画器");
	if (音量动画器 != null && 音量动画器.HasAnimation("音量界面关闭"))
	{
		GD.Print("▶️ 播放动画: 音量界面关闭");
		音量动画器.Play("音量界面关闭");
		await ToSignal(音量动画器, AnimationPlayer.SignalName.AnimationFinished);
		GD.Print("✅ 动画 音量界面关闭 播放完成");
	}
	else
	{
		GD.Print("⚠️ 没有找到音量界面关闭动画");
	}

	音量面板打开 = false;
	正在播放音量动画 = false;

	// 焦点还回到音量按钮
	if (音量按钮 != null)
		音量按钮.GrabFocus();

	GD.Print("音量面板关闭流程完成");
}
private void 设置垂直导航()
{
	GD.Print("=== 设置垂直导航（上下左右重映射：上/右=上一个，下/左=下一个） ===");
	
	// 定义按钮的顺序（从上到下的循环顺序）
	Button[] 按钮顺序 = new Button[] { 返回主菜单按钮, 音量按钮, 游戏按键按钮, 返回游戏按钮 };
	
	for (int i = 0; i < 按钮顺序.Length; i++)
	{
		Button 当前 = 按钮顺序[i];
		if (当前 == null) continue;
		
		// 上一个按钮（索引减一，循环）
		Button 上一个 = 按钮顺序[(i - 1 + 按钮顺序.Length) % 按钮顺序.Length];
		// 下一个按钮（索引加一，循环）
		Button 下一个 = 按钮顺序[(i + 1) % 按钮顺序.Length];
		
		if (上一个 != null)
		{
			当前.FocusNeighborTop = 上一个.GetPath();    // 上键 → 上一个
			当前.FocusNeighborRight = 上一个.GetPath();  // 右键 → 上一个
		}
		
		if (下一个 != null)
		{
			当前.FocusNeighborBottom = 下一个.GetPath(); // 下键 → 下一个
			当前.FocusNeighborLeft = 下一个.GetPath();  // 左键 → 下一个
		}
		
		GD.Print($"设置按钮 {当前.Name}: 上/右 → {上一个?.Name}, 下/左 → {下一个?.Name}");
	}
}


private void 连接按钮事件()
{
	GD.Print("=== 连接按钮事件 ===");

	// 辅助方法：为按钮添加焦点切换音效
	void 绑定焦点音效(Button 按钮)
	{
		if (按钮 != null)
			按钮.FocusEntered += () => 状态栏界面.获取实例()?.播放切换音效();
	}

	// 辅助方法：为按钮添加点击音效（在原有逻辑上叠加）
	void 绑定点击音效(Button 按钮)
	{
		if (按钮 != null)
			按钮.Pressed += () => 状态栏界面.获取实例()?.播放点击音效();
	}

	// --- 返回游戏按钮 ---
	if (返回游戏按钮 != null)
	{
		返回游戏按钮.Pressed += () =>
		{
			GD.Print("返回游戏按钮按下");
			EmitSignal(nameof(返回游戏请求));
		};
		绑定焦点音效(返回游戏按钮);
		绑定点击音效(返回游戏按钮); // 点击时额外播放点击音效
	}

	// --- 音量按钮 ---
	if (音量按钮 != null)
	{
		音量按钮.Pressed += 当音量按钮按下_异步版本;
		绑定焦点音效(音量按钮);
		绑定点击音效(音量按钮);
	}

	// --- 返回主菜单按钮 ---
	if (返回主菜单按钮 != null)
	{
		返回主菜单按钮.Pressed += () =>
		{
			GD.Print("返回主菜单按钮按下");
			EmitSignal(nameof(返回主菜单请求));
		};
		绑定焦点音效(返回主菜单按钮);
		绑定点击音效(返回主菜单按钮);
	}

	// --- 游戏按键按钮 ---
	if (游戏按键按钮 != null)
	{
		游戏按键按钮.Pressed += () =>
		{
			GD.Print("游戏按键按钮按下");
			EmitSignal(nameof(打开按键设置));
		};
		绑定焦点音效(游戏按键按钮);
		绑定点击音效(游戏按键按钮);
	}
}

	private async void 当音量按钮按下_异步版本()
{
	if (正在播放音量动画)
	{
		GD.Print("音量动画正在播放，忽略重复点击");
		return;
	}
	正在播放音量动画 = true;
	音量按钮.Disabled = true;

	GD.Print("音量按钮按下，开始顺序播放动画（await）");

	AnimationPlayer 音量动画器 = 音量按钮?.GetNodeOrNull<AnimationPlayer>("动画器");
	if (音量动画器 == null)
	{
		GD.PrintErr("❌ 音量按钮下没有找到 AnimationPlayer 节点！");
		音量按钮.Disabled = false;
		正在播放音量动画 = false;
		// ★ 即使没有动画，也应该触发音量设置事件
		EmitSignal(nameof(打开音量设置));
		return;  // ← 关键：直接返回
	}

	if (音量动画器.HasAnimation("点击音量后"))
	{
		GD.Print("▶️ 播放动画: 点击音量后");
		音量动画器.Play("点击音量后");
		await ToSignal(音量动画器, AnimationPlayer.SignalName.AnimationFinished);
		GD.Print("✅ 动画 点击音量后 播放完成");
	}
	else GD.PrintErr("❌ 缺少动画：点击音量后");

	if (音量动画器.HasAnimation("音量界面打开"))
	{
		GD.Print("▶️ 播放动画: 音量界面打开");
		音量动画器.Play("音量界面打开");
		await ToSignal(音量动画器, AnimationPlayer.SignalName.AnimationFinished);
		GD.Print("✅ 动画 音量界面打开 播放完成");
	}
	else GD.PrintErr("❌ 缺少动画：音量界面打开");

	淡入音量控制面板();
	激活();

	音量按钮.Disabled = false;
	正在播放音量动画 = false;

	EmitSignal(nameof(打开音量设置));
}

	// ---------- 🎬 淡入已存在的音量控制面板 ----------
private void 淡入音量控制面板()
{
	GD.Print("🎬 开始淡入音量控制面板");

	Node 父节点 = GetParent();
	if (父节点 == null)
	{
		GD.PrintErr("❌ 找不到父节点（设置）");
		return;
	}

	Node 爷节点 = 父节点.GetParent();
	if (爷节点 == null)
	{
		GD.PrintErr("❌ 找不到爷节点（设置标签）");
		return;
	}

	Control 音量面板 = 爷节点.GetNodeOrNull<Control>("音量控制面板");
	if (音量面板 == null)
	{
		GD.PrintErr("❌ 找不到音量控制面板！请确认设置标签下存在名为“音量控制面板”的节点");
		return;
	}
   // ---------- 关键：标记音量面板已打开 ----------
	音量面板打开 = true;   // <--- 添加这一行
	GD.Print($"✅ 找到音量面板: {音量面板.Name}, 当前透明度: {音量面板.Modulate.A}");

	音量面板.Modulate = Colors.Transparent;
	音量面板.MouseFilter = Control.MouseFilterEnum.Ignore;
	音量面板.Visible = true;

	Tween 渐显动画 = CreateTween();
	渐显动画.SetParallel(true);
	渐显动画.TweenProperty(音量面板, "modulate", Colors.White, 0.4f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Sine);
	渐显动画.TweenProperty(音量面板, "scale", Vector2.One, 0.3f)
			.From(new Vector2(0.9f, 0.9f))
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Back);
	渐显动画.TweenProperty(音量面板, "mouse_filter", (int)Control.MouseFilterEnum.Stop, 0.4f)
			.From((int)Control.MouseFilterEnum.Ignore);

	渐显动画.Play();

	// ✨ 关键修改：动画完成后，将焦点设置到音量面板的滑块上
	渐显动画.Finished += () =>
	{
		GD.Print("✅ 音量控制面板淡入完成，准备设置焦点");

		if (音量面板 != null)
		{
			音量面板.MouseFilter = Control.MouseFilterEnum.Stop;

			// 调用音量面板自带的设置焦点方法
			if (音量面板 is 音量控制面板 音量控制脚本)
			{
				音量控制脚本.设置默认焦点();
			}
			else
			{
				// 备用方案：直接找滑块
				var 滑块 = 音量面板.GetNodeOrNull<HSlider>("排列/背景音乐滑块");
				滑块?.GrabFocus();
				GD.Print("⚠️ 音量面板脚本不是音量控制面板，使用备用焦点设置");
			}
		}
	};
}

	// ---------- 🔥 界面激活（原始版本：始终将焦点设置到游戏按键）----------
	public void 激活()
	{
		GD.Print("========================================");
		GD.Print("设置菜单管理器.激活() 被调用");
		GD.Print("========================================");

		if (激活中) 
		{
			GD.Print("已经激活，跳过");
			return;
		}

		GD.Print($"激活前自身可见性: {Visible}");

		激活中 = true;
		Visible = true;

		GD.Print($"激活后自身可见性: {Visible}");
		GD.Print($"激活状态: {激活中}");

		if (返回游戏按钮 != null) 
		{
			GD.Print($"返回游戏按钮之前可见性: {返回游戏按钮.Visible}");
			返回游戏按钮.Visible = true;
			GD.Print($"返回游戏按钮之后可见性: {返回游戏按钮.Visible}");
		}

		if (音量按钮 != null)
		{
			GD.Print($"音量按钮之前可见性: {音量按钮.Visible}");
			音量按钮.Visible = true;
			GD.Print($"音量按钮之后可见性: {音量按钮.Visible}");
		}

		if (返回主菜单按钮 != null)
		{
			GD.Print($"返回主菜单按钮之前可见性: {返回主菜单按钮.Visible}");
			返回主菜单按钮.Visible = true;
			GD.Print($"返回主菜单按钮之后可见性: {返回主菜单按钮.Visible}");
		}

		if (游戏按键按钮 != null)
		{
			GD.Print($"游戏按键按钮之前可见性: {游戏按键按钮.Visible}");
			游戏按键按钮.Visible = true;
			GD.Print($"游戏按键按钮之后可见性: {游戏按键按钮.Visible}");

			// 🔥 延迟设置焦点到游戏按键（原始行为）
			Callable.From(() => 
			{
				GD.Print("尝试将焦点设置到游戏按键按钮");
				if (游戏按键按钮 != null)
				{
					游戏按键按钮.GrabFocus();
					GD.Print($"GrabFocus() 已调用");
					GD.Print($"游戏按键按钮是否有焦点: {游戏按键按钮.HasFocus()}");
					GD.Print($"游戏按键按钮名称: {游戏按键按钮.Name}");
				}
			}).CallDeferred();
		}
		else
		{
			GD.PrintErr("游戏按键按钮为空，无法设置焦点");
		}

		GD.Print("设置菜单已激活");
	}
	
	public void 强制重新激活()
{
	if (激活中)
	{
		// 强制重置状态
		激活中 = false;
		Visible = false;
	}
	激活();
}

public void 关闭()
{
	if (!激活中) return;
	// 关闭时如果音量面板开着，也关掉它
	if (音量面板打开)
		关闭音量面板();
	激活中 = false;
	Visible = false;
}

	public bool 是否激活中() => 激活中;


	[Signal] public delegate void 返回游戏请求EventHandler();
	[Signal] public delegate void 打开音量设置EventHandler();
	[Signal] public delegate void 返回主菜单请求EventHandler();
	[Signal] public delegate void 打开按键设置EventHandler();
}
