Shader "Custom/Standard AlphaMap" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", 2D) = "black" {}
        _AlphaMap ("Alpha Map", 2D) = "white" {}
        _BumpMap ("Bumpmap", 2D) = "bump" {}
	}
	SubShader {
		Tags { "RenderType"="Transparent" }
		LOD 200

        //ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha:blend

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
        sampler2D _Metallic;
        sampler2D _AlphaMap;
        sampler2D _BumpMap;

		struct Input {
			float2 uv_MainTex;
            float2 uv_Metallic;
            float2 uv_AlphaMap;
            float2 uv_BumpMap;
		};

		half _Glossiness;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = tex2D (_Metallic, IN.uv_Metallic);
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
            fixed4 a = tex2D(_AlphaMap, IN.uv_AlphaMap);
            if (a.r == 0) {
                o.Alpha = 0; //
            }
            o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));

		}
		ENDCG
	}
	FallBack "Diffuse"
}
