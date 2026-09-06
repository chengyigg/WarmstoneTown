using Godot;
using System.Collections.Generic;

namespace 你的项目.Scripts.资源
{
	[GlobalClass]
	public partial class 存档_收藏数据 : Resource
	{
		[Export] public Godot.Collections.Array<string> 卡牌路径列表 { get; set; }
			= new Godot.Collections.Array<string>();

		[Export] public Godot.Collections.Array<string> 已购买商品路径列表 { get; set; }
			= new Godot.Collections.Array<string>();

		[Export] public Godot.Collections.Array<string> 跟随者预制体路径列表 { get; set; }
			= new Godot.Collections.Array<string>();

		// ★ 新增：已拾取道具的资源路径列表
		[Export] public Godot.Collections.Array<string> 已拾取道具路径列表 { get; set; }
			= new Godot.Collections.Array<string>();

		// ===== 方法 =====

		public void 保存跟随者列表(Godot.Collections.Array<string> 路径列表)
		{
			跟随者预制体路径列表 = 路径列表;
		}

		public Godot.Collections.Array<string> 获取跟随者列表() => 跟随者预制体路径列表;
	}
}
