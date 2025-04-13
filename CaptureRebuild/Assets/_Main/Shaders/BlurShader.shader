Shader "Custom/BlurShader"
{   
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        Cull Off

        Pass
        {
            Name "Blur"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment BlurFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float2 _BlurDirection;
            float _BlurSize;
            uniform float _Weights[13];

            float4 BlurFrag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float4 color = 0;

                float2 offsetScale = _BlurSize / _ScreenParams.xy; 
                float2 offset = _BlurDirection * offsetScale;

                [unroll]
                for(int i = -6; i <= 6; i++)
                {
                    int weightIndex = i + 6;
                    color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset * i) * _Weights[weightIndex];
                }

                return color;
            }            
            ENDHLSL
        }
    }
}