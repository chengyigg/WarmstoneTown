using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using 你的项目.Scripts.全局;
using 你的项目.Scripts.资源;
using 你的项目.Scripts.摄像机;

namespace 你的项目.Scripts.角色
{
	public partial class 玩家控制器 : CharacterBody2D
	{
		[Export] public float 加速倍数 = 2.0f;
		public float 当前移动速度 => _当前移动速度;
		private bool 输入加速中 => Input.IsActionPressed("sprint");
		private bool _强制移动中 = false;
		[Export] public float 移动速度 = 300.0f;
		[Export] public AnimatedSprite2D 动画精灵 { get; set; }
		public bool 正在移动中 => Velocity.Length() > 1f;
		private List<Vector2> _路径队列 = new List<Vector2>();
		public IReadOnlyList<Vector2> 路径队列 => _路径队列;
		private int _过场动画锁计数 = 0;
		private bool _禁止移动 = false;
		private float _当前移动速度 => Input.IsActionPressed("sprint") ? 移动速度 * 加速倍数 : 移动速度;

		[Export] public float 格子大小 = 16.0f;

		private Vector2 _lastInputDirection = Vector2.Down;

		private string 当前测试条件 = "";
		private bool _回忆界面打开 = false;
		[Export] public bool 燃烧 { get; set; } = false;
		[Export] public bool 可以过河流 { get; set; } = false;
		public bool 可移动 { get; private set; } = true;
		private bool _过场动画播放中 = false;
		private bool _输入启用 = true;
		public Vector2 最后位置 { get; set; } = Vector2.Zero;
		public string 最后场景 { get; set; } = "";

		private TileMap 河流地图;
		private int 河流图块源ID = 0;
		private Vector2I 河流图块坐标 = new Vector2I(20, 2);
		private Vector2I 红色图块坐标 = new Vector2I(2, 2);
		private 能力选择UI 能力UI;
		private HashSet<Vector2I> 已冻结的图块 = new HashSet<Vector2I>();
		private CanvasLayer _提示图层;
		private Label _提示标签;

		private Vector2[] _方向历史;
		public Vector2[] 方向历史 => _方向历史;
		public Vector2 上次输入方向 => _lastInputDirection;

		[Export] public PackedScene 跟随者预制体1;
		[Export] public PackedScene 跟随者预制体2;
		[Export] public PackedScene 跟随者预制体3;

		[Export] public int _导出历史长度 = 60;
		private Vector2[] _位置历史;
		private int _历史索引 = 0;
		public Vector2[] 位置历史 => _位置历史;
		public int 历史索引 => _历史索引;
		public int 历史长度 => _导出历史长度;

		private bool _鼠标可见 = false;

		public 玩家控制器()
		{
			_位置历史 = new Vector2[_导出历史长度];
			_方向历史 = new Vector2[_导出历史长度];
		}

		public void 添加跟随者(PackedScene 预制体, int 滞后格子数 = 2)
		{
			if (跟随者管理器.实例 == null) return;
			跟随者管理器.实例.添加跟随者(预制体, 滞后格子数);
		}

		public void 移除所有跟随者()
		{
			if (跟随者管理器.实例 == null) return;
			跟随者管理器.实例.移除所有跟随者();
		}

		public override void _EnterTree()
		{
			if (!IsInGroup("玩家")) AddToGroup("玩家");
		}

		public override void _Ready()
		{
			if (动画精灵 == null) 动画精灵 = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			初始化河流地图引用();

			_创建提示UI();
			能力UI = new 能力选择UI();
			GetTree().Root.AddChild(能力UI);
			if (条件管理器.实例 != null) 当前测试条件 = 条件管理器.实例.获取当前测试条件();
			能力UI.注册能力回调("燃烧能力", 选择燃烧能力);
			能力UI.注册能力回调("冻结能力", 选择冻结能力);
			动画精灵.Play("walk_down");

			for (int i = 0; i < _导出历史长度; i++)
			{
				_位置历史[i] = GlobalPosition;
				_方向历史[i] = Vector2.Down;
			}

			_控制鼠标显示();
			GlobalPosition = 获取网格对齐位置(GlobalPosition);

			if (条件管理器.实例 != null && 条件管理器.实例.检查条件("已获得跟随者"))
			{
				CallDeferred(nameof(自动添加跟随者));
			}
		}

		private void 自动添加跟随者()
		{
			if (跟随者管理器.实例 == null) return;
			var 跟随者组 = GetTree().GetNodesInGroup("跟随者");
			if (跟随者组.Count == 0 && 跟随者预制体1 != null)
				添加跟随者(跟随者预制体1, 2);
		}

		private void _创建提示UI()
		{
			var 已有提示图层 = GetTree().Root.FindChild("提示标签UI", true, false) as CanvasLayer;
			if (已有提示图层 == null)
			{
				_提示图层 = new CanvasLayer();
				_提示图层.Name = "提示标签UI";
				_提示图层.Layer = 100;
				_提示标签 = new Label();
				_提示标签.Name = "提示文本";
				_提示标签.Text = "按 P 打开状态栏";
				_提示标签.AddThemeFontSizeOverride("font_size", 20);
				_提示标签.AddThemeColorOverride("font_color", Colors.White);
				_提示标签.AddThemeConstantOverride("outline_size", 2);
				_提示标签.AddThemeColorOverride("font_outline_color", Colors.Black);
				_提示标签.Position = new Vector2(20, 20);
				_提示图层.AddChild(_提示标签);
				GetTree().Root.AddChild(_提示图层);
				GD.Print("【提示UI】已创建并添加到根节点");
			}
			else
			{
				_提示图层 = 已有提示图层;
				_提示标签 = _提示图层.GetNode<Label>("提示文本");
				GD.Print("【提示UI】已复用根节点中的提示图层");
			}
		}

		public void 设置输入启用(bool 启用)
		{
			_输入启用 = 启用;
		}

		public void 设置禁止移动(bool 禁止)
		{
			_禁止移动 = 禁止;
			if (禁止)
			{
				Velocity = Vector2.Zero;
				设置输入启用(false);
			}
		}

		private void _控制鼠标显示()
		{
			bool 当前场景需要玩家 = GetTree().CurrentScene?.IsInGroup("需要玩家") ?? false;
			if (当前场景需要玩家)
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
				_鼠标可见 = false;
			}
			else
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
				_鼠标可见 = true;
			}
		}

		private void _切换鼠标显示()
		{
			if (_鼠标可见)
				Input.MouseMode = Input.MouseModeEnum.Captured;
			else
				Input.MouseMode = Input.MouseModeEnum.Visible;
			_鼠标可见 = !_鼠标可见;
		}

		public override void _ExitTree()
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}

		public override void _Input(InputEvent @event)
		{
			if (@event.IsActionPressed("toggle_mouse")) { _切换鼠标显示(); GetViewport().SetInputAsHandled(); }
			if (@event.IsActionPressed("add_follower")) { 添加跟随者(跟随者预制体1, 2); }
			if (@event.IsActionPressed("任务面板"))
			{
				var 任务面板 = GetTree().Root.FindChild("任务面板UI", true, false) as 任务面板UI;
				if (任务面板 != null) 任务面板.切换面板();
				GetViewport().SetInputAsHandled();
			}
			处理能力选择输入();
			if (@event.IsActionPressed("view_cards")) { 打开卡牌查看界面(); GetViewport().SetInputAsHandled(); }
		}

		private void 显示条件提示(string 提示内容)
		{
			Label 提示标签 = new Label();
			提示标签.Text = 提示内容;
			提示标签.AddThemeFontSizeOverride("font_size", 16);
			提示标签.AddThemeColorOverride("font_color", new Color(1,1,1));
			提示标签.Position = new Vector2(100,100);
			提示标签.ZIndex = 100;
			GetTree().Root.AddChild(提示标签);
			var 计时器 = new Timer();
			提示标签.AddChild(计时器);
			计时器.WaitTime = 2.0;
			计时器.OneShot = true;
			计时器.Timeout += () => { 提示标签.QueueFree(); };
			计时器.Start();
		}

		public void 开始过场动画()
		{
			GD.Print("[玩家控制器] 开始过场动画");
			var 摄像头 = GetTree().GetFirstNodeInGroup("摄像头") as 玩家摄像头;
			if (摄像头 != null)
				GD.Print($"  开始过场动画前，摄像头目标: {摄像头.目标玩家?.Name ?? "null"}");

			Velocity = Vector2.Zero;
			_过场动画播放中 = true;
			可移动 = false;
			_输入启用 = false;
			_禁止移动 = true;
		}

		public void 结束过场动画(bool 强制播放空闲动画 = true)
		{
			GD.Print("[玩家控制器] 结束过场动画");
			_过场动画播放中 = false;
			可移动 = true;
			_输入启用 = true;
			_禁止移动 = false;
			SetPhysicsProcess(true);
			if (强制播放空闲动画) 播放空闲动画(_lastInputDirection);
			记录位置(GlobalPosition);
			最后位置 = GlobalPosition;

			var 摄像头 = GetTree().GetFirstNodeInGroup("摄像头") as 玩家摄像头;
			if (摄像头 != null)
				GD.Print($"  结束过场动画后，摄像头目标: {摄像头.目标玩家?.Name ?? "null"}");
		}

		private void 打开卡牌查看界面()
		{
			var 当前场景 = GetTree().CurrentScene;
			if (当前场景 == null || !当前场景.IsInGroup("需要玩家")) return;
			var 界面预制体 = GD.Load<PackedScene>("res://卡牌系统/非战斗场景卡牌/卡牌查看界面.tscn");
			if (界面预制体 == null) { GD.PrintErr("界面预制体加载失败！"); return; }
			var 界面 = 界面预制体.Instantiate<卡牌查看界面>();
			当前场景.AddChild(界面);
			GetTree().Paused = true;
			界面.TreeExited += () => GetTree().Paused = false;
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_禁止移动 || _过场动画播放中 || _回忆界面打开 || !可移动 || !_输入启用)
			{
				Velocity = Vector2.Zero;
				MoveAndSlide();
				if (!_回忆界面打开) 播放空闲动画(_lastInputDirection);
				return;
			}

			Vector2 inputDir = Vector2.Zero;
			if (Input.IsActionPressed("右")) inputDir.X += 1f;
			if (Input.IsActionPressed("左")) inputDir.X -= 1f;
			if (Input.IsActionPressed("下")) inputDir.Y += 1f;
			if (Input.IsActionPressed("上")) inputDir.Y -= 1f;

			Vector2 moveDir = Vector2.Zero;
			if (inputDir != Vector2.Zero)
			{
				if (Math.Abs(inputDir.X) > Math.Abs(inputDir.Y))
					moveDir = new Vector2(Math.Sign(inputDir.X), 0);
				else if (Math.Abs(inputDir.Y) > 0)
					moveDir = new Vector2(0, Math.Sign(inputDir.Y));
				else
					moveDir = inputDir;

				_lastInputDirection = moveDir;
				播放行走动画(moveDir);
			}
			else
			{
				播放空闲动画(_lastInputDirection);
			}

			float speed = _当前移动速度;
			Velocity = moveDir * speed;
			MoveAndSlide();

			记录位置(GlobalPosition);
			最后位置 = GlobalPosition;

			if (可以过河流 && Velocity.Length() > 0.1f)
				改变接触的河流图块();
			更新碰撞状态();
		}

		public async Task<bool> 强制后退(Vector2 direction, int steps)
		{
			Vector2 step = direction * 16f;
			for (int i = 0; i < steps; i++)
			{
				GlobalPosition += step;
				await ToSignal(GetTree(), "physics_frame");
			}
			return true;
		}

		public void 强制终止移动并重置()
		{
			Velocity = Vector2.Zero;
			记录位置(GlobalPosition);
			最后位置 = GlobalPosition;
		}

		public Vector2 获取网格对齐位置(Vector2 pos)
		{
			float x = Mathf.Round(pos.X / 格子大小) * 格子大小;
			float y = Mathf.Round(pos.Y / 格子大小) * 格子大小;
			return new Vector2(x, y);
		}

		private void 记录位置(Vector2 位置)
		{
			_位置历史[_历史索引] = 位置;
			_方向历史[_历史索引] = _lastInputDirection;
			_历史索引 = (_历史索引 + 1) % _导出历史长度;
		}

		public bool 能力是否激活(string 能力名称) => 能力UI != null && 能力UI.能力是否激活(能力名称);

		private void 处理能力选择输入()
		{
			if (Input.IsActionJustPressed("ability_menu"))
			{
				if (能力UI.是否正在显示()) 能力UI.隐藏();
				else 能力UI.显示();
			}
		}

		private void 选择燃烧能力()
		{
			if (可以过河流) 设置过河流能力(false);
			切换燃烧状态();
		}

		private void 选择冻结能力()
		{
			if (燃烧) 设置燃烧状态(false);
			设置过河流能力(true);
		}

		private void 初始化河流地图引用() => 河流地图 = GetNodeOrNull<TileMap>("../TileMap");

		private TileMap 获取有效的河流地图()
		{
			if (河流地图 == null || !IsInstanceValid(河流地图)) 初始化河流地图引用();
			return 河流地图;
		}

		public void 设置可移动(bool 可移动状态) => 可移动 = 可移动状态;

		public void 切换燃烧状态() => 燃烧 = !燃烧;

		public void 设置燃烧状态(bool 状态)
		{
			燃烧 = 状态;
			if (状态 && 可以过河流) 设置过河流能力(false);
		}

		private void 改变接触的河流图块()
		{
			var 当前河流地图 = 获取有效的河流地图();
			if (当前河流地图 == null) return;
			Vector2I 玩家单元格 = 当前河流地图.LocalToMap(GlobalPosition);
			for (int x = -1; x <= 1; x++)
				for (int y = -1; y <= 1; y++)
				{
					Vector2I 检测单元格 = new Vector2I(玩家单元格.X + x, 玩家单元格.Y + y);
					if (是河流图块(检测单元格) && !已冻结的图块.Contains(检测单元格))
						冻结河流图块(检测单元格);
				}
		}

		private void 冻结河流图块(Vector2I 单元格位置)
		{
			var 当前河流地图 = 获取有效的河流地图();
			if (当前河流地图 == null) return;
			当前河流地图.SetCell(0, 单元格位置, 河流图块源ID, 红色图块坐标);
			已冻结的图块.Add(单元格位置);
		}

		private bool 是河流图块(Vector2I 单元格位置)
		{
			var 当前河流地图 = 获取有效的河流地图();
			if (当前河流地图 == null) return false;
			TileData 图块数据 = 当前河流地图.GetCellTileData(0, 单元格位置);
			if (图块数据 != null)
			{
				Vector2I 当前图块坐标 = 当前河流地图.GetCellAtlasCoords(0, 单元格位置);
				return 当前图块坐标 == 河流图块坐标;
			}
			return false;
		}

		private bool 是红色图块(Vector2I 单元格位置)
		{
			var 当前河流地图 = 获取有效的河流地图();
			if (当前河流地图 == null) return false;
			TileData 图块数据 = 当前河流地图.GetCellTileData(0, 单元格位置);
			if (图块数据 != null)
			{
				Vector2I 当前图块坐标 = 当前河流地图.GetCellAtlasCoords(0, 单元格位置);
				return 当前图块坐标 == 红色图块坐标;
			}
			return false;
		}

		private void 更新碰撞状态()
		{
			var 当前河流地图 = 获取有效的河流地图();
			if (当前河流地图 == null) { SetCollisionMaskValue(1, true); return; }
			Vector2I 玩家单元格 = 当前河流地图.LocalToMap(GlobalPosition);
			bool 玩家在红色图块上 = 是红色图块(玩家单元格);
			SetCollisionMaskValue(1, !玩家在红色图块上);
		}

		public void 设置过河流能力(bool 可以过)
		{
			可以过河流 = 可以过;
			if (可以过 && 燃烧) 设置燃烧状态(false);
		}

		public void 重置冻结状态()
		{
			var 当前河流地图 = 获取有效的河流地图();
			if (当前河流地图 != null)
				foreach (Vector2I 单元格 in 已冻结的图块)
					当前河流地图.SetCell(0, 单元格, 河流图块源ID, 河流图块坐标);
			已冻结的图块.Clear();
		}

		public Godot.Collections.Array<string> 获取跟随者预制体路径列表()
		{
			if (跟随者管理器.实例 != null) return 跟随者管理器.实例.获取所有跟随者预制体路径();
			return new Godot.Collections.Array<string>();
		}

		public void 重建跟随者(Godot.Collections.Array<string> 路径列表)
		{
			if (跟随者管理器.实例 != null) 跟随者管理器.实例.重建跟随者(路径列表);
		}

		public int 获取当前冻结数量() => 已冻结的图块.Count;

		public void 切换过河流能力() => 设置过河流能力(!可以过河流);

		public void 传送到(Vector2 位置)
		{
			Velocity = Vector2.Zero;
			GlobalPosition = 位置;
			最后位置 = 位置;
			_路径队列.Clear();
			_路径队列.Add(位置);

			if (跟随者管理器.实例 != null)
				跟随者管理器.实例.重置所有跟随者();
		}

		public void 保存状态()
		{
			最后位置 = GlobalPosition;
			最后场景 = GetTree().CurrentScene.Name;
		}

		public void 准备场景切换() => 强制终止移动并重置();

		// ---- 动画方法 ----
		private void 播放行走动画(Vector2 方向)
		{
			if (动画精灵 == null) return;

			bool 加速中 = Input.IsActionPressed("sprint");
			string 前缀 = 加速中 ? "run" : "walk";

			string 后缀 = "left";
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
				else
					return;
			}

			float 基准速度 = 移动速度;
			float 比例 = _当前移动速度 / 基准速度;
			动画精灵.SpeedScale = 比例;
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
}
