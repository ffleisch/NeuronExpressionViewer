Shader "Unlit/edgeShaderTextured"
{
	Properties
	{
		_col("Color",Color) = (0,0,0,1)
		_pixelWidth("Line Width",Float) = 5
		_n_index("selected neuron",Integer)=0
		_texture("Connection Texture",2D) = "white" {}
		_connection_offset("Offset of Connections to Neuron",Float)=0
		_color_axons("Color of Axons",Color)=(1,1,1,1)
		_color_dendrites("Color of Dendrites",Color)=(1,1,0,1)
	}	
		SubShader
	{
		Tags { "RenderType" = "Opaque"
		"Queue" = "Overlay"}
		LOD 100

		Pass
		{
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha

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
				float2 uv:TEXCOORD0;
			};

			uniform float _pixelWidth;
			uniform float4 _col;
			uniform int _n_index;
			uniform float4 _color_axons;
			uniform float4 _color_dendrites;
			uniform float _connection_offset;
			sampler2D _texture;
			//[maxvertexcount(4)]
			//void geom(lineadj input[4], inout TriangleStream<g2f> triStream) {

			[maxvertexcount(8)]
			void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream) {
				uint i1 = uint(input[0].uv.x+0.5);
				uint i2 = uint(input[1].uv.x+0.5);
				if (!((i1==_n_index)||(i2==_n_index))) {
					return;
				}
				float2 d1 = input[0].vertex.xy - input[1].vertex.xy;//input[i].mixedAdjacency.xy-input[i].vertex.xy;

				d1 *= _ScreenParams.xy;

				float2 normal = _pixelWidth * normalize(float2(-d1.y,d1.x)) / _ScreenParams.xy;

				float4 corners[4];

				float4 start = input[0].vertex;
				float4 end = input[1].vertex;
				corners[0] = float4(start.xy/start.w + normal,1,1);
				corners[1] = float4(start.xy/start.w - normal,1,1);
				corners[2] = float4(end.xy/start.w + normal,1,1);
				corners[3] = float4(end.xy/start.w - normal,1,1);
	

				float4 minor= corners[1]-corners[0];
				float4 major = corners[2] - corners[0];
				float major_length = length(major);
				
				float offset_ratio = _connection_offset;
				float line_ratio = length(minor) / (major_length*2);

				if (offset_ratio > 0.25) {
					offset_ratio = 0.25;
				}

				float a = offset_ratio;
				float b = a+line_ratio;
				float d = 1-offset_ratio;
				float c = d-line_ratio;
				if (b > c) {
					b = 0.5;
					c = 0.5;
				
				}

				g2f vertices[8];
				vertices[0].vertex = corners[0]+a*major;
				vertices[0].uv = float2(0,0);
				vertices[1].vertex = corners[1]+a*major;
				vertices[1].uv = float2(1,0);


				vertices[2].vertex = corners[0]+b*major;
				vertices[2].uv = float2(0,0.5);
				vertices[3].vertex = corners[1]+b*major;
				vertices[3].uv = float2(1,0.5);

				vertices[4].vertex = corners[0]+c*major;
				vertices[4].uv = float2(0,0.5);
				vertices[5].vertex = corners[1]+c*major;
				vertices[5].uv = float2(1,0.5);


				vertices[6].vertex = corners[0]+d*major;
				vertices[6].uv = float2(0,1);
				vertices[7].vertex = corners[1]+d*major;
				vertices[7].uv = float2(1,1);

				for (int i = 0; i < 8; i++) {
					triStream.Append(vertices[i]);
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
				float4 col = tex2Dlod(_texture,float4(i.uv,0,0));
				if (col[3] <0.5) {
					
				}
				return col[0]*_color_axons+col[1]*_color_dendrites;
			}
			ENDCG
		}
	}
}