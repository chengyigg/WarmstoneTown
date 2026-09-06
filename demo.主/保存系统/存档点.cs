using Godot;
using 你的项目.Scripts.管理器;
using 你的项目.Scripts.UI;
using 你的项目.Scripts.角色;
using System.Threading.Tasks;          // ← 添加（解决 Task 未找到）
using 你的项目.Scripts.资源;            // ← 添加（解决 转场动画资源 未找到）
namespace 你的项目.Scripts.交互
{
	public partial class 存档点 : Area2D
	{
		[Export] public string 存档点名称 { get; set; } = "存档点";
		[Export] public bool 启用提示 { get; set; } = true;
		  // ★ 新增：转场配置
		[ExportGroup("存档转场")]
		[Export] public 转场动画资源 默认转场动画 { get; set; }          // 第一种：默认转场
		[Export] public Godot.Collections.Array<条件转场配置> 条件转场列表 { get; set; }  // 第二种/第三种：条件转场集合
		[Export] public bool 启用条件转场 { get; set; } = false;        // 总开关
		private Label _提示标签;
		private bool _玩家在范围内 = false;

		public override void _Ready()
		{
			_提示标签 = GetNode<Label>("提示标签");
			_提示标签.Text = $"按 I 存档";
			_提示标签.Visible = false;
			
			BodyEntered += OnBodyEntered;
			BodyExited += OnBodyExited;
		}

		public override void _Process(double delta)
		{
			if (_玩家在范围内 && Input.IsActionJustPressed("interact"))
			{
				打开存档界面();
			}
		}

		private void OnBodyEntered(Node2D body)
		{
			if (body.IsInGroup("玩家") || body.Name == "Player")
			{
				_玩家在范围内 = true;
				if (启用提示)
				{
					_提示标签.Visible = true;
				}
			}
		}

		private void OnBodyExited(Node2D body)
		{
			if (body.IsInGroup("玩家") || body.Name == "Player")
			{
				_玩家在范围内 = false;
				_提示标签.Visible = false;
			}
		}

private void 打开存档界面()
{
	// ★ 直接打开存档界面，不播放转场
	GlobalSaveManager.实例.打开存档界面(this);
}
		private async Task 执行转场逻辑()
		{
			if (转场管理器.实例 == null) return;

			// 1. 如果未启用条件转场，或条件转场列表为空，使用默认转场
			if (!启用条件转场 || 条件转场列表 == null || 条件转场列表.Count == 0)
			{
				if (默认转场动画 != null)
					await 播放转场(默认转场动画);
				return;
			}

			// 2. 查找第一个满足条件的转场
			条件转场配置 匹配配置 = null;
			foreach (var 配置 in 条件转场列表)
			{
				if (配置 == null) continue;

				// 检查是否满足条件
				bool 条件满足 = 条件管理器.实例?.检查条件(配置.条件名称) ?? false;
				if (!条件满足) continue;

				// 如果配置了“只播放一次”，检查是否已经播放过
				if (配置.只播放一次)
				{
					string 转场ID = 配置.ResourcePath;  // 使用资源路径作为唯一ID
					var 进度数据 = 存档管理器.实例?.获取当前状态()?.进度数据;
					if (进度数据 != null && 进度数据.是否已播放转场(转场ID))
					{
						// 已播放过，跳过这个配置，继续查找下一个
						continue;
					}
				}

				// 找到匹配的配置
				匹配配置 = 配置;
				break;
			}

			// 3. 如果找到了匹配的配置，播放它
			if (匹配配置 != null)
			{
				await 播放转场(匹配配置.转场动画);

				// 如果配置了“只播放一次”，标记为已播放
				if (匹配配置.只播放一次)
				{
					string 转场ID = 匹配配置.ResourcePath;
					var 进度数据 = 存档管理器.实例?.获取当前状态()?.进度数据;
					进度数据?.标记转场已播放(转场ID);
					GD.Print($"[存档点] 条件转场已播放并记录: {转场ID}");
				}
			}
			else
			{
				// 4. 没有匹配的条件，使用默认转场
				if (默认转场动画 != null)
					await 播放转场(默认转场动画);
			}
		}

		 private async Task 播放转场(转场动画资源 转场)
		{
			if (转场 == null) return;
			await 转场管理器.实例.显示转场动画(转场);
			await 转场管理器.实例.隐藏转场动画(转场);
		}
	public void 显示保存成功提示()
{
	// ★ 先播放转场，再显示保存成功文字
	_ = 执行保存后转场和提示();
}
private async Task 执行保存后转场和提示()
{
	// 1. 先执行转场逻辑
	await 执行转场逻辑();

	// 2. 转场完成后，显示"已保存"提示
	var 保存提示 = 创建保存提示();
	GetTree().CurrentScene.AddChild(保存提示);

	await 播放文字动画(保存提示, true);
	await ToSignal(GetTree().CreateTimer(1.0), "timeout");
	await 播放文字动画(保存提示, false);

	保存提示.QueueFree();
}
		
		private async void 延迟显示保存成功提示()
		{
			var 保存提示 = 创建保存提示();
			GetTree().CurrentScene.AddChild(保存提示);
			
			await 播放文字动画(保存提示, true);
			await ToSignal(GetTree().CreateTimer(1.0), "timeout");
			await 播放文字动画(保存提示, false);
			
			保存提示.QueueFree();
		}

		private Label 创建保存提示()
		{
			var 标签 = new Label();
			标签.Name = "保存提示";
			标签.Text = "已保存";
			标签.HorizontalAlignment = HorizontalAlignment.Center;
			标签.VerticalAlignment = VerticalAlignment.Center;
			标签.AddThemeFontSizeOverride("font_size", 24);
			标签.AddThemeColorOverride("font_color", Colors.White);
			标签.AddThemeConstantOverride("outline_size", 2);
			标签.AddThemeColorOverride("font_outline_color", Colors.Black);
			
			标签.Position = GetViewport().GetVisibleRect().Size / 2 - new Vector2(50, 0);
			标签.Modulate = new Color(1, 1, 1, 0);
			
			return 标签;
		}

		private async System.Threading.Tasks.Task 播放文字动画(Label 标签, bool 淡入)
		{
			var 补间 = CreateTween();
			float 目标透明度 = 淡入 ? 1 : 0;
			补间.TweenProperty(标签, "modulate", new Color(1, 1, 1, 目标透明度), 0.5f);
			await ToSignal(补间, "finished");
		}
	}
}
