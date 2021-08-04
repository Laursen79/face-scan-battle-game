using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Battle.BzKovSoft.ActiveRagdoll;
using Battle.Laursen;
using Battle.@public.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Battle
{
    public class InputSystem : Singleton<InputSystem>
    {
        private ControlScheme _controlScheme = ControlScheme.Touch;
        [SerializeField] private GameObject touchUI;
        
        [SerializeField] private List<Controller> controllers = new List<Controller>();
        
        public void PressedButton(string buttonName)
        {
            
        }

        public void ReleasedButton()
        {
            
        }
    
        // Start is called before the first frame update
        void Start()
        {
            LocateTouchControllers();
        }

        private void LocateTouchControllers()
        {
            foreach (var controller in FindObjectsOfType<Controller>())
            {
                controllers.Add(controller);
            }

        }

        // Update is called once per frame
        void Update()
        {
            var touches = Input.touches;
            

        }
        
        private void WarnButtonsNotSet()
        {

        }
        
        
        
    }

    public enum ControlScheme
    {
        Touch
    }    
}

