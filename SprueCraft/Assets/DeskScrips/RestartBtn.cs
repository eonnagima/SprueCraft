using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartBtn : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ResetTheScene()
    {

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        print("The button is working.");

    }
}
