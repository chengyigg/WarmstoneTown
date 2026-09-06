using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class 手牌管理器 : Control
{
	[Export] private float 手牌区域宽度 = 400f;
	[Export] public Control 手牌容器;
	[Export] public PackedScene 卡牌UIPrefab;
	[Export] public Control 卡牌生成位置;
	[Export] private Control 弃牌堆位置;
	[Export] public Control 卡组位置;
	
	public List<卡牌UI> 当前卡牌UI列表 = new();
	private 卡牌战斗单位 所属单位;
	private 卡牌战斗管理器 _战斗管理器;
	private bool 队列处理暂停 = false;
	// 在手牌管理器类中添加成员变量
private int _当前选中索引 = -1;
private Color 原色调 = Colors.White;
	[Signal] public delegate void 卡牌被使用EventHandler(GodotObject 卡牌);
	
	private Queue<卡牌实例> 待添加卡牌队列 = new Queue<卡牌实例>();
	private bool 正在添加卡牌 = false;
	private float 上次排列时间 = 0f;
	private bool 正在排列 = false;
	
	public override void _Ready() { }
	
public void 初始化(卡牌战斗单位 单位, 卡牌战斗管理器 战斗管理器)
{
	GD.Print("=== 手牌管理器.初始化 ===");
	所属单位 = 单位;
	_战斗管理器 = 战斗管理器;
	GD.Print($"所属单位是否为空: {所属单位 == null}");
	GD.Print($"手牌容器是否为空: {手牌容器 == null}");
	GD.Print($"卡牌UIPrefab是否为空: {卡牌UIPrefab == null}");
	
	if (所属单位 == null)
		GD.PrintErr("所属单位为空！");
	if (手牌容器 == null)
		GD.PrintErr("手牌容器为空！");
	if (卡牌UIPrefab == null)
		GD.PrintErr("卡牌UIPrefab为空！");
	
	GD.Print("调用 更新手牌显示()");
	更新手牌显示();
	GD.Print("手牌管理器.初始化 完成");
}
	
	public void 暂停队列处理() => 队列处理暂停 = true;
	
	public void 刷新所有卡牌高光()
	{
		if (_战斗管理器 == null) return;
		if (所属单位 == null) return;
		foreach (var 卡牌UI in 当前卡牌UI列表)
		{
			bool 可用 = _战斗管理器.卡牌是否可用(卡牌UI.获取卡牌数据());
			卡牌UI.设置高光启用(可用);
		}
	}
	
	public void 恢复队列处理() => 队列处理暂停 = false;
	
	public void 添加卡牌到手中排队(卡牌实例 卡牌, bool 播放动画 = true)
	{
		待添加卡牌队列.Enqueue(卡牌);
		if (!正在添加卡牌)
			开始处理卡牌队列(播放动画);
	}
	
private async void 开始处理卡牌队列(bool 播放动画)
{
	正在添加卡牌 = true;
	int 添加前的卡牌数量 = 当前卡牌UI列表.Count;
	int 总共卡牌数量 = 添加前的卡牌数量 + 待添加卡牌队列.Count;
	
	Vector2[] 所有目标位置 = new Vector2[总共卡牌数量];
	for (int i = 0; i < 总共卡牌数量; i++)
		所有目标位置[i] = 计算单张卡牌位置(i, 总共卡牌数量);
	
	for (int i = 0; i < 添加前的卡牌数量; i++)
	{
		var 现有卡牌 = 当前卡牌UI列表[i];
		var 新位置 = 所有目标位置[i];
		if (!现有卡牌.是拖拽状态())
			现有卡牌.执行平滑移动动画(新位置, 0f, i, 0.3f, 0f);
		现有卡牌.更新原始位置(新位置);
	}
	
	int 新牌索引 = 0;
	while (待添加卡牌队列.Count > 0)
	{
		var 当前卡牌 = 待添加卡牌队列.Dequeue();
		bool 已存在 = false;
		foreach (var 已有卡牌UI in 当前卡牌UI列表)
		{
			if (已有卡牌UI.获取卡牌数据() == 当前卡牌)
			{
				已存在 = true;
				break;
			}
		}
		if (已存在) continue;
		
		var 卡牌UI实例 = 卡牌UIPrefab.Instantiate<卡牌UI>();
		手牌容器.AddChild(卡牌UI实例);
		卡牌UI实例.初始化(当前卡牌);
		
		int 新牌总索引 = 添加前的卡牌数量 + 新牌索引;
		Vector2 新牌目标位置 = 所有目标位置[新牌总索引];
		当前卡牌UI列表.Add(卡牌UI实例);
		卡牌UI实例.更新原始位置(新牌目标位置);
		
		卡牌UI实例.卡牌开始拖拽 += (卡牌UI) => 处理卡牌拖拽开始((卡牌UI)卡牌UI);
		卡牌UI实例.卡牌结束拖拽 += (卡牌UI, 位置) => 处理卡牌拖拽结束((卡牌UI)卡牌UI, 位置);
		卡牌UI实例.卡牌被使用 += (卡牌实例) => 处理卡牌被使用(卡牌实例);
		
		if (播放动画 && 卡组位置 != null)
		{
			卡牌UI实例.Position = 卡组位置.Position;
			卡牌UI实例.Scale = new Vector2(0.1f, 0.1f);
			卡牌UI实例.Rotation = Mathf.Pi;
			卡牌UI实例.Modulate = new Color(1, 1, 1, 0.8f);
			卡牌UI实例.ZIndex = 1000;
			卡牌UI实例.执行流畅入场动画(新牌目标位置, 卡组位置.Position, null);
		}
		else
		{
			卡牌UI实例.Position = 新牌目标位置;
			卡牌UI实例.Scale = 卡牌UI实例.获取原始缩放();
		}
		新牌索引++;
		if (待添加卡牌队列.Count > 0)
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
	}
	
	正在添加卡牌 = false;
	await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
	if (IsInsideTree())
		执行流畅排列动画();
	
	// 关键修复：添加完所有卡牌后刷新高光动画，使每张新卡牌都播放高光出现效果
	刷新所有卡牌高光();
}
	
	private async Task 播放单张卡牌入场动画(卡牌UI 卡牌UI, int 卡牌索引)
	{
		var 目标位置 = 计算单张卡牌位置(卡牌索引);
		卡牌UI.执行流畅入场动画(目标位置, 卡组位置.Position, null);
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
	}
	// 在初始化或刷新手牌后调用此方法重置选中
public void 重置选中索引()
{
	_当前选中索引 = 当前卡牌UI列表.Count > 0 ? 0 : -1;
	更新选中高亮();
}

// 选中上一张
public void 选中上一张()
{
	if (当前卡牌UI列表.Count == 0) return;
	_当前选中索引--;
	if (_当前选中索引 < 0) _当前选中索引 = 当前卡牌UI列表.Count - 1;
	更新选中高亮();
}
// 选中下一张
public void 选中下一张()
{
	if (当前卡牌UI列表.Count == 0) return;
	_当前选中索引++;
	if (_当前选中索引 >= 当前卡牌UI列表.Count) _当前选中索引 = 0;
	更新选中高亮();
}

// 清除高亮并高亮当前选中的卡牌
private void 更新选中高亮()
{
	for (int i = 0; i < 当前卡牌UI列表.Count; i++)
	{
		var 卡牌 = 当前卡牌UI列表[i];
		卡牌.Modulate = (i == _当前选中索引) ? Colors.Yellow : 卡牌.原始色调;
	}
}
// 获取当前选中的卡牌实例
public 卡牌实例 获取当前选中卡牌()
{
	if (_当前选中索引 >= 0 && _当前选中索引 < 当前卡牌UI列表.Count)
		return 当前卡牌UI列表[_当前选中索引].获取卡牌数据();
	return null;
}
// 修正选中索引（当卡牌被使用时，确保索引有效）
public void 修正选中索引()
{
	if (当前卡牌UI列表.Count == 0)
	{
		_当前选中索引 = -1;
	}
	else if (_当前选中索引 >= 当前卡牌UI列表.Count)
	{
		_当前选中索引 = 当前卡牌UI列表.Count - 1;
	}
	更新选中高亮();
}
// 添加公共方法获取当前选中的卡牌UI
public 卡牌UI 获取当前选中的卡牌UI()
{
	if (_当前选中索引 >= 0 && _当前选中索引 < 当前卡牌UI列表.Count)
		return 当前卡牌UI列表[_当前选中索引];
	return null;
}

public void 添加卡牌到手中(卡牌实例 卡牌, bool 播放动画 = true)
{
	// 检查是否已存在
	foreach (var ui in 当前卡牌UI列表)
		if (ui.获取卡牌数据() == 卡牌) return;

	// 直接调用更新方法（它会增量添加）
	更新手牌显示(播放动画);
}
	
	public Vector2 计算单张卡牌位置(int 索引, int 用于计算的总卡牌数量)
	{
		float 容器宽度 = 手牌区域宽度;
		float 卡牌宽度 = 120f;
		if (用于计算的总卡牌数量 == 0) return Vector2.Zero;
		float 基础间距 = 计算动态间距(用于计算的总卡牌数量);
		float 总宽度 = 用于计算的总卡牌数量 * 卡牌宽度 + Mathf.Max(0, 用于计算的总卡牌数量 - 1) * 基础间距;
		if (总宽度 > 容器宽度 * 0.9f && 用于计算的总卡牌数量 > 1)
		{
			总宽度 = 容器宽度 * 0.9f;
			基础间距 = (总宽度 - 用于计算的总卡牌数量 * 卡牌宽度) / Mathf.Max(1, 用于计算的总卡牌数量 - 1);
			基础间距 = Mathf.Max(5f, 基础间距);
			if (用于计算的总卡牌数量 * 卡牌宽度 > 容器宽度)
			{
				float 重叠量 = (用于计算的总卡牌数量 * 卡牌宽度 - 容器宽度) / Mathf.Max(1, 用于计算的总卡牌数量 - 1);
				基础间距 = -重叠量;
				总宽度 = 容器宽度;
			}
		}
		float 起始位置 = (容器宽度 - 总宽度) / 2f;
		float x = 起始位置 + 索引 * (卡牌宽度 + 基础间距);
		float y = 100f;
		return new Vector2(x, y);
	}
	
	private Vector2 计算单张卡牌位置(int 索引) => 计算单张卡牌位置(索引, 当前卡牌UI列表.Count);
	
	private void 检查并执行最终排列()
	{
		float 当前时间 = (float)Time.GetTicksMsec() / 1000f;
		if (当前时间 - 上次排列时间 > 0.5f)
		{
			上次排列时间 = 当前时间;
			var 计时器 = new Timer();
			计时器.OneShot = true;
			计时器.WaitTime = 0.1f;
			AddChild(计时器);
			计时器.Timeout += () => {
				计时器.QueueFree();
				if (IsInsideTree())
					执行流畅排列动画();
			};
			计时器.Start();
		}
	}
	
	private void 处理移除卡牌后的排列() => 执行流畅排列动画();
	
	public void 执行流畅排列动画()
	{
		if (正在排列) return;
		正在排列 = true;
		Callable.From(() => {
			int 卡牌数量 = 当前卡牌UI列表.Count;
			if (卡牌数量 == 0) { 正在排列 = false; return; }
			float 容器宽度 = 手牌区域宽度;
			float 卡牌宽度 = 120f;
			float 基础间距 = 计算动态间距(卡牌数量);
			float 总宽度 = 卡牌数量 * 卡牌宽度 + (卡牌数量 - 1) * 基础间距;
			if (总宽度 > 容器宽度 && 卡牌数量 > 1)
			{
				基础间距 = 5f;
				总宽度 = 卡牌数量 * 卡牌宽度 + (卡牌数量 - 1) * 基础间距;
			}
			float 起始位置 = (容器宽度 - 总宽度) / 2f;
			float y = 100f;
			for (int i = 0; i < 卡牌数量; i++)
			{
				var 卡牌 = 当前卡牌UI列表[i];
				float x = 起始位置 + i * (卡牌宽度 + 基础间距);
				var 目标位置 = new Vector2(x, y);
				卡牌.更新原始位置(目标位置);
				if (!卡牌.是拖拽状态())
					卡牌.执行平滑移动动画(目标位置, 0f, i, 0.3f, Mathf.Abs(i - (卡牌数量-1)/2f)*0.02f);
			}
			正在排列 = false;
		}).CallDeferred();
	}
	
	private void 计算动态弧线参数(int 卡牌数量, out float 半径, out float 总角度)
	{
		半径 = 300f;
		总角度 = Mathf.Pi * 0.6f;
		if (卡牌数量 <= 1)
		{
			半径 = 0f;
			总角度 = 0f;
		}
		else if (卡牌数量 == 2)
		{
			半径 = 200f;
			总角度 = Mathf.Pi * 0.3f;
		}
		else if (卡牌数量 == 3)
		{
			半径 = 250f;
			总角度 = Mathf.Pi * 0.45f;
		}
		else if (卡牌数量 == 4)
		{
			半径 = 300f;
			总角度 = Mathf.Pi * 0.6f;
		}
		else if (卡牌数量 == 5)
		{
			半径 = 350f;
			总角度 = Mathf.Pi * 0.7f;
		}
		else
		{
			半径 = Mathf.Min(400f + (卡牌数量 - 5) * 20f, 600f);
			总角度 = Mathf.Min(Mathf.Pi * 0.8f + (卡牌数量 - 5) * 0.1f, Mathf.Pi * 1.2f);
		}
	}
	
public void 移除卡牌UI(卡牌UI 卡牌UI, bool 播放动画 = true)
{
	if (当前卡牌UI列表.Contains(卡牌UI))
	{
		当前卡牌UI列表.Remove(卡牌UI);
		if (播放动画 && 弃牌堆位置 != null)
		{
			卡牌UI.执行弃置动画(弃牌堆位置.GlobalPosition, () => {
				卡牌UI.QueueFree();
				执行流畅排列动画();
			});
		}
		else
		{
			卡牌UI.QueueFree();
			执行流畅排列动画();
		}
	}
}
	
	public void 添加卡牌(卡牌实例 卡牌) => 添加卡牌到手中(卡牌, false);
	public void 移除卡牌(卡牌实例 卡牌)
	{
		var 卡牌UI = 当前卡牌UI列表.Find(ui => ui.获取卡牌数据() == 卡牌);
		if (卡牌UI != null) 移除卡牌UI(卡牌UI, false);
	}
	
	public void 处理卡牌拖拽结束(卡牌UI 卡牌UI, Vector2 释放位置) => 执行流畅排列动画();
	
public void 处理卡牌被使用(GodotObject 卡牌实例对象)
{
	卡牌实例 卡牌数据 = 卡牌实例对象 as 卡牌实例;
	if (卡牌数据 == null) return;

	GD.Print($"[手牌管理器] 处理卡牌被使用: {卡牌数据.基础数据?.卡牌名称}");
	
	// 通知战斗管理器使用这张卡牌
	_战斗管理器?.玩家主动打出卡牌(卡牌数据);
	
	// 原有的UI移除逻辑
	var 卡牌UI = 当前卡牌UI列表.Find(ui => ui.获取卡牌数据() == 卡牌数据);
	if (卡牌UI != null)
	{
		当前卡牌UI列表.Remove(卡牌UI);
		卡牌UI.QueueFree();
		执行流畅排列动画();
		EmitSignal(SignalName.卡牌被使用, 卡牌实例对象);
	}
}
	
	public void 立即移除卡牌UI(卡牌UI 卡牌UI)
	{
		if (当前卡牌UI列表.Contains(卡牌UI))
			当前卡牌UI列表.Remove(卡牌UI);
		if (卡牌UI.IsInsideTree())
		{
			卡牌UI.Visible = false;
			卡牌UI.QueueFree();
		}
		执行流畅排列动画();
	}
	
public void 更新手牌显示(bool 使用动画 = false)
{
	if (所属单位 == null) return;

	// 1. 构建当前手牌的卡牌实例列表（用于比较）
	var 当前手牌列表 = new List<卡牌实例>();
	foreach (var 卡牌 in 所属单位.手牌)
	{
		if (卡牌 != null) 当前手牌列表.Add(卡牌);
	}

	// 2. 构建当前UI对应的卡牌实例列表
	var 当前UI卡牌列表 = new List<卡牌实例>();
	foreach (var ui in 当前卡牌UI列表)
	{
		if (ui != null && IsInstanceValid(ui))
			当前UI卡牌列表.Add(ui.获取卡牌数据());
	}

	// 3. 找出需要移除的UI（在手牌中不存在了）
	var 待移除 = new List<卡牌UI>();
	foreach (var ui in 当前卡牌UI列表)
	{
		if (ui == null || !IsInstanceValid(ui)) continue;
		var 卡牌数据 = ui.获取卡牌数据();
		if (!当前手牌列表.Contains(卡牌数据))
			待移除.Add(ui);
	}

	// 4. 找出需要新增的卡牌（没有对应UI）
	var 待新增 = new List<卡牌实例>();
	foreach (var 卡牌 in 当前手牌列表)
	{
		bool 已有UI = false;
		foreach (var ui in 当前卡牌UI列表)
		{
			if (ui != null && IsInstanceValid(ui) && ui.获取卡牌数据() == 卡牌)
			{
				已有UI = true;
				break;
			}
		}
		if (!已有UI) 待新增.Add(卡牌);
	}

	// 5. 执行移除
	foreach (var ui in 待移除)
	{
		当前卡牌UI列表.Remove(ui);
		if (IsInstanceValid(ui)) ui.QueueFree();
	}

	// 6. 执行新增
if (待新增.Count > 0)
{
	// 先计算所有卡牌（现有+新增）的目标位置
	int 总卡牌数 = 当前卡牌UI列表.Count + 待新增.Count;
	var 所有目标位置 = new Vector2[总卡牌数];
	for (int i = 0; i < 总卡牌数; i++)
		所有目标位置[i] = 计算单张卡牌位置(i, 总卡牌数);

	// 现有卡牌先移动到新位置（后面排列会再做一次，但先保证）
	for (int i = 0; i < 当前卡牌UI列表.Count; i++)
	{
		var 旧卡牌 = 当前卡牌UI列表[i];
		var 新位置 = 所有目标位置[i];
		旧卡牌.更新原始位置(新位置);
		if (!旧卡牌.是拖拽状态())
			旧卡牌.执行平滑移动动画(新位置, 0f, i, 0.3f, 0f);
	}

	// 新增卡牌
	int 新增起始索引 = 当前卡牌UI列表.Count; // 当前卡牌数量（新增前）
	int 新增索引 = 0;
	foreach (var 卡牌 in 待新增)
	{
		var 卡牌UI实例 = 卡牌UIPrefab.Instantiate<卡牌UI>();
		手牌容器.AddChild(卡牌UI实例);
		卡牌UI实例.初始化(卡牌);
		当前卡牌UI列表.Add(卡牌UI实例);

		// 连接事件
		卡牌UI实例.卡牌开始拖拽 += (卡牌UI) => 处理卡牌拖拽开始((卡牌UI)卡牌UI);
		卡牌UI实例.卡牌结束拖拽 += (卡牌UI, 位置) => 处理卡牌拖拽结束((卡牌UI)卡牌UI, 位置);
		卡牌UI实例.卡牌被使用 += (卡牌实例) => 处理卡牌被使用(卡牌实例);

		int 目标索引 = 新增起始索引 + 新增索引;
		Vector2 最终位置 = 所有目标位置[目标索引];
		卡牌UI实例.更新原始位置(最终位置);

		if (使用动画 && 卡组位置 != null)
		{
			卡牌UI实例.Position = 卡组位置.Position;
			卡牌UI实例.Scale = new Vector2(0.1f, 0.1f);
			卡牌UI实例.Rotation = Mathf.Pi;
			卡牌UI实例.Modulate = new Color(1, 1, 1, 0.8f);
			卡牌UI实例.ZIndex = 1000;
			卡牌UI实例.执行流畅入场动画(最终位置, 卡组位置.Position, null);
		}
		else
		{
			卡牌UI实例.Position = 最终位置;
			卡牌UI实例.Scale = 卡牌UI实例.获取原始缩放();
		}
		新增索引++;
	}
}

	// 7. 重新排列所有卡牌（平滑移动）
	执行流畅排列动画();
	刷新所有卡牌高光();
}
	
	public void 处理卡牌拖拽开始(卡牌UI 卡牌UI) => 卡牌UI.ZIndex = 100;
	
	private void 排列卡牌()
{
	GD.Print("=== 排列卡牌 开始 ===");
	int 卡牌数量 = 当前卡牌UI列表.Count;
	GD.Print($"卡牌数量: {卡牌数量}");
	if (卡牌数量 == 0) return;
	
	float 卡牌宽度 = 120f;
	float 容器宽度 = 手牌区域宽度;
	GD.Print($"容器宽度: {容器宽度}");
	
	float 基础间距 = 计算动态间距(卡牌数量);
	float 总宽度 = 卡牌数量 * 卡牌宽度 + Mathf.Max(0, 卡牌数量 - 1) * 基础间距;
	GD.Print($"基础间距: {基础间距}, 总宽度: {总宽度}");
	
	if (总宽度 > 容器宽度 && 卡牌数量 > 1)
	{
		float 最小间距 = 5f;
		基础间距 = 最小间距;
		总宽度 = 卡牌数量 * 卡牌宽度 + (卡牌数量 - 1) * 基础间距;
		GD.Print($"调整后基础间距: {基础间距}, 总宽度: {总宽度}");
	}
	
	float 起始位置 = (容器宽度 - 总宽度) / 2f;
	float y = 100f;
	GD.Print($"起始位置 X: {起始位置}, Y: {y}");
	
	for (int i = 0; i < 卡牌数量; i++)
	{
		var 卡牌 = 当前卡牌UI列表[i];
		float x = 起始位置 + i * (卡牌宽度 + 基础间距);
		var 新位置 = new Vector2(x, y);
		GD.Print($"  卡牌{i}: 新位置={新位置}");
		if (!卡牌.是拖拽状态())
			卡牌.Position = 新位置;
		卡牌.更新原始位置(新位置);
		卡牌.Rotation = 0f;
		卡牌.ZIndex = i;
		卡牌.Scale = 卡牌.获取原始缩放();
	}
	GD.Print("=== 排列卡牌 结束 ===");
}
public void 播放使用卡牌动画(卡牌实例 卡牌, Action 完成回调 = null)
{
	var 卡牌UI = 当前卡牌UI列表.Find(ui => ui.获取卡牌数据() == 卡牌);
	if (卡牌UI == null)
	{
		完成回调?.Invoke();
		return;
	}
	
	// 从列表中移除（不立即销毁）
	当前卡牌UI列表.Remove(卡牌UI);
	
	// 播放向上消失动画
	卡牌UI.执行使用动画向上消失(() => {
		// 动画完成后重新排列剩余卡牌（平滑移动）
		执行流畅排列动画();
		完成回调?.Invoke();
	});
}
	public void 刷新所有卡牌描述(bool 是否强化)
	{
		foreach (var 卡牌UI in 当前卡牌UI列表)
			卡牌UI.更新描述根据强化模式(是否强化);
	}
	public void 强制启用所有高光()
{
	foreach (var 卡牌UI in 当前卡牌UI列表)
	{
		if (卡牌UI != null && IsInstanceValid(卡牌UI))
			卡牌UI.设置高光启用(true);
	}
}
	private float 计算动态间距(int 卡牌数量)
	{
		float 卡牌宽度 = 120f;
		if (卡牌数量 <= 1) return 0f;
		float 理想总宽度 = 卡牌数量 * 卡牌宽度;
		float 可用宽度 = 手牌区域宽度 - 理想总宽度;
		if (可用宽度 <= 0) return 5f;
		float 基础间距 = 可用宽度 / (卡牌数量 - 1);
		if (卡牌数量 <= 3) return Mathf.Min(100f, 基础间距);
		if (卡牌数量 <= 6) return Mathf.Clamp(基础间距, 10f, 70f);
		return Mathf.Clamp(基础间距, 5f, 30f);
	}
}
