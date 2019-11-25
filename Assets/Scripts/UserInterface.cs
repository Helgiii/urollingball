using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace mygame
{
	public class UserInterface : MonoBehaviour
	{
		public Transform m_scrNoninit = null;
		public Transform m_scrPause = null;
		public Transform m_scrIngame = null;
		public Transform m_scrDeath = null;

		protected Level m_lvl = null;

		public void SetLevel(Level lvl) { m_lvl = lvl;  }

		public void HideAllScreens()
		{
			//foreach (Transform child in transform)
			//{
			//	//child is your child transform
			//	child.gameObject.SetActive(false);
			//}

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
				GameObject.Find("txtDistance").GetComponent<Text>().text = m_lvl.GetDistanceInt().ToString();
				GameObject.Find("txtScores").GetComponent<Text>().text = m_lvl.GetScores().ToString();
				GameObject.Find("txtDistanceMax").GetComponent<Text>().text = Globals.GetInstance().m_maxDistance.ToString();
				GameObject.Find("txtScoresMax").GetComponent<Text>().text = Globals.GetInstance().m_maxScores.ToString();
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
				GameObject.Find("txtScores").GetComponent<Text>().text = m_lvl.GetScores().ToString();
				GameObject.Find("txtDistance").GetComponent<Text>().text = Mathf.FloorToInt(m_lvl.GetDistanceInt()).ToString();
			}
		}
	}

}