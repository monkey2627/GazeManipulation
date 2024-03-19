using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;


namespace UserStudy
{
    public struct SHSVoxel
    {
        public Vector3 center;
        public bool isVisited;

        public SHSVoxel(Vector3 voxelCenter,bool isVoxcelVisited)
        {
            center = voxelCenter;
            isVisited = isVoxcelVisited;
        }

    }
    public class SHSDemoController : MonoBehaviour
    {
        // ��ռ�л�����Ϣ
        public GameObject ExperimentBox; // ʵ���ռ����
        [HideInInspector] public Bounds boxBounds;
        [HideInInspector] public float boxLengthX;
        [HideInInspector] public float boxLengthY;
        [HideInInspector] public float boxLengthZ;
        [HideInInspector] public Vector3 boxOrigin;

        // �ռ����������Ϣ
        public float voxelSideLength;
        // �ռ������������������boxLength����
        [HideInInspector] public int sampleNumberX;
        [HideInInspector] public int sampleNumberY;
        [HideInInspector] public int sampleNumberZ;
        [HideInInspector] public List<List<List<SHSVoxel>>> sampleVoxelArray; // ���ڴ�����ؽṹ��


        // �ֱ�������Ϣ
        public GameObject rightHand; // �����ֱ�
        public SteamVR_Action_Boolean selectUserShoulder; // ѡ���û����λ��
        private bool isUserShoulderSelected = false; // �û����ɼ�״��

        public SteamVR_Action_Boolean startSampleHandle; // ��ʼ�ɼ��ֱ�����
        private bool isHandleSampleStart = false;

        // �ļ���д��Ϣ
        List<Vector3> positionList2File;
        FileIO fileIO;
        public string fileName;

        // ����

        

        // Start is called before the first frame update
        void Start()
        {
            InitialExperimentBox();
            CreateSampleVoxelArray();

            // ������ʼ��
            positionList2File = new List<Vector3>();
            fileIO = GetComponent<FileIO>();
        }

        // Update is called once per frame
        void Update()
        {
            SampleShoulderPosition();
            SampleHandleMovement();
        }

        private void InitialExperimentBox()
        {
            // ʵ�����Ҫ����ײ��
            boxBounds = ExperimentBox.GetComponent<BoxCollider>().bounds;
            boxLengthX = boxBounds.size.x;
            boxLengthY = boxBounds.size.y;
            boxLengthZ = boxBounds.size.z;
            boxOrigin = boxBounds.min;
        }

        /// <summary>
        /// ��ʼ�����ؽṹ�����У���������ĵ�
        /// </summary>
        private void CreateSampleVoxelArray()
        {
            // �����������ϲ���������������ȡ��
            sampleNumberX = Mathf.CeilToInt(boxLengthX / voxelSideLength);
            sampleNumberY = Mathf.CeilToInt(boxLengthY / voxelSideLength);
            sampleNumberZ = Mathf.CeilToInt(boxLengthZ / voxelSideLength);

            sampleVoxelArray = new List<List<List<SHSVoxel>>>();

            for (int ix = 0; ix < sampleNumberX; ix++)
            {
                List<List<SHSVoxel>> tempXList = new List<List<SHSVoxel>>();
                for (int iy = 0; iy < sampleNumberY; iy++)
                {
                    List<SHSVoxel> tempYList = new List<SHSVoxel>();
                    for (int iz = 0; iz < sampleNumberZ; iz++)
                    {
                        
                        Vector3 newVoxelCenter = boxOrigin +
                            ix * Vector3.right * (voxelSideLength) +
                            iy * Vector3.up * (voxelSideLength) +
                            iz * Vector3.forward * (voxelSideLength);
                        
                        SHSVoxel newVoxel = new SHSVoxel(newVoxelCenter,false);

                        tempYList.Add(newVoxel);
                    }
                    tempXList.Add(tempYList);
                }
                sampleVoxelArray.Add(tempXList);
            }
        }

        /// <summary>
        /// ��ȡ�����������
        /// </summary>
        void SampleShoulderPosition()
        {
            if (selectUserShoulder.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                if (!isUserShoulderSelected)
                {
                    // ��ȡ��ʱ��������������Ϊ���λ�ã�д��洢�б�
                    positionList2File.Add(rightHand.transform.position);

                    isUserShoulderSelected = true;
                    Debug.Log("�ɹ���ȡ�������");
                }
                else
                {
                    Debug.Log("[�ظ�����]��������ѻ�ȡ");
                }
            }
        }

        /// <summary>
        /// �����ֱ������Ӧ���������
        /// </summary>
        private void HandlePosition2Voxel(Vector3 handlePosition)
        {
            int xVoxelIndex = Mathf.FloorToInt((handlePosition.x - boxOrigin.x) / voxelSideLength - 0.5f) + 1;
            int yVoxelIndex = Mathf.FloorToInt((handlePosition.y - boxOrigin.y) / voxelSideLength - 0.5f) + 1;
            int zVoxelIndex = Mathf.FloorToInt((handlePosition.z - boxOrigin.z) / voxelSideLength - 0.5f) + 1;

            // �ж��ֱ�Խ��
            if (xVoxelIndex >= sampleNumberX || xVoxelIndex < 0 || yVoxelIndex >= sampleNumberY || yVoxelIndex < 0 || zVoxelIndex >= sampleNumberZ || zVoxelIndex < 0)
            {
                Debug.LogError("[ERROR]�ֱ�Խ��");
                return;
            }

            SHSVoxel curVoxel = sampleVoxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex];

            if (!curVoxel.isVisited)
            {
                //����������������ļ���
                positionList2File.Add(curVoxel.center);

                // �����ر��Ϊ���ʹ�
                curVoxel.isVisited = true;

                // ���������б�
                sampleVoxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex] = curVoxel;
            }
        }

        private void SampleHandleMovement()
        {
            if (startSampleHandle.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                isHandleSampleStart = !isHandleSampleStart;
                if (isHandleSampleStart)
                {
                    Debug.Log("��ʼ�����ֱ�����...");
                }
                else
                {
                    Debug.Log("���������ֱ�����,������д���ļ�...");
                    WritePosition2File();


                }
            }

            if (isHandleSampleStart)
            {
                HandlePosition2Voxel(rightHand.transform.position);

            }
        }

        private void WritePosition2File()
        {
            List<string> dataList = new List<string>();
            string filePath = "./Assets/UserStudy/Output/";
            

            // ��Vector�б�תΪa,b,c��string
            foreach(Vector3 pos in positionList2File)
            {
                dataList.Add(string.Format("{0:F3},{1:F3},{2:F3}", pos.x, pos.y, pos.z));
            }

            
            fileIO.Writef(dataList, true, filePath + fileName);

        }
        

    }
}
