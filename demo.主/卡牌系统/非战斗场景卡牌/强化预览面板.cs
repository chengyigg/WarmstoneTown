using Godot;
using System;

public partial class 强化预览面板 : Control
{
	[Export] private PackedScene 卡牌UIPrefab;
	[Export] private GridContainer 卡牌容器;
	[Export] private ColorRect 背景;

	private 卡牌UI 左边卡牌;
	private 卡牌UI 右边卡牌;

	public override void _Ready()
	{
		if (背景 != null)
		{
			背景.AnchorLeft = 0;
			背景.AnchorTop = 0;
			背景.AnchorRight = 1;
			背景.AnchorBottom = 1;
	
			背景.MouseFilter = MouseFilterEnum.Stop;
			背景.GuiInput += (InputEvent 事件) =>
			{
				if (事件 is InputEventMouseButton 鼠标事件 && 鼠标事件.Pressed && 鼠标事件.ButtonIndex == MouseButton.Left)
					关闭面板();
			};
		}

		if (卡牌容器 != null)
			卡牌容器.MouseFilter = MouseFilterEnum.Pass;

		ZIndex = 5;
	}

	public void 显示强化预览(卡牌数据 原卡牌数据)
	{
		if (原卡牌数据 == null) return;

		foreach (Node child in 卡牌容器.GetChildren())
			child.QueueFree();

		左边卡牌 = 创建卡牌UI(原卡牌数据);
		右边卡牌 = 创建卡牌UI(创建强化卡牌数据(原卡牌数据));

		if (左边卡牌 != null)
		{
			卡牌容器.AddChild(左边卡牌);
			Vector2 新缩放 = 左边卡牌.获取原始缩放() * 1.5f;
			左边卡牌.Scale = 新缩放;
			左边卡牌.设置原始缩放(新缩放);
		}
		if (右边卡牌 != null)
		{
			卡牌容器.AddChild(右边卡牌);
			Vector2 新缩放 = 右边卡牌.获取原始缩放() * 1.5f;
			右边卡牌.Scale = 新缩放;
			右边卡牌.设置原始缩放(新缩放);
		}

		var 状态栏 = 状态栏界面.获取实例();
		if (状态栏 != null)
		{
			if (左边卡牌 != null)
			{
				左边卡牌.鼠标进入卡牌 += 状态栏.当鼠标进入卡牌;
				左边卡牌.鼠标离开卡牌 += 状态栏.当鼠标离开卡牌;
			}
			if (右边卡牌 != null)
			{
				右边卡牌.鼠标进入卡牌 += 状态栏.当鼠标进入卡牌;
				右边卡牌.鼠标离开卡牌 += 状态栏.当鼠标离开卡牌;
			}
		}

		卡牌容器.Columns = 2;
		卡牌容器.AddThemeConstantOverride("h_separation", 400);
		卡牌容器.AddThemeConstantOverride("v_separation", 50);

		Callable.From(() => 播放摸动画()).CallDeferred();
	}

	private void 播放摸动画()
	{
		if (左边卡牌 != null) 播放单张卡牌摸动画(左边卡牌);
		if (右边卡牌 != null) 播放单张卡牌摸动画(右边卡牌);
	}

	private void 播放单张卡牌摸动画(卡牌UI 卡牌)
	{
		var 原始缩放 = 卡牌.获取原始缩放();
		var tween = 卡牌.CreateTween();
		tween.SetParallel(false);
		tween.TweenProperty(卡牌, "scale", 原始缩放 * 1.2f, 0.1f);
		tween.TweenProperty(卡牌, "scale", 原始缩放, 0.1f);
	}

	private 卡牌UI 创建卡牌UI(卡牌数据 数据)
	{
		if (卡牌UIPrefab == null)
		{
			GD.PrintErr("强化预览面板: 卡牌UIPrefab 为空");
			return null;
		}
		var 卡牌 = 卡牌UIPrefab.Instantiate<卡牌UI>();
		if (卡牌 == null)
		{
			GD.PrintErr("强化预览面板: 实例化卡牌UI失败");
			return null;
		}
		卡牌.设置为查看模式(数据);
		return 卡牌;
	}

	private 卡牌数据 创建强化卡牌数据(卡牌数据 原数据)
	{
		return new 卡牌数据
		{
			卡牌名称 = 原数据.卡牌名称,
			卡牌描述 = string.IsNullOrEmpty(原数据.强化描述) ? 原数据.卡牌描述 : 原数据.强化描述,
			基础伤害 = 原数据.基础伤害,

			类型 = 原数据.类型,
			卡面贴图 = 原数据.卡面贴图,
			卡牌图标 = 原数据.卡牌图标,
			卡背贴图 = 原数据.卡背贴图,
			强化描述 = 原数据.强化描述,
		};
	}

	private void 关闭面板()
	{
		var 状态栏 = 状态栏界面.获取实例();
		if (状态栏 != null)
		{
			左边卡牌.鼠标进入卡牌 -= 状态栏.当鼠标进入卡牌;
			左边卡牌.鼠标离开卡牌 -= 状态栏.当鼠标离开卡牌;
			右边卡牌.鼠标进入卡牌 -= 状态栏.当鼠标进入卡牌;
			右边卡牌.鼠标离开卡牌 -= 状态栏.当鼠标离开卡牌;
		}
		QueueFree();
	}
}
