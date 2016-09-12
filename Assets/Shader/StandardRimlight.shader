Shader "POLYCRUSHER/StandardRimlight" {
	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_NormalMap ("Normal map", 2D) = "bump" {}
		_NormalIntensity ("Normal intenisty", Float) = 1

		_RimColor ("Rim Color", Color) = (1, 1, 1, 1)
		_RimPower ("Rim Power", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0


		struct Input {
			float2 uv_MainTex;
			float3 viewDir;
		};

		sampler2D _MainTex;
		sampler2D _NormalMap;

		half _Glossiness;
		half _Metallic;
		half _NormalIntensity;
		fixed4 _Color;
		half _RimPower;
		half4 _RimColor;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;

			half3 normalResult = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
			normalResult.z *= _NormalIntensity;
			o.Normal = normalize(normalResult);

			// Rim lighting
			half3 rim = pow(1.0 - saturate(dot(IN.viewDir, o.Normal)), _RimPower);

			o.Albedo = c.rgb + rim * _RimColor;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
