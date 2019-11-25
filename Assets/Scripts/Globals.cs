using UnityEngine;
//using UnityEngine.UI;
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

		void Awake()
		{
			if (instance != null && instance != this)
			{
				Destroy(this.gameObject);
				return;
			}

			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}

		void Start()
		{
		}
	}

}