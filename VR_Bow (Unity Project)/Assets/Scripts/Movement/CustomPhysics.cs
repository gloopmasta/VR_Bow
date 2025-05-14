using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomPhysics
{
    /// <summary>
    /// Moves a Rigidbody in a specified direction with an initial high speed, decelerating to a slower speed over a specified duration, then maintains the slower speed for another duration.
    /// </summary>
    /// <param name="rb">The Rigidbody to move.</param>
    /// <param name="direction">The direction of movement.</param>
    /// <param name="initialSpeed">The starting speed.</param>
    /// <param name="slowSpeed">The speed after deceleration.</param>
    /// <param name="decelerationDuration">Duration over which to decelerate from initialSpeed to slowSpeed.</param>
    /// <param name="slowDuration">Duration to maintain slowSpeed after deceleration.</param>
    public static async UniTask MoveWithDecelerationAndSlowPhase(
        Rigidbody rb,
        Vector3 direction,
        float initialSpeed,
        float slowSpeed,
        float decelerationDuration,
        float slowDuration)
    {
        if (rb == null) return;

        rb.isKinematic = true; // Disable physics interactions
        direction.Normalize();

        float elapsed = 0f;

        // Deceleration Phase
        while (elapsed < decelerationDuration)
        {
            float deltaTime = Time.unscaledDeltaTime;
            float t = elapsed / decelerationDuration;
            float currentSpeed = Mathf.Lerp(initialSpeed, slowSpeed, t);
            rb.MovePosition(rb.position + direction * currentSpeed * deltaTime);
            elapsed += deltaTime;
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }

        // Slow-Motion Phase
        elapsed = 0f;
        while (elapsed < slowDuration)
        {
            float deltaTime = Time.unscaledDeltaTime;
            rb.MovePosition(rb.position + direction * slowSpeed * deltaTime);
            elapsed += deltaTime;
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }

        rb.isKinematic = false; // Re-enable physics interactions
    }


    public static async UniTask MoveRigidbodyOverTime(Rigidbody rb, Vector3 direction, float speed, float duration)
    {
        if (rb == null) return;

        rb.isKinematic = true; // Disable physics interactions
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float deltaTime = Time.unscaledDeltaTime;
            rb.MovePosition(rb.position + direction * speed * deltaTime);
            elapsed += deltaTime;
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }

        rb.isKinematic = false; // Re-enable physics interactions
    }
}
