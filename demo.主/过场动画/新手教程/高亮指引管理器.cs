using Godot;

public partial class 高亮指引管理器 : Node
{
	[Export] private TextureRect _洞图片;   // 预先在场景中创建的洞图片节点

	public override void _Ready()
	{
		if (_洞图片 == null)
			GD.PrintErr("高亮指引管理器：未绑定洞图片节点");
		else
			_洞图片.Visible = false;
	}

	public void 显示洞图片()
	{
		if (_洞图片 != null)
			_洞图片.Visible = true;
	}

	public void 隐藏洞图片()
	{
		if (_洞图片 != null)
			_洞图片.Visible = false;
	}
public void 显示高亮控件(string 控件路径)
{
	var 控件 = GetNodeOrNull<Control>(控件路径);
	if (控件 == null)
	{
		GD.PrintErr($"高亮指引管理器：未找到控件 {控件路径}");
		return;
	}
	
	// 获取控件的全局位置，并将洞图片移动到该位置
	Vector2 目标位置 = 控件.GlobalPosition;
	SetPosition(目标位置);   // 你可以调用现有的 设置洞图片位置 方法
	Show();                 // 显示洞图片
}

private void SetPosition(Vector2 位置)
{
	if (_洞图片 != null)
		_洞图片.Position = 位置;
}

private void Show()
{
	if (_洞图片 != null)
		_洞图片.Visible = true;
}
	// 可选：如果需要代码动态设置位置（动画也可以做）
	public void 设置洞图片位置(Vector2 位置)
	{
		if (_洞图片 != null)
			_洞图片.Position = 位置;
	}

	public void 设置洞图片大小(Vector2 大小)
	{
		if (_洞图片 != null)
			_洞图片.Size = 大小;
	}
}
