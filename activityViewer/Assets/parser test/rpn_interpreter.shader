Shader "Unlit/ron_interpreter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform float values[128];
            uniform float num_values;
            uniform float tokens[256];
            uniform int num_tokens = 0;
            
            int test_index = 0;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f inp) : SV_Target
            {


                float stack[64];
                stack[0] = 0;
                int stack_index = 0;
                int value_index = 0;
                for (int i = 0; i < num_tokens; i++) {
                    int token = floor(tokens[i]+0.5);//TODO chekc if necesseary
                    if (token == -1) {//take the next value and put it on the stack
                        stack[stack_index] = values[value_index];
                        stack_index++;
                        value_index++;
                    }
                    if (token == -2) {//put the uv.x on the stack
                        stack[stack_index] = inp.uv.x;
                        stack_index++;
                    }
                    if (token == -3) {
                        stack[stack_index] = inp.uv.y;
                        stack_index++;
                    }
                    if (token == 1) {//addition, take two value from the stack and put the result on top
                        stack[stack_index - 2] = stack[stack_index-2] + stack[stack_index-1];
                        stack_index -= 1;
                    }

                    if (token == 2) {//multiplication, take two value from the stack and put the result on top
                        stack[stack_index - 2] = stack[stack_index-2] * stack[stack_index-1];
                        stack_index -= 1;
                    }
                    if (token == 3) {//substraction, take two value from the stack and put the result on top
                        stack[stack_index - 2] = stack[stack_index-2] - stack[stack_index-1];
                        stack_index -= 1;
                    }

                    if (token ==4) {//division, take two value from the stack and put the result on top
                        stack[stack_index - 2] = stack[stack_index-2] / stack[stack_index-1];
                        stack_index -= 1;
                    }
                    if (token == 5) {//take the absolute value of the top of the stack
                        stack[stack_index - 1] = abs(stack[stack_index - 1]);
                    }

                    if (token == 6) {
                        stack[stack_index - 1] = pow(stack[stack_index - 2] , stack[stack_index - 1]);
                        stack_index -= 1;
                    }

                }

                fixed4 col = float4(stack[stack_index-1],0,0,1);

                // sample the texture
                //fixed4 col = float4(inp.uv.x,values[uint(inp.uv.y/256.0f)%5],0,1);
                //fixed4 col = float4(inp.uv.x,values[test_index%5],0,1);
                // apply fog
                UNITY_APPLY_FOG(inp.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
