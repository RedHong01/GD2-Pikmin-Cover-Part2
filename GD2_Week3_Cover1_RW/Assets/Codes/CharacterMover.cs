using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CharacterMover : MonoBehaviour
{
    private List<SelectableObject> selectedCharacters = new List<SelectableObject>();
    private SelectableObject selectedTreasure;
    public LayerMask groundLayer;
    public float moveRadius = 1f;
    public float maxDistributeRadius = 2f;
    public GameObject prefabToInstantiate;

    private GameObject instantiatedPrefab;

    void Update()
    {
        if (selectedCharacters.Count > 0 && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                EnsureOnlyOneDesObject();

                instantiatedPrefab = Instantiate(prefabToInstantiate, hit.point, Quaternion.identity);
                instantiatedPrefab.tag = "des";  // 设置tag为 "des"
                MoveSelectedCharactersToPosition(hit.point);
            }
        }

        if (selectedTreasure != null && selectedCharacters.Exists(c => c.isCarryingTreasure))
        {
            // 计算搬运角色的中心
            Vector3 groupCenter = CalculateGroupCenter();
            selectedTreasure.transform.position = groupCenter;
        }

        if (selectedCharacters.Count > 0 && instantiatedPrefab != null)
        {
            if (AllCharactersStopped())
            {
                Destroy(instantiatedPrefab);
            }
        }
    }

    void EnsureOnlyOneDesObject()
    {
        GameObject[] desObjects = GameObject.FindGameObjectsWithTag("des");

        if (desObjects.Length > 0)
        {
            GameObject oldestDesObject = desObjects[0];
            Destroy(oldestDesObject);
        }
    }

    public void SetSelectedCharacters(List<SelectableObject> characters)
    {
        selectedCharacters = characters;
    }

    public void MoveSelectedCharactersToPosition(Vector3 targetPosition)
    {
        Vector3 groupCenter = Vector3.zero;
        int carrierCount = 0;

        foreach (var character in selectedCharacters)
        {
            NavMeshAgent agent = character.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.SetDestination(targetPosition);

                // 累加搬运角色的位置
                if (character.isCarryingTreasure)
                {
                    groupCenter += character.transform.position;
                    carrierCount++;
                }
            }
        }

        // 更新宝物的位置为搬运角色的中心
        if (selectedCharacters.Exists(c => c.isCarryingTreasure) && selectedTreasure != null)
        {
            if (carrierCount > 0)
            {
                groupCenter /= carrierCount;
                selectedTreasure.transform.position = groupCenter;
            }
        }
    }

    public void SetTreasureToMove(SelectableObject treasure)
    {
        selectedTreasure = treasure;
    }

    private bool AllCharactersStopped()
    {
        foreach (SelectableObject character in selectedCharacters)
        {
            NavMeshAgent agent = character.GetComponent<NavMeshAgent>();
            if (agent != null && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
            {
                return false;
            }
        }
        return true;
    }

    private Vector3 CalculateGroupCenter()
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (var character in selectedCharacters)
        {
            if (character.isCarryingTreasure)
            {
                center += character.transform.position;
                count++;
            }
        }

        return count > 0 ? center / count : Vector3.zero;
    }
}