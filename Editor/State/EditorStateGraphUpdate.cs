using System;
using UnityEditor;

namespace Fab.Dialog.Editor
{
    public class EditorStateGraphUpdater : IStateGraphUpdater
    {
        delegate void StateGraphUpdateCallback();

        private EditorApplication.CallbackFunction callback;

        public void RegisterUpdateCallback(Action updateCallback)
        {
            if (callback != null)
                EditorApplication.update -= callback;

           callback = new EditorApplication.CallbackFunction(updateCallback);
           EditorApplication.update += callback;
        }

        public void Pause()
        {
            if (callback != null)
                EditorApplication.update -= callback;
        }

        public void Resume()
        {
            if (callback != null)
            {
                EditorApplication.update -= callback;
                EditorApplication.update += callback;
            }
        }

        public void UnregisterUpdateCallback()
        {
            if (callback != null)
                EditorApplication.update -= callback;

            callback = null;
        }
    }
}
