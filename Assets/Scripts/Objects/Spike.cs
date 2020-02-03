using UnityEngine;
using UnityEngine.UI;

namespace mygame
{
	public class Spike : BaseObject
	{
		SpriteRenderer sprRenderer;

		void Awake()
		{
			sprRenderer = gameObject.GetComponent<SpriteRenderer>();
		}

		public override float GetRightmostX()
		{
			return sprRenderer.bounds.max.x;
		}
	}
}