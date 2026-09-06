shader_type canvas_item;

// 修改为选项A的遮罩色：H:100° S:25% V:30% 透明度0.75
uniform vec3 hsv_color = vec3(0.2778, 0.25, 0.3); // H:100/360≈0.2778, S:25%, V:30%
uniform float border_thickness : hint_range(0.0, 0.5) = 0.0;
uniform float alpha = 0.75; // 添加透明度控制

// HSV转RGB函数（修正版）
vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void fragment() {
    vec2 center = vec2(0.5, 0.5);
    float dist = distance(UV, center);

    // 计算颜色
    vec3 rgb_color = hsv2rgb(hsv_color);

    if (dist < 0.5) {
        if (border_thickness > 0.0 && dist > 0.5 - border_thickness) {
            // 边框颜色（如果需要，可以调整为其他颜色）
            COLOR = vec4(1.0, 1.0, 1.0, alpha);
        } else {
            // 主要颜色区域
            COLOR = vec4(rgb_color, alpha);
        }
    } else {
        // 圆形以外的区域完全透明
        COLOR = vec4(0.0, 0.0, 0.0, 0.0);
    }
}