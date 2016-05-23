Shader "POLYCRUSHER/HeatwaveShader" {
	Properties {
		_Color ("Color, Trans (A)", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white"  {}
		[NoScaleOffset] [Normal]
		_BumpMap ("Normal map", 2D) = "bump" {}
		_Magnitude ("Magnitude", Range(0, 5)) = 0.05
		_LerpFactor ("Lerp factor", Range(-5, 1)) = 1
	}

	Category {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector" = "True"}
		Blend SrcAlpha OneMinusSrcAlpha
		SubShader {
			GrabPass { "_GrabTexture" Tags { "LightMode" = "Always" } }

			Pass {
				Tags { "LightMode" = "Always" }
				Cull Off

				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#include "UnityCG.cginc"

					sampler2D _GrabTexture;
					sampler2D _MainTex;
					sampler2D _BumpMap;
					float4 _Color;
					float _Magnitude;
					float _LerpFactor;

					// Is needed in combination with TRANSFORM_TEX
					float4 _MainTex_ST;

					struct vertInput {
						float4 vertex : POSITION;
						float2 texcoord : TEXCOORD0;
						float3 viewDir : TEXCOORD2;	// World space view direction
						float3 worldNormal : NORMAL;
					};

					struct vertOutput {
						float4 vertex : SV_POSITION;
						float2 texcoord : TEXCOORD0;
						float4 uvgrab : TEXCOORD1;
						float3 viewDir : TEXCOORD2;	// World space view direction
						float3 worldNormal : TEXCOORD3;
					};

					vertOutput vert (vertInput v) {
						vertOutput o;
						o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
						o.uvgrab = ComputeGrabScreenPos(o.vertex);
						o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
						o.worldNormal = normalize(mul(float4(v.worldNormal, 0.0), _World2Object).xyz);

						o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
						return o;
					}

					half4 frag(vertOutput i) : COLOR {
						half4 mainColor = tex2D(_MainTex, i.texcoord);
						half4 bump = tex2D(_BumpMap, i.texcoord);
						half2 distortion = UnpackNormal(bump).rg;

						// Rim border calculation
						float border = (abs(dot(i.viewDir, i.worldNormal)));
						float alpha = saturate(lerp(_LerpFactor, 1.0, border));

						// Distortion per UV coordinates
						i.uvgrab.xy += distortion * _Magnitude;
						fixed4 c = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
						half4 colorResult = c * mainColor * _Color;
						colorResult.a = alpha;

						return colorResult;
					}
				ENDCG
			}
		}
	}

	FallBack "Diffuse"
}
