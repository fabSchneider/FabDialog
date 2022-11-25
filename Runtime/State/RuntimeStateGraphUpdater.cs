using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog
{

    public class StateGraphUpdaterBehaviour : MonoBehaviour
    {
        public Action updateAction;

        public void Update()
        {
            if(updateAction != null)
                updateAction();
        }
    }

    public class RuntimeStateGraphUpdater : IStateGraphUpdater
    {
        StateGraphUpdaterBehaviour updater;

        public void RegisterUpdateCallback(Action updateCallback)
        {
            GameObject updaterGO = new GameObject();
            updaterGO.name = "StateGraphUpdater";
            updater = updaterGO.AddComponent<StateGraphUpdaterBehaviour>();

            updater.updateAction = updateCallback;
        }

        public void Pause()
        {
            updater.enabled = false;
        }


        public void Resume()
        {
            updater.enabled = true;
        }

        public void UnregisterUpdateCallback()
        {
            UnityEngine.Object.Destroy(updater.gameObject);
        }
    }
}
