using Godot;
using System.Collections.Generic;
using 你的项目.Scripts.角色;

public partial class 跟随者控制器 : CharacterBody2D
{
	[Export] public AnimatedSprite2D 动画精灵 { get; set; }
	[Export] public float 跟随距离 = 50f;            // 与玩家保持的距离
	[Export] public float 最大速度 = 200f;            // 最大移动速度
	[Export] public float 加速速度 = 300f;            // 加速时的速度
	[Export] public float 停止距离 = 5f;              // 距离小于此值时停止移动
	[Export] public string 预制体资源路径 { get; set; } = "";

	private 玩家控制器 _玩家;
	private Vector2 _目标位置;
	private Vector2 _玩家速度; // 用于预测玩家运动方向
	private Vector2 _上一帧玩家位置;
	private Vector2 _lastDirection = Vector2.Down;
	private float _当前速度;

	public override void _Ready()
	{
		if (动画精灵 == null)
			动画精灵 = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		AddToGroup("跟随者");

		_玩家 = GetTree().GetFirstNodeInGroup("玩家") as 玩家控制器;
		if (_玩家 == null)
		{
			SetPhysicsProcess(false);
			return;
		}

		GlobalPosition = _玩家.GlobalPosition;
		_目标位置 = GlobalPosition;
		_上一帧玩家位置 = _玩家.GlobalPosition;
		ZIndex = 10;
		_当前速度 = 最大速度;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_玩家 == null || !IsInstanceValid(_玩家))
		{
			_玩家 = GetTree().GetFirstNodeInGroup("玩家") as 玩家控制器;
			if (_玩家 == null) return;
		}

		// 计算玩家移动方向
		Vector2 玩家当前位置 = _玩家.GlobalPosition;
		Vector2 玩家速度 = (玩家当前位置 - _上一帧玩家位置) / (float)delta;
		_上一帧玩家位置 = 玩家当前位置;

		// 计算理想位置：玩家位置 - 玩家朝向的反方向 * 跟随距离
		Vector2 玩家朝向 = 玩家速度.Length() > 10f ? 玩家速度.Normalized() : _lastDirection;
		if (玩家朝向 == Vector2.Zero) 玩家朝向 = Vector2.Down;

		// 为了更自然，跟随位置稍微偏向玩家朝向的反方向
		Vector2 理想位置 = 玩家当前位置 - 玩家朝向 * 跟随距离;

		// 如果玩家速度很小（静止），则直接站在玩家身后
		if (玩家速度.LengthSquared() < 1f)
			理想位置 = 玩家当前位置 - _lastDirection * 跟随距离;

		// 计算到理想位置的方向和距离
		Vector2 方向 = 理想位置 - GlobalPosition;
		float 距离 = 方向.Length();

		// 如果距离很小，不移动
		if (距离 < 停止距离)
		{
			_当前速度 = 0;
			// 但是要记录朝向，用于空闲动画
			if (方向.LengthSquared() > 0.1f)
				_lastDirection = 方向.Normalized();
			播放空闲动画(_lastDirection);
			return;
		}

		// 根据距离调整速度（近时慢，远时快）
		float 速度因子 = Mathf.Clamp(距离 / 跟随距离, 0.3f, 1.5f);
		float 目标速度 = Mathf.Lerp(最大速度, 加速速度, 速度因子);
		_当前速度 = Mathf.MoveToward(_当前速度, 目标速度, (float)delta * 500f); // 平滑加速

		// 移动
		Vector2 移动方向 = 方向.Normalized();
		Vector2 移动量 = 移动方向 * _当前速度 * (float)delta;
		GlobalPosition += 移动量;

		// 更新方向并播放动画
		if (移动量.LengthSquared() > 0.01f)
		{
			_lastDirection = 移动方向;
			bool 加速中 = _当前速度 > 最大速度 * 0.8f;
			播放行走动画(移动方向, 加速中);
		}
		else
		{
			播放空闲动画(_lastDirection);
		}
	}

	/// <summary>
	/// 传送或重置时，跟随者直接跳到玩家位置后方指定距离
	/// </summary>
	public void 重新初始化()
	{
		if (_玩家 == null) return;
		Vector2 玩家位置 = _玩家.GlobalPosition;
		// 计算玩家朝向（如果有速度则用速度方向，否则用上次方向）
		Vector2 玩家朝向 = _lastDirection;
		if (_玩家.当前移动速度 > 10f)
			玩家朝向 = _玩家.当前移动速度 > 0 ? _玩家.上次输入方向 : _lastDirection;
		GlobalPosition = 玩家位置 - 玩家朝向 * 跟随距离;
		_目标位置 = GlobalPosition;
		_当前速度 = 0;
		播放空闲动画(_lastDirection);
	}

	// ---------- 动画方法 ----------
	private void 播放行走动画(Vector2 方向, bool 加速中)
	{
		if (动画精灵 == null) return;

		string 前缀 = 加速中 ? "run" : "walk";
		string 后缀 = "left";
		bool 翻转 = false;

		if (方向.Y > 0)          // 向下
			后缀 = "down";
		else if (方向.Y < 0)     // 向上
			后缀 = "up";
		else if (方向.X > 0)     // 向右 → 使用 left 并翻转
		{
			后缀 = "left";
			翻转 = true;
		}
		else if (方向.X < 0)     // 向左 → 使用 left 不翻转
		{
			后缀 = "left";
			翻转 = false;
		}

		string 动画名 = 前缀 + "_" + 后缀;

		if (动画精灵.SpriteFrames != null && 动画精灵.SpriteFrames.HasAnimation(动画名))
		{
			动画精灵.Play(动画名);
			动画精灵.FlipH = 翻转;
		}
		else
		{
			string 回退名 = 前缀 + "_left";
			if (动画精灵.SpriteFrames.HasAnimation(回退名))
			{
				动画精灵.Play(回退名);
				动画精灵.FlipH = (方向.X > 0);
			}
		}

		// 调整动画速度
		float 比例 = _当前速度 / 最大速度;
		动画精灵.SpeedScale = Mathf.Clamp(比例, 0.5f, 1.5f);
	}

	private void 播放空闲动画(Vector2 方向)
	{
		if (动画精灵 == null) return;

		string 后缀 = "down";
		bool 翻转 = false;

		if (方向.Y > 0)
			后缀 = "down";
		else if (方向.Y < 0)
			后缀 = "up";
		else if (方向.X > 0)
		{
			后缀 = "left";
			翻转 = true;
		}
		else if (方向.X < 0)
		{
			后缀 = "left";
			翻转 = false;
		}

		string 动画名 = "idle_" + 后缀;
		if (动画精灵.SpriteFrames != null && 动画精灵.SpriteFrames.HasAnimation(动画名))
		{
			动画精灵.Play(动画名);
			动画精灵.FlipH = 翻转;
		}
		else
		{
			string 回退名 = "idle_left";
			if (动画精灵.SpriteFrames.HasAnimation(回退名))
			{
				动画精灵.Play(回退名);
				动画精灵.FlipH = (方向.X > 0);
			}
			else
			{
				string 最后回退 = "walk_left";
				if (动画精灵.SpriteFrames.HasAnimation(最后回退))
				{
					动画精灵.Play(最后回退);
					动画精灵.Pause();
					动画精灵.Frame = 0;
					动画精灵.FlipH = (方向.X > 0);
				}
			}
		}
	}
}
