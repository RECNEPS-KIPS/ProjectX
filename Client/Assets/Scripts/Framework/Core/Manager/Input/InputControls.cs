//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.11.2
//     from Assets/ResourcesAssets/Settings/Input/InputActionMap.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Framework.Core.Manager.Input
{
    public partial class @InputControls: IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @InputControls()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputActionMap"",
    ""maps"": [
        {
            ""name"": ""Keyboard"",
            ""id"": ""d040de73-8303-45c3-8d5c-2374f4e888e8"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""0b653179-83db-4c12-adf5-b47f459fa4a4"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""aa4374b2-58e0-4a45-b510-32dc27d61953"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Camera"",
                    ""type"": ""Value"",
                    ""id"": ""450f5e12-f884-424a-925a-b5ddeef283e2"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Run"",
                    ""type"": ""Button"",
                    ""id"": ""456ae4b9-07cd-4983-9a92-a90cc59720de"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Backpack"",
                    ""type"": ""Button"",
                    ""id"": ""eb1d58ee-0760-4203-9dd1-55007ee7acf6"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Pick"",
                    ""type"": ""Button"",
                    ""id"": ""12ab38e2-6d10-44d1-be7c-abbf8758f154"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""3f17edce-8fe0-47c7-927a-6b6e9c5dfabb"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""21b3143f-4196-45ab-a4e2-25f24ee139b3"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""c7ad2dcd-6ac6-4ae0-af00-2a49680467df"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""b31b92d1-6eb6-438e-b571-aa56611f2d23"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""da6ee7cf-1270-44f6-9e28-36a1e0a3f0fb"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""611150a5-b445-462a-9238-8957ab9fe714"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""65f83389-5f02-4f13-8386-30eda219ccdc"",
                    ""path"": ""<Pointer>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dbbdacaf-d4ec-4acf-8176-288f4a31656f"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9969a6d5-3203-4861-9d57-ac32c06f46ea"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Backpack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b6044bd7-6857-427a-ab51-e12b7b4fbd07"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // Keyboard
            m_Keyboard = asset.FindActionMap("Keyboard", throwIfNotFound: true);
            m_Keyboard_Move = m_Keyboard.FindAction("Move", throwIfNotFound: true);
            m_Keyboard_Jump = m_Keyboard.FindAction("Jump", throwIfNotFound: true);
            m_Keyboard_Camera = m_Keyboard.FindAction("Camera", throwIfNotFound: true);
            m_Keyboard_Run = m_Keyboard.FindAction("Run", throwIfNotFound: true);
            m_Keyboard_Backpack = m_Keyboard.FindAction("Backpack", throwIfNotFound: true);
            m_Keyboard_Pick = m_Keyboard.FindAction("Pick", throwIfNotFound: true);
        }

        ~@InputControls()
        {
            UnityEngine.Debug.Assert(!m_Keyboard.enabled, "This will cause a leak and performance issues, InputControls.Keyboard.Disable() has not been called.");
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // Keyboard
        private readonly InputActionMap m_Keyboard;
        private List<IKeyboardActions> m_KeyboardActionsCallbackInterfaces = new List<IKeyboardActions>();
        private readonly InputAction m_Keyboard_Move;
        private readonly InputAction m_Keyboard_Jump;
        private readonly InputAction m_Keyboard_Camera;
        private readonly InputAction m_Keyboard_Run;
        private readonly InputAction m_Keyboard_Backpack;
        private readonly InputAction m_Keyboard_Pick;
        public struct KeyboardActions
        {
            private @InputControls m_Wrapper;
            public KeyboardActions(@InputControls wrapper) { m_Wrapper = wrapper; }
            public InputAction @Move => m_Wrapper.m_Keyboard_Move;
            public InputAction @Jump => m_Wrapper.m_Keyboard_Jump;
            public InputAction @Camera => m_Wrapper.m_Keyboard_Camera;
            public InputAction @Run => m_Wrapper.m_Keyboard_Run;
            public InputAction @Backpack => m_Wrapper.m_Keyboard_Backpack;
            public InputAction @Pick => m_Wrapper.m_Keyboard_Pick;
            public InputActionMap Get() { return m_Wrapper.m_Keyboard; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(KeyboardActions set) { return set.Get(); }
            public void AddCallbacks(IKeyboardActions instance)
            {
                if (instance == null || m_Wrapper.m_KeyboardActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_KeyboardActionsCallbackInterfaces.Add(instance);
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Camera.started += instance.OnCamera;
                @Camera.performed += instance.OnCamera;
                @Camera.canceled += instance.OnCamera;
                @Run.started += instance.OnRun;
                @Run.performed += instance.OnRun;
                @Run.canceled += instance.OnRun;
                @Backpack.started += instance.OnBackpack;
                @Backpack.performed += instance.OnBackpack;
                @Backpack.canceled += instance.OnBackpack;
                @Pick.started += instance.OnPick;
                @Pick.performed += instance.OnPick;
                @Pick.canceled += instance.OnPick;
            }

            private void UnregisterCallbacks(IKeyboardActions instance)
            {
                @Move.started -= instance.OnMove;
                @Move.performed -= instance.OnMove;
                @Move.canceled -= instance.OnMove;
                @Jump.started -= instance.OnJump;
                @Jump.performed -= instance.OnJump;
                @Jump.canceled -= instance.OnJump;
                @Camera.started -= instance.OnCamera;
                @Camera.performed -= instance.OnCamera;
                @Camera.canceled -= instance.OnCamera;
                @Run.started -= instance.OnRun;
                @Run.performed -= instance.OnRun;
                @Run.canceled -= instance.OnRun;
                @Backpack.started -= instance.OnBackpack;
                @Backpack.performed -= instance.OnBackpack;
                @Backpack.canceled -= instance.OnBackpack;
                @Pick.started -= instance.OnPick;
                @Pick.performed -= instance.OnPick;
                @Pick.canceled -= instance.OnPick;
            }

            public void RemoveCallbacks(IKeyboardActions instance)
            {
                if (m_Wrapper.m_KeyboardActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IKeyboardActions instance)
            {
                foreach (var item in m_Wrapper.m_KeyboardActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_KeyboardActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public KeyboardActions @Keyboard => new KeyboardActions(this);
        public interface IKeyboardActions
        {
            void OnMove(InputAction.CallbackContext context);
            void OnJump(InputAction.CallbackContext context);
            void OnCamera(InputAction.CallbackContext context);
            void OnRun(InputAction.CallbackContext context);
            void OnBackpack(InputAction.CallbackContext context);
            void OnPick(InputAction.CallbackContext context);
        }
    }
}