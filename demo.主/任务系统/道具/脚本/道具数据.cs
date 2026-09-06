using Godot;
using System;

[GlobalClass]
public partial class 道具数据 : Resource
{
	[Export] public int 道具ID;
	[Export] public string 名称;
	[Export] public string 描述;
	[Export] public Texture2D 图标;
	[Export] public int 数量;
	[Export] public string 效果脚本路径;   // 可选

	// 克隆方法：返回一个新的道具数据实例，复制所有字段
	public 道具数据 克隆()
	{
		return new 道具数据
		{
			道具ID = this.道具ID,
			名称 = this.名称,
			描述 = this.描述,
			图标 = this.图标,
			数量 = this.数量,
			效果脚本路径 = this.效果脚本路径
		};
	}
}
