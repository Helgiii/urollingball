using UnityEngine;
using UnityEngine.UI;

namespace mygame
{
	public class Hole : BaseObject
	{
		BoxCollider2D m_collider;

		private void Awake()
		{
			m_collider = gameObject.GetComponent<BoxCollider2D>();
		}

		public override float GetRightmostX()
		{
			//MeshRenderer mrend = GetComponentInChildren<MeshRenderer>();
			//return mrend.bounds.max.x;

			return transform.position.x + m_collider.bounds.extents.x;
		}
	}
}