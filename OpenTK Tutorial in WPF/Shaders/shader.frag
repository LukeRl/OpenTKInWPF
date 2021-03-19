#version 430 core
out vec4 FragColor;
in vec3 vertexColor;
  
// uniform vec4 vertexColor; // the input variable from the vertex shader (same name and same type)  

void main()
{
    FragColor = vec4(vertexColor, 1.0);
} 
