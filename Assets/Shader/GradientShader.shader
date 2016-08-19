Shader "Hidden/GradientShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_GradientMap("Gradient Map", 2D) = "white" {}
		_GradientStrength("Gradient Strength", Range(0.0, 1.0)) = 1.0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _GradientMap;

			half _GradientStrength;
			static const half sq = 1.73205080757;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 screenColor = tex2D(_MainTex, i.uv);


				// Gradient part
				half lengthColor = length(screenColor.rgb) / sq;
				fixed3 gradientColor =  tex2D(_GradientMap, half2(lengthColor, 0.0)).rgb;
				screenColor.rgb = lerp(screenColor, gradientColor, _GradientStrength);

				// Luminance based greyscale part
				float luminosity = 0.25 * screenColor.r + 0.6 * screenColor.g + 0.4 * screenColor.b;
				//float luminosity = 0.299 * screenColor.r + 0.587 * screenColor.g + 0.114 * screenColor.b;
				fixed3 screenWithGreyscale = lerp(screenColor, luminosity, _GradientStrength);

				// Combine Greyscale with gradient
				screenColor.rgb = lerp(screenColor, screenWithGreyscale, _GradientStrength);

				screenColor.a = 1.0;
				return screenColor;
			}
			ENDCG
		}
	}
}
