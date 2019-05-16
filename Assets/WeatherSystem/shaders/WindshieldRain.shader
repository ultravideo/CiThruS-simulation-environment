Shader "RainEffects/WindshieldRain"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _RainTex ("Rain map (BW)", 2D) = "white" {}
        _RainTexMultiplier ("Rain map multiplier (BW)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Alpha ("Alpha", Range(0.0,1)) = 0.5
        _VerticalScrollSpeed ("Vertical Scroll Speed", Range(0,10)) = 6
        _HorizontalScrollSpeed ("Scroll Warp Multiplier ", Range(0,1)) = 0.2
		[Toggle] _DoRain ("Do Rain", float) = 1
		[Toggle] _FlipVertically ("Flip Direction Vertically", float) = 0
    }
    SubShader
    {
        Tags { "Quaue"="Transparent" "RenderType"="Transparent" }
        LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha


		Cull Off
        CGPROGRAM
        #pragma surface surf Standard alpha
        #pragma target 3.0

        sampler2D _MainTex;
		sampler2D _RainTex;
		sampler2D _RainTexMultiplier;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_RainTex;
			float2 uv_RainTexMultiplier;
        };

        half _Glossiness;
        half _Metallic;
        half _Alpha;
		half _VerticalScrollSpeed;
		half _HorizontalScrollSpeed;
		float _DoRain;
		float _FlipVertically;
        fixed4 _Color;
		fixed _ScrollYSpeed;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			if (_DoRain == 1)
			{
				fixed2 rain_uv = IN.uv_RainTex;
				fixed2 rain_uv_offset = IN.uv_RainTex;

				// flip scrolling rain texture to vertical mode
				if (_FlipVertically == 1)
				{
					rain_uv = IN.uv_RainTex;
					rain_uv_offset = IN.uv_RainTex;

					// 90 degree rotation
					float2x2 rotationMat = float2x2 (0, -1, 1, 0);
					rain_uv = mul(rain_uv, rotationMat);
					rain_uv_offset = mul(rain_uv_offset, rotationMat);
				}


				fixed yScrollValue = _VerticalScrollSpeed * _Time;
				fixed xScrollValueOffset = _HorizontalScrollSpeed * _Time;

				rain_uv += fixed2(0, yScrollValue);
				rain_uv_offset += (xScrollValueOffset, 0);

				// rain effect
				fixed4 t = tex2D(_RainTex, rain_uv) * tex2D(_RainTex, rain_uv_offset);

				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
				o.Albedo = c.rgb;

				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = _Alpha * 7 * c.a * t.rgb;
				o.Normal = UnpackNormal(t);
			}
			else
			{
				// if no rain effect is desired

				fixed2 main_uv = IN.uv_MainTex;
				fixed4 t = tex2D(_MainTex, main_uv) * _Color;
				o.Albedo = t.rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = 0.08;
			}
        }
        ENDCG
    }
    FallBack "Diffuse"
}
