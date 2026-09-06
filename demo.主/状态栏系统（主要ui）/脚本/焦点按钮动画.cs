using Godot;

public partial class 焦点按钮动画 : Button
{
	[Export] public int 正常字体大小 = 16;
	[Export] public int 选中字体大小 = 20;
	[Export] public float 动画时间 = 0.15f;
	
	private Tween 动画;
	private int 当前字体大小;
	private int 目标字体大小;
	private StyleBoxFlat 正常样式;
	private StyleBoxFlat 按下样式;
	
	public override void _Ready()
	{
		base._Ready();
		
		ClipText = false;
		SizeFlagsHorizontal = SizeFlags.Expand;
		FocusMode = FocusModeEnum.All;
		
		// 创建统一的样式（包括按下状态）
		正常样式 = new StyleBoxFlat();
		正常样式.BgColor = new Color(0.2f, 0.2f, 0.3f);
		正常样式.SetCornerRadiusAll(5);
		正常样式.ContentMarginLeft = 10;
		正常样式.ContentMarginRight = 10;
		正常样式.ContentMarginTop = 5;
		正常样式.ContentMarginBottom = 5;
		
		按下样式 = (StyleBoxFlat)正常样式.Duplicate();
		按下样式.BgColor = new Color(0.1f, 0.1f, 0.2f); // 按下时的颜色
		
		// 应用到所有状态
		AddThemeStyleboxOverride("normal", 正常样式);
		AddThemeStyleboxOverride("pressed", 按下样式);
		AddThemeStyleboxOverride("hover", 正常样式);
		AddThemeStyleboxOverride("focus", 正常样式);
		
		FocusEntered += 当焦点进入;
		FocusExited += 当焦点离开;
		
		当前字体大小 = (int)GetThemeFontSize("font_size");
		if (当前字体大小 == 0) 当前字体大小 = 正常字体大小;
		设置所有状态字体大小(当前字体大小);
	}
	
	private void 设置所有状态字体大小(int 大小)
	{
		AddThemeFontSizeOverride("font_size", 大小);
		AddThemeFontSizeOverride("normal", 大小);
		AddThemeFontSizeOverride("pressed", 大小);
		AddThemeFontSizeOverride("hover", 大小);
		AddThemeFontSizeOverride("focus", 大小);
	}
	
	private void 当焦点进入()
	{
		目标字体大小 = 选中字体大小;
		开始字体动画();
	}
	
	private void 当焦点离开()
	{
		目标字体大小 = 正常字体大小;
		开始字体动画();
	}
	
	private void 开始字体动画()
	{
		if (动画 != null) 动画.Kill();
		动画 = CreateTween();
		动画.TweenMethod(Callable.From<int>(设置所有状态字体大小), 当前字体大小, 目标字体大小, 动画时间)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);
		动画.Finished += () => 当前字体大小 = 目标字体大小;
	}
}
