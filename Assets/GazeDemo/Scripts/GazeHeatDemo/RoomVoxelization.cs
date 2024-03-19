using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using Valve.VR;
using Valve.VR.Extras;

namespace GazeHeatDemo
{
    /// <summary>
    /// ������Ϣ�ṹ��
    /// </summary>
    public struct RoomBox
    {
        public Bounds RoomBounds;
        public float XLength;
        public float YLength;
        public float ZLength;
        public Vector3 BoxOriginPoint; // ������ԭ��
    }

    public struct Voxel
    {
        public GameObject voxelObject; // �����������
        public int gazeTimes; // �����ӵĴ���
    }

    public class RoomVoxelization : MonoBehaviour
    {
        public GameObject roomObject;
        public GameObject voxelPrefab;

        private RoomBox roomBox; // ����������Ϣ�ṹ��

        // public Material VoxelBeGazedMaterial;

        // ����������Ϣ
        public float voxcelSideLength; // ���ر߳�(Ĭ��������)
        private GameObject voxcelParent; // �������пո�����
        [HideInInspector] public List<List<List<Voxel>>> voxelArray;
        [HideInInspector] public int xVoxellNumber; // x������������
        [HideInInspector] public int yVoxellNumber; // y������������
        [HideInInspector] public int zVoxellNumber; // z������������

        // ���ӵ���Ϣ
        public GameObject GazeRaySample;
        private SRanipal_GazeRaySample_v2 sRanipalGazeSample;

        // VR�ֱ���������
        public SteamVR_Action_Boolean switchGazeMode;
        private bool isGazeVoxelization =false;

        // ���ӵ�����
        public GameObject gazeCursorPrefab;
        private GameObject gazeCursorObject;

        // Start is called before the first frame update
        void Start()
        {
            // ��ȡ���ӵ�ű�
            sRanipalGazeSample = GazeRaySample.GetComponent<SRanipal_GazeRaySample_v2>();

            // ��ȡ��Χ��
            Bounds roomBounds = roomObject.GetComponent<BoxCollider>().bounds;
            roomBox = new RoomBox() { 
                RoomBounds = roomBounds, 
                XLength = roomBounds.size.x, 
                YLength = roomBounds.size.y, 
                ZLength = roomBounds.size.z, 
                BoxOriginPoint = roomBounds.min,

            };

            CreateVoxelArray();

            gazeCursorObject = Instantiate(gazeCursorPrefab);
            gazeCursorObject.SetActive(false);
        }

        private void Update()
        {
            if (switchGazeMode.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                isGazeVoxelization = !isGazeVoxelization;
                if (isGazeVoxelization) { Debug.Log("�������ӵ㲶��ģʽ..."); }
                else { Debug.Log("���ӵ㲶��ģʽ�ر�..."); }
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // Debug.Log("���ӵ�����:"+sRanipalGazeSample.GazeDirectionCombined.ToString());
            
            if (isGazeVoxelization)
            {
                if (gazeCursorObject.activeSelf == false) { gazeCursorObject.SetActive(true); }
                UpdateVoxelByGazePoint(sRanipalGazeSample.GazeDirectionCombined);
            }
            if (!isGazeVoxelization)
            {
                if (gazeCursorObject.activeSelf == true) { gazeCursorObject.SetActive(false); }
            }
            
        }

        /// <summary>
        /// ������������
        /// </summary>
        private void CreateVoxelArray()
        {
            // ��������������������������ȡ��
            xVoxellNumber = Mathf.CeilToInt(roomBox.XLength / voxcelSideLength); 
            yVoxellNumber = Mathf.CeilToInt(roomBox.YLength / voxcelSideLength);
            zVoxellNumber = Mathf.CeilToInt(roomBox.ZLength / voxcelSideLength);

            // �����������и�����
            voxcelParent = new GameObject("�������и�����");

            // ������������
            voxelArray = new List<List<List<Voxel>>>();
            for (int ix = 0; ix < xVoxellNumber; ix++)
            {
                List<List<Voxel>> tempXList = new List<List<Voxel>>();
                for (int iy = 0; iy < yVoxellNumber; iy++)
                {
                    List<Voxel> tempYList = new List<Voxel>();
                    for (int iz = 0; iz < zVoxellNumber; iz++)
                    {
                        GameObject newVoxelObject = Instantiate(voxelPrefab, voxcelParent.transform);
                        newVoxelObject.name = string.Format("voxcel_{0}{1}{2}", ix, iy, iz);
                        newVoxelObject.isStatic = true;
                        // ��������λ��
                        newVoxelObject.transform.position = roomBox.BoxOriginPoint + ix * Vector3.right * voxcelSideLength + iy * Vector3.up * voxcelSideLength + iz * Vector3.forward * voxcelSideLength;

                        // ��ʱ����Ҫ��������
                        Voxel newVoxcelStruct = new Voxel() { voxelObject = newVoxelObject, gazeTimes = 0 };
                        tempYList.Add(newVoxcelStruct);
                    }
                    tempXList.Add(tempYList);
                }
                voxelArray.Add(tempXList);
            }
        }


        private void UpdateVoxelByGazePoint(Vector3 gazeDirect)
        {
            // ��ȡ��ʼ��
            Vector3 eyePosition = Camera.main.transform.position - Camera.main.transform.up * 0.05f;
            // ��ȡ���ӵ��볡������ײ��
            Ray gazeRay = new Ray(eyePosition, gazeDirect);
            RaycastHit hitInfo;
            if(Physics.Raycast(gazeRay, out hitInfo))
            {
                Debug.DrawRay(eyePosition, gazeDirect, Color.green);
                Vector3 gazePoint = hitInfo.point;
                int xVoxelIndex = Mathf.FloorToInt((gazePoint.x - roomBox.BoxOriginPoint.x) / voxcelSideLength - 0.5f) + 1;
                int yVoxelIndex = Mathf.FloorToInt((gazePoint.y - roomBox.BoxOriginPoint.y) / voxcelSideLength - 0.5f) + 1;
                int zVoxelIndex = Mathf.FloorToInt((gazePoint.z - roomBox.BoxOriginPoint.z) / voxcelSideLength - 0.5f) + 1;
                
                Voxel tempVoxel = voxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex];

                if (tempVoxel.gazeTimes == 0)
                {
                    // �����ؿ��һ�α�ѡ�У�Ϊ�䶨�ƽű�
                    Material newMaterial = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"))
                    {
                        color = new Color(168f/255f, 15f/255f, 15f/255f, 0f)
                    };
                    tempVoxel.voxelObject.GetComponent<MeshRenderer>().material = newMaterial;

                }
                tempVoxel.gazeTimes += 1;
                //Material voxelMaterial = tempVoxel.voxelObject.GetComponent<MeshRenderer>().material;
                //voxelMaterial.color = new Color(168, 15, 15, GazeTimes2Alpha(tempVoxel.gazeTimes));
                int alpha = GazeTimes2Alpha(tempVoxel.gazeTimes,50f);
                // print("alpha:" + alpha);
                tempVoxel.voxelObject.GetComponent<MeshRenderer>().material.SetColor("_Color",new Color(168f / 255f, 15f / 255f, 15f / 255f, alpha/255f));
                // print("material:" + tempVoxel.voxelObject.GetComponent<MeshRenderer>().material.color.ToString());

                voxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex] = tempVoxel;

                // ����
                gazeCursorObject.transform.position = hitInfo.point;
                // Debug.Log("��ײ�㷨����:" + hitInfo.normal.ToString());
                // gazeCursorObject.transform.eulerAngles = hitInfo.normal;
            }

        }

        private int GazeTimes2Alpha(int gazeTimes,float magnification)
        {
            float angles = Mathf.Atan(gazeTimes/magnification);
            int alpha = Mathf.FloorToInt(angles * 512 / Mathf.PI);

            return  alpha>=255?255:alpha;
        }
    }
}


