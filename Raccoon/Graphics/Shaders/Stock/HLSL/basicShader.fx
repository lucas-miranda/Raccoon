sampler2D Texture : register(s0);
float4x4 WorldViewProj;
float4 DiffuseColor;
float HealthPercentage;

// Pixel Shader out => Color, Depth

struct PSOut {
    float4 Color : COLOR0;
    float  Depth : DEPTH;
};


// Vertex Color

struct VSInVertexColor {
	float4 Position             : SV_POSITION;
	float4 Color                : COLOR0;
};

struct VSOutVertexColor {
	float4 Position             : SV_POSITION;
	float4 Diffuse              : COLOR0;
};

VSOutVertexColor VSVertexColor(VSInVertexColor input) {
    VSOutVertexColor output;

    output.Position = mul(float4(input.Position.x, input.Position.y, 0.0f, 1.0f), WorldViewProj);
    output.Diffuse = DiffuseColor * input.Color;

    return output;
}

float4 PSVertexColor(VSOutVertexColor input) : COLOR {
    return input.Diffuse;
}


// Vertex Color, Texture

struct VSInVertexColorTexture {
    float4 Position             : SV_POSITION;
    float4 Color                : COLOR0;
    float2 TextureCoord         : TEXCOORD0;
    float  Depth                : TEXCOORD1;
};

struct VSOutVertexColorTexture {
    float4 Position             : SV_POSITION;
    float4 Diffuse              : COLOR0;
    float2 TextureCoord         : TEXCOORD0;
};

VSOutVertexColorTexture VSVertexColorTexture(VSInVertexColorTexture input) {
    VSOutVertexColorTexture output;

    output.Position = mul(float4(input.Position.x, input.Position.y, 0.0f, 1.0f), WorldViewProj);
    output.Diffuse = DiffuseColor * input.Color;
    output.TextureCoord = input.TextureCoord;

    return output;
}

float4 PSVertexColorTexture(VSOutVertexColorTexture input) : COLOR0 {
    return tex2D(Texture, input.TextureCoord) * input.Diffuse;
}


// Vertex Color, Depth

struct VSOutVertexColorDepth {
	float4 Position             : SV_POSITION;
	float4 Diffuse              : COLOR0;
    float  Depth                : TEXCOORD0;
};

VSOutVertexColorDepth VSVertexColorDepth(VSInVertexColor input) {
    VSOutVertexColorDepth output;

    output.Position = mul(float4(input.Position.x, input.Position.y, 0.0f, 1.0f), WorldViewProj);
    output.Depth = input.Position.z / input.Position.w;
    output.Diffuse = DiffuseColor * input.Color;

    return output;
}

PSOut PSVertexColorDepth(VSOutVertexColorDepth input) {
    PSOut psOut;
    psOut.Color = input.Diffuse;
    
    if (psOut.Color.a < 1.0f) {
        psOut.Depth = 1.0f;
    } else {
        psOut.Depth = input.Depth;
    }

    return psOut;
}


// Vertex Color, Texture, Depth

struct VSOutVertexColorTextureDepth {
    float4 Position             : SV_POSITION;
    float4 Diffuse              : COLOR0;
    float2 TextureCoord         : TEXCOORD0;
    float  Depth                : TEXCOORD1;
};

VSOutVertexColorTextureDepth VSVertexColorTextureDepth(VSInVertexColorTexture input) {
    VSOutVertexColorTextureDepth output;

    output.Position = mul(float4(input.Position.x, input.Position.y, 0.0f, 1.0f), WorldViewProj);
    output.Depth = input.Position.z / input.Position.w;
    output.Diffuse = DiffuseColor * input.Color;
    output.TextureCoord = input.TextureCoord;

    return output;
}

PSOut PSVertexColorTextureDepth(VSOutVertexColorTextureDepth input) {
    PSOut psOut;
    psOut.Color = tex2D(Texture, input.TextureCoord) * input.Diffuse;

    if (psOut.Color.a < 1.0f) {
        psOut.Depth = 1.0f;
    } else {
        psOut.Depth = input.Depth;
    }

    return psOut;
}


// Techniques

technique Basic_VertexColor {
	pass P0 {
		VertexShader = compile vs_3_0 VSVertexColor();
		PixelShader  = compile ps_3_0 PSVertexColor();
	}
};

technique Basic_VertexColor_Texture {
	pass P0 {
		VertexShader = compile vs_3_0 VSVertexColorTexture();
		PixelShader  = compile ps_3_0 PSVertexColorTexture();
	}
};

technique Basic_VertexColor_Depth {
	pass P0 {
		VertexShader = compile vs_3_0 VSVertexColorDepth();
		PixelShader  = compile ps_3_0 PSVertexColorDepth();
	}
};

technique Basic_VertexColor_Texture_Depth {
	pass P0 {
		VertexShader = compile vs_3_0 VSVertexColorTextureDepth();
		PixelShader  = compile ps_3_0 PSVertexColorTextureDepth();
	}
};
