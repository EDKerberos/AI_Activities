using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    //super hardcoded tutorial
    // this is super temporary
    [SerializeField] GameObject tutorialUI1;
    [SerializeField] GameObject tutorialUI2;
    [SerializeField] GameObject tutorialUI3;

    public void NextPage1()
    {
        tutorialUI1.SetActive(false);
        tutorialUI2.SetActive(true);
    }

    public void NextPage2()
    {
        tutorialUI2.SetActive(false);
        tutorialUI3.SetActive(true);
    }

    public void Start()
    {
        tutorialUI3.SetActive(false);
    }
}
