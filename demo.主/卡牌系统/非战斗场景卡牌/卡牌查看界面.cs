using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class 卡牌查看界面 : Control
{
	[Export] private GridContainer _卡牌容器;
	[Export] private Button _关闭按钮;
	[Export] private PackedScene _卡牌图标预制体;
	[Export] private 生成卡组资源 卡组资源;

	public override void _Ready()
	{
		_关闭按钮.Pressed += () => QueueFree();

		if (_卡牌容器 == null)
		{
			GD.PrintErr("_Ready: _卡牌容器 为 null！");
			return;
		}
		_卡牌容器.Columns = 4;

		if (卡组资源 != null && 卡组资源.卡组 != null)
		{
			显示卡组(卡组资源.卡组);
		}
	}

	public void 显示卡组(IEnumerable<卡牌数据> 卡组)
	{
		if (_卡牌容器 == null)
		{
			GD.PrintErr("严重错误：_卡牌容器 为 null！");
			return;
		}

		foreach (Node child in _卡牌容器.GetChildren())
			child.QueueFree();

		if (卡组 == null) return;

		var 卡牌预制体 = GD.Load<PackedScene>("res://卡牌系统/场景/卡牌UI.tscn");
		if (卡牌预制体 == null)
		{
			GD.PrintErr("卡牌UI预制体加载失败！请检查路径。");
			return;
		}

		foreach (var 卡牌 in 卡组)
		{
			var 卡牌控件 = 卡牌预制体.Instantiate<卡牌UI>();
			卡牌控件.设置为查看模式(卡牌);
			_卡牌容器.AddChild(卡牌控件);
		}
	}
}
