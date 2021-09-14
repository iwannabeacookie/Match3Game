using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FailAnimationScript : MonoBehaviour
{
    public void UnInstantiateAnimation()
    {
        Destroy(gameObject);
    }

    IEnumerator WaitObj()
    {
        yield return new WaitForSeconds(2f);
        UnInstantiateAnimation();
    }

    void Start()
    {
        StartCoroutine(WaitObj());
    }
}
