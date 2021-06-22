using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class PrefabEntities : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public List<GameObject> PrefabObjects;

    public static Dictionary<string,Entity> PrefabEntity;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Debug.Log("PrefabEntities:");
        PrefabEntity = new Dictionary<string, Entity>();

        for (int i = 0; i < PrefabObjects.Count; i++)
        {
            GameObject obj = PrefabObjects[i];
            Entity prefabEntity = conversionSystem.GetPrimaryEntity(obj);
            PrefabEntities.PrefabEntity.Add(obj.name, prefabEntity);
            Debug.Log("-" + obj.name);
        } 
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        for (int i = 0; i < PrefabObjects.Count; i++)
        {
            referencedPrefabs.Add(PrefabObjects[i]);
        }
    }
}
