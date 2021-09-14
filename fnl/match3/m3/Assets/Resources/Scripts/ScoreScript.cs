using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class ScoreScript : MonoBehaviour
{
    public static ScoreScript instance;

    Text ScoreText;

    public GameObject Canvas;
    public int ScoreCount = 0;
    private int increment = 3;
    private int decrement = 0;

    public GameObject SosudInst;

    IEnumerator WaitObj()
    {
        GameObject Anim = Instantiate(SosudInst, SosudInst.transform.position, Quaternion.identity);
        Anim.transform.parent = Canvas.transform;
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void Awake()
    {
        instance = this;
        ScoreText = GameObject.Find("SCORE").GetComponent<Text>();
    }

    public void IncreaseScoreCount()
    {
        ScoreCount += increment;
    }

    public void DecreaseScoreCount()
    {
        ScoreCount -= decrement;
    }

    public void ShowScore()
    {
        ScoreText.text = ScoreCount.ToString();

        if (ScoreCount == 15)
        { 
            StartCoroutine(WaitObj());
        }
    }
}
