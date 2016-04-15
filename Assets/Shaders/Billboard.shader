Shader "Unknown Studios/Billboard" {
	Properties
	{
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Particle Texture", 2D) = "white" {}
		_CullDistance("Cull distance", Float) = 1000.0
	}

		Category
		{
			Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			AlphaTest Greater .01
			Cull Off

			// important
			//ZWrite off

			SubShader
			{
				Pass
				{
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag

					#include "UnityCG.cginc"

					uniform sampler2D _MainTex;
					uniform fixed4 _TintColor;
					uniform float _CullDistance;

					struct v2f
					{
						float4 vertex : POSITION;
						fixed4 color : COLOR;
						float2 texcoord : TEXCOORD0;
					};

					v2f vert(appdata_full v)
					{
						v2f o;
						o.vertex = mul(UNITY_MATRIX_P,
							 mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
							+ float4(v.vertex.x, v.vertex.y, 0.0, 0.0));

						o.color = v.color;
						o.texcoord = v.texcoord;
						return o;
					}

					fixed4 frag(v2f i) : COLOR
					{
						float dist = distance(_WorldSpaceCameraPos, mul(_Object2World, i.texcoord));
						clip(_CullDistance - dist);

						return 2.0f * i.color * tex2D(_MainTex, i.texcoord);
					}
					ENDCG
				}
			}
		}
}