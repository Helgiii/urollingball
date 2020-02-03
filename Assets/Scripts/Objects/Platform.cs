using UnityEngine;
using UnityEngine.UI;

namespace mygame
{
	public class Platform : BaseObject
	{
		MeshRenderer rendrer;

		private void Awake()
		{
			rendrer = gameObject.GetComponent<MeshRenderer>();
		}

		public override float GetRightmostX()
		{
			return rendrer.bounds.max.x;
		}
	}
}