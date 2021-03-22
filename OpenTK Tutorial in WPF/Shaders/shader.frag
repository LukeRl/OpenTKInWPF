#version 430 core
//out vec4 FragColor;
//in vec3 vertexColor;
out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D texture0;
  
// uniform vec4 vertexColor; // the input variable from the vertex shader (same name and same type)  

void main()
{
//    FragColor = vec4(vertexColor, 1.0);
	outputColor = texture(texture0, texCoord);
} 
