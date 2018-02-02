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


    void Awake()
    {
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

	}
	
	// Update is called once per frame
	void Update ()
    {
	    //createPolygon();
    }

    void createPolygon()
    {
        int index = 0;
        polyGrid = new Transform[CountX * CountY];
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
            }
        }
    }

    void getPixelToScale()
    {
        int v = polyGrid.Length;

        int W =Mathf.FloorToInt( polyGrid[v - 1].position.x);
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

}
