using Kwytto.Utils;
using System.Linq;
using System.Xml.Serialization;
using static Kwytto.Utils.XmlUtils;

namespace Kwytto.Interfaces
{
    public class ILibableAsContainer<D> : ILibable
    {
        [XmlIgnore]
        public D[] m_dataArray = new D[0];

        public ListWrapper<D> Data
        {
            get => new ListWrapper<D> { listVal = m_dataArray.ToList() };
            set => m_dataArray = value.listVal.ToArray();
        }
        [XmlAttribute("saveName")]
        public virtual string SaveName { get; set; }
    }

    public class ILibableAsContainer<I, D> : ILibable where I : class, new() where D : class, new()
    {
        public XmlDictionary<I, D> Data { get; set; }
        [XmlAttribute("saveName")]
        public virtual string SaveName { get; set; }
    }

}