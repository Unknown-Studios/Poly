Shader "Unknown Studios/Wind" {
	Properties{
		_Color("Tint", Color) = (1,1,1,1)
		_Wind("Wind", Vector) = (0,0,0,0)
		_MainTex("Main", 2D) = "white" {}
		_CullDistance("Cull Distance", Float) = 750.0
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

				float3 worldNormal = WorldNormalVector(IN, o.Normal);
				half w = dot(worldNormal, normalize(Vector(0,1,0,0)));
				half3 color = lerp(half3(1, 1, 1), _Color.rgb, w);

				o.Albedo = color;
				o.Alpha = _Color.a;
			}
			ENDCG
		}
		}
}