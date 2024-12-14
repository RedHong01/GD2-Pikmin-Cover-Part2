using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ObjectSelecter : MonoBehaviour
{
    public CharacterMover characterMover; // 引用 CharacterMover 脚本
    private List<SelectableObject> selectedObjects = new List<SelectableObject>(); // 当前选中的对象列表
    private SelectableObject selectedTreasure = null; // 当前选中的宝物

    public LayerMask selectableLayerMask; // 可选择的对象层
    public LayerMask groundLayerMask;    // 地面层
    [Header("Restricted Movement Settings")]
    public Color restrictedColor = Color.red; // 限制时的颜色
    public float carryRadius = 5f;            // 搬运半径
    public float shakeAmplitude = 0.1f;       // 颤动幅度
    public float shakeDuration = 0.5f;        // 颤动持续时间

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick(); // 左键点击逻辑
            UpdateTreasureWeightDisplay();
        }

        if (Input.GetMouseButtonDown(1))
        {
            DeselectAllObjects(); // 右键清除所有选中对象
            UpdateTreasureWeightDisplay();
        }
    }

    void HandleLeftClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, selectableLayerMask))
        {
            SelectableObject selectable = hit.transform.GetComponentInParent<SelectableObject>();

            if (selectable != null)
            {
                if (selectable.objectType == SelectableObject.ObjectType.Treasure)
                {
                    SelectTreasure(selectable);
                    UpdateTreasureWeightDisplay();
                }
                else if (selectable.objectType == SelectableObject.ObjectType.Character)
                {
                    SelectCharacter(selectable);
                    
                    UpdateTreasureWeightDisplay();
                }
            }
        }
    }

    void SelectTreasure(SelectableObject treasure)
    {
        DeselectAllObjects(); // 清除之前的选择

        selectedTreasure = treasure; // 设为当前选中的宝物
        treasure.SetIsSelected(true); // 设置选中状态
        selectedObjects.Add(treasure); // 添加到选中列表

        Debug.Log($"Treasure selected: {treasure.name} (Weight: {treasure.weight})");
        UpdateTreasureWeightDisplay();
    }

    void SelectCharacter(SelectableObject character)
{
    if (selectedTreasure != null)
    {
        // 如果宝物被选中，尝试绑定角色
        if (!character.isCarryingTreasure && !selectedObjects.Contains(character))
        {
            selectedObjects.Add(character);
            character.SetIsSelected(true);
            character.isCarryingTreasure = true; // 标记角色为搬运状态

            Debug.Log($"Character selected: {character.name}. Total characters: {selectedObjects.Count - 1}");

            int totalWeight = selectedTreasure.weight;
            if (selectedObjects.Count - 1 >= totalWeight) // -1 因为宝物也在列表中
            {
                Debug.Log("Treasure weight requirement met. Starting carry.");
                StartCarryTreasure();
            }

            // 更新宝物的 TMP 显示
            UpdateTreasureWeightDisplay();
        }
        else
        {
            Debug.LogWarning($"{character.name} is already carrying {selectedTreasure.name} or is already in the selected list.");
        }
    }
    else
    {
        // 正常选择角色
        DeselectAllObjects();
        character.SetIsSelected(true);
        selectedObjects.Add(character);
        characterMover.SetSelectedCharacters(selectedObjects);
        UpdateTreasureWeightDisplay();
        Debug.Log($"Character selected without treasure: {character.name}");
    }
}
    void StartCarryTreasure()
{
    foreach (var obj in selectedObjects)
    {
        if (obj.objectType == SelectableObject.ObjectType.Character)
        {
            obj.isCarryingTreasure = true;
            Debug.Log($"{obj.name} started carrying treasure: {selectedTreasure.name}");
        }
    }

    characterMover.SetSelectedCharacters(selectedObjects); // 设置角色组
    characterMover.SetTreasureToMove(selectedTreasure);    // 设置搬运的宝物
}

    void DeselectAllObjects()
{
    if (selectedTreasure != null)
    {
        // 计算搬运角色的中心点
        Vector3 groupCenter = CalculateGroupCenter();

        // 检查是否满足重量要求
        int currentCarriers = selectedObjects.Count - 1; // 不计入宝物本身
        if (currentCarriers >= selectedTreasure.weight)
        {
            // 满足重量要求，更新宝物位置到搬运角色的中心点
            if (groupCenter != Vector3.zero)
            {
                selectedTreasure.transform.position = groupCenter;
                Debug.Log($"Treasure dropped at: {selectedTreasure.transform.position}");
            }
        }
        else
        {
            // 不满足重量要求，保留原位置
            Debug.LogWarning($"Treasure cannot be moved. Insufficient carriers ({currentCarriers}/{selectedTreasure.weight}). Treasure remains at: {selectedTreasure.transform.position}");
            foreach (var obj in selectedObjects)
            {
                if (obj.objectType == SelectableObject.ObjectType.Character && obj.isCarryingTreasure)
                {
                    // 限制角色在宝物搬运半径内
                    RestrictCharacterMovement(obj);

                    // 触发颜色变化和颤动效果
                    TriggerRestrictedEffects(obj);
                }
            }
        }

        // 重置宝物的 TMP 显示
        selectedTreasure.UpdateCarriers(0);
    }

    // 清空所有选中对象
    foreach (var obj in selectedObjects)
    {
        obj.SetIsSelected(false);

        if (obj.objectType == SelectableObject.ObjectType.Character)
        {
            obj.isCarryingTreasure = false; // 重置搬运状态
            Debug.Log($"{obj.name} deselected and stopped carrying treasure.");
        }
    }

    selectedObjects.Clear();
    selectedTreasure = null;

    // 更新宝物的 TMP 显示
    UpdateTreasureWeightDisplay();
}

    void RestrictCharacterMovement(SelectableObject character)
    {
        float distance = Vector3.Distance(character.transform.position, selectedTreasure.transform.position);

        if (distance > selectedTreasure.carryRadius)
        {
            // 将角色强制移动回搬运半径内
            Vector3 direction = (character.transform.position - selectedTreasure.transform.position).normalized;
            character.transform.position = selectedTreasure.transform.position + direction * selectedTreasure.carryRadius;

            Debug.Log($"{character.name} is restricted within the treasure carry radius.");
        }
    }
    void TriggerRestrictedEffects(SelectableObject character)
    {
        Renderer renderer = character.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = restrictedColor;

            StartCoroutine(ShakeCharacter(character, originalColor));
        }
    }
IEnumerator ShakeCharacter(SelectableObject character, Color originalColor)
{
    Vector3 originalPosition = character.transform.position;
    float elapsedTime = 0f;

    while (elapsedTime < character.shakeDuration)
    {
        float offsetX = Random.Range(-character.shakeAmplitude, character.shakeAmplitude);
        float offsetY = Random.Range(-character.shakeAmplitude, character.shakeAmplitude);
        character.transform.position = new Vector3(
            originalPosition.x + offsetX,
            originalPosition.y + offsetY,
            originalPosition.z
        );

        elapsedTime += Time.deltaTime;
        yield return null;
    }

    // 恢复原始位置和颜色
    character.transform.position = originalPosition;
    Renderer renderer = character.GetComponent<Renderer>();
    if (renderer != null)
    {
        renderer.material.color = originalColor;
    }
}
    void UpdateTreasureWeightDisplay()
    {
        if (selectedTreasure != null)
        {
            int currentCarriers = selectedObjects.Count - 1; // 不计入宝物本身
            selectedTreasure.UpdateCarriers(currentCarriers);
        }
    }

    Vector3 CalculateGroupCenter()
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        // 计算所有搬运角色的中心点
        foreach (var obj in selectedObjects)
        {
            if (obj.objectType == SelectableObject.ObjectType.Character && obj.isCarryingTreasure)
            {
                center += obj.transform.position;
                count++;
            }
        }

        return count > 0 ? center / count : Vector3.zero; // 如果没有搬运角色，返回 Vector3.zero
    }
}