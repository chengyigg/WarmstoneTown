using Godot;
using System;

public partial class 标签管理器 : Control
{
	// 在编辑器中拖入4个标签按钮
	[Export] private Button[] 标签按钮列表 = new Button[4];
	
	private int 当前选中索引 = 0;
	
   public override void _Ready()
	{
		设置标签焦点系统();
		连接按钮事件();
		GD.Print("标签管理器初始化完成");
	}

	// 在 标签管理器 类中添加
public void 设置当前选中索引(int 索引)
{
	if (索引 >= 0 && 索引 < 标签按钮列表.Length)
		当前选中索引 = 索引;
}
// 隐藏所有标签按钮
public void 隐藏所有标签()
{
	foreach (var btn in 标签按钮列表)
	{
		if (btn != null)
			btn.Visible = false;
	}
}

// 显示所有标签按钮
public void 显示所有标签()
{
	foreach (var btn in 标签按钮列表)
	{
		if (btn != null)
			btn.Visible = true;
	}
}
public Button 获取标签(int 索引)
{
	if (索引 >= 0 && 索引 < 标签按钮列表.Length)
		return 标签按钮列表[索引];
	return null;
}
	
	
	private void 设置标签焦点系统()
	{
		for (int i = 0; i < 标签按钮列表.Length; i++)
		{
			if (标签按钮列表[i] == null) continue;
			
			// 设置按钮可以接收焦点
			标签按钮列表[i].FocusMode = FocusModeEnum.All;
			
			// 计算左右邻居索引
			int 左邻居索引 = (i - 1 + 标签按钮列表.Length) % 标签按钮列表.Length;
			int 右邻居索引 = (i + 1) % 标签按钮列表.Length;
			
			// 设置左右导航
			if (标签按钮列表[左邻居索引] != null)
				标签按钮列表[i].FocusNeighborLeft = 标签按钮列表[左邻居索引].GetPath();
			
			if (标签按钮列表[右邻居索引] != null)
				标签按钮列表[i].FocusNeighborRight = 标签按钮列表[右邻居索引].GetPath();
			
			// 上下导航指向自己（防止上下切换）
			标签按钮列表[i].FocusNeighborTop = 标签按钮列表[i].GetPath();
			标签按钮列表[i].FocusNeighborBottom = 标签按钮列表[i].GetPath();
		}
	}
	
	private void 连接按钮事件()
	{
		for (int i = 0; i < 标签按钮列表.Length; i++)
		{
			if (标签按钮列表[i] == null) continue;
			
			int 当前索引 = i;  // 闭包需要局部变量
			
			标签按钮列表[i].Pressed += () => 当标签按下(当前索引);
			
			// 也可以连接焦点事件，做额外处理
			标签按钮列表[i].FocusEntered += () => 当标签获得焦点(标签按钮列表[当前索引]);
		}
	}
	
	private void 当标签按下(int 索引)
	{
		GD.Print($"标签按下: {索引}");
		当前选中索引 = 索引;
		
		// 触发切换标签事件
		EmitSignal("标签切换", 索引);
		
		// 如果切换到设置标签（假设是索引3），需要特殊处理
		if (索引 == 3)
		{
			EmitSignal("打开设置菜单");
		}
	}
	
private void 当标签获得焦点(Button 标签)
{
	GD.Print($"标签获得焦点: {标签.Name}");
	
	// 找到是哪个标签
	for (int i = 0; i < 标签按钮列表.Length; i++)
	{
		if (标签按钮列表[i] == 标签)
		{
			当前选中索引 = i;
			break;
		}
	}

	// ★ 播放切换音效（通过状态栏单例）
	状态栏界面.获取实例()?.播放切换音效();
}

	// 切换到上一个标签
	public void 上一个标签()
	{
		当前选中索引--;
		if (当前选中索引 < 0)
			当前选中索引 = 标签按钮列表.Length - 1;
		
		if (标签按钮列表[当前选中索引] != null)
			标签按钮列表[当前选中索引].GrabFocus();
	}
	
	// 切换到下一个标签
	public void 下一个标签()
	{
		当前选中索引 = (当前选中索引 + 1) % 标签按钮列表.Length;
		
		if (标签按钮列表[当前选中索引] != null)
			标签按钮列表[当前选中索引].GrabFocus();
	}

	// 获取当前选中的标签
	public Button 获取当前标签()
	{
		return 标签按钮列表[当前选中索引];
	}
	
	// 信号定义
	[Signal]
	public delegate void 标签切换EventHandler(int 标签索引);
	
	[Signal]
	public delegate void 打开设置菜单EventHandler();
}
