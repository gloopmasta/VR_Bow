// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Tron"
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
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 norm: NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                half3 scale: TEXCOORD1;
            };

            half3 ObjectScale() {
                return half3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
            }

            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex) ;           
                o.uv = v.uv  ;

                // Get Scale Object
                o.scale = ObjectScale();               

                return o;
            }

            float4 _OutlineColor;
            float _WidthOutline;
            float _WidthOutlineSharpness;

            float4 _GridColor;
            float _WidthGrid;
            float _WidthGridSharpness;

            float _NumberHorizzontal;
            float _NumberVertical;

            fixed4 _Color;
            
            float _Frequency;

            

            void ColorMask(float3 In, float3 MaskColor, float width, float widthLimiter, out float4 Out)
            {
                float Distance = distance(MaskColor , In);
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
                Rotation = Rotation * (3.1415926f/180.0f);
                UV -= Center;
                float s = sin(Rotation);
                float c = cos(Rotation);
                float2x2 rMatrix = float2x2(c, -s, s, c);
                rMatrix *= 0.5;
                rMatrix += 0.5;
                rMatrix = rMatrix * 2 - 1;
                UV.xy = mul(UV.xy, rMatrix);
                UV += Center;
                Out = UV;
            }   

            void Mask(float4 col, float4 mask, float width, float widthLimiter, float sharpness,  out float4 Out)
            {
                ColorMask(col, mask, width, widthLimiter, Out);
                Out = pow(Out, sharpness) ;
            }

            
            
            fixed4 frag (v2f i) : SV_Target
            {
                //>>>>>>>>>>>>>>>>>>>>>>>>   Outline   >>>>>>>>>>>>>>>>>>>>>>>>>>
                float _ScaleY = i.scale.y;
                float _ScaleZ = i.scale.x;
                float _ScaleX = i.scale.z;
    
                //float2 uv = sin(i.uv * _Number);
                float2 uv = i.uv;
                float2 uvr;
                Rotate_Degrees(i.uv, float2(0.5,0.5), 180, uvr);

                //mask
                float4 maskg = float4(0,1,0,1);
                float4 maskr = float4(1,0,0,1);

                //Green Channel
                float4 g;
                ChannelMask_Green(float4(uv.x, uv.y, 1, 1), g );
                Mask(g, maskg, _WidthOutline / _ScaleX,_WidthOutlineSharpness, 0.5 * _ScaleX, g );

                float4 gr;
                ChannelMask_Green(float4(uvr.x, uvr.y, 1, 1), gr );
                Mask(gr, maskg,_WidthOutline / _ScaleX, _WidthOutlineSharpness, 0.5 * _ScaleX, gr );

                //Red Channel
                float4 r;
                ChannelMask_Red(float4(uv.x, uv.y, 1, 1), r );
          
                Mask(r, maskr,_WidthOutline / _ScaleZ,_WidthOutlineSharpness,  0.5 * _ScaleZ, r );

                float4 rr;
                ChannelMask_Red(float4(uvr.x, uvr.y, 1, 1), rr );
                Mask(rr, maskr,_WidthOutline / _ScaleZ,  _WidthOutlineSharpness, 0.5 * _ScaleZ, rr );

                //>>>>>>>>>>>>>>>>>>>>>>>>   Grid   >>>>>>>>>>>>>>>>>>>>>>>>>>

                // Grid
                float2 uvGridg =  sin(i.uv * _NumberHorizzontal * 100);
                float2 uvGridr =  sin(i.uv * _NumberVertical * 100);

                //G
                float4 gGrid;
                ChannelMask_Green(float4(uvGridg.x, uvGridg.y, 1, 1), gGrid );
                Mask(gGrid, maskg, 1 - (_WidthGrid / _ScaleX),1 -  (_WidthGridSharpness), 0.5 * _ScaleX, gGrid);

                 //R
                float4 rGrid;
                ChannelMask_Red(float4(uvGridr.x, uvGridr.y, 1, 1), rGrid );
                Mask(rGrid, maskr,  1- (_WidthGrid / _ScaleZ),1 - (_WidthGridSharpness), 0.5 * _ScaleZ ,rGrid);

                //>>>>>>>>>>>>>>>>>>>>  Mix Outline  >>>>>>>>>>>>>>>>>>>>>>>>
                
                float4 green = 1 - (g * gr);
                float4 red = 1 - (r * rr);
                float4 outline = (green + red) - (green * red);

                //>>>>>>>>>>>>>>>>>>>>>  Mix Grid  >>>>>>>>>>>>>>>>>>>>>>>>

                float4 grid = (gGrid + rGrid) - (gGrid * rGrid);

                //>>>>>>>>>>>>>>>>>  Base   >>>>>>>>>>>>>>>>>>>

                float4 baseOutline = (1 - outline);
                float4 baseGrid = lerp(0,baseOutline,1 - grid) *_Color;

                //>>>>>>>>>>>>>>>>>  Mix Grid Outline >>>>>>>>>>>>>>>>>>>
                
                float4  Glow = lerp(0,grid, baseOutline ) * _GridColor;
                Glow += outline * _OutlineColor + baseGrid;
                
                float time1 = sin(_Time.x * _Frequency) + 1;
                time1 /= 2;

                float time2 =  cos(_Time.x * _Frequency) + 1;
                time2 /= 2;

                float time3 =  sin(_Time.x * _Frequency) + 1;
                time3 /= 2;

                Glow *= (time1 + time2 + time3);


                return Glow;

            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
