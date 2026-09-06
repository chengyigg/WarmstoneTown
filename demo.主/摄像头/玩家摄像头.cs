using Godot;
using System.Threading.Tasks;
using System;

namespace 你的项目.Scripts.摄像机
{
	public partial class 玩家摄像头 : Camera2D
	{
		[Export] public Node2D 目标玩家 { get; set; }
		[Export] public float 跟随平滑度 = 0.1f;      // 保留但不再用于平滑，改用速度方式
		[Export] public Vector2 位置偏移 = Vector2.Zero;
		[Export] public bool 启用边界限制 = false;
		[Export] public Rect2 边界限制 = new Rect2(0, 0, 1920, 1080);
		public bool 禁止自动激活 = false;
		
		[Export] public bool 启用预测 = false;         // 建议关闭，可导致抖动
		[Export] public float 预测距离 = 50f;
		
		[Export] public Vector2 初始缩放 { get; set; } = Vector2.One;
		[Export] public bool 启用缩放保护 { get; set; } = true;
		
		[Export] public float 最大跟随速度 = 800f;     // 新增：摄像头每秒最大移动像素（可调）
		
		private Vector2 _目标位置;
		private Vector2 _玩家速度;
		private Vector2 _上一帧玩家位置;
		private bool _已初始化 = false;
		private CollisionShape2D _当前边界节点;

		public override void _Ready()
		{
			 GD.Print($"[玩家摄像头] _Ready 被调用，路径: {GetPath()}");
GD.Print($"调用堆栈:\n{System.Environment.StackTrace}");
			Zoom = 初始缩放;
			
			if (目标玩家 == null)
				自动查找玩家();
			
			if (目标玩家 != null)
			{
				GlobalPosition = 目标玩家.GlobalPosition + 位置偏移;
				_目标位置 = GlobalPosition;
				_上一帧玩家位置 = 目标玩家.GlobalPosition;
			}
			
			MakeCurrent();
			_已初始化 = true;
		}

  public override void _Process(double delta)
		{
			if (启用缩放保护) 保护缩放值();
			
			// ⭐ 修改：无条件尝试更新边界（但内部会根据是否存在边界节点决定启用/禁用）
			更新边界限制();
			
			if (!禁止自动激活 && Enabled && IsInsideTree() && !IsCurrent())
				MakeCurrent();
		}

		public override void _PhysicsProcess(double delta)
		{
			if (目标玩家 == null)
			{
				自动查找玩家();
				if (目标玩家 == null) return;
			}
			
			更新玩家速度();
			计算目标位置();
			平滑移动到目标位置(delta);   // 传入 delta
			应用边界限制();
		}

		private void 保护缩放值()
		{
			if (Zoom != 初始缩放)
				Zoom = 初始缩放;
		}

		private void 自动查找玩家()
		{
			var 玩家组 = GetTree().GetNodesInGroup("玩家");
			if (玩家组.Count > 0)
				目标玩家 = 玩家组[0] as Node2D;
			else
				目标玩家 = GetTree().CurrentScene.GetNodeOrNull<Node2D>("Player");
			if (目标玩家 == null)
				GD.PrintErr("无法找到玩家节点!");
		}

		private void 更新玩家速度()
		{
			Vector2 当前位置 = 目标玩家.GlobalPosition;
			_玩家速度 = (当前位置 - _上一帧玩家位置) / (float)GetPhysicsProcessDeltaTime();
			_上一帧玩家位置 = 当前位置;
		}

		private void 计算目标位置()
		{
			_目标位置 = 目标玩家.GlobalPosition + 位置偏移;
			if (启用预测 && _玩家速度.Length() > 10f)
			{
				Vector2 预测方向 = _玩家速度.Normalized();
				_目标位置 += 预测方向 * 预测距离 * (_玩家速度.Length() / 300f);
			}
		}

		// 修改为基于速度的平滑跟随，避免抖动
		private void 平滑移动到目标位置(double delta)
		{
			Vector2 方向 = _目标位置 - GlobalPosition;
			float 距离 = 方向.Length();
			if (距离 == 0) return;
			
			// 每帧最多移动 最大跟随速度 * delta
			float 最大移动距离 = 最大跟随速度 * (float)delta;
			if (距离 <= 最大移动距离)
			{
				GlobalPosition = _目标位置;
			}
			else
			{
				GlobalPosition += 方向 * (最大移动距离 / 距离);
			}
		}

		private void 应用边界限制()
{
	if (!启用边界限制) return;

	Vector2 限制位置 = GlobalPosition;
	Vector2 摄像头尺寸 = GetViewportRect().Size / Zoom;
	Vector2 半尺寸 = 摄像头尺寸 / 2;

	// 计算边界范围
	float minX = 边界限制.Position.X + 半尺寸.X;
	float maxX = 边界限制.End.X - 半尺寸.X;
	float minY = 边界限制.Position.Y + 半尺寸.Y;
	float maxY = 边界限制.End.Y - 半尺寸.Y;

	// ⭐ 修正：如果左 > 右，说明边界太窄，直接锁定在中间
	if (minX > maxX)
	{
		float centerX = (边界限制.Position.X + 边界限制.End.X) / 2;
		minX = maxX = centerX;   // 让 X 只能呆在中间
	}
	if (minY > maxY)
	{
		float centerY = (边界限制.Position.Y + 边界限制.End.Y) / 2;
		minY = maxY = centerY;   // 让 Y 只能呆在中间
	}

	// 现在 clamp 一定不会报错
	限制位置.X = Mathf.Clamp(限制位置.X, minX, maxX);
	限制位置.Y = Mathf.Clamp(限制位置.Y, minY, maxY);
	GlobalPosition = 限制位置;
}

 // ⭐ 修改：更新边界限制，不再依赖 `启用边界限制` 开关，而是自动检测
		private void 更新边界限制()
		{
			// 如果已经有有效边界节点，直接读取其矩形
			if (_当前边界节点 != null && IsInstanceValid(_当前边界节点) && _当前边界节点.IsInsideTree())
			{
				读取当前边界矩形();
				// 确保启用边界限制为 true（因为存在有效边界）
				if (!启用边界限制) 启用边界限制 = true;
				return;
			}

			// 没有边界节点或节点无效，尝试查找
			if (查找并设置边界())
			{
				// 找到边界，启用限制
				启用边界限制 = true;
			}
			else
			{
				// 没找到边界，禁用限制
				if (启用边界限制) 启用边界限制 = false;
				_当前边界节点 = null;
				// 可选清除边界矩形，但也可保留上次值，这里保持不动
			}
		}
 // 查找并设置边界，返回是否找到有效边界
		private bool 查找并设置边界()
		{
			Node currentScene = GetTree().CurrentScene;
			if (currentScene == null)
				return false;

			Node boundaryNode = currentScene.GetNodeOrNull<Node>("边界");
			if (boundaryNode == null)
				return false;

			CollisionShape2D shape = boundaryNode.GetChild<CollisionShape2D>(0);
			if (shape == null || shape.Shape is not RectangleShape2D rectShape)
			{
				GD.PrintErr($"边界节点 '边界' 下没有找到 CollisionShape2D 或形状不是矩形！");
				return false;
			}

			_当前边界节点 = shape;
			读取当前边界矩形();
			return true;
		}



 private void 读取当前边界矩形()
		{
			if (_当前边界节点 == null) return;
			if (_当前边界节点.Shape is RectangleShape2D rectShape)
			{
				Vector2 extents = rectShape.Size / 2;
				Vector2 center = _当前边界节点.GlobalPosition;
				Vector2 topLeft = center - extents;
				Vector2 size = rectShape.Size;
				Rect2 newBounds = new Rect2(topLeft, size);
				if (边界限制 != newBounds)
				{
					边界限制 = newBounds;
				}
				// 注意：这里不再设置 启用边界限制 = true，由调用方决定
			}
		}
		
		   // ⭐ 新增：重新检测边界（供外部调用，例如转场后强制刷新）
		public void 重新检测边界()
		{
			_当前边界节点 = null; // 强制重新查找
			更新边界限制();
		}

public void 应用缩放(Vector2 新缩放)
{
	初始缩放 = 新缩放;
	Zoom = 新缩放;
}

		public void 设置目标(Node2D 新目标)
		{
			目标玩家 = 新目标;
			if (目标玩家 != null)
				_上一帧玩家位置 = 目标玩家.GlobalPosition;
		}

		public void 立即跳转到目标()
		{
			if (目标玩家 != null)
			{
				GlobalPosition = 目标玩家.GlobalPosition + 位置偏移;
				_目标位置 = GlobalPosition;
			}
		}

		public void 设置边界限制(Rect2 新边界)
		{
			边界限制 = 新边界;
			启用边界限制 = true;
			_当前边界节点 = null;
		}

 // 保留原清除边界方法，但建议仅在确实需要强制禁用时调用
		public void 清除边界限制()
		{
			启用边界限制 = false;
			_当前边界节点 = null;
			边界限制 = new Rect2();
			GD.Print("[玩家摄像头] 边界限制已清除");
		}

		public void 设置缩放(Vector2 新缩放, bool 永久设置 = false)
		{
			if (永久设置) 初始缩放 = 新缩放;
			Zoom = 新缩放;
		}

		public async Task 平滑缩放(Vector2 目标缩放, float 时长 = 1.0f)
		{
			var 补间 = CreateTween();
			补间.SetEase(Tween.EaseType.InOut);
			补间.SetTrans(Tween.TransitionType.Quad);
			补间.TweenProperty(this, "zoom", 目标缩放, 时长);
			await ToSignal(补间, "finished");
		}
	}
}
