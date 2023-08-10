#version 330 core
#extension GL_ARB_explicit_uniform_location : enable

layout(location = 0) in vec2 vertPos;

out vec2 uv;

void main() {
    gl_Position.xy = vertPos;
    gl_Position.zw = vec2(0.0, 1.0);
    uv = vertPos * 0.5f + 0.5f;
}