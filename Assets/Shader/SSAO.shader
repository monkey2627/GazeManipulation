Shader "Custom/SSAO"
{
    // ---------------------------�����ԡ�---------------------------
    Properties
    {
        // _MainTex ("Texture", 2D) = "white" {}
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        _Color ("Color", Color) = (0.5, 1, 1, 1)
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _Shininess ("Shininess", Float) = 1000.0
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _AmbientColor ("Ambient Color", Color) = (0.2,0.2,0.2,1)
        _Glossiness ("Glossiness", Float) = 0.2
        _SecondaryLightColor ("Secondary Light Color", Color) = (0.1,0.1,0.1,0.3)
        _SecondaryLightDir ("Secondary Light Direction", Vector) = (-1,-1,-1)

        //_Hitpoint ("Hitpoint", Vector) = (0, 0, 0)
        //_Hitpoint8 ("Hitpoint8", Vector) = (0, 0, 0)
        //_Hitpoint5 ("Hitpoint5", Vector) = (0, 0, 0)
        //_camPos ("Camera Position", Vector) = (0, 0, 0)
        //_camPos1 ("Camera Position 1", Vector) = (0, 0, 0)
        //_camPos2 ("Camera Position 2", Vector) = (0, 0, 0)
        //_Forward ("Forward", Vector) = (0, 0, 0)
        //_Up ("Up", Vector) = (0, 0, 0)
        //_Side ("Side", Vector) = (0, 0, 0)
        //_Cut ("Cut", Vector) = (0.1, 0.17, 0)
    }
    // ---------------------------������ɫ����---------------------------
    SubShader
    {
        // No culling or depth
        // Cull Off ZWrite Off ZTest Always
        Tags { "RenderType"="Opaque" }
        LOD 100
        // ---------------------------����Ⱦͨ����---------------------------
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0
            //#pragma surface surf Standard

            #include "UnityCG.cginc"
            //��������ṹ��
            struct VertexInput
            {
                float4 vertex : POSITION;   // ����λ��
                float3 Normal : NORMAL;     // ����
                float2 uv : TEXCOORD0;    // ��������
                // fixed4 color : COLOR;

            };
            // ��������ṹ��
            struct VertexOutput
            {
                float4 vertex : SV_POSITION;   // ����λ��
                // float4 worldPos : POSITION;
                float2 uv : TEXCOORD0;   // ��������
                float3 Normal : TEXCOORD1;  // ����
                float3 aWorldPos : TEXCOORD2;
                // fixed4 color : COLOR;
            };

            // ��������
            sampler2D _MainTex;
            sampler2D _NormalMap;
            float4 _MainTex_ST;

            // �ⲿ��������
            float3 _Hitpoint;
            float3 _Hitpoint8;
            float3 _Hitpoint5;
            float3 _camPos;
            float3 _camPos1;
            float3 _camPos2;
            float3 _Forward;
            float3 _Up;
            float3 _Side;
            float2 _Cut;  // ����Ȧ

            float _objectID;

            // ��������
            float2 _Pos;
            float _ZoomFactor;
            float _EdgeFactor;
            float _Size;
            float _PointSize;

            half4 _Color;
            half _Metallic;
            half _Smoothness;
            float4 _SpecularColor;
            float _Shininess;
            float4 _AmbientColor;
            float _Glossiness;
            float4 _SecondaryLightColor;
            float3 _SecondaryLightDir;

            float GetTanCut(float3 pos)
            {
	            float cosCut= dot(normalize(pos - _camPos), _Forward);
	            float tanCut=sqrt(1.0f/cosCut/cosCut-1.0f);
	            return tanCut;
            }

            float3 GetLineInscConePos(float3 s, float3 e, float3 foot, float tanCut)
            {

	            float r=length(foot - _camPos)*tanCut;
	            float3 v1=e-s;
	            float3 v2=s-foot;
	            float a=dot(v1,v1);
	            float b=dot(v1,v2)*2;
	            float c=dot(v2,v2)-r*r;
	            float deta=sqrt(b*b-4*a*c);
	            float k=(-b+deta)/2.0f/a;
	            return s+k*(e-s);
            }

            // ���㴦����
            float4 ProcessPoint(float3 P)
            {
                float3 P_ = P;
                
                float3 pO = _Hitpoint;
	            float3 pOB = _Hitpoint8;
	            float3 pOC = _Hitpoint5;
	            float3 v1 = _camPos1;  // ͨ������ó�
	            float3 v2 = _camPos2;  // v2�ֶ�����λ��
	            float3 forward = _Forward;
	            float3 up = _Up;
	            float3 side = _Side;
	            float3 camPos = _camPos;
	            float2 cut = _Cut;

                if (true)
                {
                    float len_VB = dot(pOB - camPos, forward);
		            float len_VC = dot(pOC - camPos, forward);
		            float len_VPf = dot(P - camPos, forward);
		            float len_V2Pf = dot(P - v2, forward);
		            float len_V1Pf = dot(P - v1, forward);

                    float3 Ptb=P;
                    if(len_VPf > len_VB)        // ��λ��Bƽ����
		            {
			            float3 B = P + (len_VPf - len_VB)/len_V2Pf * (v2 - P);
			            float len_V1Bf = dot(B-v1,forward);
			            float3 C = B + (len_VB - len_VC)/len_V1Bf * (v1 - B);

			            float d = length(B - P);
			            d+=length(B - C);
			            float d2 = length(C - camPos);
			            float d3 = length(P - camPos);
			            Ptb = C + normalize(C-camPos) *d;

			            //Ptb = Ptb + dot(Ptb - camPos,up) * up * ((d+d2)/d3 - 1.0f);
		            }
                    else if(len_VPf > len_VC)         // ��λ��CBƽ���м�
		            {
			            float3 B = P + (len_VPf - len_VB)/len_V1Pf * (v1 - P);
			            float3 C = P + (len_VPf - len_VC)/len_V1Pf * (v1 - P);
				
			            float d = length(C - P);
			            float d2 = length(C - camPos);
			            float d3 = length(P - camPos);
			            Ptb = C + normalize(C - camPos)*d;

			            //Ptb = Ptb + dot(Ptb - camPos, up) * up * ((d+d2)/d3 - 1.0f);
		            }

                    float tanCut_P= GetTanCut(P);
		            float tanCut_Ptb= GetTanCut(Ptb);

                    if(tanCut_Ptb<cut.x)
                    { 
                        // λ����Ȧ֮��
			            P_=Ptb;
		            }
                    else if(tanCut_P<cut.y)  // ��Ȧ����Ȧ֮��                 
                    {
                        
                        if(len_VPf > len_VB)       // ��λ��Bƽ����
			            {
				            float len_V2POf=dot(pO-v2, forward);
				            float len_V2Bf=dot(pOB-v2, forward);
				            float len_V1Bf=dot(pOB-v1, forward);
				            float len_V1Cf=dot(pOC-v1, forward);
				            float3 Af=v2+len_V2Pf/len_V2POf*(pO-v2);
				            float3 Bf=v2+len_V2Bf/len_V2POf*(pO-v2);
				            float3 pOA=camPos+len_VPf/len_VC*(pOC-camPos);
				
				            float3 PB=v2+len_V2Bf/len_V2Pf*(P-v2);
				            float3 PC=v1+len_V1Cf/len_V1Bf*(PB-v1);
				
				            float3 C=pOC + len_VC * cut.x * normalize(PC-pOC);
				            float3 B=v1+len_V1Bf/len_V1Cf*(C-v1);
				            float3 A=v2+len_V2Pf/len_V2Bf*(B-v2);
				
				            float3 A_=GetLineInscConePos(Af, P, pOA, cut.y);
				            float3 B_=camPos+len_VB/len_VPf*(A_-camPos);
				            float3 C_=camPos+len_VC/len_VPf*(A_-camPos);

				            float k=length(P-A)/length(A_-A);
				            float3 Ptmp=B+k*(B_-B);
				            float3 Ptmp2=C+k*(C_-C);

				            float d = length(P-Ptmp) + length(Ptmp-Ptmp2);
				            P_ = Ptmp2 + normalize(Ptmp2-camPos) *d;
			            }
                        else if(len_VPf > len_VC)         // ��λ��BCƽ��֮�� (0.8, 0.7, 0.6, 0.5)
			            {
				            float len_V1Cf=dot(pOC-v1, forward);
				            float3 Af=v1+len_V1Pf/len_V1Cf*(pOC-v1);
				            float3 pOA=camPos+len_VPf/len_VC*(pOC-camPos);

				            float3 PC=v1+len_V1Cf/len_V1Pf*(P-v1);

				            float3 C=pOC + len_VC * cut.x * normalize(PC-pOC);
				            float3 A=v1+len_V1Pf/len_V1Cf*(C-v1);

				            float3 A_=GetLineInscConePos(Af, P, pOA, cut.y);
				            float3 C_=camPos+len_VC/len_VPf*(A_-camPos);

				            float k=length(P-A)/length(A_-A);
				            float3 Ptmp=C+k*(C_-C);

				            float d = length(P-Ptmp);
				            P_ = Ptmp + normalize(Ptmp-camPos) * d;
			            }
                        else  // ��λ��Cƽ����
                        {
				            P_=P;
			            }
                    }
                    else  // λ����Ȧ֮��
                    {
                        P_=P;
                    }
                }
                return float4(P_, 1.0);
            }

            float4 ProcessPoint1(float3 P)
            {
                P.x = P.x-0.2;
                
                return float4(P,1.0);
            }

            // ---------------------------��������ɫ����---------------------------
            VertexOutput vert (VertexInput v)
            {
               
                // �������ģ�Ϳռ�ת��������ռ�
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;               

                // ������ռ��д�����
                float4 processedPos = ProcessPoint(worldPos);

                // ��������Ķ��������ռ�ת���������ռ�
                float4 objectPos = mul(unity_WorldToObject, processedPos);
                v.vertex = mul(unity_WorldToObject, processedPos);

                // ��������ṹ
                VertexOutput o;
                o.vertex = UnityObjectToClipPos(objectPos);
                o.uv = v.uv;
                o.Normal = UnityObjectToWorldNormal(v.Normal);
                o.aWorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                return o;
            }

            // ---------------------------��ƬԪ��ɫ����---------------------------
            half4 frag (VertexOutput i) : SV_Target
            {
                float3 normal = normalize(i.Normal);
                float3 lightDir = normalize(UnityWorldSpaceLightDir(i.vertex));
                float3 secondaryLightDir = normalize(_SecondaryLightDir);
                float primaryDiff = max(dot(normal, lightDir), 0.0);

                // ������1
                fixed4 primaryDiffuse = _Color * primaryDiff;

                // �ڶ�����Դ��������
                float secondaryDiff = max(dot(normal, secondaryLightDir), 0.0);
                fixed4 secondaryDiffuse = _SecondaryLightColor * secondaryDiff;

                // ������
                fixed4 ambient = _AmbientColor;

                // �ӽǷ���
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.aWorldPos);

                // �߹ⷴ��
                float3 halfDir = normalize(lightDir + viewDir);
                float specAngle = max(dot(normal, halfDir), 0.0);
                float specIntensity = pow(specAngle, 0.8 * 128.0);  // pow(specAngle, (1.0 - _Glossiness) * 128.0)
                fixed4 specular = _SpecularColor * specIntensity;

                // ������ɫ = ������ + ������
                //fixed4 finalColor = diffuse + ambient;
                //return finalColor;

                // ������ɫ = ������ + ������ + �߹ⷴ��
                //fixed4 finalColor = diffuse + ambient + specular;
                //return finalColor;

                // ������ɫ = ������1 + ������2 + ������ + �߹ⷴ��1
                // fixed4 finalColor = primaryDiffuse + secondaryDiffuse + ambient + specular;
                fixed4 finalColor = primaryDiffuse  + ambient + specular;
                return finalColor;

                // return _Color;
            }
            ENDCG
        }
    }

}
