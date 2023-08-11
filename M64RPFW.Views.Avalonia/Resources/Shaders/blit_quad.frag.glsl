#version 330 core

// 0: default (blit texture)
// 1: grayscale alpha mask
// 2: override directly with colour
#define MODE 1

in vec2 uv;
out vec4 color;

uniform sampler2D tex;

void main() {
    #if MODE == 0
    color = texture(tex, uv);
    #elif MODE == 1
    color = vec4(texture(tex, uv).aaa, 1.0f);
    #elif MODE == 2
    color = vec4(texture(tex, uv).rgb, 1.0f);
    #endif
}