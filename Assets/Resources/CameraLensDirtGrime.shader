Shader "LensEffects/CameraLensDirtGrime"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_DirtTex("_DirtTex", 2D) = "white" {}
		_BlendingFactor("_BlendingFactor", Float) = 1
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _DirtTex;
			uniform fixed _BlendingFactor;

			fixed4 frag(v2f_img i) : COLOR
            {
				fixed4 mainTex = tex2D(_MainTex, i.uv);
				fixed4 dirtTex = tex2D(_DirtTex, i.uv);

				return mainTex + dirtTex * _BlendingFactor;
            }

            ENDCG
        }
    }
}
