using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonGrids : MonoBehaviour
{
    [SerializeField ]
    private Transform camPivot;
    [SerializeField ]
    private Transform Prefab;

    [SerializeField ]
    private Texture2D image;

    private float scaleX ;

    [SerializeField]
    private float scaleY=1f;

    [SerializeField ]
    private int CountY = 30;

    private int CountX ;

    private Transform[] polyGrid;

    private Transform[,] Grid2d;

    private GOLRule RuleInUse = new GOLRule();


    void Awake()
    {
        RuleInUse.setupRule(1, 2, 2, 3);

        scaleX = Mathf.Sqrt(Mathf.Pow(scaleY, 2) - Mathf.Pow(scaleY / 2, 2)); 

        print(scaleX);

        var _ratio = image.width / image.height;
        var _ratio2 = scaleY / scaleX;

        CountX = Mathf.FloorToInt(CountY * _ratio*_ratio2)+2;

        print(CountX);
    }

	// Use this for initialization
	void Start ()
	{

        createPolygon();

	    getPixelToScale();
	    getPixelToState() ;

	}
	
	// Update is called once per frame
	void Update ()
    {
        //createPolygon();

        calculatePoly();

        UpdateDisplayVoxel();


    }

    void createPolygon()
    {

        int index = 0;
        polyGrid = new Transform[CountX * CountY];
        Grid2d = new Transform[CountX, CountY];
        for (int i = 0; i < CountX; i++)
        {
            for (int j = 0; j < CountY; j++,index++)
            {
                var polygon = Instantiate(Prefab, transform);
                var _p1 = new Vector3(i * scaleX, 0, j * scaleY);
                var _p2 = new Vector3(i * scaleX, 0, j * scaleY + scaleY / 2);

                polygon.name ="X_"+ i.ToString() + " , " +"Y_"+ j.ToString();
                if (i % 2 == 0)
                {
                    polygon.localPosition = _p1;
                }
                else
                {
                    polygon.localPosition = _p2;
                }
                polyGrid[index] = polygon;

                Grid2d[i, j] = polygon;
            }
        }
    }

    void getPixelToScale()
    {
        int v = polyGrid.Length;
        //int W =Mathf.FloorToInt( polyGrid[v - 1].position.x);
        int H =Mathf.FloorToInt( CountY *scaleY);
        float _q = image.height / H;

        for (int i = 0; i <v; i++)
        {
            Transform currenPoly = polyGrid[i];
            int _x = Mathf.FloorToInt(currenPoly .position.x*_q);
            int _y = Mathf.FloorToInt(currenPoly .position.z*_q);

            float _t = image.GetPixel(_x, _y).grayscale;

           Vector3 toScale = currenPoly.localScale*_t;

            currenPoly .localScale = toScale;

            currenPoly.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.red , Color.green , _t);


        }
    }

    void getPixelToState()
    {
        int v = polyGrid.Length;
        //int W =Mathf.FloorToInt( polyGrid[v - 1].position.x);
        int H = Mathf.FloorToInt(CountY * scaleY);
        float _q = image.height / H;

        for (int i = 0; i < CountX; i++)
        {
            for (int j = 0; j < CountY; j++)
            {
                Transform currenPoly = Grid2d [i, j];

                currenPoly.GetComponent<Voxel>().SetupVoxel(i, 0, j, 1);
                int _x = Mathf.FloorToInt(currenPoly.position.x * _q);
                int _y = Mathf.FloorToInt(currenPoly.position.z * _q);

                float _t = image.GetPixel(_x, _y).grayscale;
                print(_t);

                currenPoly.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.red, Color.green, _t);

                if (_t <0.6f)
                {
                    currenPoly.GetComponent<Voxel>().SetState(0);
                }
                else
                {
                    currenPoly.GetComponent<Voxel>().SetState(1);
                }

              
            }
        }
    }

    void calculatePoly()
    {
        for (int i = 1; i < CountX-1; i++)
        {
            for (int j = 1; j < CountY-1; j++)
            {
                Transform currentPoly = Grid2d[i, j];
                int currentState = currentPoly.GetComponent<Voxel>().GetState();

                int n0 = Grid2d[i - 1, j].GetComponent<Voxel>().GetState();
                int n1 = Grid2d[i - 1, j + 1].GetComponent<Voxel>().GetState();
                int n2 = Grid2d[i , j - 1].GetComponent<Voxel>().GetState();
                int n3 = Grid2d[i , j + 1].GetComponent<Voxel>().GetState();
                int n4 = Grid2d[i +1, j ].GetComponent<Voxel>().GetState();
                int n5 = Grid2d[i +1, j + 1].GetComponent<Voxel>().GetState();

                int currentNeighborCount = n0 + n1 + n2 + n3 + n4 + n5;

                int Ins0 = RuleInUse.getInstruction(0);
                int Ins1 = RuleInUse.getInstruction(1);
                int Ins2 = RuleInUse.getInstruction(2);
                int Ins3 = RuleInUse.getInstruction(3);

                if (currentState == 1)
                {
                    if (currentNeighborCount < Ins0)
                    {
                        currentPoly.GetComponent<Voxel>().SetFutureState(0);
                    }
                    if (currentNeighborCount >= Ins0 && currentNeighborCount <= Ins1)
                    {
                        currentPoly.GetComponent<Voxel>().SetFutureState(1);
                    }
                    if(currentNeighborCount > Ins1)
                    {
                        currentPoly.GetComponent<Voxel>().SetFutureState(0);
                    }
                }
                if (currentState == 0)
                {
                    if (currentNeighborCount >= Ins2&& currentNeighborCount <= Ins3)
                    {
                        currentPoly.GetComponent<Voxel>().SetFutureState(1);
                    }
                }
            }
        }
    }

    void UpdateDisplayVoxel()
    {
        for (int i = 0; i < CountX; i++)
        {
            for (int j = 0; j < CountY; j++)
            {
                Grid2d[i, j].GetComponent<Voxel>().UpdateVoxel();
                Grid2d[i, j].GetComponent<Voxel>().VoxelDisplay();
            }
        }
    }
}
