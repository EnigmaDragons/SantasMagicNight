// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Hidden/TerrainEngine/Details/BillboardWavingDoublePass" {
    Properties {
        _WavingTint ("Fade Color", Color) = (.7,.6,.5, 0)
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
        _WaveAndDistance ("Wave and distance", Vector) = (12, 3.6, 1, 1)
        _Cutoff ("Cutoff", float) = 0.5
    }

CGINCLUDE
#include "UnityCG.cginc"
#include "TerrainEngine.cginc"

struct v2f {
    float4 pos : SV_POSITION;
    fixed4 color : COLOR;
    float4 uv : TEXCOORD0;
    UNITY_VERTEX_OUTPUT_STEREO
};
v2f BillboardVert (appdata_full v) {
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    WavingGrassBillboardVert (v);
    o.color = v.color;

    o.color.rgb *= ShadeVertexLights (v.vertex, v.normal);

    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.texcoord;
    return o;
}
ENDCG

    SubShader {
        Tags {
            "Queue" = "Geometry+200"
            "IgnoreProjector"="True"
            "RenderType"="GrassBillboard"
            "DisableBatching"="True"
        }
        Cull Off
        LOD 200
        ColorMask RGB

CGPROGRAM
#pragma surface surf Lambert vertex:WavingGrassBillboardVert addshadow exclude_path:deferred

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
    float3 desaturatedGrass = dot(regularGrass, float3(0.2126f, 0.7152f, 0.0722f)); // add epsilon
    float3 approxCol = (_SnowColAndCutoff.rgb + _SnowSpecAndSmoothness.rgb) * 0.5f; // very hacky, but it works
    float3 snowyGrass = saturate(lerp(0.0f, max(0.3f, desaturatedGrass), 3.0f)) * _SnowColAndCutoff.rgb;

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
    clip (o.Alpha - _Cutoff);
    o.Alpha *= IN.color.a;
}

ENDCG
    }

    Fallback Off
}
