Shader "POLYCRUSHER/PolygonShader" {
	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
		_EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
		_EmissionValue ("Emission Value", Float) = 1

		_IntersectionColor ("Intersection Color", Color) = (1, 1, 1, 1)
		_InterscetionThreshold ("Intersection Threshold", Range(0, 5)) = 0
		_InterscetionHighlight ("Intersection Highlight", Float) = 1
	}
	SubShader {
		Tags {
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Off

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert alpha:fade
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
			float4 position;
			float3 projPos;
		};

		uniform sampler2D _CameraDepthTexture;	//Depth Texture
		fixed4 _Color;
		half4 _EmissionColor;
		half _EmissionValue;

		half4 _IntersectionColor;
		half _InterscetionThreshold;
		half _InterscetionHighlight;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = _Color;
			o.Albedo = c.rgb;

			// Distance to the camera from the depth buffer
			float sceneZ = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos)).r);

			// Actual distance to the camera
			float partZ = IN.projPos.z;

			// If both are similar, than something is intersecting with this object
			float diff = abs(sceneZ - partZ) / _InterscetionThreshold;

			half3 lerpResult = _EmissionColor.rgb;
			half clampedDiff = clamp(diff, 0.0, 1.0);
			lerpResult = lerp(_IntersectionColor.rgb * _InterscetionHighlight, _EmissionColor.rgb, clampedDiff);

			o.Emission = lerpResult * _EmissionValue;
			o.Alpha = c.a;
		}

		// Vertex function
		void vert(inout appdata_base v, out Input o) {
			// For DX11 -> Initialize the Input struct
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.position = mul(UNITY_MATRIX_MVP, v.vertex);
			o.projPos = ComputeScreenPos(o.position);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
