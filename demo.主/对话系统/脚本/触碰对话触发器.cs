using Godot;
using 你的项目.Scripts.角色;
using 你的项目.Scripts.管理器;

public partial class 触碰对话触发器 : Area2D
{
	[Export] public 对话序列 对话序列资源 { get; set; }
	[Export] public bool 只触发一次 { get; set; } = true;
	[Export] public bool 触发后禁用 { get; set; } = true;
	[Export] public bool 对话后获得冻结能力 { get; set; } = false;
	[Export] public string 触发所需条件 { get; set; } = ""; // 可选：需要满足的条件名称（如“初见玩家剧情已完成”）
	private bool 已触发 = false;
	private bool 可以触发对话 = true;
	
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		if (对话播放器.实例 == null)
		{
			GD.PrintErr("触碰对话触发器: 对话播放器单例为空");
			return;
		}
		对话播放器.实例.对话开始 += () => 可以触发对话 = false;
		对话播放器.实例.对话结束 += () => 
		{
			if (!IsInstanceValid(this)) return;
			可以触发对话 = true;
			if (触发后禁用)
				Monitoring = false;
			if (对话后获得冻结能力)
				给予冻结河流能力();
		};
	}

	private void OnBodyEntered(Node2D body)
	{
		if (!body.IsInGroup("玩家")) return;
		
		// 检查触发所需条件（如果设置了）
		if (!string.IsNullOrEmpty(触发所需条件))
		{
			if (条件管理器.实例 == null || !条件管理器.实例.检查条件(触发所需条件))
				return;
		}
		
		if (可以触发对话 && 对话序列资源 != null && !已触发)
			触发对话();
	}
	
	private void 触发对话()
	{
		if (只触发一次)
			已触发 = true;
		对话播放器.实例.开始对话(对话序列资源);
	}
	
	private void 给予冻结河流能力()
	{
		var 玩家 = GetTree().CurrentScene.GetNodeOrNull<玩家控制器>("玩家");
		if (玩家 == null)
		{
			玩家 = GetNodeOrNull<玩家控制器>("/root/主游戏场景/玩家");
			if (玩家 == null)
			{
				GD.PrintErr("无法找到玩家节点，无法给予冻结河流能力");
				return;
			}
		}
		玩家.设置过河流能力(true);
	}

	public void 重新启用()
	{
		Monitoring = true;
		已触发 = false;
		可以触发对话 = true;
	}
	
	public void 强制触发对话()
	{
		if (对话序列资源 != null)
		{
			对话播放器.实例.开始对话(对话序列资源);
			if (对话后获得冻结能力)
				给予冻结河流能力();
		}
	}
	
	public void 设置对话序列(对话序列 新序列)
	{
		对话序列资源 = 新序列;
		已触发 = false;
	}
	
	public void 设置对话后获得冻结能力(bool 获得能力) => 对话后获得冻结能力 = 获得能力;
}
