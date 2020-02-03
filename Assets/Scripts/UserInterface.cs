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
			lvl.gameState = Level.GSTATE_ON;
		}

		public void OnBtnPause()
		{
			if (lvl.gameState == Level.GSTATE_ON)
				lvl.gameState = Level.GSTATE_PAUSED;
		}

		public void OnBtnContinue()
		{
			if (lvl.gameState == Level.GSTATE_PAUSED)
				lvl.gameState = Level.GSTATE_ON;
		}

		public void OnBtnPlayAgain()
		{
			if (lvl.gameState != Level.GSTATE_FAILED)
				return;

			lvl.ReinitLevel();
			lvl.gameState = Level.GSTATE_ON;
		}

		public void ProcessNewGameState(int gstate)
		{
			HideAllScreens();

			if (gstate == Level.GSTATE_NONINIT)
			{
				scrNoninit.gameObject.SetActive(true);
			}
			else if (gstate == Level.GSTATE_ON)
			{
				scrIngame.gameObject.SetActive(true);
			}
			else if (gstate == Level.GSTATE_FAILED)
			{
				scrDeath.gameObject.SetActive(true);

				// Set new values
				scrDeath.Find("txtDistance").GetComponent<Text>().text = lvl.GetDistanceInt().ToString();
				scrDeath.Find("txtScores").GetComponent<Text>().text = lvl.scores.ToString();
				scrDeath.Find("txtDistanceMax").GetComponent<Text>().text = Globals.Instance.maxDistance.ToString();
				scrDeath.Find("txtScoresMax").GetComponent<Text>().text = Globals.Instance.maxScores.ToString();
			}
			else if (gstate == Level.GSTATE_PAUSED)
			{
				scrIngame.gameObject.SetActive(true);
				scrPause.gameObject.SetActive(true);
			}
		}

		public void UpdateHud()
		{
			int lvlState = lvl.gameState;
			if (lvlState == Level.GSTATE_ON || lvlState == Level.GSTATE_PAUSED)
			{
				scrIngame.Find("txtScores").GetComponent<Text>().text = lvl.scores.ToString();
				scrIngame.Find("txtDistance").GetComponent<Text>().text = Mathf.FloorToInt(lvl.GetDistanceInt()).ToString();
			}
		}
	}

}