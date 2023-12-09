sampler uImage0 : register(s0);

float4 color;          //��ɫ
float n;                  //������
float width;           //�ߵĿ��
float k;             //������
float A(float x)
{
    if (x < 0.25)
        return sqrt(x * 0.5 - x * x);
    return 0.333 - 0.333 * x;
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float y = A(coords.x) * sin(2 * 3.14159 * n * coords.x) * k;
    if (abs((coords.y - 0.5) * 2 - y) < width)
        return color;
    return float4(0, 0, 0, 0);
}



technique Technique1
{
    pass CoilEffect
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
