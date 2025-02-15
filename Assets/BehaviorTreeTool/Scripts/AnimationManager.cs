using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Global
{
    public static class AnimationManager
    {
        public static async Task TriggerAnimation(Animator animator, string animationName,
                                                  CancellationToken cancellationToken)
        {
            var animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            animator.SetTrigger(animationName);
            await Task.Delay(TimeSpan.FromSeconds(animationLength), cancellationToken);
        }

        public static async Task TriggerAnimation(Animator animator, int animationID,
                                                  CancellationToken cancellationToken)
        {
            var animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            animator.SetTrigger(animationID);
            await Task.Delay(TimeSpan.FromSeconds(animationLength), cancellationToken);
        }

        public static async Task TriggerAnimWithCooldown(Animator animator, string animationName, float cooldown,
                                                         CancellationToken cancellationToken)
        {
            var animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            animator.SetTrigger(animationName);

            var totalWaitTime = animationLength + cooldown;
            await Task.Delay(TimeSpan.FromSeconds(totalWaitTime), cancellationToken);
        }

        public static async Task TriggerAnimWithCooldown(Animator animator, int animationID, float cooldown,
                                                         CancellationToken cancellationToken)
        {
            var animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            animator.SetTrigger(animationID);

            var totalWaitTime = animationLength + cooldown;
            await Task.Delay(TimeSpan.FromSeconds(totalWaitTime), cancellationToken);
        }
    }
}