Shader "POLYCRUSHER/MovieMask" {
	Properties {
			[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
			_Mask ("Mask", 2D) = "white" {}
			_Color ("Color", Color) = (1, 1, 1, 1)

			_StencilComp ("Stencil Comparison", Float) = 8
	     	_Stencil ("Stencil ID", Float) = 0
	     	_StencilOp ("Stencil Operation", Float) = 0
	     	_StencilWriteMask ("Stencil Write Mask", Float) = 255
	     	_StencilReadMask ("Stencil Read Mask", Float) = 255
	     	_ColorMask ("Color Mask", Float) = 15
	}
	SubShader {
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Stencil {
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
		LOD 200

		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				// Use shader model 3.0 target, to get nicer looking lighting
				#pragma target 3.0

				// prevent auto-normalize of normals & tangents on GLSL
				// because normals & tangents are used for a different purpose
				#pragma glsl_no_auto_normalization

				#include "UnityCG.cginc"
				#include "UnityUI.cginc"

				sampler2D _MainTex;
				sampler2D _Mask;

				struct appdata_t
				{
						float4 vertex           : POSITION;
						fixed4 color            : COLOR;
						half2 texcoord          : TEXCOORD0;
						half4 uvRect            : TANGENT;
				};

				struct v2f
				{
						float4 vertex           : SV_POSITION;
						fixed4 color            : COLOR;
						half2  texcoord         : TEXCOORD0;
						half4 uvRect            : TEXCOORD2;
				};

				fixed4 _Color;

				v2f vert(appdata_t IN)
				{
						v2f OUT;
						float4 pos = IN.vertex;
						OUT.vertex = mul(UNITY_MATRIX_MVP, pos);
						OUT.texcoord = IN.texcoord;
						OUT.uvRect = IN.uvRect;
						OUT.color = IN.color;
						return OUT;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
						fixed3 color = tex2D(_MainTex, IN.texcoord).rgb * _Color.rgb;
						fixed4 mask = tex2D(_Mask, IN.texcoord);
						float uvClip = UnityGet2DClipping(IN.texcoord, IN.uvRect);
						mask.a *= uvClip;

						return fixed4(color.r, color.g, color.b, mask.a);
				}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
