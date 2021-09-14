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

    static GameObject instPref;
    public GameObject failAnim;

    public GameObject[] TrombArray;
    public int CurrentTromb = 0;
    public GameObject Canvas;

    private float TimeRemain;

    private Tile oldSelectTile;
    private Vector2[] dirRay = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    private Sprite IncrementSprite;

    private bool isFindMatch = false;

    private bool draggingTile = false;

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
        if ((Input.GetMouseButtonDown(0)) && (!draggingTile))
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

    private IEnumerator dragTile(Tile tile, Tile initialTile)
    {
        while (Input.GetMouseButton(0))
        {
            tile.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            yield return null;
        }

        Destroy(tile.gameObject);

        RaycastHit2D ray = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));

        if (ray.collider != null)
        {
            if (ray.collider.gameObject.GetComponent<Tile>() != null && AdjacentTiles().Contains(ray.collider.gameObject.GetComponent<Tile>()))
            {
                SwapTwoTiles(ray.collider.gameObject.GetComponent<Tile>());
            }
        }

        initialTile.spriteRenderer.enabled = true;
        DeselectTile(initialTile);
    }

    private void CheckSelectTile(Tile tile)
    {
        if (tile.isEmpty)
        {
            return;
        }

        Tile dragAnimationTile = Instantiate(tile, tile.transform.position, Quaternion.identity);
        dragAnimationTile.transform.SetParent(GameObject.Find("Board").transform, false);
        dragAnimationTile.spriteRenderer.sortingLayerName = "AnimationSprites";
        dragAnimationTile.spriteRenderer.color = new Color(1, 1, 1);
        dragAnimationTile.gameObject.tag = "AnimationSprite";

        tile.spriteRenderer.enabled = false;
        SelectTile(tile);

        StartCoroutine(dragTile(dragAnimationTile, tile));
    }

    /*private void CheckSelectTile(Tile tile)
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
    */
    private IEnumerator animateFall(Tile topTile, int topTileY, Tile bottomTile, int bottomTileY, int x)
    {
        Tile animationTile = Instantiate(topTile, topTile.transform.position, Quaternion.identity);
        animationTile.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        animationTile.gameObject.tag = "AnimationSprite";

        tileArray[x, bottomTileY].spriteRenderer.sprite = topTile.spriteRenderer.sprite;

        tileArray[x, topTileY].spriteRenderer.sprite = null;

        tileArray[x, bottomTileY].spriteRenderer.enabled = false;

        //Destroy(animationTile.gameObject, 0.5f); // t = 3f (?)
        while (Vector3.Distance(animationTile.transform.position, bottomTile.transform.position) > 0.01f)
        {
            animationTile.transform.position = Vector3.MoveTowards(animationTile.transform.position, bottomTile.transform.position,
                Vector3.Distance(tileArray[0, 0].transform.position, tileArray[0, ySize - 1].transform.position) / 40); // 20 for build | 200 for dev
            yield return null;
        }

        Destroy(animationTile.gameObject);
        tileArray[x, bottomTileY].spriteRenderer.enabled = true;
    }

    private List<Tile> FindMatch(Tile tile, Vector2 dir)
    {
        List<Tile> cashFindTiles = new List<Tile>();
        RaycastHit2D hit = Physics2D.Raycast(tile.transform.position, dir);
        while (hit.collider != null && hit.collider.gameObject.GetComponent<Tile>().spriteRenderer.sprite == tile.spriteRenderer.sprite
            && hit.collider.gameObject.tag != "AnimationSprite" && hit.collider.gameObject.tag != "TopRowTile")
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

    IEnumerator WaitObj()
    {
        InstantiateAnimation();
        yield return new WaitForSeconds(2f);
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
                GameObject tromb = Instantiate(TrombArray[CurrentTromb], TrombArray[CurrentTromb].transform.position, Quaternion.identity);
                tromb.transform.parent = Canvas.transform;
                CurrentTromb += 1;
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

        if (tileArray[xPos, ySize - 1].isEmpty)
        {
            GenerateNewSprite(xPos);
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(tileArray[xPos, yPos].transform.position, Vector2.up);
            y++;

            if (hit.collider != null)
            {
                Debug.Log(hit.collider);
                while (hit.collider.gameObject.GetComponent<SpriteRenderer>().sprite == null)
                {
                    hit = Physics2D.Raycast(hit.collider.gameObject.transform.position, Vector2.up);
                    y++;
                }

                StartCoroutine(animateFall(hit.collider.gameObject.GetComponent<Tile>(), y, tileArray[xPos, yPos], yPos, xPos));
            }
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
        else
        {
            Debug.Log("ERROR: trying to generate top sprite though it exists");
        }
    }

    private void ComboMonitoringAlgorithm()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize - 1; y++)
            {
                Tile tile = tileArray[x, y];

                FindAllMatch(tile);
            }
        }
    }
}

