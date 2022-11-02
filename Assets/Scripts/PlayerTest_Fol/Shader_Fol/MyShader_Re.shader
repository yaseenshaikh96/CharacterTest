Shader "Unlit/MyShader_Re"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorTint("Color Tint", Color) = (1, 1, 1, 1)
        _Gloss("Gloss", Range(0, 1)) = 0.5
        _Fernel("Fernel", Range(0, 1)) = 0.5
        _AmbientLight("Ambient Light", Range(0, 1)) = 0.25

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            // #pragma multi_compile_fwdbase // nolightmap nodirlightmap nodynlightmap

            #define BASE_PASS

            #include "MyShader_Include.cginc"
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            Blend one one
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // #pragma multi_compile_fwdadd // nolightmap nodirlightmap nodynlightmap
            
            // #pragma multi_compile_fwdadd_fullshadows
            // #pragma multi_compile_fog

            #include "MyShader_Include.cginc"
            ENDCG 
        }

        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f { 
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }

        // UsePass "Legacy Shaders/VertexLit"
    }
}