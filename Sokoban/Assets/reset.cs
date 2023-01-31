using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class reset : MonoBehaviour
{
    void Update()
        //[R]を押すとリスタート
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Sokoban");
        }
    }

    public void OnClick()
        //restartボタンを押したらリスタート
    {
        SceneManager.LoadScene("Sokoban");
    }
}
