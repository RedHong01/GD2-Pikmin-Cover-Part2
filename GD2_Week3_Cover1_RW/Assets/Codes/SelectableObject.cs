using UnityEngine;
using TMPro;
using System.Collections;

public class SelectableObject : MonoBehaviour
{
    private bool isSelected = false;

    public enum ObjectType { Character, Treasure }
    public ObjectType objectType; // 在 Inspector 中设置类型

    // 如果是 Treasure，设置重量
    public int weight = 1;

    // 如果是 Character，表示是否正在搬运
    public bool isCarryingTreasure = false;

    // 当前搬运角色数量
    private int currentCarriers = 0;

    // TMP 组件（在 Inspector 中分配）
    public TMP_Text weightDisplay;

    // 限制搬运半径（仅限角色）
    public float carryRadius = 5f;

    // 被限制移动时的颜色（可在 Inspector 中分配）
    public Color restrictedColor = Color.red;

    // 颤动效果参数
    public float shakeAmplitude = 0.2f; // 颤动幅度
    public float shakeDuration = 0.5f; // 颤动持续时间

    private Color originalColor; // 记录角色的原始颜色
    private Renderer characterRenderer; // 角色的渲染器

    void Start()
    {
        if (objectType == ObjectType.Character)
        {
            characterRenderer = GetComponent<Renderer>();
            if (characterRenderer != null)
            {
                originalColor = characterRenderer.material.color;
            }
        }
    }

    void Update()
    {
        // 如果是 Treasure 类型，更新 TMP 组件显示
        if (objectType == ObjectType.Treasure && weightDisplay != null)
        {
            weightDisplay.text = $"{currentCarriers}/{weight}";
        }
    }

    public void SetIsSelected(bool selected)
    {
        isSelected = selected;
        UpdateSelectionState();
    }

    private void UpdateSelectionState()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Arrow"))
            {
                child.gameObject.SetActive(isSelected);
            }
        }
    }

    // 更新当前搬运角色数量
    public void UpdateCarriers(int carriers)
    {
        currentCarriers = carriers;

        // 如果 TMP 存在，立即更新显示
        if (weightDisplay != null)
        {
            weightDisplay.text = $"{currentCarriers}/{weight}";
        }
    }

    // 检查是否超出搬运半径并触发限制逻辑
    public bool CheckCarryRadius(Vector3 treasurePosition)
    {
        if (Vector3.Distance(transform.position, treasurePosition) > carryRadius)
        {
            TriggerRestrictedMovement();
            return false; // 超出搬运半径，返回 false
        }

        return true; // 在搬运半径内，允许移动
    }

    // 限制移动：变色并触发颤动
    private void TriggerRestrictedMovement()
    {
        if (characterRenderer != null)
        {
            characterRenderer.material.color = restrictedColor; // 变为限制颜色
            StartCoroutine(ShakeEffect()); // 触发颤动
        }
    }

    // 恢复角色颜色
    public void ResetColor()
    {
        if (characterRenderer != null)
        {
            characterRenderer.material.color = originalColor;
        }
    }

    // 颤动效果
    private IEnumerator ShakeEffect()
    {
        Vector3 originalPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            float offsetX = Random.Range(-shakeAmplitude, shakeAmplitude);
            float offsetY = Random.Range(-shakeAmplitude, shakeAmplitude);
            transform.position = new Vector3(originalPosition.x + offsetX, originalPosition.y + offsetY, originalPosition.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 颤动结束，恢复原始位置和颜色
        transform.position = originalPosition;
        ResetColor();
    }
}