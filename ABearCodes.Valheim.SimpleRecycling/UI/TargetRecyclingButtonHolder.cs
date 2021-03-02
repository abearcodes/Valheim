using System;
using System.Linq;
using ABearCodes.Valheim.SimpleRecycling.Recycling;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Logger = BepInEx.Logging.Logger;

namespace ABearCodes.Valheim.SimpleRecycling.UI
{
    public class TargetRecyclingButtonHolder : MonoBehaviour
    {
        private Button _recycleAllButton;
        private bool _prefired = false;
        private Text _textComponent;
        private Image _imageComponent;

        public delegate void RecycleAllHandler();

        public event RecycleAllHandler OnRecycleAllTriggered;
        private void FixedUpdate()
        {
            if (InventoryGui.instance == null) return;
            if (_recycleAllButton == null) SetupButton();
            if(!InventoryGui.instance && _prefired) SetButtonState(false);
        }

        private void SetupButton()
        {
            _recycleAllButton = Instantiate(InventoryGui.instance.m_takeAllButton,
                InventoryGui.instance.m_takeAllButton.transform);
            
            _recycleAllButton.transform.SetParent(InventoryGui.instance.m_takeAllButton.transform.parent);
            
            var newLocalPosition = GetSavedButtonPosition();
            _recycleAllButton.transform.localPosition = newLocalPosition; 
            
            _recycleAllButton.onClick.AddListener(OnRecycleAllPressed);
            _textComponent = _recycleAllButton.GetComponentInChildren<Text>();
            _imageComponent = _recycleAllButton.GetComponentInChildren<Image>();
            var dragger = _recycleAllButton.gameObject.AddComponent<UIDragger>();
            dragger.OnUIDropped += (source, position) =>
            {
                Plugin.Settings.ContainerRecyclingButtonPositionJsonString.Value = JsonUtility.ToJson(position);
            };
            SetButtonState(false);
        }

        private Vector3 GetSavedButtonPosition()
        {
            var maybeJson = Plugin.Settings.ContainerRecyclingButtonPositionJsonString?.Value;
            if (!TryFromJson(maybeJson, out Vector3 newLocalPosition))
                newLocalPosition = new Vector3(0f, -45.0f, -1f);
            return newLocalPosition;
        }

        private static bool TryFromJson<T>(string maybeJson, out T res)
        {
            try
            {
                res = JsonUtility.FromJson<T>(maybeJson);
                return true;
            }
            // yup, umbrella
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Tried to parse recycling button position, but couldn't. Error: {e.Message}");
                res = default(T);
            }
            return false;
        }

        private void SetButtonState(bool showPrefire)
        {
            if (showPrefire)
            {
                _prefired = true;
                _textComponent.text = "Confirm!?";
                _imageComponent.color = new Color(1f, 0.5f, 0.5f);
            }
            else
            {
                _prefired = false;
                _textComponent.text = "Recycle all";
                _imageComponent.color = new Color(0.5f, 1f, 0.5f);
            }
        }
        
        private void OnRecycleAllPressed()
        {
            if (!Player.m_localPlayer)
                return;
            if (!_prefired)
            {
                SetButtonState(true);
                return;
            }
            SetButtonState(false);
            OnRecycleAllTriggered?.Invoke();
        }

    }
}