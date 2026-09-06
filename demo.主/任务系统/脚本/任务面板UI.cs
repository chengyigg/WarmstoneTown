using Godot;
using System.Collections.Generic;
using System.Linq;
using 你的项目.Scripts.管理器;

/// <summary>
/// 任务面板UI - 显示所有任务
/// </summary>
public partial class 任务面板UI : CanvasLayer
{
	[Export] private Control 面板容器;
	[Export] private VBoxContainer 任务列表容器;
	[Export] private Button 关闭按钮;
	[Export] private PackedScene 任务卡片预制体;
	[Export] private Label 空状态标签;

	private bool _是否打开 = false;
	private List<任务卡片> _卡片列表 = new List<任务卡片>();

	public override void _Ready()
	{
		// ★ 关键：确保暂停时仍然可以处理输入
		ProcessMode = ProcessModeEnum.Always;

		Layer = 1000;

		if (面板容器 != null)
			面板容器.Visible = false;

		if (空状态标签 != null)
			空状态标签.Visible = false;

		// ★ 检查关闭按钮是否正确连接
		if (关闭按钮 != null)
		{
			GD.Print("[任务面板] 关闭按钮已连接，绑定事件");
			关闭按钮.Pressed += 关闭面板;
		}
		else
		{
			GD.PrintErr("[任务面板] 关闭按钮为空！请检查 Inspector");
		}

		// 连接任务管理器信号
		if (任务管理器.实例 != null)
		{
			任务管理器.实例.任务列表更新 += 刷新面板;
			任务管理器.实例.任务进度更新 += _ => 刷新面板();
		}

		GD.Print("[任务面板] _Ready 完成");
		刷新面板();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("任务面板"))
		{
			GetViewport().SetInputAsHandled();
			切换面板();
		}
	}

	public void 切换面板()
	{
		GD.Print($"[任务面板] 切换面板，当前状态: {_是否打开}");
		_是否打开 = !_是否打开;
		if (_是否打开)
			打开面板();
		else
			关闭面板();
	}

	private void 打开面板()
	{
		if (面板容器 == null)
		{
			GD.PrintErr("[任务面板] 面板容器为空！");
			return;
		}
		面板容器.Visible = true;
		_是否打开 = true;
		刷新面板();
		GetTree().Paused = true;
		GD.Print("[任务面板] 面板已打开");
	}

	private void 关闭面板()
	{
		if (面板容器 == null) return;
		面板容器.Visible = false;
		_是否打开 = false;
		GetTree().Paused = false;
		GD.Print("[任务面板] 面板已关闭");
	}

	private void 刷新面板()
	{
		GD.Print("[任务面板] 刷新面板开始");

		if (任务列表容器 == null)
		{
			GD.PrintErr("[任务面板] 任务列表容器为空！");
			return;
		}

		// 清除旧卡片
		foreach (var 卡片 in _卡片列表)
		{
			if (IsInstanceValid(卡片))
				卡片.QueueFree();
		}
		_卡片列表.Clear();

		var 任务列表 = 任务管理器.实例?.获取所有任务();
		GD.Print($"[任务面板] 获取到 {任务列表?.Count ?? 0} 个任务");

		if (任务列表 == null || 任务列表.Count == 0)
		{
			if (空状态标签 != null)
				空状态标签.Visible = true;
			return;
		}

		if (空状态标签 != null)
			空状态标签.Visible = false;

		var 排序列表 = 任务列表.OrderBy(t => t.是否完成).ToList();

		foreach (var 任务 in 排序列表)
		{
			var 卡片 = 创建任务卡片(任务);
			if (卡片 != null)
			{
				任务列表容器.AddChild(卡片);
				_卡片列表.Add(卡片);
				GD.Print($"[任务面板] 卡片已添加: {任务.任务名称}");
			}
		}

		GD.Print($"[任务面板] 刷新完成，共 {_卡片列表.Count} 张卡片");
	}

	private 任务卡片 创建任务卡片(任务数据 任务)
	{
		if (任务卡片预制体 == null)
		{
			GD.PrintErr("[任务面板] 任务卡片预制体为空！");
			return null;
		}

		var 卡片 = 任务卡片预制体.Instantiate<任务卡片>();
		if (卡片 == null)
		{
			GD.PrintErr("[任务面板] 实例化卡片失败");
			return null;
		}

		// ★ 确保卡片可见
		卡片.Visible = true;
		卡片.绑定任务数据(任务);
		return 卡片;
	}

	public bool 是否打开() => _是否打开;
}
