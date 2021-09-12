using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimerBarScript : MonoBehaviour
{
    Image TimerBar;
    public float Timerz = 60f;

    float timeLeft;

    void Start()
    {
        TimerBar = GetComponent<Image>();
        timeLeft = Timerz;
    }

    void Update()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            TimerBar.fillAmount = timeLeft / Timerz;
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
        }
    }
}