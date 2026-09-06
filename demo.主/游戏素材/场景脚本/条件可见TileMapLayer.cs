using Godot;
using 你的项目.Scripts.管理器;

[Tool] // 便于在编辑器中预览效果
public partial class 条件可见TileMapLayer : TileMapLayer
{
	[ExportGroup("条件设置")]
	[Export] public string 条件变量名 = "";
	[Export] public bool 条件满足时隐藏 = false; // true=条件满足时隐藏，false=条件满足时显示

	private bool _上次条件满足 = false;

	public override void _Ready()
	{
		base._Ready();
		更新状态();
	}

	public override void _Process(double delta)
	{
		bool 当前满足 = 条件允许();
		if (当前满足 != _上次条件满足)
		{
			_上次条件满足 = 当前满足;
			更新状态();
		}
	}

	private bool 条件允许()
	{
		if (string.IsNullOrEmpty(条件变量名))
			return true;
		bool 满足 = 条件管理器.实例?.检查条件(条件变量名) ?? false;
		// 如果勾选了“条件满足时隐藏”，则反转结果
		return 条件满足时隐藏 ? !满足 : 满足;
	}

	private void 更新状态()
	{
		bool 显示 = 条件允许();
		Visible = 显示;
		// ★ 关键：禁用/启用该TileMapLayer的碰撞
		CollisionEnabled = 显示;
	}
}
