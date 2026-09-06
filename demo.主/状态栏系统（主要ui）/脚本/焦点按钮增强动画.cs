using Godot;
using System;

public partial class 焦点按钮增强动画 : Button
{
	// 可以调整的参数
	[Export] public float 正常大小 = 16;          // 正常字体大小
	[Export] public float 选中大小 = 24;          // 选中时字体大小
	[Export] public float 动画时间 = 0.15f;       // 放大动画时间
	
	// 四边形参数
	[Export] private Color 下边颜色 = new Color("#3c6e3c");
	[Export] private Color 上边颜色 = new Color("#6ca66c");
	[Export] private float 四边形透明度 = 0.8f;
	[Export] private float 浮动幅度 = 5.0f;       // 浮动幅度（像素）
	[Export] private float 浮动速度 = 2.0f;       // 浮动速度
	
	// 节点引用
	private Tween 动画;
	private Label 文本标签;
	private SubViewportContainer 四边形容器;
	private SubViewport 四边形视口;
	private Polygon2D 下边四边形;
	private Polygon2D 上边四边形;
	
	// 状态
	private bool 已获得焦点 = false;
	private float 浮动时间 = 0f;
	
	public override void _Ready()
	{
		base._Ready();
		
		// 确保按钮可以接收焦点
		FocusMode = FocusModeEnum.All;
		
		// 获取按钮的文本标签
		获取文本标签();
		
		// 创建四边形显示系统
		创建四边形系统();
		
		// 连接焦点事件
		FocusEntered += 当焦点进入;
		FocusExited += 当焦点离开;
		
		// 创建动画对象
		动画 = GetTree().CreateTween();
		
		GD.Print($"{Name}: 四边形系统初始化完成");
		
		// 测试：立即显示四边形
		// 测试四边形显示();
	}
	
	private void 获取文本标签()
	{
		// 尝试获取按钮内的Label节点
		var 子节点 = GetChildren();
		foreach (var 节点 in 子节点)
		{
			if (节点 is Label 标签)
			{
				文本标签 = 标签;
				GD.Print($"{Name}: 找到文本标签");
				break;
			}
		}
		
		if (文本标签 == null)
		{
			GD.Print($"{Name}: 没有找到文本标签");
		}
	}
	
	private void 创建四边形系统()
	{
		// 创建SubViewportContainer（这是关键）
		四边形容器 = new SubViewportContainer();
		四边形容器.Name = $"{Name}_四边形容器";
		四边形容器.Size = new Vector2(200, 200); // 初始大小，会动态调整
		四边形容器.ProcessMode = ProcessModeEnum.Always; // 始终处理
		四边形容器.Visible = false; // 初始不可见
		
		// 创建SubViewport
		四边形视口 = new SubViewport();
		四边形视口.Name = "四边形视口";
		四边形视口.Size = new Vector2I(200, 200);  // 改为 Vector2I
		四边形视口.TransparentBg = true; // 透明背景
		四边形视口.RenderTargetUpdateMode = SubViewport.UpdateMode.Always; // 总是更新
		
		// 将视口添加到容器
		四边形容器.AddChild(四边形视口);
		
		// 创建四边形
		创建四边形();
		
		// 将容器添加到场景根节点
		GetTree().Root.AddChild(四边形容器);
		
		GD.Print($"{Name}: 创建四边形系统完成");
	}
	
	private void 创建四边形()
	{
		// 创建下边四边形
		下边四边形 = new Polygon2D();
		下边四边形.Name = "下边四边形";
		
		// 创建不规则四边形的顶点
		Vector2[] 下边顶点 = new Vector2[]
		{
			new Vector2(-50, 25),   // 左下
			new Vector2(-40, -15),  // 左上
			new Vector2(40, -25),   // 右上
			new Vector2(50, 20)     // 右下
		};
		
		下边四边形.Polygon = 下边顶点;
		下边四边形.Color = 下边颜色;
		下边四边形.Color = new Color(下边四边形.Color, 四边形透明度);
		
		// 创建上边四边形
		上边四边形 = new Polygon2D();
		上边四边形.Name = "上边四边形";
		
		Vector2[] 上边顶点 = new Vector2[]
		{
			new Vector2(-45, -20),  // 左下
			new Vector2(-35, -40),  // 左上
			new Vector2(45, -45),   // 右上
			new Vector2(55, -10)    // 右下
		};
		
		上边四边形.Polygon = 上边顶点;
		上边四边形.Color = 上边颜色;
		上边四边形.Color = new Color(上边四边形.Color, 四边形透明度 * 0.7f);
		
		// 添加到视口
		四边形视口.AddChild(下边四边形);
		四边形视口.AddChild(上边四边形);
		
		// 初始缩放很小
		下边四边形.Scale = new Vector2(0.1f, 0.1f);
		上边四边形.Scale = new Vector2(0.1f, 0.1f);
		
		GD.Print($"{Name}: 四边形创建完成");
	}
	
	// 测试方法：立即显示四边形
	private void 测试四边形显示()
	{
		四边形容器.Visible = true;
		下边四边形.Visible = true;
		上边四边形.Visible = true;
		下边四边形.Scale = new Vector2(1.0f, 1.0f);
		上边四边形.Scale = new Vector2(1.0f, 1.0f);
		
		// 设置到屏幕中央以便测试
		Vector2 屏幕中心 = GetViewport().GetVisibleRect().GetCenter();
		四边形容器.Position = 屏幕中心 - 四边形容器.Size / 2;
		
		// 在视口内居中四边形
		下边四边形.Position = 四边形视口.Size / 2;
		上边四边形.Position = 四边形视口.Size / 2;
		
		GD.Print($"{Name}: 四边形测试显示在: {屏幕中心}");
		GD.Print($"{Name}: 容器位置: {四边形容器.Position}, 容器大小: {四边形容器.Size}");
	}
	
	public override void _Process(double delta)
	{
		base._Process(delta);
		
		if (已获得焦点 && 四边形容器 != null && 下边四边形 != null && 上边四边形 != null)
		{
			// 更新浮动时间
			浮动时间 += (float)delta * 浮动速度;
			
			// 计算浮动偏移
			float 偏移X = Mathf.Sin(浮动时间 * 1.5f) * 浮动幅度 * 0.5f;
			float 偏移Y = Mathf.Sin(浮动时间 * 2.0f) * 浮动幅度;
			
			// 获取按钮当前全局位置
			Rect2 按钮矩形 = GetGlobalRect();
			Vector2 按钮中心 = 按钮矩形.GetCenter();
			
			// 更新容器位置（跟随按钮）
			Vector2 容器位置 = 按钮中心 - 四边形容器.Size / 2;
			四边形容器.Position = 容器位置;
			
			// 应用浮动效果（在容器内偏移）
			下边四边形.Position = 四边形视口.Size / 2 + new Vector2(偏移X, 偏移Y) * 0.7f;
			上边四边形.Position = 四边形视口.Size / 2 + new Vector2(-偏移X * 0.5f, 偏移Y * 0.5f);
			
			// 添加轻微的旋转浮动
			下边四边形.RotationDegrees = Mathf.Sin(浮动时间 * 0.8f) * 5f;
			上边四边形.RotationDegrees = Mathf.Sin(浮动时间 * 1.2f) * 3f;
			
			// 动态调整容器大小
			float 按钮大小 = Mathf.Max(按钮矩形.Size.X, 按钮矩形.Size.Y);
			Vector2 新大小 = new Vector2(按钮大小 * 3, 按钮大小 * 3);
			四边形容器.Size = 新大小;
			四边形视口.Size = new Vector2I((int)新大小.X, (int)新大小.Y);  // 改为 Vector2I
		}
	}
	
	private void 当焦点进入()
	{
		GD.Print($"{Name} 获得焦点，开始放大并显示四边形");
		已获得焦点 = true;
		
		// 确保四边形系统存在
		if (四边形容器 == null)
		{
			创建四边形系统();
		}
		
		// 停止之前动画
		if (动画 != null)
		{
			动画.Kill();
		}
		动画 = GetTree().CreateTween();
		动画.SetParallel(); // 并行执行
		
		// 1. 放大文字
		if (文本标签 != null)
		{
			var 目标大小 = 文本标签.Scale * (选中大小 / 正常大小);
			动画.TweenProperty(文本标签, "scale", 目标大小, 动画时间)
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out);
		}
		else
		{
			var 目标大小 = new Vector2(1.2f, 1.2f);
			动画.TweenProperty(this, "scale", 目标大小, 动画时间)
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out);
		}
		
		// 2. 显示容器
		四边形容器.Visible = true;
		
		// 3. 获取按钮当前位置
		Rect2 按钮矩形 = GetGlobalRect();
		Vector2 按钮中心 = 按钮矩形.GetCenter();
		float 按钮大小 = Mathf.Max(按钮矩形.Size.X, 按钮矩形.Size.Y);
		
		// 4. 设置容器位置和大小
		Vector2 容器目标大小 = new Vector2(按钮大小 * 3, 按钮大小 * 3);
		Vector2 容器目标位置 = 按钮中心 - 容器目标大小 / 2;
		
		四边形容器.Size = 容器目标大小;
		四边形视口.Size = new Vector2I((int)容器目标大小.X, (int)容器目标大小.Y);  // 改为 Vector2I
		四边形容器.Position = 容器目标位置;
		
		// 5. 设置四边形位置并显示
		下边四边形.Position = 四边形视口.Size / 2;
		上边四边形.Position = 四边形视口.Size / 2;
		
		下边四边形.Visible = true;
		上边四边形.Visible = true;
		
		// 6. 放大下边四边形
		动画.TweenProperty(下边四边形, "scale", new Vector2(1.0f, 1.0f), 动画时间 * 1.5f)
			.SetTrans(Tween.TransitionType.Back)
			.SetEase(Tween.EaseType.Out);
		
		// 7. 放大上边四边形（稍微延迟）
		动画.TweenProperty(上边四边形, "scale", new Vector2(1.0f, 1.0f), 动画时间 * 1.5f)
			.SetDelay(动画时间 * 0.2f)
			.SetTrans(Tween.TransitionType.Back)
			.SetEase(Tween.EaseType.Out);
		
		// 8. 重置旋转
		下边四边形.RotationDegrees = 0;
		上边四边形.RotationDegrees = 0;
		
		GD.Print($"{Name}: 四边形显示在容器中，容器位置: {容器目标位置}, 大小: {容器目标大小}");
	}
	
	private void 当焦点离开()
	{
		GD.Print($"{Name} 失去焦点，恢复大小并隐藏四边形");
		已获得焦点 = false;
		
		// 停止之前动画
		if (动画 != null)
		{
			动画.Kill();
		}
		动画 = GetTree().CreateTween();
		动画.SetParallel(); // 并行执行
		
		// 1. 缩小文字
		if (文本标签 != null)
		{
			var 原始大小 = 文本标签.Scale * (正常大小 / 选中大小);
			动画.TweenProperty(文本标签, "scale", 原始大小, 动画时间)
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out);
		}
		else
		{
			动画.TweenProperty(this, "scale", new Vector2(1.0f, 1.0f), 动画时间)
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out);
		}
		
		// 2. 缩小四边形
		动画.TweenProperty(下边四边形, "scale", new Vector2(0.1f, 0.1f), 动画时间 * 0.8f)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In);
		
		动画.TweenProperty(上边四边形, "scale", new Vector2(0.1f, 0.1f), 动画时间 * 0.8f)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In);
		
		// 3. 动画完成后隐藏四边形和容器
		动画.TweenCallback(Callable.From(() => 
		{
			if (四边形容器 != null)
			{
				四边形容器.Visible = false;
			}
			if (下边四边形 != null)
			{
				下边四边形.Visible = false;
				下边四边形.RotationDegrees = 0;
			}
			if (上边四边形 != null)
			{
				上边四边形.Visible = false;
				上边四边形.RotationDegrees = 0;
			}
		})).SetDelay(动画时间 * 0.8f);
	}
	
	// 重写 _ExitTree 清理资源
	public override void _ExitTree()
	{
		base._ExitTree();
		
		// 清理四边形容器
		if (四边形容器 != null && 四边形容器.IsInsideTree())
		{
			四边形容器.QueueFree();
		}
		
		GD.Print($"{Name}: 清理四边形资源");
	}
}
