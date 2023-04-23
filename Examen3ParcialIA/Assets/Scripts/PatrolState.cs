using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : BaseState
{
    public PatrolAgentFSM _sm;

    private WaitForSeconds RotateIntervalWait;

    private Coroutine RotateCoroutineRef;

    // public float fRotateAngle = 45f;

    public PatrolState(StateMachine stateMachine) : base("Patrol", stateMachine)
    {
        // el ": base("Patrol", stateMachine)" equivale a escribir las siguientes líneas, 
        // que están en el constructor de la clase padre BaseState.
        //this.name = "Patrol";
        //this.stateMachine = stateMachine;
    }

    // Sin embargo, al tener el constructor con la StateMachine, no estamos forzando al 
    // desarrollador a usar la PatrolAgentFSM que diseñamos para trabajar en conjunto.
    // Pero podemos hacer una versión del constructor que sí lo enforce.
    public PatrolState(PatrolAgentFSM stateMachine) : base("Patrol", stateMachine)
    {
        _sm = stateMachine;
    }

    // Función para rotar al personaje cada n tiempo.
    private IEnumerator Rotate()
    {
        // Solo declarar el waitForSeconds una vez y reutilizarlo, en vez de crearlo una y otra vez dentro
        // del ciclo for de la corrutina.
        RotateIntervalWait = new WaitForSeconds(_sm.FTimeToRotate);

        // Como esto está dentro de un while(true), se ejecutará una y otra vez hasta que se cancele.
        while (true)
        {
            // Esperamos n segundos antes de ejecutar el código siguiente.
            yield return RotateIntervalWait;

            // Debug.Log("_sm.transform.up = " + _sm.transform.up);
            // Rotamos a nuestro Agente patrullero respecto al eje de "arriba".
            _sm.transform.Rotate(_sm.transform.up, _sm.FRotateAngle);
        }
    }


    public override void Enter()
    {
        // base.Enter();
        // Por ejemplo, aquí podrían poner el Trigger de su animator a "SetTrigger("OnPatrol")"
        RotateCoroutineRef = _sm.StartCoroutine(this.Rotate());
    }

    public override void Exit()
    {
        _sm.StopCoroutine(RotateCoroutineRef);
        /// Debug.Log("Exited Patrol State.");
    }

    public override void UpdateLogic()
    {
        // revisamos la condición de cambio de estado, que es si el infiltrador está en el campo de visión
        // Physics.
        // if()

        // Vector3 FakeInfiltratorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // FakeInfiltratorPosition.z = 0.0f;  // Si no hacemos esto, tendrá la z de la cámara.
        // _sm.v3TargetPosition = FakeInfiltratorPosition;

        // NOTA: Quité lo de aquí y lo puse en el updatePhysics.

    }

    public override void UpdatePhysics()
    {

        bool bCheckVision = _sm.CheckFieldOfVision(_sm.fVisionDist, _sm.fVisionAngle,
            out Vector3 tmp_TargetPosition);
        if (bCheckVision)
        {
            // Debug.Log("The targetPosition is: " + tmp_TargetPosition);
            _sm.v3LastKnownTargetPos = tmp_TargetPosition;

            _sm.ChangeState(_sm.alertState);
            return;  // Para asegurarnos que salga de esta función, pues ya se disparó un cambio de estado.
        }
    }
}

public class AlertState : BaseState
{
    public PatrolAgentFSM _sm;

    private float fTotalTimeTargetHasBeenOnFOV = 0.0f;
    private float fTotalTimeBeforeGoing = 0.0f;

    private enum AlertStep
    {
        preparingToGo = 0, going = 1, goingBack = 2, finished = 3
    }
    private AlertStep bCheckingLastKnownPos = AlertStep.finished;

    public AlertState(StateMachine stateMachine) : base("Alert", stateMachine)
    {

    }

    public AlertState(PatrolAgentFSM stateMachine) : base("Alert", stateMachine)
    {
        _sm = stateMachine;
    }

    public override void Enter()
    {
        fTotalTimeTargetHasBeenOnFOV = 0.0f;
        fTotalTimeBeforeGoing = 0.0f;
        bCheckingLastKnownPos = AlertStep.preparingToGo;
    }

    public override void Exit() { }

    public override void UpdateLogic() { }

    public override void UpdatePhysics()
    {
        if (bCheckingLastKnownPos == AlertStep.preparingToGo)
        {
            fTotalTimeBeforeGoing += Time.fixedDeltaTime;
            if (fTotalTimeBeforeGoing >= _sm.FTimeBeforeCheckingTargetsLastKnownPosition)
            {
                // Entonces pasa al AlertStep.going
                bCheckingLastKnownPos = AlertStep.going;
                _sm.navMeshAgent.SetDestination(_sm.v3LastKnownTargetPos);
                // Debug.Log("Entered AlertStep.going");
            }
        }

        // Mientras no haya llegado a la última posición conocida del Infiltrador, seguir moviéndose hacia ella.
        if (bCheckingLastKnownPos == AlertStep.going)
        {
            float vDist = (_sm.transform.position - _sm.v3LastKnownTargetPos).magnitude;
            // Debug.Log("Dist between _sm.transform.position - _sm.v3LastKnownTargetPos is: "  + vDist);
            if (vDist <= 1.0f)
            {
                // Entonces ya llegó. Tiene que dar unos vistazos, y luego regresar a 
                // su posición de patrullaje.
                bCheckingLastKnownPos = AlertStep.goingBack;
                _sm.navMeshAgent.SetDestination(_sm.v3AgentPatrollingPosition);
            }
        }
        if (bCheckingLastKnownPos == AlertStep.goingBack)
        {
            float vDist = (_sm.transform.position - _sm.v3AgentPatrollingPosition).magnitude;
            if (vDist <= 1.0f)
            {
                // Entonces ya llegó a 
                // su posición de patrullaje y debe de regresar al estado de Patrullaje.
                bCheckingLastKnownPos = AlertStep.finished;
                _sm.ChangeState(_sm.patrolState);
                return;
            }
        }


        bool CheckFOV = _sm.CheckFieldOfVision(_sm.fAlertVisionDist, _sm.fAlertVisionAngle,
            out Vector3 tmp_TargetPosition);
        if (CheckFOV)
        {
            fTotalTimeTargetHasBeenOnFOV += Time.fixedDeltaTime;
        }
        // else
        // { 
        //    fTotalTimeTargetHasBeenOnFOV -= Time.fixedDeltaTime;
        // }

        if (fTotalTimeTargetHasBeenOnFOV > _sm.FTimeToGoFromAlertToAttack)
        {
            Debug.Log("Changing to AttackState");
            _sm.ChangeState(_sm.attackState);
            return;
        }

    }
}

public class AttackState : BaseState
{
    public PatrolAgentFSM _sm;
    private float fCurrentChaseTime;
    private bool bGoingBackToPatrolPos = false;

    public AttackState(StateMachine stateMachine) : base("Attack", stateMachine)
    {

    }

    public AttackState(PatrolAgentFSM stateMachine) : base("Attack", stateMachine)
    {
        _sm = stateMachine;
    }

    public override void Enter()
    {
        // Lo reseteamos a 0 cuando se entre a este estado.
        fCurrentChaseTime = 0.0f;
        bGoingBackToPatrolPos = false;
        _sm.navMeshAgent.destination = _sm.v3TargetTransform.position;
    }

    private void CheckPersecutionTime()
    {
        if (fCurrentChaseTime >= _sm.FMaxChasingTime)
        {
            // Ahora tenemos que hacer que vuelva a su posición inicial.
            _sm.navMeshAgent.destination = _sm.v3AgentPatrollingPosition;
            bGoingBackToPatrolPos = true;
        }

    }

    private void GoBackToPatrolPosition()
    {
        // IDEA: Incluso podría ser regresar primero a Alert pero con su AlertStep en goingBack...
        // Se ejecuta hasta que llegue a la posición. y una vez lo hace, vuelve al esyado de patrullaje.
        float fDist = (_sm.transform.position - _sm.v3AgentPatrollingPosition).magnitude;
        if (fDist <= 1.0f)
        {
            _sm.navMeshAgent.destination = _sm.transform.position;
            _sm.ChangeState(_sm.patrolState);
            return;
        }
    }

    public override void Exit() { }

    public override void UpdateLogic() { }

    public override void UpdatePhysics()
    {
        if (!bGoingBackToPatrolPos)
        {
            fCurrentChaseTime += Time.fixedDeltaTime;

            // Debug.Log("currently attacking pos: " + _sm.navMeshAgent.destination);
            // idealmente, no se debería setear el destination cada update, porque involucra un pathfinding
            // cada vez que se llama.
            _sm.navMeshAgent.destination = _sm.v3TargetTransform.position;

            CheckPersecutionTime();
        }
        else
        {
            GoBackToPatrolPosition();
        }
    }
}
