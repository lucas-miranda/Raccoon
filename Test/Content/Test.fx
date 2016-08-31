texture Texture;
sampler TextureSampler = sampler_state {
    Texture = <Texture>;
};

struct VertexShaderOutput {
    float4 Position : TEXCOORD0;
    float4 Color : COLOR0;
    float2 TextureCordinate : TEXCOORD0;
};

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0 {
    float4 color = tex2D(TextureSampler, input.TextureCordinate) * input.Color;
    float value = 0.299 * color.r + 0.587 * color.g + 0.114 * color.b;
    color.r = value;
    color.g = value;
    color.b = value;
    color.a = 1.0f;
    //color.gb = color.r;
    return color;
}

technique BasicColorDrawing {
    pass P0 {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
