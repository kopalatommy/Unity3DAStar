using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraUI : MonoBehaviour
{
    public static CameraUI instance;

    public Slider progressBar;
    public Text progressText;

    private void Awake()
    {
        instance = this;
        progressText.text = "0/10000";
    }

    IEnumerator handleUI()
    {
        while (GameManager.manager.gameInProgress)
        {
            progressBar.value = 0;
            yield return null;
        }
    }
}
