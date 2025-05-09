Shader"Unlit/TronURP"
{
    Properties
    {
        [Header(Base)]
        [Space(5)]
        _Color ("Color", Color) = (1,1,1,1)

        [Header(Outline)]
        [Space(5)]
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _WidthOutline("Width Outline", Range (0,1)) = 0.8
        _WidthOutlineSharpness ("Width Outline Sharpness", Range(0,1)) = 1

        [Header(Grid)]
        [Space(5)]
        _GridColor ("Grid Color", Color) = (1,1,1,1)
        _WidthGrid("Width Grid", Range (0,1)) = 0.2
        _WidthGridSharpness("Width Grid Sharpness", Range(0,1)) = 1

        _NumberHorizzontal("Number Horizontal", Range(0,1)) = 0
        _NumberVertical("Number Vertical", Range(0,1)) = 0

        [Header(Pulse)]
        [Space(5)]
        _Frequency ("Frequency", Range(0,100)) = 1

        [Header(Emission)]
        [Space(5)]
        [HDR]_EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
        _EmissionIntensity ("Emission Intensity", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
Name"Unlit"
            Tags
{"LightMode" = "UniversalForward"
}

HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            UNITY_INSTANCING_BUFFER_START(Props)
                // Add instanced properties here if needed
            UNITY_INSTANCING_BUFFER_END(Props)

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv : TEXCOORD0;
    float4 positionHCS : SV_POSITION;
    float3 scale : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
};

float4 _Color;
float4 _OutlineColor;
float _WidthOutline;
float _WidthOutlineSharpness;
float4 _GridColor;
float _WidthGrid;
float _WidthGridSharpness;
float _NumberHorizzontal;
float _NumberVertical;
float _Frequency;
float4 _EmissionColor;
float _EmissionIntensity;

float3 GetObjectScale()
{
    float3x3 m = (float3x3) UNITY_MATRIX_M;
    return float3(length(m[0]), length(m[1]), length(m[2]));
}

Varyings vert(Attributes IN)
{
    Varyings OUT;
    UNITY_SETUP_INSTANCE_ID(IN);
    UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
    OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
    OUT.uv = IN.uv;
    OUT.scale = GetObjectScale();
    return OUT;
}

void ColorMask(float3 In, float3 MaskColor, float width, float widthLimiter, out float4 Out)
{
    float Distance = distance(MaskColor, In);
    Out = saturate(1 - (Distance - (1 - width)) / max((1 - widthLimiter), 1e-5));
}

void ChannelMask_Green(float4 In, out float4 Out)
{
    Out = float4(0, In.g, 0, In.a);
}

void ChannelMask_Red(float4 In, out float4 Out)
{
    Out = float4(In.r, 0, 0, In.a);
}

void Rotate_Degrees(float2 UV, float2 Center, float Rotation, out float2 Out)
{
    Rotation = radians(Rotation);
    UV -= Center;
    float s = sin(Rotation);
    float c = cos(Rotation);
    float2x2 rMatrix = float2x2(c, -s, s, c);
    UV = mul(UV, rMatrix);
    UV += Center;
    Out = UV;
}

void Mask(float4 col, float4 mask, float width, float widthLimiter, float sharpness, out float4 Out)
{
    ColorMask(col.rgb, mask.rgb, width, widthLimiter, Out);
    Out = pow(Out, sharpness);
}

half4 frag(Varyings i) : SV_Target
{
    float _ScaleY = i.scale.y;
    float _ScaleZ = i.scale.x;
    float _ScaleX = i.scale.z;

    float2 uv = i.uv;
    float2 uvr;
    Rotate_Degrees(i.uv, float2(0.5, 0.5), 180, uvr);

    float4 maskg = float4(0, 1, 0, 1);
    float4 maskr = float4(1, 0, 0, 1);

    float4 g, gr, r, rr;
    ChannelMask_Green(float4(uv, 1, 1), g);
    Mask(g, maskg, _WidthOutline / _ScaleX, _WidthOutlineSharpness, 0.5 * _ScaleX, g);

    ChannelMask_Green(float4(uvr, 1, 1), gr);
    Mask(gr, maskg, _WidthOutline / _ScaleX, _WidthOutlineSharpness, 0.5 * _ScaleX, gr);

    ChannelMask_Red(float4(uv, 1, 1), r);
    Mask(r, maskr, _WidthOutline / _ScaleZ, _WidthOutlineSharpness, 0.5 * _ScaleZ, r);

    ChannelMask_Red(float4(uvr, 1, 1), rr);
    Mask(rr, maskr, _WidthOutline / _ScaleZ, _WidthOutlineSharpness, 0.5 * _ScaleZ, rr);

    float2 uvGridg = sin(i.uv * _NumberHorizzontal * 100);
    float2 uvGridr = sin(i.uv * _NumberVertical * 100);

    float4 gGrid, rGrid;
    ChannelMask_Green(float4(uvGridg, 1, 1), gGrid);
    Mask(gGrid, maskg, 1 - (_WidthGrid / _ScaleX), 1 - _WidthGridSharpness, 0.5 * _ScaleX, gGrid);

    ChannelMask_Red(float4(uvGridr, 1, 1), rGrid);
    Mask(rGrid, maskr, 1 - (_WidthGrid / _ScaleZ), 1 - _WidthGridSharpness, 0.5 * _ScaleZ, rGrid);

    float4 green = 1 - (g * gr);
    float4 red = 1 - (r * rr);
    float4 outline = (green + red) - (green * red);

    float4 grid = (gGrid + rGrid) - (gGrid * rGrid);

    float4 baseOutline = 1 - outline;
    float4 baseGrid = lerp(0, baseOutline, 1 - grid) * _Color;

    float4 glow = lerp(0, grid, baseOutline) * _GridColor;
    glow += baseGrid;

    float t1 = (sin(_Time.y * _Frequency) + 1) / 2;
    float t2 = (cos(_Time.y * _Frequency) + 1) / 2;
    float t3 = (sin(_Time.y * _Frequency) + 1) / 2;

    glow *= (t1 + t2 + t3);

                // Add emission ONLY to the outline
    float4 emission = outline * _EmissionColor * _EmissionIntensity;

    return glow + emission;
}
            ENDHLSL
        }
    }
FallBack"Hidden/InternalErrorShader"
}
