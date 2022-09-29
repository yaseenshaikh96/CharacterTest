Shader "Unlit/VertexColor_Shader"
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

            #define BASE_PASS

            #include "MyShaderVertColor.cginc"
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            Blend one one
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd

            #include "MyShaderVertColor.cginc"
            ENDCG
        }
    }
}