Shader "POLYCRUSHER/CharacterShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_EmissionColor ("EmissionColor", Color) = (1,1,1,1)
		_EmissionMult ("Emission Multiplicator", Float) = 1
		_Emission ("Emission", 2D) = "white" {}
		_Smoothness ("Smoothness", Range(0,1)) = 0.5
		_Specular ("Specular", 2D) = "black" {}
		_Occlusion ("Occlusion", 2D) = "white" {}
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_FillColor ("FillColor", Color) = (0,0,0,1)
		_FillEmission ("FillEmission", Color) = (1,1,1,1)
	}
	SubShader {
		ZWrite Off
        ZTest Off
        Lighting Off
		Cull Back

		CGPROGRAM
         #pragma surface surf Standard fullforwardshadows //alpha
 
         sampler2D _MainTex;
		 fixed4 _FillColor;
		 fixed4 _FillEmission;

         struct Input {
             float2 uv_MainTex;
         };
 
         void surf (Input IN, inout SurfaceOutputStandard o) {
             //half4 c = tex2D (_MainTex, IN.uv_MainTex);
             o.Albedo = _FillColor.rgb;
             o.Alpha = _FillColor.a;
			 o.Emission = _FillEmission;
         }
         ENDCG

		Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite On
        ZTest LEqual
		Lighting On
		Cull Back
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf StandardSpecular fullforwardshadows //alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Emission;
		sampler2D _Specular;
		sampler2D _Occlusion;

		struct Input {
			float2 uv_MainTex;
		};

		half _Smoothness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _EmissionColor;
		float _EmissionMult;

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Emission = tex2D (_Emission, IN.uv_MainTex) * _EmissionColor * _EmissionMult;
			o.Specular = tex2D (_Specular, IN.uv_MainTex).rgb;
			o.Albedo = c.rgb;
			o.Occlusion = tex2D (_Occlusion, IN.uv_MainTex);
			o.Smoothness = _Smoothness;
			o.Alpha = _Color.a;
		}

		ENDCG
    }

	FallBack "Diffuse"
}