sampler uImage0 : register(s0);

float progress;
float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float dist = sqrt((coords.x - 0.5) * (coords.x - 0.5) + (coords.y - 0.5) * (coords.y - 0.5));
    float rot = atan2(coords.y - 0.5, coords.x - 0.5) - 3.14159 / 2;
    if (rot < -3.14159)
        rot = 3.14159 * 2 + rot;
    float r = progress * 3.14159 * 2 - 3.14159;
    if (dist > 0.5)
    {
        return float4(0, 0, 0, 0);
    }
    if (dist > 0.4)
    {
        return float4(0, 0, 0, 1);
    }
    if (rot < r)
    {
        return float4(1, 0.5, 0, 1);
    }

    return float4(0, 0, 0, 1);
}



technique Technique1
{
    pass PieEffect
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
