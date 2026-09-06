using Godot;
using System;

public partial class 全局背景音乐管理器 : Node
{
	public static 全局背景音乐管理器 实例 { get; private set; }

	[Export] public 场景音乐绑定集合 音乐绑定表;

	// ★ 音效音量映射范围（可在检查器调节）
	[Export] private float 音效最小分贝 = -30f;
	[Export] private float 音效最大分贝 = 20f;

	private AudioStreamPlayer 音乐播放器;
	private Tween 当前动画;
	private float 当前背景音乐音量 = 0.5f;

	private AudioStreamPlayer 音效播放器;
	private float 当前音效音量 = 1.0f;

	public override void _Ready()
	{
		if (实例 == null)
		{
			实例 = this;

			音乐播放器 = new AudioStreamPlayer();
			AddChild(音乐播放器);
			音乐播放器.Name = "背景音乐播放器";
			更新背景音乐音量();

			音效播放器 = new AudioStreamPlayer();
			AddChild(音效播放器);
			音效播放器.Name = "全局音效播放器";
			更新音效音量();

			GD.Print("✅ 全局背景音乐管理器（含音效）已初始化");
		}
		else
		{
			QueueFree();
		}
	}

	// ---------- 背景音乐 ----------
	public void 根据场景播放音乐(string 场景路径)
	{
		if (音乐绑定表 == null)
		{
			GD.PrintErr("❌ 音乐绑定表未设置！");
			return;
		}
		foreach (var 条目 in 音乐绑定表.绑定列表)
		{
			if (条目 != null && 条目.场景路径 == 场景路径)
			{
				播放音乐(条目.音乐, 1.0f);
				return;
			}
		}
		GD.Print($"⚠️ 未找到场景 [{场景路径}] 对应的音乐绑定");
	}

	public void 播放音乐(AudioStream 音乐流, float 淡入淡出时间 = 1.0f)
	{
		if (音乐播放器 == null || 音乐流 == null) return;
		if (音乐播放器.Playing && 音乐播放器.Stream == 音乐流) return;

		当前动画?.Kill();

		if (音乐播放器.Playing)
		{
			当前动画 = CreateTween();
			当前动画.TweenProperty(音乐播放器, "volume_db", -80, 淡入淡出时间);
			当前动画.TweenCallback(Callable.From(() =>
			{
				音乐播放器.Stream = 音乐流;
				音乐播放器.Play();
			}));
			当前动画.TweenProperty(音乐播放器, "volume_db", Mathf.LinearToDb(当前背景音乐音量), 淡入淡出时间);
		}
		else
		{
			音乐播放器.Stream = 音乐流;
			音乐播放器.Play();
			音乐播放器.VolumeDb = Mathf.LinearToDb(当前背景音乐音量);
		}
		GD.Print($"🎵 播放背景音乐: {音乐流.ResourcePath}");
	}

	public void 停止音乐(float 淡出时间 = 0.5f)
	{
		if (音乐播放器 == null) return;
		当前动画?.Kill();
		当前动画 = CreateTween();
		当前动画.TweenProperty(音乐播放器, "volume_db", -80, 淡出时间);
		当前动画.TweenCallback(Callable.From(() => 音乐播放器.Stop()));
	}

	public void 设置背景音乐音量(float 线性音量)
	{
		当前背景音乐音量 = Mathf.Clamp(线性音量, 0f, 1f);
		更新背景音乐音量();
	}

	public float 获取背景音乐音量() => 当前背景音乐音量;

	private void 更新背景音乐音量()
	{
		if (音乐播放器 != null)
			音乐播放器.VolumeDb = Mathf.LinearToDb(当前背景音乐音量);
	}

	// ---------- 音效 ----------
	/// <summary>
	/// 将线性音量 0~1 映射到分贝范围 [音效最小分贝, 音效最大分贝]
	/// </summary>
	private float 线性转分贝(float 线性值)
	{
		return Mathf.Lerp(音效最小分贝, 音效最大分贝, Mathf.Clamp(线性值, 0f, 1f));
	}

	public void 播放音效(AudioStream 音效流)
	{
		if (音效流 == null)
		{
			GD.PrintErr("全局背景音乐管理器：音效流为空");
			return;
		}
		音效播放器.Stream = 音效流;
		音效播放器.VolumeDb = 线性转分贝(当前音效音量);
		音效播放器.Play();
		GD.Print($"🔊 播放音效: {音效流.ResourcePath} (音量: {音效播放器.VolumeDb:F1}dB)");
	}

	public void 设置音效音量(float 线性音量)
	{
		当前音效音量 = Mathf.Clamp(线性音量, 0f, 1f);
		更新音效音量();
		GD.Print($"🎛️ 全局音效音量已设为: {当前音效音量:F2} (对应分贝: {线性转分贝(当前音效音量):F1}dB)");
	}

	public float 获取音效音量() => 当前音效音量;

	private void 更新音效音量()
	{
		if (音效播放器 != null)
			音效播放器.VolumeDb = 线性转分贝(当前音效音量);
	}
}
