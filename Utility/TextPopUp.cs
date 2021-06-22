using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextPopUp : MonoBehaviour
{
    private static int sortingOrder;

    private float disappearTimer = 0;
    private float disappearTime = 1.5f;
    private float disappearSpeed = 1.5f;

    private bool chatLineCompleate = false;
    private int currentStringInt = 0;
    private float addLetterTimer = 0;
    private float addLetterTime = 0.1f;

    //private float incScaleAmount = 0.5f;
    //private float decScaleAmount = 1f;
    //private float moveZSpeed = 0.8f;

    private SpriteRenderer sr;
    private TextMeshPro textMesh;
    private Color textColor;

    private string chatLine;
    private string currentChatLine;

    public static TextPopUp Create(string text, Vector3 position)
    {
        Transform textPopUp = Instantiate(GameAssets.i.TextPopUp, position, Quaternion.identity);
        textPopUp.transform.rotation = textPopUp.transform.rotation * Quaternion.Euler(90f, 0f, 0f);
        TextPopUp textData = textPopUp.GetComponent<TextPopUp>();
        textData.SetUp(text);

        return textData;
    }

    private void Awake()
    {
        textMesh = transform.GetComponentInChildren<TextMeshPro>();
    }

    public void SetUp(string text)
    {
        sr = this.GetComponent<SpriteRenderer>();
        chatLine = text;
        currentChatLine = "";
        textColor = textMesh.color;

        sortingOrder++;
        textMesh.sortingOrder = sortingOrder;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        //transform.position += new Vector3(0, 0, moveZSpeed) * deltaTime;

        //if(disappearTimer < (disappearTime + disappearSpeed) / 2)
        //{
        //    transform.localScale += Vector3.one * incScaleAmount * deltaTime;
        //}
        //else
        //{
        //    transform.localScale -= Vector3.one * decScaleAmount * deltaTime;
        //}

        if(chatLineCompleate == false)
        {
            addLetterTimer += deltaTime;

            if (addLetterTimer >= addLetterTime)
            {
                addLetterTimer -= addLetterTime;
                if (currentChatLine.Length >= chatLine.Length)
                {
                    chatLineCompleate = true;
                    return;
                }
                else
                {
                    currentChatLine += chatLine[currentStringInt];
                    currentStringInt++;
                    textMesh.SetText(currentChatLine);
                    return;
                }
            }
        }
        else
        {
            disappearTimer += deltaTime;
            if(disappearTimer >= disappearTime) //if true start to fade
            {
                textColor.a -= disappearSpeed * deltaTime;
                sr.color = textColor;
                textMesh.color = textColor;
                if(textColor.a <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
