using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class 卡牌战斗单位 : GodotObject
{
	public string 名字 { get; set; } = "怪物";

	private int _生命值;
	private int _护盾值;
	private float _行动条 = 0f;


	private Godot.Collections.Array<卡牌实例> _主卡组 = new();
	private Stack<卡牌实例> _抽牌堆 = new();
	private Godot.Collections.Array<卡牌实例> _弃牌堆 = new();
	private Godot.Collections.Array<卡牌实例> _手牌 = new();

	private bool 已经重置过抽牌堆 = false;

	// 教程模式相关


	public int 速度值 { get; set; } = 65;
  private int _最大生命值;
	public int 最大生命值
	{
		get => _最大生命值;
		set => _最大生命值 = value;
	}
 public int 生命值
	{
		get => _生命值;
		set
		{
			_生命值 = value;
			if (_最大生命值 == 0 && value > 0) // 首次设置生命值时自动记录最大生命值
				_最大生命值 = value;
		}
	}

	public int 护盾值
	{
		get => _护盾值;
		set => _护盾值 = value;
	}

	public float 行动条
	{
		get => _行动条;
		set => _行动条 = value;
	}



	public Godot.Collections.Array<卡牌实例> 主卡组 => _主卡组;
	public Stack<卡牌实例> 抽牌堆 => _抽牌堆;
	public Godot.Collections.Array<卡牌实例> 弃牌堆 => _弃牌堆;
	public Godot.Collections.Array<卡牌实例> 手牌 => _手牌;

	public int 手牌上限 { get; set; } = 10;



// 新增属性：指向战斗管理器
public 卡牌战斗管理器 战斗管理器 { get; set; } = null;

	public void 初始化卡组(Godot.Collections.Array<卡牌数据> 卡组数据)
	{
		GD.Print($"[{名字}] 初始化卡组，传入数据数量: {卡组数据?.Count ?? 0}");
		_主卡组.Clear();
		if (卡组数据 == null)
		{
			GD.PrintErr($"[{名字}] 卡组数据为空！");
			return;
		}
		foreach (var 数据 in 卡组数据)
		{
			if (数据 == null)
			{
				GD.PrintErr($"[{名字}] 发现 null 卡牌数据，跳过");
				continue;
			}
			var 卡牌实例 = new 卡牌实例(数据);
			_主卡组.Add(卡牌实例);
			GD.Print($"[{名字}] 添加卡牌: {数据.卡牌名称}");
		}
		GD.Print($"[{名字}] 主卡组数量: {_主卡组.Count}");
		洗牌();
	}

	public void 洗牌()
	{
		if (_主卡组.Count == 0)
		{
			GD.PrintErr($"[{名字}] 主卡组为空，无法洗牌");
			return;
		}
		var 随机 = new System.Random();
		var 临时列表 = new List<卡牌实例>();
		foreach (var 卡牌 in _主卡组) 临时列表.Add(卡牌);
		for (int i = 临时列表.Count - 1; i > 0; i--)
		{
			int j = 随机.Next(i + 1);
			(临时列表[i], 临时列表[j]) = (临时列表[j], 临时列表[i]);
		}
		_抽牌堆.Clear();
		foreach (var 卡牌 in 临时列表)
		{
			卡牌.状态 = 卡牌实例.卡牌状态.在卡组中;
			_抽牌堆.Push(卡牌);
		}
		GD.Print($"[{名字}] 洗牌完成，抽牌堆数量: {_抽牌堆.Count}");
	}

	public void 回合开始准备()
	{
		GD.Print($"[{名字}] 回合开始准备，弃牌堆数量: {_弃牌堆.Count}");
		if (_弃牌堆.Count > 0)
		{
			var 随机 = new System.Random();
			var 临时列表 = new List<卡牌实例>();
			foreach (var 卡牌 in _弃牌堆)
			{
				if (卡牌 != null)
				{
					卡牌.状态 = 卡牌实例.卡牌状态.在卡组中;
					临时列表.Add(卡牌);
				}
			}
			var 当前抽牌堆列表 = new List<卡牌实例>();
			while (_抽牌堆.Count > 0)
			{
				var 卡牌 = _抽牌堆.Pop();
				if (卡牌 != null) 当前抽牌堆列表.Add(卡牌);
			}
			临时列表.AddRange(当前抽牌堆列表);
			for (int i = 临时列表.Count - 1; i > 0; i--)
			{
				int j = 随机.Next(i + 1);
				(临时列表[i], 临时列表[j]) = (临时列表[j], 临时列表[i]);
			}
			_抽牌堆.Clear();
			_弃牌堆.Clear();
			foreach (var 卡牌 in 临时列表) _抽牌堆.Push(卡牌);
			GD.Print($"[{名字}] 重置抽牌堆后，抽牌堆数量: {_抽牌堆.Count}");
		}
	}

	// 修改 抽牌() 方法开头：
public 卡牌实例 抽牌()
{
	// 优先检查是否由战斗管理器自定义抽牌
	if (战斗管理器 != null && 战斗管理器.自定义抽牌逻辑 != null)
	{
		var 自定义牌 = 战斗管理器.自定义抽牌逻辑(this);
		if (自定义牌 != null)
		{
			// 处理手牌上限
			if (_手牌.Count >= 手牌上限)
			{
				_弃牌堆.Add(自定义牌);
				自定义牌.状态 = 卡牌实例.卡牌状态.已弃置;
				return null;
			}
			自定义牌.状态 = 卡牌实例.卡牌状态.在手牌中;
			_手牌.Add(自定义牌);
			发射手牌变化信号();
			return 自定义牌;
		}
	}

	// ----- 原有正常抽牌逻辑（完全不变） -----
	if (_抽牌堆.Count == 0 && _弃牌堆.Count > 0)
	{
		回合开始准备();
	}
	if (_抽牌堆.Count == 0)
	{
		GD.PrintErr($"[{名字}] 没有可抽的卡牌");
		return null;
	}
	var 抽到的牌 = _抽牌堆.Pop();
	if (_手牌.Count >= 手牌上限)
	{
		_弃牌堆.Add(抽到的牌);
		抽到的牌.状态 = 卡牌实例.卡牌状态.已弃置;
		return null;
	}
	抽到的牌.状态 = 卡牌实例.卡牌状态.在手牌中;
	_手牌.Add(抽到的牌);
	发射手牌变化信号();
	return 抽到的牌;
}

	private void 重置抽牌堆()
	{
		if (_弃牌堆.Count == 0) return;
		var 随机 = new System.Random();
		var 临时列表 = new List<卡牌实例>();
		foreach (var 卡牌 in _弃牌堆)
		{
			if (卡牌 != null)
			{
				卡牌.状态 = 卡牌实例.卡牌状态.在卡组中;
				临时列表.Add(卡牌);
			}
		}
		for (int i = 临时列表.Count - 1; i > 0; i--)
		{
			int j = 随机.Next(i + 1);
			(临时列表[i], 临时列表[j]) = (临时列表[j], 临时列表[i]);
		}
		_抽牌堆.Clear();
		foreach (var 卡牌 in 临时列表) _抽牌堆.Push(卡牌);
		_弃牌堆.Clear();
		GD.Print($"[{名字}] 重置抽牌堆完成，新抽牌堆数量: {_抽牌堆.Count}");
	}

	public void 弃牌(卡牌实例 卡牌)
	{
		_手牌.Remove(卡牌);
		_弃牌堆.Add(卡牌);
		卡牌.状态 = 卡牌实例.卡牌状态.已弃置;
		GD.Print($"[{名字}] 弃牌: {卡牌?.基础数据?.卡牌名称 ?? "null"}，当前手牌: {_手牌.Count}");
		发射手牌变化信号();
	}

	public void 使用卡牌(卡牌实例 卡牌)
	{
		_手牌.Remove(卡牌);
		_弃牌堆.Add(卡牌);
		卡牌.状态 = 卡牌实例.卡牌状态.已使用;
		卡牌.重置状态();
		GD.Print($"[{名字}] 使用卡牌: {卡牌?.基础数据?.卡牌名称 ?? "null"}，当前手牌: {_手牌.Count}");
		发射手牌变化信号();
	}

	public void 打印卡组状态()
	{
		GD.Print($"[{名字}] 打印卡组状态: 主卡组={_主卡组.Count}, 抽牌堆={_抽牌堆.Count}, 弃牌堆={_弃牌堆.Count}, 手牌={_手牌.Count}");
	}

	[Signal]
	public delegate void 手牌变化EventHandler();

	private void 发射手牌变化信号()
	{
		EmitSignal(SignalName.手牌变化);
	}

	public void 抽起始手牌(int 数量 = 2)
	{
		GD.Print($"[{名字}] 开始抽起始手牌，数量: {数量}");
		for (int i = 0; i < 数量; i++)
		{
			var 卡牌 = 抽牌();
			if (卡牌 == null)
			{
				GD.PrintErr($"[{名字}] 抽起始手牌失败：第{i+1}张无法抽取，抽牌堆为空");
				break;
			}
		}
		GD.Print($"[{名字}] 抽起始手牌完成，当前手牌: {_手牌.Count}");
	}
}
