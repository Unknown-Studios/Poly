Shader "Unknown Studios/Wind" {
	Properties{
		_Color("Tint", Color) = (1,1,1,1)
		_Wind("Wind", Vector) = (0,0,0,0)
		_MainTex("Main", 2D) = "white" {}
		_CullDistance("Cull Distance", Float) = 1000.0

		White("Snow Color", Color) = (0,0,0,1)
	}
		Category
		{
			Tags{
				"Queue" = "Transparent-99"
				"IgnoreProjector" = "True"
				"RenderType" = "TreeTransparentCutout"
				"DisableBatching" = "True"
			}
			Cull Off
			ColorMask RGB

			SubShader
			{
				CGPROGRAM
					#pragma surface surf Lambert vertex:vert
					#include "UnityCG.cginc"

					uniform sampler2D _MainTex;
					uniform float4 _Wind;
					uniform fixed4 _Color;
					uniform float _CullDistance;
					uniform float _Weather;
					uniform fixed4 White;
					uniform float _Exponent;

					struct Input {
						float2 uv_MainTex;
						float3 worldPos;
					};

					void vert(inout appdata_full v)
					{
						v.vertex.x += sin(_Wind.x) * v.vertex.y;
						v.vertex.z += sin(_Wind.y) * v.vertex.y;
					}

					void surf(Input IN, inout SurfaceOutput o) {
						fixed4 difTex = tex2D(_MainTex, IN.uv_MainTex);

						clip(difTex.a - 0.25);

						float dist = distance(_WorldSpaceCameraPos, IN.worldPos);
						clip(_CullDistance - dist);

						o.Albedo = _Color.rgb + White;
					}
				ENDCG
			}
		}
			Fallback "Diffuse"
}