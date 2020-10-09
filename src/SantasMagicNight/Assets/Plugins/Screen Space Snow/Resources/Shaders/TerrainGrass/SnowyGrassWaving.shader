
Shader "Hidden/TerrainEngine/Details/WavingDoublePass" {
    Properties{
        _WavingTint("Fade Color", Color) = (.7,.6,.5, 0)
        _MainTex("Base (RGB) Alpha (A)", 2D) = "white" {}
        _WaveAndDistance("Wave and distance", Vector) = (12, 3.6, 1, 1)
        _Cutoff("Cutoff", float) = 0.5
    }

        SubShader{
        Tags{
        "Queue" = "Geometry+200"
        "IgnoreProjector" = "True"
        "RenderType" = "Grass"
        "DisableBatching" = "True"
    }
        Cull Off
        LOD 200
        ColorMask RGB

        CGPROGRAM
#pragma surface surf Lambert vertex:WavingGrassVert addshadow exclude_path:deferred
#include "TerrainEngine.cginc"

    sampler2D _MainTex;
    fixed _Cutoff;

    uniform float _SnowTargetLevel;
    uniform float _SnowCoverageDeg;
    uniform float4 _SnowColAndCutoff;
    uniform float4 _SnowSpecAndSmoothness;
    uniform sampler2D _Coverage;

    struct Input {
        float2 uv_MainTex;
        fixed4 color : COLOR;
        float4 screenPos;
    };

    float gain(float x, float k)
    {
        float a = 0.5f * pow(2.0f * ((x < 0.5f) ? x : 1.0f - x), k);
        return (x < 0.5f) ? a : 1.0f - a;
    }

    void surf(Input IN, inout SurfaceOutput o) {
        fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;

        float3 regularGrass = c.rgb;
        float3 desaturatedGrass = dot(regularGrass, float3(0.2126f, 0.7152f, 0.0722f)) + 0.01f; // add epsilon
        float3 approxCol = (_SnowColAndCutoff.rgb + _SnowSpecAndSmoothness.rgb) * 0.5f; // very hacky, but it works
        float3 snowyGrass = saturate(lerp(0.0f, max(0.3f, desaturatedGrass * _SnowColAndCutoff.rgb), 3.0f)) * approxCol;

        float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
#if UNITY_SINGLE_PASS_STEREO
        float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
        screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
#endif

        float coverage = _SnowTargetLevel * tex2D(_Coverage, screenUV).r;

        // Is 0 when deg is 0, is 1 when deg is 180
        snowyGrass = lerp(regularGrass, snowyGrass, min(1.0f, _SnowCoverageDeg * 7.0f));

        // if the coverage is higher than 0.5, regular grass should be turning into snowy grass
        regularGrass = lerp(regularGrass, snowyGrass, saturate(saturate(_SnowCoverageDeg - 0.5f) * 4.0f) * coverage);

        o.Albedo = lerp(regularGrass, snowyGrass, (1.0f - gain(IN.uv_MainTex.y, 4.0f)) * coverage);
        o.Alpha = c.a;
        clip(o.Alpha - _Cutoff);
        o.Alpha *= IN.color.a;
    }

    ENDCG
    }

        SubShader{
        Tags{
        "Queue" = "Geometry+200"
        "IgnoreProjector" = "True"
        "RenderType" = "Grass"
    }
        Cull Off
        LOD 200
        ColorMask RGB

        Pass{
        Tags{ "LightMode" = "Vertex" }
        Material{
        Diffuse(1,1,1,1)
        Ambient(1,1,1,1)
    }
        Lighting On
        ColorMaterial AmbientAndDiffuse
        AlphaTest Greater[_Cutoff]
        SetTexture[_MainTex]{ combine texture * primary DOUBLE, texture }
    }
        Pass{
        Tags{ "LightMode" = "VertexLMRGBM" }
        AlphaTest Greater[_Cutoff]
        BindChannels{
        Bind "Vertex", vertex
        Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
        Bind "texcoord", texcoord1 // main uses 1st uv
    }
        SetTexture[unity_Lightmap]{
        matrix[unity_LightmapMatrix]
        combine texture * texture alpha DOUBLE
    }
        SetTexture[_MainTex]{ combine texture * previous QUAD, texture }
    }
    }

        Fallback Off
}