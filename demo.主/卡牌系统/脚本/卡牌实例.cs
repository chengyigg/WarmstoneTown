using Godot;
using System;

[GlobalClass]
public partial class 卡牌实例 : GodotObject
{
	private 卡牌数据 _基础数据;
	private int _当前伤害;
	private int _当前速度值;
	private 卡牌状态 _状态 = 卡牌状态.在卡组中;
	
	public 卡牌数据 基础数据 
	{ 
		get => _基础数据; 
		set => _基础数据 = value; 
	}
	
	public int 当前伤害 
	{ 
		get => _当前伤害; 
		set => _当前伤害 = value; 
	}
	
	public int 当前速度值 
	{ 
		get => _当前速度值; 
		set => _当前速度值 = value; 
	}
	
	public 卡牌状态 状态 
	{ 
		get => _状态; 
		set => _状态 = value; 
	}
	
	public 卡牌实例()
	{
	}
	
	public 卡牌实例(卡牌数据 数据)
	{
		_基础数据 = 数据;
		_当前伤害 = 数据.基础伤害;
	
	}
	   public void 重置状态()
	{
		// 重置卡牌状态以便可以再次使用
		当前伤害 = 基础数据?.基础伤害 ?? 0;
	
		// 重置其他可能的状态
	}
	public enum 卡牌状态
	{
		在卡组中,
		在手牌中,
		已使用,
		已弃置
	}
}
