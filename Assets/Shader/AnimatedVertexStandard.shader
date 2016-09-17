Shader "POLYCRUSHER/AnimatedVertexStandard" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_Frequency ("Frequency", Float) = 0.0
		_Amplitude ("Amplitude", Float) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		half _Frequency;
		half _Amplitude;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}

		void vert(inout appdata_base v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			half displacement = sin(_Frequency * _Time + v.vertex.x) * _Amplitude;

			v.vertex.xyz += displacement;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
