Shader "Unlit/TextureTransitionShader"
{
    Properties
    {
        _CurrentTex ("Current Texture", 2D) = "white" {}
        _NextTex ("Next Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Transition ("Transition", Range(0, 1)) = 0
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 1.0
        _Smoothness ("Smoothness", Range(0.01, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _CurrentTex;
            sampler2D _NextTex;
            sampler2D _NoiseTex;
            float _Transition;
            float _NoiseIntensity;
            float _Smoothness;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col1 = tex2D(_CurrentTex, i.uv);
                fixed4 col2 = tex2D(_NextTex, i.uv);
                float noise = tex2D(_NoiseTex, i.uv).r;

                // Adjust noise influence with intensity and smoothness
                float adjustedNoise = (noise - 0.5) * _NoiseIntensity + 0.5;
                float maskedTransition = smoothstep(0.0, 1.0, (_Transition * 2) + (adjustedNoise - 0.5) * _Smoothness);

                return lerp(col1, col2, maskedTransition);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
