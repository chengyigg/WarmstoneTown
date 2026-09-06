using Godot;
using System;

public interface I卡牌效果
{
	// 效果名称
	string 效果名称 { get; }
	
  void 执行效果(卡牌战斗单位 使用者, 卡牌战斗单位 目标, 卡牌实例 卡牌, 卡牌战斗管理器 战斗管理器);
	
	// 描述
	string 获取效果描述();
}
