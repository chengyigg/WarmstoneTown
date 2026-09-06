using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using 你的项目.Scripts.角色;
using 你的项目.Scripts.资源;
using 你的项目.Scripts.管理器;
using 你的项目.Scripts.全局;

[GlobalClass]
public partial class 对话播放器 : Node
{
	public static 对话播放器 实例 { get; private set; }
	private bool _正在延迟传送后对话 = false;

	// ===== 对话上下文（内部类） =====
	public class 对话上下文
	{
		public 对话序列 待触发的传送后对话 { get; set; } = null;
		public bool 等待传送后触发 { get; set; } = false;
		public bool 禁止NPC交互 { get; set; } = false;

		public void 重置()
		{
			待触发的传送后对话 = null;
			等待传送后触发 = false;
			禁止NPC交互 = false;
		}
	}

	// ★ 当前对话上下文（唯一实例）
	private 对话上下文 _当前对话上下文 = new 对话上下文();
	public static 对话上下文 上下文 => 实例?._当前对话上下文;

	// ===== 传送后对话控制（实例方法） =====
	public void 设置待触发传送后对话(对话序列 对话)
	{
		_当前对话上下文.待触发的传送后对话 = 对话;
		_当前对话上下文.等待传送后触发 = 对话 != null;
		GD.Print($"[对话播放器] 设置待触发传送后对话: {(对话 != null ? 对话.ResourcePath : "null")}");
	}

	public void 清除待触发传送后对话()
	{
		_当前对话上下文.待触发的传送后对话 = null;
		_当前对话上下文.等待传送后触发 = false;
		GD.Print("[对话播放器] 清除待触发传送后对话");
	}

	public bool 是否等待传送后触发()
	{
		return _当前对话上下文.等待传送后触发 && _当前对话上下文.待触发的传送后对话 != null;
	}

	public 对话序列 获取待触发传送后对话()
	{
		return _当前对话上下文.待触发的传送后对话;
	}
// ===== NPC交互控制（实例方法） =====
public void 设置禁止NPC交互(bool 禁止)
{
	_当前对话上下文.禁止NPC交互 = 禁止;
	GD.Print($"[对话播放器] 设置禁止NPC交互: {禁止}");
}

public bool 是否禁止NPC交互()
{
	return _当前对话上下文.禁止NPC交互;
}

	// ===== 事件 =====
	public event Action<道具数据> 消耗失败;

	// ===== 字段 =====
	public bool 正在等待交互() => _等待交互 || _等待按键;

	private 卡牌战斗管理器 _战斗管理器 => 卡牌战斗管理器.实例;
	private bool _等待按键 = false;
	private 对话界面 _界面;
	private 选项执行器 _执行器;
	private 对话状态存储 _状态存储;
	private bool _待显示场景 = false;
	private string _待显示场景路径;
	private 对话序列 _待显示场景后续对话;
	private List<历史条目> _历史记录 = new List<历史条目>();
	private 场景展示UI _场景展示UI;
	private bool _等待场景展示结束 = false;
	private 对话序列 _场景展示后续对话;

	private 对话序列 当前序列;
	private int 当前句子索引 = 0;
	private bool _对话中 = false;
	public bool 对话进行中 => _对话中;

	private Dictionary<string, bool> 已改变对话记录 = new Dictionary<string, bool>();
	private Dictionary<string, HashSet<int>> 序列已选选项记录 = new Dictionary<string, HashSet<int>>();

	private class 回到选项数据
	{
		public 对话序列 原始序列;
		public int 选项索引;
	}
	private 回到选项数据 _回到选项数据;

	// 交互等待相关
	private bool _等待交互 = false;
	private TaskCompletionSource<bool> _交互等待;
	private 对话句子.交互类型枚举 _当前交互类型;

	// ========== 信号 ==========
	[Signal] public delegate void 请求高亮控件EventHandler(string 控件路径, string 提示文本);
	[Signal] public delegate void 请求播放动画EventHandler(string 动画名称);
	[Signal] public delegate void 对话开始EventHandler();
	[Signal] public delegate void 对话结束EventHandler();

	// ===================== _Ready =====================

	public override void _Ready()
	{
		if (实例 != null) { QueueFree(); return; }
		实例 = this;
		ProcessMode = ProcessModeEnum.Always;

		_当前对话上下文 = new 对话上下文();
		_执行器 = new 选项执行器();

		if (转场管理器.实例 != null)
			转场管理器.实例.转场完成 += 当场切换完成;

		_场景展示UI = GetNodeOrNull<场景展示UI>("/root/场景展示UI");
		if (_场景展示UI != null)
			_场景展示UI.场景展示结束 += 当场景展示结束;
	}

	// ===================== 场景加载 =====================

public void 场景加载完成()
{
	GD.Print($"[对话播放器] 场景加载完成: 等待传送后触发={是否等待传送后触发()}, 待触发的传送后对话={获取待触发传送后对话() != null}");

	// ★ 如果是战斗场景，跳过所有隐藏触发器处理（保留上下文）
	if (GetTree().CurrentScene.IsInGroup("战斗场景"))
	{
		GD.Print("[对话播放器] 当前为战斗场景，跳过隐藏触发器处理，保留上下文");
		// 不处理隐藏，直接进入后面的返回对话等
	}
	else
	{
		// ★ 恢复所有已隐藏的触发器（从存档读取）
		var 进度数据 = 存档管理器.实例?.获取当前状态()?.进度数据;
		if (进度数据 != null && 进度数据.已隐藏触发器名称列表.Count > 0)
		{
			foreach (string 名称 in 进度数据.已隐藏触发器名称列表)
			{
				var 触发器 = GetTree().CurrentScene.FindChild(名称, true, false) as 对话触发器;
				if (触发器 != null && 触发器.Visible)
				{
					触发器.Visible = false;
					触发器.设置碰撞启用(false);
					GD.Print($"[对话播放器] 从存档恢复隐藏触发器: {名称}");
				}
			}
		}

		// ★ 检查战斗胜利后要隐藏的触发器
		string 触发器名称 = 卡牌数据管理器.上下文?.战斗胜利后隐藏触发器名称;
		if (!string.IsNullOrEmpty(触发器名称))
		{
			// 先检查是否已经在存档中标记为已隐藏（防止重复处理）
			bool 已记录隐藏 = 进度数据 != null && 进度数据.是否触发器已隐藏(触发器名称);
			
			if (!已记录隐藏)
			{
				var 触发器 = GetTree().CurrentScene.FindChild(触发器名称, true, false) as 对话触发器;
				if (触发器 != null)
				{
					触发器.Visible = false;
					触发器.设置碰撞启用(false);
					GD.Print($"[对话播放器] 已隐藏对话触发器: {触发器名称}");
					
					// ★ 记录到存档
					进度数据?.记录触发器隐藏(触发器名称);
				}
				else
				{
					GD.PrintErr($"[对话播放器] 找不到对话触发器: {触发器名称}");
					// 不清空变量，保留到下次场景加载尝试
				}
			}
			else
			{
				// 已经记录过隐藏，无需重复操作
				GD.Print($"[对话播放器] 触发器 '{触发器名称}' 已在存档中标记为隐藏，跳过");
			}
			
			// ★ 只在非战斗场景中清空上下文（我们已经处于 else 分支）
			卡牌数据管理器.上下文.战斗胜利后隐藏触发器名称 = "";
		}
	}

	// 优先检查：战斗胜利后返回触发的对话
	if (卡牌数据管理器.上下文?.返回后自动触发对话序列 != null)
	{
		var 对话 = 卡牌数据管理器.上下文.返回后自动触发对话序列;
		卡牌数据管理器.上下文.返回后自动触发对话序列 = null;
		CallDeferred(nameof(延迟开始返回对话), 对话);
		return;
	}

	if (是否等待传送后触发())
		CallDeferred(nameof(延迟开始传送后对话));
}

	private void 延迟开始返回对话(对话序列 对话)
	{
		if (对话 != null && IsInstanceValid(对话))
			开始对话(对话);
	}

	// ===================== 对话控制 =====================

	public void 开始对话(对话序列 序列, bool 保持历史 = false, bool 保留回到选项数据 = false)
	{
		if (序列 == null || _对话中) return;

		if (!保持历史)
			_历史记录.Clear();

		对话序列 实际序列 = 获取实际序列(序列);
		if (实际序列.Sentences.Count == 0) return;

		当前序列 = 实际序列;
		当前句子索引 = 0;
		_对话中 = true;

		if (!保留回到选项数据)
			_回到选项数据 = null;

		_历史记录.Clear();
		显示或创建界面();
		_界面.显示对话面板();
		EmitSignal(SignalName.对话开始);
		显示当前句子();
		禁止玩家移动(true);
	}

	private void 延迟开始传送后对话()
	{
		if (_正在延迟传送后对话) return;
		_正在延迟传送后对话 = true;

		GD.Print("[对话播放器] 延迟开始传送后对话 进入");
		var 玩家组 = GetTree().GetNodesInGroup("玩家");
		foreach (var 节点 in 玩家组)
		{
			if (节点 is 玩家控制器 玩家)
			{
				玩家.开始过场动画();
				GD.Print("[对话播放器] 延迟开始传送后对话，已锁定玩家");
			}
		}

		var 待触发对话 = 获取待触发传送后对话();
		if (待触发对话 != null)
		{
			// ★ 先清除标志，再开始对话
			清除待触发传送后对话();
			// ★ 把 _正在延迟传送后对话 重置放到对话开始之后
			_正在延迟传送后对话 = false;
			开始对话(待触发对话, 保持历史: true);
			GD.Print("[对话播放器] 延迟开始传送后对话 完成");
		}
		else
		{
			_正在延迟传送后对话 = false;
			GD.Print("[对话播放器] 延迟开始传送后对话 失败：没有待触发的对话");
		}
	}

	private void 结束对话()
	{
		_对话中 = false;
		_界面?.隐藏对话面板();

		if (当前序列 != null && 当前序列.可改变对话)
		{
			string 路径 = 当前序列.ResourcePath;
			var 当前状态 = 存档管理器.实例?.获取当前状态();
			if (当前状态 != null)
			{
				当前状态.设置对话改变状态(路径, true);
				GD.Print($"[对话播放器] 记录对话改变到当前状态: {路径}");
			}
		}

		禁止玩家移动(false);
		EmitSignal(SignalName.对话结束);

		if (_待显示场景)
		{
			_待显示场景 = false;
			显示场景UI(_待显示场景路径, _待显示场景后续对话);
			return;
		}

		// 战斗触发 → 调用战斗服务
		if (!string.IsNullOrEmpty(卡牌数据管理器.上下文?.待触发战斗场景路径))
		{
			string 战斗场景路径 = 卡牌数据管理器.上下文.待触发战斗场景路径;
			转场动画资源 转场动画 = 卡牌数据管理器.上下文.待触发战斗转场动画;
			卡牌数据管理器.上下文.待触发战斗场景路径 = "";
			卡牌数据管理器.上下文.待触发战斗转场动画 = null;

			GD.Print($"[对话播放器] 目标序列结束，触发战斗: {战斗场景路径}");
			战斗服务.触发战斗(战斗场景路径, 转场动画);
			return;
		}
	}

	private void 显示或创建界面()
	{
		if (_界面 == null || !IsInstanceValid(_界面))
		{
			var 界面场景 = GD.Load<PackedScene>("res://对话系统/脚本/对话UI.tscn");
			_界面 = 界面场景.Instantiate<对话界面>();
			GetTree().CurrentScene.AddChild(_界面);
			_界面.请求下一句 += 推进对话;
			_界面.选项被选择 += 选择选项;
		}
		_界面.显示对话面板();
	}

	// ===================== 句子显示 =====================

	private void 显示当前句子()
	{
		_等待交互 = false;
		_等待按键 = false;
		var 句子 = 当前序列.Sentences[当前句子索引];
		处理句子跟随者操作(句子);

		if (句子.交互类型 != 对话句子.交互类型枚举.等待行动条满)
		{
			string 显示文本 = 替换玩家名字(句子.Text);
			if (!string.IsNullOrEmpty(显示文本))
			{
				string 说话人显示 = 替换玩家名字(句子.SpeakerName);
				if (string.IsNullOrEmpty(说话人显示)) 说话人显示 = "未知";
				string 历史文本 = 显示文本.Replace("/", " ");
				_历史记录.Add(new 历史条目 { 说话人 = 说话人显示, 文本 = 历史文本 });
			}
			float? 自定义速度 = null;
			if (句子.StyleProperties != null && 句子.StyleProperties.TryGetValue("text_speed", out Variant 速度))
				自定义速度 = (float)速度;
			_界面?.显示句子(显示文本, 替换玩家名字(句子.SpeakerName), 句子.Avatar, 自定义速度);
		}
		else
		{
			_界面?.隐藏对话面板();
		}

		_ = 处理交互并等待文字显示(句子);
	}

	private async Task 处理交互并等待文字显示(对话句子 句子)
	{
		Task 文字完成 = 句子.交互类型 != 对话句子.交互类型枚举.等待行动条满 ? 等待文字显示完成() : Task.CompletedTask;
		Task 交互完成 = Task.CompletedTask;

		if (句子.交互类型 != 对话句子.交互类型枚举.无)
		{
			_等待交互 = true;
			_交互等待 = new TaskCompletionSource<bool>();
			_ = 执行交互(句子);
			交互完成 = _交互等待.Task;
		}

		await Task.WhenAll(文字完成, 交互完成);
		_等待交互 = false;

		if (句子.交互类型 == 对话句子.交互类型枚举.无 ||
			句子.交互类型 == 对话句子.交互类型枚举.高亮或动画)
		{
			_等待按键 = true;
			await 等待交互按键();
			_等待按键 = false;
		}

		推进对话();
	}

	private async Task 执行交互(对话句子 句子)
	{
		switch (句子.交互类型)
		{
			case 对话句子.交互类型枚举.高亮或动画:
				if (!string.IsNullOrEmpty(句子.预定义动画名称))
				{
					EmitSignal(SignalName.请求播放动画, 句子.预定义动画名称);
					await _交互等待.Task;
				}
				if (!string.IsNullOrEmpty(句子.高亮控件路径))
				{
					EmitSignal(SignalName.请求高亮控件, 句子.高亮控件路径, 句子.高亮提示文本);
				}
				break;

			case 对话句子.交互类型枚举.等待行动条满:
				_战斗管理器?.强制恢复战斗();
				while (_战斗管理器?.玩家单位 == null || _战斗管理器.玩家单位.行动条 < 100)
				{
					await ToSignal(GetTree(), "process_frame");
				}
				_战斗管理器?.强制暂停战斗();

				_界面?.显示对话面板();
				string 显示文本 = 替换玩家名字(句子.Text);
				if (!string.IsNullOrEmpty(显示文本))
				{
					string 说话人显示 = 替换玩家名字(句子.SpeakerName);
					if (string.IsNullOrEmpty(说话人显示)) 说话人显示 = "未知";
					_历史记录.Add(new 历史条目 { 说话人 = 说话人显示, 文本 = 显示文本 });
				}
				float? 自定义速度 = null;
				if (句子.StyleProperties != null && 句子.StyleProperties.TryGetValue("text_speed", out Variant 速度))
					自定义速度 = (float)速度;
				_界面?.显示句子(显示文本, 替换玩家名字(句子.SpeakerName), 句子.Avatar, 自定义速度);

				await 等待文字显示完成();
				await 等待交互按键();
				_战斗管理器?.强制恢复战斗();
				完成当前交互();
				break;
		}
	}

	private async Task 等待行动条满()
	{
		_战斗管理器?.强制恢复战斗();
		while (_战斗管理器?.玩家单位 == null || _战斗管理器.玩家单位.行动条 < 100)
		{
			await ToSignal(GetTree(), "process_frame");
		}
	}

	private async Task 等待文字显示完成()
	{
		if (_界面 == null) return;
		var tcs = new TaskCompletionSource<bool>();
		对话界面.文字显示完成EventHandler 回调 = null;
		回调 = () => {
			_界面.文字显示完成 -= 回调;
			tcs.SetResult(true);
		};
		_界面.文字显示完成 += 回调;
		await tcs.Task;
	}

	private async Task 等待交互按键()
	{
		while (!Input.IsActionJustPressed("交互"))
			await ToSignal(GetTree(), "process_frame");

		while (Input.IsActionJustPressed("交互"))
			await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
	}

	public void 完成当前交互()
	{
		if (_等待交互 && _交互等待 != null && !_交互等待.Task.IsCompleted)
		{
			_交互等待.SetResult(true);
		}
	}

	private void 推进对话()
	{
		GD.Print($"[推进对话] 当前索引={当前句子索引}, 总数={当前序列.Sentences.Count}, _等待交互={_等待交互}");
		if (_等待交互) return;

		当前句子索引++;
		if (当前句子索引 < 当前序列.Sentences.Count)
		{
			显示当前句子();
		}
		else
		{
			if (当前序列.Options != null && 当前序列.Options.Count > 0)
			{
				var 可显示的选项 = new List<分支选项>();
				var 已选索引集合 = 获取当前序列已选选项();
				for (int i = 0; i < 当前序列.Options.Count; i++)
					可显示的选项.Add(当前序列.Options[i]);

				if (可显示的选项.Count > 0)
					_界面?.显示选项(可显示的选项, 替换玩家名字, null);
				else
					结束对话();
			}
			else
			{
				if (_回到选项数据 != null)
					返回到原始选项();
				else
					结束对话();
			}
		}
	}

	private void 返回到原始选项()
	{
		if (_回到选项数据 == null) return;
		var 原始序列 = _回到选项数据.原始序列;
		int 已选索引 = _回到选项数据.选项索引;
		当前序列 = 原始序列;
		var 已选索引集合 = 获取当前序列已选选项();
		var 可显示的选项 = new List<分支选项>();

		for (int i = 0; i < 原始序列.Options.Count; i++)
		{
			var opt = 原始序列.Options[i];
			if (!opt.是否回到选项 && 已选索引集合.Contains(i))
				continue;
			可显示的选项.Add(opt);
		}

		if (可显示的选项.Count > 0)
			_界面?.显示选项(可显示的选项, 替换玩家名字, null);
		else
			结束对话();

		_回到选项数据 = null;
	}

	private void 开始对话并回到选项(对话序列 目标序列, 对话序列 原始序列, int 选项索引)
	{
		_回到选项数据 = new 回到选项数据 { 原始序列 = 原始序列, 选项索引 = 选项索引 };
		开始对话(目标序列, 保持历史: false, 保留回到选项数据: true);
	}

	// ===================== 选项选择 =====================

	public async void 选择选项(int 选项索引)
	{
		if (!_对话中 || 当前序列 == null) return;
		if (选项索引 < 0 || 选项索引 >= 当前序列.Options.Count) return;

		分支选项 选项 = 当前序列.Options[选项索引];

		if (选项.允许存档)
		{
			if (全局存档UI管理器.实例 == null)
			{
				GD.PrintErr("全局存档UI管理器未初始化");
				return;
			}
			结束对话();
			await 全局存档UI管理器.实例.请求保存并等待();
			return;
		}

		记录已选选项(选项索引);
		刷新选项列表(选项索引);
		var (结果, 场景路径, 后续对话) = _执行器.执行(选项, 当前序列, 选项索引);

		switch (结果)
		{
			case 选项执行器.执行结果.继续对话:
				if (后续对话 != null)
				{
					_对话中 = false;
					_界面?.隐藏对话面板();
					if (选项.是否回到选项)
						开始对话并回到选项(后续对话, 当前序列, 选项索引);
					else
						开始对话(后续对话, 保持历史: true);
				}
				else
					结束对话();
				break;

			case 选项执行器.执行结果.结束对话:
				结束对话();
				break;

			case 选项执行器.执行结果.切换场景:
				结束对话();
				if (转场管理器.实例 != null)
				{
					int 当前存档位 = 卡牌数据管理器.当前存档位;
					转场管理器.实例.开始转场(
						场景路径,
						选项.转场动画配置,
						null,
						false,
						false,
						null,
						false,
						null,
						当前存档位,
						选项.目标传送点名称
					);
				}
				else
					GetTree().ChangeSceneToFile(场景路径);
				break;

			case 选项执行器.执行结果.显示场景UI:
				显示场景UI(场景路径, 后续对话);
				break;
		}
	}

	private void 刷新选项列表(int 排除选项索引)
	{
		var 已选索引集合 = 获取当前序列已选选项();
		var 可显示的选项 = new List<分支选项>();
		for (int i = 0; i < 当前序列.Options.Count; i++)
		{
			var opt = 当前序列.Options[i];
			可显示的选项.Add(opt);
		}

		if (可显示的选项.Count > 0)
			_界面?.显示选项(可显示的选项, 替换玩家名字, null);
		else
			_界面?.隐藏对话面板();
	}

	// ===================== 条件服务 =====================

	private 对话序列 获取实际序列(对话序列 原始序列)
	{
		if (原始序列 == null) return null;

		GD.Print($"[调试] 获取实际序列: {原始序列.ResourcePath}");
		GD.Print($"[调试] 可改变对话: {原始序列.可改变对话}");

		var 当前状态 = 存档管理器.实例?.获取当前状态();
		if (当前状态 != null)
		{
			bool 已改变 = 当前状态.获取对话改变状态(原始序列.ResourcePath);
			GD.Print($"[调试] 已改变状态: {已改变}");
		}

		return 条件服务.获取实际序列(原始序列);
	}

	public void 触发消耗失败(道具数据 需要道具)
	{
		消耗失败?.Invoke(需要道具);
	}

	// ===================== 辅助方法 =====================

	private void 记录已选选项(int 索引)
	{
		if (当前序列 == null) return;
		string key = 当前序列.ResourcePath;
		if (!序列已选选项记录.ContainsKey(key))
			序列已选选项记录[key] = new HashSet<int>();
		序列已选选项记录[key].Add(索引);
	}

	private HashSet<int> 获取当前序列已选选项()
	{
		if (当前序列 == null) return new HashSet<int>();
		string key = 当前序列.ResourcePath;
		if (!序列已选选项记录.ContainsKey(key))
			序列已选选项记录[key] = new HashSet<int>();
		return 序列已选选项记录[key];
	}

	private string 替换玩家名字(string 原文)
	{
		if (string.IsNullOrEmpty(原文)) return 原文;
		string 名字 = 玩家数据管理器.实例?.玩家名字 ?? "冒险者";
		return 原文.Replace("{玩家名字}", 名字);
	}

	private void 处理句子跟随者操作(对话句子 句子)
	{
		var 玩家组 = GetTree().GetNodesInGroup("玩家");
		if (玩家组.Count == 0) return;
		var 玩家 = 玩家组[0] as 玩家控制器;
		if (玩家 == null) return;

		if (句子.移除所有跟随者)
			玩家.移除所有跟随者();

		if (句子.添加的跟随者预制体列表 != null && 句子.添加的跟随者预制体列表.Count > 0)
		{
			foreach (var 预制体 in 句子.添加的跟随者预制体列表)
				if (预制体 != null)
					玩家.添加跟随者(预制体, 2);
		}
	}

	private void 禁止玩家移动(bool 禁止)
	{
		var 玩家组 = GetTree().GetNodesInGroup("玩家");
		foreach (var 节点 in 玩家组)
		{
			if (节点 is 玩家控制器 玩家)
			{
				if (禁止)
					玩家.开始过场动画();
				else
					玩家.结束过场动画();
			}
		}
	}

	public void 临时设置对话进行中(bool active)
	{
		if (_对话中 == active) return;
		_对话中 = active;
		if (!active)
			禁止玩家移动(false);
		else
			禁止玩家移动(true);
		GD.Print($"[对话播放器] 临时设置对话进行中 = {_对话中}");
	}

	public void 设置待显示场景(string 场景路径, 对话序列 后续对话)
	{
		_待显示场景 = true;
		_待显示场景路径 = 场景路径;
		_待显示场景后续对话 = 后续对话;
	}

	private void 显示场景UI(string 场景路径, 对话序列 后续对话)
	{
		if (_场景展示UI == null)
		{
			GD.PrintErr("找不到场景展示UI");
			结束对话();
			return;
		}
		_界面.隐藏对话面板();
		_对话中 = false;
		_等待场景展示结束 = true;
		_场景展示后续对话 = 后续对话;
		_场景展示UI.显示场景(场景路径, 后续对话, true);
	}

	private void 当场景展示结束(对话序列 后续对话)
	{
		_等待场景展示结束 = false;
		_对话中 = false;
		if (后续对话 != null)
			开始对话(后续对话);
		else
			结束对话();
	}

	private void 当场切换完成(string 新场景路径)
	{
		if (是否等待传送后触发())
			CallDeferred(nameof(延迟开始传送后对话));
	}

	public void 重置传送后对话记录()
	{
		var 当前状态 = 存档管理器.实例?.获取当前状态();
		if (当前状态 != null)
		{
			当前状态.清空所有对话记录();
			GD.Print("[对话播放器] 已清空所有传送后对话触发记录");
		}
	}

	public async Task 开始对话并等待(对话序列 序列)
	{
		var tcs = new TaskCompletionSource<bool>();
		对话结束EventHandler 结束回调 = null;
		结束回调 = () => {
			对话结束 -= 结束回调;
			tcs.SetResult(true);
		};
		对话结束 += 结束回调;
		开始对话(序列);
		await tcs.Task;
	}

	public List<历史条目> 获取历史记录() => new List<历史条目>(_历史记录);
}
