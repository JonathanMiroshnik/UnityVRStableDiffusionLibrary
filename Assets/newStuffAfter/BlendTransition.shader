Shader "Unlit/BlendTransition"
{
    Properties
    {
        _CurrentTex ("Current Texture", 2D) = "white" {}
        _NextTex ("Next Texture", 2D) = "white" {}
        _Transition ("Transition", Range(0,1)) = 0.5 // Factor to control blending (optional)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha  // Standard alpha blending

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _CurrentTex;
            sampler2D _NextTex;
            float _Transition;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Sample each texture
                float4 tex1Color = tex2D(_CurrentTex, i.uv);
                float4 tex2Color = tex2D(_NextTex, i.uv);

                // Blend the textures using their alpha values
                float4 blendedColor = tex1Color;
                
                // Blend with the second texture
                blendedColor = lerp(blendedColor, tex2Color, tex2Color.a * _Transition);

                return blendedColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
