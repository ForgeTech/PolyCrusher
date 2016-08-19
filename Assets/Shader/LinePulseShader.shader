Shader "POLYCRUSHER/LinePulseShader" {
	Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
		_Frequency ("Frequency", Float) = 1.0
		_Amplitude ("Amplitude", Range(0.0, 1.0)) = 1.0
		_Smoothing ("Smoothing", Range(0.0, 1.0)) = 0.05
		_Speed ("Speed", Float) = 0.5
		_ColorStrength ("ColorStrength", Float) = 1.0
		_LineLength ("LineLength", Float) = 1.0
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Pass {
            Cull Back
            ZWrite Off
            Blend srcAlpha OneMinusSrcAlpha

            CGPROGRAM
			#pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            half4 _Color;
			half _Frequency;
			half _Smoothing;
			half _Amplitude;
			half _Speed;
			half _ColorStrength;
			fixed _LineLength;

			/* Input struct for the vertex SubShader
			 * Unity fills the variables according to the binding semantic (e.g. 'POSITION')
			 */
            struct vertInput {
                half4 pos : POSITION;
                half2 texcoord : TEXCOORD0;
            };

            /*The vertex shader has to fill this struct which is then passed to the fragment shader.*/
            struct vertOutput {
                half4 pos : SV_POSITION;
				half2 texcoord : TEXCOORD0;
            };

            vertOutput vert (vertInput input)
            {
                vertOutput o;
                o.pos = mul (UNITY_MATRIX_MVP, input.pos);
				o.texcoord = input.texcoord;

                return o;
            }

            fixed4 frag (vertOutput input) : COLOR
            {
				// TODO: Fix -> Line moves faster when line length gets bigger
				fixed x = input.texcoord.x + _Time[1] * _Speed;
				half sine = _Amplitude * sin(x * _Frequency * _LineLength) * 0.5 + 0.5;

				half absolute = abs(input.texcoord.y - sine);
				half alpha = 1.0 - smoothstep(0.0, _Smoothing, absolute);
				half3 resultColor = _Color.rgb * _ColorStrength;
                return fixed4(resultColor.r, resultColor.g, resultColor.b, alpha);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
