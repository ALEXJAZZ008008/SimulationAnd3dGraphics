#version 330

//These are variables used when drawing the objects
uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

//These are used to dictate the amount of light reflected
in vec3 vPosition;
in vec3 vNormal;

//These are used to dictate the amount of light reflected
out vec4 oNormal;
out vec4 oSurfacePosition;

void main()
{
	//These are used to dictate the amount of light reflected
	gl_Position = vec4(vPosition, 1) * uModel * uView * uProjection;

	oSurfacePosition = vec4(vPosition, 1) * uModel * uView;
	oNormal = vec4(normalize(vNormal * mat3(transpose(inverse(uModel * uView)))), 1);
}
