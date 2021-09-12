using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoardSetting
{
    public int xSize;
    public int ySize;
    public Tile tileGO;
    public List<Sprite> tileSprite;
}

public class GameManager : MonoBehaviour
{
    public BoardSetting boardSetting;

    void Start()
    {
        BoardController.instance.SetValue(BoardScript.instance.SetValue(boardSetting.xSize, boardSetting.ySize, boardSetting.tileGO, boardSetting.tileSprite), boardSetting.xSize, boardSetting.ySize, boardSetting.tileSprite);
    }

    void Update()
    {
    
    }
}
