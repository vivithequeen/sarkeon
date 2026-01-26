shader_type canvas_item;
uniform sampler2D palette: filter_nearest;

uniform float currentRow : hint_range(0.0, 1.0);

void fragment() {

	
	float alpha = COLOR.a;
	COLOR = texture(palette, vec2(COLOR.r, currentRow));
	COLOR.a *= alpha;

	
}