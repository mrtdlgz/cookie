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
            player.tag = "Player"; // Düşmanların bulabilmesi için ZORUNLU
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
            
            GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sword.name = "Kilic_Asa";
            sword.transform.SetParent(player.transform);
            sword.transform.localPosition = new Vector3(0.7f, 0.5f, 0.6f);
            sword.transform.localScale = new Vector3(0.2f, 0.2f, 1.2f); // Made the sword larger and visible!
            Material swordMat = new Material(defaultShader);
            swordMat.color = Color.gray;
            sword.GetComponent<MeshRenderer>().sharedMaterial = swordMat;

            // 3. KAMERA
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                cam = camObj.AddComponent<Camera>();
            }
            
            cam.transform.position = new Vector3(0, 7, -6); // Move Camera much closer
            cam.transform.rotation = Quaternion.Euler(45, 0, 0); // Adjust angle for closeness
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.15f, 0.15f, 0.2f);

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
            string inputAssetJson = @"{
                  ""name"": ""PlayerControls"",
                  ""maps"": [
                    {
                      ""name"": ""Player"",
                      ""id"": ""d1b32ac0-42f1-419b-a9b0-13b772fc33c9"",
                      ""actions"": [
                        { ""name"": ""Move"", ""type"": ""Value"", ""id"": ""c32f2603-0c48-430f-b0cd-cb837ecbf892"", ""expectedControlType"": ""Vector2"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": true },
                        { ""name"": ""Attack"", ""type"": ""Button"", ""id"": ""cfbb8b5b-21da-4c4f-9e7f-561ca1251c14"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                        { ""name"": ""Dodge"", ""type"": ""Button"", ""id"": ""a5c9f592-3329-450f-a1f9-03a0b3f81e64"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false }
                      ],
                      ""bindings"": [
                        { ""name"": """", ""id"": ""11cb6ebf-18b6-455b-8f35-ab35a2d60341"", ""path"": ""<Gamepad>/leftStick"", ""interactions"": """", ""processors"": """", ""groups"": """", ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": false },
                        { ""name"": """", ""id"": ""cfbb9b5b-21da-4c4f-9e7f-561ca1251c15"", ""path"": ""<Gamepad>/buttonWest"", ""interactions"": """", ""processors"": """", ""groups"": """", ""action"": ""Attack"", ""isComposite"": false, ""isPartOfComposite"": false },
                        { ""name"": """", ""id"": ""cfbbab5b-21da-4c4f-9e7f-561ca1251c16"", ""path"": ""<Gamepad>/buttonSouth"", ""interactions"": """", ""processors"": """", ""groups"": """", ""action"": ""Dodge"", ""isComposite"": false, ""isPartOfComposite"": false }
                      ]
                    }
                  ]
                }";
                
            // Hata çıkmasını engellemek için her zaman zorla üzerine yazıyoruz ki yeni butonlar eklensin.
            File.WriteAllText(inputPath, inputAssetJson);
            AssetDatabase.ImportAsset(inputPath);
            AssetDatabase.Refresh();
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputPath);

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

            // Saldırı (Attack) Butonu
            GameObject jsAttack = new GameObject("Saldiri_Butonu");
            jsAttack.transform.SetParent(canvasObj.transform, false);
            Image attackImage = jsAttack.AddComponent<Image>();
            attackImage.sprite = knobSprite;
            attackImage.color = new Color(1f, 0.4f, 0.4f, 0.8f); 
            RectTransform attackRect = jsAttack.GetComponent<RectTransform>();
            attackRect.anchorMin = new Vector2(1, 0);
            attackRect.anchorMax = new Vector2(1, 0);
            attackRect.pivot = new Vector2(1f, 0.5f);
            attackRect.anchoredPosition = new Vector2(-250, 250); // Ekran sağı
            attackRect.sizeDelta = new Vector2(250, 250);

            OnScreenButton attackBtn = jsAttack.AddComponent<OnScreenButton>();
            SerializedObject soAttack = new SerializedObject(attackBtn);
            soAttack.FindProperty("m_ControlPath").stringValue = "<Gamepad>/buttonWest";
            soAttack.ApplyModifiedProperties();

            // Kaçınma (Dodge) Butonu
            GameObject jsDodge = new GameObject("Kacinma_Butonu");
            jsDodge.transform.SetParent(canvasObj.transform, false);
            Image dodgeImage = jsDodge.AddComponent<Image>();
            dodgeImage.sprite = knobSprite;
            dodgeImage.color = new Color(0.4f, 0.8f, 1f, 0.8f); 
            RectTransform dodgeRect = jsDodge.GetComponent<RectTransform>();
            dodgeRect.anchorMin = new Vector2(1, 0);
            dodgeRect.anchorMax = new Vector2(1, 0);
            dodgeRect.pivot = new Vector2(1f, 0.5f);
            dodgeRect.anchoredPosition = new Vector2(-100, 100); 
            dodgeRect.sizeDelta = new Vector2(160, 160);

            OnScreenButton dodgeBtn = jsDodge.AddComponent<OnScreenButton>();
            SerializedObject soDodge = new SerializedObject(dodgeBtn);
            soDodge.FindProperty("m_ControlPath").stringValue = "<Gamepad>/buttonSouth";
            soDodge.ApplyModifiedProperties();

            // 7. EVENT SYSTEM (UI tıklamaları/sürüklemeleri için)
            UnityEngine.EventSystems.EventSystem eventSystem = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>(); // Yeni Input Sistem Modülü ZORUNLUDUR
            }
            // 8. TEST ve JUICE SISTEMLERI (Kamera Sarsıntısı, Vuruş Hissi ve Kukla)
            if (cam != null)
            {
                CameraController camController = cam.GetComponent<CameraController>();
                if (camController == null) camController = cam.gameObject.AddComponent<CameraController>();
                
                camController.SetTarget(player.transform); // Always ensure target is set!
                SerializedObject soCam = new SerializedObject(camController);
                soCam.FindProperty("offset").vector3Value = new Vector3(0, 7, -6); // Closer offset
                soCam.ApplyModifiedProperties();
            }

            if (Object.FindObjectOfType<JuiceManager>() == null)
            {
                GameObject gmObj = new GameObject("Oyun_Yoneticisi_(JuiceManager)");
                gmObj.AddComponent<JuiceManager>();
            }

            if (GameObject.Find("Hedef_Kuklasi") == null)
            {
                GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                dummy.name = "Hedef_Kuklasi";
                dummy.transform.position = new Vector3(3, 1, 3);
                
                Material dummyMat = new Material(defaultShader);
                dummyMat.color = Color.magenta;
                dummy.GetComponent<MeshRenderer>().sharedMaterial = dummyMat;
                
                dummy.AddComponent<Cookie.Combat.TargetDummy>();
            }

            Debug.Log("✅ İŞLEM TAMAM! Hitbox'lar, Dash ve Sarsıntılarla dolu prototip sahnemiz hazır!");
        }
    }
}
#endif
