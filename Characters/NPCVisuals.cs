using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class NPCVisuals : MonoBehaviour
{
    EntityManager entityManager;

    Dictionary<NPC, GameObject> dudeGameObjects;

    public void SetUp(EntityManager entityManager)
    {
        dudeGameObjects = new Dictionary<NPC, GameObject>();
        //TODO: these need to be game objects for now
        this.entityManager = entityManager;
    }

    public GameObject BuildVisuals(NPC script, int2 position)
    {
        GameObject go = Instantiate(GameAssets.i.NPC);
        go.name = script.characterSheet.Name;
        go.transform.position = new Vector3(position.x, position.y);
        
        dudeGameObjects.Add(script, go);

        return go;
    }
}
