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
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            Stencil
            {
                Ref 1             
                Comp always      
                Pass replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            sampler2D _MainTex;
            sampler2D _OverlayTex;
            float4 _MianTexColor;
            float4 _OverlayTexColor;

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 baseColor = tex2D(_MainTex, i.uv) * _MianTexColor;
                fixed4 overlayColor = tex2D(_OverlayTex, i.uv) * _OverlayTexColor;
                baseColor = lerp(baseColor,overlayColor,overlayColor.a);
                return baseColor;
            }
            ENDCG
        }
    }
}
