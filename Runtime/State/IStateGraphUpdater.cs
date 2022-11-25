using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog
{
    public interface IStateGraphUpdater 
    {
        public void RegisterUpdateCallback(Action updateCallback);

        public void Pause();
        public void Resume();
        public void UnregisterUpdateCallback();
    }
}
