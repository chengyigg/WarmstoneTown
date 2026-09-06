using Godot;
using 你的项目.Scripts.全局; // ★ 添加这一行
namespace 你的项目.Scripts.UI
{
	public partial class 金币UI : CanvasLayer
	{
		[Export] public Control 图标节点;   // 在检查器中拖入图标控件（TextureRect 等）
		[Export] public Label 提示标签;     // 在检查器中拖入提示标签
		[Export] public Label 金币数值标签; // ★ 新增：显示金币数量的Label

		public override void _Ready()
		{
			   // ★ 初始隐藏，由转场管理器控制显示
	Visible = false;
			// 检查是否已指定节点
			if (图标节点 == null)
			{
				GD.PrintErr("金币UI：未设置【图标节点】");
				return;
			}
			if (提示标签 == null)
			{
				GD.PrintErr("金币UI：未设置【提示标签】");
				return;
			}
			if (金币数值标签 == null)
			{
				GD.PrintErr("金币UI：未设置【金币数值标签】");
				return;
			}

			// 初始隐藏提示标签
			提示标签.Visible = false;

			// 自动连接鼠标进入和离开事件
			图标节点.MouseEntered += OnMouseEnter;
			图标节点.MouseExited += OnMouseExit;

			// ★ 绑定金币管理器信号
			if (金币管理器.实例 != null)
			{
				金币管理器.实例.金币数量已变化 += _更新金币显示;
				_更新金币显示(金币管理器.实例.金币数量); // 初始化显示
				GD.Print("金币UI：已绑定金币管理器信号");
			}
			else
			{
				GD.PrintErr("金币UI：金币管理器实例为空，无法绑定信号");
			}

			GD.Print("金币UI：自动悬停提示已启用");
		}

		private void OnMouseEnter()
		{
			提示标签.Visible = true;
		}

		private void OnMouseExit()
		{
			提示标签.Visible = false;
		}

		private void _更新金币显示(int 新数量)
		{
			if (金币数值标签 != null && IsInstanceValid(金币数值标签))
			{
				金币数值标签.Text = 新数量.ToString();
				GD.Print($"[金币UI] 金币更新为: {新数量}");
			}
		}

		public override void _ExitTree()
		{
			if (金币管理器.实例 != null)
				金币管理器.实例.金币数量已变化 -= _更新金币显示;
		}
	}
}
