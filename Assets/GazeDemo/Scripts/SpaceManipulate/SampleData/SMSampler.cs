using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneAware
{
    public struct SHSVoxel
    {
        public Vector3 center; // ��������
        public bool isGazeVisited;
        public int gazeTimes; // ���Ӵ���
        public SHSVoxel(Vector3 voxelCenter, bool isVoxcelVisited, int voxelGazeTimes)
        {
            center = voxelCenter;
            isGazeVisited = isVoxcelVisited;
            gazeTimes = voxelGazeTimes;
        }

    }
    public class SMSampler : MonoBehaviour
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
        private bool isManipulateStart = false;

        // ������Ϣ
        List<Vector3Int> voxelIndexHeatList; // ���ڼ�¼���ʹ������ص��±꣬��ʱ��˳�򣬲����ظ�����ͬһ����
        List<Vector3> handlePositionList; // ���ڼ�¼�ֱ��켣��ÿ������ֻ�ᱻ��¼һ��
        List<Vector3> objectPositionList; // ���ڼ�¼����켣��ÿ������ֻ�ᱻ��¼һ��

        [HideInInspector] public float manipulateStartTime; // Time.time��ȡ��Ϸ������ʱ��
        [HideInInspector] public float manipulateStopTime; // ��������ʱ��
        [HideInInspector] public float manipulateError; // �������=Ŀ��λ��-��������λ��
        [HideInInspector] public float manipulateDistance; // ����ֱ�߾��� = ������� - Ŀ��λ��

        // �ļ���ȡ�����Ϣ
        FileIO fileIO;
        public string fileName; // �ļ���������׺
        public string fileType; // csv\txt��

        // Start is called before the first frame update
        void Start()
        {
            // ��ʼ��ʵ������ռ�
            InitialExperimentBox();
            CreateSampleVoxelArray();
            // ��ʼ����������
            voxelIndexHeatList = new List<Vector3Int>();
            handlePositionList = new List<Vector3>();
            objectPositionList = new List<Vector3>();

            fileIO = GetComponent<FileIO>();
        }

        // Update is called once per frame
        void Update()
        {

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

                        SHSVoxel newVoxel = new SHSVoxel(newVoxelCenter, false,0);

                        tempYList.Add(newVoxel);
                    }
                    tempXList.Add(tempYList);
                }
                sampleVoxelArray.Add(tempXList);
            }
        }

        /// <summary>
        /// ���������ȶȸ���
        /// </summary>
        /// <param name="gazePoint">���ӵ�����</param>
        public void SampleEyeGazePoint(Vector3 gazePoint)
        {
            // ����gazepoint��Ӧ������
            int xVoxelIndex = Mathf.FloorToInt((gazePoint.x - boxOrigin.x) / voxelSideLength - 0.5f) + 1;
            int yVoxelIndex = Mathf.FloorToInt((gazePoint.y - boxOrigin.y) / voxelSideLength - 0.5f) + 1;
            int zVoxelIndex = Mathf.FloorToInt((gazePoint.z - boxOrigin.z) / voxelSideLength - 0.5f) + 1;

            // �ж����ӵ�Խ��
            if (xVoxelIndex >= sampleNumberX || xVoxelIndex < 0 || yVoxelIndex >= sampleNumberY || yVoxelIndex < 0 || zVoxelIndex >= sampleNumberZ || zVoxelIndex < 0)
            {
                Debug.LogError("[WARNING]���ӵ�Խ��");
                return;
            }

            // �������ȶ�+1
            SHSVoxel curVoxel = sampleVoxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex];
            curVoxel.gazeTimes += 1;
            // �жϸ������Ƿ���ʹ������ʹ���������δ���ʹ�������б�

            if (!curVoxel.isGazeVisited)
            {
                voxelIndexHeatList.Add(new Vector3Int(xVoxelIndex, yVoxelIndex, zVoxelIndex));
                curVoxel.isGazeVisited = true;
            }

            sampleVoxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex] = curVoxel;

        }

        /// <summary>
        /// ��¼�ֱ��ƶ��켣
        /// </summary>
        /// <param name="handlePoint">�ֱ�����</param>
        public void SampleHandlePoint(Vector3 handlePoint)
        {
            // ����handlePoint��Ӧ������
            int xVoxelIndex = Mathf.FloorToInt((handlePoint.x - boxOrigin.x) / voxelSideLength - 0.5f) + 1;
            int yVoxelIndex = Mathf.FloorToInt((handlePoint.y - boxOrigin.y) / voxelSideLength - 0.5f) + 1;
            int zVoxelIndex = Mathf.FloorToInt((handlePoint.z - boxOrigin.z) / voxelSideLength - 0.5f) + 1;

            // �ж��ֱ�Խ��
            if (xVoxelIndex >= sampleNumberX || xVoxelIndex < 0 || yVoxelIndex >= sampleNumberY || yVoxelIndex < 0 || zVoxelIndex >= sampleNumberZ || zVoxelIndex < 0)
            {
                Debug.LogError("[WARNING]�ֱ�Խ��");
                return;
            }

            SHSVoxel curVoxel = sampleVoxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex];

            handlePositionList.Add(curVoxel.center);
        }

        /// <summary>
        /// ��¼�����ƶ��켣
        /// </summary>
        /// <param name="objectPoint">��������</param>
        public void SampleObjectPoint(Vector3 objectPoint)
        {
            // ����objectPoint��Ӧ������
            int xVoxelIndex = Mathf.FloorToInt((objectPoint.x - boxOrigin.x) / voxelSideLength - 0.5f) + 1;
            int yVoxelIndex = Mathf.FloorToInt((objectPoint.y - boxOrigin.y) / voxelSideLength - 0.5f) + 1;
            int zVoxelIndex = Mathf.FloorToInt((objectPoint.z - boxOrigin.z) / voxelSideLength - 0.5f) + 1;

            // �ж�����Խ��
            if (xVoxelIndex >= sampleNumberX || xVoxelIndex < 0 || yVoxelIndex >= sampleNumberY || yVoxelIndex < 0 || zVoxelIndex >= sampleNumberZ || zVoxelIndex < 0)
            {
                Debug.LogError("[WARNING]����Խ��");
                return;
            }

            SHSVoxel curVoxel = sampleVoxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex];

            objectPositionList.Add(curVoxel.center);
        }

        public void CalculateManipulateError(Vector3 objectPosition,Vector3 targetPosition)
        {
            manipulateError = Vector3.Distance(objectPosition, targetPosition);
        }

        public void WriteExperimentData2File()
        {
            WriteGaze2File();
            WriteHandlePosition2File();
            WriteObjectPosition2File();
            WriteManipulateResult2File();

            Debug.Log("����ʵ������д�����");
        }

        public void WriteGaze2File()
        {
            List<string> dataList = new List<string>();
            string filePath = "./Assets/GazeDemo/Output/GazeHeat/";
            string fullFilePath = filePath + fileName + "_gazeHeat" + "." + fileType;

            dataList.Add("x,y,z,gazetimes");

            foreach(Vector3Int voxelIndex in voxelIndexHeatList)
            {
                SHSVoxel curVoxel = sampleVoxelArray[voxelIndex.x][voxelIndex.y][voxelIndex.z];
                dataList.Add(string.Format("{0:F3},{1:F3},{2:F3},{3:D1}", curVoxel.center.x, curVoxel.center.y, curVoxel.center.z, curVoxel.gazeTimes));
            }

            fileIO.Writef(dataList, true, fullFilePath);

            Debug.Log("�������� д�����");
        }

        public void WriteHandlePosition2File()
        {
            List<string> dataList = new List<string>();
            string filePath = "./Assets/GazeDemo/Output/Handle/";
            string fullFilePath = filePath + fileName + "_handle" + "." + fileType;

            dataList.Add("x,y,z");

            foreach (Vector3 pos in handlePositionList)
            {
                dataList.Add(string.Format("{0:F3},{1:F3},{2:F3}", pos.x, pos.y,pos.z));
            }

            fileIO.Writef(dataList, true, fullFilePath);
            Debug.Log("�ֱ����� д�����");
        }

        public void WriteObjectPosition2File()
        {
            List<string> dataList = new List<string>();
            string filePath = "./Assets/GazeDemo/Output/MObject/";
            string fullFilePath = filePath + fileName + "_object" + "." + fileType;

            dataList.Add("x,y,z");

            foreach (Vector3 pos in objectPositionList)
            {
                dataList.Add(string.Format("{0:F3},{1:F3},{2:F3}", pos.x, pos.y, pos.z));
            }

            fileIO.Writef(dataList, true, fullFilePath);
            Debug.Log("������������ д�����");
        }

        /// <summary>
        /// ��¼���������Ϣ��Ŀǰ������
        /// 1.��׼��������
        /// 2.����ʱ��
        /// 3.�������
        /// </summary>
        public void WriteManipulateResult2File()
        {
            List<string> dataList = new List<string>();
            string filePath = "./Assets/GazeDemo/Output/MResult/";
            string fullFilePath = filePath + fileName + "_result" + "." + fileType;

            dataList.Add(string.Format("{0:F3},{1:F3},{2:F3}",manipulateDistance ,manipulateStopTime - manipulateStartTime, manipulateError));

            fileIO.Writef(dataList, true, fullFilePath);

            Debug.Log("����������� д�����");
        }
    }
}


