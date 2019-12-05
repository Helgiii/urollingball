using UnityEngine;
using System;
using System.Collections.Generic;

namespace mygame
{
	public class Globals : MonoBehaviour
	{
		private static Globals instance = null;
		public static Globals GetInstance()  {
			return instance;
		}

		//prefabs
		public Transform m_objPlatform = null;
		public Transform m_objSpike = null;
		public Transform m_objHole = null;


		//max distance passed (record)
		public int m_maxDistance = 0;
		public int m_maxScores = 0;

		public System.Random m_random;

		void Awake()
		{
			if (instance != null && instance != this)
			{
				Destroy(this.gameObject);
				return;
			}

			InitOnce();

			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}

		void InitOnce()
		{
			m_random = new System.Random(DateTime.Now.Millisecond);
		}

	}

}