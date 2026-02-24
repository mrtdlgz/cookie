#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Cookie.Core;
using Cookie.Input;
using Cookie.Player.Classes;
using System.IO;

namespace Cookie.EditorTools
{
    public class GameSetupWizard : EditorWindow
    {
        [MenuItem("Cookie / 🚀 Tek Tıkla Sahneyi Baştan Kur (GÜNCEL)")]
        public static void CreateAwesomeScene()
        {
            Debug.Log("Kurulum Başladı... Hatalar Çözülüyor...");

            // 1. ZEMİN VE MATERYALLER
            Shader defaultShader = GraphicsSettings.currentRenderPipeline != null 
                ? Shader.Find("Universal Render Pipeline/Lit") 
                : Shader.Find("Standard");
            if (defaultShader == null) defaultShader = Shader.Find("Standard");

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Zemin_Level";
            ground.transform.localScale = new Vector3(5, 1, 5); 
            Material groundMat = new Material(defaultShader);
            groundMat.color = new Color(0.2f, 0.4f, 0.2f); 
            ground.GetComponent<MeshRenderer>().sharedMaterial = groundMat;

            // 2. OYUNCU (Player) - İlk başta inaktif yapıyoruz ki PlayerInput hatası atmasın
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Oyuncu_Player";
            player.SetActive(false); // Bileşenleri eklerken Awake/OnEnable fırlatmasın diye kapattık
            player.transform.position = new Vector3(0, 1.1f, 0);
            
            Material playerMat = new Material(defaultShader);
            playerMat.color = new Color(0.9f, 0.4f, 0.1f);
            player.GetComponent<MeshRenderer>().sharedMaterial = playerMat;

            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Yuz";
            nose.transform.SetParent(player.transform);
            nose.transform.localPosition = new Vector3(0, 0.5f, 0.5f);
            nose.transform.localScale = new Vector3(0.5f, 0.4f, 0.4f);
            Material noseMat = new Material(defaultShader);
            noseMat.color = Color.white;
            nose.GetComponent<MeshRenderer>().sharedMaterial = noseMat;

            // 3. KAMERA
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0, 12, -10);
                cam.transform.rotation = Quaternion.Euler(50, 0, 0);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            }

            // 4. IŞIK
            Light sun = Object.FindObjectOfType<Light>();
            if (sun == null)
            {
                GameObject lightObj = new GameObject("Gunes");
                sun = lightObj.AddComponent<Light>();
                sun.type = LightType.Directional;
            }
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);

            // 5. INPUT ACTIONS
            if (!AssetDatabase.IsValidFolder("Assets/Scripts")) AssetDatabase.CreateFolder("Assets", "Scripts");
            if (!AssetDatabase.IsValidFolder("Assets/Scripts/Input")) AssetDatabase.CreateFolder("Assets/Scripts", "Input");
            
            string inputPath = "Assets/Scripts/Input/PlayerControls.inputactions";
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputPath);
            if (actions == null)
            {
                string inputAssetJson = @"{
                  ""name"": ""PlayerControls"",
                  ""maps"": [
                    {
                      ""name"": ""Player"",
                      ""id"": ""d1b32ac0-42f1-419b-a9b0-13b772fc33c9"",
                      ""actions"": [
                        { ""name"": ""Move"", ""type"": ""Value"", ""id"": ""c32f2603-0c48-430f-b0cd-cb837ecbf892"", ""expectedControlType"": ""Vector2"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": true }
                      ],
                      ""bindings"": [
                        { ""name"": """", ""id"": ""11cb6ebf-18b6-455b-8f35-ab35a2d60341"", ""path"": ""<Gamepad>/leftStick"", ""interactions"": """", ""processors"": """", ""groups"": """", ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": false }
                      ]
                    }
                  ]
                }";
                File.WriteAllText(inputPath, inputAssetJson);
                AssetDatabase.ImportAsset(inputPath);
                AssetDatabase.Refresh();
                actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputPath);
            }

            // CLASS Datası
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects")) AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Classes")) AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Classes");
            
            ClassData warriorData = AssetDatabase.LoadAssetAtPath<ClassData>("Assets/ScriptableObjects/Classes/WarriorData.asset");
            if (warriorData == null)
            {
                warriorData = ScriptableObject.CreateInstance<ClassData>();
                warriorData.className = "Savasci_Auto";
                warriorData.baseMoveSpeed = 7;
                AssetDatabase.CreateAsset(warriorData, "Assets/ScriptableObjects/Classes/WarriorData.asset");
            }

            // OYUNCUYA BİLEŞENLERİ EKLEME
            player.AddComponent<InputManager>(); // Bu satır otomatik olarak PlayerInput bileşenini de ekler (RequireComponent)
            PlayerController pController = player.AddComponent<PlayerController>();
            
            // Zaten RequireComponent ile eklendiği için GetComponent ile alıyoruz
            PlayerInput pInput = player.GetComponent<PlayerInput>();
            if (pInput == null) pInput = player.AddComponent<PlayerInput>();
            
            // Unity Editor üzerinden atama yapar gibi SerializedObject kullanmak daha güvenli
            SerializedObject soInput = new SerializedObject(pInput);
            soInput.FindProperty("m_Actions").objectReferenceValue = actions;
            soInput.FindProperty("m_DefaultActionMap").stringValue = "Player";
            soInput.FindProperty("m_NotificationBehavior").enumValueIndex = (int)PlayerNotifications.SendMessages;
            soInput.ApplyModifiedProperties();

            SerializedObject soController = new SerializedObject(pController);
            soController.FindProperty("currentClass").objectReferenceValue = warriorData;
            if (cam != null) soController.FindProperty("cameraTransform").objectReferenceValue = cam.transform;
            soController.ApplyModifiedProperties();

            // Atamalar bittikten sonra aktifleştiriyoruz (HATA ÇÖZÜMÜ)
            player.SetActive(true);

            // 6. UI JOSTICK
            GameObject canvasObj = new GameObject("Mobil_Arayuz_(Canvas)");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Faresel tıklamaların Canvas üzerinde okunabilmesi için GraphicRaycaster ZORUNLUDUR!
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Sprite knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            Sprite bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

            GameObject jsBg = new GameObject("Joystick_Arkaplan");
            jsBg.transform.SetParent(canvasObj.transform, false);
            Image bgImage = jsBg.AddComponent<Image>();
            bgImage.sprite = bgSprite;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.6f); 
            RectTransform bgRect = jsBg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0);
            bgRect.anchorMax = new Vector2(0, 0);
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = new Vector2(300, 300); 
            bgRect.sizeDelta = new Vector2(300, 300);

            GameObject jsKnob = new GameObject("Joystick_Topuz");
            jsKnob.transform.SetParent(jsBg.transform, false);
            Image knobImage = jsKnob.AddComponent<Image>();
            knobImage.sprite = knobSprite;
            knobImage.color = new Color(1f, 1f, 1f, 0.9f); 
            RectTransform knobRect = jsKnob.GetComponent<RectTransform>();
            knobRect.sizeDelta = new Vector2(120, 120);
            knobRect.anchoredPosition = Vector2.zero;

            OnScreenStick screenStick = jsBg.AddComponent<OnScreenStick>();
            screenStick.movementRange = 100;
            SerializedObject soStick = new SerializedObject(screenStick);
            soStick.FindProperty("m_ControlPath").stringValue = "<Gamepad>/leftStick";
            soStick.ApplyModifiedProperties();

            // 7. EVENT SYSTEM (UI tıklamaları/sürüklemeleri için)
            UnityEngine.EventSystems.EventSystem eventSystem = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>(); // Yeni Input Sistem Modülü ZORUNLUDUR
            }

            Debug.Log("✅ İŞLEM TAMAM! PlayerInput hatası ve görseller %100 düzeltildi.");
        }
    }
}
#endif
