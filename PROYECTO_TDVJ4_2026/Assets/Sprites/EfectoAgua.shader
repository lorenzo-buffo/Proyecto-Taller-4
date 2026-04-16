Shader "Custom/EfectoAgua"
{
  Properties
    {
        _MainTex ("Textura Agua", 2D) = "white" {}
        _Cutoff ("Limite de Fusion", Range(0, 1)) = 0.5
        _ColorAgua ("Color del Liquido", Color) = (0, 0.5, 1, 1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _Cutoff;
            float4 _ColorAgua;

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Lee la textura de la cámara secreta
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Si la transparencia es menor al límite, lo hace invisible.
                // Aquí es donde lo borroso se vuelve sólido.
                clip(col.a - _Cutoff);
                
                // Le aplica el color azul
                return _ColorAgua;
            }
            ENDCG
        }
    }
}