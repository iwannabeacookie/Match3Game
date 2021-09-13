using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoardController : MonoBehaviour
{
    public static BoardController instance;

    private int xSize, ySize;
    private List<Sprite> tileSprite = new List<Sprite>();
    private Tile[,] tileArray;
    public GameObject instPref;
    public GameObject failAnim;
    private float TimeRemain;

    private Tile oldSelectTile;
    private Vector2[] dirRay = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    private Sprite IncrementSprite;

    private bool isFindMatch = false;

    private int currentAnimationTile = 0;
    private List<Tile> animationTiles = new List<Tile>();

    public void SetValue(Tile[,] tileArray, int xSize, int ySize, List<Sprite> tileSprite)
    {
        this.xSize = xSize;
        this.ySize = ySize;
        this.tileSprite = tileSprite;
        this.tileArray = tileArray;
    }

    public void Awake()
    {
        instance = this;
    }

    void Start()
    {
        IncrementSprite = Resources.Load<Sprite>("Sprites/tromb-1 42");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D ray = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
            if (ray != false)
            {
                CheckSelectTile(ray.collider.gameObject.GetComponent<Tile>());
            }
        }

        SearchEmptyTile();

        ComboMonitoringAlgorithm();
    }

    private void SelectTile(Tile tile)
    {
        tile.isSelected = true;
        tile.spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
        oldSelectTile = tile;
    }
    private void DeselectTile(Tile tile)
    {
        tile.isSelected = false;
        tile.spriteRenderer.color = new Color(1, 1, 1);
        oldSelectTile = null;
    }
    private void CheckSelectTile(Tile tile)
    {
        if (tile.isEmpty)
        {
            return;
        }
        if (tile.isSelected)
        {
            DeselectTile(tile);
        }
        else
        {
            if (!tile.isSelected && oldSelectTile == null)
            {
                SelectTile(tile);
            }
            else
            {
                if (AdjacentTiles().Contains(tile))
                {
                    SwapTwoTiles(tile);
                    FindAllMatch(tile);
                    DeselectTile(oldSelectTile);
                }
                else
                {
                    DeselectTile(oldSelectTile);
                    SelectTile(tile);
                }
            }
        }
    }

    private void destroyAllAnimationTiles()
    {
        if (animationTiles.Count != 0)
        {
            foreach (Tile animationTile in animationTiles)
            {
                Destroy(animationTile);
            }
        }
    }

    private IEnumerator falling(Tile topTile, Tile bottomTile, int bottomTileY, int x)
    {
        //animationTiles.Add(Instantiate(topTile, topTile.transform.position, Quaternion.identity));
        //Tile animationTile = animationTiles[animationTiles.Count - 1];
        Tile animationTile = Instantiate(topTile, topTile.transform.position, Quaternion.identity);
        animationTile.gameObject.layer = LayerMask.NameToLayer("Igore Raycast");

        while (Vector3.Distance(animationTile.transform.position, bottomTile.transform.position) > 0.001f)
        {
            animationTile.transform.position = Vector3.MoveTowards(animationTile.transform.position, bottomTile.transform.position, (animationTile.transform.position.y -
                bottomTile.transform.position.y) / 20);
            yield return null;
        }

        Destroy(animationTile);
        tileArray[x, bottomTileY].spriteRenderer.enabled = true;
    }

    private void animateFall(Tile topTile, int topTileY, Tile bottomTile, int bottomTileY, int x)
    {
        tileArray[x, topTileY].spriteRenderer.sprite = null;

        tileArray[x, bottomTileY].spriteRenderer.enabled = false;

        StartCoroutine(falling(topTile, bottomTile, bottomTileY, x));

        tileArray[x, bottomTileY].spriteRenderer.sprite = topTile.spriteRenderer.sprite;
    }

    private List<Tile> FindMatch(Tile tile, Vector2 dir)
    {
        List<Tile> cashFindTiles = new List<Tile>();
        RaycastHit2D hit = Physics2D.Raycast(tile.transform.position, dir);
        while (hit.collider != null && hit.collider.gameObject.GetComponent<Tile>().spriteRenderer.sprite == tile.spriteRenderer.sprite)
        {
            cashFindTiles.Add(hit.collider.gameObject.GetComponent<Tile>());
            hit = Physics2D.Raycast(hit.collider.gameObject.transform.position, dir);
        }
        return cashFindTiles;
    }

    private void DeleteSprite(Tile tile, Vector2[] dirArray)
    {
        List<Tile> cashFindSprite = new List<Tile>();
        for (int i = 0; i < dirArray.Length; i++)
        {
            cashFindSprite.AddRange(FindMatch(tile, dirArray[i]));
        }

        if (cashFindSprite.Count >= 2)
        {
            for (int i = 0; i < cashFindSprite.Count; i++)
            {
                cashFindSprite[i].spriteRenderer.sprite = null;
            }
            isFindMatch = true;
        }
    }

    public void InstantiateAnimation()
    {
        instPref = Instantiate(failAnim, failAnim.transform.position, Quaternion.identity);
    }

    public void UnInstantiateAnimation()
    {
        Destroy(instPref);
    }

    IEnumerator WaitObj()
    {
        InstantiateAnimation();
        yield return new WaitForSeconds(2f);
        UnInstantiateAnimation();
    }

    private void FindAllMatch(Tile tile)
    {
        if (tile.isEmpty)
        {
            return;
        }

        DeleteSprite(tile, new Vector2[2] { Vector2.up, Vector2.down });
        DeleteSprite(tile, new Vector2[2] { Vector2.left, Vector2.right });

        if (isFindMatch)
        {
            isFindMatch = false;

            if (tile.GetComponent<SpriteRenderer>().sprite == IncrementSprite)
            {
                ScoreScript.instance.IncreaseScoreCount();
                ScoreScript.instance.ShowScore();
            }
            else
            {
                StartCoroutine(WaitObj());
            }

            tile.spriteRenderer.sprite = null;
        }
    }

    private void SwapTwoTiles(Tile tile)
    {
        if (oldSelectTile.spriteRenderer.sprite == tile.spriteRenderer.sprite)
        {
            return;
        }
        else
        {
            Sprite spareTile = oldSelectTile.spriteRenderer.sprite;

            oldSelectTile.spriteRenderer.sprite = tile.spriteRenderer.sprite;
            tile.spriteRenderer.sprite = spareTile;
        }
    }

    private List<Tile> AdjacentTiles()
    {
        List<Tile> cashTiles = new List<Tile>();
        for (int i = 0; i < dirRay.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(oldSelectTile.transform.position, dirRay[i]);
            if (hit.collider != null)
            {
                cashTiles.Add(hit.collider.gameObject.GetComponent<Tile>());
            }
        }
        return cashTiles;
    }

    private void SearchEmptyTile()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (tileArray[x, y].isEmpty)
                {
                    ShiftTileDown(x, y);
                    break;
                }
            }
        }
    }

    private void ShiftTileDown(int xPos, int yPos)
    {
        int y = yPos;

        if (yPos != ySize - 1)
        {
            RaycastHit2D hit = Physics2D.Raycast(tileArray[xPos, yPos].transform.position, Vector2.up);
            y++;

            if (hit.collider != null)
            {
                while (hit.collider.gameObject.GetComponent<SpriteRenderer>().sprite == null)
                {
                    hit = Physics2D.Raycast(hit.collider.gameObject.transform.position, Vector2.up);
                    y++;
                }

                animateFall(hit.collider.gameObject.GetComponent<Tile>(), y, tileArray[xPos, yPos], yPos, xPos);
            }

            //destroyAllAnimationTiles();
        }
        else
        {
            GenerateNewSprite(xPos);
        }
    }

    private void GenerateNewSprite(int xPos)
    {
        if (tileArray[xPos, ySize - 1].isEmpty)
        {
            List<Sprite> cashSpriteList = new List<Sprite>(tileSprite);

            Vector2[] vectors = new Vector2[3] { Vector2.down, Vector2.left, Vector2.right };

            for (int i = 0; i < vectors.Length; i++)
            {
                RaycastHit2D hit = Physics2D.Raycast(tileArray[xPos, ySize - 1].transform.position, vectors[i]);

                if (hit.collider != null)
                {
                    cashSpriteList.Remove(hit.collider.gameObject.GetComponent<Tile>().spriteRenderer.sprite);
                }
            }

            tileArray[xPos, ySize - 1].spriteRenderer.sprite = cashSpriteList[Random.Range(0, cashSpriteList.Count)];
        }
    }

    private void ComboMonitoringAlgorithm()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                Tile tile = tileArray[x, y];

                FindAllMatch(tile);
            }
        }
    }
}

