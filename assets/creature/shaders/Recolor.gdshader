shader_type canvas_item;
uniform sampler2D palette: filter_nearest;
void fragment() {
	float alpha = COLOR.a;
	COLOR = texture(palette, vec2(COLOR.r + .05, .5));
	COLOR.a *= alpha;
}