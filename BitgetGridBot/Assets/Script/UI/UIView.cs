using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UIView : MonoBehaviour
{
    public string parent;
    public string name;
    public GameObject ui;
    public virtual void Init()
    {

    }
    public virtual void Show()
    {
        if (ui != null) ui.SetActive(true);
    }
    public virtual void Close()
    {
        if (ui != null) ui.SetActive(false);
    }
}
