using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class 教程战斗控制器 : Node
{
	[Export] private 卡牌战斗管理器 _战斗管理器;
	[Export] private 高亮指引管理器 _高亮管理器;
	[Export] private AnimationPlayer _动画播放器;

	private 对话播放器 _对话播放器 => 对话播放器.实例;

	private 卡牌战斗单位 _玩家;
	private List<卡牌战斗单位> _怪物队伍;

	// 教程抽牌队列（用于委托）
	private Queue<卡牌实例> _玩家抽牌队列 = new Queue<卡牌实例>();
	private Queue<卡牌实例> _敌人抽牌队列 = new Queue<卡牌实例>();

	public override void _Ready()
	{
		if (_对话播放器 == null)
		{
			GD.PrintErr("[教程战斗控制器] 对话播放器单例未找到");
			return;
		}

		if (_动画播放器 == null)
		{
			GD.PrintErr("[教程战斗控制器] 未绑定 AnimationPlayer 节点，请在检查器中拖入");
		}

		_对话播放器.请求高亮控件 += On请求高亮控件;
		_对话播放器.请求播放动画 += On请求播放动画;
	}

	private async void On请求播放动画(string 动画名称)
	{
		if (_动画播放器 == null)
		{
			GD.PrintErr("[教程] 动画播放器为空");
			_对话播放器.完成当前交互();
			return;
		}

		if (_动画播放器.HasAnimation(动画名称))
		{
			_高亮管理器.显示洞图片();
			_动画播放器.Play(动画名称);
			await ToSignal(_动画播放器, AnimationPlayer.SignalName.AnimationFinished);
			_对话播放器.完成当前交互();
		}
		else
		{
			GD.PrintErr($"[教程] 未找到动画: {动画名称}");
			_对话播放器.完成当前交互();
		}
	}

	public async void 开始教程战斗(教程战斗配置 配置, 卡牌战斗单位 玩家, List<卡牌战斗单位> 怪物)
	{
		_玩家 = 玩家;
		_怪物队伍 = 怪物;

		// ---- 设置自定义抽牌逻辑（替代原来直接调用单位的教程方法） ----

		// 构建玩家抽牌队列
		_玩家抽牌队列.Clear();
		if (配置.玩家抽牌序列 != null && 配置.玩家抽牌序列.Count > 0)
		{
			foreach (var 卡牌数据 in 配置.玩家抽牌序列)
			{
				if (卡牌数据 == null) continue;
				_玩家抽牌队列.Enqueue(new 卡牌实例(卡牌数据));
			}
		}

		// 构建敌人抽牌队列（如果有多个敌人，每个都共享同一个队列？根据原逻辑每个怪都用同一个序列，可以，但要确保每个怪独立引用）
		// 注意：原代码用同一个队列给所有怪物，会导致抽牌顺序在各怪物间共享（即第一个怪抽了牌后，第二个怪从队列下一个取）
		// 如果你希望每个怪物独立队列，需要为每个怪物单独克隆。但原逻辑是共享，我们保持。
		_敌人抽牌队列.Clear();
		if (配置.敌人抽牌序列 != null && 配置.敌人抽牌序列.Count > 0 && 怪物.Count > 0)
		{
			foreach (var 卡牌数据 in 配置.敌人抽牌序列)
			{
				if (卡牌数据 == null) continue;
				_敌人抽牌队列.Enqueue(new 卡牌实例(卡牌数据));
			}
		}

		// 设置自定义抽牌委托
		_战斗管理器.自定义抽牌逻辑 = (卡牌战斗单位 单位) =>
		{
			// 判断是玩家还是怪物
			if (单位 == _玩家)
			{
				// 从玩家队列取牌
				if (_玩家抽牌队列.Count > 0)
				{
					GD.Print($"[教程] 玩家抽到预设卡牌，剩余 {_玩家抽牌队列.Count - 1} 张");
					return _玩家抽牌队列.Dequeue();
				}
				// 队列为空，返回 null，让正常抽牌处理
				return null;
			}
			else
			{
				// 怪物（注意：如果是多个怪物，它们共享同一个敌人队列）
				if (_敌人抽牌队列.Count > 0)
				{
					GD.Print($"[教程] 怪物抽到预设卡牌，剩余 {_敌人抽牌队列.Count - 1} 张");
					return _敌人抽牌队列.Dequeue();
				}
				return null;
			}
		};

		// ---- 开始战斗 ----
		_战斗管理器.开始战斗();
		_战斗管理器.强制暂停战斗();
		_战斗管理器.设置玩家交互启用(false);

		if (配置.教程对话序列 == null)
		{
			GD.PrintErr("[教程] 未设置教程对话序列");
			// 即使没有对话，也要确保战斗可以正常进行，清除委托并恢复
			清理教程();
			return;
		}

		// 等待对话完成
		await _对话播放器.开始对话并等待(配置.教程对话序列);

		// 对话完成后，清理教程状态（清除委托，恢复正常战斗）
		清理教程();
	}

	/// <summary>
	/// 清理教程状态，恢复正常战斗
	/// </summary>
	private void 清理教程()
	{
		// 清除自定义抽牌逻辑，恢复随机抽牌
		_战斗管理器.自定义抽牌逻辑 = null;

		// 清空队列（释放引用）
		_玩家抽牌队列.Clear();
		_敌人抽牌队列.Clear();

		// 恢复战斗（如果还在暂停状态）
		_战斗管理器.强制恢复战斗();

		// 确保玩家交互启用（如果已被禁用）
		_战斗管理器.设置玩家交互启用(true);

		GD.Print("[教程] 教程清理完成，恢复正常战斗");
	}

	private async void On请求高亮控件(string 控件路径, string 提示文本)
	{
		_高亮管理器.显示高亮控件(控件路径);
		if (!string.IsNullOrEmpty(提示文本))
			_战斗管理器.添加战斗日志(提示文本);
	}
}
