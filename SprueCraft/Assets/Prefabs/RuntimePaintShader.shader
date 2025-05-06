Shader "Custom/RuntimePaintShader" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _PaintTex ("Paint Texture", 2D) = "black" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "UnityInstancing.cginc"

            sampler2D _MainTex;
            sampler2D _PaintTex;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 baseColor = tex2D(_MainTex, i.uv);
                fixed4 paintColor = tex2D(_PaintTex, i.uv);
                return baseColor * (1 - paintColor.a) + paintColor;
            }
            ENDCG
        }
    }
}