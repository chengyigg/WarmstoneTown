// 修改传送点脚本
using Godot;
using 你的项目.Scripts.管理器;

namespace 你的项目.Scripts.交互
{
	public partial class 传送点 : Marker2D
	{
		[Export] public string 传送点名称 { get; set; } = "传送点";
		[Export(PropertyHint.File, "*.tscn")] public string 关联来源场景 { get; set; }
		
		[Export] public Vector2 生成位置偏移 { get; set; } = Vector2.Zero;
		private Node2D _精确生成位置;
		
		private string _规范化来源场景路径缓存;
		public string 规范化来源场景路径
		{
			get
			{
				if (string.IsNullOrEmpty(关联来源场景)) return "";
				if (_规范化来源场景路径缓存 != null) return _规范化来源场景路径缓存;

				string raw = 关联来源场景;
				if (raw.StartsWith("uid://"))
				{
					var resource = ResourceLoader.Load<Resource>(raw);
					if (resource != null)
					{
						_规范化来源场景路径缓存 = resource.ResourcePath;
						return _规范化来源场景路径缓存;
					}
					_规范化来源场景路径缓存 = raw;
					return raw;
				}
				_规范化来源场景路径缓存 = raw;
				return raw;
			}
		}
		
		public override void _Ready()
		{
			_精确生成位置 = GetNodeOrNull<Node2D>("精确生成位置");
			if (Engine.IsEditorHint()) QueueRedraw();
			GD.Print($"传送点 '{传送点名称}' 初始化，位置: {GlobalPosition}");
			AddToGroup("传送点");
		}

		public void 激活传送点()
		{
			Vector2 生成位置 = 获取精确生成位置();
			GD.Print($"=== 传送点调试信息 ===");
			GD.Print($"传送点名称: {传送点名称}");
			GD.Print($"计算后的生成位置: {生成位置}");
			if (玩家管理器.实例?.当前玩家 != null)
			{
				玩家管理器.实例.当前玩家.传送到(生成位置);
			}
			else GD.PrintErr("无法激活传送点：玩家不存在");
			GD.Print($"=====================");
		}
		
		private Vector2 获取精确生成位置()
		{
			if (_精确生成位置 != null) return _精确生成位置.GlobalPosition;
			return GlobalPosition + 生成位置偏移;
		}
		
		public override void _Draw()
		{
			if (Engine.IsEditorHint())
			{
				DrawCircle(Vector2.Zero, 15, new Color(0, 1, 0, 0.3f));
				DrawLine(Vector2.Zero, new Vector2(0, -25), new Color(0, 1, 0), 3);
				var 箭头点 = new Vector2[] { new Vector2(0, -25), new Vector2(-8, -15), new Vector2(8, -15) };
				DrawPolygon(箭头点, new Color[] { new Color(0, 1, 0) });
				if (生成位置偏移 != Vector2.Zero)
				{
					DrawCircle(生成位置偏移, 10, new Color(1, 0, 0, 0.5f));
					DrawLine(Vector2.Zero, 生成位置偏移, new Color(1, 0, 0), 2);
				}
			}
		}
	}
}
