#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
struct appdata
{
    fixed4 vertex : POSITION;
    fixed2 uv : TEXCOORD0;
    fixed3 normal : NORMAL;
    fixed4 vertColor : COLOR;
};
struct v2f
{
    fixed2 uv : TEXCOORD0;
    UNITY_FOG_COORDS(1)
    fixed4 vertex : SV_POSITION;
    LIGHTING_COORDS(3,4)
    fixed3 normal : TEXCOORD5;
    fixed3 wPos : TEXCOORD6;
// #ifdef BASE_PASS
    // SHADOW_COORDS(7)
// #endif
    fixed4 vertColor : COLOR;
};
sampler2D _MainTex;
fixed4 _MainTex_ST;
fixed4 _ColorTint;
fixed _AmbientLight;
fixed _Gloss;
fixed _Fernel;
fixed fernel;
v2f vert (appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.wPos = mul(unity_ObjectToWorld, v.vertex);
// #ifdef BASE_PASS
//     TRANSFER_SHADOW(o)
// #endif    
    o.vertColor = v.vertColor;
    UNITY_TRANSFER_FOG(o,o.vertex);
    TRANSFER_VERTEX_TO_FRAGMENT(o); // lighting
    return o; 
}
fixed4 frag (v2f i) : SV_Target
{
    fixed4 output = tex2D(_MainTex, i.uv);
    UNITY_APPLY_FOG(i.fogCoord, output);

    fixed3 N = normalize(i.normal);
    fixed3 L = normalize(UnityWorldSpaceLightDir(i.wPos));
    fixed3 V = normalize(_WorldSpaceCameraPos - i.wPos);
    fixed3 H =  normalize(L + V); 
    fixed glossExp = exp2(_Gloss * 20);
    fixed attenuation = LIGHT_ATTENUATION(i);

    fixed diffuse = saturate(dot(N, L)) * attenuation;
    fixed specular = saturate(dot(H, N)) * (diffuse > 0);
    specular = pow(specular, glossExp) * (_Gloss * 2);

    #ifdef BASE_PASS
    fixed fernel = 1 - saturate(dot(N, V));
    fernel = pow(fernel, _Fernel * 10);
    fernel = fernel * 0.4f; // add fernel and ambient in base pass only
    #endif
    
    output = i.vertColor + _ColorTint;

    output = output * diffuse;
    output = output + (specular * attenuation);

    #ifdef BASE_PASS
    output = output + (_AmbientLight * i.vertColor * attenuation);
    output = output + fernel;
    #endif

// #ifdef BASE_PASS
//     fixed shadow = SHADOW_ATTENUATION(i);
//     output *= shadow;
// #endif
    return saturate(output);
}