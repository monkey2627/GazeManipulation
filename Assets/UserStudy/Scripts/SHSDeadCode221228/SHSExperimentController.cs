using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserStudy
{
    

    public class SHSExperimentController : MonoBehaviour
    {
        // ʵ��л�����Ϣ
        public GameObject ExperimentBox; // �����壬��������
        [HideInInspector] public Bounds boxBounds;
        [HideInInspector] public float boxLengthX;
        [HideInInspector] public float boxLengthY;
        [HideInInspector] public float boxLengthZ;
        [HideInInspector] public Vector3 boxOrigin;

        // ʵ��в�������Ϣ
        public GameObject samplePrefab; // ����С��Ԥ���壬��͸��
        public float sampleScale;
        public float sampleInterval;
        [HideInInspector] public GameObject sampleParentObject;
        [HideInInspector] public int sampleNumberX;
        [HideInInspector] public int sampleNumberY;
        [HideInInspector] public int sampleNumberZ;
        [HideInInspector] public List<List<List<GameObject>>> sampleArray; // ���ڴ��


        // ���ֽű�
        private SHSHandleInput handleInputScript;
        public GameObject rightHand;


        // Start is called before the first frame update
        void Start()
        {
            InitialExperimentBox();
            CreateSampleArray();

            // ��ʼ���ű�

            handleInputScript = GetComponent<SHSHandleInput>();

        }

        // Update is called once per frame
        void Update()
        {
            if (handleInputScript.isHandleSampleActive)
            {
                // ÿ֡����ֱ�λ�ã�ӳ�䵽�������꣬ע��Խ����
                TouchSampleObject(rightHand.transform.position);
            }
        }

        /// <summary>
        /// ��ʼ��ʵ�������Ϣ
        /// </summary>
        private void InitialExperimentBox()
        {
            boxBounds = ExperimentBox.GetComponent<BoxCollider>().bounds;
            boxLengthX = boxBounds.size.x;
            boxLengthY = boxBounds.size.y;
            boxLengthZ = boxBounds.size.z;
            boxOrigin = boxBounds.min;
        }

        private void CreateSampleArray()
        {
            // �����������ϲ���������������ȡ��
            sampleNumberX = Mathf.CeilToInt(boxLengthX / sampleScale);
            sampleNumberY = Mathf.CeilToInt(boxLengthY / sampleScale);
            sampleNumberZ = Mathf.CeilToInt(boxLengthZ / sampleScale);

            // �����������и�����
            sampleParentObject = new GameObject("�������и�����");

            // ������������
            sampleArray = new List<List<List<GameObject>>>();
            for (int ix = 0; ix < sampleNumberX; ix++)
            {
                List<List<GameObject>> tempXList = new List<List<GameObject>>();
                for (int iy = 0; iy < sampleNumberY; iy++)
                {
                    List<GameObject> tempYList = new List<GameObject>();
                    for (int iz = 0; iz < sampleNumberZ; iz++)
                    {
                        GameObject newSampleObject = Instantiate(samplePrefab, sampleParentObject.transform);
                        newSampleObject.name = string.Format("voxel_{0}{1}{2}", ix, iy, iz);
                        newSampleObject.SetActive(true);
                        newSampleObject.isStatic = true;
                        newSampleObject.transform.localScale = new Vector3(sampleScale, sampleScale, sampleScale);
                        newSampleObject.transform.position = boxOrigin +
                            ix * Vector3.right * (sampleScale + sampleInterval) +
                            iy * Vector3.up * (sampleScale + sampleInterval) +
                            iz * Vector3.forward * (sampleScale + sampleInterval);
                        tempYList.Add(newSampleObject);
                    }
                    tempXList.Add(tempYList);
                }
                sampleArray.Add(tempXList);
            }
        }

        private void TouchSampleObject(Vector3 worldPosition)
        {
            int xSampleIndex = Mathf.FloorToInt((worldPosition.x - boxOrigin.x) / sampleScale - 0.5f) + 1;
            int ySampleIndex = Mathf.FloorToInt((worldPosition.y - boxOrigin.y) / sampleScale - 0.5f) + 1;
            int zSampleIndex = Mathf.FloorToInt((worldPosition.z - boxOrigin.z) / sampleScale - 0.5f) + 1;

            if (xSampleIndex >= sampleNumberX || xSampleIndex < 0 || ySampleIndex >= sampleNumberY || ySampleIndex < 0 || zSampleIndex >= sampleNumberZ || zSampleIndex < 0)
            {
                Debug.LogError("[ERROR]�ֱ�Խ��");
                return;
            }

            GameObject targetSampleObject = sampleArray[xSampleIndex][ySampleIndex][zSampleIndex];
            if (targetSampleObject.activeSelf == true)
            {
                targetSampleObject.SetActive(false);
            }

        }


    }
    
}


