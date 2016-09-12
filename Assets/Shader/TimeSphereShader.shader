Shader "POLYCRUSHER/TimesphereShader" {
	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Albedo ("Albedo", 2D) = "white" {}

		_Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5

		_EmissionTex ("Emission Texture", 2D) = "white" {}
		_EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
		_EmissionValue ("Emission Value", Float) = 1

		_NormalMap ("Normal map", 2D) = "bump" {}
		_NormalIntensity ("Normal intenisty", Float) = 1

		_IntersectionColor ("Intersection Color", Color) = (1, 1, 1, 1)
		_InterscetionThreshold ("Intersection Threshold", Range(0, 5)) = 0
		_InterscetionHighlight ("Intersection Highlight", Float) = 1

		_AnimationSpeed ("Animation Speed", Float) = 0.2

		_Frequency ("Wave Frequency", Range(0, 100)) = 2
		_Amplitude ("Wave Amplitude", Range(-1, 1)) = 1
	}
	SubShader {
		Tags {
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite On
		Cull Off	// Or Cull Back? :D

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert alpha
		#pragma target 3.0

		struct Input {
			float2 uv_Albedo;
			float4 screenPos;
			float4 position;
			float3 projPos;
		};

		uniform sampler2D _CameraDepthTexture;	//Depth Texture

		sampler2D _Albedo;
		sampler2D _EmissionTex;
		sampler2D _NormalMap;

		fixed4 _Color;
		half _Metallic;
		half _Smoothness;
		half4 _EmissionColor;
		half _EmissionValue;

		half _NormalIntensity;

		half4 _IntersectionColor;
		half _InterscetionThreshold;
		half _InterscetionHighlight;

		half _AnimationSpeed;
		half _Frequency;
		half _Amplitude;


		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = _Color;
			float2 uv = IN.uv_Albedo + sin(_Time * _AnimationSpeed);
			o.Albedo = tex2D(_Albedo, uv) * c;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;

			float3 normalResult = UnpackNormal(tex2D(_NormalMap, uv));
			normalResult.z /= _NormalIntensity;
			o.Normal = normalize(normalResult.rgb);


			// Distance to the camera from the depth buffer
			float sceneZ = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos)).r);

			// Actual distance to the camera
			float partZ = IN.projPos.z;

			// If both are similar, than something is intersecting with this object
			float diff = abs(sceneZ - partZ) / _InterscetionThreshold;

			half3 lerpResult = _EmissionColor.rgb;
			half clampedDiff = clamp(diff, 0.0, 1.0);
			lerpResult = lerp(_IntersectionColor.rgb * _InterscetionHighlight, _EmissionColor.rgb, clampedDiff);

			o.Emission = lerpResult * _EmissionValue * tex2D(_EmissionTex, uv);
			o.Alpha = c.a;
		}

		// Vertex function
		void vert(inout appdata_full v, out Input o) {
			// For DX11 -> Initialize the Input struct
			UNITY_INITIALIZE_OUTPUT(Input, o);

			v.vertex.xyz += v.normal * sin(_Time * _AnimationSpeed * _Frequency) * _Amplitude;

			// Other data
			o.position = mul(UNITY_MATRIX_MVP, v.vertex);
			o.projPos = ComputeScreenPos(o.position);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
