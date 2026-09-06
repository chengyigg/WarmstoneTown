using Godot;
using 你的项目.Scripts.资源;
using 你的项目.Scripts.管理器;
using System.Threading.Tasks;
using System;
namespace 你的项目.Scripts.UI
{
	public partial class 开始界面UI : Control
	{
		private Button _开始按钮;
		private Button _退出按钮;
		private Button _继续按钮;
		private ColorRect _背景;
		private Button _清除存档按钮;
		private 名字输入界面 _名字输入界面;
		
		private int _当前选中按钮索引 = 0;
		private Button[] _按钮列表;
		private bool _键盘导航已启用 = true;
		
		[Export] public 转场动画资源 开始游戏转场资源 { get; set; }
		[Export] public 转场动画资源 继续游戏转场资源 { get; set; }

		[Export] public Color 背景颜色 { get; set; } = new Color(0.1f, 0.1f, 0.2f);
		[Export] public Color 按钮正常颜色 { get; set; } = new Color(0.2f, 0.6f, 0.8f);
		[Export] public Color 按钮悬停颜色 { get; set; } = new Color(0.3f, 0.7f, 0.9f);
		[Export] public Color 按钮按下颜色 { get; set; } = new Color(0.1f, 0.5f, 0.7f);
		[Export] public Color 按钮选中颜色 { get; set; } = new Color(0.4f, 0.8f, 1.0f);
		[Export] public float 按钮选中缩放 { get; set; } = 1.1f;
		[Export] public float 按钮选中时间 { get; set; } = 0.15f;
		[Export] public AudioStream 按钮点击音效;
		[Export] public AudioStream 按钮切换音效;

		public override void _Ready()
		{
			GD.Print("=== 开始界面UI _Ready 开始 ===");
			初始化节点();
			设置UI();
			连接信号();
			应用样式();
			初始化键盘导航();
			if (全局背景音乐管理器.实例 != null)
			{
				string 当前场景路径 = GetTree().CurrentScene.SceneFilePath;
				全局背景音乐管理器.实例.根据场景播放音乐(当前场景路径);
			}
			Callable.From(延迟检查存档).CallDeferred();
			if (_按钮列表 != null && _按钮列表.Length > 0)
				_按钮列表[_当前选中按钮索引].GrabFocus();
			GD.Print("=== 开始界面UI _Ready 完成 ===");
		}
		
		public override void _Input(InputEvent @event)
		{
			if (Visible && _键盘导航已启用 && _按钮列表 != null && _按钮列表.Length > 0)
			{
				if (@event.IsActionPressed("ui_up"))
				{
					上一个按钮();
					GetViewport().SetInputAsHandled();
				}
				else if (@event.IsActionPressed("ui_down"))
				{
					下一个按钮();
					GetViewport().SetInputAsHandled();
				}
				else if (@event.IsActionPressed("ui_accept"))
				{
					激活当前按钮();
					GetViewport().SetInputAsHandled();
				}
			}
		}

		private void 初始化节点()
		{
			_开始按钮 = GetNode<Button>("开始按钮");
			_继续按钮 = GetNode<Button>("继续按钮");
			_退出按钮 = GetNode<Button>("退出按钮");
			_背景 = GetNode<ColorRect>("背景");
			_清除存档按钮 = GetNode<Button>("清除存档按钮");
			_名字输入界面 = GetNode<名字输入界面>("名字输入界面");
			_名字输入界面.Hide();
			_名字输入界面.名字确认 += 当名字确认;
		}
		
		private void 初始化键盘导航()
		{
			_按钮列表 = new Button[] { _开始按钮, _继续按钮, _退出按钮, _清除存档按钮 };
			foreach (Button 按钮 in _按钮列表)
				if (按钮 != null) 按钮.FocusMode = Control.FocusModeEnum.All;
			设置焦点导航();
		}
		
		private void 设置焦点导航()
		{
			var 可见按钮列表 = new System.Collections.Generic.List<Button>();
			foreach (Button 按钮 in _按钮列表)
				if (按钮 != null && 按钮.Visible) 可见按钮列表.Add(按钮);
			if (可见按钮列表.Count == 0) return;
			for (int i = 0; i < 可见按钮列表.Count; i++)
			{
				int 上一个索引 = i - 1;
				if (上一个索引 < 0) 上一个索引 = 可见按钮列表.Count - 1;
				可见按钮列表[i].FocusNeighborTop = 可见按钮列表[上一个索引].GetPath();
				可见按钮列表[i].FocusNeighborBottom = 可见按钮列表[(i + 1) % 可见按钮列表.Count].GetPath();
			}
		}
		
		private void 上一个按钮()
		{
			var 可见按钮索引列表 = new System.Collections.Generic.List<int>();
			for (int i = 0; i < _按钮列表.Length; i++)
				if (_按钮列表[i] != null && _按钮列表[i].Visible) 可见按钮索引列表.Add(i);
			if (可见按钮索引列表.Count == 0) return;
			int 当前在可见列表中的索引 = 可见按钮索引列表.IndexOf(_当前选中按钮索引);
			if (当前在可见列表中的索引 < 0)
				_当前选中按钮索引 = 可见按钮索引列表[0];
			else
			{
				当前在可见列表中的索引--;
				if (当前在可见列表中的索引 < 0)
					当前在可见列表中的索引 = 可见按钮索引列表.Count - 1;
				_当前选中按钮索引 = 可见按钮索引列表[当前在可见列表中的索引];
			}
			全局背景音乐管理器.实例?.播放音效(按钮切换音效);
			_按钮列表[_当前选中按钮索引].GrabFocus();
		}
		
		private void 下一个按钮()
		{
			var 可见按钮索引列表 = new System.Collections.Generic.List<int>();
			for (int i = 0; i < _按钮列表.Length; i++)
				if (_按钮列表[i] != null && _按钮列表[i].Visible) 可见按钮索引列表.Add(i);
			if (可见按钮索引列表.Count == 0) return;
			int 当前在可见列表中的索引 = 可见按钮索引列表.IndexOf(_当前选中按钮索引);
			if (当前在可见列表中的索引 < 0)
				_当前选中按钮索引 = 可见按钮索引列表[0];
			else
				当前在可见列表中的索引 = (当前在可见列表中的索引 + 1) % 可见按钮索引列表.Count;
			_当前选中按钮索引 = 可见按钮索引列表[当前在可见列表中的索引];
			全局背景音乐管理器.实例?.播放音效(按钮切换音效);
			_按钮列表[_当前选中按钮索引].GrabFocus();
		}
		
		private void 激活当前按钮()
		{
			if (_当前选中按钮索引 >= 0 && _当前选中按钮索引 < _按钮列表.Length)
			{
				var 当前按钮 = _按钮列表[_当前选中按钮索引];
				if (当前按钮 != null && 当前按钮.Visible)
					当前按钮.EmitSignal(Button.SignalName.Pressed);
			}
		}
		
		private void 按钮获得焦点(Button 按钮)
		{
			for (int i = 0; i < _按钮列表.Length; i++)
				if (_按钮列表[i] == 按钮) { _当前选中按钮索引 = i; break; }
			var 动画 = CreateTween();
			动画.TweenProperty(按钮, "scale", new Vector2(按钮选中缩放, 按钮选中缩放), 0.07f)
				.SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.Out);
			动画.TweenProperty(按钮, "scale", Vector2.One, 0.07f)
				.SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.In);
		}
		
		private void 按钮失去焦点(Button 按钮)
		{
			按钮.Scale = Vector2.One;
		}
		
		private void 鼠标进入按钮(Button 按钮) => 按钮.GrabFocus();
		private void 鼠标离开按钮(Button 按钮) { }

		private void 设置UI()
		{
			_开始按钮.Text = "开始游戏";
			_继续按钮.Text = "继续游戏";
			_退出按钮.Text = "退出游戏";
		}

		private void 延迟检查存档()
		{
			if (存档管理器.实例 != null)
			{
				bool 有存档 = 存档管理器.实例.是否有任何存档();
				_继续按钮.Visible = 有存档;
				设置焦点导航();
				重置选中到第一个可见按钮();
				if (有存档) 存档管理器.实例.同步玩家名字();
			}
			else
			{
				_继续按钮.Visible = false;
				设置焦点导航();
				重置选中到第一个可见按钮();
			}
		}
		
		private void 重置选中到第一个可见按钮()
		{
			for (int i = 0; i < _按钮列表.Length; i++)
			{
				if (_按钮列表[i] != null && _按钮列表[i].Visible)
				{
					_当前选中按钮索引 = i;
					_按钮列表[i].GrabFocus();
					break;
				}
			}
		}

		private void 连接信号()
		{
			_开始按钮.Pressed += 开始按钮按下;
			_继续按钮.Pressed += 继续按钮按下;
			_退出按钮.Pressed += 退出按钮按下;
			_清除存档按钮.Pressed += 清除存档按钮按下;
			_开始按钮.MouseEntered += () => 鼠标进入按钮(_开始按钮);
			_开始按钮.MouseExited += () => 鼠标离开按钮(_开始按钮);
			_继续按钮.MouseEntered += () => 鼠标进入按钮(_继续按钮);
			_继续按钮.MouseExited += () => 鼠标离开按钮(_继续按钮);
			_退出按钮.MouseEntered += () => 鼠标进入按钮(_退出按钮);
			_退出按钮.MouseExited += () => 鼠标离开按钮(_退出按钮);
		}
		
		private void 清除存档按钮按下()
		{
			全局背景音乐管理器.实例?.播放音效(按钮点击音效);
			var 确认对话框 = new AcceptDialog();
			确认对话框.DialogText = "警告：此操作将永久删除所有存档文件，且不可恢复！\n确定要继续吗？";
			确认对话框.AddButton("取消", true, "cancel");
			确认对话框.AddButton("确定", false, "confirm");
			确认对话框.Confirmed += 执行清除存档;
			AddChild(确认对话框);
			确认对话框.PopupCentered();
		}

private void 执行清除存档()
{
	int 删除计数 = 0;
	using var dir = DirAccess.Open("user://");
	if (dir != null)
	{
		var files = dir.GetFiles();
		foreach (var file in files)
		{
			if (file.EndsWith(".res") && file.Contains("游戏存档"))
			{
				var path = "user://" + file;
				if (DirAccess.RemoveAbsolute(path) == Error.Ok)
					删除计数++;
			}
		}
	}
	
	// 重新加载存档（清空内存中的存档数据）
	if (存档管理器.实例 != null)
	{
		存档管理器.实例.加载所有存档();
	}
	
	// ★ 强制更新 UI：直接隐藏继续按钮，并刷新焦点
	_继续按钮.Visible = false;
	设置焦点导航();
	重置选中到第一个可见按钮();
	
	// 显示操作完成的提示
	var 提示 = new AcceptDialog();
	提示.DialogText = $"已删除 {删除计数} 个存档文件。\n继续按钮已隐藏。";
	提示.Title = "操作完成";
	AddChild(提示);
	提示.PopupCentered();
}

		private void 应用样式()
		{
			_背景.Color = 背景颜色;
			设置按钮样式();
		}

		private void 设置按钮样式()
		{
			var 按钮样式 = new StyleBoxFlat();
			按钮样式.BgColor = 按钮正常颜色;
			按钮样式.BorderColor = new Color(1, 1, 1, 0.3f);
			按钮样式.BorderWidthLeft = 2;
			按钮样式.BorderWidthTop = 2;
			按钮样式.BorderWidthRight = 2;
			按钮样式.BorderWidthBottom = 2;
			按钮样式.CornerRadiusTopLeft = 10;
			按钮样式.CornerRadiusTopRight = 10;
			按钮样式.CornerRadiusBottomRight = 10;
			按钮样式.CornerRadiusBottomLeft = 10;
			按钮样式.ShadowSize = 4;
			按钮样式.ShadowColor = new Color(0, 0, 0, 0.3f);
			_开始按钮.AddThemeStyleboxOverride("normal", 按钮样式);
			_继续按钮.AddThemeStyleboxOverride("normal", 按钮样式);
			_退出按钮.AddThemeStyleboxOverride("normal", 按钮样式);
			
			var 焦点样式 = (StyleBoxFlat)按钮样式.Duplicate();
			焦点样式.BgColor = 按钮选中颜色;
			焦点样式.ShadowSize = 6;
			焦点样式.BorderColor = new Color(1, 1, 1, 0.8f);
			焦点样式.BorderWidthLeft = 4;
			焦点样式.BorderWidthTop = 4;
			焦点样式.BorderWidthRight = 4;
			焦点样式.BorderWidthBottom = 4;
			_开始按钮.AddThemeStyleboxOverride("focus", 焦点样式);
			_继续按钮.AddThemeStyleboxOverride("focus", 焦点样式);
			_退出按钮.AddThemeStyleboxOverride("focus", 焦点样式);

			var 悬停样式 = (StyleBoxFlat)按钮样式.Duplicate();
			悬停样式.BgColor = 按钮悬停颜色;
			悬停样式.ShadowSize = 6;
			_开始按钮.AddThemeStyleboxOverride("hover", 悬停样式);
			_继续按钮.AddThemeStyleboxOverride("hover", 悬停样式);
			_退出按钮.AddThemeStyleboxOverride("hover", 悬停样式);

			var 按下样式 = (StyleBoxFlat)按钮样式.Duplicate();
			按下样式.BgColor = 按钮按下颜色;
			按下样式.ShadowSize = 2;
			_开始按钮.AddThemeStyleboxOverride("pressed", 按下样式);
			_继续按钮.AddThemeStyleboxOverride("pressed", 按下样式);
			_退出按钮.AddThemeStyleboxOverride("pressed", 按下样式);

			_开始按钮.AddThemeFontSizeOverride("font_size", 24);
			_继续按钮.AddThemeFontSizeOverride("font_size", 24);
			_退出按钮.AddThemeFontSizeOverride("font_size", 24);
			_开始按钮.AddThemeColorOverride("font_color", Colors.White);
			_继续按钮.AddThemeColorOverride("font_color", Colors.White);
			_退出按钮.AddThemeColorOverride("font_color", Colors.White);
			_开始按钮.AddThemeColorOverride("font_focus_color", Colors.White);
			_继续按钮.AddThemeColorOverride("font_focus_color", Colors.White);
			_退出按钮.AddThemeColorOverride("font_focus_color", Colors.White);
		}

		private void 继续按钮按下()
		{
			全局背景音乐管理器.实例?.播放音效(按钮点击音效);
			var 界面场景 = GD.Load<PackedScene>("res://保存系统/存档选择界面.tscn");
			if (界面场景 == null) return;
			var 存档选择界面 = 界面场景.Instantiate() as 存档选择界面;
			if (存档选择界面 == null) return;
			GetTree().Root.AddChild(存档选择界面);
			存档选择界面.存档选中 += (int 存档位, bool 是保存) =>
			{
				if (!是保存) 当继续游戏存档选中(存档位, 存档选择界面);
			};
			存档选择界面.界面关闭 += () => 当继续游戏界面关闭(存档选择界面);
			存档选择界面.打开界面(false);
			_键盘导航已启用 = false;
		}

		private void 当继续游戏存档选中(int 存档位, 存档选择界面 界面)
		{
			存档管理器.实例?.重新加载存档(存档位);
			var 存档数据 = 存档管理器.实例.获取存档数据(存档位);
			if (存档数据 != null && 存档数据.是否有存档())
			{
				卡牌数据管理器.当前存档位 = 存档位;
				if (玩家数据管理器.实例 != null && !string.IsNullOrEmpty(存档数据.玩家名字))
					玩家数据管理器.实例.玩家名字 = 存档数据.玩家名字;
				if (条件管理器.实例 != null) 条件管理器.实例.清空所有条件();
				if (存档管理器.实例 != null) 存档管理器.实例.应用条件状态(存档位);
				玩家管理器.实例.销毁玩家();
				var 转场配置 = 继续游戏转场资源 ?? 开始游戏转场资源 ?? 转场管理器.实例?.默认转场动画资源;
				转场管理器.实例?.开始转场(存档数据.存档场景路径, 转场配置, "", true, false, null, false, null, 存档位);
				界面.QueueFree();
			}
		}

		private void 当继续游戏界面关闭(存档选择界面 界面)
		{
			_键盘导航已启用 = true;
			重置选中到第一个可见按钮();
			if (界面 != null && IsInstanceValid(界面)) 界面.QueueFree();
		}

		private void 开始按钮按下()
		{
			全局背景音乐管理器.实例?.播放音效(按钮点击音效);
			_开始按钮.Visible = false;
			_继续按钮.Visible = false;
			_退出按钮.Visible = false;
			_键盘导航已启用 = false;
			_名字输入界面.显示输入界面();
		}

private void 当名字确认(string 玩家名字)
{
	GD.Print($"=== 当名字确认 开始，玩家名字: {玩家名字} ===");
	
	try
	{
		全局背景音乐管理器.实例?.播放音效(按钮点击音效);
		GD.Print("音效播放完成");

		// 检查玩家管理器并安全销毁
		if (玩家管理器.实例 != null)
		{
			GD.Print("准备销毁玩家...");
			try
			{
				玩家管理器.实例.销毁玩家();
				GD.Print("玩家销毁成功");
			}
			catch (Exception ex)
			{
				GD.PrintErr($"销毁玩家时异常: {ex.Message}\n{ex.StackTrace}");
			}
		}
		else
		{
			GD.PrintErr("玩家管理器.实例 为空，跳过销毁玩家");
		}

		GD.Print("准备重置传送后对话记录...");
		对话播放器.实例?.重置传送后对话记录();
		GD.Print("重置传送后对话记录完成");

		GD.Print("准备清空条件...");
		if (条件管理器.实例 != null)
			条件管理器.实例.清空所有条件();
		else
			GD.PrintErr("条件管理器.实例 为空");
		GD.Print("清空条件完成");

		GD.Print($"设置 卡牌数据管理器.当前存档位 = -1 (当前值: {卡牌数据管理器.当前存档位})");
		卡牌数据管理器.当前存档位 = -1;
		GD.Print("存档位设置完成");

		GD.Print("获取转场配置...");
		var 转场配置 = 开始游戏转场资源 ?? 转场管理器.实例?.默认转场动画资源;
		GD.Print($"转场配置: {(转场配置 != null ? "有效" : "null")}");

		GD.Print($"场景加载器.实例: {(场景加载器.实例 != null ? "有效" : "null")}");
		if (场景加载器.实例 != null)
			GD.Print($"游戏场景路径: {场景加载器.实例.游戏场景路径}");
		else
			GD.PrintErr("场景加载器.实例 为空！");

		if (转场配置 != null)
		{
			GD.Print("使用转场管理器开始转场...");
			if (转场管理器.实例 != null)
			{
				try
				{
					转场管理器.实例.开始转场(场景加载器.实例.游戏场景路径, 转场配置, "", false, false, null, false, null, -1);
					GD.Print("转场管理器.开始转场 调用完成");
				}
				catch (Exception ex)
				{
					GD.PrintErr($"转场管理器.开始转场 异常: {ex.Message}\n{ex.StackTrace}");
				}
			}
			else
			{
				GD.PrintErr("转场管理器.实例 为空，无法执行转场");
			}
		}
		else
		{
			GD.Print("直接切换场景...");
			string 场景路径 = 场景加载器.实例?.游戏场景路径;
			if (!string.IsNullOrEmpty(场景路径))
			{
				try
				{
					GetTree().ChangeSceneToFile(场景路径);
					GD.Print($"ChangeSceneToFile 调用完成，路径: {场景路径}");
				}
				catch (Exception ex)
				{
					GD.PrintErr($"ChangeSceneToFile 异常: {ex.Message}\n{ex.StackTrace}");
				}
			}
			else
			{
				GD.PrintErr("场景加载器.实例 或 游戏场景路径 为空，无法切换场景");
			}
		}
	}
	catch (Exception ex)
	{
		GD.PrintErr($"当名字确认 整体异常: {ex.Message}\n{ex.StackTrace}");
	}
	GD.Print("=== 当名字确认 结束 ===");
}

		private void 退出按钮按下()
		{
			全局背景音乐管理器.实例?.播放音效(按钮点击音效);
			场景加载器.实例.退出游戏();
		}
		
public void 返回主菜单()
{
	GD.Print("返回主菜单");
	
	// 恢复所有按钮可见性
	_开始按钮.Visible = true;
	_退出按钮.Visible = true;
	
	// 检查存档并显示继续按钮
	bool 有存档 = 存档管理器.实例 != null && 存档管理器.实例.是否有任何存档();
	_继续按钮.Visible = 有存档;
	
	// 显示清除存档按钮（如果它被隐藏了，根据你的设计决定）
	_清除存档按钮.Visible = true;
	
	// 恢复键盘导航
	_键盘导航已启用 = true;
	
	// 重置焦点导航
	设置焦点导航();
	重置选中到第一个可见按钮();
}
	}
}
