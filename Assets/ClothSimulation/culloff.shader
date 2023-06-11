Shader "Custom/NoCull" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass {
                Cull Off // 这一行就是取消背面剔除的设置

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

        // 声明要在vertex和fragment shader中使用的变量
        struct appdata {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        sampler2D _MainTex;

        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
            // 把纹理样本乘以2D贴图，并输出
            fixed4 col = tex2D(_MainTex, i.uv);
            return col;
        }
        ENDCG
    }
    }
}
