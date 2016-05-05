#version 330

//These are the properties of the lights
struct LightProperties
{
	vec4 Position;
	vec3 AmbientLight;
	vec3 DiffuseLight;
	vec3 SpecularLight;
};

//These are the properties of the materials
struct MaterialProperties
{
	vec3 AmbientReflectivity;
	vec3 DiffuseReflectivity;
	vec3 SpecularReflectivity;
	float Shininess;
};

//These are variables used when drawing the objects
uniform LightProperties uLight[4];
uniform MaterialProperties uMaterial;
uniform vec4 uEyePosition;
uniform vec4 uColour;

//These are used to dictate the amount of light reflected
in vec4 oNormal;
in vec4 oSurfacePosition;

//This is the colour reflected
out vec4 FragColour;

void main()
{
	//This is the position of the camera
	vec4 eyeDirection = normalize(uEyePosition - oSurfacePosition);

	//This draws all the lights
	for(int i = 0; i < 4; ++i)
	{
		//This is the direction of the light
		vec4 lightDir = normalize(uLight[i].Position - oSurfacePosition);

		//These are the types of light
		float diffuseFactor = max(dot(oNormal, lightDir), 0);
		vec4 reflectedVector = reflect(-lightDir, oNormal);
		float specularFactor = pow(max(dot(reflectedVector, eyeDirection), 0.0), uMaterial.Shininess);

		//This adds all the lights together
		FragColour = FragColour + vec4((uLight[i].AmbientLight * uMaterial.AmbientReflectivity) + (uLight[i].DiffuseLight * uMaterial.DiffuseReflectivity * diffuseFactor) + (uLight[i].SpecularLight * uMaterial.SpecularReflectivity * specularFactor), 1);
	}

	//This adds the colour of the object to the object
	FragColour = FragColour + uColour;
}