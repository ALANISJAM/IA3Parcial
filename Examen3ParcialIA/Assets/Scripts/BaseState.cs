using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

 These StateMachine and BaseState classes are based on Mina Pecheux's work posted at:
    https://medium.com/c-sharp-progarmming/make-a-basic-fsm-in-unity-c-f7d9db965134
    https://youtu.be/-VkezxxjsSE

 */


public class BaseState
{
    // Nombre para identificar el estado
    // p.e. "Patrol", "Chase", "Attack", etc.
    public string name;
    public StateMachine stateMachine;

    // Constructor que toma el nombre del estado y una referencia a la máquina de estados que 
    // va a ser su dueña.
    public BaseState(string name, StateMachine stateMachine)
    {
        this.name = name;
        this.stateMachine = stateMachine;
    }

    public void PrintName()
    {
        Debug.Log("State name is: " + name);
    }

    // Estas funciones son virtual para que no sea indispensable implementarlas en las clases 
    // que hereden de BaseState, pero pueden sustituirlas con un 'override'.
    public virtual void Enter()
    {
        PrintName();
    }

    public virtual void Exit() { }

    public virtual void UpdateLogic() { }

    public virtual void UpdatePhysics() { }

}
