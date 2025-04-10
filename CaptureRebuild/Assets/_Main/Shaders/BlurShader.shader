Shader "Custom/BlurShader"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float2 _BlurDirection;
        float _BlurSize;

        float4 BlurFrag(Varyings input) : SV_Target
        {
            float2 uv = input.texcoord;
            float4 color = 0;

            float offsetScale = _BlurSize / _ScreenParams.xy; 
            float2 offset = _BlurDirection * offsetScale;

            color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset * -4.0) * 0.016216;
            color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset * -3.0) * 0.054054;
            color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset * -2.0) * 0.121621;
            color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset * -1.0) * 0.194595;
            color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv) * 0.227027;
            color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset *  1.0) * 0.194595;
            color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset *  2.0) * 0.121621;
            color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset *  3.0) * 0.054054;
            color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset *  4.0) * 0.016216;

            return color;
        }
    ENDHLSL

        
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always ZWrite Off Cull Off
        Pass
        {
            Name "Blur"

            Stencil
            {
                Ref 1
                Comp NotEqual
                Pass Keep
            }

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment BlurFrag
            
            ENDHLSL
        }
    }
}