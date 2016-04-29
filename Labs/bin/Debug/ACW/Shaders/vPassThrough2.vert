#version 330

uniform mat4 uModel2;
uniform mat4 uView2;
uniform mat4 uProjection2;

in vec3 vPosition2;

void main()
{
	gl_Position = vec4(vPosition2, 1) * uModel2 * uView2 * uProjection2;
}