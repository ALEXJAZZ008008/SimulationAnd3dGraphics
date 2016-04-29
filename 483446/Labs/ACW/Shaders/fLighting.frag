#version 330

struct LightProperties
{
	vec4 Position;
	vec3 AmbientLight;
	vec3 DiffuseLight;
	vec3 SpecularLight;
};

struct MaterialProperties
{
	vec3 AmbientReflectivity;
	vec3 DiffuseReflectivity;
	vec3 SpecularReflectivity;
	float Shininess;
};

uniform LightProperties uLight[3];
uniform MaterialProperties uMaterial;
uniform vec4 uEyePosition;
uniform vec4 uColour;

in vec4 oNormal;
in vec4 oSurfacePosition;

out vec4 FragColour;

void main()
{
	vec4 eyeDirection = normalize(uEyePosition - oSurfacePosition);

	for(int i = 0; i < 3; ++i)
	{
		vec4 lightDir = normalize(uLight[i].Position - oSurfacePosition);

		float diffuseFactor = max(dot(oNormal, lightDir), 0);

		vec4 reflectedVector = reflect(-lightDir, oNormal);
		float specularFactor = pow(max(dot( reflectedVector, eyeDirection), 0.0),uMaterial.Shininess);

		FragColour = FragColour + (vec4(uLight[i].AmbientLight * uMaterial.AmbientReflectivity + uLight[i].DiffuseLight * uMaterial.DiffuseReflectivity * diffuseFactor + uLight[i].SpecularLight * uMaterial.SpecularReflectivity * specularFactor, 1) + uColour);
	}
}