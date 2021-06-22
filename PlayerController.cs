using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Mathematics;

public class PlayerController : MonoBehaviour
{
    public enum MouseMode
    {
        Movement,
        Build,
        Cut
    }
    public MouseMode mouseMode { get; protected set; }
    public string BuildItem;
    Vector3 ghostOffset;
    GameObject ghost;

    #region dragData

    bool isDrag = false;

    Vector3 startDrag;
    Vector3 endDrag;

    Vector3 uiStartDrag;
    Vector3 uiEndDrag;

    #endregion

    public Player player;

    public void SetUp()
    {
        mouseMode = MouseMode.Movement;
    }

    public Player BuildPlayer(Vector2 position)
    {
        GameObject go = Instantiate(GameAssets.i.Shamen);
        go.name = "Brenda";
        GameObject mapIcon = Instantiate(GameAssets.i.MiniMapIcon);
        mapIcon.transform.SetParent(go.transform, false);
        GameAssets.i.CameraRig.transform.SetParent(go.transform, false);
        go.GetComponent<MeshRenderer>().sortingLayerName = "Player";

        CharacterSheet characterSheet = ScriptableObject.CreateInstance<CharacterSheet>();
        characterSheet.Name = "Brenda";
        characterSheet.Speed = 5;

        player = new Player(go, characterSheet);

        go.transform.position = new Vector3(position.x, position.y);

        return player;
    }

    public void FrameUpdate(float deltaTime)
    {
        player.FollowPath(deltaTime);

        if (IsMouseOverUI() == true && mouseMode != MouseMode.Cut)
        {
            return;
        }

        switch (mouseMode)
        {
            case MouseMode.Movement:
                #region Movement
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 click = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    PathJob pathJob = player.SetPathJob(click);

                    WorldController.current.findPathJobsList.Add(pathJob);
                }
                #endregion

                #region Esc
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Debug.Log("Open Esc Menu");
                }
                #endregion
                break;
            case MouseMode.Build:
                #region Build/MoveJobGhost
                if (ghost == null)
                {
                    if(WorldController.current.installedObjectManager.InstalledObjectPrototypes.ContainsKey(BuildItem) == false)
                    {
                        Debug.Log("InstalledObjectProtos dose not contain " + BuildItem);
                        return;
                    }
                    InstalledObject proto = WorldController.current.installedObjectManager.InstalledObjectPrototypes[BuildItem];

                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector3 difference = (new Vector3(proto.Size.x, proto.Size.y) - new Vector3(1, 1, 0));
                    ghostOffset = difference / 2;
                    Vector3 actualPosition = mousePos + ghostOffset;

                    ghost = WorldController.current.installedObjectManager.installedObjectVisuals.CreateMouseGhost(BuildItem, proto, actualPosition, this.transform);
                }
                int2 pos = RoundPositionToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                
                ghost.transform.position = new Vector3(pos.x, pos.y) + ghostOffset;
                #endregion

                #region IfLeftClick
                if (Input.GetMouseButtonDown(0))
                {
                    WorldController.current.PlaceJob(BuildItem, pos);

                    if (Input.GetKey(KeyCode.LeftShift) == false)
                    {
                        Debug.Log("No Shift");

                        Destroy(ghost);
                        ChangeMouseMode(MouseMode.Movement);
                    }
                }
                #endregion

                #region Esc
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Destroy(ghost);
                    ChangeMouseMode(MouseMode.Movement);
                }
                #endregion
                break;
            case MouseMode.Cut:
                #region IfLeftClick
                if (Input.GetMouseButtonDown(0))
                {
                    isDrag = true;
                    uiStartDrag = Input.mousePosition;
                    startDrag = Camera.main.ScreenToWorldPoint(uiStartDrag);
                }
                else if (Input.GetMouseButton(0))
                {
                    if(isDrag == true)
                    {
                        uiEndDrag = Input.mousePosition;

                        UpdateSeletionBox();
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (isDrag == true)
                    {
                        GameAssets.i.selectionBox.gameObject.SetActive(false);

                        if (startDrag == new Vector3(-1, -1, -1))
                        {
                            return;
                        }

                        endDrag = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                        int2 start = WorldController.current.RoundPositionToInt(startDrag);
                        int2 end = WorldController.current.RoundPositionToInt(endDrag);

                        sortDragPositionInts(start, end, out start, out end);

                        for (int x = start.x; x < end.x; x++)
                        {
                            for (int y = start.y; y < end.y; y++)
                            {
                                WorldController.current.PlaceTask(Task.Type.Cut, new int2(x, y));
                            }
                        }
                        isDrag = false;
                    }
                }
                #endregion
                
                #region Esc
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ChangeMouseMode(MouseMode.Movement);
                    startDrag = new Vector3(-1,-1,-1);
                    endDrag = new Vector3(-1, -1, -1);
                    isDrag = false;
                }
                #endregion
                break;
        }
    }

    public void ChangeMouseMode(MouseMode mode)
    {
        if(this.mouseMode == MouseMode.Build && mode != MouseMode.Build)
        {
            WorldController.current.uiManager.jobGhost = null;
        }
        this.mouseMode = mode;
    }

    bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    int2 RoundPositionToInt(Vector3 position)
    {
        return new int2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    void sortDragPositionInts(int2 start , int2 end, out int2 newStart, out int2 newEnd)
    {
        newStart.x = Mathf.Min(start.x, end.x);
        newStart.y = Mathf.Min(start.y, end.y);
        newEnd.x = Mathf.Max(start.x, end.x);
        newEnd.y = Mathf.Max(start.y, end.y);
    }

    void sortDragPositionFloat(float2 start, float2 end, out float2 newStart, out float2 newEnd)
    {
        newStart.x = Mathf.Min(start.x, end.x);
        newStart.y = Mathf.Min(start.y, end.y);
        newEnd.x = Mathf.Max(start.x, end.x);
        newEnd.y = Mathf.Max(start.y, end.y);
    }

    void UpdateSeletionBox()
    {
        if (GameAssets.i.selectionBox.gameObject.activeInHierarchy == false)
        {
            GameAssets.i.selectionBox.gameObject.SetActive(true);
        }

        float2 uiE;
        float2 uiS;

        sortDragPositionFloat(new float2(uiStartDrag.x, uiStartDrag.y), new float2(uiEndDrag.x, uiEndDrag.y), out uiS, out uiE);

        float width = uiE.x - uiS.x;
        float height = uiE.y - uiS.y;

        GameAssets.i.selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        GameAssets.i.selectionBox.position = new Vector3(uiS.x, uiS.y) + new Vector3(width / 2, height / 2);
    }
}
