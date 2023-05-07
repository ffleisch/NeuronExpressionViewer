Shader "Unlit/SimpleValueSahder"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_step("Step",Int)=0
		_n_neurons("Number of Neurons",Int)=50000
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

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);

				float intensity = i.value;//>0.5?1:0;
				fixed4 col = float4(intensity,intensity,intensity,1);


			// apply fog
			UNITY_APPLY_FOG(i.fogCoord, col);
			return float4(col); //i.color;
		}
		ENDCG
	}
		  }
}