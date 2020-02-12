using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

namespace mygame
{
	public class UserInterface : MonoBehaviour
	{
		// Attach those in the editor.

		public Transform scrNoninit = null;
		public Transform scrPause = null;
		public Transform scrIngame = null;
		public Transform scrDeath = null;
		public Level lvl = null;

		// Normal raycasts do not work on UI elements, they require a special kind.
		protected GraphicRaycaster raycaster;

		protected void Awake()
		{
			// Get both of the components we need to do this.
			raycaster = GetComponent<GraphicRaycaster>();

			// Listen to level events
			lvl.ParamsChanged += OnParamsChangedEvent;
			lvl.LevelStateChanged += OnStateChangedEvent;
		}

		protected void Update()
		{
			// Check if the left Mouse button is clicked.
			// Jump on any click outside gui elements.
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				// Set up the new Pointer Event.
				var pointerData = new PointerEventData(EventSystem.current);
				var results = new List<RaycastResult>();

				// Raycast using the Graphics Raycaster and mouse click position.
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



		protected void HideAllScreens()
		{
			scrNoninit.gameObject.SetActive(false);
			scrPause.gameObject.SetActive(false);
			scrIngame.gameObject.SetActive(false);
			scrDeath.gameObject.SetActive(false);
		}

		public void OnBtnStart()
		{
			lvl.state = Level.State.On;
		}

		public void OnBtnPause()
		{
			if (lvl.state == Level.State.On)
				lvl.state = Level.State.Paused;
		}

		public void OnBtnContinue()
		{
			if (lvl.state == Level.State.Paused)
				lvl.state = Level.State.On;
		}

		public void OnBtnPlayAgain()
		{
			if (lvl.state != Level.State.Failed)
				return;

			lvl.ReinitLevel();
			lvl.state = Level.State.On;
		}

		protected void ProcessNewLevelState(Level.State gstate)
		{
			HideAllScreens();

			if (gstate == Level.State.NonInit)
			{
				scrNoninit.gameObject.SetActive(true);
			}
			else if (gstate == Level.State.On)
			{
				scrIngame.gameObject.SetActive(true);
			}
			else if (gstate == Level.State.Failed)
			{
				scrDeath.gameObject.SetActive(true);

				// Set new values
				scrDeath.Find("txtDistance").GetComponent<Text>().text = Mathf.FloorToInt(lvl.passedDistance).ToString();
				scrDeath.Find("txtScores").GetComponent<Text>().text = lvl.scores.ToString();
				scrDeath.Find("txtDistanceMax").GetComponent<Text>().text = Globals.Instance.maxDistance.ToString();
				scrDeath.Find("txtScoresMax").GetComponent<Text>().text = Globals.Instance.maxScores.ToString();
			}
			else if (gstate == Level.State.Paused)
			{
				scrIngame.gameObject.SetActive(true);
				scrPause.gameObject.SetActive(true);
			}
		}
		
		protected void UpdateHud()
		{
			Level.State lvlState = lvl.state;
			if (lvlState == Level.State.On || lvlState == Level.State.Paused)
			{
				scrIngame.Find("txtScores").GetComponent<Text>().text = lvl.scores.ToString();
				scrIngame.Find("txtDistance").GetComponent<Text>().text = Mathf.FloorToInt(lvl.passedDistance).ToString();
			}
		}

		protected void OnParamsChangedEvent(object sender)
		{
			UpdateHud();
		}
		protected void OnStateChangedEvent(object sender, LevelStateChangedEventArgs eParams)
		{
			ProcessNewLevelState(eParams.State);
		}
	}

}