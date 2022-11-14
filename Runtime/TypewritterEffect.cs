using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog
{
    public class TypewritterEffect
    {
        private MonoBehaviour monoBehaviour;
        private string textToDisplay;

        private float speed = 0.05f;
        private WaitForSeconds wait;

        public string TextToDisplay
        {
            set
            {
                if (coroutine != null)
                {
                    monoBehaviour.StopCoroutine(coroutine);
                    onTextFinished?.Invoke();
                }
                coroutine = null;
                textToDisplay = value;
                currentText = string.Empty;
            }
        }

        private string currentText = string.Empty;

        public string CurrentText => currentText;

        public float Speed
        {
            get => speed;
            set
            {
                speed = value;
                wait = new WaitForSeconds(value);
            }
        }

        Coroutine coroutine;

        public TypewritterEffect(MonoBehaviour monoBehaviour)
        {
            this.monoBehaviour = monoBehaviour;

            wait = new WaitForSeconds(speed);
        }

        public Action<string> onNewText;
        public Action onTextFinished;

        public void Start()
        {
            if (coroutine != null)
                monoBehaviour.StopCoroutine(coroutine);
            coroutine = monoBehaviour.StartCoroutine(WriteCoroutine());
        }

        private IEnumerator WriteCoroutine()
        {
            if (string.IsNullOrEmpty(textToDisplay))
            {
                onTextFinished?.Invoke();
                yield break;
            }

            for (int i = 1; i <= textToDisplay.Length; i++)
            {
                yield return wait;
                currentText = textToDisplay.Substring(0, i);
                onNewText?.Invoke(currentText);
            }
            onTextFinished?.Invoke();
        }
    }
}
