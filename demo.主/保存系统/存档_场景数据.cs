using Godot;
using System.Collections.Generic;

namespace 你的项目.Scripts.资源
{
	[GlobalClass]
	public partial class 存档_场景数据 : Resource
	{
		[Export] public string 存档场景路径 { get; set; } = "";
		[Export] public Vector2 存档位置 { get; set; } = Vector2.Zero;
		[Export] public string 存档点名称 { get; set; } = "";
		[Export] public Godot.Collections.Dictionary<string, Vector2> 场景存档位置 { get; set; }
			= new Godot.Collections.Dictionary<string, Vector2>();
		[Export] public Godot.Collections.Dictionary<string, string> 场景存档点 { get; set; }
			= new Godot.Collections.Dictionary<string, string>();

		public bool 是否有存档() => !string.IsNullOrEmpty(存档场景路径);

		public Vector2 获取场景存档位置(string 场景路径)
		{
			return 场景存档位置.ContainsKey(场景路径) ? 场景存档位置[场景路径] : Vector2.Zero;
		}

		public void 设置场景存档位置(string 场景路径, Vector2 位置, string 存档点名称 = "")
		{
			场景存档位置[场景路径] = 位置;
			if (!string.IsNullOrEmpty(存档点名称))
				场景存档点[场景路径] = 存档点名称;
			存档场景路径 = 场景路径;
			存档位置 = 位置;
			this.存档点名称 = 存档点名称;
		}

		public bool 场景是否有存档(string 场景路径) => 场景存档位置.ContainsKey(场景路径);
	}
}
