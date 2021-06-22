using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstalledObjectVisuals : MonoBehaviour 
{
    Dictionary<InstalledObject, GameObject> installedObjectGOMap;

    public void SetUp()
    {
        installedObjectGOMap = new Dictionary<InstalledObject, GameObject>();
    }

    public GameObject CreateInstalledObject(InstalledObject obj, Vector3 position, Transform parent)
    {
        Vector3 difference = (new Vector3(obj.Size.x, obj.Size.y) - new Vector3(1, 1, 0));
        Vector3 actualPosition = position + (difference / 2);

        GameObject GO = Instantiate(GameAssets.i.GetInstalledObject(obj.Name));
        GO.transform.position = actualPosition;
        GO.transform.SetParent(parent, true);
        GO.transform.localScale = new Vector3(obj.Size.x, obj.Size.y);

        installedObjectGOMap.Add(obj, GO);
        return GO;
    }

    public GameObject CreateMouseGhost(string buildItem, InstalledObject obj, Vector3 position, Transform parent)
    {
        Vector3 difference = (new Vector3(obj.Size.x, obj.Size.y) - new Vector3(1, 1, 0));
        Vector3 actualPosition = position + (difference / 2);

        GameObject GO = Instantiate(GameAssets.i.GetInstalledObject(buildItem));
        GO.transform.position = actualPosition;
        GO.transform.SetParent(parent, true);
        GO.transform.localScale = new Vector3(obj.Size.x, obj.Size.y);

        Color greenShade = Color.green;
        greenShade.a = 0.5f;
        GO.GetComponent<SpriteRenderer>().color = greenShade;

        return GO;
    }

    public void OnInstalledObjectRemoved(InstalledObject obj)
    {
        GameObject go = installedObjectGOMap[obj];
        Destroy(go);
        installedObjectGOMap.Remove(obj);
    }

    public void DestroyGameObject(InstalledObject obj)
    {
        GameObject go = installedObjectGOMap[obj];
        installedObjectGOMap.Remove(obj);
        Destroy(go);
    }
}
