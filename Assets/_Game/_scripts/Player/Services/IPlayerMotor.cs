using UnityEngine;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Drives character motion on the XZ plane via <see cref="Rigidbody.MovePosition"/> in the fixed loop.
    /// </summary>
    public interface IPlayerMotor
    {
        /// <summary>Horizontal forward from current rotation (Y axis only).</summary>
        Vector3 Forward { get; }

        void Move(Vector2 input);

        void Move(Vector2 input, float speedMultiplier);

        /// <summary>Moves in normalized XZ world direction at the given absolute speed (units/sec).</summary>
        void MoveWorldDirection(Vector3 worldDirection, float speed);

        void Stop();

        /// <summary>Yaw-only rotation toward a world-space direction on the XZ plane.</summary>
        void FaceWorldDirection(Vector3 worldDirection);

        /// <summary>Sets yaw instantly toward the XZ direction (e.g. attack start; not smoothed).</summary>
        void FaceWorldDirectionImmediate(Vector3 worldDirection);

        /// <summary>
        /// Applies one physics step of movement and rotation. Call from <c>FixedUpdate</c> (e.g. move state's <see cref="Woi.Ninja.Core.StateMachine.IState.FixedTick"/>).
        /// </summary>
        /// <param name="rotateTowardMovement">When false, only translation is applied; yaw stays unchanged (attack strafe / lunge).</param>
        void ApplyFixedMovement(bool rotateTowardMovement = true);
    }
}
