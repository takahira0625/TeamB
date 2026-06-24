// スプライトを「白へ寄せる」フラッシュ用シェーダー。
// 元のテクスチャ色を _FlashColor へ _FlashAmount(0〜1) の割合で lerp(混ぜる)するだけ。
// 0 = 元のまま / 1 = 完全に _FlashColor。0.5 なら半分白く、スプライトの形は見える。
// Sprites/Default をベースにしているので、SpriteRenderer がスプライトのテクスチャを自動で渡してくれる。
Shader "Custom/SpriteFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FlashColor ("Flash Color", Color) = (1,1,1,1)
        _FlashAmount ("Flash Amount", Range(0,1)) = 0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _FlashColor;
            float _FlashAmount;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                // 元の色を _FlashColor へ _FlashAmount の割合だけ寄せる(RGBのみ)。
                // アルファ(形)はそのまま残すので、透明な所は透明のまま=スプライトの形が見える。
                c.rgb = lerp(c.rgb, _FlashColor.rgb, _FlashAmount);
                // 乗算済みアルファ(Blend One OneMinusSrcAlpha)に合わせてRGBにアルファを掛ける
                c.rgb *= c.a;
                return c;
            }
        ENDCG
        }
    }
}
