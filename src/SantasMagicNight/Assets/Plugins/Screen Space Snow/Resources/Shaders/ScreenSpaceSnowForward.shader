Shader "Hidden/ScreenSpaceSnowForward" 
{
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader {
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200 ZWrite Off Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

        struct Input
        {
            float2 uv;
        };

		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
