Shader "Hidden/ScreenSpaceSnow"
{
    Properties{
        _MainTex("Base (RGB)", 2D) = "white" { }
    }
        SubShader
    {

        CGINCLUDE
#pragma multi_compile ___ TILING_FIX
#pragma multi_compile ___ TRIPLANAR_PROJ
#pragma multi_compile ___ TEXTURED

#include "UnityCG.cginc"
#include "UnityPBSLighting.cginc"
#include "UnityDeferredLibrary.cginc"
#include "SnowShaderHelpers.cginc"

        // defined in SnowShaderHelpers.cginc:
        // sampler2D _AlbedoTex;
        // sampler2D _RoughnessTex;
        // sampler2D _NormalsTex;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Vertex Shader

        /* Blit vertices go from -1,-1,1 to 1,1,1 */
        v2f vert(appdata_base v)
    {
        v2f o;
        o.vertex = v.vertex * float4(2, 2, 1, 1) + float4(-1, -1, 0, 0);
        o.uvNoFix = v.texcoord;
        o.uvNoFix.y = 1.0f - o.uvNoFix.y; //blit flips the uv for some reason
        o.uv = o.uvNoFix;
        o.uv = UnityStereoTransformScreenSpaceTex(o.uv);
//#if UNITY_SINGLE_PASS_STEREO
//        float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
//        o.uv = (o.uv - scaleOffset.zw) / scaleOffset.xy;
//#endif
        return o;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Fragment Shaders

    // void Setup(v2f i, out float3 normalsWorld, out float3 wPos, out float2 snowUV, out float snowIntensity)
    //void SetupTriplanar(v2f i, out float3 normalsWorld, out float3 wPos, out float3 snowUVX, 
    //                    out float3 snowUVY, out float3 snowUVZ, out float snowIntensity)

    /* Get the Albedo color with a snowy surface */
    half4 frag_albedo(v2f i)
    {
        float snowIntensity;
#if TEXTURED
        half4 snowAlbedoCol = SampleTex(_AlbedoTex, i, snowIntensity);
        snowAlbedoCol.rgb *= _SnowColAndCutoff.rgb;
#else
        float3 normalsWorld = GetNormalsWorld(i.uv);
        snowIntensity = GetNormalsIntensity(normalsWorld);
        snowIntensity *= tex2D(_Coverage, i.uv).r;
        half4 snowAlbedoCol = half4(_SnowColAndCutoff.rgb, 1);
#endif

        // Get the current channel to modify
        half4 gbuffer0 = tex2D(_TempAlbedo, i.uv);
        gbuffer0 = lerp(gbuffer0, snowAlbedoCol, snowIntensity);
        //gbuffer0 = float4(i.uv.xy, 1.0f, 1.0f);
        return gbuffer0;
    }

    /* Get the Specular color with a snowy surface */
    half4 frag_specular(v2f i)
    {
        float snowIntensity;
#if TEXTURED
        float4 snowSpecularColor = SampleTex(_SpecularTex, i, snowIntensity);
        snowSpecularColor.rgb *= _SnowSpecAndSmoothness.rgb;
#else
        float3 normalsWorld = GetNormalsWorld(i.uv);
        snowIntensity = GetNormalsIntensity(normalsWorld);
        snowIntensity *= tex2D(_Coverage, i.uv).r;
        half4 snowSpecularColor = half4(_SnowSpecAndSmoothness.rgb, 1);
#endif
        snowSpecularColor.a = snowSpecularColor.a * _SnowSpecAndSmoothness.a;

        // Get the current channel to modify
        half4 gbuffer1 = tex2D(_TempSpecular, i.uv);
        gbuffer1 = lerp(gbuffer1, snowSpecularColor, snowIntensity);
        return gbuffer1;
    }

    /* Get the Normals with a snowy surface */
    half4 frag_normals(v2f i)
    {
        float snowIntensity;
#if TEXTURED
        float4 snowNormalMap = SampleTex(_NormalsTex, i, snowIntensity);
        float3 newNormalsWorld = normalize(UnpackNormal(snowNormalMap) * float3(_NormalStrength, _NormalStrength, 1.0f));
#else
        float3 normalsWorld = GetNormalsWorld(i.uv);
        snowIntensity = GetNormalsIntensity(normalsWorld);
        snowIntensity *= tex2D(_Coverage, i.uv).r;
        float3 newNormalsWorld = float3(0, 0, 1);
#endif

        // Get the current channel to modify
        half4 gbuffer2 = tex2D(_TempNormal, i.uv);

        float3 N = normalize(gbuffer2 * 2 - 1), X, Y; // tangents X and Y
        GetLocalFrame(N, X, Y);

        newNormalsWorld = N * newNormalsWorld.z + X * newNormalsWorld.x + Y * newNormalsWorld.y;

        gbuffer2 = lerp(gbuffer2, float4(normalize(newNormalsWorld) * 0.5 + 0.5, 1), snowIntensity);
        return gbuffer2;
    }

    half4 frag_lighting(v2f i)
    {
        // Normals intensity
        float snowIntensity;
        float3 normalsWorld = GetNormalsWorld(i.uv);
        snowIntensity = GetNormalsIntensity(normalsWorld);
        snowIntensity *= tex2D(_Coverage, i.uv).r;

        // unpack Gbuffer
        half4 gbuffer0 = tex2D(_CameraGBufferTexture0, i.uv);
        half4 gbuffer1 = tex2D(_CameraGBufferTexture1, i.uv);
        half4 gbuffer2 = tex2D(_CameraGBufferTexture2, i.uv);

        // HDR is logarithmically encoded
        half4 gbuffer3 = -log2(tex2D(_TempEmission, i.uv));

        float3 normalWorld = GetNormalsWorld(i.uv); float oneMinusReflectivity;
        gbuffer0.rgb = EnergyConservationBetweenDiffuseAndSpecular(gbuffer0.rgb, gbuffer1.rgb, /*out*/ oneMinusReflectivity);

        UnityGI gi;
        {
            gi.light.color = 0;
            gi.light.dir = float3(0, 1, 0);
            gi.indirect.diffuse = 0.1f;
            gi.indirect.specular = 0.1f;
        }

        float3 eyeVec = UNITY_MATRIX_IT_MV[2].xyz;	// TODO: THIS ISN'T EYE VEC. EYE VEC IS _WorldSpaceCameraPos - POSITION

        half4 emissiveColor = UNITY_BRDF_PBS(gbuffer0.rgb, gbuffer1.rgb, oneMinusReflectivity, gbuffer1.a, normalWorld, -eyeVec, gi.light, gi.indirect);

        gbuffer3 = lerp(gbuffer3, emissiveColor, snowIntensity);
        gbuffer0 = -log2(float4(i.uv.xy, 1.0f, 1.0f));
        return gbuffer3;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Passes

    ENDCG
        Pass
    {  // Albedo Pass - 0
        Cull Off ZWrite Off ZTest Never Blend SrcAlpha OneMinusSrcAlpha
        Fog{ Mode Off } // no fog in g-buffers pass

        CGPROGRAM
#pragma target 3.0 
#pragma vertex vert 
#pragma fragment frag 
#pragma exclude_renderers nomrt

        half4 frag(v2f i) : SV_Target
    {
        return frag_albedo(i);
    }
        ENDCG
    }

        Pass
    {  // Specular Pass - 1
        Cull Off ZWrite Off ZTest Never
        Fog{ Mode Off } // no fog in g-buffers pass

        CGPROGRAM
#pragma target 3.0 
#pragma vertex vert 
#pragma fragment frag 
#pragma exclude_renderers nomrt

        half4 frag(v2f i) : SV_Target
    {
        return frag_specular(i);
    }
        ENDCG
    }

        Pass
    {  // Normals Pass - 2
        Cull Off ZWrite Off ZTest Never
        Fog{ Mode Off } // no fog in g-buffers pass

        CGPROGRAM
#pragma target 3.0 
#pragma vertex vert 
#pragma fragment frag 
#pragma exclude_renderers nomrt

        half4 frag(v2f i) : SV_Target
    {
        return frag_normals(i);
    }
        ENDCG
    }

        Pass
    {  // Lighting Pass - 3
        Cull Off ZWrite Off ZTest Never
        Fog{ Mode Off } // no fog in g-buffers pass

        CGPROGRAM
#pragma target 3.0 
#pragma vertex vert 
#pragma fragment frag 
#pragma exclude_renderers nomrt

        half4 frag(v2f i) : SV_Target
    {
        return exp2(-frag_lighting(i));
    }
        ENDCG
    }

        Pass
    {  // Lighting Pass HDR - 4
        Cull Off ZWrite Off ZTest Never
        Fog{ Mode Off } // no fog in g-buffers pass

        CGPROGRAM
#pragma target 3.0 
#pragma vertex notrs_vert 
#pragma fragment frag 
#pragma exclude_renderers nomrt

        v2f notrs_vert(appdata_base v)
    {
        v2f o;
        o.vertex = v.vertex;
        o.uvNoFix = v.texcoord;
        o.uv = v.texcoord;
        o.uv.y = 1.0f - o.uv.y; //blit flips the uv for some reason
        return o;
    }

    half4 frag(v2f i) : SV_Target
    {
        return frag_lighting(i);
    }
        ENDCG
    }

        Pass
    {  // Simple Copy Pass - 5
        Cull Off ZWrite Off ZTest Never
        Fog{ Mode Off }

        CGPROGRAM
#pragma target 3.0 
#pragma vertex vert 
#pragma fragment frag 
#pragma exclude_renderers nomrt

        sampler2D _MainTex;

    float4 frag(v2f i) : SV_Target
    {
        return tex2D(_MainTex, i.uv);
    }
        ENDCG
    }

    }}