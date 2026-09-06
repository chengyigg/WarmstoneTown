using Godot;
using 你的项目.Scripts.资源;

namespace 你的项目.Scripts.UI
{
	public partial class 场景切换动画UI : CanvasLayer
	{
		private ColorRect _背景;
		private Label _文本标签;
		private ProgressBar _进度条;
		
		private 转场动画资源 _当前配置;
		private Tween _动画补间;

		public override void _Ready()
		{
			_背景 = GetNode<ColorRect>("背景");
			_文本标签 = GetNode<Label>("文本标签");
			_进度条 = GetNode<ProgressBar>("进度条");
			
			// 初始状态：完全透明
			_背景.Modulate = new Color(1, 1, 1, 0);
			_文本标签.Modulate = new Color(1, 1, 1, 0);
			_进度条.Modulate = new Color(1, 1, 1, 0);
			
			// 隐藏进度条，除非需要显示
			_进度条.Visible = false;
		}
		
		public void 配置转场(转场动画资源 配置)
		{
			_当前配置 = 配置;
			
			// 设置背景颜色
			_背景.Color = 配置.背景颜色;
			
			// 设置文本
			_文本标签.Text = 配置.显示文本;
			_文本标签.AddThemeColorOverride("font_color", 配置.文本颜色);
			_文本标签.AddThemeFontSizeOverride("font_size", 配置.字体大小);
			
			// 加载自定义字体
			if (!string.IsNullOrEmpty(配置.自定义字体路径))
			{
				var 字体 = ResourceLoader.Load<FontFile>(配置.自定义字体路径);
				if (字体 != null)
				{
					_文本标签.AddThemeFontOverride("font", 字体);
				}
			}
			
			// 显示/隐藏进度条
			_进度条.Visible = 配置.显示加载进度;
		}
		
		public async System.Threading.Tasks.Task 播放淡入动画()
		{
			_动画补间 = CreateTween();
			_动画补间.SetParallel(true);
			
			// 背景淡入
			_动画补间.TweenProperty(_背景, "modulate", new Color(1, 1, 1, 1), _当前配置.淡入时间);
			
			// 文本淡入
			_动画补间.TweenProperty(_文本标签, "modulate", new Color(1, 1, 1, 1), _当前配置.淡入时间);
			
			// 进度条淡入（如果显示）
			if (_当前配置.显示加载进度)
			{
				_动画补间.TweenProperty(_进度条, "modulate", new Color(1, 1, 1, 1), _当前配置.淡入时间);
			}
			
			await ToSignal(_动画补间, "finished");
			
			// 显示文本一段时间
			if (_当前配置.显示时间 > 0)
			{
				await ToSignal(GetTree().CreateTimer(_当前配置.显示时间), "timeout");
			}
		}
		
		public async System.Threading.Tasks.Task 播放淡出动画()
		{
			_动画补间 = CreateTween();
			_动画补间.SetParallel(true);
			
			// 背景淡出
			_动画补间.TweenProperty(_背景, "modulate", new Color(1, 1, 1, 0), _当前配置.淡出时间);
			
			// 文本淡出
			_动画补间.TweenProperty(_文本标签, "modulate", new Color(1, 1, 1, 0), _当前配置.淡出时间);
			
			// 进度条淡出（如果显示）
			if (_当前配置.显示加载进度)
			{
				_动画补间.TweenProperty(_进度条, "modulate", new Color(1, 1, 1, 0), _当前配置.淡出时间);
			}
			
			await ToSignal(_动画补间, "finished");
		}
		
		public void 更新进度(float 进度)
		{
			if (_进度条.Visible)
			{
				_进度条.Value = 进度 * 100;
			}
		}
	}
}
