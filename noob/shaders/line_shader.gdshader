shader_type canvas_item;
uniform vec4 colors[4];
uniform float speed_mul = 1.f;
uniform vec2 sizer = vec2(1.f,1.f);
void fragment() {
	int index = int(mod((UV.y*2.f*sizer.y+sin(TIME*0.1f*speed_mul)+cos(floor(UV.x * sizer.x * 100.f)*2000.f+floor(UV.y * sizer.y * 100.f)*1.f * cos(TIME*.1f*speed_mul))),4));
	COLOR = colors[index] * COLOR;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
