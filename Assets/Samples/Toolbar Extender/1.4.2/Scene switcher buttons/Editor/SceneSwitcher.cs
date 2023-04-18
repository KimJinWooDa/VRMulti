using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityToolbarExtender.Examples
{
	static class ToolbarStyles
	{
		public static readonly GUIStyle commandButtonStyle;

		static ToolbarStyles()
		{
			commandButtonStyle = new GUIStyle("Command")
			{
				fontSize = 16,
				alignment = TextAnchor.MiddleCenter,
				imagePosition = ImagePosition.ImageAbove,
				fontStyle = FontStyle.Bold
			};
		}
	}

	[InitializeOnLoad]
	public class SceneSwitchLeftButton
	{
		static SceneSwitchLeftButton()
		{
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
			ToolbarExtender.RightToolbarGUI.Add(OnToolbarSettingGUI);
		}
		
		static void OnToolbarGUI()
		{
			GUILayout.FlexibleSpace();

			if(GUILayout.Button(new GUIContent("StartUp", "StartUp Scene"))) 
			{
				SceneHelper.OpenScene("StartUp");
			}

			if(GUILayout.Button(new GUIContent("MainMenu", "MainMenu Scene")))
			{
				SceneHelper.OpenScene("MainMenu");
			}
			
			if(GUILayout.Button(new GUIContent("Lobby", "Lobby Scene")))
			{
				SceneHelper.OpenScene("Lobby");
			}
			
			if(GUILayout.Button(new GUIContent("MainGame", "MainGame Scene")))
			{
				SceneHelper.OpenScene("MainGame");
			}
		}

		static void OnToolbarSettingGUI()
		{
			GUILayout.FlexibleSpace();

			if(GUILayout.Button(new GUIContent("셋업씬")))
			{
				SetUpFromStartScene.FromStartScene();
			}
			if(GUILayout.Button(new GUIContent("현재씬")))
			{
				StartFromThisScene.FromThisScene();
			}
		}
	}

	static class SetUpFromStartScene
	{
		public static void FromStartScene()
		{
			var pathOfFirstScene = EditorBuildSettings.scenes[0].path;
			var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(pathOfFirstScene);
			EditorSceneManager.playModeStartScene = sceneAsset;
			UnityEditor.EditorApplication.isPlaying = true;
		}
	}
	
	static class StartFromThisScene
	{
		public static void FromThisScene()
		{
			EditorSceneManager.playModeStartScene = null;
			UnityEditor.EditorApplication.isPlaying = true;
		}
	}


	static class SceneHelper
	{
		public static void OpenScene(string name)
		{
			var saved = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
			if (saved)
			{
				_ = EditorSceneManager.OpenScene($"Assets/Scenes/{name}.unity");
			}
		}
	}
	
}