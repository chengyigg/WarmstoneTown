using Godot;
using System;

public partial class 键盘输入处理器 : Node
{
	public enum 输入模式 { 浏览手牌, 预选卡牌, 选择目标 }

	[Export] private StyleBoxFlat _描述框样式;
	private 输入模式 _当前模式 = 输入模式.浏览手牌;
	private 卡牌UI _当前预选卡牌UI;
	private 卡牌实例 _当前预选卡牌;

	[Export] private 手牌管理器 _手牌管理器;
	[Export] private 卡牌战斗管理器 _战斗管理器;
	[Export] private 目标选择器 _目标选择器;

	private 卡牌UI _查看中的卡牌UI;
	private Control _临时描述框;
	private CanvasLayer _临时描述层;
	private bool _正在结束查看 = false;

	public override void _Ready()
	{
		if (_手牌管理器 == null || _战斗管理器 == null)
			GD.PrintErr("键盘输入处理器：未设置必要的管理器引用");
	}

	public override void _Input(InputEvent 事件)
{
	// 对话进行中时，所有战斗键盘输入无效
	if (对话播放器.实例 != null && 对话播放器.实例.对话进行中)
		return;

	if (!_战斗管理器.战斗进行中) return;

	// ----- 处理 C 键（查看卡牌描述）——不受行动限制 -----
	if (事件 is InputEventKey keyC && keyC.Keycode == Key.C)
	{
		if (keyC.Pressed)
		{
			if (_查看中的卡牌UI == null)
				开始查看当前卡牌();
		}
		else
		{
			结束查看当前卡牌();
		}
		return;  // 处理完 C 键后直接返回，不再检查行动限制
	}

	// ----- 其他所有战斗操作必须检查玩家是否可以行动 -----
	if (!_战斗管理器.玩家是否可以行动())
		return;

	// 只处理键盘按下事件（释放事件忽略）
	if (事件 is InputEventKey 键盘事件 && 键盘事件.Pressed)
	{
		switch (_当前模式)
		{
			case 输入模式.浏览手牌:
				处理浏览模式按键(键盘事件);
				break;
			case 输入模式.预选卡牌:
				处理预选模式按键(键盘事件);
				break;
			case 输入模式.选择目标:
				处理选目标模式按键(键盘事件);
				break;
		}
	}
}
	// ==================== 临时描述框 ====================
	private void 显示临时描述框(卡牌UI 目标卡牌, string 描述文本)
	{
		强制销毁描述框();
		
		_临时描述层 = new CanvasLayer();
		_临时描述层.Layer = 10;
		_临时描述层.Name = "临时描述层";
		GetTree().Root.AddChild(_临时描述层);
		
		_临时描述框 = new Control();
		_临时描述框.Name = "描述框";
		_临时描述框.Visible = true;
		_临时描述框.MouseFilter = Control.MouseFilterEnum.Ignore;
		_临时描述层.AddChild(_临时描述框);
		
		var 背景 = new Panel();
		背景.Name = "背景面板";
		
		StyleBoxFlat 实际样式;
		if (_描述框样式 != null)
		{
			实际样式 = (StyleBoxFlat)_描述框样式.Duplicate();
			if (实际样式.BgColor.A < 0.01f)
			{
				GD.PrintErr("[键盘] 警告：样式的背景色完全透明！将强制使用默认背景色");
				实际样式.BgColor = new Color(0.05f, 0.05f, 0.05f, 0.95f);
			}
		}
		else
		{
			实际样式 = new StyleBoxFlat();
			实际样式.BgColor = new Color(0.05f, 0.05f, 0.05f, 0.95f);
			实际样式.SetCornerRadiusAll(8);
			实际样式.SetBorderWidthAll(2);
			实际样式.BorderColor = new Color(0.9f, 0.7f, 0.2f);
			实际样式.ContentMarginLeft = 10;
			实际样式.ContentMarginRight = 10;
			实际样式.ContentMarginTop = 10;
			实际样式.ContentMarginBottom = 10;
		}
		背景.AddThemeStyleboxOverride("panel", 实际样式);
		_临时描述框.AddChild(背景);
		
		var 标签 = new Label();
		标签.Text = 描述文本;
		标签.HorizontalAlignment = HorizontalAlignment.Left;
		标签.VerticalAlignment = VerticalAlignment.Top;
		标签.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		标签.AddThemeFontSizeOverride("font_size", 17);
		标签.AddThemeColorOverride("font_color", Colors.White);
		背景.AddChild(标签);
		
		float 框宽度 = 300f;
		float 内边距 = 30f;
		float 文本可用宽度 = 框宽度 - 内边距 * 2;
		标签.Size = new Vector2(文本可用宽度, 0);
		标签.UpdateMinimumSize();
		float 文本高度 = 标签.GetLineHeight() * (标签.GetLineCount() + 1);
		float 框高度 = 文本高度 + 内边距 * 2;
		_临时描述框.Size = new Vector2(框宽度, 框高度);
		标签.Position = new Vector2(内边距, 内边距);
		背景.Size = _临时描述框.Size;
		
		Vector2 目标位置 = 目标卡牌.GlobalPosition + new Vector2(200, 60);
		var 视口大小 = GetViewport().GetVisibleRect().Size;
		if (目标位置.X + 框宽度 > 视口大小.X)
			目标位置.X = 视口大小.X - 框宽度 - 10;
		if (目标位置.Y + 框高度 > 视口大小.Y)
			目标位置.Y = 视口大小.Y - 框高度 - 10;
		_临时描述框.Position = 目标位置;
		
		_临时描述框.Scale = new Vector2(0.8f, 0.8f);
		var tween = _临时描述框.CreateTween();
		tween.SetTrans(Tween.TransitionType.Back);
		tween.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(_临时描述框, "scale", Vector2.One, 0.2f);
	}
	public void 强制重置()
{
	// 清除正在查看的卡牌
	if (_查看中的卡牌UI != null)
		结束查看当前卡牌();
	
	// 清除预选状态
	if (_当前预选卡牌UI != null && IsInstanceValid(_当前预选卡牌UI))
		_当前预选卡牌UI.处理结束预览();
	_当前预选卡牌UI = null;
	_当前预选卡牌 = null;
	
	// 隐藏目标选择器（如果有显示）
	_目标选择器?.显示(false);
	
	// 重置模式为浏览手牌
	_当前模式 = 输入模式.浏览手牌;
	
	// 强制刷新当前选中卡牌高亮（手牌管理器内部会修正索引）
	_手牌管理器?.重置选中索引();
	
	// 可选：将焦点显式设置到手牌容器或第一个卡牌
	if (_手牌管理器 != null && _手牌管理器.当前卡牌UI列表.Count > 0)
	{
		var 第一个卡牌 = _手牌管理器.当前卡牌UI列表[0] as Control;
		第一个卡牌?.GrabFocus();
	}
	
	// 强制刷新战斗管理器的玩家交互启用（确保按钮没有被禁用）
	_战斗管理器?.通知玩家卡牌交互启用(true);
	
	GD.Print("[键盘输入处理器] 强制重置完成，模式=浏览手牌");
}
	private void 强制销毁描述框()
	{
		if (_临时描述框 != null)
		{
			var tween = _临时描述框.CreateTween();
			tween?.Kill();
			_临时描述框.QueueFree();
			_临时描述框 = null;
		}
		if (_临时描述层 != null)
		{
			_临时描述层.QueueFree();
			_临时描述层 = null;
		}
	}
	
	private void 销毁临时描述框()
	{
		if (_临时描述框 != null)
		{
			var tween = _临时描述框.CreateTween();
			tween.SetTrans(Tween.TransitionType.Cubic);
			tween.SetEase(Tween.EaseType.In);
			tween.TweenProperty(_临时描述框, "scale", new Vector2(0.8f, 0.8f), 0.1f);
			tween.TweenCallback(Callable.From(() => 强制销毁描述框()));
		}
		else
		{
			强制销毁描述框();
		}
	}
	
	// ==================== 查看功能（C键） ====================
	  private void 开始查看当前卡牌()
	{
		if (_查看中的卡牌UI != null) return;

		卡牌UI 当前卡牌UI = null;
		if (_当前模式 == 输入模式.预选卡牌 && _当前预选卡牌UI != null)
			当前卡牌UI = _当前预选卡牌UI;
		else if (_当前模式 == 输入模式.浏览手牌 || _当前模式 == 输入模式.选择目标)
			当前卡牌UI = _手牌管理器?.获取当前选中的卡牌UI();

		if (当前卡牌UI == null) return;

		_查看中的卡牌UI = 当前卡牌UI;
		当前卡牌UI.处理开始预览();
		string 描述 = 当前卡牌UI.获取卡牌描述();
		显示临时描述框(当前卡牌UI, 描述);
	}

	private void 结束查看当前卡牌()
	{
		if (_正在结束查看) return;
		_正在结束查看 = true;

		if (_查看中的卡牌UI != null && IsInstanceValid(_查看中的卡牌UI))
		{
			_查看中的卡牌UI.处理结束预览();
			_查看中的卡牌UI = null;
		}
		销毁临时描述框();
		_正在结束查看 = false;
	}

	// ==================== 浏览手牌模式 ====================
	private void 处理浏览模式按键(InputEventKey 事件)
	{
		if (事件.Keycode == Key.Right || 事件.Keycode == Key.Left)
		{
			结束查看当前卡牌();
			if (_当前预选卡牌UI != null)
				取消预选();
			if (事件.Keycode == Key.Right)
				_手牌管理器?.选中下一张();
			else
				_手牌管理器?.选中上一张();
		}
		else if (事件.Keycode == Key.Z)
		{
			结束查看当前卡牌();
			var 当前卡牌UI = _手牌管理器?.获取当前选中的卡牌UI();
			if (当前卡牌UI == null) return;
			var 卡牌 = 当前卡牌UI.获取卡牌数据();
			if (卡牌 == null) return;

			_当前预选卡牌UI = 当前卡牌UI;
			_当前预选卡牌 = 卡牌;
			_当前预选卡牌UI.处理开始预览();
			_当前模式 = 输入模式.预选卡牌;
			_战斗管理器?.添加战斗日志($"预选了 {卡牌.基础数据.卡牌名称}，再按 Z 确认使用");
		}
		else if (事件.Keycode == Key.E)
		{
			结束查看当前卡牌();
			_战斗管理器?.结束玩家回合();
		}
		else if (事件.Keycode == Key.A)
		{
			结束查看当前卡牌();
			_战斗管理器?.切换强化模式();
		}
	}

	private void 处理预选模式按键(InputEventKey 事件)
{
	if (事件.Keycode == Key.Z)
	{
		结束查看当前卡牌();
		if (_当前预选卡牌 == null || _当前预选卡牌UI == null)
		{
			取消预选();
			return;
		}

		if (_当前预选卡牌.基础数据.类型 == 卡牌数据.卡牌类型.攻击)
		{
			// 先检查行动值是否足够
			if (_战斗管理器.玩家单位.行动条 < _当前预选卡牌.基础数据.行动值消耗)
			{
				_战斗管理器?.添加战斗日志("行动值不足，无法使用此卡牌！");
				取消预选();
				return;
			}
			_战斗管理器?.添加战斗日志("请选择目标（左右键/Tab切换，Z确认，X取消）");
			_当前模式 = 输入模式.选择目标;
			_目标选择器?.显示(true);
		}
		else
		{
			_战斗管理器?.玩家使用卡牌(_当前预选卡牌);
			处理卡牌使用后();
		}
	}
	else if (事件.Keycode == Key.X)
	{
		结束查看当前卡牌();
		_战斗管理器?.添加战斗日志("取消使用卡牌");
		取消预选();
	}
	else if (事件.Keycode == Key.Right || 事件.Keycode == Key.Left)
	{
		结束查看当前卡牌();
		var 旧卡牌UI = _当前预选卡牌UI;
		取消预选();

		if (事件.Keycode == Key.Right)
			_手牌管理器?.选中下一张();
		else
			_手牌管理器?.选中上一张();

		var 新卡牌UI = _手牌管理器?.获取当前选中的卡牌UI();
		if (新卡牌UI != null)
		{
			_当前预选卡牌UI = 新卡牌UI;
			_当前预选卡牌 = 新卡牌UI.获取卡牌数据();
			_当前预选卡牌UI.处理开始预览();
			_当前模式 = 输入模式.预选卡牌;
			_战斗管理器?.添加战斗日志($"预选了 {_当前预选卡牌.基础数据.卡牌名称}，再按 Z 确认使用");
		}
		else
		{
			_当前模式 = 输入模式.浏览手牌;
		}
	}
}

private void 处理选目标模式按键(InputEventKey 事件)
{
	if (事件.Keycode == Key.Z)
	{
		结束查看当前卡牌();
		// 再次验证行动值（防止等待期间行动值被消耗）
		if (_战斗管理器.玩家单位.行动条 < _当前预选卡牌.基础数据.行动值消耗)
		{
			_战斗管理器?.添加战斗日志("行动值不足，无法使用此卡牌！");
			取消预选();
			_目标选择器?.显示(false);
			_当前模式 = 输入模式.浏览手牌;
			return;
		}
		var 目标 = _目标选择器?.获取当前选中的目标();
		if (目标 == null)
		{
			_战斗管理器?.添加战斗日志("没有选中目标");
			return;
		}
		_战斗管理器?.玩家使用卡牌(_当前预选卡牌, 目标);
		处理卡牌使用后();
	}
		else if (事件.Keycode == Key.X || 事件.Keycode == Key.Escape)
		{
			_战斗管理器?.添加战斗日志("取消选择目标");
			_目标选择器?.显示(false);
			_当前模式 = 输入模式.预选卡牌;
		}
		else if (事件.Keycode == Key.Right || 事件.Keycode == Key.Left || 事件.Keycode == Key.Tab)
		{
			_目标选择器?.切换下一个目标();
		}
	}

	// ==================== 辅助方法 ====================
	private void 取消预选()
	{
		if (_当前预选卡牌UI != null && IsInstanceValid(_当前预选卡牌UI))
		{
			_当前预选卡牌UI.处理结束预览();
		}
		_当前预选卡牌UI = null;
		_当前预选卡牌 = null;
		_当前模式 = 输入模式.浏览手牌;
	}

	private void 处理卡牌使用后()
	{
		_目标选择器?.显示(false);
		_当前预选卡牌UI = null;
		_当前预选卡牌 = null;
		_当前模式 = 输入模式.浏览手牌;
		_手牌管理器?.修正选中索引();
		结束查看当前卡牌();
	}
}
