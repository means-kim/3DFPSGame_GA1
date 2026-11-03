// 로그인 배경용 아지랑이 셰이더.
// UI/Default와 같은 구조(스텐실·RectMask2D 클립)를 유지하면서, 노이즈 텍스처를 흘려 배경 UV를 살짝 비튼다.
// 해 주변과 화면 아래 도로 띠에서만 강하게 일렁이도록 마스크를 둔다.
Shader "UI/HeatHaze"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Heat Haze)]
        _HazeTex ("Haze Noise (RG)", 2D) = "gray" {}
        _Strength ("Strength", Range(0, 0.02)) = 0.004
        _Speed ("Speed", Float) = 0.06
        _NoiseScale ("Noise Scale", Float) = 3
        _SunPos ("Sun UV", Vector) = (0.571, 0.654, 0, 0)
        _SunRadius ("Sun Radius", Float) = 0.22
        _Aspect ("Image Aspect", Float) = 1.767
        _RoadHeight ("Road Height (UV)", Float) = 0.35
        _RoadStrength ("Road Strength", Range(0, 1)) = 0.6

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _HazeTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _Strength;
            float _Speed;
            float _NoiseScale;
            float4 _SunPos;
            float _SunRadius;
            float _Aspect;
            float _RoadHeight;
            float _RoadStrength;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float time = _Time.y * _Speed;

                // 서로 다른 방향·크기로 흐르는 노이즈 두 장을 섞어 불규칙하게 일렁이게 한다(위로 피어오르는 느낌).
                float2 noiseA = tex2D(_HazeTex, uv * _NoiseScale + float2(0.0, -time)).rg;
                float2 noiseB = tex2D(_HazeTex, uv * _NoiseScale * 1.7 + float2(time * 0.4, -time * 1.3)).rg;
                float2 offset = noiseA + noiseB - 1.0;

                // 해 주변(원형)과 아래쪽 도로 띠에서만 왜곡한다.
                float2 toSun = (uv - _SunPos.xy) * float2(_Aspect, 1.0);
                float sunMask = saturate(1.0 - length(toSun) / _SunRadius);
                float roadMask = saturate(1.0 - uv.y / _RoadHeight) * _RoadStrength;
                float mask = saturate(sunMask * sunMask + roadMask);

                uv += offset * _Strength * mask;

                half4 color = (tex2D(_MainTex, uv) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                color.rgb *= color.a;
                return color;
            }
        ENDCG
        }
    }
}
