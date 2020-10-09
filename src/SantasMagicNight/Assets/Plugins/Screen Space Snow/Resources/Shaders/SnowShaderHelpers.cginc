// Samplers
uniform sampler2D _AlbedoTex;       // <-PBR
uniform sampler2D _SpecularTex;     // <-PBR
uniform sampler2D _NormalsTex;      // <-PBR
uniform sampler2D _Coverage;

// sampler2D _CameraDepthTexture; // redefined
sampler2D _TempAlbedo;
sampler2D _TempSpecular;
sampler2D _TempNormal;
sampler2D _TempEmission;
sampler2D _CameraGBufferTexture0;
sampler2D _CameraGBufferTexture1;
sampler2D _CameraGBufferTexture2;
sampler2D _CameraGBufferTexture3;

// For VR (single and multi pass)
uniform float4x4 _SnowLeftWorldFromView;
uniform float4x4 _SnowRightWorldFromView;
uniform float4x4 _SnowLeftViewFromScreen;
uniform float4x4 _SnowRightViewFromScreen;

// Parameters
uniform float4 _SnowFallParameters;
uniform float  _SnowGainPower;

uniform float4 _SnowColAndCutoff;
uniform float4 _SnowSpecAndSmoothness;
uniform float  _SnowTextureScale;
uniform float  _NormalStrength;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Simple VS Output

struct v2f
{
    float2 uv : TEXCOORD0;
    float2 uvNoFix : TEXCOORD1;
    float4 vertex : SV_POSITION;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Utility Functions

// Hashing functions

float4 hash4(float2 p) {
    return frac(sin(float4(1.0 + dot(p, float2(37.0, 17.0)),
        2.0 + dot(p, float2(11.0, 47.0)),
        3.0 + dot(p, float2(41.0, 29.0)),
        4.0 + dot(p, float2(23.0, 31.0))))*103.0);
}

// Prevent texture repetition (expensiveish - 4 texture lookups)
// http://www.iquilezles.org/www/articles/texturerepetition/texturerepetition.htm
float4 TextureNoTile(sampler2D samp, in float2 uv)
{
    int2 iuv = int2(floor(uv));
    float2 fuv = frac(uv);

    // generate per-tile transform
    float4 ofa = hash4(iuv + int2(0, 0));
    float4 ofb = hash4(iuv + int2(1, 0));
    float4 ofc = hash4(iuv + int2(0, 1));
    float4 ofd = hash4(iuv + int2(1, 1));

    float2 _ddx = ddx(uv);
    float2 _ddy = ddy(uv);

    // transform per-tile uvs
    ofa.zw = sign(ofa.zw - 0.5);
    ofb.zw = sign(ofb.zw - 0.5);
    ofc.zw = sign(ofc.zw - 0.5);
    ofd.zw = sign(ofd.zw - 0.5);

    // uv's, and derivatives (for correct mipmapping)
    float2 uva = uv * ofa.zw + ofa.xy, ddxa = _ddx * ofa.zw, ddya = _ddy * ofa.zw;
    float2 uvb = uv * ofb.zw + ofb.xy, ddxb = _ddx * ofb.zw, ddyb = _ddy * ofb.zw;
    float2 uvc = uv * ofc.zw + ofc.xy, ddxc = _ddx * ofc.zw, ddyc = _ddy * ofc.zw;
    float2 uvd = uv * ofd.zw + ofd.xy, ddxd = _ddx * ofd.zw, ddyd = _ddy * ofd.zw;

    // fetch and blend
    float2 b = smoothstep(0.25, 0.75, fuv);

    return lerp(lerp(tex2Dgrad(samp, uva, ddxa, ddya),
        tex2Dgrad(samp, uvb, ddxb, ddyb), b.x),
        lerp(tex2Dgrad(samp, uvc, ddxc, ddyc),
            tex2Dgrad(samp, uvd, ddxd, ddyd), b.x), b.y);
}

// Generate an orthonormalBasis from 3d unit vector
void GetLocalFrame(float3 N, out float3 tangentX, out float3 tangentY)
{
    float3 upVector = abs(N.z) < 0.999f ? float3(0.0f, 0.0f, 1.0f) : float3(1.0f, 0.0f, 0.0f);
    tangentX = normalize(cross(upVector, N));
    tangentY = cross(N, tangentX);
}

float3 GetWorldSpaceCoordFromDepth(v2f i)
{
    // Read none linear depth texture, accounting for 
    float d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv); // non-linear Z

    // Pick one of the passed in projection/view matrices based on stereo eye selection (always left if not vr)
    float4x4 proj, eyeToWorld;
    if (unity_StereoEyeIndex == 0)
    {
        proj = _SnowLeftViewFromScreen;
        eyeToWorld = _SnowLeftWorldFromView;
    }
    else
    {
        proj = _SnowRightViewFromScreen;
        eyeToWorld = _SnowRightWorldFromView;
    }

    // Bit of matrix math to take the screen space coord (u,v,depth) and transform to world space
    float2 uvClip = i.uvNoFix * 2.0 - 1.0;
    float4 clipPos = float4(uvClip, d, 1.0);
    float4 viewPos = mul(proj, clipPos); // inverse projection by clip position
    viewPos /= viewPos.w; // perspective division
    return mul(eyeToWorld, viewPos).xyz;
}

float GetNormalsIntensity(float3 normalsWorld)
{
    float NoS = max(0.0f, dot(normalsWorld, _SnowFallParameters.xyz));
    return smoothstep(_SnowFallParameters.w, lerp(_SnowFallParameters.w + 0.001f, 1.0f, _SnowGainPower), NoS);
}

// Get the planar UVs
float2 PlanarProjectionUVs(float3 wPos, float3 N)
{
    float3 X, Y; // tangents
    GetLocalFrame(N, X, Y);

    float4x4 trs = float4x4 (
        float4(X, 0.0f),
        float4(N, 0.0f),
        float4(Y, 0.0f),
        float4(0.0f, 0.0f, 0.0f, 1.0f));

    return mul(trs, float4(wPos, 0.0f)).xz;
}

// Return world space normals from the planar UVs
float3 GetNormalsWorld(float2 uv)
{
    half4 gbuffer2 = tex2D(_TempNormal, uv);
    half3 normalsWorld = gbuffer2.rgb * 2 - 1;
    return normalize(normalsWorld);
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// SETUP FUNCTIONS

void Setup(v2f i, out float3 normalsWorld, out float3 wPos, out float2 snowUV, out float snowIntensity)
{
    /* Get Normals*/
    normalsWorld = GetNormalsWorld(i.uv);
    wPos = GetWorldSpaceCoordFromDepth(i);
    snowUV = PlanarProjectionUVs(wPos, _SnowFallParameters.xyz);
    snowIntensity = GetNormalsIntensity(normalsWorld);
    snowIntensity *= tex2D(_Coverage, i.uv).r;
}

void SetupTriplanar(v2f i, out float3 normalsWorld, out float3 wPos, out float3 snowUVX,
    out float3 snowUVY, out float3 snowUVZ, out float snowIntensity)
{
    /* Get Normals*/
    normalsWorld = GetNormalsWorld(i.uv);
    wPos = GetWorldSpaceCoordFromDepth(i);
    float3 tangentX, tangentY, snowDir = _SnowFallParameters.xyz;
    GetLocalFrame(snowDir, tangentX, tangentY);
    snowUVX.xy = PlanarProjectionUVs(wPos, snowDir);    // Triplanar UVs
    snowUVY.xy = PlanarProjectionUVs(wPos, tangentX);   // Triplanar UVs
    snowUVZ.xy = PlanarProjectionUVs(wPos, tangentY);   // Triplanar UVs

    float3 normalizedWeight = float3(
        abs(dot(snowDir, normalsWorld)),     // Triplanar weights
        abs(dot(tangentX, normalsWorld)),    // Triplanar weights
        abs(dot(tangentY, normalsWorld))     // Triplanar weights
        );

    normalizedWeight /= normalizedWeight.x + normalizedWeight.y + normalizedWeight.z;

    snowUVX.z = normalizedWeight.x;
    snowUVY.z = normalizedWeight.y;
    snowUVZ.z = normalizedWeight.z;

    snowIntensity = GetNormalsIntensity(normalsWorld);
    snowIntensity *= tex2D(_Coverage, i.uv).r;
}

// Non deferred v2f input
void Setup2(in float3 normalsWorld, in float3 wPos, in float2 uv, out float2 snowUV, out float snowIntensity)
{
    /* Get Normals*/
    snowUV = PlanarProjectionUVs(wPos, _SnowFallParameters.xyz);
    snowIntensity = GetNormalsIntensity(normalsWorld);
    snowIntensity *= tex2D(_Coverage, uv).r;
}

void SetupTriplanar2(in float3 normalsWorld, in float3 wPos, in float2 uv, out float3 snowUVX,
    out float3 snowUVY, out float3 snowUVZ, out float snowIntensity)
{
    /* Get Normals*/
    float3 tangentX, tangentY, snowDir = _SnowFallParameters.xyz;
    GetLocalFrame(snowDir, tangentX, tangentY);
    snowUVX.xy = PlanarProjectionUVs(wPos, snowDir);    // Triplanar UVs
    snowUVY.xy = PlanarProjectionUVs(wPos, tangentX);   // Triplanar UVs
    snowUVZ.xy = PlanarProjectionUVs(wPos, tangentY);   // Triplanar UVs

    float3 normalizedWeight = float3(
        abs(dot(snowDir, normalsWorld)),     // Triplanar weights
        abs(dot(tangentX, normalsWorld)),    // Triplanar weights
        abs(dot(tangentY, normalsWorld))     // Triplanar weights
        );

    normalizedWeight /= normalizedWeight.x + normalizedWeight.y + normalizedWeight.z;

    snowUVX.z = normalizedWeight.x;
    snowUVY.z = normalizedWeight.y;
    snowUVZ.z = normalizedWeight.z;

    snowIntensity = GetNormalsIntensity(normalsWorld);
    snowIntensity *= tex2D(_Coverage, uv).r;
}

float4 SampleTex(sampler2D samp, v2f i, out float snowIntensity)
{
    // Sample the snow texture here
#ifndef TRIPLANAR_PROJ
    float3 normalsWorld, wPos; float2 snowUV;
    Setup(i, normalsWorld, wPos, snowUV, snowIntensity);

#ifndef TILING_FIX
    float4 snowTexCol = tex2D(samp, snowUV * _SnowTextureScale);
#else
    float4 snowTexCol = TextureNoTile(samp, snowUV * _SnowTextureScale);
#endif
#else   // TRIPLANAR_PROJ
    float3 normalsWorld, wPos, snowUVX, snowUVY, snowUVZ;
    SetupTriplanar(i, normalsWorld, wPos, snowUVX, snowUVY, snowUVZ, snowIntensity);

#ifndef TILING_FIX
    float4 snowTexCol = tex2D(samp, snowUVX.xy * _SnowTextureScale) * snowUVX.z;
    snowTexCol += tex2D(samp, snowUVY.xy * _SnowTextureScale) * snowUVY.z;
    snowTexCol += tex2D(samp, snowUVZ.xy * _SnowTextureScale) * snowUVZ.z;
#else
    // Tiling and triplanar is about 12 times more expensive than traditional snow. If you can
    // get away with non-triplanar and tiling looking stuff, do that. Snow tiling is easy to avoid
    float4 snowTexCol = TextureNoTile(samp, snowUVX.xy * _SnowTextureScale) * snowUVX.z;
    snowTexCol += TextureNoTile(samp, snowUVY.xy * _SnowTextureScale) * snowUVY.z;
    snowTexCol += TextureNoTile(samp, snowUVZ.xy * _SnowTextureScale) * snowUVZ.z;
#endif
#endif
    return snowTexCol;
}

float4 SampleTex(sampler2D samp, in float3 wPos, in float3 normalsWorld, in float2 uv, out float snowIntensity)
{
    // Sample the snow texture here
#ifndef TRIPLANAR_PROJ
    float2 snowUV;
    Setup2(normalsWorld, wPos, uv, snowUV, snowIntensity);

#ifndef TILING_FIX
    float4 snowTexCol = tex2D(samp, snowUV * _SnowTextureScale);
#else
    float4 snowTexCol = TextureNoTile(samp, snowUV * _SnowTextureScale);
#endif
#else   // TRIPLANAR_PROJ
    float3 snowUVX, snowUVY, snowUVZ;
    SetupTriplanar2(normalsWorld, wPos, uv, snowUVX, snowUVY, snowUVZ, snowIntensity);

#ifndef TILING_FIX
    float4 snowTexCol = tex2D(samp, snowUVX.xy * _SnowTextureScale) * snowUVX.z;
    snowTexCol += tex2D(samp, snowUVY.xy * _SnowTextureScale) * snowUVY.z;
    snowTexCol += tex2D(samp, snowUVZ.xy * _SnowTextureScale) * snowUVZ.z;
#else
    // Tiling and triplanar is about 12 times more expensive than traditional snow. If you can
    // get away with non-triplanar and tiling looking stuff, do that. Snow tiling is easy to avoid
    float4 snowTexCol = TextureNoTile(samp, snowUVX.xy * _SnowTextureScale) * snowUVX.z;
    snowTexCol += TextureNoTile(samp, snowUVY.xy * _SnowTextureScale) * snowUVY.z;
    snowTexCol += TextureNoTile(samp, snowUVZ.xy * _SnowTextureScale) * snowUVZ.z;
#endif
#endif
    return snowTexCol;
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// USER FUNCTIONS

/*
----- Functions that can be used to add snow support to pixel shaders -----

IMPORTANT: Unfortunately, I can't guarantee this works with every pixel shader. You might get an error saying
_CameraDepthTexture is undefined, so add this line BEFORE including this cginc:

sampler2D _CameraDepthTexture;

If you get an error saying v2f is undefined, you can add this declaration before including this cginc:

struct v2f
{
float4 pos : SV_POSITION;
float4 uv : TEXCOORD0;
float3 ray : TEXCOORD1;
};

----- How to use the functions -----

It's pretty simple to use. If you want to have textures, make sure you have the world space coordinates avaiailable. You
don't need world space coordinates for untextured snow. Most of the variables for snow are global, so they come from the
SnowRenderer object in your scene to control these functions.
*/

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// UNTEXTURED FUNCTIONS

/*  Name: SnowOnAlbedo
Purpose: Adds snow to an albedo color. Doesn't use textures.

Arguments:
inout float3 color - This is the color you would like to add snow to, and it will contain the new color after the function is finished
in float3 normalsWorld - These are the normals of your object, in world space. They won't be modified
in float2 screenPos - These are the screen coordinates. You can use ComputeScreenPos to get this value */
void SnowOnAlbedo(inout float3 color, in float3 normalsWorld, in float2 screenPos)
{
    float snowIntensity = GetNormalsIntensity(normalsWorld);
    float2 screenUV = screenPos;
#if UNITY_SINGLE_PASS_STEREO
    float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
    screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
#endif
    snowIntensity *= tex2D(_Coverage, screenUV).r;
    half3 snowAlbedoCol = half3(_SnowColAndCutoff.rgb);

    // Return the new Albedo color
    color = lerp(color, snowAlbedoCol, snowIntensity);
}

/*  Name: SnowOnSpecular
Purpose: Adds snow to a specular color and smoothness value. Doesn't use textures.

Arguments:
inout float3 specular - This is the specualr you would like to add snow to, and it will contain the new color after the function is finished
inout float smoothness - This is the smoothness channel, and it will also be modified
in float3 normalsWorld - These are the normals of your object, in world space. They won't be modified
in float2 screenPos - These are the screen coordinates. You can use ComputeScreenPos to get this value */
void SnowOnSpecular(inout float3 specular, inout float smoothness, in float3 normalsWorld, in float2 screenPos)
{
    float snowIntensity = GetNormalsIntensity(normalsWorld);
    float2 screenUV = screenPos;
#if UNITY_SINGLE_PASS_STEREO
    float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
    screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
#endif
    snowIntensity *= tex2D(_Coverage, screenUV).r;
    half3 snowSpecularColor = half3(_SnowSpecAndSmoothness.rgb);
    smoothness = lerp(smoothness, smoothness * _SnowSpecAndSmoothness.a, snowIntensity);

    // Return the new Specular color
    specular = lerp(specular, snowSpecularColor, snowIntensity);
}

/*  Name: SnowOnNormals
Purpose: Smooths the input normals to match vertical snow normals. Doesn't use textures.

Arguments:
inout float3 normalsWorld - These are the normals of your object, in world space, and it will contain the new normals after the function finishes
in float2 screenPos - These are the screen coordinates. You can use ComputeScreenPos to get this value */
void SnowOnNormals(inout float3 normalsWorld, in float2 screenPos)
{
    float snowIntensity = GetNormalsIntensity(normalsWorld);
    float2 screenUV = screenPos;
#if UNITY_SINGLE_PASS_STEREO
    float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
    screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
#endif
    snowIntensity *= tex2D(_Coverage, screenUV).r;
    float3 newNormalsWorld = float3(0, 0, 1);

    float3 N = normalize(normalsWorld), X, Y;
    GetLocalFrame(N, X, Y);
    newNormalsWorld = N * newNormalsWorld.z + X * newNormalsWorld.x + Y * newNormalsWorld.y;

    // Return the new Normals
    normalsWorld = lerp(normalsWorld, newNormalsWorld, snowIntensity);
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// TEXTURED FUNCTIONS

/*  Name: SnowOnAlbedoTextured
Purpose: Adds snow to an albedo color. uses textures.

Arguments:
inout float3 color - This is the color you would like to add snow to, and it will contain the new color after the function is finished
in float3 normalsWorld - These are the normals of your object, in world space. They won't be modified
in float2 screenPos - These are the screen coordinates. You can use ComputeScreenPos to get this value
in float3 worldPos - world position fragment coordinates*/
void SnowOnAlbedoTextured(inout float3 color, in float3 normalsWorld, in float2 screenPos, in float3 worldPos, in sampler2D samp)
{
    float snowIntensity;
    float2 screenUV = screenPos;
#if UNITY_SINGLE_PASS_STEREO
    float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
    screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
#endif
    half4 snowAlbedoCol = SampleTex(samp, worldPos, normalsWorld, screenUV, snowIntensity);
    snowAlbedoCol.rgb *= _SnowColAndCutoff.rgb;

    // Return the new Albedo color
    color = lerp(color, snowAlbedoCol.rgb, snowIntensity);
}

/*  Name: SnowOnSpecularTextured
Purpose: Adds snow to a specular color and smoothness value. uses textures.

Arguments:
inout float3 specular - This is the specualr you would like to add snow to, and it will contain the new color after the function is finished
inout float smoothness - This is the smoothness channel, and it will also be modified
in float3 normalsWorld - These are the normals of your object, in world space. They won't be modified
in float2 screenPos - These are the screen coordinates. You can use ComputeScreenPos to get this value
in float3 worldPos - world position fragment coordinates*/
void SnowOnSpecularTextured(inout float3 specular, inout float smoothness, in float3 normalsWorld, in float2 screenPos, in float3 worldPos, in sampler2D samp)
{
    float snowIntensity;
    float2 screenUV = screenPos;
#if UNITY_SINGLE_PASS_STEREO
    float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
    screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
#endif
    half4 snowSpecularColor = SampleTex(samp, worldPos, normalsWorld, screenUV, snowIntensity);
    snowSpecularColor.rgb *= _SnowSpecAndSmoothness.rgb;
    smoothness = lerp(smoothness, snowSpecularColor.a * _SnowSpecAndSmoothness.a, snowIntensity);

    // Return the new Specular color
    specular = lerp(specular, snowSpecularColor, snowIntensity);
}

/*  Name: SnowOnNormalsTextured
Purpose: Smooths the input normals to match vertical snow normals. uses textures.

Arguments:
inout float3 normalsWorld - These are the normals of your object, in world space, and it will contain the new normals after the function finishes
in float2 screenPos - These are the screen coordinates. You can use ComputeScreenPos to get this value
in float3 worldPos - world position fragment coordinates*/
void SnowOnNormalsTextured(inout float3 normalsWorld, in float2 screenPos, in float3 worldPos)
{
    float snowIntensity = GetNormalsIntensity(normalsWorld);
    snowIntensity *= tex2D(_Coverage, screenPos).r;
    float3 newNormalsWorld = float3(0, 0, 1);

    float3 N = normalize(normalsWorld), X, Y;
    GetLocalFrame(N, X, Y);
    newNormalsWorld = N * newNormalsWorld.z + X * newNormalsWorld.x + Y * newNormalsWorld.y;

    // Return the new Normals
    normalsWorld = lerp(normalsWorld, newNormalsWorld, snowIntensity);
}