// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'

// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'

// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'

Shader "RainEffects/WindshieldRain_glass"
{

	Properties
	{
		_RainTex("Rain Texture", 2D) = "white" {}
		_WhitenTex("White Texture", 2D) = "white" {}
		_MainTex("Main Texture", 2D) = "white" {}
		_Alpha("alpha multiplier", Float) = 0.5
		_BlurAmount("_BlurAmount", Range(0,4)) = 2.2
		_Cliping("clipping factor", Float) = 0.5
		_VerticalScrollSpeed("_VerticalScrollSpeed", Float) = 6
		_Distortion("_Distortion", Float) = 5
		_DistStrength("_Distortion strength", Float) = 1
		_HorizontalScrollSpeed("_HorizontalScrollSpeed", Float) = 0.5
		_SpeedMultiplier("_SpeedMultiplier", Float) = 1
		_AmountMultiplier("_AmountMultiplier", Float) = 1
		[Toggle] _DoRain("do rain", Float) = 0
	}

	SubShader
	{
		Tags { "Quaue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100
		Cull Off ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		GrabPass
		{
			Name "BASE"
			Tags {"LightMode" = "Always"}
		}

		Pass
		{
			Name "BASE"
			Tags {"LightMode" = "Always"}

		CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D _RainTex;
			uniform sampler2D _MainTex;
			uniform sampler2D _WhiteTex;
			uniform sampler2D _GrabTexture;
			uniform fixed _Alpha;
			uniform fixed _BlurAmount;
			uniform fixed _Cliping;
			uniform fixed _VerticalScrollSpeed;
			uniform fixed _HorizontalScrollSpeed;
			uniform fixed _Distortion;
			uniform fixed _DistStrength;
			uniform fixed _AmountMultiplier;
			uniform fixed _DoRain;
			float4 _MainTex_TexelSize;

			uniform fixed _SpeedMultiplier;

			// gaussian blur, distance [0,1]
			half4 gaussian_filter(float distance, float2 uv, float2 stride, sampler2D _Tex)
			{
				half4 s = tex2D(_Tex, uv) * 0.227027027 * _DistStrength;

				float2 d1 = stride * 10 * _Distortion * distance;
				s += tex2D(_Tex, uv + d1) * 0.3162162162 * _BlurAmount;
				s += tex2D(_Tex, uv - d1) * 0.3162162162 * _BlurAmount;

				return s;
			}

			fixed4 frag(v2f_img i) : COLOR
			{
				//if (!_DoRain)
				//	return tex2D(_MainTex, i.uv);


				_HorizontalScrollSpeed = 0.3;

				fixed4 mainTex = tex2D(_GrabTexture, i.uv);
				fixed4 whiteTex = tex2D(_WhiteTex, i.uv);

				fixed yScrollValue = _VerticalScrollSpeed * _Time[0]; // *_SpeedMultiplier;
				fixed xScrollValue = _HorizontalScrollSpeed * _Time[0];

				float2 rain_uv = i.uv;
				float2 rain_uv_offset = i.uv;
				rain_uv.y += yScrollValue;
				rain_uv_offset += float2(xScrollValue, yScrollValue * 0.5);

				fixed4 mask = tex2D(_RainTex, rain_uv) *
					tex2D(_RainTex, rain_uv_offset);

				mask = mask * tex2D(_WhiteTex, i.uv) * _Alpha * (1.0 / _AmountMultiplier);
				mask.a = 1;

				fixed4 rTex = tex2D(_GrabTexture, i.uv);

				float isMask = 0;
				if (mask.r > _Cliping && mask.g > _Cliping && mask.b > _Cliping)
					isMask = 1;

				mask = mask * (1 - isMask);
				rTex = mainTex * isMask + (1 - mask) * gaussian_filter(mask, i.uv, float2(_MainTex_TexelSize.x, 0), _GrabTexture) * (1 - isMask);

				return mainTex;



				return rTex;
			}


		ENDCG
	}
	}
}
