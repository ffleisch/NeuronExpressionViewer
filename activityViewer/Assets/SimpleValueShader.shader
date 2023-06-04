Shader "Unlit/SimpleValueSahder"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_step("Step",Integer)=0
		_n_neurons("Number of Neurons",Integer)=50000

		_windowMax("Maximum for Windowing function",Float)=0
		_windowMin("Minimum for Windowing function",Float)=1

	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				uint id:SV_VertexID;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float value : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			int _n_neurons;
			int _step;

			float _windowMax;
			float _windowMin;

			float DecodeFloat(float4 c){
				int val =  ((int)(c[3]*255))<<24|
					((int)(c[2]*255))<<16|
					((int)(c[1]*255))<<8|
					(int)(c[0]*255);
				return asfloat(val);
			}
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);

				uint index = (uint)v.uv.x+_n_neurons*_step;
				uint width = (uint)_MainTex_TexelSize.z;
				float x = _MainTex_TexelSize.x * (index % width);
				float y = _MainTex_TexelSize.y * (width - (index / width) - 1);
				float4 color = tex2Dlod(_MainTex,float4(x,y,0,0));

				//float value = ((((uint)(255*color[0])) * (255 << 2)) + (((uint)(255*color[1])) * (255 << 1)) + (((uint)(255*color[2]) * 255) + ((uint)255*color[3]));
				o.value = DecodeFloat(color);// value;
				return o;
			}
			
			bool approx(float a, float b) {
				return a<=b + 0.00001 && a>=b-0.00001;
			}
			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);

				float intensity = (i.value-_windowMin)/(_windowMax-_windowMin);//(i.value-30)/100.0f+1.0f;//>0.5?1:0;
				intensity = clamp(intensity, 0, 1);
				
				
				
				//float4 area_col = float4(i.uv.y / 50.0f, 1-(i.uv.y/50.0f), 1, 1);
				//float4 area_col = float4((i.uv.x %50000)/ 50000.0f, (i.uv.x%5000)/5000.0f, (i.uv.y%50)/50.0f, 1);
				float4 area_col = approx(i.uv.y,8)?float4(0,1,0,1):(approx(i.uv.y,43)?float4(1,0,0,1):float4(1,1,1,1));
				
				
				fixed4 col = float4(intensity, intensity, intensity, 1)*area_col;


			// apply fog
			UNITY_APPLY_FOG(i.fogCoord, col);
			return float4(col); //i.color;
		}
		ENDCG
	}
		  }
}
