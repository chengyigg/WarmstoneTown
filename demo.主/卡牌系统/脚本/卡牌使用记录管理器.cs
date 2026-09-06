// CardUsageHistoryManager.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class 卡牌使用记录管理器 : Control
{
	[Signal] public delegate void 记录已更新EventHandler();
	
	[Export] private Control 所有卡牌记录容器;
	[Export] private Control 装备记录容器;
	[Export] private PackedScene 记录图标Prefab;
	[Export] private PackedScene 卡牌详情窗口Prefab;
	[Export] private int 最大显示数量 = 6;
	[Export] private PackedScene 卡牌UIPrefab;
	
	private List<卡牌使用记录> 所有卡牌记录列表 = new List<卡牌使用记录>();
	private List<卡牌使用记录> 装备记录列表 = new List<卡牌使用记录>();
	private Control 当前详情窗口 = null;
	private 卡牌使用记录 当前按下的记录 = null;
	
	
	public override void _Ready()
	{
		if (所有卡牌记录容器 == null)
			GD.PrintErr("所有卡牌记录容器为空");
		if (装备记录容器 == null)
			GD.PrintErr("装备记录容器为空");
		if (记录图标Prefab == null)
			GD.PrintErr("记录图标Prefab为空");
	}
	
	public void 添加记录(卡牌实例 卡牌, bool 是玩家使用的)
	{
		if (卡牌?.基础数据 == null)
		{
			GD.PrintErr("无法添加记录：卡牌数据为空");
			return;
		}
		
		var 记录 = new 卡牌使用记录(卡牌, 是玩家使用的);
		所有卡牌记录列表.Add(记录);
		
		if (卡牌.基础数据.类型 == 卡牌数据.卡牌类型.装备)
		{
			装备记录列表.Add(记录);
		}
		
		限制记录数量();
		更新UI显示();
		EmitSignal(SignalName.记录已更新);
	}
	
	private void 限制记录数量()
	{
		if (所有卡牌记录列表.Count > 最大显示数量)
		{
			所有卡牌记录列表 = 所有卡牌记录列表
				.OrderByDescending(r => r.使用时间)
				.Take(最大显示数量)
				.ToList();
		}
		
		if (装备记录列表.Count > 最大显示数量)
		{
			装备记录列表 = 装备记录列表
				.OrderByDescending(r => r.使用时间)
				.Take(最大显示数量)
				.ToList();
		}
	}
	
	private void 更新UI显示()
	{
		清空容器UI(所有卡牌记录容器);
		清空容器UI(装备记录容器);
		
		for (int i = 0; i < 所有卡牌记录列表.Count; i++)
			创建记录图标(所有卡牌记录列表[i], 所有卡牌记录容器, i);
		
		for (int i = 0; i < 装备记录列表.Count; i++)
			创建记录图标(装备记录列表[i], 装备记录容器, i);
	}
	
	private void 创建记录图标(卡牌使用记录 记录, Control 容器, int 索引)
	{
		if (记录图标Prefab == null) return;
		
		var 图标实例 = 记录图标Prefab.Instantiate<TextureButton>();
		容器.AddChild(图标实例);
		
		图标实例.Scale = new Vector2(0.2f, 0.2f);
		float 图标间距 = 30f;
		图标实例.Position = new Vector2(索引 * 图标间距, 0);
		图标实例.Modulate = Colors.White;
		
		if (记录.卡牌.基础数据.卡牌图标 != null)
			图标实例.TextureNormal = 记录.卡牌.基础数据.卡牌图标;
		else if (记录.卡牌.基础数据.卡面贴图 != null)
			图标实例.TextureNormal = 记录.卡牌.基础数据.卡面贴图;
		
		Color 图标颜色 = 记录.获取显示颜色();
		图标实例.Modulate = 图标颜色;
		
		图标实例.MouseEntered += () => {
			图标实例.Scale = new Vector2(0.25f, 0.25f);
			图标实例.Modulate = 图标颜色.Lightened(0.3f);
		};
		图标实例.MouseExited += () => {
			图标实例.Scale = new Vector2(0.2f, 0.2f);
			图标实例.Modulate = 图标颜色;
		};
		图标实例.ButtonDown += () => {
			当前按下的记录 = 记录;
			显示卡牌详情(记录);
		};
		图标实例.ButtonUp += () => {
			if (当前详情窗口 != null && IsInstanceValid(当前详情窗口))
			{
				当前详情窗口.QueueFree();
				当前详情窗口 = null;
			}
			当前按下的记录 = null;
		};
		图标实例.TooltipText = $"{记录.卡牌.基础数据.卡牌名称}\n{(记录.是玩家使用的 ? "玩家" : "敌人")}使用";
	}
	
	private void 设置图标显示(Control 图标实例, 卡牌使用记录 记录)
	{
		图标实例.Scale = new Vector2(0.2f, 0.2f);
		var 纹理矩形 = 图标实例 as TextureRect;
		if (纹理矩形 != null)
		{
			if (记录.卡牌.基础数据.卡牌图标 != null)
				纹理矩形.Texture = 记录.卡牌.基础数据.卡牌图标;
			else if (记录.卡牌.基础数据.卡面贴图 != null)
				纹理矩形.Texture = 记录.卡牌.基础数据.卡面贴图;
			纹理矩形.Modulate = 记录.获取显示颜色();
		}
		var 纹理按钮 = 图标实例 as TextureButton;
		if (纹理按钮 != null)
		{
			if (记录.卡牌.基础数据.卡牌图标 != null)
				纹理按钮.TextureNormal = 记录.卡牌.基础数据.卡牌图标;
			纹理按钮.Modulate = 记录.获取显示颜色();
		}
	}
	
	private void 添加点击事件(Control 图标实例, 卡牌使用记录 记录)
	{
		图标实例.MouseEntered += () => 图标实例.Scale = new Vector2(1.1f, 1.1f);
		图标实例.MouseExited += () => 图标实例.Scale = new Vector2(1.0f, 1.0f);
		if (图标实例 is TextureButton 按钮)
			按钮.Pressed += () => 显示卡牌详情(记录);
		else
		{
			图标实例.GuiInput += (输入事件) => {
				if (输入事件 is InputEventMouseButton 鼠标事件 && 
					鼠标事件.ButtonIndex == MouseButton.Left && 
					鼠标事件.Pressed)
					显示卡牌详情(记录);
			};
		}
	}
	
	public void 显示卡牌详情(卡牌使用记录 记录)
	{
		if (当前详情窗口 != null && IsInstanceValid(当前详情窗口))
			当前详情窗口.QueueFree();
		
		var 背景 = new ColorRect();
		背景.Name = "详情背景";
		背景.Color = new Color(0, 0, 0, 0.7f);
		背景.Size = GetViewportRect().Size;
		背景.ZIndex = 999;
		
		背景.GuiInput += (输入事件) => {
			if (输入事件 is InputEventMouseButton 鼠标事件 && 
				鼠标事件.ButtonIndex == MouseButton.Left && 
				鼠标事件.Pressed)
			{
				背景.QueueFree();
				当前详情窗口 = null;
			}
		};
		
		if (卡牌UIPrefab != null)
		{
			var 卡牌UI = 卡牌UIPrefab.Instantiate<卡牌UI>();
			背景.AddChild(卡牌UI);
			卡牌UI.初始化(记录.卡牌);
			var 原始尺寸 = 卡牌UI.Size;
			卡牌UI.Scale = new Vector2(0.5f, 0.5f);
			var 缩小后尺寸 = 原始尺寸 * 卡牌UI.Scale;
			var 屏幕大小 = GetViewportRect().Size;
			var 中心位置 = (屏幕大小 - 缩小后尺寸) / 2;
			var 偏移位置 = 中心位置 + new Vector2(-100, -200);
			卡牌UI.Position = 偏移位置;
			卡牌UI.MouseFilter = MouseFilterEnum.Ignore;
			设置卡牌UI边框颜色(卡牌UI, 记录);
			添加使用者标签(背景, 记录);
		}
		else
		{
			GD.PrintErr("卡牌UIPrefab为空，无法显示详情");
			创建简单详情窗口(记录);
		}
		
		GetTree().Root.AddChild(背景);
		当前详情窗口 = 背景;
	}
	
	private void 设置详情窗口内容(Control 详情窗口, 卡牌使用记录 记录)
	{
		var 卡面贴图 = 详情窗口.GetNodeOrNull<TextureRect>("卡面贴图");
		if (卡面贴图 != null && 记录.卡牌.基础数据.卡面贴图 != null)
			卡面贴图.Texture = 记录.卡牌.基础数据.卡面贴图;
		var 卡牌名称 = 详情窗口.GetNodeOrNull<Label>("卡牌名称");
		if (卡牌名称 != null)
			卡牌名称.Text = 记录.卡牌.基础数据.卡牌名称;
		var 使用者标签 = 详情窗口.GetNodeOrNull<Label>("使用者");
		if (使用者标签 != null)
		{
			使用者标签.Text = 记录.是玩家使用的 ? "玩家使用" : "敌人使用";
			使用者标签.Modulate = 记录.获取显示颜色();
		}
		详情窗口.GuiInput += (输入事件) => {
			if (输入事件 is InputEventMouseButton 鼠标事件 && 
				鼠标事件.ButtonIndex == MouseButton.Left && 
				鼠标事件.Pressed)
			{
				详情窗口.QueueFree();
				当前详情窗口 = null;
			}
		};
	}
	
	private void 设置卡牌UI边框颜色(卡牌UI 卡牌UI, 卡牌使用记录 记录)
	{
		var 边框 = new Panel();
		边框.Size = 卡牌UI.Size + new Vector2(10, 10);
		边框.Position = 卡牌UI.Position - new Vector2(5, 5);
		边框.ZIndex = 卡牌UI.ZIndex - 1;
		var 边框样式 = new StyleBoxFlat();
		边框样式.BgColor = new Color(0, 0, 0, 0);
		边框样式.BorderColor = 记录.获取显示颜色();
		边框样式.BorderWidthLeft = 3;
		边框样式.BorderWidthTop = 3;
		边框样式.BorderWidthRight = 3;
		边框样式.BorderWidthBottom = 3;
		边框.AddThemeStyleboxOverride("panel", 边框样式);
		卡牌UI.GetParent().AddChild(边框);
	}
	
	private void 添加使用者标签(Control 背景, 卡牌使用记录 记录)
	{
		var 使用者标签 = new Label();
		使用者标签.Text = 记录.是玩家使用的 ? "玩家使用" : "敌人使用";
		使用者标签.AddThemeColorOverride("font_color", 记录.获取显示颜色());
		使用者标签.AddThemeFontSizeOverride("font_size", 24);
		使用者标签.Position = new Vector2(20, 20);
		背景.AddChild(使用者标签);
	}
	
	private void 创建简单详情窗口(卡牌使用记录 记录)
	{
		var 背景 = new ColorRect();
		背景.Color = new Color(0, 0, 0, 0.7f);
		背景.Size = GetViewportRect().Size;
		背景.ZIndex = 999;
		
		var 卡牌显示 = new Control();
		卡牌显示.Size = new Vector2(300, 450);
		卡牌显示.Position = (GetViewportRect().Size - 卡牌显示.Size) / 2;
		卡牌显示.ZIndex = 1000;
		
		var 卡牌背景 = new Panel();
		卡牌背景.Size = 卡牌显示.Size;
		var 背景样式 = new StyleBoxFlat();
		背景样式.BgColor = new Color(0.1f, 0.1f, 0.1f);
		背景样式.BorderColor = 记录.获取显示颜色();
		背景样式.BorderWidthLeft = 4;
		背景样式.BorderWidthTop = 4;
		背景样式.BorderWidthRight = 4;
		背景样式.BorderWidthBottom = 4;
		卡牌背景.AddThemeStyleboxOverride("panel", 背景样式);
		
		var 卡面贴图 = new TextureRect();
		卡面贴图.Size = new Vector2(260, 360);
		卡面贴图.Position = new Vector2(20, 20);
		卡面贴图.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		卡面贴图.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		if (记录.卡牌.基础数据.卡面贴图 != null)
			卡面贴图.Texture = 记录.卡牌.基础数据.卡面贴图;
		
		var 卡牌名称 = new Label();
		卡牌名称.Text = 记录.卡牌.基础数据.卡牌名称;
		卡牌名称.Position = new Vector2(20, 390);
		卡牌名称.AddThemeColorOverride("font_color", new Color(1, 1, 1));
		卡牌名称.AddThemeFontSizeOverride("font_size", 24);
		
		var 使用者 = new Label();
		使用者.Text = 记录.是玩家使用的 ? "玩家使用" : "敌人使用";
		使用者.Position = new Vector2(20, 420);
		使用者.AddThemeColorOverride("font_color", 记录.获取显示颜色());
		使用者.AddThemeFontSizeOverride("font_size", 18);
		
		卡牌显示.AddChild(卡牌背景);
		卡牌显示.AddChild(卡面贴图);
		卡牌显示.AddChild(卡牌名称);
		卡牌显示.AddChild(使用者);
		背景.AddChild(卡牌显示);
		
		GetTree().Root.AddChild(背景);
		当前详情窗口 = 背景;
		
		背景.GuiInput += (输入事件) => {
			if (输入事件 is InputEventMouseButton 鼠标事件 && 
				鼠标事件.ButtonIndex == MouseButton.Left && 
				鼠标事件.Pressed)
			{
				背景.QueueFree();
				当前详情窗口 = null;
			}
		};
	}
	
	private void 清空容器UI(Control 容器)
	{
		if (容器 == null) return;
		foreach (Node 子节点 in 容器.GetChildren())
		{
			if (子节点 is Control 控件)
				控件.QueueFree();
		}
	}
	
	public void 清空记录()
	{
		所有卡牌记录列表.Clear();
		装备记录列表.Clear();
		更新UI显示();
	}
	
	public int 获取所有记录数量() => 所有卡牌记录列表.Count;
	public int 获取装备记录数量() => 装备记录列表.Count;
}
