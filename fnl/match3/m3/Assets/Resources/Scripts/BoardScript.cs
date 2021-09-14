using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour
{
    public static BoardScript instance;

    private int xSize;
    private int ySize;
    private Tile tileGO;
    private List<Sprite> tileSprite = new List<Sprite>();
    
    private void Awake()
    {
        instance = this;
    }

    public Tile[,] SetValue(int xSize, int ySize, Tile tileGO, List<Sprite> tileSprite)
    {
        this.xSize = xSize;
        this.ySize = ySize;
        this.tileGO = tileGO;
        this.tileSprite = tileSprite;

        return CreateBoard();
    }

    private Tile[,] CreateBoard()
    {
        Tile[,] tileArray = new Tile[xSize, ySize];
        float xPos = transform.position.x;
        float yPos = transform.position.y;
        Vector2 tileSize = tileGO.spriteRenderer.bounds.size;

        Sprite remSprite = null;
        
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                Tile newTile = Instantiate(tileGO, transform.position, Quaternion.identity);
                newTile.transform.position = new Vector3(xPos + (tileSize.x * x), yPos + (tileSize.y * y) , 0);
                newTile.transform.parent = transform;
                newTile.spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                newTile.gameObject.layer = LayerMask.NameToLayer("TileLayer");

                if (y == ySize - 1)
                {
                    newTile.gameObject.tag = "TopRowTile";
                }

                tileArray[x, y] = newTile;

                List<Sprite> tempSprite = new List<Sprite>();
                tempSprite.AddRange(tileSprite);

                tempSprite.Remove(remSprite);
                if(x > 0)
                {
                    tempSprite.Remove(tileArray[x - 1, y].spriteRenderer.sprite);
                }
                newTile.spriteRenderer.sprite = tempSprite[Random.Range(0, tempSprite.Count)];
                remSprite = newTile.spriteRenderer.sprite;
            }
        }

        return tileArray;

    }
}
