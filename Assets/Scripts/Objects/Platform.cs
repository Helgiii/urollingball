using UnityEngine;
using UnityEngine.UI;

namespace mygame
{
	public class Platform : BaseObject
	{
		MeshRenderer m_rendrer;

		private void Awake()
		{
			m_rendrer = gameObject.GetComponent<MeshRenderer>();
		}

		public override float GetRightmostX()
		{
			return m_rendrer.bounds.max.x;
		}
	}
}