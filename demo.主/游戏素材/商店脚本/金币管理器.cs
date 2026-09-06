using Godot;

namespace 你的项目.Scripts.全局
{
	public partial class 金币管理器 : Node
	{
		public static 金币管理器 实例 { get; private set; }

		[Signal]
		public delegate void 金币数量已变化EventHandler(int 新数量);

		private int _金币数量 = 0;

		public int 金币数量
		{
			get => _金币数量;
			set
			{
				if (_金币数量 != value)
				{
					_金币数量 = value;
					EmitSignal(SignalName.金币数量已变化, _金币数量);
				}
			}
		}

		public override void _EnterTree()
		{
			if (实例 == null)
			{
				实例 = this;
			}
			else
			{
				QueueFree();
			}
		}

		public void 增加金币(int 增加量)
{
	GD.Print($"[金币管理器] 增加金币被调用，增加量: {增加量}，当前金币: {_金币数量}");
	金币数量 += 增加量;
	GD.Print($"[金币管理器] 增加后金币: {_金币数量}");
}

		public void 减少金币(int 减少量)
		{
			金币数量 -= 减少量;
		}
	}
}
