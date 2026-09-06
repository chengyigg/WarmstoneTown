using Godot;
using System.Text;

/// <summary>
/// 任务卡片 - 显示单个任务信息
/// </summary>
public partial class 任务卡片 : Panel
{
	[Export] private TextureRect 图标;
	[Export] private Label 名称标签;
	[Export] private Label 描述标签;
	[Export] private Label 进度标签;
	[Export] private ProgressBar 进度条;
	[Export] private Label 状态标签;
	[Export] private Label 奖励标签;
	[Export] private VBoxContainer 需求容器;  // ★ 新增：用于显示多个需求

	private 任务数据 _绑定的任务;

	public void 绑定任务数据(任务数据 任务)
	{
		_绑定的任务 = 任务;
		Visible = true;
		更新UI();
		GD.Print($"[任务卡片] 绑定任务: {任务.任务名称}, Visible={Visible}");
	}

	private void 更新UI()
{
	if (_绑定的任务 == null)
	{
		GD.PrintErr("[任务卡片] 绑定任务为空");
		return;
	}

	if (图标 != null)
		图标.Texture = _绑定的任务.任务图标;

	if (名称标签 != null)
		名称标签.Text = _绑定的任务.任务名称;

	if (描述标签 != null)
		描述标签.Text = _绑定的任务.任务描述;

	// ★ 进度显示（不变）
	if (进度标签 != null)
	{
		if (_绑定的任务.需求列表.Count == 0)
		{
			进度标签.Text = "无需求";
		}
		else if (_绑定的任务.需求列表.Count == 1)
		{
			var 需求 = _绑定的任务.需求列表[0];
			int 当前 = _绑定的任务.获取进度(需求.目标ID);
			进度标签.Text = $"{当前}/{需求.所需数量}";
		}
		else
		{
			int 已完成需求 = 0;
			foreach (var 需求 in _绑定的任务.需求列表)
			{
				int 当前 = _绑定的任务.获取进度(需求.目标ID);
				if (当前 >= 需求.所需数量) 已完成需求++;
			}
			进度标签.Text = $"进度: {已完成需求}/{_绑定的任务.需求列表.Count}";
		}
	}

	// ★ 进度条（不变）
	if (进度条 != null)
	{
		if (_绑定的任务.需求列表.Count == 1)
		{
			var 需求 = _绑定的任务.需求列表[0];
			int 当前 = _绑定的任务.获取进度(需求.目标ID);
			进度条.MaxValue = 需求.所需数量;
			进度条.Value = 当前;
			进度条.Visible = true;
		}
		else if (_绑定的任务.需求列表.Count > 1)
		{
			int 总需求 = 0;
			int 已完成 = 0;
			foreach (var 需求 in _绑定的任务.需求列表)
			{
				总需求 += 需求.所需数量;
				已完成 += _绑定的任务.获取进度(需求.目标ID);
			}
			进度条.MaxValue = 总需求;
			进度条.Value = 已完成;
			进度条.Visible = true;
		}
		else
		{
			进度条.Visible = false;
		}
	}

	// ★ 需求列表（不变）
	if (需求容器 != null)
	{
		foreach (Node child in 需求容器.GetChildren())
			child.QueueFree();

		if (_绑定的任务.需求列表.Count > 1)
		{
			foreach (var 需求 in _绑定的任务.需求列表)
			{
				Label 需求标签 = new Label();
				int 当前 = _绑定的任务.获取进度(需求.目标ID);
				string 类型名 = 需求.需求类型 == 任务类型枚举.击杀怪物 ? "击杀" : "收集";
				需求标签.Text = $"{类型名} {需求.目标ID}: {当前}/{需求.所需数量}";
				需求标签.AddThemeFontSizeOverride("font_size", 14);
				需求容器.AddChild(需求标签);
			}
			需求容器.Visible = true;
		}
		else
		{
			需求容器.Visible = false;
		}
	}

	// ★★★★★ 修改状态标签逻辑 ★★★★★
	if (状态标签 != null)
	{
		if (_绑定的任务.是否完成)
		{
			状态标签.Text = "✅ 已完成";
			状态标签.AddThemeColorOverride("font_color", Colors.Green);
		}
		else if (_绑定的任务.所有需求已完成())   // ← 新增：进度已满但未完成
		{
			状态标签.Text = "✅ 可交付";
			状态标签.AddThemeColorOverride("font_color", Colors.Orange);
		}
		else
		{
			状态标签.Text = "⏳ 进行中";
			状态标签.AddThemeColorOverride("font_color", Colors.Yellow);
		}
	}

	// ★ 奖励标签（不变）
	if (奖励标签 != null)
	{
		string 奖励文本 = "";
		if (_绑定的任务.奖励金币 > 0)
			奖励文本 += $"{_绑定的任务.奖励金币} 金币 ";
		if (_绑定的任务.奖励道具列表 != null && _绑定的任务.奖励道具列表.Count > 0)
		{
			foreach (var 道具 in _绑定的任务.奖励道具列表)
			{
				奖励文本 += $"{道具.名称} x{道具.数量} ";
			}
		}
		if (_绑定的任务.奖励卡组 != null && _绑定的任务.奖励卡组.卡组.Count > 0)
			奖励文本 += "卡牌奖励 ";
		if (string.IsNullOrEmpty(奖励文本))
			奖励文本 = "无奖励";

		奖励标签.Text = $"🎁 奖励: {奖励文本}";
	}

	GD.Print($"[任务卡片] UI更新完成: {_绑定的任务.任务名称}");
}
}
