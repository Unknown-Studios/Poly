Shader "Unknown Studios/Terrain" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Tint", 2D) = "white" {}
		_CullDistance("Cull Distance", Float) = 2500.0
	}
		Category
		{
			Tags{
			"Queue" = "Transparent-99"
			"RenderType" = "TreeTransparentCutout"
		}
			Cull Off
			ColorMask RGB

			SubShader
		{
			CGPROGRAM
			#pragma surface surf Lambert vertex:vert
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			uniform float _CullDistance;
			fixed4 _Color;

			struct Input {
				float2 uv_MainTex;
				float3 worldPos;
				float3 worldNormal;
				INTERNAL_DATA
			};

			void vert(inout appdata_full v)
			{
			}

			void surf(Input IN, inout SurfaceOutput o) {
				fixed4 difTex = tex2D(_MainTex, IN.uv_MainTex);

				float dist = distance(_WorldSpaceCameraPos, IN.worldPos);
				clip(_CullDistance - dist);

				o.Albedo = difTex.rgb;
			}
			ENDCG
		}
			FallBack "Diffuse"
		}
}