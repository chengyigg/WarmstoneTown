shader_type canvas_item;

// 接收窥视窗的矩形信息（相对于 Panel2 的本地坐标）
uniform vec2 mask_position;
uniform vec2 mask_size;

void fragment() {
    // UV 范围是 (0,0) 左上角 ~ (1,1) 右下角
    // 将 UV 转换为 Panel2 本地像素坐标（假设 Panel2 的大小为 TEXTURE_PIXEL_SIZE 的倒数）
    vec2 local_pos = UV * TEXTURE_PIXEL_SIZE; // TEXTURE_PIXEL_SIZE = 1.0 / 纹理尺寸

    // 判断当前像素是否在窥视窗矩形内
    bool inside_mask = (local_pos.x >= mask_position.x &&
                        local_pos.x <= mask_position.x + mask_size.x &&
                        local_pos.y >= mask_position.y &&
                        local_pos.y <= mask_position.y + mask_size.y);

    // 如果在矩形内，完全透明（露出底层的 Panel1）
    if (inside_mask) {
        COLOR.a = 0.0;
    } else {
        // 其余区域保留原有颜色（可改为半透明黑等遮罩效果）
        COLOR = texture(TEXTURE, UV);
    }
}