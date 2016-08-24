Shader "POLYCRUSHER/TextGLowAndShadow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {
            }
        _Color ("Tint", Color) = (1,1,1,1)
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

		_ShadowShiftX ("Shadow Shift X", Float) = 5
		_ShadowShiftY ("Shadow Shift Y", Float) = 5
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass
        {
            CGPROGRAM
            #pragma vertex ui_vert
            #pragma fragment ui_frag
            #pragma target 3.0
 
            // prevent auto-normalize of normals & tangents on GLSL
            // because normals & tangents are used for a different purpose
            #pragma glsl_no_auto_normalization

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex           : POSITION;
                fixed4 color            : COLOR;
                half2 texcoord          : TEXCOORD0;
                half4 uvRect            : TANGENT;
                fixed3 glowColor        : NORMAL;
                half2 glowSize          : TEXCOORD1;
            }
;
            struct v2f
            {
                float4 vertex           : SV_POSITION;
                fixed4 color            : COLOR;
                half2  texcoord         : TEXCOORD0;
                half4 uvRect            : TEXCOORD2;
                fixed4 glowColor        : TEXCOORD3;
                half2 glowSize          : TEXCOORD4;
            }
;
            sampler2D _MainTex;
            fixed4 _Color;
            float _GlowSize;
            float _ShadowShiftX;
            float _ShadowShiftY;

            float SCurve (float x) {
                x = x * 2.0 - 1.0;
                return -x * abs(x) * 0.5 + x + 0.5;
            }

			float4 BlurH (sampler2D source, float2 size, float2 uv, float radius) {
                if (radius >= 1.0)
				{
                    float4 A = float4(0,0,0,0);
                    float4 C = float4(0,0,0,0);
                    float width = 1.0 / size.x;
                    float divisor = 0.0;
                    float weight = 0.0;
                    float radiusMultiplier = 1.0 / radius;
        
					for (float x = -radius; x <= radius; x++)
					{
                        A = tex2D(source, uv + float2(x * width, 0.0));
                        weight = SCurve(1.0 - (abs(x) * radiusMultiplier));
                        C += A * weight;
                        divisor += weight;
                    }
					return float4(C.r / divisor, C.g / divisor, C.b / divisor, 1.0);
                }
				return tex2D(source, uv);
            }

			float4 BlurV (sampler2D source, float2 size, float2 uv, float radius) {
                if (radius >= 1.0)
				{
                    float4 A = float4(0,0,0,0);
                    float4 C = float4(0,0,0,0);
                    float height = 1.0 / size.y;
                    float divisor = 0.0;
                    float weight = 0.0;
                    float radiusMultiplier = 1.0 / radius;
        
					for (float y = -radius; y <= radius; y++)
					{
                        A = tex2D(source, uv + float2(0.0, y * height));
						weight = SCurve(1.0 - (abs(y) * radiusMultiplier)); 
						C += A * weight; 
						divisor += weight;
                    }
					return float4(C.r / divisor, C.g / divisor, C.b / divisor, 1.0);
                }
				return tex2D(source, uv);
            }

            v2f ui_vert(appdata_t IN)
            {
                v2f OUT;
                IN.vertex.x += _ShadowShiftX;
                IN.vertex.y += _ShadowShiftY;
                float4 pos = IN.vertex;
                OUT.glowSize = IN.glowSize;
                OUT.glowColor.rgb = IN.glowColor;
                OUT.glowColor.a = saturate(IN.vertex.z);
                pos.z = 0.0f;
                OUT.vertex = mul(UNITY_MATRIX_MVP, pos);
                OUT.texcoord = IN.texcoord;
                OUT.uvRect = IN.uvRect;
                OUT.color = IN.color * _Color;
                return OUT;
            }


            fixed4 ui_frag(v2f IN) : SV_Target
            {
                fixed4 color = fixed4(0,0,0,0);

                // defines UV offsets (scaled by glow size) where to sample from the texture
                float2 g_kernelOffsets[8] = {
					float2(-0.7f,-0.7f), float2(0.f,-1.f), float2(0.7f,-0.7f),
                    float2(-1.f , 0.0f)                  , float2(1.f , 0.f ),
                    float2(-0.7f, 0.7f), float2(0.f, 1.f), float2(0.7f, 0.7f)};

                half2 tc = IN.texcoord;
                float uvClip;
                float4 glow = float4(0,0,0,0);

                for(int i=0; i<8; ++i)
                {
                    // build new UV coords with offset by the kernel
                    half2 glowTC = tc + g_kernelOffsets[i] * IN.glowSize;
                    // calculate clipping
                    uvClip = UnityGet2DClipping(glowTC, IN.uvRect);
                    float4 sample = tex2D(_MainTex, glowTC);
                    // fonts are alpha only so use white
                    sample.rgb = float3(1,1,1);
                    glow.rgb += sample.rgb;
                    glow.a += sample.a * uvClip;
                }

                // divide by 8 to have an average color
                glow.rgb *= 0.125f;
                // mix with main texture output
                fixed3 glowColor = IN.glowColor.rgb * glow.rgb;
                fixed glowAlpha = IN.glowColor.a;
                glow.a *= glowAlpha * IN.color.a;
                color.rgb = lerp(glowColor, color.rgb, color.a);
                color.a += glow.a;
                return color;
            }

            ENDCG
        }

		Pass
        {
            CGPROGRAM
            #pragma vertex ui_vert
            #pragma fragment ui_frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex           : POSITION;
                fixed4 color            : COLOR;
                half2 texcoord          : TEXCOORD0;
                half4 uvRect            : TANGENT;
            }
;
            struct v2f
            {
                float4 vertex           : SV_POSITION;
                fixed4 color            : COLOR;
                half2  texcoord         : TEXCOORD0;
                half4 uvRect            : TEXCOORD2;
            }
;
            sampler2D _MainTex;
            fixed4 _Color;
            v2f ui_vert(appdata_t IN)
            {
                v2f OUT;
                float4 pos = IN.vertex;
                OUT.vertex = mul(UNITY_MATRIX_MVP, pos);
                OUT.texcoord = IN.texcoord;
                OUT.uvRect = IN.uvRect;
                OUT.color = IN.color * _Color;
                return OUT;
            }


            fixed4 ui_frag(v2f IN) : SV_Target
            {
                fixed4 color;
                color.rgb = IN.color.rgb;
                color.a = tex2D(_MainTex, IN.texcoord).a * IN.color.a;
                half2 tc = IN.texcoord;
                float uvClip = UnityGet2DClipping(tc, IN.uvRect);
                color.a *= uvClip;
                return color;
            }

            ENDCG
        }
    }


}