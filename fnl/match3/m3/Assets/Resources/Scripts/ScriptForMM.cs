using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptForMM : MonoBehaviour
{
    public void Button()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);    
    }
}
