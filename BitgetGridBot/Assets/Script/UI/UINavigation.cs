using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UINavigation : MonoBehaviour
{
    [SerializeField]
    public List<UIView> uis;
    public static UINavigation instance;
    private void Awake()
    {
        instance = this;
        InitAll();
        CloseAll();
    }
    void InitAll()
    {
        for (int i = 0; i < uis.Count; i++)
            uis[i].Init();
    }
    public void CloseAll()
    {
        for (int i = 0; i < uis.Count; i++)
            uis[i].Close();
    }
    public UIView GetUIView(string name)
    {
        UIView ui = uis.Find(delegate (UIView a)
        {
            return a.name == name;
        });
        return ui;
    }

    public void PopUIView(string name)
    {
        UIView ui = GetUIView(name);
        ui.Show();
    }
}
