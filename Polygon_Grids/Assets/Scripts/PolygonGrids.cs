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

    private float scaleX;

    [SerializeField]
    private float scaleY=1f;

    [SerializeField]
    private float scaleZ = 1f;

    [SerializeField ]
    private int CountY = 30;

    private int CountX ;

    private Transform[] polyGrid;

    private Transform[,] Grid2d;

    private Transform[,,] Grid3d;

    private GOLRule RuleInUse = new GOLRule();

    public int TimeEnd = 10;

    int currentFrame = 0;

    private bool ReadyToCheck = false;

    void Awake()
    {
        RuleInUse.setupRule(2, 4, 3, 4);

        scaleX = Mathf.Sqrt(Mathf.Pow(scaleY, 2) - Mathf.Pow(scaleY / 2, 2)); 

        print(scaleX);

        var _ratio = image.width / image.height;
        var _ratio2 = scaleY / scaleX;

        CountX = Mathf.FloorToInt(CountY * _ratio*_ratio2)+2;

       
    }

	// Use this for initialization
	void Start ()
	{
	    camPivot.position = new Vector3(CountX / 2, 10, CountY / 2);
        createPolygon();
	   // getPixelToScale();
	    getPixelToState();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (currentFrame < TimeEnd)
        {
            calculatePoly();

            UpdateDisplayVoxel();

            MoveUp();
        }
        if (currentFrame < TimeEnd)
        {
            finalDisplay();
        }
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

           // currenPoly.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.red , Color.green , _t);
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
                

                //currenPoly.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.red, Color.green, _t);

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
                if (currentPoly.GetComponent<Voxel>().GetAge() > 1)
                {
                    currentPoly.GetComponent<Voxel>().SetFutureState(0);
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
                //Grid2d[i, j].GetComponent<Voxel>().VoxelDisplay();
            }
        }
    }

    void MoveUp()
    {
        Grid3d = new Transform[CountX, CountY, TimeEnd];
        for (int i = 0; i < CountX; i++)
        {
            for (int j = 0; j < CountY; j++)
            {
                int baseState = Grid2d[i, j].GetComponent<Voxel>().GetState();
                if (baseState == 1)
                {
                    Vector3 L0 = Grid2d[i, j].position;

                    Vector3 currentPosition = new Vector3(L0.x, currentFrame*scaleZ, L0.z);

                    Quaternion currentRotation = Grid2d[i, j].rotation;

                    Transform CurrentVoxel = Instantiate(Grid2d[i, j], currentPosition,currentRotation);

                    //CurrentVoxel.GetComponent<Voxel>().SetupVoxel(i,currentFrame,j,0);

                   // CurrentVoxel.GetComponent<Voxel>().SetState(1);

                   // CurrentVoxel.GetComponent<Voxel>().VoxelDisplay();

                    Grid3d[i, j, currentFrame] = CurrentVoxel;
                }
            }
        }
        currentFrame++;
    }

    private void checkConnection(Transform Voxel)
    {

        var x = Voxel.position.x;
        var z = Voxel.position.z;
        var y = Voxel.position.y;

        float Distance = 1f;

        Vector3 currentVector = Voxel.position;
        Vector3 _n0 = new Vector3(x, y, z + 1 * scaleY);
        Vector3 _n1 = new Vector3(x + 1 * scaleX, y, z + 0.5f * scaleY);
        Vector3 _n2 = new Vector3(x + 1 * scaleX, y, z - 0.5f * scaleY);
        Vector3 _n3 = new Vector3(x, y, z - 1 * scaleY);
        Vector3 _n4 = new Vector3(x - 1 * scaleX, y, z - 0.5f * scaleY);
        Vector3 _n5 = new Vector3(x - 1 * scaleX, y, z + 0.5f * scaleY);
        Vector3 _n6 = new Vector3(x, y + 1 * scaleZ, z);
        Vector3 _n7 = new Vector3(x, y - 1 * scaleZ, z);

        int N0, N1, N2, N3, N4, N5, N6, N7;

        if (Physics.Raycast(currentVector, _n0, Distance  * scaleY) == true)
        {
            N0 = 1;
        }
        else
        {
            N0 = 0;
        }
        if (Physics.Raycast(currentVector, _n1, Distance * scaleY) == true)
        {
            N1 = 1;
        }
        else
        {
            N1 = 0;
        }
        if (Physics.Raycast(currentVector, _n2, Distance * scaleY) == true)
        {
            N2 = 1;
        }
        else
        {
            N2 = 0;
        }
        if (Physics.Raycast(currentVector, _n3, Distance * scaleY) == true)
        {
            N3 = 1;
        }
        else
        {
            N3 = 0;
        }
        if (Physics.Raycast(currentVector, _n4, Distance * scaleY) == true)
        {
            N4 = 1;
        }
        else
        {
            N4 = 0;
        }
        if (Physics.Raycast(currentVector, _n5, Distance * scaleY) == true)
        {
            N5 = 1;
        }
        else
        {
            N5 = 0;
        }
        if (Physics.Raycast(currentVector, _n6, Distance * scaleZ) == true)
        {
            N6 = 1;
        }
        else
        {
            N6 = 0;
        }
        if (Physics.Raycast(currentVector, _n7, Distance * scaleZ) == true)
        {
            N7 = 1;
        }
        else
        {
            N7 = 0;
        }
        int Neighbor2d = N0 + N1 + N2 + N3 + N4 + N5;

        if (N7 == 0 && Neighbor2d ==0)
        {
            Destroy(Voxel.gameObject);
            //print("destroy");
        }

        if (Neighbor2d > 3)
        {
            Voxel.GetComponent<Voxel>().SetupVoxel(x, y, z, 0);
            Voxel.GetComponent<Voxel>().SetState(1);
            Voxel .GetComponent<Voxel>().VoxelDisplay();
        }

        if (Neighbor2d <= 3)
        {
            if(Neighbor2d !=0) print(Neighbor2d);

            if (N0 == 1 && N3 == 1)
            {
                Voxel.GetComponent<Voxel>().SetupVoxel(x, y, z, 1);
                Voxel.GetComponent<Voxel>().SetState(1);
                Voxel.GetComponent<Voxel>().VoxelDisplay();
            }
            else if (N1 == 1 && N4 == 1)
            {
                Voxel.GetComponent<Voxel>().SetupVoxel(x, y, z, 2);
                Voxel.GetComponent<Voxel>().SetState(1);
                Voxel.GetComponent<Voxel>().VoxelDisplay();
            }
            else if (N2 == 1 && N5 == 1)
            {
                Voxel.GetComponent<Voxel>().SetupVoxel(x, y, z, 3);
                Voxel.GetComponent<Voxel>().SetState(1);
                Voxel.GetComponent<Voxel>().VoxelDisplay();
            }
            else
            {
                Voxel.GetComponent<Voxel>().SetupVoxel(x, y, z, 0);
                Voxel.GetComponent<Voxel>().SetState(1);
                Voxel.GetComponent<Voxel>().VoxelDisplay();
            }
        }
       
    }

    void finalDisplay()
    {
        for (int i = 1; i < CountX - 1; i++)
        {
            for (int j = 1; j < CountY - 1; j++)
            {
                for (int k = 0; k < TimeEnd; k++)
                {
                    Transform currentPoly = Grid3d[i, j,k];

                    if (currentPoly == null)
                    {
                        continue;
                    }
                    else
                    {
                        int n0, n1, n2, n3, n4, n5;
                        if (Grid2d[i - 1, j] != null)
                        {
                            n0 = Grid2d[i - 1, j].GetComponent<Voxel>().GetState();
                        }
                        else
                        {
                            n0 = 0;
                        }
                        if (Grid2d[i - 1, j + 1] != null)
                        {
                            n1 = Grid2d[i - 1, j + 1].GetComponent<Voxel>().GetState();
                        }
                        else
                        {
                            n1 = 0;
                        }
                        if (Grid2d[i, j - 1] != null)
                        {
                            n2 = Grid2d[i, j - 1].GetComponent<Voxel>().GetState();
                        }
                        else
                        {
                            n2 = 0;
                        }
                        if (Grid2d[i, j + 1] != null)
                        {
                            n3 = Grid2d[i, j + 1].GetComponent<Voxel>().GetState();
                        }
                        else
                        {
                            n3 = 0;
                        }
                        if (Grid2d[i + 1, j] != null)
                        {
                            n4 = Grid2d[i + 1, j].GetComponent<Voxel>().GetState();
                        }
                        else
                        {
                            n4 = 0;
                        }
                        if (Grid2d[i + 1, j + 1] != null)
                        {
                            n5 = Grid2d[i + 1, j + 1].GetComponent<Voxel>().GetState();
                        }
                        else
                        {
                            n5 = 0;
                        }
                      
                        int currentNeighborCount = n0 + n1 + n2 + n3 + n4 + n5;

                        if (currentNeighborCount ==0)
                        {
                            Destroy(currentPoly.gameObject);
                        }

                        if (currentNeighborCount==2)
                        {
                            if (n2 == 1 && n3 == 1)
                            {
                                currentPoly.GetComponent<Voxel>().SetupVoxel(i, currentFrame, j, 1);
                            }
                            else if (n0 == 1 && n5 == 1)
                            {
                                currentPoly.GetComponent<Voxel>().SetupVoxel(i, currentFrame, j, 2);
                            }
                            else if (n1 == 1 && n4 == 1)
                            {
                                currentPoly.GetComponent<Voxel>().SetupVoxel(i, currentFrame, j, 3);
                            }
                            else
                            {
                                currentPoly.GetComponent<Voxel>().SetupVoxel(i, currentFrame, j, 0);
                            }
                        }
                        else
                        {
                            currentPoly.GetComponent<Voxel>().SetupVoxel(i, currentFrame, j, 4);
                        }
                        currentPoly.GetComponent<Voxel>().SetState(1);
                        currentPoly.GetComponent<Voxel>().VoxelDisplay();
                    }
                }
            }
        }
    }
}
