Shader "POLYCRUSHER/GlowWithCutout" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

		_Cutoff ("Cutoff value", Range(0, 1)) = 0

		_EmissionTex ("Emission Texture", 2D) = "white" {}
		_EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
		_EmissionStrength ("Emission Strength", Float) = 0

		_OffsetSpeed ("Offset Speed", Float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull Off

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
		};
		sampler2D _MainTex;
		sampler2D _EmissionTex;

		fixed4 _Color;
		fixed4 _EmissionColor;
		fixed _EmissionStrength;
		half _Cutoff;
		half _OffsetSpeed;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float2 uv = IN.uv_MainTex + float2(_Time[0] * _OffsetSpeed, 0);
			fixed4 c = tex2D (_MainTex, uv) * _Color;

			if(c.a < _Cutoff)
				discard;

			o.Albedo = c.rgb;
			o.Emission = tex2D(_EmissionTex, uv) * _EmissionColor * _EmissionStrength;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
