  Shader "Unlit/ScreenPos" {
	Properties {
	  _MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
	  Tags { "RenderType" = "Opaque" }
	  CGPROGRAM
	  #pragma surface surf NoLighting
	  struct Input {
		  float2 uv_MainTex;
		  float4 screenPos;
	  };
	  sampler2D _MainTex;
	  void surf (Input IN, inout SurfaceOutput o) {
		  float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
		  screenUV *= float2(1,1);
		  o.Albedo = tex2D (_MainTex, screenUV).rgb;
	  }

fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
{
	fixed4 c;
	c.rgb = s.Albedo; 
	c.a = s.Alpha;
	return c;
}

	  ENDCG
	} 
	Fallback "Diffuse"
  }