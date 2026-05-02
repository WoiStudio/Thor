using System;
using UnityEngine;
using Obvious.Soap;

public class InputManager : MonoBehaviour, IInputProvider
{
    [SerializeField] private InputSets inputSets;
    private PlayerInputActions inputActions;
    private InputContextStack contextStack;
    
    public PlayerInputActions InputActions => inputActions;

    private void Start()
    {
        inputActions = new PlayerInputActions();
        inputActions.Enable();  
        contextStack = new InputContextStack();

        PushContexts(inputSets.PCContexts);
    }

    private void Update()
    {
        contextStack?.Update();
    }
    
    private void OnDestroy()
    {
        inputActions?.Disable();
        contextStack?.Clear();
        inputActions?.Dispose();
    }
    
    public void PushContexts(InputContext[] contexts)
    {
        foreach (var ctx in contexts)
        {
            ctx.Initialize(inputActions);
            contextStack.PushContext(ctx);
        }
    }
    
    public void PopContext(InputContext context)
    {
        contextStack.PopContext(context);
    }
    
    public void ClearAllContexts()
    {
        contextStack.Clear();
    }

    /*private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            inputActions.Disable();
        }
        else
        {
            inputActions.Enable();
        }
    }*/

    [Serializable]
    public struct InputSets
    {
        public InputContext[] PCContexts;
    }     
}

public interface IInputProvider
{
    PlayerInputActions InputActions { get; }
    void PushContexts(InputContext[] contexts);
    void PopContext(InputContext context);
    void ClearAllContexts();
}