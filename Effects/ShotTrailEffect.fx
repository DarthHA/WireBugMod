sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4x4 uTransform;
float4 color;
float progress;

struct VSInput
{
    float2 Pos : POSITION0;
    float4 Color : COLOR0;
    float3 Texcoord : TEXCOORD0;
};

struct PSInput
{
    float4 Pos : SV_POSITION;
    float4 Color : COLOR0;
    float3 Texcoord : TEXCOORD0;
};


float4 PixelShaderFunction(PSInput input) : COLOR0
{
    float3 coord = input.Texcoord;
    float X = coord.x + progress;
    if (X > 1)
    {
        X -= 1;
    }
    float4 c1 = tex2D(uImage0, float2(coord.x, coord.y));
    float4 c2 = tex2D(uImage1, float2(X, coord.y));
    c2 = clamp(coord.x, float4(1, 1, 1, 1), c2);
    return c1 * c2 * color;
}

PSInput VertexShaderFunction(VSInput input)
{
    PSInput output;
    output.Color = input.Color;
    output.Texcoord = input.Texcoord;
    output.Pos = mul(float4(input.Pos, 0, 1), uTransform);
    return output;
}


technique Technique1
{
    pass NormalVSEffect
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}