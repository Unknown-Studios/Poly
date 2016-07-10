Shader "Unknown Studios/Low Poly" 
{
	Properties 
	{
		_Color("Color", Color) = (1,0,0,1)
	}
	SubShader 
	{
	
	    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

    	Pass 
    	{

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			
			uniform float4 _LightColor0;

			uniform float4 _Color;

			struct v2g 
			{
    			float4  pos : SV_POSITION;
				float3	norm : NORMAL;
     			float2  uv : TEXCOORD0;
			};
			
			struct g2f 
			{
    			float4  pos : SV_POSITION;
				float3  norm : NORMAL;
    			float2  uv : TEXCOORD0;            
				float3 diffuseColor : TEXCOORD1;
			};

			v2g vert(appdata_full v)
			{
				float3 v0 = mul(_Object2World, v.vertex).xyz;

				v.vertex.xyz = mul((float3x3)_World2Object, v0);

    			v2g OUT;
				OUT.pos = v.vertex;
				OUT.norm = v.normal;
    			OUT.uv = v.texcoord;
    			return OUT;
			}
			
			[maxvertexcount(3)]
			void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
			{
				float3 v0 = IN[0].pos.xyz;
				float3 v1 = IN[1].pos.xyz;
				float3 v2 = IN[2].pos.xyz;

				float3 centerPos = (v0 + v1 + v2) / 3.0;

				float3 vn = normalize(cross(v1 - v0, v2 - v0));
				
				float4x4 modelMatrix = _Object2World;
				float4x4 modelMatrixInverse = _World2Object;

				float3 normalDirection = normalize(
					mul(float4(vn, 0.0), modelMatrixInverse).xyz);
				float3 viewDirection = normalize(_WorldSpaceCameraPos
					- mul(modelMatrix, float4(centerPos, 0.0)).xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float attenuation = 1.0;

				float3 ambientLighting =
					UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb;

				float3 diffuseReflection =
					attenuation * _LightColor0.rgb * _Color.rgb
					* max(0.0, dot(normalDirection, lightDirection));

				g2f OUT;
				OUT.pos = mul(UNITY_MATRIX_MVP, IN[0].pos);
				OUT.norm = vn;
				OUT.uv = IN[0].uv;
				OUT.diffuseColor = ambientLighting + diffuseReflection;
				triStream.Append(OUT);

				OUT.pos = mul(UNITY_MATRIX_MVP, IN[1].pos);
				OUT.norm = vn;
				OUT.uv = IN[1].uv;
				OUT.diffuseColor = ambientLighting + diffuseReflection;
				triStream.Append(OUT);

				OUT.pos = mul(UNITY_MATRIX_MVP, IN[2].pos);
				OUT.norm = vn;
				OUT.uv = IN[2].uv;
				OUT.diffuseColor = ambientLighting + diffuseReflection;
				triStream.Append(OUT);
				
			}
			
			half4 frag(g2f IN) : COLOR
			{
				return float4(IN.diffuseColor, 1.0);
			}
			
			ENDCG

    	}
	}
}