Shader "Hidden/ScreenSpaceSnowOccluders"
{
SubShader
{
CGINCLUDE
#pragma target 3.0
#pragma fragment frag
#pragma exclude_renderers nomrt

#include "UnityCG.cginc"
#include "UnityPBSLighting.cginc"
#include "UnityDeferredLibrary.cginc"

uniform float _SnowAmount;
uniform float4 _FeatherScale;
sampler2D _DepthCopy;

float3 GetWorldSpaceCoordFromDepth(unity_v2f_deferred i)
{
    i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
    float2 uv = i.uv.xy / i.uv.w;

    // read depth and reconstruct world position
    float depth = tex2D(_DepthCopy, uv).r;
    float4 vpos = float4(i.ray * depth, 1);
    float3 wpos = mul(unity_CameraToWorld, vpos).xyz;

    return wpos;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Vertex Shader

unity_v2f_deferred vert(float4 vertex : POSITION)
{
    unity_v2f_deferred o;
    o.pos = UnityObjectToClipPos(vertex);
    o.uv = ComputeScreenPos(o.pos);
    o.ray = UnityObjectToViewPos(vertex) * float3(-1,-1,1);
    return o;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Passes

ENDCG

Pass
{   // Mesh Pass - 0
    Fog{ Mode Off } // no fog in g-buffers pass
    Blend Off

    CGPROGRAM
    #pragma vertex vert

    float4 frag(unity_v2f_deferred i) : SV_Target
    {
        return _SnowAmount.xxxx;
    }
    ENDCG
}

Pass
{   // Box Pass - 1
    Cull Front ZWrite Off ZTest Always
    Fog{ Mode Off } // no fog in g-buffers pass
    Blend SrcAlpha OneMinusSrcAlpha

    CGPROGRAM
    #pragma vertex vert

    float4 frag(unity_v2f_deferred i) : SV_Target
    {
        float3 wPos = GetWorldSpaceCoordFromDepth(i);
        float3 opos = mul(unity_WorldToObject, float4(wPos, 1)).xyz;


        float3 size = float3(0.5f, 0.5f, 0.5f);
        float3 sizeInner = size * _FeatherScale.xyz;

        float3 d = abs(opos) - size;
        float dist = min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
        dist = max(-dist, 0);

        float3 d2 = abs(opos) - sizeInner;
        float dist2 = min(max(d2.x, max(d2.y, d2.z)), 0.0) + length(max(d2, 0.0));
        float length = dist + dist2;

        float occlusion = 1;
        if (dist > 0.0f)
        {
            occlusion = 0;
            if (length != 0)
            {
                occlusion = smoothstep(0, 1, dist2 / length);
            }
        }
        return float4(_SnowAmount.xxx, 1 - occlusion);
    }
    ENDCG
}

Pass
{   // Sphere Pass - 2
    Cull Front ZWrite Off ZTest Always
    Fog{ Mode Off } // no fog in g-buffers pass
    Blend SrcAlpha OneMinusSrcAlpha

    CGPROGRAM
    #pragma vertex vert

    float4 frag(unity_v2f_deferred i) : SV_Target
    {
        float3 wPos = GetWorldSpaceCoordFromDepth(i);
        float3 opos = mul(unity_WorldToObject, float4(wPos, 1)).xyz;
        
	    float3 size = 1.0f;
	    float3 sizeInner = size * _FeatherScale.x;

	    float dist = length(opos) - size;
	    float dist2 = length(opos) - sizeInner;
	    dist = max(-dist, 0);

	    float length = dist + dist2;

        float occlusion = 1;
        if (dist > 0.0f)
        {
            occlusion = 0;
            if (length != 0)
            {
                occlusion = smoothstep(0, 1, dist2 / length);
            }
        }
        return float4(_SnowAmount.xxx, 1 - occlusion);
    }
    ENDCG
}

Pass
{   // Blit Depth Pass - 3
    Cull Off ZWrite Off ZTest Never
	Fog { Mode Off }
    
    CGPROGRAM
    #pragma vertex vert2

    struct appdata
    {
        float4 vertex : POSITION;
        half2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    v2f vert2(appdata v)
    {
        v2f o;
        o.vertex = v.vertex;
        o.uv = v.uv;
        return o;
    }

    fixed4 frag(v2f i) : SV_Target
	{
        return half4(Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv)).xxxx);
	}
	ENDCG
}

}}
