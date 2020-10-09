Shader "SSS/Extras/Icicles" 
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Specular("Specular (RGB)", 2D) = "black" {}
		_Smoothness("Smoothness", Range(0, 1.0)) = 0.5
		_Normal("Normals", 2D) = "bump" {}
		_Occlusion("Occlusion", 2D) = "white" {}

		_Height("Height", 2D) = "black"{}
		_IceNormals("IcicleNormals", 2D) = "bump"{}
		_HeightmapTiling("Heightmap tiling", Float) = 1.0
		_Tess("Icicle Detail", Range(1,32)) = 4
		_Displacement("Icicle Length", Range(0, 10.0)) = 0.3
		_IcicleSpecular("Ice Spec(RGB), Smooth(A)", Color) = (0.9,0.9,0.9,0.9)
		_IcicleColor("Ice Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf StandardSpecular  fullforwardshadows vertex:disp tessellate:tessNormal addshadow

		#pragma target 5.0
		#include "Tessellation.cginc"

		#define interpMax 1.0f

		struct appdata
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
			float2 texcoord2 : TEXCOORD2;
			float4 color : COLOR;
		};

		float _Tess;
		sampler2D _Height;
		float _HeightmapTiling;

		float4 tessNormal(appdata v0, appdata v1, appdata v2)
		{
			float4 averageNormal = float4(normalize((v0.normal + v1.normal + v2.normal) * 0.333f), 0);
			float3 worldSpaceNrm = mul(unity_ObjectToWorld, averageNormal).xyz;
			
			return lerp(1, _Tess, smoothstep(-0.2, 0.0, dot(worldSpaceNrm, float3(0, -1, 0))));
		}

		float _Displacement;
		sampler2D _IceNormals;

		void disp(inout appdata v)
		{
			float d = tex2Dlod(_Height , float4(v.texcoord.xy * _HeightmapTiling,0,0)).r;

			float3 worldSpaceNrm = mul(unity_ObjectToWorld, v.normal).xyz;
			float3 worldSpaceDwn = mul(unity_WorldToObject, float4(0, -1, 0, 0)).xyz;
			
			float4 icicleAmount = float4(dot(worldSpaceNrm, float3(0, -1, 0)), d * _Displacement, 1, 1);

			float interpVal = smoothstep(0, interpMax, icicleAmount.x);

			v.vertex.xyz += worldSpaceDwn * d * _Displacement * interpVal;

			float3 icicleNormals = tex2Dlod(_IceNormals, float4(v.texcoord.xy * _HeightmapTiling, 0, 0)).xzy;

			v.normal.xyz = lerp(v.normal, icicleNormals, interpVal * saturate(_Displacement));
			v.color = icicleAmount;
		}

		sampler2D _MainTex;
		sampler2D _Specular;
		sampler2D _Normal;
		sampler2D _Occlusion;
		float _Smoothness;

		struct Input
		{
			float2 uv_MainTex;
			float2 color : COLOR;
			float3 worldNormal; INTERNAL_DATA
		};

		fixed4 _Color;
		fixed4 _IcicleColor;
		fixed4 _IcicleSpecular;

		void surf(Input IN, inout SurfaceOutputStandardSpecular o)
		{
			float3 wsNorm = WorldNormalVector(IN, o.Normal);
			
			float interpVal = smoothstep(0, 0.1f, IN.color.x * IN.color.y);

			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			c.rgb = lerp(c.rgb * _Color, _IcicleColor.rgb, interpVal);
			o.Albedo = c.rgb;

			// Specular and smoothness
			float4 specularTexture = lerp(tex2D(_Specular, IN.uv_MainTex), _IcicleSpecular, interpVal);
			o.Specular = specularTexture.rgb;
			o.Smoothness = specularTexture.a;
			o.Normal = UnpackNormal(lerp(tex2D(_Normal, IN.uv_MainTex), float4(0.5f, 0.5f, 1.0f, 1.0f), interpVal));

			o.Occlusion = lerp(tex2D(_Occlusion, IN.uv_MainTex), 1.0f, interpVal);
			o.Alpha = c.a;
		}
	
		ENDCG
	}
	FallBack "Diffuse"
}