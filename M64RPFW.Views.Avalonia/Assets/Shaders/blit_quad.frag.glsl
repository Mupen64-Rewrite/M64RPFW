#version 330 core

in vec2 uv;
out vec3 color;

layout(location = 0) uniform sampler2D tex;

void main() {
    color = texture(tex, uv);
}