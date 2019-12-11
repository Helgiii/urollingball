using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace mygame
{
	public class UserInterface : MonoBehaviour
	{
		public Transform m_scrNoninit = null;
		public Transform m_scrPause = null;
		public Transform m_scrIngame = null;
		public Transform m_scrDeath = null;

		public Level m_lvl = null;

		// Normal raycasts do not work on UI elements, they require a special kind
		GraphicRaycaster m_raycaster;

		void Awake()
		{
			// Get both of the components we need to do this
			m_raycaster = GetComponent<GraphicRaycaster>();
		}

		void Update()
		{
			//Check if the left Mouse button is clicked
			//jump on any click outside gui elements
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				//Set up the new Pointer Event
				PointerEventData pointerData = new PointerEventData(EventSystem.current);
				List<RaycastResult> results = new List<RaycastResult>();

				//Raycast using the Graphics Raycaster and mouse click position
				pointerData.position = Input.mousePosition;
				m_raycaster.Raycast(pointerData, results);

				//For every result returned, output the name of the GameObject on the Canvas hit by the Ray
				//foreach (RaycastResult result in results)
				//{
					//Debug.Log("Hit " + result.gameObject.name);
				//}

				if (results.Count == 0)
					m_lvl.TryJump();
			}
		}



		public void HideAllScreens()
		{
			m_scrNoninit.gameObject.SetActive(false);
			m_scrPause.gameObject.SetActive(false);
			m_scrIngame.gameObject.SetActive(false);
			m_scrDeath.gameObject.SetActive(false);
		}

		public void OnBtnStart()
		{
			m_lvl.SetState(Level.GSTATE_ON);
		}

		public void OnBtnPause()
		{
			if (m_lvl.GetState() == Level.GSTATE_ON)
				m_lvl.SetState(Level.GSTATE_PAUSED);
		}

		public void OnBtnContinue()
		{
			if (m_lvl.GetState() == Level.GSTATE_PAUSED)
				m_lvl.SetState(Level.GSTATE_ON);
		}

		public void OnBtnPlayAgain()
		{
			if (m_lvl.GetState() != Level.GSTATE_FAILED)
				return;

			m_lvl.ReinitLevel();
			m_lvl.SetState(Level.GSTATE_ON);
		}

		public void ProcessNewGameState(int gstate)
		{
			HideAllScreens();

			if (gstate == Level.GSTATE_NONINIT)
			{
				m_scrNoninit.gameObject.SetActive(true);
			}
			else if (gstate == Level.GSTATE_ON)
			{
				m_scrIngame.gameObject.SetActive(true);
			}
			else if (gstate == Level.GSTATE_FAILED)
			{
				m_scrDeath.gameObject.SetActive(true);

				//set new values
				m_scrDeath.Find("txtDistance").GetComponent<Text>().text = m_lvl.GetDistanceInt().ToString();
				m_scrDeath.Find("txtScores").GetComponent<Text>().text = m_lvl.GetScores().ToString();
				m_scrDeath.Find("txtDistanceMax").GetComponent<Text>().text = Globals.Instance.m_maxDistance.ToString();
				m_scrDeath.Find("txtScoresMax").GetComponent<Text>().text = Globals.Instance.m_maxScores.ToString();
			}
			else if (gstate == Level.GSTATE_PAUSED)
			{
				m_scrIngame.gameObject.SetActive(true);
				m_scrPause.gameObject.SetActive(true);
			}
		}

		public void UpdateHud()
		{
			int lvlState = m_lvl.GetState();
			if (lvlState == Level.GSTATE_ON || lvlState == Level.GSTATE_PAUSED)
			{
				m_scrIngame.Find("txtScores").GetComponent<Text>().text = m_lvl.GetScores().ToString();
				m_scrIngame.Find("txtDistance").GetComponent<Text>().text = Mathf.FloorToInt(m_lvl.GetDistanceInt()).ToString();
			}
		}
	}

}