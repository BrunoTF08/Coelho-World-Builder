Shader "Hidden/CoelhoLabs/PlacementGhost"
{
    Properties { _Color ("Color", Color) = (0.2,1,0.5,0.35) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _Color;
            struct Input { float4 vertex : POSITION; };
            struct Output { float4 pos : SV_POSITION; };
            Output vert(Input v) { Output o; o.pos = UnityObjectToClipPos(v.vertex); return o; }
            fixed4 frag(Output i) : SV_Target { return _Color; }
            ENDCG
        }
    }
}
