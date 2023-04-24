using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Basado en el script mostrado en:
// https://docs.unity3d.com/Manual/nav-MoveToClickPoint.html
// Añadí la máscara de colisión para que solo cheque contra el piso y no contra todas las 
// capas.

public class MoveToClickNav : MonoBehaviour
{
    NavMeshAgent _agent = null;
    LayerMask floorMask;
    public Animator animator;
    public GameObject agente;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        floorMask = LayerMask.GetMask("Floor");
        agente.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit,
                100.0f, floorMask))
            {
                // Le decimos que vaya al punto en el piso que chocó con el rayo de la cámara.
                _agent.destination = hit.point;
                animator.SetBool("IsWalking", true);
                animator.SetBool("IsIdle", false);

            }
            else
            {
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsIdle", true );
            }    
            
        }
    }

    private void OnDrawGizmos()
    {
        if (_agent != null && _agent.destination != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_agent.destination, 1.0f);
        }
    }
}
