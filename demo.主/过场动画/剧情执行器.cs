using Godot;
using System.Threading.Tasks;
using 你的项目.Scripts.资源;
using 你的项目.Scripts.管理器;
using 你的项目.Scripts.摄像机;
using 你的项目.Scripts.角色;

public partial class 剧情执行器 : Node
{
	public static 剧情执行器 实例 { get; private set; }

	private 清理上下文 _待清理上下文 = null;

	private class 清理上下文
	{
		public Node2D 替身节点;
		public AnimationPlayer 动画器;
		public 玩家控制器 真实玩家;
		public bool 是替身模式;
		public bool 强制空闲;
		public NodePath 一次性动画节点路径;
	}

	public override void _Ready()
	{
		实例 = this;
		if (转场管理器.实例 != null) 转场管理器.实例.转场完成 += OnSceneChanged;
	}

private async void OnSceneChanged(string 新场景路径)
{
	await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	var 剧情 = 转场管理器.实例?.取出待播放剧情();
	if (剧情 == null) return;

	玩家控制器 玩家 = GetTree().GetFirstNodeInGroup("玩家") as 玩家控制器;
	if (玩家 != null)
	{
		玩家.强制终止移动并重置();
		玩家.开始过场动画();
	}

	if (剧情.摄像头动画配置 != null) await 播放摄像头动画(剧情.摄像头动画配置);
	bool 有动画 = await 播放替身动画_仅播放(剧情);

	if (剧情.对话序列 != null && 对话播放器.实例 != null)
	{
		对话播放器.实例.开始对话(剧情.对话序列);
		await ToSignal(对话播放器.实例, 对话播放器.SignalName.对话结束);
	}

	if (有动画 && _待清理上下文 != null) 清理替身动画();

// ========== 记录剧情触发（退化用） ==========
if (剧情.启用退化)
{
	// ★ 改为使用当前状态，不再依赖存档位
	var 当前状态 = 存档管理器.实例?.获取当前状态();
	if (当前状态 != null)
	{
		GD.Print($"[剧情执行器] 准备记录剧情: {剧情.ResourcePath}");
		当前状态.记录剧情触发(剧情.ResourcePath, true);
		GD.Print($"[剧情执行器] 记录后状态: {当前状态.获取剧情触发状态(剧情.ResourcePath)}");
	}
	else
	{
		GD.PrintErr($"[剧情执行器] 无法获取当前状态，无法记录剧情触发");
	}

	// ★ 立即设置条件管理器，使退化立即生效
	string 条件名 = $"剧情_{剧情.ResourcePath.GetFile().GetBaseName()}";
	if (条件管理器.实例 != null)
	{
		条件管理器.实例.设置条件满足(条件名);
		GD.Print($"[剧情执行器] 已设置条件管理器条件: {条件名} = true");
	}
	else
	{
		GD.PrintErr($"[剧情执行器] 条件管理器实例不存在，无法设置退化条件");
	}
}

	if (玩家 != null && IsInstanceValid(玩家)) 玩家.结束过场动画();
}

	private async Task 播放摄像头动画(摄像头动画资源 配置)
	{
		if (转场管理器.实例 != null) await 转场管理器.实例.播放摄像头动画资源(配置);
	}

	private async Task<bool> 播放替身动画_仅播放(剧情序列资源 剧情)
	{
		替身动画条目[] 动画列表 = 剧情.动画列表;
		string 单个动画名 = 剧情.替身动画名称;
		NodePath 节点路径 = 剧情.替身节点路径;
		bool 是替身模式 = 剧情.是否为替身动画;
		bool 强制空闲 = 剧情.动画结束后强制空闲;

		if ((动画列表 == null || 动画列表.Length == 0) && !string.IsNullOrEmpty(单个动画名))
			动画列表 = new 替身动画条目[] { new 替身动画条目 { 动画名称 = 单个动画名, 播放前延迟 = 0f, 等待动画结束 = true } };
		if (动画列表 == null || 动画列表.Length == 0) return false;

		var 替身 = GetTree().CurrentScene.GetNodeOrNull<Node2D>(节点路径);
		if (替身 == null) { GD.PrintErr($"找不到替身节点: {节点路径}"); return false; }
		var 动画器 = 替身.GetNode<AnimationPlayer>("AnimationPlayer");
		if (动画器 == null) { GD.PrintErr($"替身节点 {替身.Name} 下没有 AnimationPlayer"); return false; }

		NodePath 一次性路径 = 获取一次性动画节点路径(剧情);
		if (一次性路径 != null) 一次性动画管理器.实例?.注册一次性动画(一次性路径);

		_待清理上下文 = new 清理上下文
		{
			替身节点 = 替身,
			动画器 = 动画器,
			真实玩家 = null,
			是替身模式 = 是替身模式,
			强制空闲 = 强制空闲,
			一次性动画节点路径 = 一次性路径
		};

		玩家控制器 真实玩家 = null;
		if (是替身模式)
		{
			真实玩家 = GetTree().GetFirstNodeInGroup("玩家") as 玩家控制器;
			if (真实玩家 != null)
			{
				真实玩家.开始过场动画();
				真实玩家.Visible = false;
			}
			替身.GlobalPosition = 真实玩家?.GlobalPosition ?? Vector2.Zero;
			_待清理上下文.真实玩家 = 真实玩家;
		}
		替身.Visible = true;

		for (int i = 0; i < 动画列表.Length; i++)
		{
			var 条目 = 动画列表[i];
			if (string.IsNullOrEmpty(条目.动画名称)) continue;
			if (!动画器.HasAnimation(条目.动画名称)) { GD.PrintErr($"AnimationPlayer 中没有动画: {条目.动画名称}"); continue; }
			if (条目.播放前延迟 > 0f) await ToSignal(GetTree().CreateTimer(条目.播放前延迟), "timeout");
			动画器.Play(条目.动画名称);
			if (条目.等待动画结束) await ToSignal(动画器, AnimationPlayer.SignalName.AnimationFinished);
		}
		return true;
	}

	private NodePath 获取一次性动画节点路径(剧情序列资源 剧情)
	{
		if (剧情.一次性动画节点路径 != null && !剧情.一次性动画节点路径.IsEmpty) return 剧情.一次性动画节点路径;
		if (剧情.是否为一次性动画) return 剧情.替身节点路径;
		return null;
	}

	private void 清理替身动画()
	{
		if (_待清理上下文 == null) return;
		var ctx = _待清理上下文;
		if (ctx.是替身模式 && ctx.真实玩家 != null)
		{
			ctx.真实玩家.GlobalPosition = ctx.替身节点.GlobalPosition;
			ctx.真实玩家.结束过场动画(ctx.强制空闲);
			ctx.真实玩家.Visible = true;
		}
		if (IsInstanceValid(ctx.替身节点)) ctx.替身节点.Visible = false;
		if (ctx.一次性动画节点路径 != null && !ctx.一次性动画节点路径.IsEmpty)
			一次性动画管理器.实例?.标记动画已触发(ctx.一次性动画节点路径);
		_待清理上下文 = null;
	}
}
