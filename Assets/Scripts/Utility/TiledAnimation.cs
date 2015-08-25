using UnityEngine;
using System.Collections;

public class TiledAnimation : MonoBehaviour
{

	//vars for the whole sheet
	public int colCount =  8;
	public int rowCount =  8;

	//vars for animation
	public int  rowNumber  =  0; //Zero Indexed
	public int colNumber = 0; //Zero Indexed
	public int totalCells = 64;
	public int  fps = 20;

	private Vector2 offset;
	private Renderer r;

    // Size of every cell
    float sizeX;
    float sizeY;
    Vector2 size;

    int uIndex;
    int vIndex;

	void Start () 
    {
		r = this.GetComponent< Renderer >();

        sizeX = 1.0f / colCount;
        sizeY = 1.0f / rowCount;

        size = new Vector2(sizeX, sizeY);
	}

	//Update
	void Update () 
    {
        SetSpriteAnimation(colCount,rowCount,rowNumber,colNumber,totalCells,fps); 
    }

	//SetSpriteAnimation
	void SetSpriteAnimation(int colCount ,int rowCount ,int rowNumber ,int colNumber,int totalCells,int fps ){

	  // Calculate index
	  int index  = (int) (Time.time * fps);
	  // Repeat when exhausting all cells
	  index = index % totalCells;

	  // Size of every cell
      //float sizeX = 1.0f / colCount;
      //float sizeY = 1.0f / rowCount;
	  //Vector2 size =  new Vector2(sizeX, sizeY);

	  // split into horizontal and vertical index
	  uIndex = index % colCount;
	  vIndex = index / colCount;

	  // build offset
	  // v coordinate is the bottom of the image in opengl so we need to invert.
	  float offsetX = (uIndex + colNumber) * size.x;
	  float offsetY = (1.0f - size.y) - (vIndex + rowNumber) * size.y;
	  Vector2 offset = new Vector2(offsetX, offsetY);

	  r.material.SetTextureOffset ("_MainTex", offset);
	  r.material.SetTextureScale  ("_MainTex", size);
	}
}
