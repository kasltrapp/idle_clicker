using System.Collections;
using UnityEngine;

namespace EasyIdleGame.UI
{
    public interface IOpenable
    {
        GameObject GameObject { get; }
        Animator Animator { get; }
        MonoBehaviour MonoBehaviour { get; }

        AnimationClip OpenAnimation { get; }
        AnimationClip CloseAnimation { get; }

        /// <summary>
        /// Opens the Displayer
        /// </summary>
        void Open()
        {
            GameObject.SetActive(true);

            if (OpenAnimation)
                Animator.Play(OpenAnimation.name);
        }

        /// <summary>
        /// Closes the Displayer with an animation
        /// </summary>
        void Close()
        {
            if (!MonoBehaviour.isActiveAndEnabled)
            {
                if (GameObject.activeSelf) GameObject.SetActive(false);
                return;
            }

            MonoBehaviour.StartCoroutine(CloseCoroutine());
        }

        /// <summary>
        /// Closes the Displayer immediately without an animation
        /// </summary>
        void CloseImmediately()
        {
            GameObject.SetActive(false);
        }

        IEnumerator CloseCoroutine()
        {
            if (CloseAnimation)
            {
                Animator.Play(CloseAnimation.name);
                yield return new WaitForSeconds(CloseAnimation.length);
            }

            CloseImmediately();
        }
    }
}
