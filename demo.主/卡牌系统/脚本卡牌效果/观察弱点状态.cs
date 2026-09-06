using Godot;

public static class 观察弱点状态
{
	// 静态变量，全局共享
	public static int 玩家下次攻击伤害加成 = 0;
	
	// 重置状态
	public static void 重置()
	{
		玩家下次攻击伤害加成 = 0;
	}
}
