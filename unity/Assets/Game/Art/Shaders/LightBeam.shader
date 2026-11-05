// A visible shaft of light in the studio's haze (build_studio.py, beam()): an open cone from a lamp to where its light
// lands, drawn additively. It is brightest where the cone is seen face on and fades out at its silhouette, near the lamp
// and towards the far end, so it reads as lit air rather than a solid cone. A slow drifting noise breaks it up like
// moving haze. No fog and no depth writes; it never lights anything (the real light does that).
Shader "FamilyQuiz/LightBeam"
{
    Properties
    {
        [HDR] _Color ("Colour", Color) = (1, 0.7, 0.45, 1)
        _Intensity ("Intensity", Float) = 0.08
        _EdgePower ("Edge softness", Float) = 2.2
        _LengthPower ("Fall off along the beam", Float) = 1.3
        _NoiseScale ("Haze noise scale", Float) = 1.6
        _NoiseAmount ("Haze noise amount", Range(0, 1)) = 0.45
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent+10" "IgnoreProjector" = "True" }

        Pass
        {
            Name "LightBeam"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Intensity;
                half _EdgePower;
                half _LengthPower;
                half _NoiseScale;
                half _NoiseAmount;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            // Cheap value noise for the drifting haze.
            float Hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float Noise(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(lerp(Hash(i), Hash(i + float3(1, 0, 0)), f.x), lerp(Hash(i + float3(0, 1, 0)), Hash(i + float3(1, 1, 0)), f.x), f.y),
                            lerp(lerp(Hash(i + float3(0, 0, 1)), Hash(i + float3(1, 0, 1)), f.x), lerp(Hash(i + float3(0, 1, 1)), Hash(i + float3(1, 1, 1)), f.x), f.y), f.z);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 view = normalize(GetWorldSpaceViewDir(input.positionWS));
                float facing = abs(dot(normalize(input.normalWS), view));
                float edge = pow(saturate(facing), _EdgePower);
                float along = input.uv.y;                                  // 0 at the lamp, 1 where the light lands
                float fall = pow(saturate(1.0 - along), _LengthPower) * saturate(along * 10.0);
                float3 drift = input.positionWS * _NoiseScale + float3(0.0, -_Time.y * 0.05, _Time.y * 0.03);
                float haze = lerp(1.0, Noise(drift) * 0.7 + Noise(drift * 2.7) * 0.6, _NoiseAmount);
                // Both faces of the open cone are drawn: halve each.
                return half4(_Color.rgb * (_Intensity * 0.5 * edge * fall * haze), 0);
            }
            ENDHLSL
        }
    }
}
