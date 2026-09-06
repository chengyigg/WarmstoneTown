using Godot;

public partial class 点光源 : Node2D
{
	// 点光源属性（可在检查器中调整）
	[Export(PropertyHint.Range, "0,1000")]
	public float 光源大小 { get; set; } = 200f;
	
	[Export]
	public Vector2 光源位置 { get; set; } = Vector2.Zero;
	
	[Export(PropertyHint.Range, "0,10")]
	public float 光源亮度 { get; set; } = 1.5f;
	
	[Export]
	public Color 光源颜色 { get; set; } = new Color(1f, 0.95f, 0.9f);
	
	// 新增：初始开关状态
	[Export]
	public bool 初始开启 { get; set; } = true;
	
	public PointLight2D _点光源节点;

	public override void _Ready()
	{
		  // 创建点光源
	创建点光源();


		
		
	}

	private void 创建点光源()
	{
		// 创建点光源
		_点光源节点 = new PointLight2D();
		_点光源节点.Name = "点光源";
		_点光源节点.Texture = 创建圆形纹理();
		_点光源节点.Color = 光源颜色;
		_点光源节点.Energy = 光源亮度;
		_点光源节点.TextureScale = 光源大小 / 100f;
		_点光源节点.Position = 光源位置;
		_点光源节点.Enabled = 初始开启; // 使用 Enabled 属性控制开关
		
		AddChild(_点光源节点);
	}

	private Texture2D 创建圆形纹理()
	{
		// 创建图像
		int 尺寸 = 512;
		var 图像 = Image.Create(尺寸, 尺寸, false, Image.Format.Rgba8);
		图像.Fill(Colors.Transparent);
		
		// 绘制圆形
		float 半径 = 尺寸 / 2;
		float 半径平方 = 半径 * 半径;
		Vector2 中心 = new Vector2(半径, 半径);
		
		for (int y = 0; y < 尺寸; y++)
		{
			for (int x = 0; x < 尺寸; x++)
			{
				float dx = x - 中心.X;
				float dy = y - 中心.Y;
				float 距离平方 = dx * dx + dy * dy;
				
				if (距离平方 <= 半径平方)
				{
					float 距离 = Mathf.Sqrt(距离平方);
					float 透明度 = 1.0f - Mathf.Clamp(距离 / 半径, 0f, 1f);
					透明度 = Mathf.Pow(透明度, 0.5f);
					图像.SetPixel(x, y, new Color(1, 1, 1, 透明度));
				}
			}
		}
		
		// 创建纹理
		var 纹理 = new ImageTexture();
		纹理.SetImage(图像);
		return 纹理;
	}
	
	// 当属性变化时更新光源
	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		string 属性名 = property["name"].AsString();
		
		if (_点光源节点 != null)
		{
			if (属性名 == "光源大小")
			{
				_点光源节点.TextureScale = 光源大小 / 100f;
			}
			else if (属性名 == "光源位置")
			{
				_点光源节点.Position = 光源位置;
			}
			else if (属性名 == "光源亮度")
			{
				_点光源节点.Energy = 光源亮度;
			}
			else if (属性名 == "光源颜色")
			{
				_点光源节点.Color = 光源颜色;
			}
			else if (属性名 == "初始开启")
			{
				_点光源节点.Enabled = 初始开启;
			}
		}
	}
	
	// 新增：公共方法用于开关光源
	public void 开启光源()
	{
		if (_点光源节点 != null)
		{
			_点光源节点.Enabled = true;
		}
	}
	
	public void 关闭光源()
	{
		if (_点光源节点 != null)
		{
			_点光源节点.Enabled = false;
		}
	}
	
	public void 切换光源()
	{
		if (_点光源节点 != null)
		{
			_点光源节点.Enabled = !_点光源节点.Enabled;
		}
	}
	
	// 新增：获取当前光源状态
	public bool 是否开启()
	{
		return _点光源节点 != null && _点光源节点.Enabled;
	}
}
