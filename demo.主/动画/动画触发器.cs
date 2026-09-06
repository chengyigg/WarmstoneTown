using Godot;
using System.Collections.Generic;
using 你的项目.Scripts.角色;
using 你的项目.Scripts.摄像机;
using 你的项目.Scripts.资源;
using 你的项目.Scripts.管理器;

public partial class 动画触发器 : Area2D
{
	[Export] public string 动画名称 = "walk_left_cutscene";
	[Export] public bool 只触发一次 = true;
	[Export] public bool 触发后禁用 = true;
	[Export] public bool 动画结束后强制空闲 = true;
	[Export] public Node2D 动画替身;
	[Export] public bool 替身初始不可见 = true;
	[Export] public 对话序列 动画结束后对话序列 { get; set; }

	private bool _初始化完成 = false;
	private string _唯一标识;
	private bool 已触发 = false;
	private 玩家控制器 真实玩家;
	private AnimationPlayer 替身动画器;
	private 玩家摄像头 主摄像头;

	private Node2D 原始摄像头目标;
	private float 原始跟随速度;
	private bool 原始边界状态;
	private bool 原始物理处理状态;

	private List<(Camera2D camera, bool enabled)> 临时禁用的摄像头 = new();

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;

		if (动画替身 == null)
		{
			GD.PrintErr("动画触发器: 未设置动画替身节点");
			return;
		}

		替身动画器 = 动画替身.GetNode<AnimationPlayer>("AnimationPlayer");
		if (替身动画器 == null)
		{
			GD.PrintErr("动画触发器: 动画替身节点下没有 AnimationPlayer");
			return;
		}

		if (替身初始不可见)
			动画替身.Visible = false;

		// 使用节点名称生成唯一标识（确保场景内唯一）
		_唯一标识 = GetTree().CurrentScene.SceneFilePath + "|" + Name;

		// 从当前状态读取触发记录
		if (只触发一次)
		{
			var 当前状态 = 存档管理器.实例?.获取当前状态();
			if (当前状态 != null)
			{
				bool 已记录 = 当前状态.获取动画触发状态(_唯一标识);
				GD.Print($"[动画触发器] 读取当前状态触发记录：{已记录}，键={_唯一标识}");
				if (已记录)
				{
					已触发 = true;
					Monitoring = false;
				}
			}
			else
			{
				GD.PrintErr("[动画触发器] 无法获取当前状态，将保持激活");
			}
		}
		_初始化完成 = true;
	}

	public override void _Process(double delta)
	{
		if (!_初始化完成) return;

		if (替身动画器 != null && 替身动画器.IsPlaying() && 主摄像头 != null && IsInstanceValid(主摄像头))
		{
			主摄像头.GlobalPosition = 动画替身.GlobalPosition;
		}
	}

	private async void OnBodyEntered(Node2D body)
	{
		if (游戏管理器.实例 != null && !游戏管理器.实例.允许交互)
		{
			GD.Print("[动画触发器] 禁止交互，忽略触发");
			return;
		}

		if (!body.IsInGroup("玩家")) return;
		if (已触发 && 只触发一次) return;

		if (!替身动画器.HasAnimation(动画名称))
		{
			GD.PrintErr($"动画 '{动画名称}' 不存在");
			return;
		}

		真实玩家 = body as 玩家控制器;
		if (真实玩家 == null) { GD.PrintErr("没有玩家控制器"); return; }

		主摄像头 = GetTree().GetFirstNodeInGroup("摄像头") as 玩家摄像头;
		if (主摄像头 == null)
			主摄像头 = GetTree().Root.FindChild("玩家摄像头", true, false) as 玩家摄像头;
		if (主摄像头 == null) { GD.PrintErr("找不到主摄像头"); return; }

		原始摄像头目标 = 主摄像头.目标玩家;
		原始跟随速度 = 主摄像头.最大跟随速度;
		原始边界状态 = 主摄像头.启用边界限制;
		原始物理处理状态 = 主摄像头.IsPhysicsProcessing();

		GD.Print($"[动画触发器] OnBodyEntered: 原始目标={原始摄像头目标?.Name}, 原始速度={原始跟随速度}, 原始边界={原始边界状态}, 物理处理={原始物理处理状态}");

		// 标记触发并保存到当前状态
		if (只触发一次)
		{
			已触发 = true;
			var 当前状态 = 存档管理器.实例?.获取当前状态();
			if (当前状态 != null)
			{
				当前状态.记录动画触发(_唯一标识, true);
				GD.Print($"[动画触发器] 已记录触发状态到当前状态，键={_唯一标识}");
			}
			else
			{
				GD.PrintErr("[动画触发器] 无法获取当前状态，记录失败");
			}

			if (触发后禁用)
				Callable.From(() => Monitoring = false).CallDeferred();
		}

		真实玩家.开始过场动画();
		真实玩家.Visible = false;

		动画替身.GlobalPosition = 真实玩家.GlobalPosition;
		动画替身.Visible = true;

		GD.Print($"[动画触发器] 设置替身位置: {动画替身.GlobalPosition}");

		禁用其他摄像头();

		主摄像头.Enabled = true;
		主摄像头.SetPhysicsProcess(false);
		GD.Print("[动画触发器] 已禁用主摄像头物理处理");

		主摄像头.设置目标(动画替身);
		GD.Print($"[动画触发器] 设置目标后，主摄像头目标玩家={主摄像头.目标玩家?.Name}");
		主摄像头.最大跟随速度 = 999999f;
		主摄像头.启用边界限制 = false;
		主摄像头.立即跳转到目标();
		GD.Print($"[动画触发器] 立即跳转后，主摄像头位置={主摄像头.GlobalPosition}");
		主摄像头.MakeCurrent();
		GD.Print($"[动画触发器] MakeCurrent 后，IsCurrent={主摄像头.IsCurrent()}");

		替身动画器.Seek(0);
		替身动画器.AnimationFinished += On替身动画结束;
		替身动画器.Play(动画名称);
		GD.Print($"[动画触发器] 开始播放动画: {动画名称}");
	}

	private void 禁用其他摄像头()
	{
		临时禁用的摄像头.Clear();
		var 所有摄像头 = new List<Camera2D>();
		收集所有摄像头(GetTree().Root, 所有摄像头);

		ulong 主摄像头ID = 主摄像头.GetInstanceId();
		GD.Print($"[调试] 主摄像头ID: {主摄像头ID}, 名称: {主摄像头.Name}");

		foreach (var 相机 in 所有摄像头)
		{
			if (相机.GetInstanceId() == 主摄像头ID)
			{
				GD.Print("[调试] 跳过主摄像头");
				continue;
			}
			临时禁用的摄像头.Add((相机, 相机.Enabled));
			相机.Enabled = false;
			GD.Print($"[动画触发器] 禁用摄像头: {相机.Name}, 原启用={相机.Enabled}");
		}
	}

	private void 收集所有摄像头(Node 节点, List<Camera2D> 列表)
	{
		if (节点 is Camera2D 相机) 列表.Add(相机);
		foreach (Node 子 in 节点.GetChildren())
			收集所有摄像头(子, 列表);
	}

	private void 恢复所有摄像头()
	{
		foreach (var (相机, 原始状态) in 临时禁用的摄像头)
		{
			if (IsInstanceValid(相机))
			{
				相机.Enabled = 原始状态;
				GD.Print($"[动画触发器] 恢复摄像头: {相机.Name}, 启用={相机.Enabled}");
			}
		}
		临时禁用的摄像头.Clear();
	}

	private void On替身动画结束(StringName animName)
	{
		if (animName != 动画名称) return;
		if (替身动画器.IsPlaying()) return;

		GD.Print($"[动画触发器] 动画结束: {animName}");
		替身动画器.AnimationFinished -= On替身动画结束;

		Vector2 动画结束点 = 动画替身.GlobalPosition;
		卡牌数据管理器.上下文.动画结束位置 = 动画结束点;
		卡牌数据管理器.上下文.使用动画结束位置 = true;

		if (动画结束后对话序列 != null && 对话播放器.实例 != null)
		{
			对话播放器.实例.开始对话(动画结束后对话序列);
			对话播放器.实例.对话结束 += On动画后对话结束;
			return;
		}

		执行恢复();
	}

	private void On动画后对话结束()
	{
		if (对话播放器.实例 != null)
			对话播放器.实例.对话结束 -= On动画后对话结束;
		执行恢复();
	}

	private async void 执行恢复()
	{
		GD.Print($"[动画触发器] 执行恢复开始");

		真实玩家.GlobalPosition = 动画替身.GlobalPosition;
		真实玩家.最后位置 = 动画替身.GlobalPosition;

		主摄像头.设置目标(真实玩家);
		主摄像头.最大跟随速度 = 999999f;
		主摄像头.启用边界限制 = false;
		主摄像头.立即跳转到目标();
		主摄像头.MakeCurrent();

		恢复所有摄像头();

		await ToSignal(GetTree(), "process_frame");

		主摄像头.SetPhysicsProcess(true);
		主摄像头.最大跟随速度 = 原始跟随速度;
		主摄像头.启用边界限制 = 原始边界状态;
		主摄像头.立即跳转到目标();

		真实玩家.结束过场动画(动画结束后强制空闲);
		真实玩家.Visible = true;
		动画替身.Visible = false;
		GD.Print("[动画触发器] 恢复玩家控制完成");
	}

	public void 重新启用()
	{
		Monitoring = true;
		已触发 = false;
	}
}
