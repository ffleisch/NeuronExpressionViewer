Shader "Unlit/edgeShaderTextured"
{
	//billboarding shader for displaying neuron connections


	Properties
	{
		_col("Color",Color) = (0,0,0,1)
		_pixelWidth("Line Width",Float) = 5
		_n_index("selected neuron",Integer)=0
		_n_area("selected area",Integer)=0
		_texture("Connection Texture",2D) = "white" {}
		_connection_offset("Offset of Connections to Neuron",Range(0.0,5))=0
		_color_axons("Color of Axons",Color)=(1,1,1,1)
		_color_dendrites("Color of Dendrites",Color)=(1,1,0,1)
		_color_selected("Color of Selected Neuron",Color)=(1,0,0,1)

		[Toggle] _show_same_edges("Show persistant Edges",Float)=1
		[Toggle] _show_new_edges("Show new Edges",Float)=1
		[Toggle] _show_removed_edges("Show removed Edges",Float)=0
		
		
		[Toggle] _show_selected_neuron("Show selected neuron",Float)=1
		[Toggle] _show_selected_area  ("Show selected area",Float)=0
		[Toggle] _show_all("Show all neurons",Float)=0
	
	
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
				float2 edgeType:TEXCOORD1;
			};

			struct v2g
			{
				float4 vertex : SV_POSITION;
				float2 uv:TEXCOORD0;
				float2 edgeType:TEXCOORD1;
			};

			struct g2f {
				float4 vertex : POSITION;
				float2 uv:TEXCOORD0;
				float  selectionInfo : TANGENT;
			};

			uniform float _pixelWidth;
			uniform float4 _col;
			uniform int _n_index;
			uniform int _n_area;
			uniform float4 _color_axons;
			uniform float4 _color_dendrites;
			uniform float4 _color_selected;
			uniform float _connection_offset;

			uniform float _show_same_edges;
			uniform float _show_new_edges;
			uniform float _show_removed_edges;

			uniform float _show_selected_neuron;
			uniform float _show_selected_area;
			uniform float _show_all;
			
			sampler2D _texture;
			//[maxvertexcount(4)]
			//void geom(lineadj input[4], inout TriangleStream<g2f> triStream) {

			[maxvertexcount(8)]
			void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream) {
		

				//indices of the connected neurons
				uint i1 = uint(input[0].uv.x+0.5);
				uint i2 = uint(input[1].uv.x+0.5);
				
				//areas of the connected neurons
				uint a1 = uint(input[0].uv.y+0.5);
				uint a2 = uint(input[1].uv.y+0.5);


				bool direction=false;

				//selection of to show connection
				if (!_show_all) {

					//at least one end is in the current selected area
					if (_show_selected_area) {
						if (!((a1 == _n_area) || (a2 == _n_area))) {
							return;
						}

						if (a1 == _n_area) {
							direction = true;
						}
					}
					else {
						//at least one end is the current selected neuron
						if (_show_selected_neuron) {
							if (!((i1==_n_index)||(i2==_n_index))) {
								return;
							}
							if (i1 == _n_index) {
								direction = true;
							}

						}
						else {
							return;//yikes, there has to be a better way with multiple shaders and less ifs
						}

					}
				}
				
				//check and hide/show difeerent edge types
				uint edge_type = uint(input[0].edgeType.x + 0.5);
				if (_show_same_edges == 0 && edge_type == 0) {
					return;
				}

				if (_show_new_edges == 0 && edge_type == 1) {
					return;
				}
				if (_show_removed_edges == 0 && edge_type == 2) {
					return;
				}



				//clipsapce to screenspace
				float2 start = input[0].vertex.xy/input[0].vertex.w;
				float2 end = input[1].vertex.xy/input[1].vertex.w;

				//direction of the connection in screen space
				float2 major = end - start;
				float2 normal = _pixelWidth * normalize(float2(-major.y,major.x)/_ScreenParams.xy);
				
				float len = length((end - start)*_ScreenParams.xy);
				
				//how wide is the connection rtelative to its length
				float ratio = length(normal) / len;
				

				float offset_ratio = _connection_offset*ratio;
				//float line_ratio =length(minor) / (major_length*2);

				if (offset_ratio > 0.25) {
					offset_ratio = 0.25;
				}


				//offset the connection from the neuron position, except when its to close
				float a =direction? offset_ratio:0;
				float b = a+ratio;
				float d =direction?1:( 1-offset_ratio);
				float c = d-ratio;// d - line_ratio;
				if (b > c) {
					b =(a+d)/2.0;
					c = (a + d) / 2.0;
				}
				

				//create the 4 vertices for the billboarding
				g2f vertices[8];
				vertices[0].vertex = float4(start+a*major+normal/_ScreenParams.xy,1,1);
				vertices[0].uv = float2(0,0);
				vertices[1].vertex = float4(start+a*major-normal/_ScreenParams.xy,1,1);
				vertices[1].uv = float2(1,0);


				vertices[2].vertex = float4(start+b*major+normal/_ScreenParams.xy,1,1);
				vertices[2].uv = float2(0,0.5);
				vertices[3].vertex = float4(start+b*major-normal/_ScreenParams.xy,1,1);
				vertices[3].uv = float2(1,0.5);

				vertices[4].vertex = float4(start+c*major+normal/_ScreenParams.xy,1,1);
				vertices[4].uv = float2(0,0.5);
				vertices[5].vertex = float4(start+c*major-normal/_ScreenParams.xy,1,1);
				vertices[5].uv = float2(1,0.5);


				vertices[6].vertex = float4(start+d*major+normal/_ScreenParams.xy,1,1);
				vertices[6].uv = float2(0,1);
				vertices[7].vertex = float4(start+d*major-normal/_ScreenParams.xy,1,1);
				vertices[7].uv = float2(1,1);

				for (int i = 0; i < 8; i++) {
					vertices[i].selectionInfo = float(direction);
					triStream.Append(vertices[i]);
				}

				triStream.RestartStrip();
			}



			v2g vert(appdata v)
			{
				v2g o;

				//project the vertices into screenspace before the geom shader
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.edgeType = v.edgeType;
				//o.vertex = v.vertex;
				return o;
			}

			fixed4 frag(g2f i) : SV_Target
			{
				// sample the texture
				// apply fog


				//apply the selected colors to the connection texture
				float4 col = tex2Dlod(_texture,float4(i.uv,0,0));
				if (i.selectionInfo) {
					col = col[0] * _color_axons + col[1]* _color_dendrites * _color_selected;
				
				}
				else {
					col = col[0]* _color_axons * _color_selected + col[1] * _color_dendrites;
				
				}
				return col;
			}
			ENDCG
		}
	}
}
