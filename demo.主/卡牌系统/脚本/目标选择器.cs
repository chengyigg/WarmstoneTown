using Godot;
using System.Collections.Generic;

public partial class 目标选择器 : Node2D
{
	private Tween _浮动动画;
	private Vector2 _箭头原始位置;
	[Export] private float 浮动周期 = 0.4f;
	[Export] private PackedScene 箭头预制体;
	[Export] private float 偏移Y = -60f;   // 显示在目标上方的偏移（负数向上），可根据需要调整
	private Node2D 箭头实例;
	private List<(卡牌战斗单位 单位, Node 视觉节点)> 敌人条目 = new();
	private int 当前索引 = -1;

	public void 初始化(List<(卡牌战斗单位, Node)> 敌人视觉列表, Node2D 箭头父节点)
	{
		GD.Print("[目标选择器] 初始化开始");
		敌人条目.Clear();
		foreach (var (单位, 视觉) in 敌人视觉列表)
			敌人条目.Add((单位, 视觉));
		GD.Print($"[目标选择器] 敌人条目数量: {敌人条目.Count}");

		当前索引 = 敌人条目.Count > 0 ? 0 : -1;
		GD.Print($"[目标选择器] 当前索引: {当前索引}");

		if (箭头预制体 == null)
			GD.PrintErr("[目标选择器] 箭头预制体为空！");
		else
			GD.Print("[目标选择器] 箭头预制体存在");

		if (箭头父节点 == null)
			GD.PrintErr("[目标选择器] 箭头父节点为空！");
		else
			GD.Print($"[目标选择器] 箭头父节点: {箭头父节点.Name}");

		if (箭头预制体 != null && 箭头父节点 != null)
		{
			箭头实例 = 箭头预制体.Instantiate<Node2D>();
			if (箭头实例 == null)
			{
				GD.PrintErr("[目标选择器] 箭头实例化失败！");
				return;
			}
			箭头父节点.AddChild(箭头实例);
			箭头实例.Visible = false;
			GD.Print("[目标选择器] 箭头实例已创建并添加，初始不可见");
			if (当前索引 >= 0)
				移动箭头到当前目标();
		}
		else
		{
			GD.PrintErr("目标选择器: 缺少箭头预制体或父节点");
		}
		GD.Print("[目标选择器] 初始化结束");
	}

	public void 切换下一个目标()
	{
		GD.Print("[目标选择器] 切换下一个目标");
		if (敌人条目.Count == 0)
		{
			GD.Print("[目标选择器] 无敌人条目，无法切换");
			return;
		}
		当前索引 = (当前索引 + 1) % 敌人条目.Count;
		GD.Print($"[目标选择器] 切换后当前索引: {当前索引}");
		移动箭头到当前目标();
	}

	private void 移动箭头到当前目标()
	{
		GD.Print("[目标选择器] 移动箭头到当前目标");
		if (当前索引 < 0 || 当前索引 >= 敌人条目.Count)
		{
			GD.PrintErr($"[目标选择器] 无效索引: {当前索引}");
			return;
		}
		if (箭头实例 == null)
		{
			GD.PrintErr("[目标选择器] 箭头实例为空，无法移动");
			return;
		}

		var 视觉 = 敌人条目[当前索引].视觉节点;
		Vector2 目标中心点;

		// 优先查找视觉节点下的 "图片" 子节点（TextureRect）
		TextureRect 图片节点 = null;
		if (视觉 is Control c)
			图片节点 = c.GetNodeOrNull<TextureRect>("图片");
		else if (视觉 is Node2D n)
			图片节点 = n.GetNodeOrNull<TextureRect>("图片");

		if (图片节点 != null)
		{
			目标中心点 = 图片节点.GlobalPosition + 图片节点.Size * 0.5f;
			GD.Print($"[目标选择器] 使用图片节点: {图片节点.Name}, 全局位置: {图片节点.GlobalPosition}, Size: {图片节点.Size}, 中心点: {目标中心点}");
		}
		else
		{
			// 回退到原有逻辑（Control 或 Node2D）
			if (视觉 is Control control)
			{
				目标中心点 = control.GlobalPosition + control.Size * 0.5f;
				GD.Print($"[目标选择器] 回退到 Control，GlobalPosition: {control.GlobalPosition}, Size: {control.Size}, 中心点: {目标中心点}");
			}
			else if (视觉 is Node2D node2D)
			{
				目标中心点 = node2D.GlobalPosition;
				if (node2D is Sprite2D sprite)
				{
					目标中心点 += sprite.Offset;
					GD.Print($"[目标选择器] Sprite2D 类型，Offset: {sprite.Offset}");
				}
				GD.Print($"[目标选择器] 回退到 Node2D，GlobalPosition: {node2D.GlobalPosition}, 中心点: {目标中心点}");
			}
			else
			{
				GD.PrintErr($"[目标选择器] 无法获取目标中心点，不支持的视觉节点类型: {视觉.GetType()}");
				return;
			}
		}

		Vector2 箭头目标位置 = 目标中心点 + new Vector2(0, 偏移Y);
		箭头实例.GlobalPosition = 箭头目标位置;
		GD.Print($"[目标选择器] 箭头目标全局位置: {箭头目标位置}");

		_箭头原始位置 = 箭头实例.Position;
		GD.Print($"[目标选择器] 箭头局部位置（原始）: {_箭头原始位置}");

		if (箭头实例.Visible)
		{
			GD.Print("[目标选择器] 箭头当前可见，重新启动浮动动画");
			停止浮动动画();
			播放浮动动画();
		}
		else
		{
			GD.Print("[目标选择器] 箭头当前不可见，不启动浮动动画（等显示时再启动）");
		}
	}

	public 卡牌战斗单位 获取当前选中的目标()
	{
		if (当前索引 >= 0 && 当前索引 < 敌人条目.Count)
			return 敌人条目[当前索引].单位;
		return null;
	}

	private void 播放浮动动画()
	{
		if (箭头实例 == null)
		{
			GD.PrintErr("[目标选择器] 播放浮动动画失败: 箭头实例为空");
			return;
		}
		GD.Print("[目标选择器] 开始播放浮动动画");
		_浮动动画?.Kill();
		_浮动动画 = CreateTween();
		_浮动动画.SetLoops();
		_浮动动画.TweenProperty(箭头实例, "position:y", _箭头原始位置.Y - 10f, 浮动周期)
				 .SetTrans(Tween.TransitionType.Sine)
				 .SetEase(Tween.EaseType.InOut);
		_浮动动画.TweenProperty(箭头实例, "position:y", _箭头原始位置.Y, 浮动周期)
				 .SetTrans(Tween.TransitionType.Sine)
				 .SetEase(Tween.EaseType.InOut);
	}

	private void 停止浮动动画()
	{
		GD.Print("[目标选择器] 停止浮动动画");
		_浮动动画?.Kill();
		if (箭头实例 != null)
			箭头实例.Position = _箭头原始位置;
	}

	public void 显示(bool 可见)
	{
		GD.Print($"[目标选择器] 显示({可见})被调用");
		if (箭头实例 == null)
		{
			GD.PrintErr("[目标选择器] 箭头实例为空，无法显示/隐藏");
			return;
		}
		箭头实例.Visible = 可见;
		if (可见)
		{
			GD.Print("[目标选择器] 设置箭头可见，获取当前箭头位置并启动浮动动画");
			_箭头原始位置 = 箭头实例.Position;
			播放浮动动画();
			播放浮动动画();
		}
		else
		{
			GD.Print("[目标选择器] 隐藏箭头，停止浮动动画");
			停止浮动动画();
		}
	}

	public void 清空()
	{
		GD.Print("[目标选择器] 清空");
		敌人条目.Clear();
		当前索引 = -1;
		if (箭头实例 != null)
		{
			箭头实例.QueueFree();
			箭头实例 = null;
		}
	}
}
