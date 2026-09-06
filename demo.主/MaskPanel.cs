using Godot;

public partial class MaskPanel : Panel
{
	// 通过 [Export] 可以在 Inspector 中拖拽赋值，避免硬编码路径
	[Export] private Control _maskWindow;   // 窥视窗节点（Panel3）

	public override void _Ready()
	{
		// 确保材质是 ShaderMaterial
		if (Material is ShaderMaterial shaderMaterial && _maskWindow != null)
		{
			// 初始化时立即更新一次
			UpdateMaskRect(shaderMaterial);
		}
	}

	public override void _Process(double delta)
	{
		if (Material is ShaderMaterial shaderMaterial && _maskWindow != null)
		{
			UpdateMaskRect(shaderMaterial);
		}
	}

	private void UpdateMaskRect(ShaderMaterial material)
	{
		// 获取窥视窗相对于 Panel2 的本地位置和大小
		Vector2 localPos = _maskWindow.Position;
		Vector2 localSize = _maskWindow.Size;

		material.SetShaderParameter("mask_position", localPos);
		material.SetShaderParameter("mask_size", localSize);
	}
}
