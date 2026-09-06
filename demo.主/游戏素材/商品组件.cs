using Godot;
using System;
using 你的项目.Scripts.资源;

[GlobalClass]
public partial class 商品组件 : TextureRect
{
	[Signal] public delegate void 鼠标进入商品EventHandler(商品组件 商品);
	[Signal] public delegate void 鼠标离开商品EventHandler(商品组件 商品);
	[Signal] public delegate void 购买请求EventHandler(商品组件 商品, int 价格);

	[Export] private 卡牌数据 商品卡牌数据;
	[Export] private int 价格 = 10;
	
	// 您自己放置在商品组件下的节点，在检查器中拖拽绑定
	[Export] private Label 价格标签;    // 显示价格文本的 Label
	[Export] public  Button 购买按钮;   // 购买按钮

	public override void _Ready()
	{
		if (商品卡牌数据 == null)
		{
			GD.PrintErr($"商品 {Name} 没有设置卡牌数据");
			return;
		}

		// 鼠标交互（TextureRect 自带）
		MouseFilter = MouseFilterEnum.Pass;
		MouseEntered += () => EmitSignal(SignalName.鼠标进入商品, this);
		MouseExited  += () => EmitSignal(SignalName.鼠标离开商品, this);

		// 设置价格标签文本
		if (价格标签 != null)
		{
			价格标签.Text = $"{价格}金币";
		}
		else
		{
			GD.PrintErr($"{Name}: 未绑定价格标签节点");
		}

		// 绑定购买按钮
		if (购买按钮 != null)
		{
			购买按钮.Pressed += () => EmitSignal(SignalName.购买请求, this, 价格);
		}
		else
		{
			GD.PrintErr($"{Name}: 未绑定购买按钮节点");
		}

		// 可选：自动设置卡牌图片（如果您希望在代码中设置）
		if (商品卡牌数据.卡面贴图 != null && Texture == null)
		{
			Texture = 商品卡牌数据.卡面贴图;
		}
	}

	public 卡牌数据 获取卡牌数据() => 商品卡牌数据;

	/// <summary>
	/// 购买成功后调用此方法，可以禁用按钮、改变文本等
	/// </summary>
	public void 执行购买效果()
	{
		if (购买按钮 != null)
		{
			购买按钮.Disabled = true;
			购买按钮.Text = "已购";
		}
		// 也可以淡出商品等效果，您自己决定
	}
}
