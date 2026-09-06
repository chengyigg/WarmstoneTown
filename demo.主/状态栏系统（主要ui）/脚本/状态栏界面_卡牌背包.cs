using Godot;
using System;
using 你的项目.Scripts.管理器;

public partial class 状态栏界面
{
	// ======================== 卡牌背包相关 ========================
private void 初始化背包()
{
	try
	{
		GD.Print("=== 开始初始化背包 ===");
		if (卡牌标签Button == null) { GD.PrintErr("❌ 卡牌标签按钮未设置"); return; }
		GD.Print("✅ 卡牌标签按钮存在");
		if (背包面板 == null) { GD.PrintErr("❌ 背包面板未设置"); return; }
		GD.Print("✅ 背包面板存在");
		if (格子Container == null) { GD.PrintErr("❌ 格子容器未设置"); return; }
		GD.Print("✅ 格子容器存在");
		

		// 强制更新布局，确保滚动区域正确
		格子Container.UpdateMinimumSize();
		背包面板.UpdateMinimumSize();
		// =============================================
		
		背包面板.Visible = false;
		刷新背包按类型(null);
		卡牌标签Button.Pressed += 切换背包显示;

		GD.Print("=== 背包初始化完成 ===");
	}
	catch (Exception e) { GD.PrintErr($"❌ 初始化背包异常：{e.Message}\n{e.StackTrace}"); }
}

	private void 切换背包显示()
	{
		if (背包面板 == null) return;
		bool 当前可见 = 背包面板.Visible;
		if (当前可见) 关闭背包();
		else
		{
			背包面板.Visible = true;
			背包打开中 = true;
			if (背包过滤器节点 != null) 背包过滤器节点.Visible = true;

			刷新背包按类型(null);
			背包过滤器节点?.重置选中();
		}
	}

	private void 关闭背包()
	{
		if (背包面板 != null && 背包面板.Visible)
		{
			背包面板.Visible = false;
			背包打开中 = false;
			if (背包过滤器节点 != null) 背包过滤器节点.Visible = false;
			隐藏详情框();
			if (当前选中的卡牌 != null)
			{
				当前选中的卡牌.取消选中();
				当前选中的卡牌 = null;
			}
		}
	}

public void 刷新背包按类型(卡牌数据.卡牌类型? 类型 = null)
{
	foreach (Node child in 格子Container.GetChildren()) child.QueueFree();
	var 玩家卡组 = 玩家卡组管理器.实例?.玩家初始卡组;
	if (玩家卡组 == null) return;
	foreach (var 卡牌数据 in 玩家卡组)
	{
		if (卡牌数据 == null) continue;
		if (类型 == null || 卡牌数据.类型 == 类型) 创建卡牌UI(卡牌数据);
	}
	格子Container.QueueSort();
	隐藏详情框();
	if (当前选中的卡牌 != null)
	{
		当前选中的卡牌.取消选中();
		当前选中的卡牌 = null;
	}
	播放所有卡牌摸动画();

// ========== 强制刷新滚动条 ==========
格子Container.UpdateMinimumSize();
格子Container.ForceUpdateTransform();
背包面板.UpdateMinimumSize();
背包面板.ForceUpdateTransform();

// 强制 ScrollContainer 重新评估滚动需求（重置滚动模式）
var 原模式 = 背包面板.VerticalScrollMode;
背包面板.VerticalScrollMode = ScrollContainer.ScrollMode.Disabled;
背包面板.VerticalScrollMode = 原模式;                // 再恢复

// 延迟一帧再刷新一次（确保布局完成）
CallDeferred(nameof(延迟刷新滚动条));
}
private void 延迟刷新滚动条()
{
	if (背包面板 == null) return;
	// 再次重置滚动模式，确保最终生效
	var 原模式 = 背包面板.VerticalScrollMode;
	背包面板.VerticalScrollMode = ScrollContainer.ScrollMode.Disabled;
	背包面板.VerticalScrollMode = 原模式;
	背包面板.UpdateMinimumSize();
}
	private void 创建卡牌UI(卡牌数据 卡牌数据)
	{
		if (卡牌UIPrefab == null || 格子Container == null) return;
		var 卡牌UI控件 = 卡牌UIPrefab.Instantiate<卡牌UI>();
		if (卡牌UI控件 == null) return;
		卡牌UI控件.设置为查看模式(卡牌数据);
		卡牌UI控件.鼠标进入卡牌 += (卡牌) => 当鼠标进入卡牌(卡牌);
		卡牌UI控件.鼠标离开卡牌 += (卡牌) => 当鼠标离开卡牌(卡牌);
		卡牌UI控件.卡牌被点击 += (godotObject) =>
		{
			var 卡牌 = godotObject as 卡牌UI;
			if (卡牌 != null) 当卡牌被点击(卡牌);
			else GD.PrintErr("转换失败！");
		};
		格子Container.AddChild(卡牌UI控件);
		卡牌UI控件.应用初始缩放();
		卡牌UI控件.CallDeferred(nameof(卡牌UI.应用初始缩放));
	}

	private void 播放所有卡牌摸动画()
	{
		if (格子Container == null) return;
		foreach (Node child in 格子Container.GetChildren())
		{
			if (child is 卡牌UI 卡牌)
			{
				var tween = 卡牌.CreateTween();
				tween.SetParallel(false);
				tween.TweenProperty(卡牌, "scale", 卡牌.获取原始缩放() * 1.2f, 0.1f);
				tween.TweenProperty(卡牌, "scale", 卡牌.获取原始缩放(), 0.1f);
			}
		}
	}

	public void 当鼠标进入卡牌(卡牌UI 卡牌)
	{
		bool 在强化预览面板内 = false;
		Node 当前节点 = 卡牌.GetParent();
		while (当前节点 != null)
		{
			if (当前节点 is 强化预览面板) { 在强化预览面板内 = true; break; }
			当前节点 = 当前节点.GetParent();
		}
		if (在强化预览面板内)
		{
			显示详情框(卡牌, 卡牌.获取卡牌描述());
			return;
		}
		if (当前选中的卡牌 != null && 当前选中的卡牌 != 卡牌)
		{
			当前选中的卡牌.取消选中();
			隐藏详情框();
		}
		卡牌.设置选中(true);
		当前选中的卡牌 = 卡牌;
		显示详情框(卡牌, 卡牌.获取卡牌描述());
	}

	public void 当鼠标离开卡牌(卡牌UI 卡牌)
	{
		bool 在强化预览面板内 = false;
		Node 当前节点 = 卡牌.GetParent();
		while (当前节点 != null)
		{
			if (当前节点 is 强化预览面板) { 在强化预览面板内 = true; break; }
			当前节点 = 当前节点.GetParent();
		}
		if (在强化预览面板内) { 隐藏详情框(); return; }
		if (当前选中的卡牌 == 卡牌)
		{
			卡牌.取消选中();
			当前选中的卡牌 = null;
			隐藏详情框();
		}
	}

	private void 当卡牌被点击(卡牌UI 卡牌)
	{
		if (强化预览面板预制体 == null) { GD.PrintErr("强化预览面板预制体未设置"); return; }
		var 卡牌数据 = 卡牌.当前查看卡牌数据;
		if (卡牌数据 == null) { GD.PrintErr("无法获取卡牌数据"); return; }
		try
		{
			var 面板实例 = 强化预览面板预制体.Instantiate<强化预览面板>();
			if (面板实例 == null) { GD.PrintErr("实例化强化预览面板失败"); return; }
			面板实例.Name = "强化预览面板";
			AddChild(面板实例);
			面板实例.AnchorLeft = 0;
			面板实例.AnchorTop = 0;
			面板实例.AnchorRight = 1;
			面板实例.AnchorBottom = 1;
			面板实例.Size = Vector2.Zero;
			面板实例.显示强化预览(卡牌数据);
		}
		catch (Exception e) { GD.PrintErr($"强化预览面板异常: {e.Message}\n{e.StackTrace}"); }
	}


}
