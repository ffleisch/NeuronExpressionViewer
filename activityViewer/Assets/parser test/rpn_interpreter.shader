
//take two arrays representing a mathemarttical expression in posfix notation and render it for every pixel

Shader "Unlit/rpn_interpreter"
{
    Properties
    {
        _attributesArrayTexture("Attributes Array Texture", 2DArray) = "" {}
        _MainTex ("Texture", 2D) = "white" {}
        _neuronMap("Map from surface to the neurons",2D) = "black"{}
		_n_neurons("Number of Neurons",Integer)=50000
		_step("Step",Integer)=0
        test_index("Test index",Integer)=0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma require 2darray
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                //float4 col: COLOR;
            };

            sampler2D _MainTex;
            sampler2D _neuronMap;

            float4 _MainTex_ST;
            uniform float values[128];
            uniform float num_values;
            uniform float tokens[256];
            uniform int num_tokens = 0;
           
            uniform float _attributeIndices[16];
			int _n_neurons;
			int _step;
            uint _attributesArrayTextureWidth;
            float4 _attributesArrayTextureTexelSize;

            int test_index = 0;
            UNITY_DECLARE_TEX2DARRAY(_attributesArrayTexture);

            float decodeFloat(float4 c){
				int val =  ((int)(c[3]*255))<<24|
					((int)(c[2]*255))<<16|
					((int)(c[1]*255))<<8|
					(int)(c[0]*255);
				return asfloat(val);
			}
            
            float sampleStep(float attributeIndex,float index) {
                uint intIndex = index+_n_neurons*_step;
				uint width = _attributesArrayTextureWidth;
				float x = _attributesArrayTextureTexelSize.x * (index % width);
				float y = _attributesArrayTextureTexelSize.y * (width - (index / width) - 1);
                float4 col = UNITY_SAMPLE_TEX2DARRAY_LOD(_attributesArrayTexture, float3(x,y,attributeIndex),0);

                //float col= UNITY_SAMPLE_TEX2DARRAY(_attributesArrayTexture,float3(,,attributeIndex))
                return decodeFloat(col);
			}
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

               
                //o.col = col;
                return o;
            }
            



            fixed4 frag(v2f inp) : SV_Target
            {
                float stack[64];//stack of values
                stack[0] = 0;
                int stack_index = 0;
                int value_index = 0;
                float4 colOut;
                for (int i = 0; i < num_tokens; i++) {
                    int token = floor(tokens[i]+0.5);//TODO chekc if necesseary
                    if (token == -1) {//take the next value and put it on the stack
                        stack_index++;
                        stack[stack_index] = values[value_index];
                        value_index++;
                    }else
                    if (token == -2) {//put the uv.x on the stack
                        stack_index++;
                        stack[stack_index] = inp.uv.x;
                        value_index++;
                    }else
                    if (token == -3) {
                        stack_index++;
                        stack[stack_index] = inp.uv.y;
                        value_index++;
                    }else

					if (token == -4) {
                        stack_index++;
                        stack[stack_index] = sampleStep(values[value_index],floor(decodeFloat(tex2Dlod(_neuronMap,float4(inp.uv,0,0)))+0.5));
                        value_index++;
                    }else

                    if (token == 1) {//addition, take two value from the stack and put the result on top
                        stack[stack_index - 1] = stack[stack_index-1] + stack[stack_index];
                        stack_index -= 1;
                    }else

                    if (token == 2) {//multiplication, take two value from the stack and put the result on top
                        stack[stack_index - 1] = stack[stack_index-1] * stack[stack_index];
                        stack_index -= 1;
                    }else
                    if (token == 3) {//substraction, take two value from the stack and put the result on top
                        stack[stack_index - 1] = stack[stack_index-1] - stack[stack_index];
                        stack_index -= 1;
                    }else

                    if (token ==4) {//division, take two value from the stack and put the result on top
                        stack[stack_index - 1] = stack[stack_index-1] / stack[stack_index];
                        stack_index -= 1;
                    }else
                    if (token == 5) {//take the absolute value of the top of the stack
                        stack[stack_index] = abs(stack[stack_index]);
                    }else

                    if (token == 6) {//power function
                        stack[stack_index - 1] = pow(stack[stack_index - 1] , stack[stack_index]);
                        stack_index -= 1;
                    }else

					if (token == 7) {//square root
                        stack[stack_index] = sqrt(stack[stack_index]);
					}else

                    if (token == 20) {//map a value from one range to another map(x,inp_l,inp_h,outp_l,outp_h)
                        float x = stack[stack_index-4];
                        float i_l = stack[stack_index-3];
                        float i_h = stack[stack_index-2];
                        float o_l = stack[stack_index-1];
                        float o_h = stack[stack_index];
                        stack[stack_index-4] =o_l +((x-i_l)/(i_h-i_l))*(o_h-o_l);
                        stack_index -= 4;
                    }else

					if (token == 21) {//clip a value to a range				        	
                        float x = stack[stack_index-2];
                        float l = stack[stack_index-1];
                        float h = stack[stack_index-0];
                        stack[stack_index-2]=x<l?l:(x>h?h:x);
                        stack_index -= 2;
					}else

                    if (token == 100) {//output a single color
                        float v = stack[stack_index];
                        colOut = float4(v,v,v,1);
                        stack_index -= 1;
                    }else
                    if (token == 101) {//output rgb
                        colOut = float4(stack[stack_index - 2],stack[stack_index - 1],stack[stack_index],1);
                        stack_index -= 3;
                    }else
                    if (token == 102) {//sample a gradient                    
                        
                    }



                }
                //the final color is the top element of the stack
                fixed4 col = colOut;//float4(stack[stack_index-1],0,0,1);



                // sample the texture
                //fixed4 col = float4(inp.uv.x,values[uint(inp.uv.y/256.0f)%5],0,1);
                //fixed4 col = float4(inp.uv.x,values[test_index%5],0,1);
                // apply fog
                //UNITY_APPLY_FOG(inp.fogCoord, inp.col);
                return col;
            }
            ENDCG
        }
    }
}
