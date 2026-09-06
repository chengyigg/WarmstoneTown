using Godot;
using System.Threading.Tasks;
using System.Linq;
using 你的项目.Scripts.资源;
using 你的项目.Scripts.管理器;
using 你的项目.Scripts.摄像机;
using 你的项目.Scripts.角色;

namespace 你的项目.Scripts.交互
{
	public enum 后退方向 { 上, 下, 左, 右 }

	public partial class 场景切换触发器 : Area2D
	{
		[Export] public 剧情序列资源 切换后剧情配置 { get; set; }
		[Export(PropertyHint.File, "*.tscn")] public string 目标场景 { get; set; } = "";
		[Export] public 转场动画资源 转场动画配置 { get; set; }
		[Export] public string 交互提示文本 { get; set; } = "";
		[Export] public bool 触碰传送 { get; set; } = false;
		[Export] public bool 传送后播放摄像头动画 { get; set; } = false;
		[Export] public 摄像头动画资源 摄像头动画配置 { get; set; }
		[Export] public bool 转场后触发对话 { get; set; } = false;
		[Export] public 对话序列 转场后对话序列 { get; set; }

		// ===== 条件设置 =====
		[ExportGroup("条件设置")]
		[Export] public bool 需要条件满足 { get; set; } = false;
		[Export] public string 条件变量名 { get; set; } = "可以切换场景";

		// ===== 消耗道具解锁 =====
		[ExportGroup("消耗解锁")]
		[Export] public Godot.Collections.Array<道具数据> 消耗道具列表 { get; set; }

		[Export] public 对话序列 失败对话序列 { get; set; }
		[Export] public 后退方向 后退方向配置 { get; set; }
		[Export] public int 后退步数 { get; set; } = 2;

		// ===== 提示设置 =====
		[ExportGroup("提示设置")]
		[Export] public string 条件不足提示文本 { get; set; } = "条件不足，无法通行";
		[Export] public float 提示显示时长 { get; set; } = 1.5f;

		// ===== ★ 音效设置 =====
		[ExportGroup("音效")]
		[Export] public AudioStream 触发成功音效 { get; set; }      // 传送/切换场景时播放
		[Export] public AudioStream 条件不足音效 { get; set; }      // 条件不足时播放
		[Export] public AudioStream 解锁成功音效 { get; set; }      // 消耗道具解锁成功时播放

		private Label _提示标签;
		private bool _玩家在范围内 = false;
		private bool _正在处理中 = false;
		private bool _正在处理失败 = false;
		private string _解锁条件名;
		private CanvasLayer _提示图层;
		private Label _浮动提示标签;
		private Tween _提示动画;

		[Signal] public delegate void 触发器激活EventHandler();
		[Signal] public delegate void 场景切换开始EventHandler();
		[Signal] public delegate void 摄像头动画开始EventHandler();
		[Signal] public delegate void 摄像头动画结束EventHandler();

		public override void _Ready()
		{
			_提示标签 = GetNode<Label>("提示标签");
			if (!string.IsNullOrEmpty(交互提示文本))
				_提示标签.Text = 交互提示文本;
			else
				_提示标签.Text = "";
			_提示标签.Visible = false;

			int hash = GetPath().GetHashCode();
			_解锁条件名 = $"触发器_解锁_{hash}";

			_提示图层 = new CanvasLayer();
			_提示图层.Layer = 1000;
			GetTree().Root.AddChild(_提示图层);
			_浮动提示标签 = new Label();
			_浮动提示标签.Text = 条件不足提示文本;
			_浮动提示标签.AddThemeFontSizeOverride("font_size", 32);
			_浮动提示标签.AddThemeColorOverride("font_color", Colors.White);
			_浮动提示标签.AddThemeConstantOverride("outline_size", 2);
			_浮动提示标签.AddThemeColorOverride("font_outline_color", Colors.Black);
			_浮动提示标签.HorizontalAlignment = HorizontalAlignment.Center;
			_浮动提示标签.VerticalAlignment = VerticalAlignment.Center;
			_浮动提示标签.Modulate = new Color(1, 1, 1, 0);
			_提示图层.AddChild(_浮动提示标签);
			var 视口大小 = GetViewport().GetVisibleRect().Size;
			_浮动提示标签.Position = (视口大小 - _浮动提示标签.GetMinimumSize()) / 2;

			BodyEntered += OnBodyEntered;
			BodyExited += OnBodyExited;
			if (触碰传送) _提示标签.Visible = false;
		}

		public override void _ExitTree()
		{
			if (_提示图层 != null && IsInstanceValid(_提示图层))
				_提示图层.QueueFree();
		}

		private void 显示浮动提示(string 文本, Color 颜色 = default)
		{
			if (_浮动提示标签 == null || !IsInstanceValid(_浮动提示标签)) return;
			if (_提示动画 != null && _提示动画.IsRunning())
				_提示动画.Kill();

			_浮动提示标签.Modulate = new Color(1, 1, 1, 1);
			if (颜色 != default) _浮动提示标签.Modulate = 颜色;
			_浮动提示标签.Text = 文本;

			var 视口大小 = GetViewport().GetVisibleRect().Size;
			_浮动提示标签.Position = (视口大小 - _浮动提示标签.GetMinimumSize()) / 2;

			_提示动画 = CreateTween();
			_提示动画.TweenProperty(_浮动提示标签, "modulate:a", 0f, 提示显示时长)
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.In);
		}

		public override void _Input(InputEvent @event)
		{
			if (_正在处理中) return;
			if (!触碰传送 && _玩家在范围内 && @event.IsActionPressed("交互"))
			{
				if (检查是否可以切换())
				{
					GetViewport().SetInputAsHandled();
					激活触发器();
				}
				else
				{
					// ★ 播放条件不足音效
					全局背景音乐管理器.实例?.播放音效(条件不足音效);
					显示浮动提示(条件不足提示文本);
					GetViewport().SetInputAsHandled();
				}
			}
		}

		private void OnBodyEntered(Node2D body)
		{
			if (body.IsInGroup("玩家"))
			{
				_玩家在范围内 = true;
				if (触碰传送)
				{
					if (检查是否可以切换())
						激活触发器();
					else
					{
						// ★ 播放条件不足音效
						全局背景音乐管理器.实例?.播放音效(条件不足音效);
						显示浮动提示(条件不足提示文本);
						if (!_正在处理失败 && !_正在处理中) _ = 处理失败();
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(交互提示文本))
						_提示标签.Visible = true;
				}
			}
		}

		private void OnBodyExited(Node2D body)
		{
			if (body.IsInGroup("玩家"))
			{
				_玩家在范围内 = false;
				_提示标签.Visible = false;
			}
		}

		private bool 检查是否可以切换()
		{
			if (需要条件满足)
			{
				bool 条件满足 = 条件管理器.实例?.检查条件(条件变量名) ?? false;
				if (!条件满足)
				{
					GD.Print($"[场景切换触发器] 条件变量 {条件变量名} 不满足");
					return false;
				}
			}

			bool 已解锁 = 条件管理器.实例?.检查条件(_解锁条件名) ?? false;
			if (已解锁) return true;

			if (消耗道具列表 == null || 消耗道具列表.Count == 0)
				return true;

			return 尝试消耗并解锁();
		}

		private bool 尝试消耗并解锁()
		{
			var 管理器 = 玩家道具管理器.实例;
			if (管理器 == null)
			{
				GD.PrintErr("[场景切换触发器] 玩家道具管理器不存在");
				return false;
			}

			foreach (var 需要道具 in 消耗道具列表)
			{
				if (需要道具 == null) continue;
				int 拥有数量 = 0;
				foreach (var 已有道具 in 管理器.玩家道具列表)
				{
					if (已有道具.道具ID == 需要道具.道具ID)
					{
						拥有数量 = 已有道具.数量;
						break;
					}
				}
				if (拥有数量 < 需要道具.数量)
				{
					GD.Print($"[场景切换触发器] 道具不足：需要 {需要道具.名称} x{需要道具.数量}，拥有 {拥有数量}");
					显示浮动提示($"需要 {需要道具.名称} x{需要道具.数量}");
					// ★ 播放条件不足音效
					全局背景音乐管理器.实例?.播放音效(条件不足音效);
					return false;
				}
			}

			foreach (var 需要道具 in 消耗道具列表)
			{
				消耗指定数量道具(需要道具.道具ID, 需要道具.数量);
			}

			if (条件管理器.实例 != null)
			{
				条件管理器.实例.设置条件满足(_解锁条件名);
				GD.Print($"[场景切换触发器] 消耗道具成功，永久解锁（条件：{_解锁条件名}）");
				// ★ 播放解锁成功音效
				全局背景音乐管理器.实例?.播放音效(解锁成功音效);
				显示浮动提示("已解锁！", new Color(0, 1, 0));
			}
			else
			{
				GD.PrintErr("[场景切换触发器] 条件管理器实例为空，无法记录解锁状态！");
			}
			return true;
		}

		private void 消耗指定数量道具(int 道具ID, int 数量)
		{
			var 管理器 = 玩家道具管理器.实例;
			if (管理器 == null) return;
			var 道具列表 = 管理器.玩家道具列表;
			for (int i = 道具列表.Count - 1; i >= 0; i--)
			{
				var 道具 = 道具列表[i];
				if (道具.道具ID == 道具ID)
				{
					int 消耗量 = System.Math.Min(数量, 道具.数量);
					道具.数量 -= 消耗量;
					if (道具.数量 <= 0)
						道具列表.RemoveAt(i);
				}
			}
			管理器.EmitSignal(玩家道具管理器.SignalName.道具列表已更新);
		}

		private async Task 处理失败()
		{
			if (_正在处理失败 || _正在处理中) return;
			_正在处理失败 = true;
			SetDeferred("monitoring", false);
			
			var 玩家 = 玩家管理器.实例?.当前玩家;
			if (玩家 != null) 玩家.设置输入启用(false);
			
			if (失败对话序列 != null && 对话播放器.实例 != null)
			{
				对话播放器.实例.开始对话(失败对话序列);
				await ToSignal(对话播放器.实例, 对话播放器.SignalName.对话结束);
			}
			
			if (玩家 != null && 后退步数 > 0)
			{
				Vector2 后退方向向量 = 获取后退方向向量();
				await 玩家.强制后退(后退方向向量, 后退步数);
			}
			
			if (玩家 != null)
			{
				玩家.设置输入启用(true);
				玩家.Velocity = Vector2.Zero;
			}
			
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			if (_玩家在范围内 && 玩家 != null && 玩家.GlobalPosition.DistanceTo(GlobalPosition) < 16f)
				await 玩家.强制后退(获取后退方向向量(), 1);
			
			SetDeferred("monitoring", true);
			_正在处理失败 = false;
		}

		private Vector2 获取后退方向向量()
		{
			switch (后退方向配置)
			{
				case 后退方向.上: return Vector2.Up;
				case 后退方向.下: return Vector2.Down;
				case 后退方向.左: return Vector2.Left;
				case 后退方向.右: return Vector2.Right;
				default: return Vector2.Down;
			}
		}

		private async void 激活触发器()
		{
			var 当前玩家 = 玩家管理器.实例?.当前玩家;
			if (当前玩家 != null)
			{
				if (当前玩家.正在移动中) 当前玩家.强制终止移动并重置();
				当前玩家.设置禁止移动(true);
			}
			
			if (_正在处理中) return;
			_正在处理中 = true;
			
			// ★ 播放触发成功音效
			全局背景音乐管理器.实例?.播放音效(触发成功音效);
			
			EmitSignal(SignalName.触发器激活);
			SetProcess(false);
			SetDeferred("monitoring", false);
			_提示标签.Visible = false;
			
			if (玩家管理器.实例 != null && 玩家管理器.实例.当前玩家 != null)
			{
				玩家管理器.实例.当前玩家.保存状态();
				if (传送后播放摄像头动画) 玩家管理器.实例.当前玩家.设置输入启用(false);
			}
			
			string 来源场景路径 = GetTree().CurrentScene.SceneFilePath;
			string 目标路径 = 目标场景 ?? string.Empty;
			bool 使用新剧情 = 切换后剧情配置 != null;
			int 当前存档位 = 卡牌数据管理器.当前存档位;
			
			bool 应退化 = false;
			if (使用新剧情 && 切换后剧情配置.启用退化)
			{
				string 条件名 = $"剧情_{切换后剧情配置.ResourcePath.GetFile().GetBaseName()}";
				应退化 = 条件管理器.实例?.检查条件(条件名) ?? false;
				GD.Print($"[退化检查] 剧情: {切换后剧情配置.ResourcePath}, 条件名={条件名}, 应退化={应退化}");
			}
			
			if (应退化)
			{
				转场管理器.实例.开始转场(目标路径, 转场动画配置, 来源场景路径, false, false, null, false, null, 当前存档位);
				_正在处理中 = false;
				return;
			}
			
			if (!string.IsNullOrEmpty(目标路径))
			{
				if (使用新剧情) 转场管理器.实例.设置待播放剧情(切换后剧情配置);
				转场管理器.实例.开始转场(目标路径, 转场动画配置, 来源场景路径, false,
					使用新剧情 ? false : 传送后播放摄像头动画,
					使用新剧情 ? null : 摄像头动画配置,
					使用新剧情 ? false : 转场后触发对话,
					使用新剧情 ? null : 转场后对话序列,
					当前存档位);
			}
		}
		
		public void 手动激活()
		{
			if (!_正在处理中 && 检查是否可以切换()) 激活触发器();
		}
	}
}
