using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    private GridManager gridMgr = new GridManager();
    public GameObject obj;
    public Transform parent;
    public GameObject[][] imgs;
    [FormerlySerializedAs("agent")] public GameObject agentPrefab;

    private Queue<Agent> agentQueue = new Queue<Agent>();
    private List<Grid> _path;
    private float spawnAgentTimer = 3;

    public LineRenderer lineRender;

    void Start()
    {
        gridMgr.Init();
        genGrid();
    }

    private void genGrid()
    {
        float x = -(Screen.width * 0.5f + 32 * 0.5f);
        float y = Screen.height * 0.5f + 32 * 0.5f;
        imgs = new GameObject[gridMgr.dijkstraGrid.Length][];
        for (int i = 0; i < gridMgr.dijkstraGrid.Length; i++)
        {
            GameObject[] temp = new GameObject[gridMgr.dijkstraGrid[i].Length];
            for (int j = 0; j < gridMgr.dijkstraGrid[i].Length; j++)
            {
                Grid grid = gridMgr.dijkstraGrid[i][j];
                GameObject gridObj = Instantiate(obj, parent);
                gridObj.GetComponent<RectTransform>().sizeDelta = new Vector2(32, 32);
                gridObj.transform.localPosition = new Vector3(x + i * 32, y - j * 32);
                Transform lockObj = gridObj.transform.Find("lock");
                if (grid.distance == int.MaxValue)
                {
                    gridObj.GetComponent<Image>().color = Color.red;
                }
                else
                {
                    gridObj.transform.Find("Text").GetComponent<Text>().text =
                        grid.distance == -1 ? "null" : "" + grid.distance;
                    if (grid.distance != -1)
                    {
                        Vector2 dir = gridMgr.flowField[i][j];

                        // double angle = Math.Atan2(dir.y, dir.x) * 180 / Math.PI;
                        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, dir);

                        lockObj.gameObject.SetActive(true);
                        lockObj.rotation = rotation;
                    }
                    else
                    {
                        lockObj.gameObject.SetActive(false);
                    }
                }

                temp[j] = gridObj;
            }

            imgs[i] = temp;
        }
    }


    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyUp(KeyCode.Space))
        // {
        //     for (int i = 0; i < imgs.Length; i++)
        //     {
        //         for (int j = 0; j < imgs[i].Length; j++)
        //         {
        //             Destroy(imgs[i][j]);
        //         }
        //     }
        //
        //     gridMgr.Init();
        //     genGrid();
        // }

        // if (Input.GetKeyUp(KeyCode.A))
        // {
        //     Debug.Log("抬起A键");
        //     _path = gridMgr.generatePathFromDijkstraGrid();
        //     if (_path != null)
        //         for (int i = 0; i < _path.Count; i++)
        //         {
        //             Grid grid = _path[i];
        //             imgs[grid.x][grid.y].GetComponent<Image>().color = Color.green;
        //         }
        //
        //     spawnAgentTimer = 3;
        // }

        // if (_path != null && _path.Count > 0)
        // {
        //     if (spawnAgentTimer >= UnityEngine.Random.Range(0.5f, 3f))
        //     {
        //         spawnAgentTimer = 0;
        //         GameObject agentObj = Instantiate(agentPrefab, parent);
        //         Agent agent = agentObj.GetComponent<Agent>();
        //         agent.SetMvoe(_path, gridMgr.flowField);
        //         agentQueue.Enqueue(agent);
        //     }
        //
        //     spawnAgentTimer += Time.deltaTime;
        // }
    }

    public Vector3 myscreenToworld(Vector3 mousepos)
    {
        //计算是节点，需要知道处置屏幕的投影距离
        Vector3 worldpos = Camera.main.ScreenToWorldPoint(new Vector3(mousepos.x, mousepos.y, 10));
        return worldpos;
    }

    private void OnDrawGizmos()
    {
        // if (gridMgr.dijkstraGrid != null && gridMgr.dijkstraGrid.Length > 0)
        // {
        //     lineRender.SetPosition(0, gridMgr.dijkstraGrid[0][0].position);
        //     lineRender.SetPosition(1, gridMgr.dijkstraGrid[0][1].position);
        // }

        // for (x = 0; x < gridWidth; x++) {
        //     for (y = 0; y < gridWidth; y++) {
        //         if (flowField[x][y]) {
        //             var f = flowField[x][y];
        //             flowFieldShape.graphics.moveTo(x * gridPx, y * gridPx);
        //             flowFieldShape.graphics.lineTo((x + 0.5 * f.x) * gridPx, (y + 0.5 * f.y) * gridPx);
        //         }
        //     }
        // }

        // for (int i = 0; i < gridMgr.flowField.Length; i++)
        // {
        //     for (int j = 0; j < gridMgr.flowField[i].Length; j++)
        //     {
        //         var f = gridMgr.flowField[i][j];
        //         Gizmos.DrawLine();
        //     }
        // }
    }
}