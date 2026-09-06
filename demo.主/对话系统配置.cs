using Godot;

// 创建一个配置脚本，用于在编辑器中设置所有对话关系
public partial class 对话系统配置 : Node
{
	[Export] public 能力选择UI 能力UI;
	[Export] public 对话播放器 对话播放器;
	
	// 配置示例
	public override void _Ready()
	{
		// 确保单例存在
		if (道具管理器.实例 == null)
		{
			AddChild(new 道具管理器());
		}
		
		// 连接信号
		if (能力UI != null && 对话播放器 != null)
		{
			能力UI.能力选择完成 += (能力名称, 对话序列) => {
				对话播放器.开始对话(对话序列);
			};
		}
		
   // 修改这里：使用"添加道具"代替"注册道具替换规则"
		道具管理器.实例.添加道具("冰霜宝石", "冰霜宝石", 创建冰霜选项());
	}
	
	private 分支选项 创建冰霜选项()
	{
		var 选项 = new 分支选项();
		选项.选项文本 = "释放冰霜宝石的力量";
		选项.选项ID = "冰霜增强选项";
		// 选项.目标序列 可以在编辑器中设置
		return 选项;
	}
}
