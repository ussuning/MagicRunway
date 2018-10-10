// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/UserBlendShader" 
{
	Properties
	{
		_MainTex ("MainTex", 2D) = "white" {}
		_BackTex ("BackTex", 2D) = "white" {}
        _Threshold ("Depth Threshold", Range(0, 0.5)) = 0.1
		_AlphaThreshold("Alpha Threshold", Range(0.01, 0.5)) = 0.01
		_EdgeThreshold("Edge Threshold", Range(0.01, 1.0)) = 0.25
	}

	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }

		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
		
			CGPROGRAM
			#pragma target 5.0
			//#pragma enable_d3d11_debug_symbols

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			//float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			sampler2D _CameraDepthTexture;

			uniform sampler2D _BackTex;
			uniform float _Threshold;
			uniform float _AlphaThreshold;
			uniform float _EdgeThreshold;

			uniform float _ColorResX;
			uniform float _ColorResY;
			uniform float _DepthResX;
			uniform float _DepthResY;

			uniform float _ColorOfsX;
			uniform float _ColorMulX;
			uniform float _ColorOfsY;
			uniform float _ColorMulY;

			//uniform float _DepthFactor;

			StructuredBuffer<float2> _DepthCoords;
			StructuredBuffer<float> _DepthBuffer;


			struct v2f 
			{
			   float4 pos : SV_POSITION;
			   float2 uv : TEXCOORD0;
			   float2 uv2 : TEXCOORD1;
			   float4 scrPos : TEXCOORD2;
			};

			v2f vert (appdata_base v)
			{
			   v2f o;
			   
			   o.pos = UnityObjectToClipPos (v.vertex);
			   o.uv = v.texcoord;

			   o.uv2.x = o.uv.x;
			   o.uv2.y = 1 - o.uv.y;

			   o.scrPos = ComputeScreenPos(o.pos);

			   return o;
			}

			half4 frag (v2f i) : COLOR
			{
			    float camDepth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);
				//float camDepth01 = Linear01Depth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);

				float2 ctUv = float2(_ColorOfsX + i.uv.x * _ColorMulX, 1.0 - i.uv.y /**_ColorOfsY + i.uv.y * _ColorMulY*/);
#if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0)
                {
                    ctUv.y = 1.0 - ctUv.y;
                }
#endif

				// for non-flipped textures
				float2 ctUv2 = float2(ctUv.x, ctUv.y);
				
				int cx = (int)(ctUv.x * _ColorResX);
				int cy = (int)(ctUv.y * _ColorResY);
				int ci = (int)(cx + cy * _ColorResX);

				half4 clrMain = tex2D(_MainTex, i.uv);
				half4 clrBack = tex2D(_BackTex, ctUv2);
				if (!isinf(_DepthCoords[ci].x) && !isinf(_DepthCoords[ci].y))
				{
					int dx = (int)_DepthCoords[ci].x;
					int dy = (int)_DepthCoords[ci].y;
					int di = (int)(dx + dy * _DepthResX);

					//float di_length = _DepthResX * _DepthResY;
					//if(di >= 0 && di < di_length)
					{
						float kinDepth = _DepthBuffer[di] / 1000.0;
						if (di > 0 && di < _DepthResX*_DepthResY) {
							// Average the two most close values.
							float di_diplus1 = abs(_DepthBuffer[di] - _DepthBuffer[di + 1]) / 1000.0;
							float di_diminus1= abs(_DepthBuffer[di] - _DepthBuffer[di - 1]) / 1000.0;
							if (di_diplus1 > _EdgeThreshold || di_diminus1 > _EdgeThreshold)
								//return half4(1.0, 0, 0, 1.0);
								kinDepth = kinDepth + _AlphaThreshold * 0.5f;
						}
						//kinDepth *= _DepthFactor;
					
						if(camDepth > 0.1 && camDepth < 10.0 && 
							(kinDepth < 0.1 || (kinDepth < 10.0 && camDepth <= (kinDepth + _Threshold))))
						{
							return clrMain;
						}
						else
						{
							//return clrMain;
							float diff = (camDepth - kinDepth);
							float alpha = diff / _AlphaThreshold;
							if (alpha > 1.0)
								alpha = 1.0;
							half3 blend = clrBack.rgb * alpha + clrMain.rgb * (1.0 - alpha);
							return half4(blend, 1.0);
						}
					}
				}
				else
				{
					//return half4(i.uv.x, i.uv.y, 0, 1);
					if(camDepth > 0.1 && camDepth < 10.0)
					{
						//return half4(0.0, 0.0, 1.0, 1.0);
						return clrMain;
					}
					else
					{
						//return half4(0.0, 1.0, 0.0, 1.0);
						return half4(clrBack.rgb, 1.0);
					} 
				}
			}

			ENDCG
		}
	}

	FallBack "Diffuse"
}
