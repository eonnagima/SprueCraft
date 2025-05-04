using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialOpener : MonoBehaviour
{
    public GameObject TutorialScreen;

    private void Start()
    {
        if(TutorialScreen != null)
        {
            TutorialScreen.SetActive(false);
        }
    }

    public void OpenTutorial()
    {
        if (TutorialScreen != null)
        {

            TutorialScreen.SetActive(true);

            print("Button is working.");
        }
        else
        {
            print("TutorialScreen is not assigned");
        }
    }

    public void CloseTutorial()
    {
        if (TutorialScreen != null)
        {
            TutorialScreen.SetActive(false);
            print("Tutorial screen is now hidden");
        }
    }
}
