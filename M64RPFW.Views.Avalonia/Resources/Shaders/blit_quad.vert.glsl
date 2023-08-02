#version 330 core
#extension GL_ARB_explicit_uniform_location : enable

layout(location = 0) in vec2 vertPos;
layout(location = 1) in vec2 vertUV;

out vec2 uv;

void main() {
    gl_Position = vec4(vertPos, 0.0, 1.0);
    uv = vertUV;
}