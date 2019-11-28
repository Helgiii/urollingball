using UnityEngine;
using UnityEngine.UI;

namespace mygame
{
	public class Spike : BaseObject
	{
		SpriteRenderer m_renderer;

		void Awake()
		{
			m_renderer = gameObject.GetComponent<SpriteRenderer>();
		}

		public override float GetRightmostX()
		{
			return m_renderer.bounds.max.x;
		}
	}
}