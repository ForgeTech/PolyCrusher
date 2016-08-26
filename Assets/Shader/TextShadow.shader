Shader "POLYCRUSHER/TextShadow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
		_ShadowColor ("Shadow Color", Color) = (0,0,0,0.2)
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

		_ShadowShiftX ("Shadow Shift X", Float) = 5
		_ShadowShiftY ("Shadow Shift Y", Float) = 5

		_Quality ("Blur Quality", Range(0,1)) = 0.05
		_Radius ("Blur Radius", Range(0.1,1)) = 0.1
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
			fixed4 _ShadowColor;
            float _ShadowShiftX;
            float _ShadowShiftY;
			float _Quality;
			float _Radius;


			float4 blur(sampler2D tex, float2 uv, float4 uvRect) {
                float4 col = float4(0,0,0,0);
                for(float r0 = 0.0; r0 < 1.0; r0 += _Radius) {
                    float r = r0 * _Quality;
                    for(float a0 = 0.0; a0 < 1.0; a0 += _Radius) {
                        float a = a0 * 6.283184;
						float2 blurcoord = uv + float2(cos(a), sin(a)) * r;
						float uvClip = UnityGet2DClipping(blurcoord, uvRect);
                        col.rgb += tex2D(tex, blurcoord).rgb;
						col.a += tex2D(tex, blurcoord).a * uvClip;
                    }

				}
				col *= _Radius * _Radius;
                return col;
            }


            v2f ui_vert(appdata_t IN)
            {
                v2f OUT;
                IN.vertex.x += _ShadowShiftX;
                IN.vertex.y += _ShadowShiftY;
                float4 pos = IN.vertex;
                pos.z = 0.0f;
                OUT.vertex = mul(UNITY_MATRIX_MVP, pos);
                OUT.texcoord = IN.texcoord;
                OUT.uvRect = IN.uvRect;
                OUT.color = IN.color * _Color;
                return OUT;
            }



            fixed4 ui_frag(v2f IN) : SV_Target
            {
                fixed4 color = blur(_MainTex, IN.texcoord, IN.uvRect);
				color.rgb = _ShadowColor.rgb;
				color.a *= _ShadowColor.a;
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