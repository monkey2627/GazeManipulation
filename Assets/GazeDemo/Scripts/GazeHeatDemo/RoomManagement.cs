using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using Valve.VR;
using Valve.VR.Extras;

namespace GazeHeat
{
    public class RoomManagement : MonoBehaviour
    {
        // ���������Ϣ
        public GameObject roomObject;
        [HideInInspector] public Bounds roomBounds;
        [HideInInspector] public float roomLengthX;
        [HideInInspector] public float roomLengthY;
        [HideInInspector] public float roomLengthZ;
        [HideInInspector] public Vector3 roomOrigin;

        // �������ػ������Ϣ
        public float voxelSideLength;
        public GameObject voxelPrefab;
        public float stableLifeTime;
        [HideInInspector] public GameObject voxelParentObject;
        [HideInInspector] public int voxelNumberX;
        [HideInInspector] public int voxelNumberY;
        [HideInInspector] public int voxelNumberZ;
        [HideInInspector] public List<List<List<GameObject>>> voxelArray;
        

        // �۲�������Ϣ
        public GameObject GazeRaySample;
        private SRanipal_GazeRaySample_v2 sRanipalGazeSample;

        // �ֱ�ʻ��ű�
        private HandleInput handleInputScript;

        // Start is called before the first frame update
        void Start()
        {
            // ��ȡ���ӵ�ű�
            sRanipalGazeSample = GazeRaySample.GetComponent<SRanipal_GazeRaySample_v2>();
            // ��ȡ�ֱ�����ű�
            handleInputScript = GetComponent<HandleInput>(); // ������ͬһ��������

            InitialRoomBasicData();

            CreateVoxelArray();

        }

        // Update is called once per frame
        void Update()
        {
           
        }

        private void FixedUpdate()
        {
            if (handleInputScript.isGazeVoxelization)
            {
                UpdateVoxelByGazeDirect(sRanipalGazeSample.GazeDirectionCombined);
            }
            
        }

        /// <summary>
        /// ��ʼ�����������Ϣ
        /// </summary>
        void InitialRoomBasicData()
        {
            roomBounds = roomObject.GetComponent<BoxCollider>().bounds;
            roomLengthX = roomBounds.size.x;
            roomLengthY = roomBounds.size.y;
            roomLengthZ = roomBounds.size.z;
            roomOrigin = roomBounds.min;
        }

        /// <summary>
        /// ����һ���յ���������
        /// </summary>
        private void CreateVoxelArray()
        {
            // ��������������������������ȡ��
            voxelNumberX = Mathf.CeilToInt(roomLengthX / voxelSideLength);
            voxelNumberY = Mathf.CeilToInt(roomLengthY / voxelSideLength);
            voxelNumberZ = Mathf.CeilToInt(roomLengthZ / voxelSideLength);

            // �����������и�����
            voxelParentObject = new GameObject("�������и�����");

            // �����յ���������
            voxelArray = new List<List<List<GameObject>>>();
            for (int ix = 0; ix < voxelNumberX; ix++)
            {
                List<List<GameObject>> tempXList = new List<List<GameObject>>();
                for (int iy = 0; iy < voxelNumberY; iy++)
                {
                    List<GameObject> tempYList = new List<GameObject>();
                    for (int iz = 0; iz < voxelNumberZ; iz++)
                    {
                        tempYList.Add(null);
                    }
                    tempXList.Add(tempYList);
                }
                voxelArray.Add(tempXList);
            }
        }

        private void UpdateVoxelByGazeDirect(Vector3 gazeDirect)
        {
            // ��ȡ��ʼ��
            //Vector3 eyePosition = Camera.main.transform.position - Camera.main.transform.up * 0.05f;
            //Vector3 eyePosition = gazeOrigin;
            Vector3 eyePosition = Camera.main.transform.position;
            // ��ȡ���ӵ��볡������ײ��
            Ray gazeRay = new Ray(eyePosition, gazeDirect);
            RaycastHit hitInfo;
            if (Physics.Raycast(gazeRay, out hitInfo))
            {
                Vector3 gazePoint = hitInfo.point;
                int xVoxelIndex = Mathf.FloorToInt((gazePoint.x - roomOrigin.x) / voxelSideLength - 0.5f) + 1;
                int yVoxelIndex = Mathf.FloorToInt((gazePoint.y - roomOrigin.y) / voxelSideLength - 0.5f) + 1;
                int zVoxelIndex = Mathf.FloorToInt((gazePoint.z - roomOrigin.z) / voxelSideLength - 0.5f) + 1;

                GameObject targetVoxelObject = voxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex];
                if (targetVoxelObject == null)
                {
                    targetVoxelObject = Instantiate(voxelPrefab, voxelParentObject.transform);
                    targetVoxelObject.name = string.Format("voxel_{0}{1}{2}", xVoxelIndex,yVoxelIndex,zVoxelIndex);
                    targetVoxelObject.SetActive(false);
                    targetVoxelObject.isStatic = true;
                    targetVoxelObject.transform.localScale = new Vector3(voxelSideLength, voxelSideLength, voxelSideLength);
                    targetVoxelObject.transform.position = roomOrigin+ xVoxelIndex * Vector3.right * voxelSideLength + yVoxelIndex * Vector3.up * voxelSideLength + zVoxelIndex * Vector3.forward * voxelSideLength;
                    Voxel newVoxel = targetVoxelObject.AddComponent<Voxel>();
                    newVoxel.stableLifeTime = stableLifeTime;
                    voxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex] = targetVoxelObject;
                }
                targetVoxelObject.SetActive(true);
                Voxel targetVoxelScript = targetVoxelObject.GetComponent<Voxel>();
                targetVoxelScript.BeGazedAt();

            }
        }
    
        
        
    }
}

