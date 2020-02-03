using UnityEngine;
using System;
using System.Collections.Generic;

namespace mygame
{
	public class Globals : Singleton<Globals>
	{
		// Max distance passed (record)
		public int maxDistance { get; set; } = 0;
		public int maxScores { get; set; } = 0;

		public System.Random random { get; private set; }


		public Globals()
		{
			random = new System.Random(DateTime.Now.Millisecond);
		}
	}
}