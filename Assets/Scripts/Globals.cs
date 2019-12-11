using UnityEngine;
using System;
using System.Collections.Generic;

namespace mygame
{
	public class Globals : Singleton<Globals>
	{
		//max distance passed (record)
		public int m_maxDistance = 0;
		public int m_maxScores = 0;

		public System.Random m_random;


		public Globals()
		{
			m_random = new System.Random(DateTime.Now.Millisecond);
		}
	}
}