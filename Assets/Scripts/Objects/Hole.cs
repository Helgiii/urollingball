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
			return m_collider.bounds.max.x;
		}
	}
}