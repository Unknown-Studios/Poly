Shader "Unknown Studios/Cull at distance" {
	Properties{
		_Color("Tint", Color) = (1,1,1,1)
		_MainTex("Main", 2D) = "white" {}
		_CullDistance("Cull Distance", Float) = 250.0
	}

		Category
		{
			Tags {"RenderType" = "Opague"}
			Cull Off
			SubShader
			{
				CGPROGRAM
					#pragma surface surf Lambert
					#include "UnityCG.cginc"

					uniform sampler2D _MainTex;
					uniform fixed4 _Color;
					uniform float _CullDistance;

					struct Input {
						float2 uv_MainTex;
						float3 worldPos;
					};

					void surf(Input IN, inout SurfaceOutput o) {
						fixed4 difTex = tex2D(_MainTex, IN.uv_MainTex);

						float dist = distance(_WorldSpaceCameraPos, IN.worldPos);
						clip(_CullDistance - dist);

						o.Albedo = difTex.rgb * _Color.rgb;
					}
				ENDCG
			}
		}
}