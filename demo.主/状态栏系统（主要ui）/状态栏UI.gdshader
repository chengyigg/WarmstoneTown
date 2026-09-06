shader_type canvas_item;

// 直接写死颜色（HSV 120°, 35%, 65%）
uniform vec3 hsv_color = vec3(0.333, 0.35, 0.65); // 已经对了！
uniform float border_thickness : hint_range(0.0, 0.5) = 0.0;

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
            COLOR = vec4(1.0, 1.0, 1.0, 1.0);
        } else {
            COLOR = vec4(rgb_color, 1.0);
        }
    } else {
        discard; // 或者 COLOR = vec4(0.0);
    }
}
