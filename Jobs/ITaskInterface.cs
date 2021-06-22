using UnityEngine;
using System.Collections;

public interface ITaskInterface
{
    void Execute(NPC dude, float deltaTime);
    void OnComplete(NPC dude);
}
