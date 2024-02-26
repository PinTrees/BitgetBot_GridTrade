using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_Debug : UIView
{
    public TextMeshProUGUI oderStartDebug;
    public TextMeshProUGUI liveOL_txt;
    public TextMeshProUGUI liveCL_txt;
    public TextMeshProUGUI newCL_txt;
    public TextMeshProUGUI filledOL_txt;
    public TextMeshProUGUI time;
    public TextMeshProUGUI updateTime;
    public TextMeshProUGUI rebound;
    public TextMeshProUGUI system;

    void Start()
    {
        Init();
        Show();
    }
    public override void Init()
    {
        base.Init();
    }
    public override void Show()
    {
        base.Show();
    }
    public void Display_OrderStart(string messege)
    {
        oderStartDebug.text = messege;
    }
    public void Display_Live_OL(string messege)
    {
        oderStartDebug.text = messege;
    }
}
