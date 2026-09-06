using Godot;
using System;
using 你的项目.Scripts.角色;  // 添加这行
public partial class 可燃烧物 : StaticBody2D
{
	// 是否已经被触发
	private bool 已触发 = false;
	
	// 节点引用
	private Area2D 检测区域;
	private AnimatedSprite2D 动画精灵;
	private CollisionShape2D 碰撞形状;

	public override void _Ready()
	{
		// 获取节点引用
		检测区域 = GetNode<Area2D>("Area2D");
		动画精灵 = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		碰撞形状 = GetNode<CollisionShape2D>("CollisionShape2D");
		
		// 连接信号
		检测区域.BodyEntered += 当物体进入检测区域;
		
		// 设置初始动画
		if (动画精灵 != null)
		{
			动画精灵.Play("idle");
		}
		
		GD.Print("可燃烧物已就绪");
	}

	// 当有物体进入检测区域
	private void 当物体进入检测区域(Node2D 物体)
	{
		// 如果已经触发过，不再处理
		if (已触发) return;
		
		// 检查是否是玩家
		if (物体.IsInGroup("玩家"))
		{
			GD.Print("检测到玩家进入");
			
			// 获取玩家控制器
			玩家控制器 玩家 = 物体 as 玩家控制器;
			
			if (玩家 != null)
			{
				// 检查玩家是否处于燃烧状态
				if (玩家.燃烧)
				{
					GD.Print("玩家处于燃烧状态，触发消失");
					触发消失();
				}
				else
				{
					GD.Print("玩家未燃烧，无法通过");
					// 这里可以添加提示效果，比如闪烁
					播放提示动画();
				}
			}
		}
	}

	// 触发消失动画
	private async void 触发消失()
	{
		已触发 = true;
		
		GD.Print("开始播放消失动画");
		
		// 播放消失动画
		if (动画精灵 != null)
		{
			动画精灵.Play("disappear");
			
			// 等待动画完成
			await ToSignal(动画精灵, "animation_finished");
			
			GD.Print("消失动画播放完成，移除可燃烧物");
			
			// 动画完成后移除节点
			QueueFree();
		}
		else
		{
			GD.Print("错误：未找到AnimatedSprite2D，直接移除");
			QueueFree();
		}
	}
	
	// 播放提示动画（当玩家未燃烧时）
	private void 播放提示动画()
	{
		// 这里可以添加一个闪烁效果或其他提示
		// 例如：短暂改变颜色或播放一个提示动画
		
		// 简单实现：创建一个临时动画
		var 动画 = CreateTween();
		动画.TweenProperty(动画精灵, "modulate", new Color(1, 0.5f, 0.5f, 1), 0.1f);
		动画.TweenProperty(动画精灵, "modulate", new Color(1, 1, 1, 1), 0.1f);
		动画.SetLoops(2);
	}
}
