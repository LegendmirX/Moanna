using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIReferences : MonoBehaviour
{
    public static UIReferences i;

    public GameObject BlankBtn;

    [Space]
    [Header("BuildUI")]
    public GameObject BuildOptionsPanel;

    public void SetUp()
    {
        i = this;
    }
}
