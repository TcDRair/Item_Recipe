using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SampleSelection : MonoBehaviour
{
    public CanvasGroup cg;

    void Start() {
        cg.alpha = 0;
        cg.blocksRaycasts = false;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) Switch();
    }

    bool isActive = false;
    public void Switch() {
        isActive = !isActive;
        cg.alpha = isActive ? 0.75f : 0;
        cg.blocksRaycasts = isActive;
    }
}
