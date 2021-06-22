using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject jobGhost;

    public void SetUp()
    {
        GameObject panel = UIReferences.i.BuildOptionsPanel;
        foreach(string item in WorldController.current.installedObjectManager.InstalledObjectPrototypes.Keys)
        {
            GameObject obj = Instantiate(UIReferences.i.BlankBtn);
            obj.transform.SetParent(panel.transform, false);
            obj.name = item + "Btn";
            obj.GetComponentInChildren<Text>().text = item;
            Button btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                WorldController.current.playerController.ChangeMouseMode(PlayerController.MouseMode.Build);
                WorldController.current.playerController.BuildItem = item;
            });
        }
        panel.SetActive(false);
    }

    public void OnBuildBTNClicked()
    {
        UIReferences.i.BuildOptionsPanel.SetActive(true);
    }

    public void OnActionsBTNClicked()
    {
        WorldController.current.playerController.ChangeMouseMode(PlayerController.MouseMode.Cut);
    }
}
