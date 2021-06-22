using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class TextMeshUtil : MonoBehaviour
{
    public static TextMeshPro CreateWorldText   (
                                                string text, 
                                                Transform parent = null, 
                                                Vector3 localPosition = default(Vector3),
                                                Vector3 localRotation = default(Vector3), 
                                                int fontSize = 40, Color color = default, 
                                                TextAlignmentOptions textAlignment = TextAlignmentOptions.Center, 
                                                string sortingLayerName = null
                                                )
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        transform.Rotate(localRotation);
        TextMeshPro textMesh = gameObject.GetComponent<TextMeshPro>();
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        gameObject.GetComponent<Renderer>().sortingLayerName = sortingLayerName;
        return textMesh;
    }
}
