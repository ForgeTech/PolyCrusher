Shader "Hidden/Gradient"
{
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Blend ("Blend", Range (0, 1)) = 0
		_GreenBlend ("GreenBlend", Range (0, 1)) = 0
	}

	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform half _Blend;
			uniform half _GreenBlend;

			fixed4 frag(v2f_img i) : COLOR {
				fixed4 c = tex2D(_MainTex, i.uv);
    			half luma = 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
				half3 colorYellow = float3(1.0, 1.0, 0.0);
				half3 colorGreen = float3(0.4745098039, 0.6392156863, 0.3882352941);

    			// Blend yellow to green
				colorYellow.rgb += _GreenBlend * (colorGreen - colorYellow);

    			// Gradient blend
				half3 gradientBlend = colorGreen + luma * (colorYellow - colorGreen);

    			// Black and white blend
				half3 blackWhiteBlend = c + _Blend * (luma - c);

    			// Black and white to c
    			c.rgb = float3 (blackWhiteBlend.r, blackWhiteBlend.g, blackWhiteBlend.b);

    			// Save gradient image to result
				fixed4 result = float4(gradientBlend.r, gradientBlend.g, gradientBlend.b, 1.0);

				// Lerp normal image and gradient image
				result.rgb = lerp(c.rgb, result, _Blend);

				// Return result
				return result;
			}
			ENDCG
		}
	}
}
