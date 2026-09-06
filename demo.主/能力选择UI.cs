using Godot;
using System;
using System.Collections.Generic;

public partial class 能力选择UI : CanvasLayer
{
	// 导出变量
	[Export] public float 显示动画时长 = 0.3f;
	[Export] public float 隐藏动画时长 = 0.2f;
	[Export] public float 圆环半径 = 150f;
	[Export] public float 悬停放大比例 = 1.2f;
	[Export] public float 选项基础大小 = 64f;
	
	// 修改对话相关导出变量
	[Export] public Godot.Collections.Dictionary<string, 对话序列> 能力第一次对话序列 { get; set; } 
		= new Godot.Collections.Dictionary<string, 对话序列>();
		
	[Export] public Godot.Collections.Dictionary<string, 对话序列> 能力后续对话序列 { get; set; } 
		= new Godot.Collections.Dictionary<string, 对话序列>();
	
	 // 新增：记录每个能力的对话状态
	private Dictionary<string, bool> 能力已对话记录 = new Dictionary<string, bool>();
	
	
	// 节点引用
	private Control 根节点;
	private Control 能力选项容器;
	private ColorRect 圆环背景;
	private Tween 动画补间;
	
	// 状态变量
	private bool 正在显示 = false;
	private bool 正在隐藏 = false;
	private 能力选项 当前悬停选项 = null;
	private Dictionary<能力选项, Vector2> 选项位置 = new Dictionary<能力选项, Vector2>();
	private Vector2 屏幕中心;
	
	// 能力选项类
	public partial class 能力选项
	{
		public TextureButton 按钮;
		public string 能力名称;
		public Action 选择回调;
		public Vector2 原始尺寸;
		public bool 已激活 = false;
	}
	
	private List<能力选项> 所有选项 = new List<能力选项>();
	
	// 当前激活的能力
	private 能力选项 当前激活的能力 = null;
	
	// 新增：选择完成事件
	[Signal] public delegate void 能力选择完成EventHandler(string 能力名称, 对话序列 对话序列);
	
	public override void _Ready()
	{
		// 计算屏幕中心
		var 视口大小 = GetViewport().GetVisibleRect().Size;
		屏幕中心 = 视口大小 / 2;
		
		// 创建UI元素
		创建UI元素();
		
		// 初始化隐藏
		根节点.Modulate = new Color(1, 1, 1, 0);
		根节点.Visible = false;
		
		// 收集所有能力选项
		收集能力选项();
		
		// 排列选项位置
		排列选项位置();
		
		GD.Print("能力选择UI已初始化");
		GD.Print($"屏幕中心: {屏幕中心}, 视口大小: {视口大小}");
	}
	
	public override void _Process(double delta)
	{
		if (正在显示)
		{
			检测悬停选项();
		}
	}
	
	// 创建UI元素
	private void 创建UI元素()
	{
		// 创建根节点
		根节点 = new Control();
		根节点.Name = "根节点";
		AddChild(根节点);
		
		// 创建圆环背景
		圆环背景 = new ColorRect();
		圆环背景.Name = "圆环背景";
		圆环背景.Color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
		圆环背景.Size = new Vector2(圆环半径 * 2.5f, 圆环半径 * 2.5f);
		圆环背景.Position = 屏幕中心 - 圆环背景.Size / 2;
		
		// 创建圆环效果
		var 圆环材质 = new ShaderMaterial();
		var 圆环着色器 = new Shader();
		圆环着色器.Code = @"
			shader_type canvas_item;
			void fragment() {
				vec2 center = vec2(0.5, 0.5);
				float dist = distance(UV, center);
				float outer_radius = 0.48;
				float inner_radius = 0.35;
				if (dist < outer_radius && dist > inner_radius) {
					COLOR = vec4(0.3, 0.3, 0.3, 0.9);
				} else {
					COLOR = vec4(0.0, 0.0, 0.0, 0.0);
				}
			}
		";
		圆环材质.Shader = 圆环着色器;
		圆环背景.Material = 圆环材质;
		
		根节点.AddChild(圆环背景);
		
		// 创建能力选项容器
		能力选项容器 = new Control();
		能力选项容器.Name = "能力选项容器";
		能力选项容器.Position = 屏幕中心;
		根节点.AddChild(能力选项容器);
		
		// 创建能力选项
		创建能力选项("燃烧能力");
		创建能力选项("冻结能力");
	}
	
	// 创建单个能力选项
	private void 创建能力选项(string 能力名称)
	{
		var 选项按钮 = new TextureButton();
		选项按钮.Name = 能力名称 + "选项";
		选项按钮.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
		选项按钮.CustomMinimumSize = new Vector2(选项基础大小, 选项基础大小);
		选项按钮.Size = 选项按钮.CustomMinimumSize;
		
		// 创建占位纹理
		var 图标图像 = Image.Create((int)选项基础大小, (int)选项基础大小, false, Image.Format.Rgba8);
		
		// 根据能力名称设置不同颜色
		Color 图标颜色 = 能力名称 == "燃烧能力" ? new Color(1, 0.3f, 0.1f) : new Color(0.1f, 0.5f, 1);
		图标图像.Fill(图标颜色);
		
		// 绘制圆形图标
		int 中心X = (int)选项基础大小 / 2;
		int 中心Y = (int)选项基础大小 / 2;
		int 半径 = (int)选项基础大小 / 2 - 4;
		
		for (int x = 0; x < 选项基础大小; x++)
		{
			for (int y = 0; y < 选项基础大小; y++)
			{
				float 距离 = new Vector2(x - 中心X, y - 中心Y).Length();
				if (距离 > 半径)
				{
					图标图像.SetPixel(x, y, new Color(0, 0, 0, 0));
				}
			}
		}
		
		var 图标纹理 = ImageTexture.CreateFromImage(图标图像);
		选项按钮.TextureNormal = 图标纹理;
		选项按钮.TextureHover = 图标纹理;
		选项按钮.TexturePressed = 图标纹理;
		选项按钮.TextureDisabled = 图标纹理;
		
		能力选项容器.AddChild(选项按钮);
	}
	
	// 收集所有能力选项
	private void 收集能力选项()
	{
		所有选项.Clear();
		
		foreach (Node 子节点 in 能力选项容器.GetChildren())
		{
			if (子节点 is TextureButton 按钮)
			{
				var 选项 = new 能力选项
				{
					按钮 = 按钮,
					能力名称 = 按钮.Name.ToString().Replace("选项", ""),
					原始尺寸 = 按钮.Scale
				};
				
				// 连接按钮信号
				按钮.MouseEntered += () => 选项悬停进入(选项);
				按钮.MouseExited += () => 选项悬停离开(选项);
				按钮.Pressed += () => 选项被选择(选项);
				
				所有选项.Add(选项);
				选项位置[选项] = Vector2.Zero;
			}
		}
	}
	
	// 排列选项成圆形
	private void 排列选项位置()
	{
		int 选项数量 = 所有选项.Count;
		
		for (int i = 0; i < 选项数量; i++)
		{
			var 选项 = 所有选项[i];
			
			// 计算角度 (从顶部开始，顺时针)
			float 角度 = (float)i / 选项数量 * Mathf.Pi * 2;
			
			// 计算位置
			Vector2 位置 = new Vector2(
				Mathf.Sin(角度) * 圆环半径,
				-Mathf.Cos(角度) * 圆环半径
			);
			
			// 设置位置
			选项.按钮.Position = 位置 - 选项.按钮.Size / 2;
			选项位置[选项] = 位置;
		}
	}
	
	// 显示能力选择UI
	public void 显示()
	{
		if (正在显示 || 正在隐藏) return;
		
		正在显示 = true;
		根节点.Visible = true;
		
		// 重置状态
		当前悬停选项 = null;
		
		// 将鼠标移动到圆环中心
		Input.WarpMouse(屏幕中心);
		
		// 停止之前的动画
		动画补间?.Kill();
		动画补间 = CreateTween();
		
		// 显示动画
		动画补间.TweenProperty(根节点, "modulate", new Color(1, 1, 1, 1), 显示动画时长);
	}
	
	// 隐藏能力选择UI
	public void 隐藏()
	{
		if (!正在显示 || 正在隐藏) return;
		
		正在显示 = false;
		正在隐藏 = true;
		
		// 停止之前的动画
		动画补间?.Kill();
		动画补间 = CreateTween();
		
		// 隐藏动画
		动画补间.TweenProperty(根节点, "modulate", new Color(1, 1, 1, 0), 隐藏动画时长);
		动画补间.TweenCallback(Callable.From(() => {
			根节点.Visible = false;
			正在隐藏 = false;
			
			// 重置所有选项尺寸
			foreach (var 选项 in 所有选项)
			{
				选项.按钮.Scale = 选项.原始尺寸;
			}
		}));
	}
	
	// 检测悬停选项
	private void 检测悬停选项()
	{
		Vector2 鼠标位置 = GetViewport().GetMousePosition();
		Vector2 中心位置 = 屏幕中心;
		Vector2 相对位置 = 鼠标位置 - 中心位置;
		
		// 计算距离
		float 距离 = 相对位置.Length();
		
		// 如果鼠标在圆环外，清除悬停
		if (距离 > 圆环半径 * 1.5f)
		{
			if (当前悬停选项 != null)
			{
				选项悬停离开(当前悬停选项);
				当前悬停选项 = null;
			}
			return;
		}
		
		// 找到最近的选项
		能力选项 最近选项 = null;
		float 最近距离 = float.MaxValue;
		
		foreach (var 选项 in 所有选项)
		{
			Vector2 选项位置世界 = 中心位置 + 选项位置[选项];
			float 选项距离 = 选项位置世界.DistanceTo(鼠标位置);
			
			if (选项距离 < 最近距离)
			{
				最近距离 = 选项距离;
				最近选项 = 选项;
			}
		}
		
		// 更新悬停状态
		if (最近选项 != null && 最近选项 != 当前悬停选项)
		{
			if (当前悬停选项 != null)
			{
				选项悬停离开(当前悬停选项);
			}
			
			当前悬停选项 = 最近选项;
			选项悬停进入(当前悬停选项);
		}
	}
	
	// 选项悬停进入
	private void 选项悬停进入(能力选项 选项)
	{
		// 放大动画
		选项.按钮.Scale = 选项.原始尺寸 * 悬停放大比例;
	}
	
	// 选项悬停离开
	private void 选项悬停离开(能力选项 选项)
	{
		// 恢复原始尺寸
		选项.按钮.Scale = 选项.原始尺寸;
	}
	
	// 选项被选择
	private void 选项被选择(能力选项 选项)
	{
		GD.Print($"选择能力: {选项.能力名称}");
		
		// 如果选择的是已激活的能力，则取消激活
		if (选项 == 当前激活的能力)
		{
			取消激活能力(选项);
		}
		else
		{
			// 先取消当前激活的能力（如果有）
			if (当前激活的能力 != null)
			{
				取消激活能力(当前激活的能力);
			}
			
			// 激活新选择的能力
			激活能力(选项);
		}
		
		// 调用回调函数
		选项.选择回调?.Invoke();
		
		// 新增：触发能力选择完成事件
		触发能力选择对话(选项.能力名称);
		
		// 隐藏UI
		隐藏();
	}
	
	private void 触发能力选择对话(string 能力名称)
	{
		对话序列 使用的对话序列 = 获取能力对话序列(能力名称);
		
		if (使用的对话序列 != null)
		{
			// 标记这个能力已经对话过
			if (!能力已对话记录.ContainsKey(能力名称))
			{
				能力已对话记录[能力名称] = true;
			}
			
			EmitSignal(nameof(能力选择完成), 能力名称, 使用的对话序列);
			GD.Print($"触发能力选择对话: {能力名称} ({(能力已对话记录[能力名称] ? "后续" : "第一次")})");
		}
		else
		{
			GD.Print($"没有为能力 {能力名称} 配置对话序列");
		}
	}
	
	// 新增：根据对话状态获取正确的对话序列
	private 对话序列 获取能力对话序列(string 能力名称)
	{
		bool 已对话过 = 能力已对话记录.ContainsKey(能力名称) && 能力已对话记录[能力名称];
		
		if (!已对话过)
		{
			// 第一次对话
			if (能力第一次对话序列.ContainsKey(能力名称))
			{
				return 能力第一次对话序列[能力名称];
			}
		}
		else
		{
			// 后续对话
			if (能力后续对话序列.ContainsKey(能力名称))
			{
				return 能力后续对话序列[能力名称];
			}
			// 如果没有配置后续对话，回退到第一次对话
			else if (能力第一次对话序列.ContainsKey(能力名称))
			{
				return 能力第一次对话序列[能力名称];
			}
		}
		
		return null;
	}
	
	// 新增：重置能力对话记录
	public void 重置能力对话记录()
	{
		能力已对话记录.Clear();
	}
	
	// 新增：手动设置能力对话状态
	public void 设置能力对话状态(string 能力名称, bool 已对话)
	{
		能力已对话记录[能力名称] = 已对话;
	}
	
	// 新增：检查能力是否已对话过
	public bool 能力是否已对话(string 能力名称)
	{
		return 能力已对话记录.ContainsKey(能力名称) && 能力已对话记录[能力名称];
	}
	
	// 激活能力
	private void 激活能力(能力选项 选项)
	{
		选项.已激活 = true;
		当前激活的能力 = 选项;
		
		// 更新按钮外观 - 添加边框或其他视觉反馈
		var 材质 = new ShaderMaterial();
		var 着色器 = new Shader();
		着色器.Code = @"
			shader_type canvas_item;
			void fragment() {
				vec4 original_color = texture(TEXTURE, UV);
				// 添加发光效果
				if (original_color.a > 0.1) {
					COLOR = vec4(original_color.rgb * 1.5, original_color.a);
				} else {
					COLOR = original_color;
				}
			}
		";
		材质.Shader = 着色器;
		选项.按钮.Material = 材质;
	}
	
	// 取消激活能力
	private void 取消激活能力(能力选项 选项)
	{
		选项.已激活 = false;
		if (当前激活的能力 == 选项)
		{
			当前激活的能力 = null;
		}
		
		// 恢复按钮外观
		选项.按钮.Material = null;
	}
	
	// 注册能力选项回调
	public void 注册能力回调(string 能力名称, Action 回调)
	{
		foreach (var 选项 in 所有选项)
		{
			if (选项.能力名称 == 能力名称)
			{
				选项.选择回调 = 回调;
				return;
			}
		}
	}
	
	// 检查是否正在显示
	public bool 是否正在显示()
	{
		return 正在显示;
	}
	
	// 获取当前激活的能力
	public string 获取当前激活能力()
	{
		return 当前激活的能力?.能力名称;
	}
	
	// 检查特定能力是否激活
	public bool 能力是否激活(string 能力名称)
	{
		foreach (var 选项 in 所有选项)
		{
			if (选项.能力名称 == 能力名称 && 选项.已激活)
			{
				return true;
			}
		}
		return false;
	}
}
