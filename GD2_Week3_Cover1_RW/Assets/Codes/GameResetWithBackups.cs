using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AI;

public class GameResetWithBackups : MonoBehaviour
{
    public List<GameObject> gameObjectsToBackup;  // 需要备份和重置的SelectableObjects
    private List<GameObject> backupObjects;  // 存储备份对象
    public Button resetButton;  // UI 按钮引用

    public CharacterMover characterMover;  // 引用到 CharacterMover

    void Start()
    {
        CreateBackups();  // 游戏开始时创建备份
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetGame);  // 为按钮添加监听器
        }
    }

    void Update()
    {
        // 按下 R 键时进行重置
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
    }

    // 创建重要对象的备份
    void CreateBackups()
    {
        backupObjects = new List<GameObject>();

        foreach (GameObject obj in gameObjectsToBackup)
        {
            GameObject backup = Instantiate(obj, obj.transform.position, obj.transform.rotation);
            backup.SetActive(false);  // 使备份对象处于不活动状态
            backupObjects.Add(backup);
        }
    }

    // 重置游戏，恢复对象到其备份状态，并删除带有 "des" 标签的对象
    void ResetGame()
    {
        // 删除场景中所有带有 "des" 标签的对象
        DestroyDesTaggedObjects();

        // 取消选中所有当前选中的对象
        characterMover.SetSelectedCharacters(new List<SelectableObject>());

        // 停止当前选中的对象的移动
        foreach (GameObject obj in gameObjectsToBackup)
        {
            NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.ResetPath();  // 停止移动
            }
            obj.SetActive(false);  // 禁用当前对象
        }

        // 激活备份对象
        foreach (GameObject backup in backupObjects)
        {
            backup.SetActive(true);
        }

        // 更新对象列表并重新创建备份
        gameObjectsToBackup = new List<GameObject>(backupObjects);
        CreateBackups();
    }

    // 删除带有 "des" 标签的对象
    void DestroyDesTaggedObjects()
    {
        GameObject[] desObjects = GameObject.FindGameObjectsWithTag("des");
        foreach (GameObject desObj in desObjects)
        {
            Destroy(desObj);  // 销毁带有 "des" 标签的对象
        }
    }
}
