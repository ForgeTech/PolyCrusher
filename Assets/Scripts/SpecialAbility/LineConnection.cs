using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class LineConnection : MonoBehaviour {

    public float heightOffset;
    public float healAmount;
    public float energyHealAmount;
    public float normalLineWidth;
    public float healLineWidth;
    public GameObject[] healingparticles;

    public float healDistance;
    public float lerpSpeed;
    public Material[] mats;


    public List<BaseEnemy> enemies;

   
    public GameObject[] players;

    

    public GameObject particles;

    private Vector3[] oldPlayerPosition;

    private float[] incremenetTimers;
    private float[] incrementEnergyTimers;

    public bool[] playerMoved;
    public bool[] playerHealing;
    //public float[] healTimers;


    public BasePlayer[] playerScripts;

    public int[] playerAlignment;
    

    public float charging;

    public int[] intersectedLines;

    public Mesh plane;

    public int oldDonkey;
    public int donkey;



    public bool changedToTri;
    public bool changedToQuad;

    public float lerpTime ;

    public int intersection;

    public float riseTime;

    public SphereCollider sphere;

    public GameObject poly;
    public GameObject triPoly;



    private Mesh polygon;
    private Mesh flippedPolygon;




    public bool detonate;

    private Mesh triPolygon;
    private Mesh flippedTriPolygon;


    private MeshFilter flippedMeshFilter;
    private MeshFilter meshFilter;

    private PolygonCollider2D polyCollider;
    private PolygonCollider2D triPolyCollider;

    public Vector3 middlePoint = new Vector3();

    private LineRenderer[] lineRenderer;
    private int[] linesNeeded;
    private int[] firstVertex;
    private int[] secondVertex;
    public float[] lerpTimes;
    public float[] blinkTime;
    public float pulseTime;
    public bool[] energyFillUp;

    public bool[] isHealedRightNow;

    private int[] activeMaterial;
    private int lineRendererLength;
    public bool playerCountUpdateNeeded = true;
    public bool fU;
    public Vector3 intersectPosition;
    public float[] angles;
    public float[] oldAngles;
    public bool firstAngles;

	// Use this for initialization
	void Start () {
        linesNeeded = new int[] { 0, 1, 3, 6 };
        firstVertex = new int[] { 0, 1, 2, 0, 1, 2 };
        secondVertex = new int[] { 1, 2, 0, 3, 3, 3};      
        playerMoved = new bool[4];
        playerHealing = new bool[4];
        isHealedRightNow = new bool[4];
        playerScripts = new BasePlayer[4];
        lerpTimes = new float[4];
        blinkTime = new float[4];
        energyFillUp = new bool[4];
        incrementEnergyTimers = new float[4];
        PlayerManager.PlayerJoinedEventHandler += UpdatePlayerStatus;
        BasePlayer.PlayerDied += UpdatePlayerStatus;

        UpdatePlayerStatus();
        //healTimers = new float[4];
        


	}
	
	// Update is called once per frame
	//void Update () {
        

        
        

       

        


        //if (players.Length == 4)
        //{            
        //    if (!HorseInTheMiddle())
        //    {
        //        GetAlignment(1);
        //        donkey = -1;
        //        HandleIntersection();
        //    }
        //    else
        //    {
        //        GetAlignment(1);
        //    }                    
        //}
        


        //if (changedToQuad)
        //{                      
        //        flippedMeshFilter.sharedMesh = dummy;                
        //        changedToQuad = false;
        //}


        //if (changedToTri)
        //{           
        //        meshFilter.sharedMesh = dummy;               
        //        changedToTri = false;            
        //}

       // HandleSpecialPolygon();

       // int tmp = 0;
       // for (int i = 0; i < players.Length; i++ )
       // {
       //     if (players[i].transform.position == oldPlayerPosition[i])
       //     {
       //         tmp++;
       //     }
       //     else
       //     {
       //         oldPlayerPosition[i] = players[i].transform.position;
       //     }

       // }

       // if (tmp == players.Length)
       // {
       //     charging -= Time.deltaTime;
       //     if (charging <= 0.0f)
       //     {
       //         detonate = true;
       //     }

       // }
       // else
       // {
       //     charging = 1.0f;
       // }



       // if (detonate)
       // {
       //     detonate = false;
       //     sphere.enabled = true;
       //     StartCoroutine(WaitForSeconds());

       // }


       // //if (players.Length == 2 && animation)
       // //{
       // //    lerpTime += Time.deltaTime;

       // //    //float scale = 0.5f;
       // //    if(lerpTime >= 1){
       // //        lerpTime = 1;
       // //        animation = false;


       // //    }
       // //    Vector3 newPos = new Vector3(poly.transform.position.x*lerpTime,1/lerpTime ,poly.transform.position.z*lerpTime);
       // //    //poly.transform.position *= lerpTime;

       // //    poly.transform.position = newPos;
       // //    poly.transform.localScale = new Vector3(lerpTime, 1.0f, lerpTime);
       // //}



      


       //if (oldDonkey != donkey)
       //{
       //    if (donkey == -1)
       //    {
       //        changedToQuad = true;
       //    }
       //    else
       //    {
       //        changedToTri = true;              
       //    }
       //    oldDonkey = donkey;
       //}

	//}


    void Update()
    {
      

        UpdateLines();

    }


    void FixedUpdate()
    {
        //UpdatePlayerStatus();



        UpdateConnectionType();

        UpdatePlayerPosition();


        HandleHealthRecovery();

        HealOverTime();


    }
  


    void UpdatePlayerPosition()
    {
        
        for (int i = 0; i < players.Length; i++)
        {
            playerMoved[i] = false;
            
            if (Vector3.Distance(oldPlayerPosition[i],players[i].transform.position) > 0.02f)
            {
                oldPlayerPosition[i] = players[i].transform.position;  
                playerMoved[i] = true;
                playerHealing[i] = false;
            }
           
            
        }

    }


    void HandleHealthRecovery()
    {
        for (int i = 0; i < players.Length; i++)
        {
            for (int j = 0; j < players.Length; j++)
            {
                if (i != j)
                {
                    
                    if (Vector3.Distance(players[i].transform.position, players[j].transform.position) <= healDistance)
                    {
                        if (!playerMoved[i])
                        {
                            
                           
                                playerHealing[i] = true;
                            
                            

                        }
                        else
                        {
                            
                            playerHealing[i] = false;
                        }


                        if (!playerMoved[j])
                        {
                           
                                playerHealing[j] = true;
                            

                        }
                        else
                        {
                            
                            playerHealing[j] = false;
                        }


                    }                  
                }
            }
        }


    }



    void HealOverTime()
    {
        for (int i = 0; i < players.Length; i++)
        {
            


            if (playerHealing[i])
            {
                //playerHealing[i] = false;
                if (playerScripts[i].Health <= 99)
                {
                    //isHealedRightNow[i] = true;
                    incremenetTimers[i] += Time.deltaTime * healAmount;
                    if (incremenetTimers[i] >= 1.0f)
                    {
                        incremenetTimers[i] = 0.0f;
                        playerScripts[i].Health += 1;
                    }

                    if (healingparticles[i] == null)
                    {
                        healingparticles[i] = Instantiate(particles, players[i].transform.position, Quaternion.Euler(-90, 0, 0)) as GameObject;
                    }

                }
                else
                {
                    //playerHealing[i] = false;
                    if (healingparticles[i] != null)
                    {
                        Destroy(healingparticles[i].gameObject);
                    }
                    
                    //isHealedRightNow[i] = false;

                }


                if (playerScripts[i].Energy <= 99)
                {
                    energyFillUp[i] = true;
                    incrementEnergyTimers[i] += Time.deltaTime * energyHealAmount;
                    if (incrementEnergyTimers[i] >= 1.0f)
                    {
                        incrementEnergyTimers[i] = 0.0f;
                        playerScripts[i].Energy += 1;
                    }


                }
                else
                {
                    energyFillUp[i] = false;
                }



                
            }
            else
            {
                if (healingparticles[i] != null)
                {
                    Destroy(healingparticles[i].gameObject); 
                }
            }
        }
    }


    void OnTriggerStay(Collider coll)
    {
        if (coll.tag == "Enemy")
        {
            

            if (IsPointInTri(coll.transform.position, players[0].transform.position, players[1].transform.position, players[2].transform.position) || IsPointInTri(coll.transform.position, players[0].transform.position, players[2].transform.position, players[3].transform.position))
            {
                coll.GetComponent<BaseEnemy>().InstantKill();
            }
        }


    }




    float Sign(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
    }

    bool IsPointInTri(Vector3 pt, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        bool b1, b2, b3;

        b1 = Sign(pt, v1, v2) < 0.0f;
        b2 = Sign(pt, v2, v3) < 0.0f;
        b3 = Sign(pt, v3, v1) < 0.0f;

        return ((b1 == b2) && (b2 == b3));
    }

    private bool HorseInTheMiddle()
    {
        for (int i = 0; i < players.Length; i++)
        {
           
          if(IsPointInTri(players[i%4].transform.position, players[(i+1)%4].transform.position, players[(i+2)%4].transform.position, players[(i+3)%4].transform.position)){
              intersectedLines[0] = -1;
              intersectedLines[1] = -1;
              donkey = i;
              return true;
          }
        }
        return false;
    } 

    void HandleIntersection()
    {
        intersectedLines[0] = -1;
        intersectedLines[1] = -1;
      //  if (!fU)
        {
            fU = true;
            for (int i = 0; i < lineRenderer.Length; i++)
            {
                float x1 = players[firstVertex[i]].transform.position.x;
                float y1 = players[firstVertex[i]].transform.position.z;

                float x2 = players[secondVertex[i]].transform.position.x;
                float y2 = players[secondVertex[i]].transform.position.z;

                for (int j = 0; j < lineRenderer.Length; j++)
                {

                    if (i != j)
                    {
                        float x3 = players[firstVertex[j]].transform.position.x;
                        float y3 = players[firstVertex[j]].transform.position.z;

                        float x4 = players[secondVertex[j]].transform.position.x;
                        float y4 = players[secondVertex[j]].transform.position.z;

                        float A1 = y2 - y1;
                        float B1 = x1 - x2;
                        float C1 = A1 * x1 + B1 * y1;

                        float A2 = y4 -y3;
                        float B2 = x3 - x4;
                        float C2 = A2*x3+B2*y3;


                        float delta = (A1 * B2) - (A2 * B1);
                        if (delta != 0)
                        {
                            float interX = (B2 * C1 - B1 * C2) / delta;
                            float interY = (A1 * C2 - A2 * C1) / delta;

                            intersectPosition = new Vector3(interX, 0, interY);

                            float newX1 = x1;
                            float newY1 = y1;
                            float newX2 = x2;
                            float newY2 = y2;

                            float newX3 = x3;
                            float newY3 = y3;
                            float newX4 = x4;
                            float newY4 = y4;
                            

                            if (x1 > x2)
                            {
                                newX1 = x2;
                                newX2 = x1;
                            }

                            if (y1 > y2)
                            {
                                newY1 = y2;
                                newY2 = y1;
                            }

                            if (x3 > x4)
                            {
                                newX3 = x4;
                                newX4 = x3;
                            }

                            if (y3 > y4)
                            {
                                newY3 = y4;
                                newY4 = y3;
                            }


                            if (newX1+0.01 < interX && newX2-0.010 > interX && newY1+0.010 < interY && newY2-0.010 > interY && newX3+0.010 < interX && newX4-0.010 > interX && newY3+0.010 < interY && newY4-0.010 > interY)
                            {
                               
                                //intersectedLines[0] = i;
                                //intersectedLines[1] = j;
                                return;

                            }
                            
                        }


                    }
                }

            }
            intersectedLines[0] = -1;
            intersectedLines[1] = -1;
        }


    }


    


    private void GetAlignment(int type)
    {
        middlePoint = new Vector3();
        float[] lengths = new float[players.Length];
        float steps = 0.0f;



        for (int i = 0; i < players.Length; i++)
        {
            lengths[i] = Vector3.Magnitude(players[i].transform.position);
            steps += lengths[i];
            
        }

        steps = 1 / steps;

        for (int i = 0; i < players.Length; i++)
        {
            middlePoint += (players[i].transform.position);
        }        

        middlePoint /= players.Length;

        for (int i = 0; i < players.Length; i++)
        {
            Vector3 tmp = new Vector3() ;
            Vector3 tmp2 = new Vector3();
            if (type == 1)
            {
                 tmp = (players[i].transform.position - middlePoint);
                 tmp2 = (Vector3.zero - middlePoint);
            }
            else if (type == 0)
            {
                tmp = (players[i].transform.position - players[donkey].transform.position);
                tmp2 = (Vector3.zero - players[donkey].transform.position);
            }            
           
            float newAngle = Mathf.Atan2(tmp.x * tmp2.x, tmp.z * tmp2.z) / (float)Mathf.PI * 180;
            if (i != donkey)
            {
                angles[i] = newAngle;
            }
            
           
        }

        if (type == 0)
        {
            angles[donkey] = 0.0f;
        }

        for (int i = 0; i < players.Length; i++)
        {

            for (int j = 0; j < players.Length; j++)
            {       

                if(i!=j && angles[i] < angles[j] && i != donkey && j!= donkey){
                    GameObject go = players[i];
                    players[i] = players[j];
                    players[j] = go;

                    float tmp = angles[i];
                    angles[i] = angles[j];
                    angles[j] = tmp;

                }
            }
        }

       
    }


    void HandleSpecialPolygon()
    {
        sphere.center = middlePoint;

        if (polygon == null && players.Length != 1)
        {
            CreatePolygon();

        }
        
        if (players.Length == 4)
        {
            
               

                if (players.Length == 4 && donkey == -1)
                {

                    Vector3[] vertices3d = new Vector3[players.Length+1];

                    for (int i = 0; i < players.Length; i++)
                    {   

                        vertices3d[i] = players[i].transform.position;
                    }
                    
                   
                    vertices3d[players.Length] = intersectPosition;
                    polygon.vertices = vertices3d;
                    
                    polygon.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector3(0.5f,0.5f)};
                    polygon.triangles = new int[] { 0, 1, 4, 1, 2, 4, 2, 3, 4, 3, 0, 4 };

         
                    polygon.RecalculateNormals();

                    polygon.RecalculateBounds();
                    polygon.Optimize();


                    meshFilter.sharedMesh = polygon;
                    middlePoint = intersectPosition;


                    
                }
                else if (donkey != -1)
                {

                    Vector3[] vertices3d = new Vector3[players.Length];
                    int currentPos = 0;

                    for (int i = 0; i < players.Length; i++)
                    {
                        if (i != donkey)
                        {
                           
                            vertices3d[currentPos++] = players[i].transform.position;
                        }
                       

                    }
                    vertices3d[players.Length - 1] = players[donkey].transform.position;                

                    triPolygon.vertices = vertices3d;
                    
                    
                    triPolygon.triangles = new int[] { 0, 1, 3, 1,2,3,2,0,3};

                    triPolygon.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) , new Vector2(0.5f,0.5f)};



                    triPolygon.RecalculateNormals();

                    triPolygon.RecalculateBounds();
                    triPolygon.Optimize();


                     flippedMeshFilter.sharedMesh = triPolygon;

                    

                }               
           }

        if (players.Length == 3)
        {
            Vector3[] vertices3d = new Vector3[players.Length];
            
            middlePoint = new Vector3();

            for (int i = 0; i < players.Length; i++)
            {
                vertices3d[i] = players[i].transform.position;
                middlePoint += vertices3d[i];
            }
            //vertices3d[players.Length - 1] = players[donkey].transform.position;

            middlePoint /= 3;



            triPolygon.vertices = vertices3d;


            triPolygon.triangles = new int[] { 2, 1, 0};

            triPolygon.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };



            triPolygon.RecalculateNormals();

            triPolygon.RecalculateBounds();
            triPolygon.Optimize();


            flippedMeshFilter.sharedMesh = triPolygon;
        }

        if (players.Length == 2)
        {
            Vector3 direction = new Vector3();

            direction = players[0].transform.position - players[1].transform.position;

            Vector3 pos = (players[0].transform.position + players[1].transform.position) / 2;

            Vector3 pointA = pos + (Quaternion.Euler(0, -90, 0) * direction/2);

            Vector3 pointB = pos + (Quaternion.Euler(0, 90, 0) * direction/2);

            Vector3[] vertices3d = new Vector3[players.Length+2];


            vertices3d[0] = players[0].transform.position;
           
            vertices3d[1] = pointA;

            vertices3d[2] = players[1].transform.position;

            vertices3d[3] = pointB;


            polygon.vertices = vertices3d;

            polygon.triangles = new int[] {0,1,2,0,2,3 };

            polygon.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1,1) };


            polygon.RecalculateBounds();

            polygon.RecalculateNormals();

            polygon.Optimize();
            
            meshFilter.sharedMesh = polygon;


            middlePoint = pos;




        }



    }

    private void UpdatePolygonShape()
    {
        Vector3[] updatedVertices = new Vector3[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            updatedVertices[i] = players[i].transform.position;
        }

        polygon.vertices = updatedVertices;

    }


    private void RiseOfThePolygon()
    {
        
        {
           // gameObject.transform.localScale += new Vector3(0.01f, 0.0f, 0.01f);
        }


    }

    void CreatePolygon()
    {
        polygon = new Mesh();
        polygon.name = "Poly";

     

        triPolygon = new Mesh();
        triPolygon.name = "TriPoly";

    

       

        meshFilter = poly.AddComponent<MeshFilter>();

        flippedMeshFilter = triPoly.AddComponent<MeshFilter>();

        
       

        MeshRenderer meshrenderer = poly.AddComponent<MeshRenderer>();
        meshrenderer.sharedMaterial = mats[2];

        MeshRenderer triMeshRenderer = triPoly.AddComponent<MeshRenderer>();
        triMeshRenderer.sharedMaterial = mats[2];


      
        
        



        Vector3 pos = new Vector3();

        for (int i = 0; i < players.Length; i++)
        {
            pos += players[i].transform.position;
        }
        pos /= players.Length;

        poly.transform.parent = gameObject.transform;
        triPoly.transform.parent = gameObject.transform;

       
       





    }
   

    void UpdateLines()
    {
        for (int i = 0; i < lineRenderer.Length; i++)
        {
            //lineRenderer[i].SetWidth(0.1f, 0.1f);

            //lineRenderer[i].SetWidth(normalLineWidth, normalLineWidth);
            if (energyFillUp[firstVertex[i]]==true && energyFillUp[secondVertex[i]]==true && activeMaterial[i] == 1)
            {
                //if(!playerMoved[firstVertex[i]] && !playerMoved[secondVertex[i]]){


                lineRenderer[i].SetWidth(normalLineWidth + (healLineWidth / 100.0f * (100 - (float)playerScripts[firstVertex[i]].Energy)), normalLineWidth + (healLineWidth / 100.0f * (100 - (float)playerScripts[secondVertex[i]].Energy)));
               // }                
            }


            else if (energyFillUp[firstVertex[i]] == true  && energyFillUp[secondVertex[i]] == false && activeMaterial[i] == 1)
            {
               // if(!playerMoved[firstVertex[i]]){
                lineRenderer[i].SetWidth(normalLineWidth + healLineWidth / 100.0f * (100 - (float)playerScripts[firstVertex[i]].Energy),normalLineWidth);
                //}
                

            }
            else if (energyFillUp[firstVertex[i]] == false && energyFillUp[secondVertex[i]] == true && activeMaterial[i] == 1)
            {
               // if (playerMoved[secondVertex[i]])
                {
                    lineRenderer[i].SetWidth(normalLineWidth, normalLineWidth + healLineWidth / 100.0f * (100 - (float)playerScripts[secondVertex[i]].Energy));
                }
                
            }
            else if (activeMaterial[i] == 0 ||activeMaterial[i] == 1)
            {
                lineRenderer[i].SetWidth(normalLineWidth, normalLineWidth);
            }


            lineRenderer[i].SetPosition(0, new Vector3(players[firstVertex[i]].transform.position.x, players[firstVertex[i]].transform.position.y+heightOffset, players[firstVertex[i]].transform.position.z));
            lineRenderer[i].SetPosition(1, new Vector3(players[secondVertex[i]].transform.position.x, players[secondVertex[i]].transform.position.y+heightOffset, players[secondVertex[i]].transform.position.z));
        }


        for (int i = 0; i < players.Length; i++)
        {
            //if (healingparticles[i] != null)
            //{
                
            //    if (blinkTime[i] >= pulseTime)
            //    {
            //        blinkTime[i] -= Time.deltaTime;
            //    }
            //    else
            //    {
            //        blinkTime[i] += Time.deltaTime;
            //    }

            //}
            //else
            //{
            //    blinkTime[i] = 0.0f;

            //}

            if (playerHealing[i])
            {
                playerHealing[i] = false;
            }

          
        }
    }


    void UpdateConnectionType()
    {
        for (int i = 0; i < lineRenderer.Length; i++)
        {
            //if (firstVertex[i] == donkey || secondVertex[i] == donkey)
            //{
            //    lineRenderer[i].SetWidth(0.05f, 0.05f);
            //}


            

            if (activeMaterial[i] == 0 && Vector3.Distance(players[firstVertex[i]].transform.position, players[secondVertex[i]].transform.position) < healDistance)
            {
                if(lerpTimes[i] <= 1 ){
                    lerpTimes[i] += lerpSpeed * Time.deltaTime;
                    lineRenderer[i].material.Lerp(mats[0],mats[1], lerpTimes[i]);
                }else{
                    lineRenderer[i].material = mats[1];
                    if (playerHealing[firstVertex[i]])
                    {
                        lineRenderer[i].SetWidth(healLineWidth, normalLineWidth);

                    }


                    if (playerHealing[secondVertex[i]])
                    {
                        lineRenderer[i].SetWidth(normalLineWidth, healLineWidth);
                    }
                    activeMaterial[i] = 1;
                    lerpTimes[i] = 0.0f;
                }              
            }
            else if (activeMaterial[i] == 1 && Vector3.Distance(players[firstVertex[i]].transform.position, players[secondVertex[i]].transform.position) > healDistance) 
            {
                if (lerpTimes[i] <= 1)
                {
                    lerpTimes[i] += lerpSpeed * Time.deltaTime;
                    lineRenderer[i].material.Lerp(mats[1], mats[0], lerpTimes[i]);
                }
                else
                {
                    lineRenderer[i].material = mats[0];
                    activeMaterial[i] = 0;
                    lerpTimes[i] = 0.0f;
                }
                
            }

            if (i == intersectedLines[0] || i == intersectedLines[1])
            {
               // lineRenderer[i].SetWidth(0.05f, 0.05f);
                
            }

        }
    }

    void UpdatePlayerStatus()
    {
        

        players = GameObject.FindGameObjectsWithTag("Player");

        for(int i = 0; i<players.Length; i++){
            playerScripts[i] = players[i].GetComponent<BasePlayer>();
        }

            



        //int childCount = transform.childCount;
        foreach (Transform t in transform)
        {
            GameObject.Destroy(t.gameObject);
        }
        //collider = new GameObject();
        sphere = gameObject.AddComponent<SphereCollider>();
        sphere.enabled = false;
        sphere.isTrigger = true;
        sphere.radius = 8;

        charging = 1.0f;
        oldPlayerPosition = new Vector3[players.Length];

        //enemies = new List<BaseEnemy>();

        lerpTime = 0.0f;
        firstAngles = true;
        poly = new GameObject();
        poly.transform.parent = transform;
        triPoly = new GameObject();
        triPoly.transform.parent = transform;
        //poly.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);

            
           


        
        changedToTri = false;
        changedToQuad = false;
          
        intersection = 0;
        //players = GameObject.FindGameObjectsWithTag("Player");
        oldPlayerPosition = new Vector3[players.Length];
        lineRenderer = new LineRenderer[linesNeeded[players.Length - 1]];
        lerpTimes = new float[linesNeeded[players.Length -1]];
        activeMaterial = new int[linesNeeded[players.Length - 1]];
        intersectedLines = new int[2];
        playerAlignment = new int[players.Length];
        angles = new float[players.Length];
        oldAngles = new float[players.Length];
        intersectedLines[0] = -1;
        intersectedLines[1] = -1;


        for (int i = 0; i < players.Length; i++)
        {
            oldPlayerPosition[i] = players[i].transform.position;


        }


        healingparticles = new GameObject[players.Length];
        incremenetTimers = new float[players.Length];


        for (int i = 0; i < linesNeeded[players.Length - 1]; i++)
        {
            GameObject go = new GameObject();
            lineRenderer[i] = go.AddComponent<LineRenderer>();
            lineRenderer[i].SetVertexCount(2);
            lineRenderer[i].sharedMaterial = mats[0];
            lineRenderer[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            go.transform.parent = this.gameObject.transform;


        }

        CreatePolygon();
        playerCountUpdateNeeded = true;
        
  
    }


    private IEnumerator WaitForSeconds()
    {
        yield return new WaitForSeconds(0.2f);
        sphere.enabled = false;
        

    }
}
