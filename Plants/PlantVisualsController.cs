using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantVisualsController : MonoBehaviour
{
    Dictionary<Plant, GameObject> plantGOMap;
    
    public void SetUp()
    {
        plantGOMap = new Dictionary<Plant, GameObject>();
    }

    public GameObject CreatePlant(Plant plant, Vector3 position, Transform parent = null)
    {
        GameObject plantGO = Instantiate(GameAssets.i.GetPlantObject(plant.Name));
        plantGO.name = plant.Name;
        plantGO.transform.position = position;

        plantGO.transform.SetParent(parent, true);

        plantGOMap.Add(plant, plantGO);
        return plantGO;
    }

    public void OnPlantChanged(Plant plant)
    {
        GameObject GO = plantGOMap[plant];
        SpriteRenderer sr = GO.GetComponent<SpriteRenderer>();
        //sr.sprite = SpriteManager.current.GetSprite(SpriteManager.SpriteCatagory.Plants, plant.GetPlantSpriteName());
    }

    public void OnPlantRemoved(Plant plant)
    {
        if(plantGOMap.ContainsKey(plant) == true)
        {
            GameObject go = plantGOMap[plant];

            Destroy(go);
        }
    }
}
