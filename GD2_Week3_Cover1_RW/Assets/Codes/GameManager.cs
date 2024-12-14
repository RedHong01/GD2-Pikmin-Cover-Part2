using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // 倒计时（在Inspector中设置）
    public float gameDuration = 60f;

    // TextMeshPro 对象，用于显示倒计时
    public TMP_Text countdownText;

    // 场景名称
    public string winSceneName;
    public string loseSceneName;

    // 所有 GoalTrigger 引用
    public List<GoalTrigger> goalTriggers;

    // 游戏是否结束
    private bool gameEnded = false;

    void Start()
    {
        // 开始倒计时
        StartCoroutine(GameCountdown());
    }

    void Update()
    {
        if (!gameEnded)
        {
            CheckAllGoalsMet();
        }
    }

    // 检查是否所有 GoalTrigger 都满足条件
    void CheckAllGoalsMet()
    {
        foreach (var goalTrigger in goalTriggers)
        {
            if (!goalTrigger.AreAllAssignedObjectsInTrigger())
            {
                return;  // 如果有一个未满足条件，直接返回
            }
        }

        // 如果所有目标区域都满足条件
        SwitchScene(true);
    }

    // 倒计时功能
    IEnumerator GameCountdown()
    {
        float timeRemaining = gameDuration;
        while (timeRemaining > 0)
        {
            // 更新 UI 文本
            countdownText.text = $"Time Left: {timeRemaining:F1} seconds";

            yield return new WaitForSeconds(1f);
            timeRemaining -= 1f;
        }

        // 倒计时结束，如果未满足目标，切换到lose场景
        if (!gameEnded)
        {
            SwitchScene(false);
        }
    }

    // 切换场景，参数 isWin 用于判断是切换到 win 场景还是 lose 场景
    void SwitchScene(bool isWin)
    {
        if (!gameEnded)
        {
            gameEnded = true;
            if (isWin)
            {
                SceneManager.LoadScene(winSceneName);  // 切换到 win 场景
            }
            else
            {
                SceneManager.LoadScene(loseSceneName);  // 切换到 lose 场景
            }
        }
    }
}
