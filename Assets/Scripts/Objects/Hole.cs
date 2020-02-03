using UnityEngine;
using UnityEngine.UI;

namespace mygame
{
	public class Hole : BaseObject
	{
		BoxCollider2D boxCollider;

		private void Awake()
		{
			boxCollider = gameObject.GetComponent<BoxCollider2D>();
		}

		public override float GetRightmostX()
		{
			//MeshRenderer mrend = GetComponentInChildren<MeshRenderer>();
			//return mrend.bounds.max.x;

			return transform.position.x + boxCollider.bounds.extents.x;
		}
	}
}