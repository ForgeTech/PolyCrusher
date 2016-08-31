Shader "POLYCRUSHER/EnemyShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_EmissionTex ("Emission (RGB)", 2D) = "white" {}
		_Emission ("Emission", Float) = 0.0
		_EmissionColor ("Emission Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_MetallicTex ("Metallic (R), Smoothness (G)", 2D) = "white" {}

		_GlowEmissionColor ("Glow Color", Color) = (1,1,1,1)
		_GlowEmission ("Glow Emission", Float) = 1.0
		_EffectAmount ("Effect Amount", Range (0, 1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _EmissionTex;
		sampler2D _MetallicTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _GlowEmissionColor;
		half _GlowEmission;
		half _Emission;
		fixed4 _EmissionColor;
		float _EffectAmount;


		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 glowC = fixed4(1,1,1,1);
			o.Albedo = lerp(c.rgb, glowC.rgb, _EffectAmount);

			fixed4 e = tex2D (_EmissionTex, IN.uv_MainTex) * _EmissionColor * _Emission;
			fixed4 glowE = fixed4(1,1,1,1) * _GlowEmissionColor * _GlowEmission;
			o.Emission = lerp(e.rgb, glowE.rgb, _EffectAmount);

			half3 metallic = tex2D (_MetallicTex, IN.uv_MainTex).rgb;
			o.Metallic = metallic.r * _Metallic;
			o.Smoothness = metallic.g * _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
