using UnityEngine;
using UnityEngine.SceneManagement;  // 引入 SceneManager
using UnityEngine.UI;  // 引入 UI 组件

public class SceneSwitcher : MonoBehaviour
{
    // 场景名称
    public string sceneName;

    // 按钮的引用
    public Button switchSceneButton;

    void Start()
    {
        // 为按钮添加点击事件监听器
        switchSceneButton.onClick.AddListener(SwitchScene);
    }

    // 切换场景方法
    public void SwitchScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);  // 切换到指定的场景
        }
        else
        {
            Debug.LogWarning("Scene name is empty or null!");
        }
    }
}
