﻿#version 430 core
layout (location = 0) in vec3 aPos; // the position variable has attribute position 0
layout (location = 1) in vec2 aTexCoord;
// layout (location = 1) in vec3 aColor; // the color variable has attribute position 1
  
// out vec3 vertexColor; // specify a color output to the fragment shader
out vec2 texCoord;

void main()
{
    texCoord = aTexCoord;
    gl_Position = vec4(aPos, 1.0); // see how we directly give a vec3 to vec4's constructor
    // vertexColor = vec4(0.5, 0.0, 0.0, 1.0); // set the output variable to a dark-red color
    // vertexColor = aColor; // color retrieved from the vertex data
}