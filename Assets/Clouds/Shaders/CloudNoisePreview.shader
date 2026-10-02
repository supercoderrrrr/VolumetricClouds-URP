Shader "Hidden/VolumetricClouds/CloudNoisePreview"
{
    Properties
    {
        _NoiseTex ("Noise Texture", 3D) = "" {}
        _Slice ("Slice", Range(0, 1)) = 0
        _Channel ("Channel", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }

        Pass
        {
            ZTest Always

            ZWrite Off

            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler3D _NoiseTex;
            float _Slice;
            float _Channel;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.positionCS = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }

            float4 frag(v2f input) : SV_Target
            {
                float4 noise = tex3D(_NoiseTex, float3(input.uv, _Slice));

                float value = noise.r;

                if (_Channel == 1)
                    value = noise.g;
                else if (_Channel == 2)
                    value = noise.b;
                else if (_Channel == 3)
                    value = noise.a;

                return float4(value, value, value, 1);
            }
            ENDHLSL
        }
    }
}