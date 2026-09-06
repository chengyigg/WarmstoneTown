// CardUsageRecord.cs
using Godot;
using System;

public partial class 卡牌使用记录 : RefCounted
{
	public 卡牌实例 卡牌 { get; set; }
	public bool 是玩家使用的 { get; set; }
	public float 使用时间 { get; set; } // 记录时间戳
	
	public 卡牌使用记录(卡牌实例 卡牌, bool 是玩家使用的)
	{
		this.卡牌 = 卡牌;
		this.是玩家使用的 = 是玩家使用的;
		this.使用时间 = Time.GetTicksMsec(); // 记录当前时间
	}
	
	// 获取显示颜色（玩家用蓝色，敌人用红色）
	public Color 获取显示颜色()
	{
		return 是玩家使用的 ? new Color(0.3f, 0.3f, 0.9f) : new Color(0.9f, 0.3f, 0.3f);
	}
}
