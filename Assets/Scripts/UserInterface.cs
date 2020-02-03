using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace mygame
{
	public class UserInterface : MonoBehaviour
	{
		public Transform scrNoninit = null;
		public Transform scrPause = null;
		public Transform scrIngame = null;
		public Transform scrDeath = null;

		public Level lvl = null;

		// Normal raycasts do not work on UI elements, they require a special kind
		protected GraphicRaycaster raycaster;

		void Awake()
		{
			// Get both of the components we need to do this
			raycaster = GetComponent<GraphicRaycaster>();
		}

		void Update()
		{
			// Check if the left Mouse button is clicked
			// Jump on any click outside gui elements
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				// Set up the new Pointer Event
				var pointerData = new PointerEventData(EventSystem.current);
				var results = new List<RaycastResult>();

				// Raycast using the Graphics Raycaster and mouse click position
				pointerData.position = Input.mousePosition;
				raycaster.Raycast(pointerData, results);

				// For every result returned, output the name of the GameObject on the Canvas hit by the Ray
				//foreach (RaycastResult result in results)
				//{
					//Debug.Log("Hit " + result.gameObject.name);
				//}

				if (results.Count == 0)
					lvl.TryJump();
			}
		}



		public void HideAllScreens()
		{
			scrNoninit.gameObject.SetActive(false);
			scrPause.gameObject.SetActive(false);
			scrIngame.gameObject.SetActive(false);
			scrDeath.gameObject.SetActive(false);
		}

		public void OnBtnStart()
		{
			lvl.gameState = Level.GameState.On;
		}

		public void OnBtnPause()
		{
			if (lvl.gameState == Level.GameState.On)
				lvl.gameState = Level.GameState.Paused;
		}

		public void OnBtnContinue()
		{
			if (lvl.gameState == Level.GameState.Paused)
				lvl.gameState = Level.GameState.On;
		}

		public void OnBtnPlayAgain()
		{
			if (lvl.gameState != Level.GameState.Failed)
				return;

			lvl.ReinitLevel();
			lvl.gameState = Level.GameState.On;
		}

		public void ProcessNewGameState(Level.GameState gstate)
		{
			HideAllScreens();

			if (gstate == Level.GameState.NonInit)
			{
				scrNoninit.gameObject.SetActive(true);
			}
			else if (gstate == Level.GameState.On)
			{
				scrIngame.gameObject.SetActive(true);
			}
			else if (gstate == Level.GameState.Failed)
			{
				scrDeath.gameObject.SetActive(true);

				// Set new values
				scrDeath.Find("txtDistance").GetComponent<Text>().text = Mathf.FloorToInt(lvl.passedDistance).ToString();
				scrDeath.Find("txtScores").GetComponent<Text>().text = lvl.scores.ToString();
				scrDeath.Find("txtDistanceMax").GetComponent<Text>().text = Globals.Instance.maxDistance.ToString();
				scrDeath.Find("txtScoresMax").GetComponent<Text>().text = Globals.Instance.maxScores.ToString();
			}
			else if (gstate == Level.GameState.Paused)
			{
				scrIngame.gameObject.SetActive(true);
				scrPause.gameObject.SetActive(true);
			}
		}

		public void UpdateHud()
		{
			Level.GameState lvlState = lvl.gameState;
			if (lvlState == Level.GameState.On || lvlState == Level.GameState.Paused)
			{
				scrIngame.Find("txtScores").GetComponent<Text>().text = lvl.scores.ToString();
				scrIngame.Find("txtDistance").GetComponent<Text>().text = Mathf.FloorToInt(lvl.passedDistance).ToString();
			}
		}
	}

}