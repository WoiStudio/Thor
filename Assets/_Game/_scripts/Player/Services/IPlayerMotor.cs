using UnityEngine;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Drives character motion on the XZ plane via <see cref="Rigidbody.MovePosition"/> in the fixed loop.
    /// </summary>
    public interface IPlayerMotor
    {
        void Move(Vector2 input);

        void Stop();

        /// <summary>
        /// Applies one physics step of movement and rotation. Call from <c>FixedUpdate</c> (e.g. move state's <see cref="Woi.Ninja.Core.StateMachine.IState.FixedTick"/>).
        /// </summary>
        void ApplyFixedMovement();
    }
}
