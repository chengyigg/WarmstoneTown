using Godot;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using 你的项目.Scripts.资源;
using 你的项目.Scripts.角色;
using 你的项目.Scripts.摄像机;
using 你的项目.Scripts.交互;

namespace 你的项目.Scripts.管理器
{
	public partial class 转场管理器 : Node
	{
		private static 转场管理器 _实例;
		public static 转场管理器 实例 => _实例;
		private string _二倍缩放场景文件夹 = "res://Scenes/Game/房屋场景/"; // 你可以改成你存放场景的文件夹
		private CanvasLayer _转场界面;
		private ColorRect _背景;
		private Label _文本标签;
		private CenterContainer _居中容器;
		private bool _等待播放摄像头动画 = false;
		private 摄像头动画资源 _等待的摄像头动画配置;
		private string _等待的摄像头动画来源场景;
		private 剧情序列资源 _待播放剧情;
		[Export] public 转场动画资源 默认转场动画资源 { get; set; }
		[Export] public Vector2 默认摄像头缩放 { get; set; } = new Vector2(1.50f, 1.50f);
private List<string> _二倍缩放场景路径列表 = new List<string>();
		[Signal] public delegate void 转场完成EventHandler(string 新场景路径);

public override void _Ready()
{
	if (_实例 == null)
	{
		_实例 = this;
		ProcessMode = ProcessModeEnum.Always;
	}
	else QueueFree();

	初始化二倍缩放场景列表();

	if (默认转场动画资源 == null) 默认转场动画资源 = 转场动画资源.创建默认();
	创建转场界面();
	AddChild(_转场界面);

	// ★ 使用 CallDeferred 延迟创建金币UI
	CallDeferred(nameof(创建金币UI));
}

private void 创建金币UI()
{
	// 检查是否已经存在
	var 已有金币UI = GetTree().Root.FindChild("金币UI", true, false);
	if (已有金币UI != null)
	{
		GD.Print("【转场管理器】金币UI已存在，无需创建");
		return;
	}

	// 加载预制体
	var 预制体路径 = "res://游戏素材/金币场景.tscn"; // ← 修改为实际路径
	var 预制体 = GD.Load<PackedScene>(预制体路径);
	if (预制体 == null)
	{
		GD.PrintErr($"【转场管理器】无法加载金币UI预制体：{预制体路径}");
		return;
	}

	var 新金币UI = 预制体.Instantiate<CanvasLayer>();
	新金币UI.Name = "金币UI";
	GetTree().Root.AddChild(新金币UI);
	GD.Print("【转场管理器】已创建金币UI并添加到根节点");
}
		private void 初始化二倍缩放场景列表()
{
	_二倍缩放场景路径列表.Clear();
	
	using var dir = DirAccess.Open(_二倍缩放场景文件夹);
	if (dir == null)
	{
		GD.PrintErr($"[转场管理器] 无法打开文件夹: {_二倍缩放场景文件夹}");
		return;
	}

	dir.ListDirBegin();
	string 文件名;
	while ((文件名 = dir.GetNext()) != "")
	{
		if (文件名.EndsWith(".tscn"))
		{
			string 完整路径 = _二倍缩放场景文件夹 + 文件名;
			_二倍缩放场景路径列表.Add(完整路径);
			GD.Print($"[转场管理器] 添加二倍缩放场景: {完整路径}");
		}
	}
	dir.ListDirEnd();

	GD.Print($"[转场管理器] 共找到 {_二倍缩放场景路径列表.Count} 个场景，将使用 2 倍缩放");
}
		private void 创建转场界面()
		{
			_转场界面 = new CanvasLayer();
			_转场界面.Name = "转场界面";
			_转场界面.Layer = 100;
			_背景 = new ColorRect();
			_背景.Name = "背景";
			_背景.AnchorRight = 1.0f;
			_背景.AnchorBottom = 1.0f;
			_背景.Color = new Color(0, 0, 0, 0);
			_居中容器 = new CenterContainer();
			_居中容器.Name = "居中容器";
			_居中容器.AnchorRight = 1.0f;
			_居中容器.AnchorBottom = 1.0f;
			_文本标签 = new Label();
			_文本标签.Name = "文本标签";
			_文本标签.HorizontalAlignment = HorizontalAlignment.Center;
			_文本标签.VerticalAlignment = VerticalAlignment.Center;
			_文本标签.Modulate = new Color(1, 1, 1, 0);
			_居中容器.AddChild(_文本标签);
			_转场界面.AddChild(_背景);
			_转场界面.AddChild(_居中容器);
			_转场界面.Visible = false;
		}

public async void 开始转场(string 目标场景路径, 转场动画资源 转场配置 = null, string 来源场景路径 = "", 
bool 是继续游戏 = false, bool 播放摄像头动画 = false, 摄像头动画资源 摄像头动画配置 = null, 
bool 转场后触发对话 = false, 对话序列 转场后对话序列 = null, int 存档位 = 0,
string 目标传送点名称 = "")
{
	// ⭐ 打印当前加载的场景路径（用于定位）
	GD.Print($"🔄 [转场管理器] 正在加载场景: {目标场景路径}");
	// ⭐ 关键：在加载新场景之前就设置好当前存档位
	if (是继续游戏 || 存档位 == -1)
	{
		if (存档位 >= 0 || 存档位 == -1)
			卡牌数据管理器.当前存档位 = 存档位;
	}

	if (转场配置 == null) 转场配置 = 默认转场动画资源;

	if (播放摄像头动画 && 摄像头动画配置 != null)
	{
		_等待播放摄像头动画 = true;
		_等待的摄像头动画配置 = 摄像头动画配置;
		_等待的摄像头动画来源场景 = 来源场景路径;
	}

	var 现有玩家 = 玩家管理器.实例.当前玩家;
	if (现有玩家 != null && IsInstanceValid(现有玩家)) 现有玩家.准备场景切换();

	await 显示转场动画(转场配置);

	// 恢复条件/看法状态
	if (是继续游戏 && 存档位 >= 0 && 存档管理器.实例 != null)
	{
		存档管理器.实例.应用条件状态(存档位);
		存档管理器.实例.应用看法解锁状态(存档位);
	}

	if (现有玩家 != null && 现有玩家.GetParent() != null)
		现有玩家.GetParent().RemoveChild(现有玩家);

	var 新场景资源 = GD.Load<PackedScene>(目标场景路径);
	if (新场景资源 == null)
	{
		await 隐藏转场动画(转场配置);
		EmitSignal(nameof(转场完成), 目标场景路径);
		return;
	}

	Callable.From(() => GetTree().ChangeSceneToPacked(新场景资源)).CallDeferred();
	await ToSignal(GetTree(), "process_frame");
	await ToSignal(GetTree(), "process_frame");
	await ToSignal(GetTree(), "process_frame");

	if (GetTree().CurrentScene == null)
	{
		await 隐藏转场动画(转场配置);
		return;
	}

	var 新场景根节点 = GetTree().CurrentScene;
	bool 新场景需要玩家 = 新场景根节点 != null && 新场景根节点.IsInGroup(游戏分组.需要玩家);
	if (目标场景路径.Contains("黑屏过渡场景")) 新场景需要玩家 = false;

	if (新场景需要玩家)
{
	var 玩家 = 玩家管理器.实例.当前玩家;
	if (玩家 == null) 玩家 = 玩家管理器.实例.创建玩家();
	if (玩家 != null)
	{
		// ★ 查找名为 "世界" 的 Node2D 容器（已启用 YSort）
		Node2D 容器 = null;
		// 优先查找场景根节点下的直接子节点
		foreach (Node child in 新场景根节点.GetChildren())
		{
			if (child is Node2D node && node.Name == "世界")
			{
				容器 = node;
				break;
			}
		}
		// 如果找不到，尝试递归查找（备选）
		if (容器 == null)
			容器 = 新场景根节点.GetNodeOrNull<Node2D>("世界");

		if (容器 == null)
		{
			// 如果没有预设容器，动态创建一个（但建议您在场景中提前放好）
			容器 = new Node2D();
			容器.Name = "世界";
			容器.YSortEnabled = true;
			新场景根节点.AddChild(容器);
			GD.Print("[转场管理器] 未找到 '世界' 节点，已动态创建");
		}

		// 将玩家添加到容器
		容器.AddChild(玩家);

		// 设置玩家初始位置（位置使用全局坐标，容器位置为(0,0)时无影响）
		设置玩家初始位置(来源场景路径, 是继续游戏, 存档位, 目标传送点名称);

		bool 需要摄像头 = 新场景根节点.IsInGroup(游戏分组.需要摄像头);
		if (需要摄像头) 设置玩家摄像头(玩家);

		if (玩家 != null)
		{
			玩家.结束过场动画();
			玩家.设置输入启用(true);
		}
		玩家.强制终止移动并重置();
	}
}
	else 玩家管理器.实例.销毁玩家();

	// 应用金币和卡组状态
	if (是继续游戏 && 存档位 >= 0 && 存档管理器.实例 != null)
	{
		存档管理器.实例.应用金币状态(存档位);
		存档管理器.实例.应用卡组状态(存档位);
	}

	await 隐藏转场动画(转场配置);

	一次性动画管理器.实例?.恢复当前场景一次性动画();

	if (跟随者管理器.实例 != null)
	{
		if (新场景需要玩家) 跟随者管理器.实例.显示所有跟随者();
		else 跟随者管理器.实例.隐藏所有跟随者();
	}

	EmitSignal(nameof(转场完成), 目标场景路径);

	// 刷新对话触发器
	if (是继续游戏 && 存档位 >= 0 && 存档管理器.实例 != null)
	{
		var 所有触发器 = GetTree().GetNodesInGroup("对话触发器");
		foreach (Node 节点 in 所有触发器)
		{
			if (节点 is 对话触发器 触发器)
				触发器.强制刷新条件();
		}
		var 状态栏 = GetTree().Root.FindChild("状态栏界面", true, false) as 状态栏界面;
		if (状态栏 != null && 状态栏.界面已打开) 状态栏.刷新背包按类型(null);

		var 存档数据 = 存档管理器.实例?.获取存档数据(存档位);
		if (存档数据 != null && 玩家管理器.实例?.当前玩家 != null)
			玩家管理器.实例.当前玩家.重建跟随者(存档数据.获取跟随者列表());
	}

	if (_等待播放摄像头动画 && _等待的摄像头动画配置 != null)
	{
		if (玩家管理器.实例 != null && 玩家管理器.实例.当前玩家 != null)
			玩家管理器.实例.当前玩家.开始过场动画();
		await 执行摄像头动画(_等待的摄像头动画配置);
		_等待播放摄像头动画 = false;
		_等待的摄像头动画配置 = null;
		if (玩家管理器.实例 != null && 玩家管理器.实例.当前玩家 != null)
			玩家管理器.实例.当前玩家.结束过场动画();
	}

	if (对话播放器.实例 != null) 对话播放器.实例.场景加载完成();
	if (游戏管理器.实例 != null) 游戏管理器.实例.场景加载完成(存档位);
	if (全局背景音乐管理器.实例 != null)
		全局背景音乐管理器.实例.根据场景播放音乐(GetTree().CurrentScene.SceneFilePath);

	await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	CallDeferred(nameof(更新全局UI可见性));

	// ★ 在场景加载完成后，检查并添加任务面板
	CallDeferred(nameof(延迟添加任务面板));

// ★ 处理转场后触发对话（只有显式传参为true才立即触发）
// ★★★ 处理转场后对话（包括立即触发和待触发）★★★
// ★★★ 处理转场后对话（包括立即触发和待触发）★★★
// ★★★ 处理转场后对话（包括立即触发和待触发）★★★
if (转场后触发对话 && 转场后对话序列 != null)
{
	// 立即触发对话，由对话播放器锁定玩家
	await ToSignal(GetTree(), "process_frame");
	if (对话播放器.实例 != null)
	{
		对话播放器.实例.开始对话(转场后对话序列);
	}
}
else
{
	// 没有立即触发对话，检查是否有待触发的传送后对话 或 对话正在进行
// 改为：
bool 有待触发传送后对话 = false;
bool 对话进行中 = false;
if (对话播放器.实例 != null)
{
	有待触发传送后对话 = 对话播放器.实例.是否等待传送后触发();
	对话进行中 = 对话播放器.实例.对话进行中;
}

	if (有待触发传送后对话 || 对话进行中)
	{
		GD.Print($"[转场管理器] 检测到待触发对话或对话进行中（进行中={对话进行中}），保持玩家锁定");
		// 什么也不做，对话播放器会在对话结束后解锁
	}
	else
	{
		// 安全解锁
		var 玩家实例 = 玩家管理器.实例?.当前玩家;
		if (玩家实例 != null)
		{
			await ToSignal(GetTree(), "process_frame");
			玩家实例.结束过场动画(true);
			玩家实例.设置输入启用(true);
			玩家实例.设置禁止移动(false);
			GD.Print("[转场管理器] 已强制重置玩家移动状态（无待触发对话且无对话进行）");
		}
	}
}
} 

private Vector2 获取场景缩放(string 场景路径)
{
	if (_二倍缩放场景路径列表.Contains(场景路径))
		return new Vector2(2, 2);
	else
		return Vector2.One; // 默认1倍
}
private void 延迟更新UI可见性()
{
	var 当前场景 = GetTree().CurrentScene;
	if (当前场景 == null) return;

	bool 需要玩家 = 当前场景.IsInGroup("需要玩家");
	bool 是战斗场景 = 当前场景.IsInGroup("战斗场景");
	bool 状态栏打开 = 状态栏界面.获取实例()?.界面已打开 ?? false;

	// ★ 金币UI和提示标签：只有在需要玩家的场景中、非战斗、状态栏关闭时才显示
	bool 应该显示 = 需要玩家 && !是战斗场景 && !状态栏打开;

	var 金币UI = GetTree().Root.FindChild("金币UI", true, false) as CanvasLayer;
	if (金币UI != null)
		金币UI.Visible = 应该显示;

	var 提示UI = GetTree().Root.FindChild("提示标签UI", true, false) as CanvasLayer;
	if (提示UI != null)
		提示UI.Visible = 应该显示;
}
private void 延迟添加任务面板()
{
	// 检查并添加任务面板（已有代码）
	var 现有面板 = GetTree().Root.FindChild("任务面板UI", true, false);
	if (现有面板 == null)
	{
		var 面板预制体 = GD.Load<PackedScene>("res://任务系统/场景/任务面板UI.tscn");
		if (面板预制体 != null)
		{
			var 面板 = 面板预制体.Instantiate();
			面板.Name = "任务面板UI";
			GetTree().Root.AddChild(面板);
			GD.Print("[转场管理器] 已添加任务面板");
		}
	}

// ★ 检查并添加任务提示UI（直接创建脚本实例，不依赖预制体）
var 现有提示 = GetTree().Root.FindChild("任务提示UI", true, false);
if (现有提示 == null)
{
	var 提示 = new 任务提示UI();
	提示.Name = "任务提示UI";
	GetTree().Root.AddChild(提示);
	GD.Print("[转场管理器] 已创建任务提示UI（脚本实例）");
}
}public void 更新全局UI可见性()
{
	CallDeferred(nameof(延迟更新UI可见性));
}


		public void 设置待播放剧情(剧情序列资源 剧情) => _待播放剧情 = 剧情;
		public 剧情序列资源 取出待播放剧情() { var 剧情 = _待播放剧情; _待播放剧情 = null; return 剧情; }
		
		public async Task 执行摄像头动画(摄像头动画资源 动画配置)
		{
			if (动画配置 == null || !动画配置.启用动画) return;
		var 摄像头 = GetTree().GetFirstNodeInGroup("摄像头") as 玩家摄像头;

			if (摄像头 == null) { GD.PrintErr("无法找到玩家摄像头!"); return; }
			
			Vector2 原始位置 = 摄像头.GlobalPosition;
			bool 原始预测状态 = 摄像头.启用预测;
			bool 原始边界限制状态 = 摄像头.启用边界限制;
			摄像头.SetPhysicsProcess(false);
			摄像头.启用预测 = false;
			摄像头.启用边界限制 = false;
			
			foreach (var 动画 in 动画配置.序列)
			{
				if (动画.移动时长 > 0)
				{
					Vector2 目标位置 = 原始位置 + 动画.目标位置偏移;
					await 移动摄像头到位置(摄像头, 目标位置, 动画.移动时长, 动画.缓动类型, 动画.过渡类型);
				}
				if (动画.停留时间 > 0) await ToSignal(GetTree().CreateTimer(动画.停留时间), "timeout");
			}
			await 移动摄像头到位置(摄像头, 原始位置, 1.0f);
			摄像头.启用预测 = 原始预测状态;
			摄像头.启用边界限制 = 原始边界限制状态;
			摄像头.SetPhysicsProcess(true);
		}
		
		public async Task 播放摄像头动画资源(摄像头动画资源 配置)
		{
			if (配置 == null) { GD.PrintErr("转场管理器: 摄像头动画资源为空"); return; }
			await 执行摄像头动画(配置);
		}
		
		private async Task 移动摄像头到位置(玩家摄像头 摄像头, Vector2 目标位置, float 时长, 
			Tween.EaseType 缓动类型 = Tween.EaseType.InOut, Tween.TransitionType 过渡类型 = Tween.TransitionType.Quad)
		{
			var 补间 = CreateTween();
			补间.SetEase(缓动类型);
			补间.SetTrans(过渡类型);
			补间.TweenProperty(摄像头, "global_position", 目标位置, 时长);
			await ToSignal(补间, "finished");
		}

	private void 设置玩家初始位置(string 来源场景路径, bool 是继续游戏 = false, int 存档位 = 0, string 目标传送点名称 = "")
{
	var 玩家 = 玩家管理器.实例.当前玩家;
	if (玩家 == null) return;
	string 当前场景路径 = GetTree().CurrentScene.SceneFilePath;
			if (是继续游戏)
			{
				bool 当前场景有存档 = 存档管理器.实例?.场景是否有存档(存档位, 当前场景路径) ?? false;
				if (当前场景有存档)
				{
					var 存档数据 = 存档管理器.实例.获取存档数据(存档位);
					Vector2 场景特定位置 = 存档数据.获取场景存档位置(当前场景路径);
					玩家.传送到(场景特定位置);
				}
				else if (存档管理器.实例?.是否有存档(存档位) ?? false)
				{
					var 存档数据 = 存档管理器.实例.获取存档数据(存档位);
					玩家.传送到(存档数据.存档位置);
				}
				else 传送到对应位置(来源场景路径, 存档位);
			}
			 else
	{
		// ★ 将目标传送点名称传递给传送到对应位置
		传送到对应位置(来源场景路径, 存档位, 目标传送点名称);
	}
		}

private void 设置玩家摄像头(玩家控制器 玩家)
{
	string 当前场景路径 = GetTree().CurrentScene.SceneFilePath;
	Vector2 目标缩放 = 获取场景缩放(当前场景路径);

	var 已有摄像头 = GetTree().GetFirstNodeInGroup("摄像头") as 玩家摄像头;
	if (已有摄像头 != null)
	{
		已有摄像头.设置目标(玩家);
		已有摄像头.立即跳转到目标();
		已有摄像头.MakeCurrent();
		已有摄像头.SetPhysicsProcess(true);
		已有摄像头.重新检测边界();
		// 应用缩放
		已有摄像头.应用缩放(目标缩放);
		GD.Print($"[转场管理器] 设置摄像头缩放为 {目标缩放} (场景: {当前场景路径})");
		return;
	}

	// 创建新摄像头
	var 摄像头 = new 玩家摄像头();
	摄像头.Name = "玩家摄像头";
	摄像头.初始缩放 = 目标缩放;
	摄像头.启用缩放保护 = true;
	GetTree().CurrentScene.AddChild(摄像头);
	摄像头.设置目标(玩家);
	摄像头.立即跳转到目标();
	摄像头.MakeCurrent();
	GD.Print($"[转场管理器] 创建新的玩家摄像头，缩放 {目标缩放}");
}
		
private void 传送到对应位置(string 来源场景路径, int 存档位 = 0, string 目标传送点名称 = "")
{
	var 玩家 = 玩家管理器.实例.当前玩家;
	if (玩家 == null) return;


			if (卡牌数据管理器.上下文.从战斗返回)
			{
				if (卡牌数据管理器.上下文.返回玩家位置 != Vector2.Zero)
					玩家.GlobalPosition = 卡牌数据管理器.上下文.返回玩家位置;
				卡牌数据管理器.上下文.从战斗返回 = false;
				卡牌数据管理器.上下文.返回玩家位置 = Vector2.Zero;
				return;
			}
			
		  var 所有传送点 = GetTree().GetNodesInGroup("传送点").OfType<传送点>().ToList();
			if (所有传送点.Count == 0)
			{
				if (存档管理器.实例?.是否有存档(存档位) ?? false)
				{
					var 存档数据 = 存档管理器.实例.获取存档数据(存档位);
					玩家.传送到(存档数据.存档位置);
				}
				return;
			}
			
		 传送点 匹配传送点 = null;

	// ★ 优先按名称匹配
	if (!string.IsNullOrEmpty(目标传送点名称))
	{
		匹配传送点 = 所有传送点.FirstOrDefault(point => point.传送点名称 == 目标传送点名称);
		if (匹配传送点 != null)
		{
			GD.Print($"[转场管理器] 按名称匹配到传送点: {目标传送点名称}");
			匹配传送点.激活传送点();
			return;
		}
		GD.Print($"[转场管理器] 未找到名称为 '{目标传送点名称}' 的传送点，回退到来源场景匹配");
	}

	// 按来源场景匹配
	匹配传送点 = 所有传送点.FirstOrDefault(point => point.规范化来源场景路径 == 来源场景路径);
	if (匹配传送点 != null)
	{
		匹配传送点.激活传送点();
	}
	else
	{
		// 使用第一个传送点
		所有传送点[0].激活传送点();
		GD.Print($"[转场管理器] 未找到匹配的传送点，使用第一个");
	}
}
		
		public  async Task 显示转场动画(转场动画资源 配置)
		{
			_转场界面.Visible = true;
			_背景.Color = new Color(配置.背景颜色.R, 配置.背景颜色.G, 配置.背景颜色.B, 0);
			_文本标签.AddThemeColorOverride("font_color", 配置.文本颜色);
			_文本标签.AddThemeFontSizeOverride("font_size", 配置.字体大小);
			_文本标签.Modulate = new Color(1, 1, 1, 0);
			
			if (配置.自定义字体 != null) _文本标签.AddThemeFontOverride("font", 配置.自定义字体);
			else if (!string.IsNullOrEmpty(配置.自定义字体路径))
			{
				var 字体资源 = GD.Load<FontFile>(配置.自定义字体路径);
				if (字体资源 != null) _文本标签.AddThemeFontOverride("font", 字体资源);
			}
			
			string 原始文本 = 配置.显示文本;
			string[] 句子列表 = null;
			bool 使用轮流显示 = 原始文本.Contains("/");
			if (使用轮流显示)
			{
				var 有效句子列表 = new List<string>();
				foreach (string 句 in 原始文本.Split('/'))
					if (!string.IsNullOrEmpty(句)) 有效句子列表.Add(句.Trim());
				句子列表 = 有效句子列表.ToArray();
				if (句子列表.Length <= 1) 使用轮流显示 = false;
			}
			
			if (!使用轮流显示) _文本标签.Text = 原始文本;
			else _文本标签.Text = 句子列表[0];
			
			var 淡入补间 = CreateTween();
			淡入补间.SetParallel(true);
			淡入补间.TweenProperty(_背景, "color", new Color(_背景.Color.R, _背景.Color.G, _背景.Color.B, 1), 配置.淡入时间);
			淡入补间.TweenProperty(_文本标签, "modulate", new Color(1, 1, 1, 1), 配置.淡入时间);
			await ToSignal(淡入补间, "finished");
			
			if (!使用轮流显示)
			{
				if (配置.显示时间 > 0) await ToSignal(GetTree().CreateTimer(配置.显示时间), "timeout");
			}
			else
			{
				// 简化轮流显示逻辑
				for (int i = 0; i < 句子列表.Length; i++)
				{
					if (i > 0) _文本标签.Text = 句子列表[i];
					if (配置.显示时间 > 0) await ToSignal(GetTree().CreateTimer(配置.显示时间), "timeout");
				}
			}
		}
		
		public  async Task 隐藏转场动画(转场动画资源 配置)
		{
			var 补间 = CreateTween();
			补间.SetParallel(true);
			补间.TweenProperty(_背景, "color", new Color(_背景.Color.R, _背景.Color.G, _背景.Color.B, 0), 配置.淡出时间);
			补间.TweenProperty(_文本标签, "modulate", new Color(1, 1, 1, 0), 配置.淡出时间);
			await ToSignal(补间, "finished");
			_转场界面.Visible = false;
		}
	}
}
