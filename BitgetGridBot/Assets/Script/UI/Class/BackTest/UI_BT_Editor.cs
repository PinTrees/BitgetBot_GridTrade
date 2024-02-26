using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UI_BT_Editor : UIView
{
    public TMP_InputField start;
    public TMP_InputField exite;

    public BackTextSystem backText;
    public void Start()
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
    public override void Close()
    {
        base.Close();
    }
    public void SET_Setting()
    {
        DateTime s, e;

        string[] str_s = "2018/01/01".Split('/');
        string[] str_e = "2018/12/31".Split('/');

        //string[] str_s = start.text.Split('/');
        //string[] str_e = exite.text.Split('/');
        s = new DateTime(int.Parse(str_s[0]), int.Parse(str_s[1]), int.Parse(str_s[2]), 0, 0, 0);
        e = new DateTime(int.Parse(str_e[0]), int.Parse(str_e[1]), int.Parse(str_e[2]), 0, 0, 0);
        backText.SET_StartTime(s, e);
    }
    public void TR_Start_BackTest()
    {
        backText.StartBackTest();
    }
}
