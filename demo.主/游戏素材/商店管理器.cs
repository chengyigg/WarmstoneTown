using Godot;
using System.Collections.Generic;
using 你的项目.Scripts.全局;
using 你的项目.Scripts.管理器;

public partial class 商店管理器 : CanvasLayer
{
	[Export] private Button 返回按钮;
	[Export] private 对话序列 离开商店对话序列;

	public List<商品组件> 所有商品 = new List<商品组件>();
	private Control 详情框;
	private Label 详情描述标签;
	[Export] private Font 详情框字体;

	private bool _等待离开对话切换 = false;
	private bool _强制退出 = false;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		创建详情框();
		所有商品.Clear();
		递归查找商品(this);

		if (返回按钮 != null)
			返回按钮.Pressed += On返回按钮Pressed;
		else
			GD.PrintErr("【商店管理器】未找到返回按钮，请在检查器中拖入");

		if (对话播放器.实例 != null)
		{
			对话播放器.实例.对话结束 -= 当对话结束;
			对话播放器.实例.对话结束 += 当对话结束;
		}

		GetTree().Paused = true;
		恢复已购买商品状态();
		恢复按钮状态();
	}

	private void 当对话结束()
	{
		if (_等待离开对话切换)
		{
			_等待离开对话切换 = false;
		对话播放器.实例?.设置禁止NPC交互(false);
			执行场景切换();
		}
	}

	private void 恢复已购买商品状态()
	{
		int 存档位 = 卡牌数据管理器.当前存档位;
		var 存档数据 = 存档管理器.实例?.获取存档数据(存档位);
		if (存档数据 == null) return;

		var 已购买路径列表 = 存档数据.已购买商品路径列表;
		if (已购买路径列表 == null || 已购买路径列表.Count == 0) return;

		foreach (var 商品 in 所有商品)
		{
			var 卡牌路径 = 商品.获取卡牌数据()?.ResourcePath;
			if (string.IsNullOrEmpty(卡牌路径)) continue;
			if (已购买路径列表.Contains(卡牌路径))
				商品.执行购买效果();
		}
	}

	private void 递归查找商品(Node 父节点)
	{
		foreach (Node child in 父节点.GetChildren())
		{
			if (child is 商品组件 商品)
			{
				所有商品.Add(商品);
				商品.鼠标进入商品 += 当鼠标进入商品;
				商品.鼠标离开商品 += 当鼠标离开商品;
				商品.购买请求 += 当购买商品;
			}
			递归查找商品(child);
		}
	}

	private void 创建详情框()
	{
		详情框 = new Control();
		详情框.Name = "商品详情框";
		详情框.Visible = false;
		详情框.MouseFilter = Control.MouseFilterEnum.Ignore;
		详情框.ZIndex = 200;

		var 背景面板 = new Panel();
		背景面板.Name = "背景面板";
		背景面板.MouseFilter = Control.MouseFilterEnum.Ignore;

		var 背景样式 = new StyleBoxFlat();
		背景样式.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
		背景样式.BorderColor = new Color(0.4f, 0.4f, 0.5f);
		背景样式.BorderWidthLeft = 2;
		背景样式.BorderWidthTop = 2;
		背景样式.BorderWidthRight = 2;
		背景样式.BorderWidthBottom = 2;
		背景样式.CornerRadiusTopLeft = 10;
		背景样式.CornerRadiusTopRight = 10;
		背景样式.CornerRadiusBottomRight = 10;
		背景样式.CornerRadiusBottomLeft = 10;
		背景样式.ShadowSize = 8;
		背景样式.ShadowColor = new Color(0, 0, 0, 0.3f);
		背景样式.ContentMarginLeft = 40;
		背景样式.ContentMarginTop = 40;
		背景样式.ContentMarginRight = 80;
		背景样式.ContentMarginBottom = 80;

		背景面板.AddThemeStyleboxOverride("panel", 背景样式);
		背景面板.AnchorLeft = 0;
		背景面板.AnchorTop = 0;
		背景面板.AnchorRight = 1;
		背景面板.AnchorBottom = 1;

		详情描述标签 = new Label();
		详情描述标签.Name = "描述标签";
		详情描述标签.HorizontalAlignment = HorizontalAlignment.Left;
		详情描述标签.VerticalAlignment = VerticalAlignment.Top;
		详情描述标签.AutowrapMode = TextServer.AutowrapMode.WordSmart;

		if (详情框字体 != null)
		{
			var settings = new LabelSettings();
			settings.Font = 详情框字体;
			settings.FontSize = 14;
			详情描述标签.LabelSettings = settings;
		}
		else
		{
			详情描述标签.AddThemeFontSizeOverride("font_size", 14);
		}

		背景面板.AddChild(详情描述标签);
		详情框.AddChild(背景面板);
		AddChild(详情框);
	}

	private void 显示详情框(商品组件 商品)
	{
		if (详情框 == null) return;
		卡牌数据 数据 = 商品.获取卡牌数据();
		if (数据 == null) return;

		详情描述标签.Text = $"{数据.卡牌名称}\n{数据.卡牌描述}";

		详情描述标签.Size = new Vector2(0, 0);
		详情描述标签.CustomMinimumSize = Vector2.Zero;
		float 框宽度 = 250;
		float 手动边距 = 30;
		float 标签可用宽度 = 框宽度 - 手动边距 * 2;
		详情描述标签.Size = new Vector2(标签可用宽度, 0);
		详情描述标签.ForceUpdateTransform();
		详情描述标签.Position = new Vector2(手动边距, 手动边距);

		float 文本高度 = 详情描述标签.GetLineHeight() * (详情描述标签.GetLineCount() + 1);
		float 框高度 = 文本高度 + 手动边距 * 2;
		详情框.Size = new Vector2(框宽度, 框高度);

		Vector2 目标位置 = 商品.GlobalPosition + new Vector2(150, 50);
		详情框.Position = 目标位置;
		详情框.ZIndex = 6;
		详情框.Scale = Vector2.One;

		var tween = 详情框.CreateTween();
		tween.TweenProperty(详情框, "scale", new Vector2(1.2f, 1.2f), 0.2f)
			 .SetEase(Tween.EaseType.Out)
			 .SetTrans(Tween.TransitionType.Back);
		详情框.Visible = true;
	}

	private void 隐藏详情框()
	{
		if (详情框 != null)
		{
			var tween = 详情框.CreateTween();
			tween.Stop();
			详情框.Scale = Vector2.One;
			详情框.Visible = false;
		}
	}

	private void 恢复按钮状态()
	{
		if (玩家卡组管理器.实例 == null)
		{
			CallDeferred(nameof(恢复按钮状态));
			return;
		}
		
		var 玩家卡组 = 玩家卡组管理器.实例.玩家初始卡组;
		if (玩家卡组 == null) return;
		
		foreach (var 商品 in 所有商品)
		{
			var 卡牌数据 = 商品.获取卡牌数据();
			if (卡牌数据 == null) continue;
			bool 已拥有 = false;
			foreach (var 已有卡牌 in 玩家卡组)
			{
				if (已有卡牌 == 卡牌数据)
				{
					已拥有 = true;
					break;
				}
			}
			if (已拥有)
				商品.执行购买效果();
		}
	}

	private void 当鼠标进入商品(商品组件 商品)
	{
		显示详情框(商品);
	}

	private void 当鼠标离开商品(商品组件 商品)
	{
		隐藏详情框();
	}

private void 当购买商品(商品组件 商品, int 价格)
{
	卡牌数据 卡牌数据 = 商品.获取卡牌数据();
	if (卡牌数据 == null)
	{
		GD.PrintErr("商品卡牌数据为空");
		return;
	}

	if (金币管理器.实例 == null)
	{
		GD.PrintErr("金币管理器实例为空");
		return;
	}

	bool 已拥有 = false;
	if (玩家卡组管理器.实例 != null)
	{
		foreach (var 已有卡牌 in 玩家卡组管理器.实例.玩家初始卡组)
		{
			if (已有卡牌 == 卡牌数据)
			{
				已拥有 = true;
				break;
			}
		}
	}
	if (已拥有)
	{
		显示提示("你已经拥有这张卡牌了！");
		return;
	}

	if (金币管理器.实例.金币数量 < 价格)
	{
		显示金币不足提示();
		return;
	}

	金币管理器.实例.减少金币(价格);

	if (玩家卡组管理器.实例 == null)
	{
		GD.PrintErr("玩家卡组管理器实例为空");
		return;
	}
	玩家卡组管理器.实例.添加卡牌(卡牌数据);

	刷新背包UI();
	商品.执行购买效果();

	// ★ 新增：将购买记录写入当前状态（累积记录）
	var 当前状态 = 存档管理器.实例?.获取当前状态();
	if (当前状态 != null)
	{
		string 卡牌路径 = 卡牌数据.ResourcePath;
		if (!string.IsNullOrEmpty(卡牌路径) && !当前状态.已购买商品路径列表.Contains(卡牌路径))
		{
			当前状态.已购买商品路径列表.Add(卡牌路径);
			GD.Print($"[商店管理器] 记录购买商品到当前状态: {卡牌路径}");
		}
	}
	else
	{
		GD.PrintErr("[商店管理器] 无法获取当前状态，购买记录未保存到累积状态");
	}

	显示购买成功提示(商品);
}

	private void 显示提示(string 文本)
	{
		var 提示 = new Label();
		提示.Text = 文本;
		提示.AddThemeFontSizeOverride("font_size", 20);
		提示.AddThemeColorOverride("font_color", Colors.Yellow);
		提示.AddThemeConstantOverride("outline_size", 2);
		提示.AddThemeColorOverride("font_outline_color", Colors.Black);
		提示.Position = GetViewport().GetVisibleRect().Size / 2 - new Vector2(150, 50);
		提示.Modulate = new Color(1, 1, 1, 0);
		GetTree().CurrentScene.AddChild(提示);
		
		var tween = 提示.CreateTween();
		tween.TweenProperty(提示, "modulate:a", 1, 0.2f);
		tween.TweenProperty(提示, "modulate:a", 0, 0.2f).SetDelay(1.0f);
		tween.TweenCallback(Callable.From(() => 提示.QueueFree()));
	}

	private void 刷新背包UI()
	{
		var 状态栏 = GetTree().Root.FindChild("状态栏界面", true, false) as 状态栏界面;
		if (状态栏 != null && 状态栏.界面已打开)
			状态栏.刷新背包按类型(null);
	}

	private void 显示购买成功提示(商品组件 商品)
	{
		var 提示 = new Label();
		提示.Text = $"购买了 {商品.获取卡牌数据()?.卡牌名称}！";
		提示.AddThemeFontSizeOverride("font_size", 20);
		提示.AddThemeColorOverride("font_color", Colors.White);
		提示.AddThemeConstantOverride("outline_size", 2);
		提示.AddThemeColorOverride("font_outline_color", Colors.Black);
		提示.Position = GetViewport().GetVisibleRect().Size / 2 - new Vector2(100, 50);
		提示.Modulate = new Color(1, 1, 1, 0);
		GetTree().CurrentScene.AddChild(提示);

		var tween = 提示.CreateTween();
		tween.TweenProperty(提示, "modulate:a", 1, 0.2f);
		tween.TweenProperty(提示, "modulate:a", 0, 0.2f).SetDelay(1.0f);
		tween.TweenCallback(Callable.From(() => 提示.QueueFree()));
	}

	private void 显示金币不足提示()
	{
		var 提示 = new Label();
		提示.Text = "金币不足！";
		提示.AddThemeFontSizeOverride("font_size", 20);
		提示.AddThemeColorOverride("font_color", Colors.Red);
		提示.AddThemeConstantOverride("outline_size", 2);
		提示.AddThemeColorOverride("font_outline_color", Colors.Black);
		提示.Position = GetViewport().GetVisibleRect().Size / 2 - new Vector2(50, 50);
		提示.Modulate = new Color(1, 1, 1, 0);
		GetTree().CurrentScene.AddChild(提示);

		var tween = 提示.CreateTween();
		tween.TweenProperty(提示, "modulate:a", 1, 0.2f);
		tween.TweenProperty(提示, "modulate:a", 0, 0.2f).SetDelay(1.0f);
		tween.TweenCallback(Callable.From(() => 提示.QueueFree()));
	}

	private void On返回按钮Pressed()
	{
		if (返回按钮 != null)
			返回按钮.Disabled = true;

		if (离开商店对话序列 != null && 对话播放器.实例 != null)
		{
			GetTree().Paused = false;
			_等待离开对话切换 = true;
	对话播放器.实例?.设置禁止NPC交互(true);
			对话播放器.实例.开始对话(离开商店对话序列);
			开始轮询对话结束();
		}
		else
		{
			执行场景切换();
		}
	}

	private void 开始轮询对话结束()
	{
		Timer 轮询计时器 = new Timer();
		轮询计时器.WaitTime = 0.1f;
		轮询计时器.OneShot = false;
		轮询计时器.Timeout += () =>
		{
			if (_强制退出 || !_等待离开对话切换)
			{
				轮询计时器.QueueFree();
				return;
			}
			if (对话播放器.实例 == null || !对话播放器.实例.对话进行中)
			{
				轮询计时器.QueueFree();
				if (_等待离开对话切换 && !_强制退出)
				{
					_等待离开对话切换 = false;
				对话播放器.实例?.设置禁止NPC交互(false);
					执行场景切换();
				}
			}
		};
		AddChild(轮询计时器);
		轮询计时器.Start();
	}

	private void 执行场景切换()
	{
		string 目标路径 = 卡牌数据管理器.上下文.返回场景路径;
		if (string.IsNullOrEmpty(目标路径))
		{
			GD.PrintErr("商店管理器: 返回场景路径为空，无法切换场景！");
			GetTree().Paused = false;
			Visible = false;
			return;
		}
		GetTree().Paused = false;
		if (转场管理器.实例 != null)
			转场管理器.实例.开始转场(目标路径, null, null, false);
		else
			GetTree().ChangeSceneToFile(目标路径);
		Visible = false;
	}
}
