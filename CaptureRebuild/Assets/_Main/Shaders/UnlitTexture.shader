Shader "Custom/UnlitTexture"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _MianTexColor ("Base Color", Color) = (1, 1, 1, 1)
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _OverlayTexColor ("Overlay Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline" "LightMode"="Unblurred"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            struct appdata 
            { 
                float4 vertex : POSITION; 
                float2 uv : TEXCOORD0; 
            };

            struct v2f 
            { 
                float4 pos : SV_POSITION; 
                float2 uv : TEXCOORD0; 
            };

            Texture2D _MainTex;
            Texture2D _OverlayTex;
            float4 _MianTexColor;
            float4 _OverlayTexColor;
            SamplerState sampler_LinearClamp;

            v2f Vert(appdata v) 
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 Frag(v2f i) : SV_Target 
            {
                float4 baseColor = _MainTex.Sample(sampler_LinearClamp, i.uv) * _MianTexColor;
                float4 overlayColor = _OverlayTex.Sample(sampler_LinearClamp, i.uv) * _OverlayTexColor;
                return lerp(baseColor, overlayColor, overlayColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
