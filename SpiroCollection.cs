using System;
using System.Collections;
using System.Xml.Serialization;

namespace Spirograph
{
	/// <summary>
	/// Summary description for SpiroCollection.
	/// </summary>
	[XmlInclude(typeof(Epitrochoid)),
	XmlInclude(typeof(Hypotrochoid))]
	public class SpiroCollection : ArrayList
	{
		public SpiroCollection()
		{
		}
	}
}

