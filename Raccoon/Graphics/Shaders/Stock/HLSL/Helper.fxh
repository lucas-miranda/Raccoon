//#define HALF_TEXEL_OFFSET

float isSame(float3 colorA, float3 colorB) {
    return abs(colorB.r - colorA.r) < 0.01f 
        && abs(colorB.g - colorA.g) < 0.01f
        && abs(colorB.b - colorA.b) < 0.01f;
}

float angle(float2 from, float2 to) {
    return atan2(to.x - from.x, to.y - from.y);
}

float wrapAngle(float rad) {
    float deg = degrees(rad);
    return deg >= 0.0f ? deg : (360.0f + deg);
}

float4 snapPixel(float4 position, float2 renderTargetSize) {
    /*
    float hpcX = renderTargetSize.x * 0.5;
    float hpcY = renderTargetSize.y * 0.5;

#ifdef HALF_TEXEL_OFFSET
    float hpcOX = -0.5;
    float hpcOY = -0.5;
#else
    float hpcOX = 0;
    float hpcOY = 0;
#endif	

    float4 snappedPos = position;

    float pos = floor((position.x / position.w) * hpcX + 0.5f) + hpcOX;
    snappedPos.x = pos / hpcX * position.w;

    pos = floor((position.y / position.w) * hpcY + 0.5f) + hpcOY;
    snappedPos.y = pos / hpcY * position.w;

    return snappedPos;
    */
    return position;
}

float4 samplePixel(sampler2D textureSampler, float2 textureCoord) {
    //return tex2Dgrad(textureSampler, frac(textureCoord), ddx(textureCoord), ddy(textureCoord));
    return tex2D(textureSampler, textureCoord);
}
