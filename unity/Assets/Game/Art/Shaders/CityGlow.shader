// The night city behind the window (build_studio.py, build_city): unlit emissive windows on a near black facade, with
// no fog, so the room's haze (StudioBuilder, RenderSettings.fog) never greys out a city hundreds of metres away. No depth
// pass either: the city stays at the far plane of the depth texture, so the depth of field blurs it fully, like a
// translight seen through a lens focused on the guests.
Shader "FamilyQuiz/CityGlow"
{
    Properties
    {
        _BaseColor ("Facade", Color) = (0, 0, 0, 1)
        _BaseMap ("Tiling (uses this map's scale and offset)", 2D) = "white" {}
        _EmissionMap ("Windows", 2D) = "black" {}
        [HDR] _EmissionColor ("Window light", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "CityGlow"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _EmissionColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 windows = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb * _EmissionColor.rgb;
                return half4(_BaseColor.rgb + windows, 1);
            }
            ENDHLSL
        }
    }
}
