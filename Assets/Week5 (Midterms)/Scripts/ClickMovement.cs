using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class ClickMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private LayerMask groundMask;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); 
    }

    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundMask))
                agent.SetDestination(hit.point);
        }
    }
}
