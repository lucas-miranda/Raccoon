#include "Structures.fxh"
#include "Helper.fxh"

Texture2D Texture;
sampler2D TextureSampler = sampler_state {
    Texture = <Texture>;
    MipFilter = NONE;
    MinFilter = Point;
    MagFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

float4x4 WorldViewProj;
float4 DiffuseColor;

float4 TextureAreaClip;
float2 Repeat;

float2 repeat(float2 textureCoord) {
    return TextureAreaClip.xy + TextureAreaClip.zw * fmod(((textureCoord - TextureAreaClip.xy) / TextureAreaClip.zw) * Repeat, 1.0f);
}

// Vertex Color

VSOut_PosColor VSVertexColor(VSIn_PosColor input) {
    VSOut_PosColor output;

    output.Position = mul(float4(input.Position.x, input.Position.y, 0.0f, 1.0f), WorldViewProj);
    output.PositionPS = input.Position;
    output.Diffuse = DiffuseColor * input.Color;

    return output;
}

float4 PSVertexColor(PSIn_PosColor input) : COLOR0 {
    if (input.Diffuse.a < .01f) {
        discard;
    }

    return float4(input.Diffuse.rgb, 1.0f) * input.Diffuse.a;
}


// Vertex Color, Texture

VSOut_PosColorTexture VSVertexColorTexture(VSIn_PosColorTexture input) {
    VSOut_PosColorTexture output;

    output.Position = mul(float4(input.Position.x, input.Position.y, 0.0f, 1.0f), WorldViewProj);
    output.PositionPS = input.Position;
    output.Diffuse = DiffuseColor * input.Color;
    output.TextureCoord = input.TextureCoord;

    return output;
}

float4 PSVertexColorTexture(PSIn_PosColorTexture input) : COLOR0 {
    float4 px = tex2D(TextureSampler, repeat(input.TextureCoord));
    px = float4(px.rgb * input.Diffuse.rgb, 1.0f) * input.Diffuse.a * px.a;

    if (px.a < .01f) {
        discard;
    }

    return px;
}


// Vertex Color, Depth

VSOut_PosColorDepth VSVertexColorDepth(VSIn_PosColor input) {
    VSOut_PosColorDepth output;

    output.Position = mul(float4(input.Position.x, input.Position.y, 0.0f, 1.0f), WorldViewProj);
    output.PositionPS = input.Position;
    output.Depth = input.Position.z / input.Position.w;
    output.Diffuse = DiffuseColor * input.Color;

    return output;
}

PSOut_ColorDepth PSVertexColorDepth(PSIn_PosColorDepth input) {
    PSOut_ColorDepth psOut;
    psOut.Color = float4(input.Diffuse.rgb, 1.0f) * input.Diffuse.a;

    if (psOut.Color.a < .01f) {
        discard;
    }

    psOut.Depth = input.Depth;
    return psOut;
}

// Vertex Color, Texture, Depth

VSOut_PosColorTextureDepth VSVertexColorTextureDepth(VSIn_PosColorTexture input) {
    VSOut_PosColorTextureDepth output;

    output.Position = mul(float4(input.Position.x, input.Position.y, 0.0f, 1.0f), WorldViewProj);
    output.PositionPS = input.Position;
    output.Depth = input.Position.z / input.Position.w;
    output.Diffuse = DiffuseColor * input.Color;
    output.TextureCoord = input.TextureCoord;

    return output;
}

PSOut_ColorDepth PSVertexColorTextureDepth(PSIn_PosColorTextureDepth input) {
    PSOut_ColorDepth psOut;
    psOut.Color = tex2D(TextureSampler, repeat(input.TextureCoord));
    psOut.Color = float4(psOut.Color.rgb * input.Diffuse.rgb, 1.0f) * input.Diffuse.a * psOut.Color.a;

    if (psOut.Color.a < .01f) {
        discard;
    }

    psOut.Depth = input.Depth;
    return psOut;
}


// Techniques

technique Repeat_PosColor {
	pass {
		VertexShader = compile vs_3_0 VSVertexColor();
		PixelShader  = compile ps_3_0 PSVertexColor();
	}
};

technique Repeat_PosColor_Texture {
	pass {
		VertexShader = compile vs_3_0 VSVertexColorTexture();
		PixelShader  = compile ps_3_0 PSVertexColorTexture();
	}
};

technique Repeat_PosColor_Depth {
	pass {
		VertexShader = compile vs_3_0 VSVertexColorDepth();
		PixelShader  = compile ps_3_0 PSVertexColorDepth();
	}
};

technique Repeat_PosColor_Texture_Depth {
	pass {
		VertexShader = compile vs_3_0 VSVertexColorTextureDepth();
		PixelShader  = compile ps_3_0 PSVertexColorTextureDepth();
	}
};
