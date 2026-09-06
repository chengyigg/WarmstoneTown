using Godot;
using System.Collections.Generic;

public partial class 回忆主界面 : CanvasLayer
{
	// 添加导出数组，可以直接在检查器中添加看法数据
	[Export] public Godot.Collections.Array<角色看法数据> 角色看法列表 
		= new Godot.Collections.Array<角色看法数据>();
	
	// 用于快速查找的字典
	private Dictionary<string, 角色看法数据> 角色看法字典 = new Dictionary<string, 角色看法数据>();
	
	// 导出节点引用，可以在检查器中手动指定
	[Export] public VBoxContainer 头像列表容器;
	[Export] public VBoxContainer 主题列表容器;
	[Export] public RichTextLabel 内容显示标签;
	[Export] public Button 关闭按钮;
	
	private string 当前选中角色ID;
	private 角色看法数据 当前角色数据;
	
	// 样式
	private StyleBoxFlat 面板样式;
	private StyleBoxFlat 按钮正常样式;
	private StyleBoxFlat 按钮选中样式;
	
	public override void _Ready()
	{
		GD.Print("回忆主界面: _Ready() 开始");
		
		// 初始化角色看法字典
		初始化角色看法字典();
		
		// 尝试自动查找节点（如果导出引用为空）
		自动查找节点();
		
		// 验证必要节点
		if (!验证必要节点())
		{
			GD.PrintErr("回忆主界面: 必要节点缺失，界面可能无法正常工作");
			return;
		}
		
		// 连接信号
		关闭按钮.Pressed += 关闭界面;
		GD.Print("回忆主界面: 关闭按钮信号连接成功");
		
		// 创建样式
		创建样式();
		应用样式();
		
		// 初始隐藏右侧内容
		主题列表容器.Visible = false;
		内容显示标签.Visible = false;
		
		GD.Print("回忆主界面: _Ready() 完成");
	}
	
	// 新增：自动查找节点（作为导出引用的后备）
	private void 自动查找节点()
	{
		GD.Print("回忆主界面: 开始自动查找节点");
		
		// 如果导出引用为空，尝试自动查找
		if (头像列表容器 == null)
		{
			头像列表容器 = 查找节点<VBoxContainer>("头像列表容器");
			GD.Print($"回忆主界面: 自动查找头像列表容器: {头像列表容器 != null}");
		}
		
		if (主题列表容器 == null)
		{
			主题列表容器 = 查找节点<VBoxContainer>("主题列表容器");
			GD.Print($"回忆主界面: 自动查找主题列表容器: {主题列表容器 != null}");
		}
		
		if (内容显示标签 == null)
		{
			内容显示标签 = 查找节点<RichTextLabel>("内容显示标签");
			GD.Print($"回忆主界面: 自动查找内容显示标签: {内容显示标签 != null}");
		}
		
		if (关闭按钮 == null)
		{
			关闭按钮 = 查找节点<Button>("关闭按钮");
			GD.Print($"回忆主界面: 自动查找关闭按钮: {关闭按钮 != null}");
		}
	}
	
	// 新增：递归查找节点
	private T 查找节点<T>(string 节点名称) where T : Node
	{
		return 查找节点<T>(this, 节点名称);
	}
	
	private T 查找节点<T>(Node 父节点, string 节点名称) where T : Node
	{
		if (父节点 == null) return null;
		
		// 检查当前节点
		if (父节点.Name == 节点名称 && 父节点 is T 目标节点)
		{
			return 目标节点;
		}
		
		// 递归检查所有子节点
		foreach (Node 子节点 in 父节点.GetChildren())
		{
			T 找到的节点 = 查找节点<T>(子节点, 节点名称);
			if (找到的节点 != null)
			{
				return 找到的节点;
			}
		}
		
		return null;
	}
	
	// 新增：验证必要节点
	private bool 验证必要节点()
	{
		bool 所有节点有效 = true;
		
		if (头像列表容器 == null)
		{
			GD.PrintErr("回忆主界面: 头像列表容器未找到");
			所有节点有效 = false;
		}
		
		if (主题列表容器 == null)
		{
			GD.PrintErr("回忆主界面: 主题列表容器未找到");
			所有节点有效 = false;
		}
		
		if (内容显示标签 == null)
		{
			GD.PrintErr("回忆主界面: 内容显示标签未找到");
			所有节点有效 = false;
		}
		
		if (关闭按钮 == null)
		{
			GD.PrintErr("回忆主界面: 关闭按钮未找到");
			所有节点有效 = false;
		}
		
		if (所有节点有效)
		{
			GD.Print("回忆主界面: 所有必要节点验证通过");
		}
		else
		{
			GD.PrintErr("回忆主界面: 部分必要节点缺失，请在检查器中手动指定或确保场景中有对应节点");
		}
		
		return 所有节点有效;
	}
	
	private void 初始化角色看法字典()
	{
		角色看法字典.Clear();
		
		GD.Print($"回忆主界面: 检查器中设置了 {角色看法列表?.Count ?? 0} 个角色看法");
		
		if (角色看法列表 == null)
		{
			GD.PrintErr("回忆主界面: 角色看法列表为null");
			return;
		}
		
		for (int i = 0; i < 角色看法列表.Count; i++)
		{
			var 角色数据 = 角色看法列表[i];
			if (角色数据 != null)
			{
				// 使用角色名称作为ID
				string 角色ID = 角色数据.角色名称;
				角色看法字典[角色ID] = 角色数据;
				GD.Print($"  角色[{i}]: {角色数据.角色名称} - {角色数据.看法列表?.Count ?? 0} 个看法");
			}
			else
			{
				GD.PrintErr($"  角色[{i}]: 空引用");
			}
		}
		
		GD.Print($"回忆主界面: 字典中总共 {角色看法字典.Count} 个角色");
	}
	
	public void 打开界面()
	{
		GD.Print("回忆主界面: 打开界面() 被调用");
		GD.Print($"回忆主界面: 当前Visible = {Visible}");
		
		// 确保界面可见
		Visible = true;
		GD.Print($"回忆主界面: 设置Visible = {Visible}");
		
		// 清空现有内容
		清空头像列表();
		
		// 加载所有角色头像
		加载角色头像列表();
		
		// 重置选择
		当前选中角色ID = null;
		当前角色数据 = null;
		主题列表容器.Visible = false;
		内容显示标签.Visible = false;
		
		GD.Print("回忆主界面: 打开界面完成");
	}
	
	private void 清空头像列表()
	{
		if (头像列表容器 == null)
		{
			GD.PrintErr("回忆主界面: 头像列表容器为空，无法清空");
			return;
		}
		
		int 子节点数量 = 头像列表容器.GetChildCount();
		GD.Print($"回忆主界面: 清空头像列表，原有 {子节点数量} 个子节点");
		
		foreach (Node child in 头像列表容器.GetChildren())
		{
			child.QueueFree();
		}
		
		GD.Print($"回忆主界面: 清空后子节点数量: {头像列表容器.GetChildCount()}");
	}
	
	private void 加载角色头像列表()
	{
		if (头像列表容器 == null)
		{
			GD.PrintErr("回忆主界面: 头像列表容器为空，无法加载头像");
			return;
		}
		
		GD.Print($"回忆主界面: 开始加载角色头像，字典中有 {角色看法字典.Count} 个角色");
		
		// 使用本地字典而不是回忆系统管理器
		foreach (var 键值对 in 角色看法字典)
		{
			string 角色ID = 键值对.Key;
			角色看法数据 角色数据 = 键值对.Value;
			
			if (角色数据 != null)
			{
				GD.Print($"回忆主界面: 创建头像按钮 - {角色数据.角色名称}");
				创建头像按钮(角色ID, 角色数据);
			}
			else
			{
				GD.PrintErr($"回忆主界面: 角色数据为空 - {角色ID}");
			}
		}
		
		GD.Print($"回忆主界面: 加载了 {头像列表容器.GetChildCount()} 个角色头像");
	}
	
	private void 创建头像按钮(string 角色ID, 角色看法数据 数据)
	{
		if (头像列表容器 == null) return;
		
		Button 头像按钮 = new Button();
		头像列表容器.AddChild(头像按钮);
		
		GD.Print($"回忆主界面: 创建头像按钮 - {数据.角色名称}");
		
		// 设置按钮大小
		头像按钮.CustomMinimumSize = new Vector2(80, 80);
		
		// 加载头像纹理
		if (!string.IsNullOrEmpty(数据.角色头像路径))
		{
			Texture2D 头像纹理 = GD.Load<Texture2D>(数据.角色头像路径);
			if (头像纹理 != null)
			{
				头像按钮.Icon = 头像纹理;
				GD.Print($"回忆主界面: 成功加载头像纹理 - {数据.角色头像路径}");
			}
			else
			{
				GD.PrintErr($"回忆主界面: 无法加载头像纹理 - {数据.角色头像路径}");
				// 设置一个默认图标
				头像按钮.Text = 数据.角色名称;
			}
		}
		else
		{
			GD.PrintErr($"回忆主界面: 角色头像路径为空 - {数据.角色名称}");
			头像按钮.Text = 数据.角色名称;
		}
		
		// 设置工具提示
		头像按钮.TooltipText = 数据.角色名称;
		
		// 应用样式
		if (按钮正常样式 != null)
		{
			头像按钮.AddThemeStyleboxOverride("normal", 按钮正常样式);
			头像按钮.AddThemeStyleboxOverride("hover", 创建悬停样式(按钮正常样式));
			头像按钮.AddThemeStyleboxOverride("pressed", 创建按下样式(按钮正常样式));
		}
		
		// 连接信号
		头像按钮.Pressed += () => 选择角色(角色ID, 数据);
		
		GD.Print($"回忆主界面: 头像按钮创建完成 - {数据.角色名称}");
	}
	
	private void 选择角色(string 角色ID, 角色看法数据 数据)
	{
		当前选中角色ID = 角色ID;
		当前角色数据 = 数据;
		
		GD.Print($"回忆系统: 选择角色 - {数据.角色名称}");
		
		// 更新按钮样式
		更新头像按钮样式();
		
		// 加载该角色的主题列表
		加载主题列表(数据);
		
		主题列表容器.Visible = true;
		内容显示标签.Visible = false;
	}
	
	private void 更新头像按钮样式()
	{
		if (头像列表容器 == null || 按钮选中样式 == null) return;
		
		foreach (Button 按钮 in 头像列表容器.GetChildren())
		{
			if (按钮.TooltipText == 当前角色数据?.角色名称)
			{
				按钮.AddThemeStyleboxOverride("normal", 按钮选中样式);
			}
			else
			{
				按钮.AddThemeStyleboxOverride("normal", 按钮正常样式);
			}
		}
	}
	
	private void 加载主题列表(角色看法数据 数据)
{
	if (主题列表容器 == null)
	{
		GD.PrintErr("回忆主界面: 主题列表容器为空，无法加载主题");
		return;
	}
	
	// 清空现有主题
	int 原有主题数量 = 主题列表容器.GetChildCount();
	GD.Print($"回忆主界面: 清空主题列表，原有 {原有主题数量} 个主题");
	
	foreach (Node child in 主题列表容器.GetChildren())
	{
		child.QueueFree();
	}
	
	// 添加主题按钮
	int 已解锁主题数量 = 0;
	int 可显示主题数量 = 0;
	
	GD.Print($"回忆主界面: 详细检查 {数据.角色名称} 的看法列表:");
	
	foreach (看法条目 看法 in 数据.看法列表)
	{
		GD.Print($"  主题: '{看法.主题}'");
		GD.Print($"    已解锁: {看法.已解锁}");
		GD.Print($"    解锁条件: '{看法.解锁条件}'");
		
		if (看法.已解锁) // 只显示已解锁的看法
		{
			// 检查是否满足条件
			bool 满足条件 = 看法.是否满足条件();
			GD.Print($"    满足条件: {满足条件}");
			
			if (满足条件)
			{
				创建主题按钮(看法);
				可显示主题数量++;
				GD.Print($"    状态: 可显示");
			}
			else
			{
				GD.Print($"    状态: 条件不满足，跳过显示");
				
				// 如果条件不满足，检查条件管理器状态
				if (条件管理器.实例 != null && 看法.解锁条件 == "测试")
				{
					bool 条件状态 = 条件管理器.实例.检查条件("测试");
					GD.Print($"    条件管理器状态: '测试' = {条件状态}");
				}
			}
			
			已解锁主题数量++;
		}
		else
		{
			GD.Print($"    状态: 未解锁，跳过");
		}
	}
	
	GD.Print($"回忆主界面: 为 {数据.角色名称} 加载了 {可显示主题数量} 个可显示主题（已解锁 {已解锁主题数量} 个，总共 {数据.看法列表.Count} 个主题）");
	
	// 如果没有可显示的主题，显示提示
	if (可显示主题数量 == 0 && 已解锁主题数量 > 0)
	{
		Label 提示标签 = new Label();
		提示标签.Text = "暂无满足条件的看法";
		提示标签.HorizontalAlignment = HorizontalAlignment.Center;
		提示标签.VerticalAlignment = VerticalAlignment.Center;
		主题列表容器.AddChild(提示标签);
		GD.Print("回忆主界面: 显示'暂无满足条件的看法'提示");
	}
}
	
	private void 创建主题按钮(看法条目 看法)
	{
		if (主题列表容器 == null) return;
		
		Button 主题按钮 = new Button();
		主题列表容器.AddChild(主题按钮);
		
		// 设置按钮文本（包含条件信息）
		string 按钮文本 = 看法.主题;
	
		主题按钮.Text = 按钮文本;
		主题按钮.CustomMinimumSize = new Vector2(180, 50);
		
		// 应用样式
		if (按钮正常样式 != null)
		{
			主题按钮.AddThemeStyleboxOverride("normal", 按钮正常样式);
			主题按钮.AddThemeStyleboxOverride("hover", 创建悬停样式(按钮正常样式));
			主题按钮.AddThemeStyleboxOverride("pressed", 创建按下样式(按钮正常样式));
		}
		
		// 连接信号
		主题按钮.Pressed += () => 显示看法内容(看法);
		
		GD.Print($"回忆主界面: 创建主题按钮 - {看法.主题}, 条件: {看法.解锁条件}");
	}
	
	private void 显示看法内容(看法条目 看法)
	{
		if (内容显示标签 == null) 
		{
			GD.PrintErr("回忆主界面: 内容显示标签为空");
			return;
		}
		
		GD.Print($"回忆主界面: 设置内容前 - 标签可见性: {内容显示标签.Visible}, 文本长度: {内容显示标签.Text.Length}");
		
		内容显示标签.Text = 看法.内容;
		内容显示标签.Visible = true;
		
		GD.Print($"回忆主界面: 设置内容后 - 标签可见性: {内容显示标签.Visible}, 文本长度: {内容显示标签.Text.Length}");
		GD.Print($"回忆系统: 显示看法 - {看法.主题}");
		GD.Print($"看法内容: {看法.内容}");
		
		// 强制刷新显示
		内容显示标签.QueueRedraw();
	}
	
	private void 关闭界面()
	{
		GD.Print("回忆主界面: 关闭按钮被点击");
		
		// 通过管理器关闭界面
		if (回忆系统管理器.实例 != null)
		{
			回忆系统管理器.实例.关闭回忆界面();
		}
		else
		{
			GD.PrintErr("回忆主界面: 回忆系统管理器实例为空");
			Visible = false;
		}
	}
	
	private void 创建样式()
	{
		GD.Print("回忆主界面: 创建样式");
		
		// 主面板样式
		面板样式 = new StyleBoxFlat();
		面板样式.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
		面板样式.BorderColor = new Color(0.4f, 0.4f, 0.5f);
		面板样式.BorderWidthLeft = 2;
		面板样式.BorderWidthTop = 2;
		面板样式.BorderWidthRight = 2;
		面板样式.BorderWidthBottom = 2;
		面板样式.CornerRadiusTopLeft = 10;
		面板样式.CornerRadiusTopRight = 10;
		面板样式.CornerRadiusBottomRight = 10;
		面板样式.CornerRadiusBottomLeft = 10;
		
		// 按钮正常样式
		按钮正常样式 = new StyleBoxFlat();
		按钮正常样式.BgColor = new Color(0.2f, 0.2f, 0.3f);
		按钮正常样式.BorderColor = new Color(0.4f, 0.4f, 0.5f);
		按钮正常样式.BorderWidthLeft = 1;
		按钮正常样式.BorderWidthTop = 1;
		按钮正常样式.BorderWidthRight = 1;
		按钮正常样式.BorderWidthBottom = 1;
		按钮正常样式.CornerRadiusTopLeft = 5;
		按钮正常样式.CornerRadiusTopRight = 5;
		按钮正常样式.CornerRadiusBottomRight = 5;
		按钮正常样式.CornerRadiusBottomLeft = 5;
		
		// 按钮选中样式
		按钮选中样式 = 按钮正常样式.Duplicate() as StyleBoxFlat;
		按钮选中样式.BgColor = new Color(0.3f, 0.3f, 0.5f);
		
		GD.Print("回忆主界面: 样式创建完成");
	}
	
	private StyleBoxFlat 创建悬停样式(StyleBoxFlat 基础样式)
	{
		var 样式 = 基础样式.Duplicate() as StyleBoxFlat;
		样式.BgColor = new Color(0.35f, 0.35f, 0.45f);
		return 样式;
	}
	
	private StyleBoxFlat 创建按下样式(StyleBoxFlat 基础样式)
	{
		var 样式 = 基础样式.Duplicate() as StyleBoxFlat;
		样式.BgColor = new Color(0.15f, 0.15f, 0.25f);
		return 样式;
	}
	
	private void 应用样式()
	{
		GD.Print("回忆主界面: 应用样式");
		
		// 尝试获取主面板
		var 主面板 = 查找节点<Panel>("MainPanel");
		if (主面板 == null)
		{
			主面板 = 查找节点<Panel>("Panel");
		}
		
		if (主面板 != null && 面板样式 != null)
		{
			主面板.AddThemeStyleboxOverride("panel", 面板样式);
			GD.Print("回忆主界面: 样式应用成功");
		}
		else
		{
			GD.PrintErr("回忆主界面: 无法应用样式 - 主面板或面板样式为空");
		}
	}
	
	// 新增：获取所有角色ID
	public string[] 获取所有角色ID()
	{
		string[] 角色数组 = new string[角色看法字典.Keys.Count];
		角色看法字典.Keys.CopyTo(角色数组, 0);
		GD.Print($"回忆主界面: 获取到 {角色数组.Length} 个角色ID");
		return 角色数组;
	}
	
	// 新增：获取角色看法
	public 角色看法数据 获取角色看法(string 角色ID)
	{
		if (角色看法字典.ContainsKey(角色ID))
		{
			return 角色看法字典[角色ID];
		}
		GD.PrintErr($"回忆主界面: 未找到角色看法 - {角色ID}");
		return null;
	}
	
	// 新增：解锁条件看法
	public int 解锁条件看法(string 条件名称)
	{
		int 解锁数量 = 0;
		
		foreach (var 键值对 in 角色看法字典)
		{
			string 角色ID = 键值对.Key;
			角色看法数据 角色数据 = 键值对.Value;
			
			if (角色数据 != null)
			{
				foreach (看法条目 看法 in 角色数据.看法列表)
				{
					// 检查看法的解锁条件是否匹配
					if (!string.IsNullOrEmpty(看法.解锁条件) && 看法.解锁条件 == 条件名称)
					{
						看法.已解锁 = true;
						解锁数量++;
						GD.Print($"回忆主界面: 解锁看法 - {角色数据.角色名称} - {看法.主题}");
					}
				}
			}
		}
		
		GD.Print($"回忆主界面: 总共解锁了 {解锁数量} 个条件为 '{条件名称}' 的看法");
		return 解锁数量;
	}
}
