Shader "FX/Hologram Shader"
{
	Properties
	{
		_Color("Color", Color) = (0, 1, 1, 1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_AlphaTexture ("Alpha Mask (R)", 2D) = "white" {}
		//Alpha Mask Properties
		_Scale ("Alpha Tiling", Float) = 3
		_ScrollSpeedV("Alpha scroll Speed", Range(0, 5.0)) = 1.0
		// Glow
		_GlowIntensity ("Glow Intensity", Range(0.01, 1.0)) = 0.5
	}

	SubShader
	{
		Tags{ "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		Pass
		{
			Lighting Off 
			ZWrite On
			Blend SrcAlpha One
			Cull Back

			CGPROGRAM
				
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct vertIn{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float3 normal : NORMAL;
				};

				struct vertOut{
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
					float3 grabPos : TEXCOORD1;
					float3 viewDir : TEXCOORD2;
					float3 worldNormal : NORMAL;
				};

				fixed4 _Color, _MainTex_ST;
				sampler2D _MainTex, _AlphaTexture;
				float _Scale, _ScrollSpeedV, _GlowIntensity;

				vertOut vert(vertIn i){
					vertOut o;

					o.vertex = UnityObjectToClipPos(i.vertex);
					o.uv = TRANSFORM_TEX(i.uv, _MainTex);

					//Alpha mask coordinates
					o.grabPos = UnityObjectToViewPos(i.vertex);

					//Scroll Alpha mask uv
					o.grabPos.y += _Time * _ScrollSpeedV;

					o.worldNormal = UnityObjectToWorldNormal(i.normal);
					o.viewDir = normalize(UnityWorldSpaceViewDir(o.grabPos.xyz));

					return o;
				}

				fixed4 frag(vertOut o) : SV_Target{
					
					fixed4 alphaColor = tex2D(_AlphaTexture,  o.grabPos.xy * _Scale);
					fixed4 pixelColor = tex2D (_MainTex, o.uv);
					pixelColor.w = alphaColor.w;

					// Rim Light
					float rim = 1.0-saturate(dot(o.viewDir, o.worldNormal));

					return pixelColor * _Color * ( rim + _GlowIntensity);
				}
			ENDCG
		}
	}
}