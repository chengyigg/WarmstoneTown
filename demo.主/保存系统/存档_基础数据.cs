using Godot;
using System;
namespace 你的项目.Scripts.资源
{
	[GlobalClass]
	public partial class 存档_基础数据 : Resource
	{
		[Export] public string 玩家名字 { get; set; } = "玩家";
		[Export] public int 金币数量 { get; set; } = 0;
		[Export] public string 存档时间字符串 { get; set; } = "";

		public DateTime 存档时间
		{
			get
			{
				if (string.IsNullOrEmpty(存档时间字符串))
					return DateTime.Now;
				if (DateTime.TryParse(存档时间字符串, out DateTime 结果))
					return 结果;
				return DateTime.Now;
			}
			set => 存档时间字符串 = value.ToString("yyyy-MM-dd HH:mm:ss");
		}

		public string 获取格式化时间() => 存档时间.ToString("yyyy-MM-dd HH:mm");

		public 存档_基础数据() => 存档时间 = DateTime.Now;
	}
}
