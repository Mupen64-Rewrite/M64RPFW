#version 330 core
#extension GL_ARB_explicit_uniform_location : enable

in vec2 uv;
out vec4 color;

layout(location = 0) uniform sampler2D tex;

void main() {
    color = texture(tex, uv);
}