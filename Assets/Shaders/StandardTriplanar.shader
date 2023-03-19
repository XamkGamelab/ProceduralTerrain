// Standard shader with triplanar mapping
// https://github.com/keijiro/StandardTriplanar

// Pekka Vilpponen: added Y projection maps and changed blending function

Shader "Standard Triplanar"
{
    Properties
    {
        //general properties
        _Glossiness("Glossiness", Range(0, 1)) = 0.5
        [Gamma] _Metallic("Metallic", Range(0, 1)) = 0
        _BumpScale("Bump Scale", Float) = 1
        _OcclusionStrength("Occlusion Strength", Range(0, 1)) = 1
        _MapScale("Map Scale", Float) = 1        
        _Blend("Map Blending", Range(0, 10)) = 1

        //xz tex properties
        _Color("Color XZ", Color) = (1, 1, 1, 1)
        _MainTex("Albedo XZ", 2D) = "white" {}        
        _BumpMap("Normal XZ", 2D) = "bump" {}
        _OcclusionMap("Occlusion XZ", 2D) = "white" {}

        //y tex properties
        _ColorY("Color Y", Color) = (1, 1, 1, 1)
        _MainTexY("Albedo Y", 2D) = "white" {}        
        _BumpMapY("Normal Y", 2D) = "bump" {}        
        _OcclusionMapY("Occlusion Y", 2D) = "white" {}
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }

            CGPROGRAM

            #pragma surface surf Standard vertex:vert fullforwardshadows addshadow

            #pragma target 3.0

            half _BumpScale;
            half _Glossiness;
            half _Metallic;
            half _OcclusionStrength;
            half _MapScale;
            half _Blend;

            //xz tex
            half4 _Color;
            sampler2D _MainTex;
            sampler2D _BumpMap;            
            sampler2D _OcclusionMap;

            //y tex
            half4 _ColorY;
            sampler2D _MainTexY;                                
            sampler2D _BumpMapY;            
            sampler2D _OcclusionMapY;

            struct Input
            {
                float3 localCoord;
                float3 localNormal;
            };

            void vert(inout appdata_full v, out Input data)
            {
                UNITY_INITIALIZE_OUTPUT(Input, data);
                data.localCoord = v.vertex.xyz;
                data.localNormal = v.normal.xyz;
            }

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                // Blending factor of triplanar mapping                
                float3 bf = pow(abs(IN.localNormal), _Blend);
                bf /= dot(bf, 1.0);

                // Triplanar mapping
                float2 tx = IN.localCoord.yz * _MapScale;
                float2 ty = IN.localCoord.zx * _MapScale;
                float2 tz = IN.localCoord.xy * _MapScale;

                // Base color
                half4 cx = tex2D(_MainTex, tx) * bf.x;
                half4 cy = tex2D(_MainTexY, ty) * bf.y;
                half4 cz = tex2D(_MainTex, tz) * bf.z;
                half4 color = (cx * _Color + cy * _ColorY + cz * _Color);
                o.Albedo = color.rgb;
                o.Alpha = color.a;

                // Normal map
                half4 nx = tex2D(_BumpMap, tx) * bf.x;
                half4 ny = tex2D(_BumpMapY, ty) * bf.y;
                half4 nz = tex2D(_BumpMap, tz) * bf.z;
                o.Normal = UnpackScaleNormal(nx + ny + nz, _BumpScale);
            
                // Occlusion map
                half ox = tex2D(_OcclusionMap, tx).g * bf.x;
                half oy = tex2D(_OcclusionMapY, ty).g * bf.y;
                half oz = tex2D(_OcclusionMap, tz).g * bf.z;
                o.Occlusion = lerp((half4)1, ox + oy + oz, _OcclusionStrength);
            
                // Misc parameters
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;                
            }
            ENDCG
        }
            FallBack "Diffuse"
                CustomEditor "StandardTriplanarInspector"
}