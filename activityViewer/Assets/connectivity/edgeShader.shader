Shader "Unlit/edgeShader"
{
	Properties
	{
		_col("Color",Color) = (0,0,0,1)
		_pixelWidth("Line Width",Float) = 5
		_n_index("selected neuron",Integer)=0
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv:TEXCOORD0;
			};

			struct v2g
			{
				float4 vertex : SV_POSITION;
				float2 uv:TEXCOORD0;
			};

			struct g2f {
				float4 vertex : POSITION;
			};

			uniform float _pixelWidth;
			uniform float4 _col;
			uniform int _n_index;
			//[maxvertexcount(4)]
			//void geom(lineadj input[4], inout TriangleStream<g2f> triStream) {

			[maxvertexcount(4)]
			void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream) {
				uint i1 = uint(input[0].uv.x+0.5);
				uint i2 = uint(input[1].uv.x+0.5);
				if (!((i1==_n_index)||(i2==_n_index))) {
					return;
				}
				float2 d1 = input[0].vertex.xy - input[1].vertex.xy;//input[i].mixedAdjacency.xy-input[i].vertex.xy;

				d1 *= _ScreenParams.xy;

				float2 normal = _pixelWidth * normalize(float2(-d1.y,d1.x)) / _ScreenParams.xy;

				for (int i = 0; i < 2; i++) {

					 g2f v1;
					 g2f v2;

					 //float2 d1=input[i].mixedAdjacency.xy-input[i].vertex.xy;

					 //float4 clip = UnityWorldToClipPos(input[i].vertex.xyz);
					 float4 v = input[i].vertex;
					 v1.vertex = float4(v.xy/v.w + normal,1,1);
					 v2.vertex = float4(v.xy/v.w - normal, 1,1);

					 triStream.Append(v1);
					 triStream.Append(v2);

					}

				triStream.RestartStrip();
			}



			v2g vert(appdata v)
			{
				v2g o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				//o.vertex = v.vertex;
				return o;
			}

			fixed4 frag(g2f i) : SV_Target
			{
				// sample the texture
				// apply fog
				return _col;
			}
			ENDCG
		}
	}
}
