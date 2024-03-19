using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using Valve.VR;
using Valve.VR.Extras;

    public class Yaogan : MonoBehaviour
    {

        // public GameObject mainCameraObject; // VR�������,��Ҫ����
        private LineRenderer line;
        public GameObject rightHand;

  
        public GameObject userHead;
        public SteamVR_Action_Vector2 rightRocker; 
        public SteamVR_Action_Vector2 leftRocker; 
        public SteamVR_Action_Boolean xbRotate;
        public SteamVR_Action_Boolean yaRotate;

        // 4.����ѡȡĿ������
        public GameObject targetObject;
        public SteamVR_Action_Boolean selectManipulateObject; // ѡ����Ҫ����������

        public GameObject[] targetposition;//Ŀ��λ��
        void Start()
        {
            targetObject = null;
            line = GetComponent<LineRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            //����ѡ��һ������
            if(targetObject == null)
                GetManipulateObject();
        if (targetObject != null) { 
            TranslateObjectByRocker();

            CheckTarget();}
            // ��������

           // SampleManipulateData();
        }
        public void CheckTarget()
    {
        foreach (var item in targetposition)
        {
            if((item.transform.position - targetObject.transform.position).magnitude <= 0.02)
            {
                targetObject.transform.position = item.transform.position;
                targetObject.transform.rotation = item.transform.rotation;
                targetObject = null;
            }
        }
    }

        /// <summary>
        /// ͨ���ֱ�����ѡȡĿ������
        /// Ŀǰʹ�����߻�ȡ:GetObjectByRaycast()
        /// </summary>
        void GetManipulateObject()
        {
            if (selectManipulateObject.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                Debug.Log("�����˻�ȡ����İ���...");
                GetObjectByRaycast(rightHand.transform.position, rightHand.transform.forward);
            }
        }

        /// <summary>
        /// ��Ҫ��GetManipulateObject����
        /// �㷨1��ͨ�����߻�ȡ��Ҫ����������
        /// </summary>
        /// <param name="handPosition"></param>
        /// <param name="handForward"></param>
        void GetObjectByRaycast(Vector3 handPosition, Vector3 handForward)
        {
            line.SetPosition(0, handPosition);
            line.SetPosition(1, handPosition + 100f * handForward);
            Ray handRay = new Ray(handPosition, handForward);
            RaycastHit hitInfo;
            if (Physics.Raycast(handRay, out hitInfo, Mathf.Infinity, (1 << 10)))
            {
                targetObject = hitInfo.collider.gameObject;
                while (targetObject.transform.parent != null)
                {
                    targetObject = targetObject.transform.parent.gameObject;
                }
               //ģʽ��� ѡ������ģʽ
                // ʱ�����
              //  if (isSampleData)
               // {
                //    smSamplerScript.manipulateStartTime = Time.time; // ��ȡ��Ϸ��ǰʱ��
                //    smSamplerScript.manipulateDistance = Vector3.Distance(targetObject.transform.position,standardObject.transform.position); // ���������յ�ľ���
              //  }

                Debug.Log("��ȡĿ������:" + targetObject.name);
            }
            else
            {
                // ��ѡ����Ϊ������ǰ����
                Debug.Log("��Ŀ���������ų��Һ���û�꣡");
             
                
            }
        }

   
        /// <summary>
        /// ������ֱ�����λ��ӳ�䣬�������ռ��н���ͬ��
        /// </summary>
        void TranslateObjectByRocker()
        {
            //��ת
            if (xbRotate.GetState(SteamVR_Input_Sources.RightHand) && !yaRotate.GetState(SteamVR_Input_Sources.RightHand))
            {
            Debug.Log("2");
                //��x����תһ���Ƕ�
                //�����Χ�������x�������ת������transform.Rotate(new Vector3(1, 0, 0));
                //�����Χ�������y�������ת������transform.Rotate(new Vector3(0, 1, 0));
                //�����Χ�������z�������ת������transform.Rotate(new Vector3(0, 0, 1));
                targetObject.transform.Rotate(new Vector3(1, 0, 0));
            }

           else if (!xbRotate.GetState(SteamVR_Input_Sources.RightHand) && yaRotate.GetState(SteamVR_Input_Sources.RightHand))
            {
            Debug.Log("1");
            //��x����תһ���Ƕ�
            //�����Χ�������x�������ת������transform.Rotate(new Vector3(1, 0, 0));
            //�����Χ�������y�������ת������transform.Rotate(new Vector3(0, 1, 0));
            //�����Χ�������z�������ת������transform.Rotate(new Vector3(0, 0, 1));
            targetObject.transform.Rotate(new Vector3(0, 1, 0));
            }

           else if (xbRotate.GetState(SteamVR_Input_Sources.RightHand) && yaRotate.GetState(SteamVR_Input_Sources.RightHand))
            {
            //��x����תһ���Ƕ�
            //�����Χ�������x�������ת������transform.Rotate(new Vector3(1, 0, 0));
            //�����Χ�������y�������ת������transform.Rotate(new Vector3(0, 1, 0));
            //�����Χ�������z�������ת������transform.Rotate();
            targetObject.transform.Rotate(new Vector3(0, 0, 1));
            }

            var temp = 0.01f * rightRocker.GetAxis(SteamVR_Input_Sources.RightHand);
            //�ƶ�
            if (temp.magnitude > 0)
            {
                targetObject.transform.position += new Vector3(temp.x,temp.y,0);
            }
            temp = 0.01f * leftRocker.GetAxis(SteamVR_Input_Sources.LeftHand);
            //�ƶ�
            if (temp.magnitude > 0)
            {
                targetObject.transform.position += new Vector3(0, 0, temp.y);
            }
        }
      /*  void SampleManipulateData()
        {
           // if (!isSampleData) { return; }
            if (manipulateStatus.Equals(ManipulateStatus.UNSELECTED)) { return; }
            // �������ӵ�-��Ϊ��ͷ�����ߴ���
            //Vector3 gazeDirect = sRanipalGazeSample.GazeDirectionCombined;
            Vector3 gazeDirect = Camera.main.transform.forward;
            //Vector3 eyePosition = Camera.main.transform.position;
            Vector3 eyePosition = Camera.main.transform.position;
            Ray gazeRay = new Ray(eyePosition, gazeDirect);
            RaycastHit hitInfo;
            if (Physics.Raycast(gazeRay, out hitInfo))
              {
                 smSamplerScript.SampleEyeGazePoint(hitInfo.point);
              }

            // �����ֱ�
           // smSamplerScript.SampleHandlePoint(rightHand.transform.position);
            // ���������ƶ�
            smSamplerScript.SampleObjectPoint(targetObject.transform.position);
        }*/
    }


