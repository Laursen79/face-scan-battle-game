using System.Collections;
using System.Collections.Generic;
using Battle.Laursen;
using UnityEngine;

namespace Battle
{
    public class InputSystem : Singleton<InputSystem>
    {
        private ControlScheme _controlScheme = ControlScheme.Touch;
        [SerializeField] private GameObject touchUI;

        // The buttons that were clicked this frame.
        private List<string> clicked = new List<string>();

        // The buttons that have not been released yet.
        private List<string> pressed = new List<string>();
        
        // The buttons that were released this frame.
        private List<string> released = new List<string>();
        
        public void PressedButton(string buttonName)
        {
            
        }

        public void ReleasedButton()
        {
            
        }
    
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            
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

