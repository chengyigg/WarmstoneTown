using Godot;
using System;
using 你的项目.Scripts.管理器;
using System.Collections.Generic;

public partial class 状态栏界面 : CanvasLayer
{
	// 单例实例
	private static 状态栏界面 实例;
	public static 状态栏界面 获取实例() => 实例;
private CanvasLayer 当前按键设置图层 = null;
	// ===== 背包过滤器节点引用 =====
	[Export] private 背包过滤器 背包过滤器节点;
	// 详情框相关
	private Control 详情框;
	private Label 详情描述标签;

	// 节点引用（在编辑器中拖入）
	[Export] private Control 动画容器;
	[Export] private AnimationPlayer 界面动画播放器;
	[Export] private 标签管理器 标签管理器节点;
	[Export] private 设置菜单管理器 设置菜单管理器节点;
	[Export] private AnimationPlayer 设备动画播放器;
	[Export] private PackedScene 卡牌UIPrefab;
	[Export] private PackedScene 强化预览面板预制体;
	// ========== 背包相关变量（直接在场景中拖入）==========
	[Export] private Button 卡牌标签Button;
	[Export] private ScrollContainer 背包面板;
	[Export] private GridContainer 格子Container;
	// ===============================================================
	[Export] private Font 详情框字体;
	// 新增：可拖入的详情框背景样式资源
[Export] private StyleBox 详情框背景样式;   
	// 道具相关UI节点
	[Export] private ScrollContainer 道具面板;
	[Export] private GridContainer 道具格子容器;
	[Export] private Control 道具描述框;
	[Export] private Label 道具描述标签;
[ExportGroup("音效")]
[Export] public AudioStream 打开音效;
[Export] public AudioStream 关闭音效;
[Export] public AudioStream 标签切换音效;
[Export] public AudioStream 按钮点击音效; // 通用
// 可拖入自定义详情框背景样式

	private ConfirmationDialog 使用确认对话框;

	private List<道具槽> 所有道具槽 = new List<道具槽>();
	private int 当前选中行 = 0;
	private int 当前选中列 = 0;
	private bool 道具标签激活 = false;






	// 状态
	private bool 已打开 = false;
	public bool 界面已打开 => 已打开;
	private bool 正在播放离场动画 = false;

	// 记录当前选中的卡牌
	private 卡牌UI 当前选中的卡牌 = null;

	// 背包打开状态标记
	private bool 背包打开中 = false;

	// 输入动作名称
	private const string 状态栏动作 = "打开状态栏";

	// ======================== 生命周期 ========================
	public override void _Ready()
	{
		// 先设置单例
		if (实例 == null)
		{
			实例 = this;
			ProcessMode = ProcessModeEnum.Always;
		}
		else
		{
			QueueFree();
			return;
		}

		// 初始化道具对话框（必须在添加信号前）
		初始化道具对话框();

		// 连接道具管理器信号（先判空）
		if (玩家道具管理器.实例 != null)
		{
			玩家道具管理器.实例.道具列表已更新 += 刷新道具UI;
			刷新道具UI();
		}
		else
		{
			GD.PrintErr("玩家道具管理器实例不存在，请确保场景中有玩家道具管理器节点");
		}

		// 初始隐藏道具面板
		if (道具面板 != null) 道具面板.Visible = false;
		if (背包过滤器节点 != null) 背包过滤器节点.Visible = false;

		GD.Print("状态栏界面初始化开始");
		初始化组件();
		初始化背包();
		创建详情框();
		初始隐藏();
		CallDeferred("延迟初始化修正");
		GD.Print("状态栏界面初始化完成");
	}

	public override void _Input(InputEvent 事件)
	{
		if (正在播放离场动画)
		{
			GetViewport().SetInputAsHandled();
			return;
		}

		if (事件.IsActionPressed(状态栏动作))
		{
			切换显示状态();
			GetViewport().SetInputAsHandled();
		}

		// 道具导航
		if (道具标签激活 && 道具面板.Visible && !正在播放离场动画)
		{
			if (事件.IsActionPressed("ui_up"))
			{
				int 新行 = 当前选中行 - 1;
				if (新行 >= 0)
				{
					当前选中行 = 新行;
					更新选中高亮和描述();
				}
				GetViewport().SetInputAsHandled();
			}
			else if (事件.IsActionPressed("ui_down"))
			{
				int 新行 = 当前选中行 + 1;
				int 新索引 = 新行 * 2 + 当前选中列;
				if (新索引 < 所有道具槽.Count)
				{
					当前选中行 = 新行;
					更新选中高亮和描述();
				}
				GetViewport().SetInputAsHandled();
			}
			else if (事件.IsActionPressed("ui_left"))
			{
				if (当前选中列 == 1)
				{
					当前选中列 = 0;
					更新选中高亮和描述();
				}
				GetViewport().SetInputAsHandled();
			}
			else if (事件.IsActionPressed("ui_right"))
			{
				if (当前选中列 == 0)
				{
					int 新索引 = 当前选中行 * 2 + 1;
					if (新索引 < 所有道具槽.Count)
					{
						当前选中列 = 1;
						更新选中高亮和描述();
					}
				}
				GetViewport().SetInputAsHandled();
			}
			else if (事件.IsActionPressed("ui_accept"))
			{
				使用当前选中的道具();
				GetViewport().SetInputAsHandled();
			}
		}

		// 原有的取消键逻辑
	if (已打开 && Visible && 事件.IsActionPressed("ui_cancel"))
{
	// 1. 优先关闭强化面板
	var 强化面板 = GetNodeOrNull<强化预览面板>("强化预览面板");
	if (强化面板 != null && 强化面板.Visible)
	{
		if (强化面板.HasMethod("关闭面板")) 强化面板.Call("关闭面板");
		else 强化面板.QueueFree();
		GetViewport().SetInputAsHandled();
		return;
	}
	
	// 2. 如果背包打开，只关闭背包（不关闭状态栏）
	if (背包面板 != null && 背包面板.Visible)
	{
		关闭背包();
		标签管理器节点?.获取当前标签()?.GrabFocus();
		GetViewport().SetInputAsHandled();
		return; // ✅ 关键：返回，不继续执行关闭界面
	}

	// 3. 如果道具面板激活且可见，只关闭道具面板
	if (道具标签激活 && 道具面板.Visible)
	{
		道具面板.Visible = false;
		道具标签激活 = false;
		if (道具描述框 != null) 道具描述框.Visible = false;
		标签管理器节点?.获取当前标签()?.GrabFocus();
		GetViewport().SetInputAsHandled();
		return;
	}
	
	// 4. 如果设置菜单激活，处理设置菜单的关闭
	if (设置菜单管理器节点 != null && 设置菜单管理器节点.是否激活中())
	{
		if (设置菜单管理器节点.音量面板打开)
			_ = 设置菜单管理器节点.关闭音量面板();
		else
			关闭设置菜单();
		GetViewport().SetInputAsHandled();
		return;
	}
	
	// 5. 没有任何子面板打开，则关闭整个状态栏
	关闭界面();
	GetViewport().SetInputAsHandled();
}
}
	// ======================== 基础控制方法 ========================
	private void 初始化组件()
	{
		GD.Print("=== 初始化组件 ===");
		if (标签管理器节点 != null)
		{
			标签管理器节点.标签切换 += (int 索引) => 当标签切换(索引);
			标签管理器节点.打开设置菜单 += () => 当打开设置菜单();
			GD.Print("✅ 标签管理器信号连接成功");
		}
		else GD.PrintErr("❌ 标签管理器节点为空");

		if (设置菜单管理器节点 != null)
		{
			设置菜单管理器节点.返回游戏请求 += 当返回游戏请求;
			设置菜单管理器节点.打开音量设置 += 当打开音量设置;
			设置菜单管理器节点.返回主菜单请求 += 当返回主菜单请求;
			设置菜单管理器节点.打开按键设置 += 当打开按键设置;
			GD.Print("✅ 设置菜单管理器信号连接成功");
		}
		else GD.PrintErr("❌ 设置菜单管理器节点为空");
	}

	private void 延迟初始化修正()
	{
		GD.Print("=== 延迟初始化修正 ===");
		动态修正动画轨道路径();
		确保设置菜单节点树可见();
	}

	private void 控制玩家移动(bool 启用)
	{
		if (玩家管理器.实例 != null && 玩家管理器.实例.玩家存在())
		{
			玩家管理器.实例.当前玩家.设置可移动(启用);
			GD.Print($"玩家移动已{(启用 ? "启用" : "禁用")}");
		}
		else GD.PrintErr("无法控制玩家移动：玩家管理器或玩家实例不存在");
	}

	private void 切换显示状态()
	{
		if (正在播放离场动画) return;
		if (已打开)
		{
			GD.Print("关闭状态栏");
			关闭界面();
		}
		else
		{
			var 当前场景 = GetTree().CurrentScene;
			if (当前场景 == null || !当前场景.IsInGroup("需要玩家"))
			{
				GD.Print("当前场景不在'需要玩家'分组中，不能打开状态栏");
				return;
			}
			if (玩家管理器.实例 == null || !玩家管理器.实例.玩家存在())
			{
				GD.PrintErr("玩家不存在，不能打开状态栏");
				return;
			}
			GD.Print("打开状态栏");
			打开界面();
		}
	}
public override void _ExitTree()
{
	// ★ 新增：节点被销毁时清理详情框
	隐藏详情框();
	base._ExitTree();
}
	private void 打开界面()
	{
		if (正在播放离场动画) return;
		已打开 = true;
		Visible = true;
		标签管理器节点?.隐藏所有标签();
		控制玩家移动(false);
		string 当前场景路径 = GetTree().CurrentScene.SceneFilePath;
		全局背景音乐管理器.实例?.根据场景播放音乐(当前场景路径);
		GD.Print("状态栏已打开");
		if (设置菜单管理器节点 != null) 设置菜单管理器节点.关闭();
		if (界面动画播放器 != null && 界面动画播放器.HasAnimation("打开"))
		{
			界面动画播放器.Play("打开");
			界面动画播放器.AnimationFinished += 当开场动画完成;
		}
		else 当开场动画完成("无动画");
		  if (打开音效 != null && 全局背景音乐管理器.实例 != null)
		全局背景音乐管理器.实例.播放音效(打开音效);
	}

private void 关闭界面()
{
	if (!Visible || 正在播放离场动画) return;
	GD.Print("开始关闭界面");
	正在播放离场动画 = true;
	
	隐藏详情框();
	标签管理器节点?.隐藏所有标签();
	
	if (界面动画播放器 != null && 界面动画播放器.HasAnimation("离开"))
	{
		界面动画播放器.Play("离开");
		界面动画播放器.AnimationFinished += 当离场动画完成;  // ← 等待动画完成
	}
	else 当离场动画完成("无动画");
	
	
	if (关闭音效 != null && 全局背景音乐管理器.实例 != null)
		全局背景音乐管理器.实例.播放音效(关闭音效);
}


	private void 初始隐藏()
	{
		已打开 = false;
		Visible = false;
		正在播放离场动画 = false;
		GD.Print("状态栏初始隐藏");
	}

private void 当开场动画完成(StringName 动画名称)
{
	if (界面动画播放器 != null) 界面动画播放器.AnimationFinished -= 当开场动画完成;
	GD.Print("开场动画完成");
	标签管理器节点?.显示所有标签();
	if (标签管理器节点 != null)
	{
		标签管理器节点.设置当前选中索引(0);
		标签管理器节点.获取标签(0)?.GrabFocus();
	}

	// ★ 通知转场管理器更新全局UI可见性（状态栏已打开，应隐藏提示标签）
	转场管理器.实例?.更新全局UI可见性();
}

private void 当离场动画完成(StringName 动画名称)
{
	if (界面动画播放器 != null) 界面动画播放器.AnimationFinished -= 当离场动画完成;
	GD.Print("离场动画完成");
	已打开 = false;
	Visible = false;
	正在播放离场动画 = false;
	控制玩家移动(true);
	GD.Print("状态栏已关闭");

	// ★ 通知转场管理器更新全局UI可见性（状态栏已关闭，应恢复提示标签显示）
	转场管理器.实例?.更新全局UI可见性();
}

	// ======================== 标签切换 ========================
private void 当标签切换(int 索引)
{
	if (背包打开中) 关闭背包();

	// 道具标签索引假设为 1
	if (索引 == 0)
	{
		道具面板.Visible = true;
		道具标签激活 = true;
		刷新道具UI();
		道具面板.ZIndex = 100;
		背包面板.Visible = false;
		
		// ★ 显示描述框
		if (道具描述框 != null)
			道具描述框.Visible = true;
	}
	else
	{
		道具面板.Visible = false;
		道具标签激活 = false;
		
		// ★ 隐藏描述框
		if (道具描述框 != null)
			道具描述框.Visible = false;
	}

	if (索引 == 3) 播放设备动画_修正版();
	  if (标签切换音效 != null && 全局背景音乐管理器.实例 != null)
		全局背景音乐管理器.实例.播放音效(标签切换音效);
}
private void 调试打印动画容器状态(string 阶段)
{
	var 动画容器 = 获取动画容器节点();
	if (动画容器 == null)
	{
		GD.PrintErr($"[{阶段}] 找不到动画容器节点！");
		return;
	}
	
	GD.Print($"[{阶段}] 动画容器.Rotation = {动画容器.Rotation} 弧度 ≈ {动画容器.Rotation * 180 / Mathf.Pi}°");
	
	if (设备动画播放器 == null) return;
	GD.Print($"[{阶段}] AnimationPlayer 当前动画: {设备动画播放器.CurrentAnimation}, 是否播放中: {设备动画播放器.IsPlaying()}, 当前播放位置: {设备动画播放器.CurrentAnimationPosition}");
}

private void 调试打印动画关键帧(string 动画名)
{
	if (设备动画播放器 == null || !设备动画播放器.HasAnimation(动画名)) return;
	var 动画 = 设备动画播放器.GetAnimation(动画名);
	GD.Print($"=== 动画 [{动画名}] 信息 ===");
	GD.Print($"长度: {动画.Length}");
	for (int i = 0; i < 动画.GetTrackCount(); i++)
	{
		string 路径 = 动画.TrackGetPath(i);
		if (路径.Contains("rotation") || 路径.Contains("动画容器"))
		{
			GD.Print($"轨道 {i}: {路径}, 关键帧数量: {动画.TrackGetKeyCount(i)}");
			for (int j = 0; j < 动画.TrackGetKeyCount(i); j++)
			{
				double 时间 = 动画.TrackGetKeyTime(i, j);
				var 值 = 动画.TrackGetKeyValue(i, j);
				GD.Print($"  关键帧{j}: 时间={时间}秒, 值={值}");
			}
		}
	}
}
	private void 当打开设置菜单()
	{
		GD.Print("收到打开设置菜单信号");
		播放设备动画_修正版();
	}

	private void 当返回游戏请求()
	{
		GD.Print("收到返回游戏请求");
		if (设置菜单管理器节点 != null && 设置菜单管理器节点.是否激活中()) 关闭设置菜单();
		else 关闭界面();
	}

	private void 当打开音量设置() => GD.Print("打开音量设置");
private void 当返回主菜单请求()
{
	GD.Print("返回主菜单请求（来自设置菜单）");
	
	// ★★★ 使用重置内存状态，避免副作用 ★★★
	if (存档管理器.实例 != null)
	{
		int 当前存档位 = 卡牌数据管理器.当前存档位;
		if (当前存档位 >= 0)
		{
			存档管理器.实例.重置内存状态到磁盘(当前存档位);
			GD.Print($"[状态栏] 已重置存档位 {当前存档位} 的状态到磁盘版本");
		}
	}
	
	// 立即隐藏所有跟随者，避免转场黑屏动画期间闪现
	跟随者管理器.实例?.隐藏所有跟随者();
	
	// 1. 先关闭设置菜单（如果打开）
	if (设置菜单管理器节点 != null && 设置菜单管理器节点.是否激活中())
		关闭设置菜单();
	
	// 2. 确保游戏恢复运行（如果之前因暂停而暂停）
	GetTree().Paused = false;
	
	// 3. 关闭状态栏界面本身（如果打开）
	if (界面已打开)
		关闭界面();
	
	// 4. 使用转场管理器返回主菜单
	if (转场管理器.实例 != null)
	{
		转场管理器.实例.开始转场(
			场景加载器.实例.开始场景路径,
			null,
			GetTree().CurrentScene.SceneFilePath
		);
	}
	else
	{
		场景加载器.实例.加载开始菜单();
	}
}
// ---------- 公共音效播放方法（供子节点调用）----------
public void 播放切换音效()
{
	全局背景音乐管理器.实例?.播放音效(标签切换音效);
}

public void 播放点击音效()
{
	全局背景音乐管理器.实例?.播放音效(按钮点击音效);
}
private void 当打开按键设置()
{
	GD.Print("打开按键设置界面");
	
	// 如果已经有打开的键位设置界面，先强制关闭并销毁
	if (当前按键设置图层 != null && IsInstanceValid(当前按键设置图层))
	{
		当前按键设置图层.QueueFree();
		当前按键设置图层 = null;
	}
	
	// 先关闭设置菜单管理器，清除激活状态
	设置菜单管理器节点?.关闭();
	
	var 重绑定场景 = GD.Load<PackedScene>("res://状态栏系统（主要ui）/按键重绑定界面.tscn");
	if (重绑定场景 == null)
	{
		GD.PrintErr("找不到按键重绑定界面.tscn，请确认路径");
		return;
	}
	
	var 新图层 = new CanvasLayer();
	新图层.Layer = 9;
	AddChild(新图层);
	当前按键设置图层 = 新图层;  // 记录当前图层
	
	var 重绑定界面 = 重绑定场景.Instantiate<Control>();
	新图层.AddChild(重绑定界面);
	
	GetTree().Paused = true;
	
	重绑定界面.TreeExited += () => 
	{
		GetTree().Paused = false;
		if (当前按键设置图层 == 新图层)
			当前按键设置图层 = null;  // 清除记录
		新图层.QueueFree();
		
		// 强制重新激活设置菜单
		设置菜单管理器节点?.强制重新激活();
	};
}
}
