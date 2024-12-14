using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    // 在Unity Inspector中设置该Obstacle的类型
    public string obstacleType; // "Water", "Fire", or "Electric"

    void Start()
    {
        // 将障碍物的tag设置为它的类型
        SetObstacleTagBasedOnType();
    }

    private void SetObstacleTagBasedOnType()
    {
        if (obstacleType == "Water")
        {
            gameObject.tag = "Water";
            // 你可以在这里设置水障碍物的具体表现，例如外观或碰撞体
        }
        else if (obstacleType == "Fire")
        {
            gameObject.tag = "Fire";
            // 设置火障碍物的表现
        }
        else if (obstacleType == "Electric")
        {
            gameObject.tag = "Electric";
            // 设置电障碍物的表现
        }
        else
        {
            Debug.LogError("Unrecognized obstacle type!");
        }
    }
}
