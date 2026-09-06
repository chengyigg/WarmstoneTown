using Godot;
using System;

public partial class PasswordLock : Control
{
	// 声明节点变量
	private Label 数字标签1, 数字标签2, 数字标签3;
	private Button 左按钮1, 右按钮1;
	private Button 左按钮2, 右按钮2;
	private Button 左按钮3, 右按钮3;
	private Button 确认按钮;
	
	// 当前密码值
	private int[] 当前数字 = new int[3] {0, 0, 0};
	
	// 预设的正确密码
	private int[] 正确密码 = new int[3] {3, 7, 2};
	
	// 对话播放器引用
	private 对话播放器 对话播放器实例;
	
	// 导出对话序列
	[Export] public 对话序列 密码正确对话 { get; set; }
	[Export] public 对话序列 密码错误对话 { get; set; }
	
	// 信号：密码锁关闭
	[Signal] public delegate void 密码锁关闭EventHandler(bool 密码正确);
	
	// 当节点进入场景树时调用
	public override void _Ready()
	{
		GD.Print("密码锁: _Ready() 开始");
		
		// 获取所有节点引用
		数字标签1 = GetNodeOrNull<Label>("数字容器/数字位1/数字标签1");
		数字标签2 = GetNodeOrNull<Label>("数字容器/数字位2/数字标签2");
		数字标签3 = GetNodeOrNull<Label>("数字容器/数字位3/数字标签3");
		
		左按钮1 = GetNodeOrNull<Button>("数字容器/数字位1/左按钮1");
		右按钮1 = GetNodeOrNull<Button>("数字容器/数字位1/右按钮1");
		左按钮2 = GetNodeOrNull<Button>("数字容器/数字位2/左按钮2");
		右按钮2 = GetNodeOrNull<Button>("数字容器/数字位2/右按钮2");
		左按钮3 = GetNodeOrNull<Button>("数字容器/数字位3/左按钮3");
		右按钮3 = GetNodeOrNull<Button>("数字容器/数字位3/右按钮3");
		确认按钮 = GetNodeOrNull<Button>("确认按钮");
		
		// 打印节点获取状态
		GD.Print($"数字标签1: {数字标签1 != null}");
		GD.Print($"数字标签2: {数字标签2 != null}");
		GD.Print($"数字标签3: {数字标签3 != null}");
		GD.Print($"左按钮1: {左按钮1 != null}");
		GD.Print($"右按钮1: {右按钮1 != null}");
		GD.Print($"左按钮2: {左按钮2 != null}");
		GD.Print($"右按钮2: {右按钮2 != null}");
		GD.Print($"左按钮3: {左按钮3 != null}");
		GD.Print($"右按钮3: {右按钮3 != null}");
		GD.Print($"确认按钮: {确认按钮 != null}");
		
		// 连接按钮信号
		if (左按钮1 != null) 左按钮1.Pressed += 左按钮1按下;
		if (右按钮1 != null) 右按钮1.Pressed += 右按钮1按下;
		if (左按钮2 != null) 左按钮2.Pressed += 左按钮2按下;
		if (右按钮2 != null) 右按钮2.Pressed += 右按钮2按下;
		if (左按钮3 != null) 左按钮3.Pressed += 左按钮3按下;
		if (右按钮3 != null) 右按钮3.Pressed += 右按钮3按下;
		if (确认按钮 != null) 确认按钮.Pressed += 确认按钮按下;
		
		// 获取对话播放器实例
		对话播放器实例 = GetNodeOrNull<对话播放器>("/root/DialogueManager");
		GD.Print($"对话播放器实例: {对话播放器实例 != null}");
		
		// 初始化显示
		更新显示();
		
		// 初始隐藏
		Visible = false;
		
		// 打印密码锁尺寸和位置
		GD.Print($"密码锁尺寸: {Size}, 位置: {Position}");
		
		GD.Print("密码锁: _Ready() 完成");
	}
	
	// 显示密码锁
	public void 显示密码锁()
	{
		Visible = true;
		// 重置密码为默认值
		当前数字 = new int[3] {0, 0, 0};
		更新显示();
		GD.Print("密码锁已显示");
		GD.Print($"密码锁当前Visible: {Visible}");
		GD.Print($"密码锁父节点: {GetParent()?.Name ?? "无父节点"}");
	}
	
	// 隐藏密码锁
	public void 隐藏密码锁()
	{
		Visible = false;
		GD.Print("密码锁已隐藏");
	}
	
	// 更新所有数字显示
	private void 更新显示()
	{
		if (数字标签1 != null) 数字标签1.Text = 当前数字[0].ToString();
		if (数字标签2 != null) 数字标签2.Text = 当前数字[1].ToString();
		if (数字标签3 != null) 数字标签3.Text = 当前数字[2].ToString();
	}
	
	// 第一个数字位的左按钮
	private void 左按钮1按下()
	{
		当前数字[0]--;
		if (当前数字[0] < 0)
			当前数字[0] = 9;
		更新显示();
	}
	
	// 第一个数字位的右按钮
	private void 右按钮1按下()
	{
		当前数字[0]++;
		if (当前数字[0] > 9)
			当前数字[0] = 0;
		更新显示();
	}
	
	// 第二个数字位的左按钮
	private void 左按钮2按下()
	{
		当前数字[1]--;
		if (当前数字[1] < 0)
			当前数字[1] = 9;
		更新显示();
	}
	
	// 第二个数字位的右按钮
	private void 右按钮2按下()
	{
		当前数字[1]++;
		if (当前数字[1] > 9)
			当前数字[1] = 0;
		更新显示();
	}
	
	// 第三个数字位的左按钮
	private void 左按钮3按下()
	{
		当前数字[2]--;
		if (当前数字[2] < 0)
			当前数字[2] = 9;
		更新显示();
	}
	
	// 第三个数字位的右按钮
	private void 右按钮3按下()
	{
		当前数字[2]++;
		if (当前数字[2] > 9)
			当前数字[2] = 0;
		更新显示();
	}
	
	// 确认按钮
	private void 确认按钮按下()
	{
		// 检查密码是否正确
		bool 是否正确 = true;
		for (int i = 0; i < 3; i++)
		{
			if (当前数字[i] != 正确密码[i])
			{
				是否正确 = false;
				break;
			}
		}
		
		if (是否正确)
		{
			GD.Print("密码正确");
			// 触发正确对话
			if (密码正确对话 != null && 对话播放器实例 != null)
			{
				对话播放器实例.开始对话(密码正确对话);
			}
			// 发射信号
			EmitSignal(nameof(密码锁关闭), true);
		}
		else
		{
			GD.Print("密码错误");
			// 触发错误对话
			if (密码错误对话 != null && 对话播放器实例 != null)
			{
				对话播放器实例.开始对话(密码错误对话);
			}
			// 发射信号
			EmitSignal(nameof(密码锁关闭), false);
		}
		
		// 隐藏密码锁
		隐藏密码锁();
	}
	
	// 处理输入 - 重写此方法以防止空格键关闭
	public override void _Input(InputEvent @event)
	{
		if (Visible && @event.IsActionPressed("ui_accept"))
		{
			// 阻止空格键关闭密码锁
			GetViewport().SetInputAsHandled();
			GD.Print("密码锁: 阻止了空格键关闭");
		}
	}
	
	// 设置正确密码（可选，用于动态设置密码）
	public void 设置正确密码(int 第一位, int 第二位, int 第三位)
	{
		正确密码[0] = 第一位;
		正确密码[1] = 第二位;
		正确密码[2] = 第三位;
		GD.Print($"密码已设置为: {第一位}{第二位}{第三位}");
	}
}
